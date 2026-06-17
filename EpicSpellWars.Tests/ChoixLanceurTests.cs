using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Étape 6 (manques résolveur — Choix d'option par le LANCEUR) : EffetChoixLanceur (le lanceur tranche
// entre N options ; « Payez N : faites les deux » exécute toutes les options) + Cibles PvPair/PvImpair.
// Cartes closes : Brikébix, Groclonar (clause « Donjon : faites les deux » = pilier 2, hors scope).
public class ChoixLanceurTests
{
    private static CarteSort Carte(string nom) =>
        Sources.Toutes().Concat(Qualites.Toutes()).Single(c => c.Nom == nom);

    private static void Resoudre(Table t, CarteSort carte)
    {
        t.Ctx.SortEnCours = [carte];
        t.Ctx.ResoudreSort();
    }

    // Brikébix option A : prend le Donjon et inflige 1 dégât à chaque adversaire.
    [Fact]
    public void Brikebix_option_A_prend_le_donjon_et_frappe_tous()
    {
        var t = new Table { ProchainOption = 0 };

        Resoudre(t, Carte("Brikébix"));

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(19, t.Gandalf.PointsDeVie);
        Assert.Equal(19, t.Saroumane.PointsDeVie);
    }

    // Brikébix option B : 3 dégâts à un seul adversaire sans Donjon, pas de prise.
    [Fact]
    public void Brikebix_option_B_frappe_un_seul_adversaire()
    {
        var t = new Table { ProchainOption = 1 };

        Resoudre(t, Carte("Brikébix"));

        Assert.Null(t.Ctx.ControleurDonjon);
        Assert.Equal(17, t.Gandalf.PointsDeVie);   // 1er adversaire sans Donjon : 20 - 3
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // Brikébix payé : « faites les deux » — A (Donjon + 1 à chacun) puis B (3 à un adversaire sans Donjon).
    [Fact]
    public void Brikebix_paye_fait_les_deux()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 3;

        Resoudre(t, Carte("Brikébix"));

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(0, t.Merlin.Sang);            // 3 payés
        Assert.Equal(16, t.Gandalf.PointsDeVie);   // -1 (A) puis -3 (B, 1er sans Donjon)
        Assert.Equal(19, t.Saroumane.PointsDeVie); // -1 (A) seulement
    }

    // Groclonar option A : 2 dégâts à chaque adversaire avec un nombre PAIR de PV.
    [Fact]
    public void Groclonar_option_A_frappe_les_PV_pairs()
    {
        var t = new Table { ProchainOption = 0 };
        t.Gandalf.PointsDeVie = 20;    // pair
        t.Saroumane.PointsDeVie = 15;  // impair

        Resoudre(t, Carte("Groclonar"));

        Assert.Equal(18, t.Gandalf.PointsDeVie);   // pair → -2
        Assert.Equal(15, t.Saroumane.PointsDeVie); // impair → épargné
    }

    // Groclonar option B : 3 dégâts à chaque adversaire avec un nombre IMPAIR de PV.
    [Fact]
    public void Groclonar_option_B_frappe_les_PV_impairs()
    {
        var t = new Table { ProchainOption = 1 };
        t.Gandalf.PointsDeVie = 20;    // pair
        t.Saroumane.PointsDeVie = 15;  // impair

        Resoudre(t, Carte("Groclonar"));

        Assert.Equal(20, t.Gandalf.PointsDeVie);   // pair → épargné
        Assert.Equal(12, t.Saroumane.PointsDeVie); // impair → -3
    }
}
