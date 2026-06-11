using System.Reflection;
using System.Text.Json;
using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Pont catalogue C# <-> data/*.json : le JSON est la source unique du texte verbatim ; le C# encode
// les effets. Jointure par Id (EP2-xxx). Appele une fois a l'assemblage de la pioche.
public static class TexteLoader
{
    // id -> texte, charge une seule fois depuis les JSON embarques (EmbeddedResource, cf. .csproj).
    private static readonly Lazy<Dictionary<string, string>> Textes = new(Charger);

    // Renseigne Texte sur chaque carte dont l'Id matche une entree JSON. Les autres gardent "".
    public static IReadOnlyList<Carte> AppliquerTextes(IReadOnlyList<Carte> cartes)
    {
        foreach (var carte in cartes)
            if (Textes.Value.TryGetValue(carte.Id, out var texte))
                carte.Texte = texte;
        return cartes;
    }

    private static Dictionary<string, string> Charger()
    {
        var asm = Assembly.GetExecutingAssembly();
        var textes = new Dictionary<string, string>();

        foreach (var ressource in asm.GetManifestResourceNames().Where(n => n.EndsWith(".json")))
        {
            using var flux = asm.GetManifestResourceStream(ressource)!;
            var cartes = JsonSerializer.Deserialize<List<JsonElement>>(flux) ?? [];
            foreach (var carte in cartes)
            {
                var id = carte.GetProperty("id").GetString()!;
                textes[id] = carte.GetProperty("texte").GetString()!;
            }
        }

        return textes;
    }
}
