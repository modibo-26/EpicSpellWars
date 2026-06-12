using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 20 Qualités (chacune en 2 exemplaires). Mêmes conventions que Sources :
//   // TODO  = clause d'un autre pilier (Donjon / Réaction / déclencheurs).
//   // GAP   = mécanique non encore exprimable par le résolveur (à concevoir).
public static class Qualites
{
    public static List<CarteSort> Toutes() =>
    [
        new("Trésormodix", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-042",
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi },
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.SansTresor },   // chaque adversaire sans Trésor en gagne 1 (ensemble)
                    ],
                },
                new EffetOptionnelPayant
                {
                    Cout = 8, Libelle = "Infligez 2 dégâts par Trésor en jeu à chaque adversaire",
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParTresorEnJeu(2) }],
                },
            ],
        },

        new("Gromago", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-044",
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi, Valeur = new ValeurFixe(2) },
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.NaPasJoue },
                    ],
                },
                new EffetOptionnelPayant
                {
                    Cout = 2, Libelle = "Chaque adversaire ayant déjà joué défausse 1 Trésor",
                    SiPaye = [new Action { Type = TypeAction.DefausserTresor, Cible = Cible.ADejaJoue }],
                },
            ],
        },

        new("Roulepélax", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-045",
            // TODO Donjon (même effet sur le voisin de droite).
            Effets =
            [
                new EffetProposition
                {
                    Cible = Cible.AdversaireGauche,
                    Proposition = "Donner 1 carte Arcane ? (sinon 1 Trésor)",
                    SiAccepte = [new Action { Type = TypeAction.PasserCartes, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.Glyphe == Glyphe.Arcane }],
                    SiRefuse = [new Action { Type = TypeAction.VolerTresor, Cible = Cible.MemeCible }],
                },
            ],
        },

        new("Trankilus", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Exemplaires = 2,
            Id = "EP2-048",
            // Lance 1 dé MÉMORISÉ (DernierDe) ; gain de 🩸 = ce dé. Réutilisation OK via ValeurDernierDe.
            // TODO étape 5 (conditionnelle) : « si ≤ 4, le plus faible se soigne de DernierDe PV » (même dé).
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.LancerDeMemorise, Cible = Cible.Soi },
                        new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurDernierDe(1) },
                    ],
                },
            ],
        },

        new("Asphixis", TypeComposant.Qualite, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-050",
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) }] },
                new EffetOptionnelPayant
                {
                    Cout = 1, Libelle = "Un adversaire subit votre Sang ; vous subissez le sien",
                    SiPaye =
                    [
                        // « Il subit autant de dégâts que VOTRE niveau de 🩸 » (le choix pose DerniereCible).
                        new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurParSang(1, SourceSorcier.Lanceur) },
                        // « et vous subissez autant de dégâts que SON niveau de 🩸 ».
                        new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurParSang(1, SourceSorcier.DerniereCible) },
                    ],
                },
            ],
        },

        new("Oeilcrevax", TypeComposant.Qualite, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-051",
            // TODO Réaction au blocage (gagnez 1 🩸 si une Créature encaisse) — pilier déclencheurs.
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.PlusFort, Valeur = new ValeurFixe(3) }] }],
        },

        new("Chancedecocus", TypeComposant.Qualite, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-053",
            // TODO Donjon : « chaque autre adversaire subit aussi autant de dégâts que son résultat ».
            Effets = [new EffetChancedecocus()],
        },

        new("Mortalriktus", TypeComposant.Qualite, Glyphe.Tenebres)
        {
            Exemplaires = 2,
            Id = "EP2-056",
            Effets =
            [
                new EffetSimple { Actions = [new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, TypeAuChoix = true }] },
                new EffetChoixPayant
                {
                    Cout = 4, Libelle = "Doublez les dégâts infligés",
                    Base = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurQuantiteChoisie(1) }],
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurQuantiteChoisie(2) }],
                },
            ],
        },

        new("Sabruledepartoux", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-057",
            // GAP : cibler le contrôleur du Donjon (peut être le lanceur) + conditionnelle « si personne » + voisins du contrôleur.
            Effets = [],
        },

        new("Poilcramus", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-059",
            // « le double du nombre de Créatures dans sa main » = ValeurParCreatureEnMain(2). Base = un adversaire, payé = chacun.
            Effets =
            [
                new EffetChoixPayant
                {
                    Cout = 3, Libelle = "L'effet s'applique à chaque adversaire au lieu d'un",
                    Base =
                    [
                        new Action { Type = TypeAction.RevelerMain, Cible = Cible.AdversaireAuChoix },   // choisit + révèle (pose DerniereCible)
                        new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurParCreatureEnMain(2) },
                    ],
                    SiPaye =
                    [
                        new Action { Type = TypeAction.RevelerMain, Cible = Cible.TousAdversaires },
                        new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParCreatureEnMain(2) },
                    ],
                },
            ],
        },

        new("Oulacécho", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-061",
            // GAP : Cible.DesigneParDe non implémentée (le tirage désigne la cible). + Donjon.
            Effets = [],
        },

        new("Fourchétix", TypeComposant.Qualite, Glyphe.Elementaire)
        {
            Exemplaires = 2,
            Id = "EP2-064",
            // « Donnez le Donjon à un adversaire qui ne l'avait pas, puis 3 dégâts à cet adversaire » :
            // DonnerDonjon sur SansDonjon + CibleUnique (le « prenez le Donjon » est absorbé — la cible finit avec).
            // GAP Payez 3 : « Tuez d'abord toutes ses Créatures » = TuerCreature « TOUTES » (montant non fixe).
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.DonnerDonjon, Cible = Cible.SansDonjon, CibleUnique = true },   // pose DerniereCible
                        new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurFixe(3) },
                    ],
                },
            ],
        },

        new("Groclonar", TypeComposant.Qualite, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-065",
            // GAP : choix d'option par le LANCEUR + Cible « PV pair / impair ».
            Effets = [],
        },

        new("Cadopourrix", TypeComposant.Qualite, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-068",
            // GAP Payez 4 : option payante enveloppant un IEffet (EffetRevelerPioche), pas une liste d'Actions.
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.Type == TypeComposant.Destination }] }],
        },

        new("Boulcheloux", TypeComposant.Qualite, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-070",
            // « Choisissez un adversaire sans Trésor : il en gagne 1 » (SansTresor + CibleUnique, pose DerniereCible)
            // « puis 2 dégâts à chaque adversaire qui a un Trésor » (ATresor, ensemble). NB : l'ordre « puis » fait
            // que la cible qui vient de recevoir un Trésor est désormais éligible aux dégâts (lecture séquentielle).
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.SansTresor, CibleUnique = true },
                        new Action { Type = TypeAction.Degats, Cible = Cible.ATresor, Valeur = new ValeurFixe(2) },
                    ],
                },
            ],
        },

        new("Coupéhendus", TypeComposant.Qualite, Glyphe.Illusion)
        {
            Exemplaires = 2,
            Id = "EP2-071",
            Effets = [new EffetCoupehendus()],
        },

        new("Peutidardus", TypeComposant.Qualite, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-073",
            // TODO Donjon (Nombre = 2).
            Effets = [new EffetRevelerPioche { Critere = c => c.EstCreature, Nombre = 1, Destination = DestinationRevele.Sort }],
        },

        new("Bégoniax", TypeComposant.Qualite, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-075",
            // GAP : conditionnelle « si aucun sorcier mort → gagnez 2 🩸 ».
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi },
                        new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParMort(1) },
                    ],
                },
            ],
        },

        new("Spiralex", TypeComposant.Qualite, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-076",
            Effets = [new EffetSpiralex { CoutDouble = 2 }],
        },

        new("Sarabandus", TypeComposant.Qualite, Glyphe.Primaire)
        {
            Exemplaires = 2,
            Id = "EP2-079",
            Effets =
            [
                new EffetSimple
                {
                    Actions =
                    [
                        new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, Valeur = new ValeurFixe(3), FiltreCarte = c => c.Glyphe != Glyphe.Primaire },
                        new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurQuantiteChoisie(1) },
                    ],
                },
            ],
        },
    ];
}
