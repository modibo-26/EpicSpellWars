using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 8 Sorciers crevés (piochés à la mort ; exemplaires variables). Texte = data/sorciers_creves.json
// (TexteLoader, jointure Id). DÉCLENCHEUR « pioché à la mort » CÂBLÉ (tranche E, GameContext.PiocherSorcierCreve) :
// Immediat = effet tout de suite (du point de vue du mort) ; MancheSuivante = différé à DebutManche ;
// Passif (Petit Ange) = conservé. Les 4 Immédiat à Actions pures sont actifs ; conditionnelles/différés en GAP.
//   // GAP   = clause conditionnelle / valeur pas encore exprimable.
//   // TODO  = effet « manche suivante » dépendant du flux de manche (vainqueur, mise en jeu), Effets vides.
public static class SorciersCreves
{
    public static List<SorcierCreve> Toutes() =>
    [
        // « Gardez jusqu'à votre prochain Jet pour une Créature : +1 dé » = modificateur BonusDesJet (GAP, cf. Shub-Niggurath).
        new("Petit Ange Parti Trop Tôt", [], TriggerType.Passif) { Id = "EP2-130", Exemplaires = 3 },

        // Immédiat : +2 🩸 (base encodée). // GAP : +2 🩸 si premier tué de la manche.
        new("Tournée d'Adieu",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurFixe(2) }] }],
            TriggerType.Immediat) { Id = "EP2-134", Exemplaires = 3 },

        // Immédiat : volez 1 🩸 à chaque sorcier vivant (= TousAdversaires, le lanceur est mort). Complet.
        new("Bilan Sanguin",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.VolerSang, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) }] }],
            TriggerType.Immediat) { Id = "EP2-139", Exemplaires = 3 },

        // Immédiat : prenez le Donjon (base encodée). // GAP : +4 🩸 (au lieu de 1) si Donjon en fin de tour.
        new("Sorcier sous Terre",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }] }],
            TriggerType.Immediat) { Id = "EP2-140", Exemplaires = 3 },

        // Immédiat : +1 🩸 par jeton Dernier Survivant (base encodée). // GAP : si aucun, 1 Trésor la manche suivante.
        new("Repos Mérité",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurParJeton(1) }] }],
            TriggerType.Immediat) { Id = "EP2-144", Exemplaires = 4 },

        // MancheSuivante : le vainqueur de la manche pioche 2 cartes de moins au début de la suivante (EffetDoigtMagique).
        new("Doigt Magique", [new EffetDoigtMagique()], TriggerType.MancheSuivante) { Id = "EP2-145", Exemplaires = 4 },

        // MancheSuivante : révéler la pioche jusqu'à une Créature et la mettre en jeu devant soi (EffetRonronne).
        new("Ronronne en Paix", [new EffetRonronne()], TriggerType.MancheSuivante) { Id = "EP2-149", Exemplaires = 3 },

        // MancheSuivante : arme l'augmentation du 1er sort de la manche suivante (1re carte de pioche, gestion
        // Magie féroce incluse côté ordonnanceur via AugmenterSortDepuisPioche). Voir EffetBaisseDeTension.
        new("Baisse de Tension", [new EffetBaisseDeTension()], TriggerType.MancheSuivante) { Id = "EP2-152", Exemplaires = 2 },
    ];
}
