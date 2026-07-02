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

    // Créature (Destination) à effet observable (frappe l'adversaire de gauche) : sert à vérifier qu'une
    // Créature « encaisseuse » (Brademinus) est bien consommée AVANT de résoudre son propre effet.
    private static CarteSort Creature() => new("Bête", TypeComposant.Destination, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) }] }],
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

    // Dépipax : sa Réaction REPORTE le gain d'1 Trésor au début de la prochaine manche (un mort défausse tout,
    // donc pas de gain immédiat). L'effet différé est empilé dans EffetsDifferes, résolu à DebutManche.
    [Fact]
    public void Depipax_reporte_le_gain_de_tresor_a_la_manche_suivante()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        t.Ctx.PiocheTresor = [new Tresor("T", [], TriggerType.Passif)];
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Dépipax")];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Empty(t.Merlin.Tresors);          // rien tout de suite (un mort ne garde pas de Trésor)
        Assert.Single(t.Ctx.EffetsDifferes);     // gain reporté à la prochaine manche

        // Au début de la manche suivante, l'effet différé se résout → Merlin gagne enfin son Trésor.
        new EpicSpellWars.Application.Services.OrdonnanceurDeTour().DebutManche(t.Ctx);
        Assert.Single(t.Merlin.Tresors);
        Assert.Empty(t.Ctx.EffetsDifferes);
    }

    // Momidisis : sa Réaction fait piocher 2 crevés SUPPLÉMENTAIRES (en plus du crevé de consolation d'OnMort),
    // soit 3 au total à la mort avant résolution.
    [Fact]
    public void Momidisis_pioche_deux_creves_supplementaires_en_reaction()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        // 3 crevés Passif neutres disponibles (2 de la Réaction + 1 de consolation).
        t.Ctx.PiocheSorcierCreve = [.. Enumerable.Range(0, 3).Select(i => new SorcierCreve($"C{i}", [], TriggerType.Passif))];
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Momidisis")];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(3, t.Merlin.SorciersCreves.Count);   // 2 (Réaction) + 1 (consolation OnMort)
    }

    // Brademinus : une Créature présente dans le sort encaisse le coup fatal → le lanceur survit (PV restaurés
    // à l'avant-coup), et la Créature est consommée (ni résolution de son effet, ni GARDEZ).
    [Fact]
    public void Brademinus_la_creature_encaisse_et_empeche_la_mort()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        var creature = Creature();
        // Suicide (Source) résout avant Brademinus (Source) → Brademinus non résolu quand Merlin meurt.
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Brademinus"), creature];

        t.Ctx.ResoudreSort();

        Assert.True(t.Merlin.EstVivant);
        Assert.Equal(3, t.Merlin.PointsDeVie);          // PV d'avant-coup restaurés (« vous ne mourez pas »)
        Assert.Equal(20, t.Gandalf.PointsDeVie);        // la Créature est consommée → son effet ne se résout pas
        Assert.DoesNotContain(creature, t.Merlin.Creatures);   // sacrifiée, pas gardée
    }

    // Brademinus sans Créature dans le sort : la Réaction n'a rien pour encaisser → la mort tient.
    [Fact]
    public void Brademinus_sans_creature_ne_empeche_pas_la_mort()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;
        t.Ctx.SortEnCours = [Suicide(TypeComposant.Source), Source("Brademinus")];

        t.Ctx.ResoudreSort();

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(0, t.Merlin.PointsDeVie);
    }
}
