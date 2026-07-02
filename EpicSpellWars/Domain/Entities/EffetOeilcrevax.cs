using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Oeilcrevax (IEffet sur-mesure) : « Infligez 3 dégâts à votre adversaire le plus fort. S'il bloque avec une
// Créature, gagnez 1 🩸. » On remet à zéro le signal de blocage, on inflige les dégâts (via Appliquer →
// InfligerDegats, qui arme DerniereInstanceBloquee si la cible sacrifie une Créature), puis on lit le signal.
public class EffetOeilcrevax : IEffet
{
    public void Execute(GameContext context)
    {
        context.DerniereInstanceBloquee = false;
        context.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(3) });
        if (context.DerniereInstanceBloquee)
            context.Lanceur.Sang = Math.Min(context.Lanceur.SangMax, context.Lanceur.Sang + 1);
    }
}
