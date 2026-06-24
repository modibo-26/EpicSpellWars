using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors « courts » : Divan le Terrible (un joker Magie féroce révèle 2 cartes au lieu d'1) et Bœuf aux
// Hormones (Payez 3 après une Créature pour la GARDER).
public class DivanEtBoeufTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);

    private static CarteSort CreatureJet(int seuil, bool peutGarder) =>
        new("Bête", TypeComposant.Destination, Glyphe.Arcane)
        {
            Effets = [new EffetJetDePuissance { Glyphe = Glyphe.Arcane, Tranches = [new TrancheJetDePuissance { Seuil = seuil, PeutGarder = peutGarder, Actions = [] }] }],
        };

    // Divan le Terrible : le joker révèle 2 cartes du type cherché (les deux rejoignent le sort).
    [Fact]
    public void Divan_le_terrible_joker_revele_deux_cartes()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Divan le Terrible"));
        var q1 = new CarteSort("Q1", TypeComposant.Qualite, Glyphe.Arcane);
        var src = new CarteSort("S", TypeComposant.Source, Glyphe.Arcane);
        var q2 = new CarteSort("Q2", TypeComposant.Qualite, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [q1, src, q2];
        var joker = new MagieFeroce { TypeRemplace = TypeComposant.Qualite };
        var sort = new List<CarteSort> { joker };

        t.Ctx.ResoudreJokersDuSort(sort, t.Merlin);

        Assert.Equal([q1, q2], sort);              // 2 Qualités ajoutées
        Assert.Contains(joker, t.Ctx.Defausse);
        Assert.Contains(src, t.Ctx.Defausse);      // révélée entre les deux, non retenue
    }

    // Sans Divan : le joker ne révèle qu'une seule carte.
    [Fact]
    public void Sans_divan_joker_revele_une_seule_carte()
    {
        var t = new Table();
        var q1 = new CarteSort("Q1", TypeComposant.Qualite, Glyphe.Arcane);
        var src = new CarteSort("S", TypeComposant.Source, Glyphe.Arcane);
        var q2 = new CarteSort("Q2", TypeComposant.Qualite, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [q1, src, q2];
        var joker = new MagieFeroce { TypeRemplace = TypeComposant.Qualite };
        var sort = new List<CarteSort> { joker };

        t.Ctx.ResoudreJokersDuSort(sort, t.Merlin);

        Assert.Equal([q1], sort);                  // 1 seule
        Assert.Equal(2, t.Ctx.PiochePrincipale.Count);   // src et q2 restent
    }

    // Bœuf aux Hormones : Payez 3 🩸 après une Créature non gardée pour la GARDER.
    [Fact]
    public void Boeuf_aux_hormones_paye_garde_la_creature()
    {
        var t = new Table { ProchainPaye = true, ProchainDe = 5 };
        t.Merlin.Sang = 3;
        t.Merlin.Tresors.Add(Tresor("Bœuf aux Hormones"));
        var creature = CreatureJet(seuil: 1, peutGarder: false);   // le jet NE garde PAS
        t.Ctx.SortEnCours = [creature];

        t.Ctx.ResoudreSort();

        Assert.Contains(creature, t.Merlin.Creatures);   // gardée via Bœuf
        Assert.Equal(0, t.Merlin.Sang);                  // 3 payés
    }

    // Bœuf aux Hormones non payé : la Créature n'est pas gardée.
    [Fact]
    public void Boeuf_aux_hormones_non_paye_ne_garde_pas()
    {
        var t = new Table { ProchainPaye = false, ProchainDe = 5 };
        t.Merlin.Sang = 3;
        t.Merlin.Tresors.Add(Tresor("Bœuf aux Hormones"));
        var creature = CreatureJet(seuil: 1, peutGarder: false);
        t.Ctx.SortEnCours = [creature];

        t.Ctx.ResoudreSort();

        Assert.DoesNotContain(creature, t.Merlin.Creatures);
        Assert.Equal(3, t.Merlin.Sang);
    }

    // Bœuf aux Hormones : si le Jet a déjà GARDÉ la Créature, aucun paiement n'est proposé.
    [Fact]
    public void Boeuf_aux_hormones_ne_paye_pas_si_deja_gardee_par_le_jet()
    {
        var t = new Table { ProchainPaye = true, ProchainDe = 10 };
        t.Merlin.Sang = 3;
        t.Merlin.Tresors.Add(Tresor("Bœuf aux Hormones"));
        var creature = CreatureJet(seuil: 10, peutGarder: true);   // 1 dé = 10 → le jet GARDE
        t.Ctx.SortEnCours = [creature];

        t.Ctx.ResoudreSort();

        Assert.Contains(creature, t.Merlin.Creatures);   // gardée par le jet
        Assert.Equal(3, t.Merlin.Sang);                  // Bœuf n'a rien payé
    }
}
