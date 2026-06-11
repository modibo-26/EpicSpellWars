using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « <base>. Payez N 🩸 : <effet> à la place » (Taléboulas, Flaminus). On joue SOIT la base SOIT la
// branche payee, jamais les deux. TenterPayer decide (et debite) avant de resoudre la branche.
public class EffetChoixPayant : IEffet
{
    public int Cout { get; set; }
    public string Libelle { get; set; } = "";
    public List<Action> Base { get; set; } = [];
    public List<Action> SiPaye { get; set; } = [];

    public void Execute(GameContext context)
    {
        var actions = context.TenterPayer(Cout, Libelle) ? SiPaye : Base;
        foreach (var action in actions)
            context.Appliquer(action);
    }
}
