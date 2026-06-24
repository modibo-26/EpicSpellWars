using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Buffet à Volonté » (Immediat, à l'obtention) : choisissez 1 Composant de votre main et glissez-le sous ce
// Trésor — son Glyphe compte alors dans CHACUN de vos sorts (lu par GameContext.CompterGlyphes via
// Sorcier.SousBuffet). Simplification : la carte est rangée sur le Sorcier (SousBuffet), sans lien avec
// l'instance précise du Trésor.
public class EffetBuffet : IEffet
{
    public void Execute(GameContext context)
    {
        var main = context.Lanceur.Main;
        if (main.Count == 0)
            return;

        foreach (var carte in context.ChoisirCartes(main, 1, 1).ToList())
        {
            main.Remove(carte);
            context.Lanceur.SousBuffet.Add(carte);
        }
    }
}
