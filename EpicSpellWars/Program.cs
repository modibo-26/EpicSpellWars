using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars;

// Demo « tranche verticale » : valide GameContext. Appliquer sur 5 cartes graines.
// Chaque carte est encodee à la main (Effets) ; le but est d'exercer le resolveur,
// pas de constituer le catalogue (qui viendra du loader JSON).
class Program
{
    static void Main()
    {
        var merlin = new Sorcier("Merlin") { PointsDeVie = 18 };   // lanceur
        var gandalf = new Sorcier("Gandalf");                       // gauche de Merlin
        var saroumane = new Sorcier("Saroumane") { JetonsDernierSurvivant = 1 }; // droite

        var prochainDe = 1;
        var prochainChoix = true;   // reponse oui/non du prochain ChoisirOption (stub demo)
        var prochainType = TypeComposant.Source;   // type renvoye par le prochain ChoisirTypeComposant
        var ctx = new GameContext
        {
            Lanceur = merlin,
            Sorciers = [merlin, gandalf, saroumane],   // ordre de table : gauche = suivant
            ChoisirCible = candidats => candidats.First(),
            LancerDe = () => prochainDe,
            // Stub demo : prend autant de cartes que permis (jusqu'au max), en tete de main.
            ChoisirCartes = (candidats, min, max) => candidats.Take(max).ToList(),
            ChoisirOption = (sorcier, proposition) => prochainChoix,
            ChoisirTypeComposant = (sorcier, types) => types.Contains(prochainType) ? prochainType : types.First(),
            PiocheTresor = [new Tresor("Trésor démo", [], TriggerType.Passif)],
        };

        Etat(ctx, "Depart");

        // 1) Taléboulas — Infligez 3 dégâts à votre adversaire de gauche.
        Jouer(ctx, "Taléboulas", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }],
        });

        // 2) Flaminus — Infligez 1 dégât à chaque adversaire.
        Jouer(ctx, "Flaminus", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) }],
        });

        // 3) Pèredodux — Soignez-vous de 1 PV par Créature en jeu. (2 Créatures Primaire → +2)
        merlin.Creatures.Add(new CarteSort("Familier 1", TypeComposant.Destination, Glyphe.Primaire));
        merlin.Creatures.Add(new CarteSort("Familier 2", TypeComposant.Destination, Glyphe.Primaire));
        Jouer(ctx, "Pèredodux", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurParCreature(1) }],
        });

        // 4) Cubengelus — Destination Arcane, Jet de puissance. Dé forcé à 10 → tranche 10+.
        //    10+ : 4 dégâts (gauche) + gagnez 1 Trésor + GARDEZ.
        var cubengelus = new CarteSort("Cubengelus", TypeComposant.Destination, Glyphe.Arcane, initiative: 12);
        ctx.SortEnCours = [cubengelus];          // 1 carte Arcane → 1 dé
        ctx.CreatureEnCours = cubengelus;
        prochainDe = 10;
        Jouer(ctx, "Cubengelus (Jet=10)", new EffetJetDePuissance
        {
            Glyphe = Glyphe.Arcane,
            Tranches =
            [
                new Tranche { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) }] },
                new Tranche { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }] },
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
        });
        ctx.SortEnCours = [];
        ctx.CreatureEnCours = null;
        Console.WriteLine($"   → Créatures de Merlin : {string.Join(", ", merlin.Creatures.Select(c => c.Nom))}");

        // 5) Necrophilus — Prenez le Donjon. Payez 1 🩸 : 2 dégâts par jeton Dernier Survivant en jeu, à chaque adversaire.
        Jouer(ctx, "Necrophilus (base)", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi }],
        });
        Jouer(ctx, "Necrophilus (payé : ValeurParJeton)", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParJeton(2) }],
        });

        // 6) Vishnakrax — Ajoutez à votre sort 1 carte de votre main (groupe A, GagnerCarte).
        //    Main de Merlin : 1 Créature + 1 Source. Filtre Créature → la Créature part dans SortEnCours.
        merlin.Main.Add(new CarteSort("Bestiole", TypeComposant.Destination, Glyphe.Tenebres));
        merlin.Main.Add(new CarteSort("Étincelle", TypeComposant.Source, Glyphe.Elementaire));
        Jouer(ctx, "Vishnakrax (ajoute 1 Créature au sort)", new EffetSimple
        {
            Actions = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.EstCreature }],
        });
        Console.WriteLine($"   → Main : {string.Join(", ", merlin.Main.Select(c => c.Nom))} | SortEnCours : {string.Join(", ", ctx.SortEnCours.Select(c => c.Nom))}");
        ctx.SortEnCours = [];

        // 7) Sarabandus — Défaussez jusqu'à 3 cartes non Primaires, soin = 1 PV / carte (groupe B).
        //    Main : 2 non-Primaires + 1 Primaire. Le stub en défausse 2 → +2 PV via ValeurQuantiteChoisie.
        merlin.Main.Clear();
        merlin.Main.Add(new CarteSort("Brume", TypeComposant.Qualite, Glyphe.Illusion));
        merlin.Main.Add(new CarteSort("Foudre", TypeComposant.Source, Glyphe.Elementaire));
        merlin.Main.Add(new CarteSort("Caillou", TypeComposant.Source, Glyphe.Primaire));
        merlin.PointsDeVie = 10;
        Jouer(ctx, "Sarabandus (défausse non-Primaires → soin)", new EffetSimple
        {
            Actions =
            [
                new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, Valeur = new ValeurFixe(3), FiltreCarte = c => c.Glyphe != Glyphe.Primaire },
                new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurQuantiteChoisie(1) },
            ],
        });
        Console.WriteLine($"   → Main restante : {string.Join(", ", merlin.Main.Select(c => c.Nom))} | défaussées : {ctx.DerniereQuantite}");

        // 8) Roulepélax — l'adversaire de gauche donne 1 carte Arcane, SINON il donne 1 Trésor (groupe C).
        //    Ici Gandalf ACCEPTE → sa carte Arcane part dans la main de Merlin (PasserCartes).
        merlin.Main.Clear();
        gandalf.Main.Add(new CarteSort("Rune", TypeComposant.Qualite, Glyphe.Arcane));
        prochainChoix = true;
        Jouer(ctx, "Roulepélax (Gandalf accepte → donne 1 Arcane)", new EffetProposition
        {
            Cible = Cible.AdversaireGauche,
            Proposition = "Donner 1 carte Arcane ?",
            SiAccepte = [new Action { Type = TypeAction.PasserCartes, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.Glyphe == Glyphe.Arcane }],
            SiRefuse = [new Action { Type = TypeAction.VolerTresor, Cible = Cible.MemeCible }],
        });
        Console.WriteLine($"   → Main Merlin : {string.Join(", ", merlin.Main.Select(c => c.Nom))} | Main Gandalf : {string.Join(", ", gandalf.Main.Select(c => c.Nom))}");

        // 9) Boucledorus — l'adversaire donne 1 Créature au sort, SINON 1 dé de dégâts (groupe C).
        //    Ici Gandalf REFUSE → il subit 1 dé de dégâts (dé forcé à 4).
        prochainChoix = false;
        prochainDe = 4;
        Jouer(ctx, "Boucledorus (Gandalf refuse → 1 dé de dégâts)", new EffetProposition
        {
            Cible = Cible.AdversaireGauche,
            Proposition = "Donner 1 Créature ?",
            SiAccepte = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.EstCreature }],
            SiRefuse = [new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurDe(1) }],
        });

        // 10) Mortalriktus — choisissez un adversaire et défaussez les cartes d'un type choisi de votre
        //     main ; il subit autant de dégâts que de cartes défaussées (TypeAuChoix + AdversaireAuChoix).
        //     Main : 2 Source + 1 Qualité ; type choisi = Source → 2 défaussées → 2 dégâts à Gandalf.
        merlin.Main.Clear();
        merlin.Main.Add(new CarteSort("Flamme", TypeComposant.Source, Glyphe.Elementaire));
        merlin.Main.Add(new CarteSort("Givre", TypeComposant.Source, Glyphe.Elementaire));
        merlin.Main.Add(new CarteSort("Halo", TypeComposant.Qualite, Glyphe.Arcane));
        prochainType = TypeComposant.Source;
        gandalf.PointsDeVie = 10;
        Jouer(ctx, "Mortalriktus (défausse Source → dégâts = nb défaussé)", new EffetSimple
        {
            Actions =
            [
                new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, TypeAuChoix = true },
                new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurQuantiteChoisie(1) },
            ],
        });
        Console.WriteLine($"   → Main Merlin : {string.Join(", ", merlin.Main.Select(c => c.Nom))} | défaussées : {ctx.DerniereQuantite}");

        // 11) Foulremix — chaque sorcier passe toutes ses cartes d'un type au voisin de gauche, puis subit
        //     autant de dégâts que de cartes reçues. Type = Source. Gauche = case suivante.
        //     Merlin[1 Source] → Gandalf ; Gandalf[2 Source] → Saroumane ; Saroumane[0] → Merlin.
        //     Dégâts attendus : Merlin 0, Gandalf 1, Saroumane 2.
        foreach (var s in ctx.Sorciers) { s.Main.Clear(); s.PointsDeVie = 20; }
        merlin.Main.Add(new CarteSort("M-Source", TypeComposant.Source, Glyphe.Primaire));
        gandalf.Main.Add(new CarteSort("G-Source 1", TypeComposant.Source, Glyphe.Primaire));
        gandalf.Main.Add(new CarteSort("G-Source 2", TypeComposant.Source, Glyphe.Primaire));
        saroumane.Main.Add(new CarteSort("S-Qualité", TypeComposant.Qualite, Glyphe.Illusion));
        prochainType = TypeComposant.Source;
        Jouer(ctx, "Foulremix (passe les Source à gauche → dégâts = reçues)", new EffetFoulremix());
        foreach (var s in ctx.Sorciers)
            Console.WriteLine($"   → {s.Nom,-10} main : {string.Join(", ", s.Main.Select(c => c.Nom))}");

        // 12) Résolution complète d'un sort multi-composants (ResoudreSort).
        //     Sort donné en désordre [Destination, Qualité, Source] → résolu Source→Qualité→Destination.
        //     La Qualité ajoute une Source de la main (GagnerCarte) qui se résout AVANT la Destination.
        //     Destination = Créature Arcane, Jet 10+ : 4 dégâts gauche + GARDEZ.
        //     Dégâts à Gandalf attendus : Source 2 + Source ajoutée 1 + Destination 4 = 7 (20→13).
        foreach (var s in ctx.Sorciers) { s.Main.Clear(); s.PointsDeVie = 20; }
        merlin.Creatures.Clear();
        var source0 = new CarteSort("Source-A", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) }] }],
        };
        var bonusSource = new CarteSort("Source-bonus", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(1) }] }],
        };
        merlin.Main.Add(bonusSource);   // sera tirée dans le sort par la Qualité
        var qualite = new CarteSort("Qualité-A", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.Type == TypeComposant.Source }] }],
        };
        var destination = new CarteSort("Créature-A", TypeComposant.Destination, Glyphe.Arcane, initiative: 8)
        {
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches = [new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) }] }],
                },
            ],
        };
        ctx.SortEnCours = [destination, qualite, source0];   // désordre volontaire
        prochainDe = 10;
        Console.WriteLine("\n== Sort multi-composants (ResoudreSort) ==");
        ctx.ResoudreSort();
        Etat(ctx, "  après");
        Console.WriteLine($"   → Main Merlin : {string.Join(", ", merlin.Main.Select(c => c.Nom))}");
        Console.WriteLine($"   → Sort résolu : {string.Join(", ", ctx.SortEnCours.Select(c => c.Nom))}");
        Console.WriteLine($"   → Créatures gardées : {string.Join(", ", merlin.Creatures.Select(c => c.Nom))}");
        ctx.SortEnCours = [];

        // 13) Peutidardus — révèle la pioche jusqu'à une Créature, l'ajoute au sort (EffetRevelerPioche).
        //     Pioche : [Source, Qualité, CRÉATURE, Source]. On révèle 3 cartes, garde la Créature dans
        //     le sort ; les 2 premières (non-Créatures) partent à la Défausse.
        ctx.SortEnCours = [];
        ctx.Defausse = [];
        ctx.PiochePrincipale =
        [
            new CarteSort("Pioche-Source", TypeComposant.Source, Glyphe.Elementaire),
            new CarteSort("Pioche-Qualité", TypeComposant.Qualite, Glyphe.Illusion),
            new CarteSort("Pioche-Créature", TypeComposant.Destination, Glyphe.Tenebres),
            new CarteSort("Pioche-reste", TypeComposant.Source, Glyphe.Primaire),
        ];
        Console.WriteLine("\n== Peutidardus (révèle jusqu'à une Créature → sort) ==");
        new EffetRevelerPioche { Critere = c => c.EstCreature, Nombre = 1 }.Execute(ctx);
        Console.WriteLine($"   → SortEnCours : {string.Join(", ", ctx.SortEnCours.Select(c => c.Nom))}");
        Console.WriteLine($"   → Défausse   : {string.Join(", ", ctx.Defausse.Select(c => c.Nom))}");
        Console.WriteLine($"   → Pioche     : {string.Join(", ", ctx.PiochePrincipale.Select(c => c.Nom))}");
    }

    static void Jouer(GameContext ctx, string nom, IEffet effet)
    {
        ctx.DerniereCible = null;     // nouveau sort → cible relative remise à zero
        ctx.DerniereQuantite = 0;     // nouveau sort → compte du dernier choix de main remis à zero
        Console.WriteLine($"\n== {nom} ==");
        effet.Execute(ctx);
        Etat(ctx, "  après");
    }

    static void Etat(GameContext ctx, string libelle)
    {
        var donjon = ctx.ControleurDonjon?.Nom ?? "personne";
        Console.WriteLine($"{libelle} | Donjon: {donjon}");
        foreach (var s in ctx.Sorciers)
            Console.WriteLine($"   {s.Nom,-10} PV={s.PointsDeVie,2}  Sang={s.Sang,2}  Trésors={s.Tresors.Count}  Jetons={s.JetonsDernierSurvivant}");
    }
}
