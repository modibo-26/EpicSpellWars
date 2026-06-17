using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche B (Réactions) : un EffetReaction d'un composant NON résolu se déclenche quand le
// lanceur meurt en résolvant son propre sort (cas SELF). Timing fin : le composant qui tue et ceux déjà
// résolus ne réagissent pas. Une Réaction peut empêcher la mort (Gonzofungus PV→1).
public class ReactionsTests
{
    private static CarteSort Source(string nom) => Sources.Toutes().Single(c => c.Nom == nom);

    // Composant qui suicide le lanceur (AutoDegats létal) — sert à déclencher la mort en cours de sort.
    private static CarteSort Suicide(TypeComposant type) => new("Suicide", type, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) }] }],
    };

    // Composant portant une Réaction observable (dégâts à l'adversaire de gauche).
    private static CarteSort AvecReaction(TypeComposant type) => new("Réacteur", type, Glyphe.Arcane)
    {
        Effets = [new EffetReaction { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) }] }],
    };

    // La Réaction d'un composant NON résolu se déclenche à la mort du lanceur.
    [Fact]
    public void Reaction_d_un_composant_non_resolu_se_declenche_a_la_mort()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        // Source (suicide, rang 0) résout AVANT la Qualité (Réaction, rang 1) → la Qualité est non résolue.
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), AvecReaction(TypeComposant.Qualite)];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(16, t.Gandalf.PointsDeVie);   // la Réaction a frappé
    }

    // Timing fin : un composant DÉJÀ résolu ne réagit pas (sa Réaction ne se déclenche pas à une mort ultérieure).
    [Fact]
    public void Reaction_d_un_composant_deja_resolu_ne_se_declenche_pas()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        // Source (Réaction, rang 0) résout AVANT la Destination (suicide, rang 2) → la Source est déjà résolue.
        t.Ctx.SortEnCours = [AvecReaction(TypeComposant.Source), Suicide(TypeComposant.Destination)];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(20, t.Gandalf.PointsDeVie);   // Réaction NON déclenchée (Source déjà résolue)
    }

    // Une Réaction qui n'est PAS dans le sort (restée en main) ne se déclenche pas.
    [Fact]
    public void Reaction_restee_en_main_ne_se_declenche_pas()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        t.Merlin.Main = [AvecReaction(TypeComposant.Qualite)];   // jouée NULLE PART
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source)];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(20, t.Gandalf.PointsDeVie);   // pas jouée → aucune Réaction
    }

    // Gonzofungus : sa Réaction empêche la mort (PV = 1) ; pas de récompense au kill.
    [Fact]
    public void Gonzofungus_empeche_la_mort()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        // Le suicide (Source) résout avant Gonzofungus (Source, 2e dans la liste) → Gonzofungus non résolu.
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Gonzofungus")];

        t.Ctx.ResoudreSort();

        Assert.True(t.Merlin.EstVivant);
        Assert.Equal(1, t.Merlin.PointsDeVie);   // PV → 1, « vous ne mourez pas »
    }

    // Dépipax : sa Réaction fait gagner 1 Trésor (partie immédiate ; le « jouez-le manche suivante » est différé).
    [Fact]
    public void Depipax_gagne_un_tresor_en_reaction()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        t.Ctx.PiocheTresor = [new Tresor("T", [], TriggerType.Passif)];
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Dépipax")];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Single(t.Merlin.Tresors);   // gagné via la Réaction (la mort arrête le sort : effet principal non joué)
    }
}
