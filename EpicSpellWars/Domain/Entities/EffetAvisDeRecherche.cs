using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Avis de Recherche » (Immediat, à l'obtention) : placez ce Trésor au milieu (= une prime active) et gagnez
// un autre Trésor. Ensuite, le prochain sorcier qui en tue un autre consomme la prime et gagne 3 🩸 (géré dans
// GameContext.OnMort via PrimesEnJeu). Simplification : la carte reste dans les Trésors du porteur (sans autre
// effet) au lieu d'être physiquement déplacée au centre ; la prime fonctionnelle est le compteur PrimesEnJeu.
public class EffetAvisDeRecherche : IEffet
{
    public void Execute(GameContext context)
    {
        context.PrimesEnJeu++;
        context.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });
    }
}
