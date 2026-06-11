using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Cartes Sources encodées + mécanisme de coût « Payez 🩸 », testés sur les vraies cartes du catalogue.
public class SourcesCatalogueTests
{
    private static CarteSort Source(string nom) => Sources.Toutes().Single(c => c.Nom == nom);

    // Joue les effets d'une carte (composant unique) dans le contexte.
    private static void Jouer(Table t, CarteSort carte)
    {
        t.Ctx.DerniereCible = null;
        t.Ctx.DerniereQuantite = 0;
        foreach (var effet in carte.Effets)
            effet.Execute(t.Ctx);
    }

    [Fact]
    public void Pioche_principale_contient_les_exemplaires_encodes()
    {
        // 20 Sources × 2 + 20 Qualités × 2 + 20 Destinations × 2 = 120 (Magie Féroce pas encore encodée).
        Assert.Equal(120, Catalogue.PiochePrincipale().Count);
        Assert.All(Sources.Toutes(), c => Assert.Equal(2, c.Exemplaires));
        Assert.All(Qualites.Toutes(), c => Assert.Equal(2, c.Exemplaires));
        Assert.All(Destinations.Toutes(), c => Assert.Equal(2, c.Exemplaires));
    }

    // Taléboulas — EffetChoixPayant : sans payer = base, en payant = branche « à la place ».
    [Fact]
    public void Taleboulas_non_paye_frappe_seulement_la_gauche()
    {
        var t = new Table { ProchainPaye = false };
        Jouer(t, Source("Taléboulas"));

        Assert.Equal(17, t.Gandalf.PointsDeVie);     // 3 à gauche
        Assert.Equal(20, t.Saroumane.PointsDeVie);   // pas touché
    }

    [Fact]
    public void Taleboulas_paye_frappe_tout_le_monde_a_la_place()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        Jouer(t, Source("Taléboulas"));

        Assert.Equal(17, t.Gandalf.PointsDeVie);
        Assert.Equal(17, t.Saroumane.PointsDeVie);   // touché aussi (à la place)
        Assert.Equal(0, t.Merlin.Sang);              // 2 🩸 débités
    }

    // Necrophilus — EffetOptionnelPayant : la base (Donjon) tourne toujours, le payé s'ajoute.
    [Fact]
    public void Necrophilus_base_plus_option_payee()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 1;
        t.Saroumane.JetonsDernierSurvivant = 1;   // 1 jeton en jeu → 2 dégâts
        Jouer(t, Source("Necrophilus"));

        Assert.Same(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(18, t.Gandalf.PointsDeVie);
        Assert.Equal(18, t.Saroumane.PointsDeVie);
        Assert.Equal(0, t.Merlin.Sang);
    }

    [Fact]
    public void Necrophilus_sans_payer_ne_fait_que_prendre_le_donjon()
    {
        var t = new Table { ProchainPaye = false };
        t.Saroumane.JetonsDernierSurvivant = 1;
        Jouer(t, Source("Necrophilus"));

        Assert.Same(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(20, t.Gandalf.PointsDeVie);   // pas de dégâts
    }

    // Pèredodux — EffetPaiementVariable : soin de base + X PV payés.
    [Fact]
    public void Peredodux_soin_de_base_plus_X_paye()
    {
        var t = new Table { ProchainPaye = true, ProchainMontant = 3 };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Sang = 5;
        t.Merlin.Creatures.Add(new CarteSort("Familier", TypeComposant.Destination, Glyphe.Primaire));
        Jouer(t, Source("Pèredodux"));

        Assert.Equal(14, t.Merlin.PointsDeVie);   // +1 (1 Créature) +3 (X payé)
        Assert.Equal(2, t.Merlin.Sang);           // 5 - 3
    }

    // Nyarlaprizdetep — EffetProposition : la cible accepte de donner 2 🩸, sinon subit 3 dégâts.
    [Fact]
    public void Nyarlaprizdetep_cible_accepte_donne_du_sang()
    {
        var t = new Table { ProchainChoix = true };
        t.Gandalf.Sang = 4;
        Jouer(t, Source("Nyarlaprizdetep"));

        Assert.Equal(2, t.Gandalf.Sang);      // a donné 2
        Assert.Equal(2, t.Merlin.Sang);       // les a reçus
        Assert.Equal(20, t.Gandalf.PointsDeVie);
    }

    [Fact]
    public void Nyarlaprizdetep_cible_refuse_subit_les_degats()
    {
        var t = new Table { ProchainChoix = false };
        t.Gandalf.Sang = 4;
        Jouer(t, Source("Nyarlaprizdetep"));

        Assert.Equal(4, t.Gandalf.Sang);          // n'a rien donné
        Assert.Equal(17, t.Gandalf.PointsDeVie);  // a subi 3
    }

    // Règle du Sang : un coût de Composant n'est payable qu'1×/tour.
    [Fact]
    public void Un_seul_cout_de_composant_paye_par_tour()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 10;

        Assert.True(t.Ctx.TenterPayer(2, "premier"));
        Assert.False(t.Ctx.TenterPayer(2, "deuxième"));   // déjà payé ce tour
        Assert.Equal(8, t.Merlin.Sang);                   // un seul débit
    }

    // Pas assez de Sang → pas de paiement.
    [Fact]
    public void Pas_assez_de_sang_pas_de_paiement()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 1;

        Assert.False(t.Ctx.TenterPayer(5, "trop cher"));
        Assert.Equal(1, t.Merlin.Sang);
    }
}
