using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Capacité activée d'un Trésor : « Payez N 🩸 : <Actions> ». Analogue de EffetOptionnelPayant mais sur la
// limite de paiement des TRÉSORS (TenterPayerTresor, 1×/tour distincte des Composants). Posé sur
// Tresor.Activation et déclenché par l'ordonnanceur au tour d'Initiative du porteur (Lanceur = porteur).
public class EffetActivationTresor : IEffet
{
    public int Cout { get; set; }
    public string Libelle { get; set; } = "";
    public List<Action> SiPaye { get; set; } = [];

    public void Execute(GameContext context)
    {
        if (context.TenterPayerTresor(Cout, Libelle))
            foreach (var action in SiPaye)
                context.Appliquer(action);
    }
}
