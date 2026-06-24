using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Conditionnelle de « Repos Mérité » : « Si vous n'avez aucun jeton Dernier Survivant, gagnez 1 Trésor au
// début de la prochaine manche. » (La partie de base « +1 🩸 par jeton » est un EffetSimple séparé.) Au
// déclenchement (immédiat, du point de vue du mort), si le propriétaire n'a aucun jeton, on EMPILE un gain
// de Trésor en effet différé (joué à DebutManche, comme les crevés MancheSuivante).
public class EffetReposMerite : IEffet
{
    public void Execute(GameContext context)
    {
        if (context.Lanceur.JetonsDernierSurvivant != 0)
            return;

        var proprietaire = context.Lanceur;
        context.EffetsDifferes.Add(
            ([new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi, Valeur = new ValeurFixe(1) }] }],
             proprietaire));
    }
}
