using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Execute toutes ses actions (conjonction)
public class EffetSimple : IEffet
{
    public List<Action> Actions { get; set; } = [];

    public void Execute(GameContext context)
    {
        // chaque Action = une INSTANCE distincte : ne jamais sommer les degats avant de les
        // appliquer (une Creature ne bloque qu'une seule instance par effet, rulebook p.9).
        foreach (var action in Actions)
            context.Appliquer(action);
    }
}
