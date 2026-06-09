using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Révélez des cartes de la pioche jusqu'à trouver <Critère>, puis ajoutez-la à votre sort »
// (Peutidardus, Cadopourrix payé : Critère = Créature). Repete `Nombre` fois (Peutidardus Donjon = 2).
// La carte ajoutee au sort est reprise par GameContext.ResoudreSort a son rang de type, comme une
// carte ajoutee par GagnerCarte. Les revelees non retenues sont defaussees par la routine.
public class EffetRevelerPioche : IEffet
{
    public Predicate<CarteSort> Critere { get; set; } = c => c.EstCreature;
    public int Nombre { get; set; } = 1;
    public DestinationRevele Destination { get; set; } = DestinationRevele.Sort;

    public void Execute(GameContext context)
    {
        for (var i = 0; i < Nombre; i++)
        {
            var carte = context.RevelerPiocheJusqua(Critere);
            if (carte is null)
                break;   // pioche epuisee : on s'arrete (cf. TODO remelange dans RevelerPiocheJusqua)

            if (Destination == DestinationRevele.Sort)
                context.SortEnCours.Add(carte);
            else
                context.Lanceur.Main.Add(carte);
        }
    }
}
