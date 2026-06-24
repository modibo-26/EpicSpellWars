using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 25 Tresors (permanents face visible, chacun en 1 exemplaire). Texte = data/tresors.json (charge
// par TexteLoader, jointure Id). 2e PILIER (declencheurs) — modes de declenchement :
//   Immediat (TriggerType)   = effet one-shot « Lorsque vous gagnez ce Tresor » (DeclencherTresor a GagnerTresor).
//   Activation (Tresor.Activation) = capacite payante « Payez X 🩸 » au tour d'Initiative (phase Standby).
//   Clauses (Tresor.Clauses) = clauses de PHASE DebutTour / FinTour (pipeline de l'ordonnanceur).
//   Passif data-driven       = champ lu au goulot concerne (BonusSangParKill dans OnMort, BonusSangDonjonFinTour).
// ENCODES : Braguette (DebutTour), Gang du Gong/Nachos/Bébé Monstre/Coupe du Tocard (Activation), Bébé Monstre
// (Immediat), Liste du Père Fouettard + Chalisman (passifs), Menottes d'Avarice + Smoking de Location (FinTour).
// Restent Effets=[] : mecaniques non construites (relance de des, redirection, Magie feroce, prediction,
// modif de Glyphes, hooks de Jet, declencheurs mort/kill, Mains Poisseuses/Vers Pas Solitaires).
public static class Tresors
{
    public static List<Tresor> Toutes() =>
    [
        // DebutTour : au début de chacun de vos tours, volez 1 🩸 à n'importe quel adversaire.
        new("Braguette de Cthulhu", [], TriggerType.Passif)
        {
            Id = "EP2-146",
            Clauses =
            [
                new ClausePhase
                {
                    Phase = PhaseTour.DebutTour,
                    Effet = new EffetSimple { Actions = [new Action { Type = TypeAction.VolerSang, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurFixe(1) }] },
                },
            ],
        },
        // Capacité activée : Payez 2 🩸 → 1 dégât à un ennemi par tranche complète de 5 PV qu'il possède.
        new("Gang du Gong", [], TriggerType.Passif)
        {
            Id = "EP2-154",
            Activation = new EffetActivationTresor
            {
                Cout = 2,
                Libelle = "1 dégât par tranche de 5 PV de la cible",
                SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurParTrancheDePv(1, 5) }],
            },
        },
        // Passif (Magie féroce) : un joker révèle 2 cartes du type cherché au lieu d'1, ajoutées au sort.
        new("Divan le Terrible", [], TriggerType.Passif) { Id = "EP2-155", JokerTrouveDeux = true },
        new("Dés Pipés", [], TriggerType.Passif) { Id = "EP2-156" },
        // Immediat : Prenez le Donjon. Passif : +1 🩸 supplémentaire au gain « Donjon » de fin de tour (BonusSangDonjonFinTour).
        new("Chalisman",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }] }],
            TriggerType.Immediat) { Id = "EP2-157", BonusSangDonjonFinTour = 1 },
        // GAP : « égalité à l'initiative, payez 1 pour gagner » = hook de résolution d'ordre (hors pipeline de phases).
        new("Vers Pas Solitaires", [], TriggerType.Passif) { Id = "EP2-158" },
        new("Dissuasion Nucléaire", [], TriggerType.Passif) { Id = "EP2-159" },
        new("Baguette Bicéphale", [], TriggerType.Passif) { Id = "EP2-160" },
        new("Manuel de Cryptozoic", [], TriggerType.Passif) { Id = "EP2-161" },
        new("Pièces du Destin", [], TriggerType.Immediat) { Id = "EP2-162" },
        new("Buffet à Volonté", [], TriggerType.Passif) { Id = "EP2-163" },
        // Capacité activée : Payez 3 🩸 → soignez-vous de 2 PV.
        new("Nachos de la Rage", [], TriggerType.Passif)
        {
            Id = "EP2-164",
            Activation = new EffetActivationTresor
            {
                Cout = 3,
                Libelle = "Soignez-vous de 2 PV",
                SiPaye = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurFixe(2) }],
            },
        },
        // Passif (Payez 3) : après avoir résolu une Créature de votre sort, payez 3 🩸 pour la GARDER.
        new("Bœuf aux Hormones", [], TriggerType.Passif) { Id = "EP2-165", GarderCreaturePayant = true },
        // Passif : si votre mort met fin à la manche, +3 🩸 (BonusSangMortFinManche, lu dans OnMort).
        // Capacité activée : Payez 1 🩸 → soignez-vous de 1 PV (1×/tour, assuré par la limite de paiement Trésor).
        new("Coupe du Tocard", [], TriggerType.Passif)
        {
            Id = "EP2-167",
            BonusSangMortFinManche = 3,
            Activation = new EffetActivationTresor
            {
                Cout = 1,
                Libelle = "Soignez-vous de 1 PV",
                SiPaye = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurFixe(1) }],
            },
        },
        // Immediat (partie one-shot) : 1 dégât à chaque sorcier (soi inclus), puis +2 🩸 par sorcier à 5 PV
        // ou moins. Capacité activée : Payez 4 🩸 → 2 dégâts à n'importe quel adversaire.
        new("Bébé Monstre",
            [new EffetSimple { Actions =
            [
                new Action { Type = TypeAction.Degats, Cible = Cible.TousSorciers, Valeur = new ValeurFixe(1) },
                new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurParSorcierFaible(2, 5) },
            ] }],
            TriggerType.Immediat)
        {
            Id = "EP2-168",
            Activation = new EffetActivationTresor
            {
                Cout = 4,
                Libelle = "2 dégâts à n'importe quel adversaire",
                SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurFixe(2) }],
            },
        },
        // FinTour : si vous êtes le dernier à jouer dans l'ordre du tour, gagnez un Trésor.
        new("Menottes d'Avarice", [], TriggerType.Passif)
        {
            Id = "EP2-169",
            Clauses =
            [
                new ClausePhase
                {
                    Phase = PhaseTour.FinTour,
                    Condition = ctx => ctx.LanceurEstDernierAJouer,
                    Effet = new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi }] },
                },
            ],
        },
        new("Fusil à Triple Canon", [], TriggerType.Passif) { Id = "EP2-171" },
        // Immediat : placez une prime au milieu + gagnez un autre Trésor ; le prochain tueur gagne 3 🩸 (OnMort).
        new("Avis de Recherche", [new EffetAvisDeRecherche()], TriggerType.Immediat) { Id = "EP2-172" },
        // GAP : « si premier à jouer, échangez CE Trésor contre un Trésor adverse » = besoin de l'identité du
        // Trésor porteur de la clause (échange de cette carte précise) + action d'échange. Phase DebutTour prête.
        new("Mains Poisseuses !", [], TriggerType.Passif) { Id = "EP2-173" },
        // Passif data-driven : +1 🩸 (en plus des +3) à chaque kill du porteur — lu dans OnMort.
        new("Liste du Père Fouettard", [], TriggerType.Passif) { Id = "EP2-174", BonusSangParKill = 1 },
        // Passif (hook de Jet) : +1 🩸 à chaque fois que vous obtenez 13 ou plus à un Jet de puissance.
        new("Chipodada", [], TriggerType.Passif) { Id = "EP2-175", SeuilBonusSangJet = 13 },
        // Passif (hook de Jet) : +1 aux dégâts qu'une de vos Créatures inflige à la suite d'un Jet de puissance.
        new("Granoloup", [], TriggerType.Passif) { Id = "EP2-176", BonusDegatsCreatureJet = 1 },
        // Passif : à chaque fois qu'un adversaire pioche un crevé, lancez un dé ; 5-6 → soin 1 PV (PiocherSorcierCreve).
        new("Bouclier Anti-Fiente", [], TriggerType.Passif) { Id = "EP2-177", SoigneSurPiocheCreveAdverse = true },
        new("Globe Sacrificiel", [], TriggerType.Passif) { Id = "EP2-178" },
        // FinTour : si vous êtes (à égalité) le sorcier le plus faible en fin de tour, soin 2 PV + gagnez un Trésor.
        new("Smoking de Location", [], TriggerType.Passif)
        {
            Id = "EP2-179",
            Clauses =
            [
                new ClausePhase
                {
                    Phase = PhaseTour.FinTour,
                    Condition = ctx => ctx.LanceurEstLePlusFaible,
                    Effet = new EffetSimple { Actions =
                    [
                        new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurFixe(2) },
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi },
                    ] },
                },
            ],
        },
    ];
}
