using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Réaction : si vous mourez avant que cette carte ne soit résolue, <effet> » ([[reaction-timing]]).
// Un EffetReaction NE s'exécute PAS pendant la résolution normale du sort (ResoudreComposant l'ignore) :
// il n'est déclenché que par la mort du lanceur alors que ce composant est encore NON résolu, via
// GameContext.OnMort (cas SELF : la victime résout son propre sort). Une Réaction peut empêcher la mort
// (ex. Gonzofungus : PvAUn → PV = 1, « vous ne mourez pas »).
// Limite actuelle : le « jouez-le la manche suivante » (Dépipax) = effet différé MancheSuivante non couvert
// (seule la partie immédiate est encodée) ; et le cas CROSS-WIZARD (mourir pendant le tour d'un autre)
// nécessitera que l'ordonnanceur garde le sort déclaré + l'état de résolution de chaque sorcier.
public class EffetReaction : IEffet
{
    public List<Action> Actions { get; set; } = [];

    public void Execute(GameContext context)
    {
        foreach (var action in Actions)
            context.Appliquer(action);
    }
}
