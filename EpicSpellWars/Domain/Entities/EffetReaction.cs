using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Réaction : si vous mourez avant que cette carte ne soit résolue, <effet> » ([[reaction-timing]]).
// Un EffetReaction NE s'exécute PAS pendant la résolution normale du sort (ResoudreComposant l'ignore) :
// il n'est déclenché que par la mort de la victime alors que ce composant est encore NON résolu, via
// GameContext.OnMort. Couvre le cas SELF (la victime résout son propre sort) ET le cas CROSS-WIZARD (mourir
// pendant le tour d'un autre : OnMort retrouve le sort déclaré via SortsDeclares et l'état de résolution via
// _composantsResolus à portée tour). Une Réaction peut empêcher la mort (ex. Gonzofungus : PvAUn → PV = 1,
// « vous ne mourez pas ») ; elle peut aussi viser Cible.Tueur (Fukushimax : 1 dé au sorcier qui vous a tué).
// Limite actuelle : le « jouez-le la manche suivante » (Dépipax) = effet différé MancheSuivante non couvert
// (seule la partie immédiate est encodée).
public class EffetReaction : IEffet
{
    public List<Action> Actions { get; set; } = [];

    public void Execute(GameContext context)
    {
        foreach (var action in Actions)
            context.Appliquer(action);
    }
}
