using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Une clause d'un Trésor déclenchée à une PHASE de tour donnée (pipeline OrdonnanceurDeTour), si sa Condition
// est remplie. La clause s'exécute du POINT DE VUE de son porteur (Lanceur temporaire), comme un Trésor au
// déclenchement. Condition null = toujours. Ex. Menottes d'Avarice = (FinTour, « je suis le dernier à jouer »,
// GagnerTresor) ; Smoking de Location = (FinTour, « je suis le plus faible », Soin + GagnerTresor).
public class ClausePhase
{
    public PhaseTour Phase { get; init; }
    public Predicate<GameContext>? Condition { get; init; }
    public required IEffet Effet { get; init; }
}
