using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 20 Sources (chacune en 2 exemplaires). Texte = verbatim de la carte (= data/sources.json,
// reference de lecture) ; Effets = encodage executable. Conventions :
//   // TODO Réaction / Donjon : clause relevant d'un autre pilier (declencheurs), pas du resolveur.
//   // GAP : clause non encore exprimable par le resolveur (mecanique a concevoir).
public static class Sources
{
    public static List<CarteSort> Toutes() =>
    [
        new("Foulremix", TypeComposant.Source, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-002",
            Effets = [new EffetFoulremix { CoutDouble = 4 }],
        },

        new("Taléboulas", TypeComposant.Source, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-003",
            Effets =
            [
                new EffetChoixPayant
                    
                {
                    Cout = 2, Libelle = "Infligez 3 dégâts à chaque adversaire à la place",
                    Base = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }],
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(3) }],
                },
            ],
        },

        new("Tikérestox", TypeComposant.Source, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-006",
            Effets = 
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.VolerSang, Cible = Cible.ADejaJoue, Valeur = new ValeurFixe(1) },
                        new Action { Type = TypeAction.Degats, Cible = Cible.NaPasJoue, Valeur = new ValeurFixe(2) },
                    ],
                },
            ],
        },

        new("Dépipax", TypeComposant.Source, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-008",
            // Branchement par dé ; Réaction : si vous mourez avant résolution, gagnez 1 Trésor (immédiat).
            // GAP : « et jouez-le au début de la prochaine manche » = effet différé MancheSuivante (non couvert).
            Effets =
            [
                new EffetBranchement
                {
                    NbDes = 1,
                    Tranches =
                    [
                        new Tranche { Seuil = 1, Actions = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }] },
                        new Tranche { Seuil = 3, Actions = [new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi }] },
                        new Tranche
                        {
                            Seuil = 5,
                            Actions =
                            [
                                new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                                new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi },
                            ],
                        },
                    ],
                },
                new EffetReaction { Actions = [new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi }] },
            ],
        },

        new("Castoramax", TypeComposant.Source, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-010",
            Effets = [new EffetCastoramax()],
        },

        new("Necrophilus", TypeComposant.Source, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-011",
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }] },
                new EffetOptionnelPayant
                {
                    Cout = 1, Libelle = "Infligez 2 dégâts par jeton Dernier Survivant à chaque adversaire",
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParJeton(2) }],
                },
            ],
        },

        new("Momidisis", TypeComposant.Source, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-014",
            // GAP Réaction (tranche B faite) : « piochez 2 Sorcier crevé supplémentaires » dépend du système
            // de pioche de Sorciers crevés à la mort (tranche E).
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, Valeur = new ValeurFixe(2) }] }],
        },

        new("Nyarlaprizdetep", TypeComposant.Source, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-015",
            Effets =
            [
                new EffetProposition
                {
                    Cible = Cible.AdversaireGauche,
                    Proposition = "Donner 2 🩸 ? (sinon subir 3 dégâts)",
                    SiAccepte = [new Action { Type = TypeAction.VolerSang, Cible = Cible.MemeCible, Valeur = new ValeurFixe(2) }],
                    SiRefuse = [new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurFixe(3) }],
                },
            ],
        },

        new("Flaminus", TypeComposant.Source, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-017",
            Effets =
            [
                new EffetChoixPayant
                {
                    Cout = 5, Libelle = "Infligez 5 dégâts au lieu de 1",
                    Base = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) }],
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(5) }],
                },
            ],
        },

        new("Fukushimax", TypeComposant.Source, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-019",
            // « Infligez 2 dégâts à votre adversaire de droite. Réaction : si vous mourez avant que cette carte
            // ne soit résolue, infligez 1 dé de dégâts au sorcier qui vous a tué. » La Réaction utilise
            // Cible.Tueur + le cas CROSS-WIZARD (mourir pendant le tour d'un autre), cf. DeclencherReactions.
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(2) }] },
                new EffetReaction { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.Tueur, Valeur = new ValeurDe(1) }] },
            ],
        },

        new("Volcanino", TypeComposant.Source, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-021",
            // Base + bonus « Donjon : 1 dé de dégâts à un AUTRE adversaire » (≠ le plus fort déjà visé).
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        // « si vous en avez » : DonnerSang est borné au Sang dispo du lanceur. Pose DerniereCible.
                        new Action { Type = TypeAction.DonnerSang, Cible = Cible.PlusFort, Valeur = new ValeurFixe(1) },
                        new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurDe(1) },
                    ],
                },
                new EffetConditionnel
                {
                    Condition = ctx => ctx.LanceurControleDonjon,
                    SiVrai = [new Action { Type = TypeAction.Degats, Cible = Cible.AutreAdversaire, Valeur = new ValeurDe(1) }],
                },
            ],
        },

        new("Brikébix", TypeComposant.Source, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-024",
            // « Choisissez : (A) prenez le Donjon + 1 dégât à chaque adversaire, ou (B) 3 dégâts à un adversaire
            // sans Donjon. Payez 3 🩸 : faites les deux. » Le lanceur tranche A/B ; payé → A puis B (après A, le
            // lanceur contrôle le Donjon → SansDonjon de B = n'importe quel adversaire).
            Effets =
            [
                new EffetChoixLanceur
                {
                    CoutTout = 3, LibelleTout = "Faites les deux",
                    Options =
                    [
                        new OptionLanceur
                        {
                            Libelle = "Prenez le Donjon et 1 dégât à chaque adversaire",
                            Actions =
                            [
                                new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                                new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) },
                            ],
                        },
                        new OptionLanceur
                        {
                            Libelle = "3 dégâts à un adversaire sans Donjon",
                            Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.SansDonjon, CibleUnique = true, Valeur = new ValeurFixe(3) }],
                        },
                    ],
                },
            ],
        },

        new("Bébéfédex", TypeComposant.Source, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-026",
            // « 2 dégâts au plus fort. Payez 3 🩸 : ajoutez 1 carte AU HASARD de votre main au sort. »
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(2) }] },
                new EffetOptionnelPayant
                {
                    Cout = 3,
                    Libelle = "Ajoutez 1 carte au hasard de votre main au sort",
                    SiPaye = [new Action { Type = TypeAction.GagnerCarteAuHasard, Cible = Cible.Soi }],
                },
            ],
        },

        new("Vishnakrax", TypeComposant.Source, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-028",
            // « Ajoutez 1 carte de la main au sort ; Donjon : 1 carte supplémentaire ».
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1 }] },
                new EffetConditionnel
                {
                    Condition = ctx => ctx.LanceurControleDonjon,
                    SiVrai = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1 }],
                },
            ],
        },

        new("Brademinus", TypeComposant.Source, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-030",
            // « Infligez 1 dégât à n'importe quel adversaire. Réaction : si vous mourez avant que cette carte
            // ne soit résolue et qu'une Créature est présente dans votre sort, elle encaisse à votre place
            // (vous ne mourez pas). » L'absorption (CreatureEncaisse) restaure les PV d'avant-coup et consomme
            // la Créature du sort ; sans Créature, la Réaction n'empêche pas la mort.
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurFixe(1) }] },
                new EffetReaction { Actions = [new Action { Type = TypeAction.CreatureEncaisse, Cible = Cible.Soi }] },
            ],
        },

        new("Multitax", TypeComposant.Source, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-031",
            // « Prenez le Donjon. Si vous le prenez à un adversaire vivant, 3 dégâts à un AUTRE adversaire. »
            // La condition lit l'état d'AVANT (PrendreDonjon est dans la branche, joué après l'évaluation).
            // « un autre adversaire » : les dégâts (Cible.SansDonjon) sont infligés AVANT PrendreDonjon, tant que
            // l'ancien contrôleur détient encore le Donjon → SansDonjon l'exclut bien. CibleUnique = le lanceur choisit.
            Effets =
            [
                new EffetConditionnel
                {
                    Condition = ctx => ctx.ControleurDonjon is { EstVivant: true } c && c != ctx.Lanceur,
                    SiVrai =
                    [
                        new Action { Type = TypeAction.Degats, Cible = Cible.SansDonjon, CibleUnique = true, Valeur = new ValeurFixe(3) },
                        new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                    ],
                    SiFaux = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }],
                },
            ],
        },

        new("Pèredodux", TypeComposant.Source, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-033",
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurParCreature(1) }] },
                new EffetPaiementVariable
                {
                    Libelle = "Soignez-vous de X PV supplémentaires",
                    SiPaye = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurQuantiteChoisie(1) }],
                },
            ],
        },

        new("Gonzofungus", TypeComposant.Source, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-035",
            // Soin = 1 dé ; Réaction : si vous mourez avant résolution, PV = 1 (vous ne mourez pas).
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurDe(1) }] },
                new EffetReaction { Actions = [new Action { Type = TypeAction.PvAUn, Cible = Cible.Soi }] },
            ],
        },

        new("Hydraponix", TypeComposant.Source, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-037",
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(3) }] },
                new EffetOptionnelPayant
                {
                    Cout = 4, Libelle = "Infligez 7 dégâts à chaque adversaire avec un jeton",
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.AJetonDernierSurvivant, Valeur = new ValeurFixe(7) }],
                },
            ],
        },

        new("Boucledorus", TypeComposant.Source, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-039",
            Effets =
            [
                new EffetProposition
                {
                    Cible = Cible.AdversaireAuChoix,
                    Proposition = "Donner 1 Créature ? (sinon subir 1 dé de dégâts)",
                    SiAccepte = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.EstCreature }],
                    SiRefuse = [new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurDe(1) }],
                },
            ],
        },
    ];
}
