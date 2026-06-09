using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Catalogue;

// Sorcier crevé — 8 cartes uniques / 25 cartes au total : 1 carte x2 + 5 cartes x3 + 2 cartes x4.
// La multiplicité par carte est portée ici (Exemplaires) ; Creer() développe la pile.
// TODO effets : remplir Effets + fixer les Exemplaires réels une fois les cartes physiques relevées.
public static class SorciersCreveCatalogue
{
    // (fabrique d'instance fraiche, nb d'exemplaires dans la pioche)
    public static List<(Func<SorcierCreve> Fabrique, int Exemplaires)> Uniques() =>
    [
        (() => new SorcierCreve("Sorcier sous terre", [], TriggerType.Immediat)
        {
            Texte = "Immédiat : Prenez le Donjon. Si vous contrôlez le Donjon alors que vous êtes mort à la fin d'un tour, "
                  + "gagnez 4 Sang au lieu de 1.",
        }, 3), // TODO: exemplaires réels à confirmer
        // TODO: compléter jusqu'à 8 uniques (1x2 + 5x3 + 2x4 = 25 cartes).
    ];

    public static List<SorcierCreve> Creer()
    {
        var pile = new List<SorcierCreve>();
        foreach (var (fabrique, exemplaires) in Uniques())
            for (var i = 0; i < exemplaires; i++)
                pile.Add(fabrique());
        return pile;
    }
}
