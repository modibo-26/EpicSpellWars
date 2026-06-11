// Carte de base

using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public abstract class Carte(string nom, List<IEffet>? effets = null)
{
    // Identifiant carte (EP2-xxx) ; cle de jointure avec data/*.json (texte verbatim charge par TexteLoader).
    public string Id { get; init; } = "";

    public string Nom { get; } = nom;

    // Nombre d'exemplaires de cette carte dans la pioche (chiffre imprime / compo du deck).
    public int Exemplaires { get; init; } = 1;

    // Texte imprime sur la carte (verbatim, pour l'UI) ; Effets en est l'encodage executable.
    public string Texte { get; set; } = "";

    public List<IEffet> Effets { get; init; } = effets ?? [];
}

