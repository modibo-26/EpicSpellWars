using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors « relance de dés » : Dés Pipés (relance auto les 1), Manuel de Cryptozoic (relance tout le jet,
// 1×/tour, au choix), Globe Sacrificiel (Payez 2 → relance le plus petit dé). Branchés dans
// EffetJetDePuissance.AjusterDes → GameContext.AppliquerRelancesJet.
public class RelanceDeDesTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static CarteSort Glyphe2(string nom) => new(nom, TypeComposant.Source, Glyphe.Arcane);

    private static CarteSort CreatureJet(int seuil, params Action[] actions) =>
        new("Bête", TypeComposant.Destination, Glyphe.Arcane)
        {
            Effets = [new EffetJetDePuissance { Glyphe = Glyphe.Arcane, Tranches = [new TrancheJetDePuissance { Seuil = seuil, Actions = [.. actions] }] }],
        };

    private static Action Frappe(int montant) => new() { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(montant) };

    private static void DesSequence(Table t, params int[] valeurs)
    {
        var file = new Queue<int>(valeurs);
        t.Ctx.LancerDe = () => file.Dequeue();
    }

    // Dés Pipés : un dé montrant 1 est relancé automatiquement.
    [Fact]
    public void Des_pipes_relance_les_uns()
    {
        var t = new Table();
        DesSequence(t, 1, 6);   // 1 dé : tiré à 1 → relancé en 6
        t.Merlin.Tresors.Add(Tresor("Dés Pipés"));
        t.Ctx.SortEnCours = [CreatureJet(5, Frappe(4))];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // 6 ≥ seuil 5 → 4 dégâts
    }

    // Sans Dés Pipés : le 1 reste, le seuil n'est pas atteint.
    [Fact]
    public void Sans_des_pipes_le_un_reste()
    {
        var t = new Table();
        DesSequence(t, 1);
        t.Ctx.SortEnCours = [CreatureJet(5, Frappe(4))];

        t.Ctx.ResoudreSort();

        Assert.Equal(20, t.Gandalf.PointsDeVie);
    }

    // Manuel de Cryptozoic : on relance tout le jet et on garde le nouveau résultat.
    [Fact]
    public void Manuel_de_cryptozoic_relance_tout_le_jet()
    {
        var t = new Table { ProchainChoix = true };
        DesSequence(t, 2, 6);   // tiré à 2 → relance du jet → 6
        t.Merlin.Tresors.Add(Tresor("Manuel de Cryptozoic"));
        t.Ctx.SortEnCours = [CreatureJet(5, Frappe(4))];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);
    }

    // Manuel de Cryptozoic : si on refuse, le jet initial est conservé.
    [Fact]
    public void Manuel_de_cryptozoic_refus_garde_le_jet()
    {
        var t = new Table { ProchainChoix = false };
        DesSequence(t, 2);
        t.Merlin.Tresors.Add(Tresor("Manuel de Cryptozoic"));
        t.Ctx.SortEnCours = [CreatureJet(5, Frappe(4))];

        t.Ctx.ResoudreSort();

        Assert.Equal(20, t.Gandalf.PointsDeVie);
    }

    // Globe Sacrificiel : Payez 2 🩸 → relance le plus petit dé.
    [Fact]
    public void Globe_sacrificiel_paye_relance_le_plus_petit_de()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        DesSequence(t, 1, 5, 6);   // dés [1, 5] → relance du plus petit (1) → 6 → somme 11
        t.Merlin.Tresors.Add(Tresor("Globe Sacrificiel"));
        t.Ctx.SortEnCours = [CreatureJet(10, Frappe(4)), Glyphe2("A1")];   // 2 Glyphes Arcane → 2 dés

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // 11 ≥ seuil 10 → 4 dégâts
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Globe Sacrificiel non payé : pas de relance.
    [Fact]
    public void Globe_sacrificiel_non_paye_ne_relance_pas()
    {
        var t = new Table { ProchainPaye = false };
        t.Merlin.Sang = 2;
        DesSequence(t, 1, 5);
        t.Merlin.Tresors.Add(Tresor("Globe Sacrificiel"));
        t.Ctx.SortEnCours = [CreatureJet(10, Frappe(4)), Glyphe2("A1")];

        t.Ctx.ResoudreSort();

        Assert.Equal(20, t.Gandalf.PointsDeVie);   // somme 6 < 10
        Assert.Equal(2, t.Merlin.Sang);
    }
}
