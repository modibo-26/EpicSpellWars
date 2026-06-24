using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Sorcier crevé « Baisse de Tension » (MancheSuivante) : « Lorsque vous jouerez votre premier sort de la
// prochaine manche, ajoutez-lui la 1re carte de la pioche (si Magie féroce, prenez-la en main et ajoutez la
// suivante). » Déclenché en différé à DebutManche, il ne fait qu'ARMER le propriétaire : l'augmentation
// proprement dite (AugmenterSortDepuisPioche) a lieu à la déclaration de son premier sort de la manche.
public class EffetBaisseDeTension : IEffet
{
    public void Execute(GameContext context) => context.Lanceur.AugmenterPremierSort = true;
}
