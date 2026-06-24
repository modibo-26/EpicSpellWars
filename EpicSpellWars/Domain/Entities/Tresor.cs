using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class Tresor(string nom, List<IEffet> effets, TriggerType triggerType) : Carte(nom, effets)
{
    public TriggerType TriggerType { get; } = triggerType;

    // Passif data-driven (modif au goulot) : 🩸 gagné EN PLUS des +3 du kill, à chaque fois que le porteur
    // tue un adversaire (Liste du Père Fouettard = 1). Lu dans GameContext.OnMort. 0 = pas de bonus.
    public int BonusSangParKill { get; init; }

    // Capacité ACTIVÉE « Payez X 🩸 : <effet> », utilisable au tour d'Initiative du porteur (gère son propre
    // paiement via TenterPayerTresor). null = Trésor sans capacité activée. Déclenchée par l'ordonnanceur.
    public IEffet? Activation { get; init; }

    // Clauses déclenchées à une PHASE de tour (DebutTour / FinTour), exécutées par le pipeline de
    // l'ordonnanceur du point de vue du porteur (cf. [[tresors-effets-speciaux]]).
    public List<ClausePhase> Clauses { get; init; } = [];

    // Passif data-driven : 🩸 EN PLUS du gain « Donjon » de fin de tour quand le porteur contrôle le Donjon
    // (Chalisman = 1). Lu dans OrdonnanceurDeTour. 0 = pas de bonus.
    public int BonusSangDonjonFinTour { get; init; }

    // Passif data-driven : 🩸 gagné par le porteur si SA mort met fin à la manche (Coupe du Tocard = 3).
    // Lu dans GameContext.OnMort. 0 = pas de bonus.
    public int BonusSangMortFinManche { get; init; }

    // Passif data-driven : à chaque fois qu'un ADVERSAIRE du porteur pioche un Sorcier crevé, le porteur lance
    // un dé et se soigne de 1 PV sur 5-6 (Bouclier Anti-Fiente). Lu dans GameContext.PiocherSorcierCreve.
    public bool SoigneSurPiocheCreveAdverse { get; init; }

    // Passif data-driven (hooks de Jet de puissance, lus par EffetJetDePuissance) :
    //   SeuilBonusSangJet     : +1 🩸 si le résultat du Jet atteint ce seuil (Chipodada = 13). 0 = pas de bonus.
    //   BonusDegatsCreatureJet : +n à chaque instance de dégâts d'un Jet du porteur (Granoloup = 1).
    public int SeuilBonusSangJet { get; init; }
    public int BonusDegatsCreatureJet { get; init; }

    // Divan le Terrible : un joker Magie féroce révèle 2 cartes du type au lieu d'1 (lu dans ResoudreJokersDuSort).
    public bool JokerTrouveDeux { get; init; }

    // Bœuf aux Hormones : après avoir résolu une Créature, le porteur peut payer 3 🩸 pour la GARDER
    // (lu dans GameContext.ResoudreComposant). Coût de Trésor (1×/tour).
    public bool GarderCreaturePayant { get; init; }

    // Relances de dés sur un Jet de puissance (lues dans GameContext.AppliquerRelancesJet) :
    //   RelanceLesUns    : relance auto tous les 1 (Dés Pipés).
    //   RelanceJetEntier : option de relancer TOUT le jet, 1×/tour (Manuel de Cryptozoic).
    //   RelanceUnDePayant : Payez 2 🩸 → relance le plus petit dé (Globe Sacrificiel).
    public bool RelanceLesUns { get; init; }
    public bool RelanceJetEntier { get; init; }
    public bool RelanceUnDePayant { get; init; }

    // Fusil à Triple Canon : +1 🩸 par série complète de 3 Glyphes identiques dans le sort (lu en fin de
    // ResoudreSort). Buffet à Volonté n'a pas de champ : son effet est l'Immediat EffetBuffet + Sorcier.SousBuffet.
    public bool SangParTroisGlyphes { get; init; }

    // Redirection de ciblage (lues dans GameContext) :
    //   InverseGaucheDroite     : Baguette Bicéphale — le porteur peut inverser gauche↔droite pour tout son sort.
    //   RedirigeSortSeuleCible  : Dissuasion Nucléaire — si le porteur est la SEULE cible d'un sort adverse, il
    //     peut payer 3 🩸 pour rediriger le sort vers un autre sorcier (pas le lanceur).
    public bool InverseGaucheDroite { get; init; }
    public bool RedirigeSortSeuleCible { get; init; }

    // Vers Pas Solitaires : à égalité d'initiative, le porteur peut payer 1 🩸 pour remporter l'égalité
    // (priorité de départage, lue dans OrdonnanceurDeTour). Mains Poisseuses n'a pas de champ : clause DebutTour
    // + EffetMainsPoisseuses ; Pièces du Destin : Immediat EffetPiecesDuDestin + GameContext.Prediction.
    public bool RemporteEgaliteInitiativePayant { get; init; }
}
