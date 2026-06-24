using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Si <condition> : <effets> » au niveau IEffet (≠ EffetConditionnel qui ne branche que des Actions). Sert
// aux bonus « Donjon : <effet composé> » dont l'effet additionnel est lui-même un IEffet et non une simple
// liste d'Actions. Ex. Roulépélax « Donjon : appliquez le même effet (un EffetProposition) au voisin de droite ».
public class EffetSiCondition : IEffet
{
    public required Predicate<GameContext> Condition { get; set; }
    public List<IEffet> Effets { get; set; } = [];

    public void Execute(GameContext context)
    {
        if (Condition(context))
            foreach (var effet in Effets)
                effet.Execute(context);
    }
}
