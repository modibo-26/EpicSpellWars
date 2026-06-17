using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Domain.Entities;

// Joker : remplace n'importe quelle carte d'un sort. Le joueur DÉCLARE le type qu'il remplace
// (TypeRemplace) ; GameContext.ResoudreMagieFeroce révèle la pioche jusqu'à une carte de ce type.
public class MagieFeroce() : Carte("Magie Féroce")
{
    public TypeComposant? TypeRemplace { get; set; }
}
