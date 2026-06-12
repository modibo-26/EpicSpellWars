using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 20 Destinations (Créatures, chacune en 2 exemplaires). Texte = verbatim de la carte
// (= data/destinations.json) ; Effets = encodage executable. Une Destination porte l'Initiative et
// un Jet de puissance (EffetJetDePuissance, Glyphe = celui de la carte) dont les tranches reprennent
// les paliers 1-4 / 5-9 / 10+ (Seuil 1 / 5 / 10) ; « GARDEZ » = PeutGarder sur la tranche concernee.
// Conventions :
//   // TODO Réaction / Donjon : clause relevant d'un autre pilier (declencheurs), pas du resolveur.
//   // GAP : clause non encore exprimable par le resolveur (mecanique a concevoir).
//
// DEUX FAMILLES DE GAP RÉVÉLÉES PAR LES DESTINATIONS — RÉSOLUES (étape 1, manques résolveur) :
//   1. CIBLE UNIQUE PARMI UN FILTRE → flag Action.CibleUnique : le lanceur choisit UN match du filtre
//      (ADejaJoue/SansDonjon/SansCreature) via ChoisirCible. NB ADonjon renvoie déjà au plus 1 (contrôleur
//      unique). Superlatifs (PlusFort/PlusFaible/...) déjà mono-cible. FAIT.
//   2. « CHAQUE AUTRE ADVERSAIRE » = Cible.AutresAdversaires (tous sauf DerniereCible ; ≠ TousAdversaires
//      qui l'inclut, ≠ AutreAdversaire qui en désigne UN). FAIT.
public static class Destinations
{
    public static List<CarteSort> Toutes() =>
    [
        new("Cubengelus", TypeComposant.Destination, Glyphe.Arcane, initiative: 12)
        {
            Exemplaires = 2,
            Id = "EP2-082",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Barbaryaga", TypeComposant.Destination, Glyphe.Arcane, initiative: 14)
        {
            Exemplaires = 2,
            Id = "EP2-084",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(2) },
                                new Action { Type = TypeAction.PerdreSang, Cible = Cible.MemeCible, Valeur = new ValeurFixe(1) },
                            ],
                        },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(3) },
                                new Action { Type = TypeAction.PerdreSang, Cible = Cible.MemeCible, Valeur = new ValeurFixe(2) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Écrabouillax", TypeComposant.Destination, Glyphe.Arcane, initiative: 1)
        {
            Exemplaires = 2,
            Id = "EP2-086",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeTresors, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeTresors, Valeur = new ValeurFixe(3) },
                                new Action { Type = TypeAction.DefausserTresor, Cible = Cible.MemeCible, Valeur = new ValeurFixe(1) },
                            ],
                        },
                        new TrancheJetDePuissance { Seuil = 10, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeTresors, Valeur = new ValeurFixe(7) }] },
                    ],
                },
            ],
        },

        new("Ouististyx", TypeComposant.Destination, Glyphe.Arcane, initiative: 17)
        {
            Exemplaires = 2,
            Id = "EP2-087",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurFixe(1) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Gracula", TypeComposant.Destination, Glyphe.Tenebres, initiative: 13)
        {
            Exemplaires = 2,
            Id = "EP2-090",
            // « Payez 1 🩸 : GARDEZ » = GARDEZ payé, INDÉPENDANT du jet → TypeAction.Garder (GarderCreatureEnCours
            // hors tranche). La Créature en cours est posée par ResoudreComposant pendant la résolution. FAIT.
            Effets =
            [
                new EffetOptionnelPayant
                {
                    Cout = 1, Libelle = "GARDEZ",
                    SiPaye = [new Action { Type = TypeAction.Garder, Cible = Cible.Soi }],
                },
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Tenebres,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 10, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(4) }] },
                    ],
                },
            ],
        },

        new("Canibalocircus", TypeComposant.Destination, Glyphe.Tenebres, initiative: 16)
        {
            Exemplaires = 2,
            Id = "EP2-091",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Tenebres,
                    Tranches =
                    [
                        // 1-4 : Vous subissez 1 dégât.
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.TuerCreature, Cible = Cible.PlusDeCreatures },   // « une de ses Créatures » (montant défaut = 1)
                                new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurFixe(3) },
                            ],
                        },
                        new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeCreatures, Valeur = new ValeurFixe(4) }] },
                    ],
                },
            ],
        },

        new("Shub-Niggurath", TypeComposant.Destination, Glyphe.Tenebres, initiative: 6)
        {
            Exemplaires = 2,
            Id = "EP2-093",
            // Cible « Adversaire qui a déjà joué » = filtrante + CibleUnique. FAIT.
            // « Payez 1 🩸 : Ajoutez 1 dé à vos Jets pour une Créature ce tour » = AjouterBonusDe (BonusDesJetCreature).
            // Placé AVANT le Jet pour que le dé bonus s'applique aussi au Jet de Shub lui-même. FAIT.
            Effets =
            [
                new EffetOptionnelPayant
                {
                    Cout = 1, Libelle = "Ajoutez 1 dé à vos Jets pour une Créature ce tour",
                    SiPaye = [new Action { Type = TypeAction.AjouterBonusDe, Cible = Cible.Soi, Valeur = new ValeurFixe(1) }],
                },
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Tenebres,
                    Tranches =
                    [
                        // 1-4 : aucun effet (pas de tranche Seuil = 1).
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(2) },
                                // « jouez une autre Créature de votre main » : ajout au sort (résolu par ResoudreSort).
                                new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.EstCreature },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Mecha-Satana", TypeComposant.Destination, Glyphe.Tenebres, initiative: 3)
        {
            Exemplaires = 2,
            Id = "EP2-096",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Tenebres,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFaible, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusFaible, Valeur = new ValeurFixe(2) },
                                new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(1) },
                            ],
                        },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusFaible, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(2) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Bruleculus", TypeComposant.Destination, Glyphe.Elementaire, initiative: 9)
        {
            Exemplaires = 2,
            Id = "EP2-097",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Elementaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(3) }] },
                        new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(4) }] },
                    ],
                },
            ],
        },

        new("Beeeh-zerker!", TypeComposant.Destination, Glyphe.Elementaire, initiative: 11)
        {
            Exemplaires = 2,
            Id = "EP2-099",
            // Cible « Adversaire sans Donjon » = SansDonjon + CibleUnique (peut matcher plusieurs → choix). FAIT.
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Elementaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansDonjon, CibleUnique = true, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansDonjon, CibleUnique = true, Valeur = new ValeurFixe(2) }] },
                        // GAP valeur : « 3 dégâts ou 6 si c'est votre dernier adversaire » (conditionnelle « dernier
                        //       adversaire » non exprimable ; le 3 de base est encodé).
                        new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansDonjon, CibleUnique = true, Valeur = new ValeurFixe(3) }] },
                    ],
                },
            ],
        },

        new("Tabapassif", TypeComposant.Destination, Glyphe.Elementaire, initiative: 2)
        {
            Exemplaires = 2,
            Id = "EP2-102",
            // Cible « Adversaire qui a déjà joué » = ADejaJoue + CibleUnique. FAIT.
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Elementaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(3) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(3) },
                                // « 3 dégâts à chaque adversaire qui a un jeton Dernier Survivant » = ensemble (correct).
                                new Action { Type = TypeAction.Degats, Cible = Cible.AJetonDernierSurvivant, Valeur = new ValeurFixe(3) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Golemacifas", TypeComposant.Destination, Glyphe.Elementaire, initiative: 7)
        {
            Exemplaires = 2,
            Id = "EP2-103",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Elementaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(2) }] },
                        // « 1 dé de dégâts » = ValeurDe(1).
                        new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurDe(1) }] },
                    ],
                },
            ],
        },

        new("Gaztoxicus", TypeComposant.Destination, Glyphe.Illusion, initiative: 20)
        {
            Exemplaires = 2,
            Id = "EP2-106",
            // Cible « Adversaire qui a déjà joué » = ADejaJoue + CibleUnique. FAIT.
            // GAP paiement : « Payez 2 🩸 : résoudre chaque dé de ce Jet individuellement contre des adversaires
            //       différents » = dés du Jet utilisés SEPAREMENT + ciblage par dé (cf. Castoramax / Coupéhendus).
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Illusion,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 10, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(4) }] },
                    ],
                },
            ],
        },

        new("Tentaculax", TypeComposant.Destination, Glyphe.Illusion, initiative: 16)
        {
            Exemplaires = 2,
            Id = "EP2-107",
            // Cible « Adversaire sans Créature » = SansCreature + CibleUnique. FAIT.
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Illusion,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansCreature, CibleUnique = true, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansCreature, CibleUnique = true, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.SansCreature, CibleUnique = true, Valeur = new ValeurFixe(3) },
                                new Action { Type = TypeAction.VolerTresor, Cible = Cible.MemeCible, Valeur = new ValeurFixe(1) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Logocrypto", TypeComposant.Destination, Glyphe.Illusion, initiative: 5)
        {
            Exemplaires = 2,
            Id = "EP2-110",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Illusion,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.VolerSang, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(1) },
                                new Action { Type = TypeAction.VolerSang, Cible = Cible.MemeCible, Valeur = new ValeurFixe(1) },
                            ],
                        },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.VolerSang, Cible = Cible.MemeCible, Valeur = new ValeurFixe(1) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Ignôminus", TypeComposant.Destination, Glyphe.Illusion, initiative: 19)
        {
            Exemplaires = 2,
            Id = "EP2-112",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Illusion,
                    Tranches =
                    [
                        // 1-4 : Chaque adversaire se soigne de 1 PV (ensemble = correct).
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Soin, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance { Seuil = 5, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFaible, Valeur = new ValeurFixe(3) }] },
                        // 5 dégâts (pose DerniereCible) puis chaque AUTRE adversaire (= tous sauf la cible) se soigne de 1 PV.
                        new TrancheJetDePuissance
                        {
                            Seuil = 10,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusFaible, Valeur = new ValeurFixe(5) },
                                new Action { Type = TypeAction.Soin, Cible = Cible.AutresAdversaires, Valeur = new ValeurFixe(1) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Coco-Cocoricus", TypeComposant.Destination, Glyphe.Primaire, initiative: 10)
        {
            Exemplaires = 2,
            Id = "EP2-113",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Primaire,
                    Tranches =
                    [
                        // 1-4 : GARDEZ seul (aucun dégât).
                        new TrancheJetDePuissance { Seuil = 1, PeutGarder = true, Actions = [] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 10, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(4) }] },
                    ],
                },
            ],
        },

        new("Tropintimus", TypeComposant.Destination, Glyphe.Primaire, initiative: 8)
        {
            Exemplaires = 2,
            Id = "EP2-116",
            // Cible « Adversaire qui a déjà joué » = ADejaJoue + CibleUnique. FAIT.
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Primaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(3) }] },
                        // 4 dégâts à la cible (pose DerniereCible) puis 1 dégât à chaque AUTRE adversaire.
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.Degats, Cible = Cible.AutresAdversaires, Valeur = new ValeurFixe(1) },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Sauvéwillix", TypeComposant.Destination, Glyphe.Primaire, initiative: 13)
        {
            Exemplaires = 2,
            Id = "EP2-118",
            // Cible « Adversaire qui a le Donjon » : ADonjon renvoie déjà au plus 1 (contrôleur unique) → mono-cible. FAIT.
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Primaire,
                    Tranches =
                    [
                        new TrancheJetDePuissance { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADonjon, Valeur = new ValeurFixe(1) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.ADonjon, Valeur = new ValeurFixe(2) },
                                new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                            ],
                        },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.ADonjon, Valeur = new ValeurFixe(5) },
                                new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                            ],
                        },
                    ],
                },
            ],
        },

        new("Crassdépargnus", TypeComposant.Destination, Glyphe.Primaire, initiative: 18)
        {
            Exemplaires = 2,
            Id = "EP2-119",
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Primaire,
                    Tranches =
                    [
                        // 1-4 : aucun effet (pas de tranche Seuil = 1).
                        new TrancheJetDePuissance { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(2) }] },
                        new TrancheJetDePuissance
                        {
                            Seuil = 10, PeutGarder = true,
                            Actions =
                            [
                                new Action { Type = TypeAction.Degats, Cible = Cible.PlusDeSang, Valeur = new ValeurFixe(4) },
                                new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurFixe(2) },
                            ],
                        },
                    ],
                },
            ],
        },
    ];
}
