using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Réaction : si vous mourez avant que cette carte ne soit résolue, <effet> » ([[reaction-timing]]).
// Un EffetReaction NE s'exécute PAS pendant la résolution normale du sort (ResoudreComposant l'ignore) :
// il n'est déclenché que par la mort de la victime alors que ce composant est encore NON résolu, via
// GameContext.OnMort. Couvre le cas SELF (la victime résout son propre sort) ET le cas CROSS-WIZARD (mourir
// pendant le tour d'un autre : OnMort retrouve le sort déclaré via SortsDeclares et l'état de résolution via
// _composantsResolus à portée tour). Une Réaction peut empêcher la mort (ex. Gonzofungus : PvAUn → PV = 1,
// « vous ne mourez pas ») ; elle peut aussi viser Cible.Tueur (Fukushimax : 1 dé au sorcier qui vous a tué).
// `Actions` = effet immédiat de la Réaction. `ActionsDifferees` = effet REPORTÉ au début de la prochaine
// manche (Dépipax : « gagnez 1 Trésor et jouez-le au début de la prochaine manche ») : on ne l'applique pas
// tout de suite (un mort défausse tout à la mort / au FinManche), on l'empile dans EffetsDifferes pour la
// victime, résolu par DeclencherEffetsDifferes à DebutManche.
public class EffetReaction : IEffet
{
    public List<Action> Actions { get; set; } = [];
    public List<Action> ActionsDifferees { get; set; } = [];

    public void Execute(GameContext context)
    {
        foreach (var action in Actions)
            context.Appliquer(action);

        // Reporté à la manche suivante, pour le POINT DE VUE courant (Lanceur = la victime pendant la Réaction).
        if (ActionsDifferees.Count > 0)
            context.EffetsDifferes.Add(([new EffetSimple { Actions = ActionsDifferees }], context.Lanceur));
    }
}
