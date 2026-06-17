using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Si <condition> : <effet> [sinon <autre effet>] ». Branche générique sur un prédicat de l'état de
// jeu (≠ EffetProposition qui est une décision oui/non prise par une CIBLE, ≠ EffetBranchement qui
// dépend d'un dé). La condition est évaluée AU MOMENT de l'exécution : les Actions placées avant cet
// effet dans le composant ont déjà modifié l'état (ex. Trankilus lit DernierDe après LancerDeMemorise).
//   - SiVrai exécuté si Condition(context) ; sinon SiFaux (vide = rien).
//   - Multitax : la condition lit l'ÉTAT D'AVANT (ControleurDonjon) car PrendreDonjon est dans une branche,
//     donc joué APRÈS l'évaluation du prédicat.
public class EffetConditionnel : IEffet
{
    public required Predicate<GameContext> Condition { get; set; }
    public List<Action> SiVrai { get; set; } = [];
    public List<Action> SiFaux { get; set; } = [];

    public void Execute(GameContext context)
    {
        foreach (var action in Condition(context) ? SiVrai : SiFaux)
            context.Appliquer(action);
    }
}
