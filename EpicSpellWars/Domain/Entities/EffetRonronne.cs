using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Sorcier crevé « Ronronne en Paix » (MancheSuivante) : au début de la manche suivante, révèle des cartes
// de la pioche principale jusqu'à trouver une Créature, et la met EN JEU devant le propriétaire (les autres
// cartes révélées sont défaussées par RevelerPiocheJusqua). Déclenché en différé (EffetsDifferes) du point
// de vue du propriétaire (Lanceur courant) lors de DebutManche.
public class EffetRonronne : IEffet
{
    public void Execute(GameContext context)
    {
        var creature = context.RevelerPiocheJusqua(c => c.EstCreature);
        if (creature is not null)
            context.Lanceur.Creatures.Add(creature);
    }
}
