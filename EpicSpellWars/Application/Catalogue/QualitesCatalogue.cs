using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Catalogue;

// Qualités — 20 cartes uniques (5 Glyphes x 4), chacune en 2 exemplaires dans la pioche.
// TODO effets : remplir Effets une fois le resolveur GameContext.Appliquer disponible.
// NB : textes semes depuis le rulebook — a verifier sur les cartes physiques (icones Sang/Trésor).
public static class QualitesCatalogue
{
    public static List<CarteSort> Creer() =>
    [
        new("Poilcramus", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Texte = "Choisissez un adversaire qui doit révéler sa main. Il subit autant de dégâts que le double du nombre de Créatures dans sa main. "
                  + "Payez 3 Sang : L'effet s'applique à chaque adversaire au lieu d'un.",
        },
        new("Fourchétix", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Texte = "Prenez le Donjon et donnez-le à un adversaire qui ne l'avait pas. Puis infligez 3 dégâts à cet adversaire. "
                  + "Payez 3 Sang : Tuez d'abord toutes ses Créatures.",
        },
        new("Gromago", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Texte = "Gagnez 2 Trésors. Chaque adversaire qui n'a pas encore joué ce tour gagne 1 Trésor. "
                  + "Payez 2 Sang : Chaque adversaire qui a déjà joué ce tour doit défausser 1 de ses Trésors.",
        },
        new("Groclonar", TypeComposant.Qualite, Glyphe.Illusion)
        {
            Texte = "Choisissez une option : infligez 2 dégâts à chaque adversaire avec un nombre pair de PV "
                  + "ou infligez 3 dégâts à chaque adversaire avec un nombre impair de PV. Donjon : Faites les deux (pair puis impair).",
        },
        new("Chancedecocus", TypeComposant.Qualite, Glyphe.Tenebres)
        {
            Texte = "Chaque adversaire lance 1 dé. Celui qui obtient le plus grand résultat gagne 1 Sang et subit autant de dégâts que son résultat. "
                  + "Donjon : Chaque autre adversaire subit également autant de dégâts que son résultat.",
        },
        new("Bégoniax", TypeComposant.Qualite, Glyphe.Primaire)
        {
            Texte = "Prenez le Donjon et infligez 1 dégât par sorcier mort à chaque adversaire. Si aucun sorcier n'a été tué, gagnez 2 Trésors. "
                  + "[symbole gain à vérifier sur la carte physique]",
        },
        // TODO: compléter jusqu'à 20 uniques (actuellement 6).
    ];
}
