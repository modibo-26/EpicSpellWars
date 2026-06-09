using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Catalogue;

// Trésors — 25 cartes uniques, 1 exemplaire chacune.
// Effets speciaux : permanents face visible (passifs / au tour d'Initiative), hors flux de resolution du sort.
// TODO effets : remplir Effets + affiner TriggerType une fois le resolveur disponible.
public static class TresorsCatalogue
{
    public static List<Tresor> Creer() =>
    [
        new("Chalisman", [], TriggerType.Passif)
        {
            Texte = "Prenez le Donjon. Chaque fois que vous gagnez du Sang parce que vous contrôlez le Donjon en fin de tour, "
                  + "gagnez 1 Sang supplémentaire.",
        },
        // TODO: compléter jusqu'à 25 uniques (actuellement 1).
    ];
}
