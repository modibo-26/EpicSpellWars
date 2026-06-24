using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Choisissez une option : <A> ou <B> » — le LANCEUR tranche entre plusieurs options (≠ EffetProposition
// qui est une décision oui/non prise par une CIBLE, ≠ EffetBranchement qui dépend d'un dé). Le hook
// ChoisirOptionLanceur renvoie l'index de l'option retenue ; ses Actions sont alors appliquées.
//   - « Payez N 🩸 : Faites les deux » (Brikébix) : si CoutTout > 0 et que le lanceur paie, TOUTES les
//     options sont exécutées dans l'ordre AU LIEU d'en choisir une (TenterPayer gère solde / 1×-par-tour).
//   - « Donjon : Faites les deux » (Groclonar) : si ConditionTout est vraie, idem mais GRATUITEMENT.
public class EffetChoixLanceur : IEffet
{
    public List<OptionLanceur> Options { get; set; } = [];
    public int CoutTout { get; set; }
    public string LibelleTout { get; set; } = "";

    // Condition « faites toutes les options » sans coût (ex. contrôle du Donjon). Évaluée AVANT le coût payant
    // pour ne pas débiter de Sang quand elle suffit déjà.
    public Predicate<GameContext>? ConditionTout { get; set; }

    public void Execute(GameContext context)
    {
        var faireTout = (ConditionTout?.Invoke(context) ?? false)
                        || (CoutTout > 0 && context.TenterPayer(CoutTout, LibelleTout));
        if (faireTout)
        {
            foreach (var option in Options)
                foreach (var action in option.Actions)
                    context.Appliquer(action);
            return;
        }

        var index = context.ChoisirOptionLanceur(context.Lanceur, Options.Select(o => o.Libelle).ToList());
        foreach (var action in Options[index].Actions)
            context.Appliquer(action);
    }
}

// Une option d'un EffetChoixLanceur : un libellé (présenté au lanceur) + les Actions qu'elle déclenche.
public class OptionLanceur
{
    public string Libelle { get; set; } = "";
    public List<Action> Actions { get; set; } = [];
}
