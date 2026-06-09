using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Catalogue;

// Sources — 20 cartes uniques (5 Glyphes x 4), chacune en 2 exemplaires dans la pioche.
// Creer() renvoie des instances FRAICHES (chaque appel = un nouveau jeu de cartes).
// TODO effets : remplir Effets une fois le resolveur GameContext.Appliquer disponible.
// NB : textes semes depuis le rulebook — a verifier sur les cartes physiques (icones Sang/Trésor).
public static class SourcesCatalogue
{
    public static List<CarteSort> Creer() =>
    [
        new("Taléboulas", TypeComposant.Source, Glyphe.Arcane)
        {
            Texte = "Infligez 3 dégâts à votre adversaire de gauche. Payez 2 : Infligez 3 dégâts à chaque adversaire à la place.",
        },
        new("Dépipax", TypeComposant.Source, Glyphe.Arcane)
        {
            Texte = "Lancez 1 dé : 1-2 Prenez le Donjon ; 3-4 Gagnez 1 Trésor ; 5+ Faites les deux. "
                  + "Réaction : Si vous mourez avant que cette carte ne soit résolue, gagnez 1 Trésor et jouez-le au début de la prochaine manche.",
        },
        new("Castoramax", TypeComposant.Source, Glyphe.Tenebres)
        {
            Texte = "Lancez 2 dés. Utilisez-en un pour infliger autant de dégâts que son résultat à votre adversaire de gauche. "
                  + "Subissez autant de dégâts que le résultat de l'autre dé, sauf si vous contrôlez le Donjon ou si vous payez 1 Sang.",
        },
        new("Necrophilus", TypeComposant.Source, Glyphe.Tenebres)
        {
            Texte = "Prenez le Donjon. Payez 1 Sang : Infligez 2 dégâts par jeton Dernier Survivant en jeu à chaque adversaire.",
        },
        new("Flaminus", TypeComposant.Source, Glyphe.Elementaire)
        {
            Texte = "Infligez 1 dégât à chaque adversaire. Payez 5 Sang : Infligez 5 dégâts au lieu de 1.",
        },
        new("Multitax", TypeComposant.Source, Glyphe.Illusion)
        {
            Texte = "Prenez le Donjon. Si vous le prenez à un adversaire vivant, infligez 3 dégâts à un autre adversaire.",
        },
        new("Pèredodux", TypeComposant.Source, Glyphe.Primaire)
        {
            Texte = "Soignez-vous de 1 PV par Créature en jeu. Payez X Sang : Soignez-vous de X PV supplémentaires.",
        },
        // TODO: compléter jusqu'à 20 uniques (actuellement 7).
    ];
    
}
