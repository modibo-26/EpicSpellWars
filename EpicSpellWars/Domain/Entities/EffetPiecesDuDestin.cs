using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Pièces du Destin » (Immediat, à l'obtention) : annoncez le prochain sorcier qui sera tué. Si l'annonce est
// juste (la 1re mort qui suit le correspond), gagnez 2 🩸 (même si c'est vous). La prédiction est stockée sur
// le contexte (GameContext.Prediction) et résolue dans OnMort à la première mort.
public class EffetPiecesDuDestin : IEffet
{
    public void Execute(GameContext context)
    {
        var vivants = context.Sorciers.Where(s => s.EstVivant).ToList();
        if (vivants.Count == 0)
            return;
        context.Prediction = (context.Lanceur, context.ChoisirCible(vivants));
    }
}
