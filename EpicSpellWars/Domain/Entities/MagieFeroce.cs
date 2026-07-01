using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Domain.Entities;

// Magie féroce : remplace n'importe quelle carte d'un sort. C'est une CarteSort (Type null = sans type, Glyphe
// Aucun = hors comptage de Glyphes, Initiative 0) pour pouvoir vivre dans la main et le sort déclaré comme
// n'importe quel composant. Le joueur DÉCLARE le type qu'il remplace (TypeRemplace) ; à la résolution,
// GameContext.ResoudreMagieFeroceDuSort la remplace par une vraie carte révélée de la pioche (de ce type).
public class MagieFeroce() : CarteSort("Magie Féroce", type: null, glyphe: Glyphe.Aucun)
{
    public TypeComposant? TypeRemplace { get; set; }
}
