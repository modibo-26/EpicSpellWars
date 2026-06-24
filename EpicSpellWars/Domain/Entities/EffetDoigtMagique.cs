using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Sorcier crevé « Doigt Magique » (MancheSuivante) : le VAINQUEUR de la manche où ce crevé a été pioché
// pioche 2 cartes de moins au début de la manche suivante. Cible = le dernier survivant mémorisé sur le
// contexte (VainqueurDerniereManche), pas le propriétaire du crevé (un mort). Déclenché en différé à
// DebutManche : on pose une réduction one-shot consommée par le premier CompleterMain de la manche.
public class EffetDoigtMagique : IEffet
{
    public void Execute(GameContext context)
    {
        if (context.VainqueurDerniereManche is { } vainqueur)
            vainqueur.ReductionPiocheProchainTour += 2;
    }
}
