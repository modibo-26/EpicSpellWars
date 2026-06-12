using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Magie féroce (1 carte unique, 8 exemplaires). Joker : remplace n'importe quelle carte d'un sort,
// révèle la pioche jusqu'au type remplacé (Initiative 0, mécanique de résolution spéciale = pilier 2).
// Texte = data/magie_feroce.json (TexteLoader, jointure Id). Effets vides ; pas de TriggerType.
public static class MagiesFeroces
{
    public static List<MagieFeroce> Toutes() =>
    [
        new() { Id = "EP2-123", Exemplaires = 8 },
    ];
}
