using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Payez N 🩸 : <effet> » qui S'AJOUTE a l'effet de base de la carte (Necrophilus, Hydraponix).
// La base est portee par d'autres Effets de la carte ; ce wrapper n'execute QUE la branche payee,
// et seulement si le lanceur paie (TenterPayer gere solde / 1x-par-tour / decision avant resolution).
public class EffetOptionnelPayant : IEffet
{
    public int Cout { get; set; }
    public string Libelle { get; set; } = "";
    public List<Action> SiPaye { get; set; } = [];

    public void Execute(GameContext context)
    {
        if (context.TenterPayer(Cout, Libelle))
            foreach (var action in SiPaye)
                context.Appliquer(action);
    }
}
