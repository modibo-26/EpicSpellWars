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
}
