// Carte de base

using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public abstract class Carte(string nom, List<IEffet>? effets = null)
{
    public string Nom { get; set; } = nom;

    // Texte imprime sur la carte (source de verite) ; Effets en est l'encodage.
    public string Texte { get; set; } = "";

    public List<IEffet> Effets { get; set; } = effets ?? [];
}

