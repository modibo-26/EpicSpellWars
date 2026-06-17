using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Étape 5 (manques résolveur — Conditionnelles) : EffetConditionnel (branche d'Actions sur un prédicat
// d'état) + ValeurConditionnelle (montant conditionnel). Cartes closes : Trankilus, Bégoniax, Multitax,
// Sabruledepartoux (base + conditionnelle ; payé reste GAP), Beeeh-zerker (valeur 3/6).
public class ConditionnellesTests
{
    private static CarteSort Carte(string nom) =>
        Sources.Toutes().Concat(Qualites.Toutes()).Concat(Destinations.Toutes()).Single(c => c.Nom == nom);

    private static void Resoudre(Table t, CarteSort carte, int de = 1)
    {
        t.ProchainDe = de;
        t.Ctx.SortEnCours = [carte];
        t.Ctx.ResoudreSort();
    }

    // Trankilus, dé ≤ 4 : +🩸 = dé, ET l'adversaire le plus faible se soigne du MÊME dé.
    [Fact]
    public void Trankilus_de_4_ou_moins_soigne_le_plus_faible()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 10;   // le plus faible des adversaires

        Resoudre(t, Carte("Trankilus"), de: 3);

        Assert.Equal(3, t.Merlin.Sang);          // gain = dé
        Assert.Equal(13, t.Gandalf.PointsDeVie); // 10 + 3 (même dé)
    }

    // Trankilus, dé > 4 : gain de 🩸 seulement, aucun soin.
    [Fact]
    public void Trankilus_de_superieur_a_4_ne_soigne_pas()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 10;

        Resoudre(t, Carte("Trankilus"), de: 6);

        Assert.Equal(6, t.Merlin.Sang);
        Assert.Equal(10, t.Gandalf.PointsDeVie); // intact
    }

    // Bégoniax, aucun sorcier mort : +2 🩸 (les dégâts ParMort valent 0).
    [Fact]
    public void Begoniax_aucun_mort_gagne_2_sang()
    {
        var t = new Table();

        Resoudre(t, Carte("Bégoniax"));

        Assert.Equal(2, t.Merlin.Sang);
        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(20, t.Gandalf.PointsDeVie);  // ParMort(1) × 0 mort
    }

    // Bégoniax, un sorcier mort : pas de gain, et 1 dégât/mort à chaque adversaire vivant.
    [Fact]
    public void Begoniax_un_mort_inflige_des_degats_et_ne_gagne_pas_de_sang()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 0;   // un mort

        Resoudre(t, Carte("Bégoniax"));

        Assert.Equal(0, t.Merlin.Sang);            // condition fausse
        Assert.Equal(19, t.Gandalf.PointsDeVie);   // 20 - (1 × 1 mort)
    }

    // Multitax, Donjon pris à un adversaire vivant : 3 dégâts à un AUTRE adversaire, puis prise du Donjon.
    [Fact]
    public void Multitax_pris_a_un_vivant_frappe_un_autre_adversaire()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Gandalf;   // adversaire vivant détenteur

        Resoudre(t, Carte("Multitax"));

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon); // pris
        Assert.Equal(17, t.Saroumane.PointsDeVie);      // l'AUTRE adversaire (≠ Gandalf) : 20 - 3
        Assert.Equal(20, t.Gandalf.PointsDeVie);        // celui à qui on l'a pris : épargné
    }

    // Multitax, Donjon libre (personne) : prise simple, aucun dégât.
    [Fact]
    public void Multitax_donjon_libre_prend_sans_degats()
    {
        var t = new Table();   // ControleurDonjon = null

        Resoudre(t, Carte("Multitax"));

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(20, t.Gandalf.PointsDeVie);
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // Sabruledepartoux : le contrôleur du Donjon (ici un adversaire) subit 4.
    [Fact]
    public void Sabruledepartoux_frappe_le_controleur_du_donjon()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Gandalf;

        Resoudre(t, Carte("Sabruledepartoux"));

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // contrôleur : 20 - 4
        Assert.Equal(20, t.Saroumane.PointsDeVie);
        Assert.Equal(20, t.Merlin.PointsDeVie);
    }

    // Sabruledepartoux, Donjon non contrôlé : TOUS les sorciers (lanceur inclus) subissent 4.
    [Fact]
    public void Sabruledepartoux_personne_au_donjon_frappe_tout_le_monde()
    {
        var t = new Table();   // ControleurDonjon = null

        Resoudre(t, Carte("Sabruledepartoux"));

        Assert.Equal(16, t.Merlin.PointsDeVie);     // lanceur inclus
        Assert.Equal(16, t.Gandalf.PointsDeVie);
        Assert.Equal(16, t.Saroumane.PointsDeVie);
    }

    // Beeeh-zerker 10+, dernier adversaire vivant : 6 dégâts (valeur conditionnelle).
    [Fact]
    public void BeehZerker_10plus_dernier_adversaire_inflige_6()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 0;   // ne reste qu'un adversaire vivant : Gandalf

        Resoudre(t, Carte("Beeeh-zerker!"), de: 10);

        Assert.Equal(14, t.Gandalf.PointsDeVie);   // 20 - 6
    }

    // Beeeh-zerker 10+, plusieurs adversaires : 3 dégâts (valeur de base).
    [Fact]
    public void BeehZerker_10plus_plusieurs_adversaires_inflige_3()
    {
        var t = new Table();

        Resoudre(t, Carte("Beeeh-zerker!"), de: 10);

        Assert.Equal(17, t.Gandalf.PointsDeVie);   // 20 - 3 (1er adversaire sans Donjon)
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }
}
