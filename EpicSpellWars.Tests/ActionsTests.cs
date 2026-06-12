using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Étape 3 (manques résolveur — Actions) : DonnerSang (transfert lanceur→cible), DonnerDonjon (à un
// adversaire), EffetSpiralex (dégâts croissants vers la droite), EffetChancedecocus (dé/adversaire + max).
public class ActionsTests
{
    // DonnerSang : le lanceur transfère du Sang à la cible (inverse de VolerSang).
    [Fact]
    public void DonnerSang_transfere_du_lanceur_vers_la_cible()
    {
        var t = new Table();
        t.Merlin.Sang = 5;

        t.Ctx.Appliquer(new Action { Type = TypeAction.DonnerSang, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) });

        Assert.Equal(3, t.Merlin.Sang);    // 5 - 2
        Assert.Equal(2, t.Gandalf.Sang);   // 0 + 2
    }

    // DonnerSang est borné au Sang disponible (« si vous en avez »).
    [Fact]
    public void DonnerSang_borne_au_sang_disponible()
    {
        var t = new Table();
        t.Merlin.Sang = 1;

        t.Ctx.Appliquer(new Action { Type = TypeAction.DonnerSang, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) });

        Assert.Equal(0, t.Merlin.Sang);    // ne donne que ce qu'il a
        Assert.Equal(1, t.Gandalf.Sang);
    }

    // DonnerDonjon : le Donjon passe à la cible (≠ PrendreDonjon qui le donne au lanceur).
    [Fact]
    public void DonnerDonjon_attribue_le_donjon_a_la_cible()
    {
        var t = new Table();

        t.Ctx.Appliquer(new Action { Type = TypeAction.DonnerDonjon, Cible = Cible.AdversaireDroite });

        Assert.Equal(t.Saroumane, t.Ctx.ControleurDonjon);
    }

    // Spiralex : 1 dégât à droite, 2 au suivant à droite... (le lanceur est épargné).
    [Fact]
    public void Spiralex_inflige_des_degats_croissants_vers_la_droite()
    {
        var t = new Table();

        new EffetSpiralex().Execute(t.Ctx);

        Assert.Equal(19, t.Saroumane.PointsDeVie);   // adversaire de droite → -1
        Assert.Equal(18, t.Gandalf.PointsDeVie);     // suivant à droite → -2
        Assert.Equal(20, t.Merlin.PointsDeVie);      // lanceur épargné
    }

    // Spiralex payé : les dégâts sont doublés.
    [Fact]
    public void Spiralex_paye_double_les_degats()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;

        new EffetSpiralex { CoutDouble = 2 }.Execute(t.Ctx);

        Assert.Equal(18, t.Saroumane.PointsDeVie);   // -1×2
        Assert.Equal(16, t.Gandalf.PointsDeVie);     // -2×2
        Assert.Equal(0, t.Merlin.Sang);              // 2 payés
    }

    // Chancedecocus : chaque adversaire lance 1 dé ; le plus haut gagne 1 🩸 et subit son résultat.
    [Fact]
    public void Chancedecocus_le_plus_haut_de_gagne_du_sang_et_subit_son_resultat()
    {
        var t = new Table();
        var des = new Queue<int>([5, 2]);            // Gandalf = 5, Saroumane = 2 (ordre des adversaires)
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetChancedecocus().Execute(t.Ctx);

        Assert.Equal(15, t.Gandalf.PointsDeVie);     // max (5) → 20 - 5
        Assert.Equal(1, t.Gandalf.Sang);             // +1 🩸
        Assert.Equal(20, t.Saroumane.PointsDeVie);   // pas le max → intact (base, sans le Donjon)
    }
}
