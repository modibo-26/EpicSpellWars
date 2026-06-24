using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche B (Réactions) — cas CROSS-WIZARD : un sorcier peut mourir pendant le tour d'un AUTRE.
// L'ordonnanceur mémorise les sorts déclarés (SortsDeclares) et l'état de résolution à portée tour
// (_composantsResolus) ; OnMort déclenche alors les Réactions des composants NON résolus de la victime, du
// point de vue de la victime (Cible.Tueur = le sorcier qui a porté le coup fatal). Carte : Fukushimax.
public class CrossWizardReactionsTests
{
    private static CarteSort Fukushimax() => Sources.Toutes().Single(c => c.Nom == "Fukushimax");
    private static CarteSort Brademinus() => Sources.Toutes().Single(c => c.Nom == "Brademinus");

    private static CarteSort Filler() => new("Filler", TypeComposant.Qualite, Glyphe.Arcane);
    private static CarteSort Creature() => new("Bête", TypeComposant.Destination, Glyphe.Arcane);

    private static CarteSort Tueur(Cible cible, int montant) => new("Tueur", TypeComposant.Source, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) }] }],
    };

    // Merlin meurt pendant le tour de Gandalf, AVANT d'avoir joué : sa Réaction Fukushimax (non résolue)
    // riposte au tueur (Gandalf) ; son effet principal, lui, ne se déclenche pas (Merlin n'a jamais résolu).
    [Fact]
    public void Fukushimax_riposte_au_tueur_quand_la_victime_n_a_pas_encore_joue()
    {
        var t = new Table { ProchainDe = 2 };
        t.Merlin.PointsDeVie = 3;

        // Gandalf (1 composant) résout AVANT Merlin (2 composants) et tue Merlin (AdversaireDroite = Merlin).
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Gandalf] = [Tueur(Cible.AdversaireDroite, 5)],
            [t.Merlin] = [Fukushimax(), Filler()],
        });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(18, t.Gandalf.PointsDeVie);   // Réaction : 1 dé (=2) au tueur
        Assert.True(t.Gandalf.EstVivant);
        Assert.Equal(3, t.Gandalf.Sang);           // +3 au kill (Gandalf a tué Merlin)
        Assert.Equal(20, t.Saroumane.PointsDeVie); // l'effet PRINCIPAL de Fukushimax (2 à droite=Saroumane) n'a PAS joué
    }

    // Saroumane joue son Fukushimax PUIS meurt plus tard dans le tour (tué par Merlin) : ses composants sont
    // déjà résolus → AUCUNE Réaction (la règle se déduit de _composantsResolus, pas d'un cas « a déjà joué »).
    [Fact]
    public void Fukushimax_ne_riposte_pas_si_la_victime_a_deja_joue()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 3;

        // Saroumane (1 composant) résout AVANT Merlin (2 composants) ; puis Merlin tue Saroumane.
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Saroumane] = [Fukushimax()],
            [t.Merlin] = [Tueur(Cible.AdversaireDroite, 5), Filler()],
        });

        Assert.False(t.Saroumane.EstVivant);
        Assert.Equal(18, t.Gandalf.PointsDeVie);   // effet PRINCIPAL de Fukushimax (2 à droite=Gandalf) joué à son tour
        Assert.Equal(20, t.Merlin.PointsDeVie);    // PAS de riposte : Fukushimax déjà résolu avant la mort
        Assert.Equal(3, t.Merlin.Sang);            // +3 au kill (Merlin a tué Saroumane)
    }

    // Cascade : Gandalf tue Merlin, dont la riposte Fukushimax (1 dé létal) tue Gandalf en retour. Il ne reste
    // que Saroumane → UN SEUL jeton Dernier Survivant (le garde-fou empêche le double-comptage des OnMort
    // imbriqués). Les +3 🩸 sont par-kill et restent attribués correctement (chacun à son tueur).
    [Fact]
    public void Riposte_mortelle_en_chaine_ne_decerne_qu_un_seul_jeton()
    {
        var t = new Table { ProchainDe = 25 };   // 1 dé de Fukushimax = 25 → létal pour Gandalf (20 PV)
        t.Merlin.PointsDeVie = 3;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Gandalf] = [Tueur(Cible.AdversaireDroite, 5)],   // tue Merlin
            [t.Merlin] = [Fukushimax(), Filler()],              // riposte au tueur (Gandalf)
        });

        Assert.False(t.Merlin.EstVivant);
        Assert.False(t.Gandalf.EstVivant);                     // tué par la riposte
        Assert.True(t.Saroumane.EstVivant);
        Assert.Equal(1, t.Saroumane.JetonsDernierSurvivant);   // UN seul jeton malgré 2 morts en cascade
        Assert.Equal(3, t.Merlin.Sang);                        // Merlin a tué Gandalf (riposte) → +3 (Sang persiste après mort)
        Assert.Equal(3, t.Gandalf.Sang);                       // Gandalf a tué Merlin → +3
    }

    // Cross-wizard : Merlin meurt pendant le tour de Gandalf, AVANT d'avoir joué ; la Créature de son sort
    // déclaré encaisse (Brademinus) → Merlin survit, donc Gandalf ne touche AUCUNE récompense de kill.
    [Fact]
    public void Brademinus_la_creature_encaisse_un_kill_cross_wizard()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 3;

        // Gandalf (1 composant) résout AVANT Merlin (2 composants) et tue Merlin (AdversaireDroite = Merlin).
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Gandalf] = [Tueur(Cible.AdversaireDroite, 5)],
            [t.Merlin] = [Brademinus(), Creature()],
        });

        Assert.True(t.Merlin.EstVivant);
        Assert.Equal(3, t.Merlin.PointsDeVie);   // PV restaurés : la Créature a encaissé
        Assert.Equal(0, t.Gandalf.Sang);         // Merlin n'est pas mort → pas de +3 au kill
    }
}
