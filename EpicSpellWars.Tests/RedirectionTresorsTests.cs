using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors de redirection : Baguette Bicéphale (inverse gauche↔droite sur votre sort) et Dissuasion Nucléaire
// (seule cible d'un sort adverse → Payez 3 🩸 pour rediriger le sort vers un autre sorcier).
public class RedirectionTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);

    private static CarteSort Frappe(Cible cible, int montant) => new("S", TypeComposant.Source, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) }] }],
    };

    // Baguette Bicéphale : le porteur inverse gauche↔droite → un effet « adversaire de gauche » touche la droite.
    [Fact]
    public void Baguette_bicephale_inverse_gauche_droite()
    {
        var t = new Table { ProchainChoix = true };
        t.Merlin.Tresors.Add(Tresor("Baguette Bicéphale"));
        t.Ctx.SortEnCours = [Frappe(Cible.AdversaireGauche, 3)];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Saroumane.PointsDeVie);   // gauche → droite
        Assert.Equal(20, t.Gandalf.PointsDeVie);
    }

    // Baguette Bicéphale : si le porteur n'inverse pas, la direction reste normale.
    [Fact]
    public void Baguette_bicephale_refus_garde_la_direction()
    {
        var t = new Table { ProchainChoix = false };
        t.Merlin.Tresors.Add(Tresor("Baguette Bicéphale"));
        t.Ctx.SortEnCours = [Frappe(Cible.AdversaireGauche, 3)];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);     // gauche normale
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // Dissuasion Nucléaire : seule cible d'un sort adverse → paie 3 🩸 et redirige vers un autre sorcier.
    [Fact]
    public void Dissuasion_nucleaire_paye_redirige_le_sort()
    {
        var t = new Table { ProchainPaye = true };
        t.Gandalf.Tresors.Add(Tresor("Dissuasion Nucléaire"));
        t.Gandalf.Sang = 3;
        t.Ctx.SortEnCours = [Frappe(Cible.AdversaireGauche, 3)];   // Merlin → Gandalf (seule cible)

        t.Ctx.ResoudreSort();

        Assert.Equal(20, t.Gandalf.PointsDeVie);     // redirigé : épargné
        Assert.Equal(17, t.Saroumane.PointsDeVie);   // reçoit à la place
        Assert.Equal(0, t.Gandalf.Sang);             // 3 payés
    }

    // Dissuasion Nucléaire non payée : le porteur subit le sort.
    [Fact]
    public void Dissuasion_nucleaire_non_paye_subit_le_sort()
    {
        var t = new Table { ProchainPaye = false };
        t.Gandalf.Tresors.Add(Tresor("Dissuasion Nucléaire"));
        t.Gandalf.Sang = 3;
        t.Ctx.SortEnCours = [Frappe(Cible.AdversaireGauche, 3)];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);
        Assert.Equal(20, t.Saroumane.PointsDeVie);
        Assert.Equal(3, t.Gandalf.Sang);
    }

    // Dissuasion Nucléaire : ne se déclenche PAS si le porteur n'est pas la seule cible (sort multi-cibles).
    [Fact]
    public void Dissuasion_nucleaire_pas_seule_cible_ne_redirige_pas()
    {
        var t = new Table { ProchainPaye = true };
        t.Gandalf.Tresors.Add(Tresor("Dissuasion Nucléaire"));
        t.Gandalf.Sang = 3;
        t.Ctx.SortEnCours = [Frappe(Cible.TousAdversaires, 3)];   // 2 cibles → pas « seule cible »

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);     // subit
        Assert.Equal(17, t.Saroumane.PointsDeVie);
        Assert.Equal(3, t.Gandalf.Sang);             // rien payé
    }
}
