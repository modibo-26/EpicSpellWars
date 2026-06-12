using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Étape 4 (sous-système de dés) : réutilisation d'un tirage (LancerDeMemorise + ValeurDernierDe),
// BonusDesJet (Shub payé), GARDEZ hors tranche (Gracula), 2 dés séparés (Castoramax, Coupéhendus).
public class DesTests
{
    // Un seul tirage mémorisé alimente plusieurs effets (pas de relance).
    [Fact]
    public void LancerDeMemorise_reutilise_le_meme_resultat()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 10;
        var des = new Queue<int>([3, 99]);          // 99 ne doit JAMAIS être consommé
        t.Ctx.LancerDe = () => des.Dequeue();

        t.Ctx.Appliquer(new Action { Type = TypeAction.LancerDeMemorise, Cible = Cible.Soi });
        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurDernierDe(1) });
        t.Ctx.Appliquer(new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurDernierDe(1) });

        Assert.Equal(3, t.Merlin.Sang);             // gain = dé mémorisé
        Assert.Equal(13, t.Merlin.PointsDeVie);     // soin = MÊME dé (3), pas 99
    }

    // AjouterBonusDe nourrit BonusDesJet (Shub payé : +1 dé aux Jets pour une Créature).
    [Fact]
    public void AjouterBonusDe_augmente_le_bonus_de_jet()
    {
        var t = new Table();

        t.Ctx.Appliquer(new Action { Type = TypeAction.AjouterBonusDe, Cible = Cible.Soi, Valeur = new ValeurFixe(1) });

        Assert.Equal(1, t.Ctx.BonusDesJetCreature);
        Assert.Equal(1, t.Ctx.BonusDesJet(new EffetJetDePuissance()));
    }

    // GARDEZ hors tranche : l'action Garder garde la Créature en cours (Gracula « Payez 1 🩸 : GARDEZ »).
    [Fact]
    public void Garder_conserve_la_creature_en_cours_hors_tranche()
    {
        var t = new Table();
        var gracula = new CarteSort("Gracula", TypeComposant.Destination, Glyphe.Tenebres, initiative: 13);
        t.Ctx.CreatureEnCours = gracula;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Garder, Cible = Cible.Soi });

        Assert.Contains(gracula, t.Merlin.Creatures);
    }

    // Castoramax : le dé offensif (le plus grand via ChoisirDe) frappe à gauche, l'autre est l'auto-dégât.
    [Fact]
    public void Castoramax_un_de_attaque_lautre_est_lauto_degat()
    {
        var t = new Table();                        // ProchainPaye = false, pas de Donjon
        var des = new Queue<int>([6, 2]);
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetCastoramax().Execute(t.Ctx);

        Assert.Equal(14, t.Gandalf.PointsDeVie);    // gauche : -6 (dé offensif)
        Assert.Equal(18, t.Merlin.PointsDeVie);     // auto-dégât : -2 (l'autre dé)
    }

    // Castoramax : contrôler le Donjon évite l'auto-dégât.
    [Fact]
    public void Castoramax_le_donjon_evite_lauto_degat()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;
        var des = new Queue<int>([6, 2]);
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetCastoramax().Execute(t.Ctx);

        Assert.Equal(14, t.Gandalf.PointsDeVie);    // gauche : -6
        Assert.Equal(20, t.Merlin.PointsDeVie);     // Donjon → pas d'auto-dégât
    }

    // Coupéhendus payé : le plus petit dé sur la cible choisie, l'autre sur un autre adversaire.
    [Fact]
    public void Coupehendus_paye_min_sur_la_cible_et_autre_ailleurs()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        var des = new Queue<int>([2, 5]);
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetCoupehendus().Execute(t.Ctx);

        Assert.Equal(18, t.Gandalf.PointsDeVie);    // cible choisie : -2 (le plus petit)
        Assert.Equal(15, t.Saroumane.PointsDeVie);  // autre adversaire : -5 (l'autre dé)
        Assert.Equal(0, t.Merlin.Sang);             // 2 payés
    }

    // Coupéhendus non payé : seul le plus petit dé est infligé.
    [Fact]
    public void Coupehendus_non_paye_seul_le_min()
    {
        var t = new Table();                        // ProchainPaye = false
        var des = new Queue<int>([2, 5]);
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetCoupehendus().Execute(t.Ctx);

        Assert.Equal(18, t.Gandalf.PointsDeVie);    // -2
        Assert.Equal(20, t.Saroumane.PointsDeVie);  // pas payé → intact
    }
}
