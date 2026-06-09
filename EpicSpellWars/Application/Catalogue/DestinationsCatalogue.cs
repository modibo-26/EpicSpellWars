using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Catalogue;

// Destinations — 20 cartes uniques (5 Glyphes x 4), chacune en 2 exemplaires dans la pioche.
// Toutes les Destinations sont des Créatures (Type == Destination) et portent un Jet de puissance
// (Initiative non nulle). Effets = EffetJetDePuissance avec ses tranches (GARDEZ).
// TODO effets : remplir Effets une fois le resolveur GameContext.Appliquer disponible.
// TROU CONNU : Élémentaire = 3/4 uniques (1 carte manquante a retrouver).
public static class DestinationsCatalogue
{
    public static List<CarteSort> Creer() =>
    [
        new("Gaztoxicus", TypeComposant.Destination, Glyphe.Illusion, initiative: 20)
        {
            Texte = "Cible : Adversaire qui a déjà joué ce tour. Jet de puissance — 1-4 : 1 dégât. GARDEZ. | 5-9 : 2 dégâts. | 10+ : 4 dégâts. "
                  + "Payez 2 Sang : Vous pouvez résoudre chaque dé de ce Jet individuellement contre des adversaires différents.",
        },
        new("Mecha-satana", TypeComposant.Destination, Glyphe.Tenebres, initiative: 3)
        {
            Texte = "Cible : Adversaire le plus faible. Jet de puissance — 1-4 : 1 dégât. | 5-9 : 2 dégâts et subissez 1 dégât. | "
                  + "10+ : 4 dégâts puis 2 dégâts à votre adversaire le plus fort. GARDEZ.",
        },
        new("Shub-niggurath", TypeComposant.Destination, Glyphe.Tenebres, initiative: 6)
        {
            Texte = "Cible : Adversaire qui a déjà joué ce tour. Jet de puissance — 1-4 : Aucun effet. | 5-9 : 1 dégât. | "
                  + "10+ : 2 dégâts et jouez une autre Créature de votre main. GARDEZ. "
                  + "Payez 1 Sang : Ajoutez 1 dé à chacun de vos Jets de puissance pour une Créature ce tour.",
        },
        new("Écrabouillax", TypeComposant.Destination, Glyphe.Arcane, initiative: 1)
        {
            Texte = "Cible : Adversaire avec le plus de Trésors. Jet de puissance — 1-4 : 1 dégât. | "
                  + "5-9 : 3 dégâts et la cible doit défausser 1 de ses Trésors. | 10+ : 7 dégâts.",
        },
        new("Golemacifas", TypeComposant.Destination, Glyphe.Elementaire, initiative: 7)
        {
            Texte = "Cible : Adversaire le plus fort. Jet de puissance — 1-4 : 1 dégât. | 5-9 : 2 dégâts. | 10+ : 1 dé de dégâts. GARDEZ.",
        },
        new("Coco-cocoricus", TypeComposant.Destination, Glyphe.Primaire, initiative: 10)
        {
            Texte = "Cible : Adversaire le plus fort. Jet de puissance — 1-4 : GARDEZ. | 5-9 : 2 dégâts. | 10+ : 4 dégâts.",
        },
        // TODO: compléter jusqu'à 20 uniques (actuellement 6 ; Élémentaire = 1/4 ici, dont 1 trou physique).
    ];
}
