using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class Tresor(string nom, List<IEffet> effets, TriggerType triggerType) : Carte(nom, effets)
{
    public TriggerType TriggerType { get; } = triggerType;

    // Passif data-driven (modif au goulot) : 🩸 gagné EN PLUS des +3 du kill, à chaque fois que le porteur
    // tue un adversaire (Liste du Père Fouettard = 1). Lu dans GameContext.OnMort. 0 = pas de bonus.
    public int BonusSangParKill { get; init; }

    // Capacité ACTIVÉE « Payez X 🩸 : <effet> », utilisable au tour d'Initiative du porteur (gère son propre
    // paiement via TenterPayerTresor). null = Trésor sans capacité activée. Déclenchée par l'ordonnanceur.
    public IEffet? Activation { get; init; }
}
