using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 25 Tresors (permanents face visible, chacun en 1 exemplaire). Texte = data/tresors.json (charge
// par TexteLoader, jointure Id). 2e PILIER (declencheurs) — tranche D : modele d'abonnement par TriggerType.
//   SurInitiative = se declenche a votre tour / sur l'ordre du tour (premier/dernier a jouer, egalite).
//   Immediat      = effet one-shot « Lorsque vous gagnez ce Tresor ».
//   Passif        = reactif/conditionnel permanent ou capacite activee payante.
// ENCODES (representants tranche D) : Braguette de Cthulhu (SurInitiative debut de tour), Bébé Monstre
// (Immediat, partie one-shot), Liste du Père Fouettard (passif data-driven BonusSangParKill, lu dans OnMort).
// Les autres restent Effets=[] : ils dependent de mecaniques non construites (relance de des, redirection,
// capacites ACTIVEES a une phase dediee, Magie feroce, prediction, modif de Glyphes, ordre du tour).
public static class Tresors
{
    public static List<Tresor> Toutes() =>
    [
        // SurInitiative : au début de chacun de vos tours, volez 1 🩸 à n'importe quel adversaire.
        new("Braguette de Cthulhu",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.VolerSang, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurFixe(1) }] }],
            TriggerType.SurInitiative) { Id = "EP2-146" },
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
        new("Divan le Terrible", [], TriggerType.Passif) { Id = "EP2-155" },
        new("Dés Pipés", [], TriggerType.Passif) { Id = "EP2-156" },
        new("Chalisman", [], TriggerType.Passif) { Id = "EP2-157" },
        new("Vers Pas Solitaires", [], TriggerType.SurInitiative) { Id = "EP2-158" },
        new("Dissuasion Nucléaire", [], TriggerType.Passif) { Id = "EP2-159" },
        new("Baguette Bicéphale", [], TriggerType.Passif) { Id = "EP2-160" },
        new("Manuel de Cryptozoic", [], TriggerType.SurInitiative) { Id = "EP2-161" },
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
        new("Bœuf aux Hormones", [], TriggerType.Passif) { Id = "EP2-165" },
        // Capacité activée : Payez 1 🩸 → soignez-vous de 1 PV (1×/tour, assuré par la limite de paiement
        // Trésor). GAP : la clause « si votre mort met fin à la manche, +3 🩸 » = déclencheur de mort (à part).
        new("Coupe du Tocard", [], TriggerType.Passif)
        {
            Id = "EP2-167",
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
        new("Menottes d'Avarice", [], TriggerType.SurInitiative) { Id = "EP2-169" },
        new("Fusil à Triple Canon", [], TriggerType.Passif) { Id = "EP2-171" },
        new("Avis de Recherche", [], TriggerType.Immediat) { Id = "EP2-172" },
        new("Mains Poisseuses !", [], TriggerType.SurInitiative) { Id = "EP2-173" },
        // Passif data-driven : +1 🩸 (en plus des +3) à chaque kill du porteur — lu dans OnMort.
        new("Liste du Père Fouettard", [], TriggerType.Passif) { Id = "EP2-174", BonusSangParKill = 1 },
        new("Chipodada", [], TriggerType.Passif) { Id = "EP2-175" },
        new("Granoloup", [], TriggerType.Passif) { Id = "EP2-176" },
        new("Bouclier Anti-Fiente", [], TriggerType.Passif) { Id = "EP2-177" },
        new("Globe Sacrificiel", [], TriggerType.Passif) { Id = "EP2-178" },
        new("Smoking de Location", [], TriggerType.SurInitiative) { Id = "EP2-179" },
    ];
}
