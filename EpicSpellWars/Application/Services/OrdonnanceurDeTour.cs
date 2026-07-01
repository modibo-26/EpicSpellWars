using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Services;

// Ordonnanceur de tour : ordonne les sorts du tour puis les resout un par un via
// GameContext.ResoudreSort, en nettoyant la fin de chaque sort.
//
// TRANCHE FINE volontaire : tri + boucle + nettoyage. Les recompenses (Sang, +3 au kill, vol du
// Donjon, jeton, bonus « Donjon : » de fin de tour) relevent du systeme de declencheurs (2e pilier)
// et ne sont PAS traitees ici — elles se brancheront par-dessus dans un commit dedie.
public class OrdonnanceurDeTour
{
    // Joue un tour : chaque sorcier ayant un sort le resout dans l'ordre reglementaire
    //   1. moins de Composants d'abord ;
    //   2. egalite → Initiative de la Destination la plus haute ;
    //   3. egalite → depart au de (plus haut resultat d'abord).
    // Renvoie l'ordre effectif de resolution (les sorciers qui ont joue).
    public IReadOnlyList<Sorcier> JouerTour(GameContext ctx, IReadOnlyDictionary<Sorcier, List<CarteSort>> sorts)
    {
        foreach (var s in ctx.Sorciers)
        {
            s.ADejaJoueCeTour = false;
            s.ADejaPayeCeTour = false;
            s.ADejaPayeTresorCeTour = false;
            s.ADejaRelanceJetCeTour = false;
        }

        // Memorise les sorts declares + reset de l'etat de resolution a portee tour (Reactions cross-wizard).
        ctx.PreparerTour(sorts);

        var aJouer = ctx.Sorciers
            .Where(s => s.EstVivant && sorts.TryGetValue(s, out var sort) && sort.Count > 0)
            .ToList();

        // De de depart tire UNE seule fois par sorcier (sinon OrderBy reevaluerait la cle).
        var deDepart = aJouer.ToDictionary(s => s, _ => ctx.LancerDe());

        // Vers Pas Solitaires : un porteur à égalité d'initiative (même nb de composants ET même Initiative
        // qu'un autre) peut payer 1 🩸 pour remporter l'égalité et jouer avant les autres (priorité de départage).
        var prioriteEgalite = new HashSet<Sorcier>();
        foreach (var s in aJouer.Where(s => s.Tresors.Any(t => t.RemporteEgaliteInitiativePayant)))
        {
            var aEgalite = aJouer.Any(o => o != s && sorts[o].Count == sorts[s].Count && InitiativeDe(sorts[o]) == InitiativeDe(sorts[s]));
            ctx.Lanceur = s;   // le porteur paie (TenterPayerTresor débite le Lanceur)
            if (aEgalite && ctx.TenterPayerTresor(1, "Vers Pas Solitaires : remporter l'égalité"))
                prioriteEgalite.Add(s);
        }

        var ordre = aJouer
            .OrderBy(s => sorts[s].Count)
            .ThenByDescending(s => InitiativeDe(sorts[s]))
            .ThenByDescending(s => prioriteEgalite.Contains(s))   // Vers Pas Solitaires gagne l'égalité
            .ThenByDescending(s => deDepart[s])
            .ToList();
        ctx.OrdreDuTour = ordre;   // pour les clauses « si premier/dernier à jouer »

        foreach (var lanceur in ordre)
        {
            if (!lanceur.EstVivant)
                continue;   // tue par un sort precedent ce tour → ne resout pas le sien

            ctx.Lanceur = lanceur;
            ctx.CreatureEnCours = null;
            ctx.BonusDesJetCreature = 0;   // bonus de dés (Shub payé) à portée du tour du lanceur

            // Créatures gardées des tours PRÉCÉDENTS (snapshot AVANT résolution) : elles réattaqueront après le
            // sort. Snapshot ici pour exclure une Créature gardée CE tour (elle vient d'attaquer comme Destination).
            var creaturesAReattaquer = lanceur.Creatures.ToList();

            // Phase DEBUT DE TOUR : clauses « début de votre tour » des Trésors du porteur (Braguette de
            // Cthulhu, Mains Poisseuses « si premier à jouer »...).
            ctx.DeclencherClausesPhase(PhaseTour.DebutTour, [lanceur]);

            // Phase STANDBY : le porteur peut activer UNE capacité « Payez X 🩸 » (paiement Trésor 1×/tour).
            // Le choix (quel Trésor, ou aucun) est délégué au hook.
            var activables = lanceur.Tresors.Where(t => t.Activation is not null).ToList();
            if (activables.Count > 0)
                ctx.ChoisirActivationTresor(lanceur, activables)?.Activation!.Execute(ctx);

            // Phase SORT : « révélation » du sort → on remplace MAINTENANT (à la résolution, dans l'ordre du
            // tour) les Magie féroce par de vraies cartes révélées de la pioche. Fait ici et non à la
            // déclaration pour que l'ordonnancement ci-dessus ait bien vu une Magie féroce (Initiative 0).
            ctx.ResoudreMagieFeroceDuSort(sorts[lanceur], lanceur);

            ctx.SortEnCours = [..sorts[lanceur]];   // copie : ResoudreSort/nettoyage ne touchent pas l'entree

            ctx.ResoudreSort();

            // Nettoyage fin de sort : composants non gardes → Defausse. Les Creatures gardees sont deja
            // dans Lanceur.Creatures (via GarderCreatureEnCours) ; on ne les defausse pas.
            foreach (var composant in ctx.SortEnCours.Where(composant => !lanceur.Creatures.Contains(composant)))
                ctx.Defausse.Add(composant);
            ctx.SortEnCours = [];

            // Réattaque forcée des Créatures gardées des tours précédents, APRÈS le sort déclaré (et hors
            // SortEnCours → leur Jet ne compte que les Glyphes des Créatures gardées). GARDEZ → reste ; sinon défaussée.
            ctx.ReattaquerCreaturesGardees(creaturesAReattaquer);

            lanceur.ADejaJoueCeTour = true;
        }

        // Phase FIN DE TOUR (une seule fois, tous les porteurs) : clauses « fin de tour » (Menottes d'Avarice
        // « si dernier à jouer », Smoking de Location « si le plus faible »...).
        ctx.DeclencherClausesPhase(PhaseTour.FinTour, ctx.Sorciers);

        // Fin de tour : le contrôleur du Donjon le conserve et gagne du Sang ([[donjon-controle]]). S'applique
        // même mort (le Sang persiste). Sorcier sous Terre : un contrôleur MORT gagne 4 au lieu de 1 ;
        // Chalisman : +1 supplémentaire (BonusSangDonjonFinTour).
        if (ctx.ControleurDonjon is { } gardien)
        {
            var gain = 1;
            if (!gardien.EstVivant)
            {
                var gainSiMort = gardien.SorciersCreves.Max(c => (int?)c.GainDonjonMortFinTour) ?? 0;
                if (gainSiMort > 0)
                    gain = gainSiMort;
            }
            gain += gardien.Tresors.Sum(t => t.BonusSangDonjonFinTour);
            gardien.Sang = Math.Min(gardien.SangMax, gardien.Sang + gain);
        }

        ctx.Tour++;
        return ordre;
    }

    // Joue une MANCHE complète : une suite de tours jusqu'à ce qu'il ne reste qu'un seul survivant.
    // Séquence : DebutManche (Donjon au centre, réveil des morts, effets différés) → tours (chaque tour :
    // les vivants complètent leur main à 8 et les morts piochent un Sorcier crevé ; chacun déclare son sort ;
    // résolution via JouerTour) → FinManche (défausse mains + Trésors + Créatures ; les crevés sont gardés).
    // Le jeton Dernier Survivant est décerné par OnMort pendant le tour fatal. Renvoie le vainqueur (ou null).
    public Sorcier? JouerManche(GameContext ctx, int tailleMain = 8)
    {
        DebutManche(ctx);

        // Une manche dure « autant de tours que nécessaire pour qu'un joueur l'emporte ». Garde-fou contre une
        // boucle infinie (pioche/mains épuisées sans mort possible) : plafond de tours au-delà duquel on sort.
        const int plafondTours = 100;
        var tours = 0;
        while (ctx.Sorciers.Count(s => s.EstVivant) > 1 && tours++ < plafondTours)
        {
            // Début de tour : les vivants complètent leur main à 8 ; les morts piochent un nouveau crevé.
            foreach (var s in ctx.Sorciers)
                if (s.EstVivant)
                    ctx.CompleterMain(s, tailleMain);
                else
                    ctx.PiocherSorcierCreve(s);

            // Déclaration : chaque vivant choisit ses composants EN MAIN (retirés de la main → ils forment le sort).
            var sorts = new Dictionary<Sorcier, List<CarteSort>>();
            foreach (var s in ctx.Sorciers.Where(s => s.EstVivant))
            {
                var composants = ctx.DeclarerSort(s).ToList();
                foreach (var c in composants)
                    s.Main.Remove(c);

                // Baisse de Tension : augmente le PREMIER sort (non vide) de la manche avec la 1re carte de la pioche.
                if (s.AugmenterPremierSort && composants.Count > 0)
                {
                    ctx.AugmenterSortDepuisPioche(composants, s);
                    s.AugmenterPremierSort = false;
                }

                // NB : les Magie féroce NE sont PAS résolues ici. Le rulebook impose qu'une Magie féroce en
                // emplacement Destination vaille Initiative 0 pour l'ordre du tour, et qu'elle ne soit remplacée
                // par une vraie carte qu'« au moment où le sort est révélé » (= à sa résolution). Le remplacement
                // est donc fait dans JouerTour, après l'ordonnancement et dans l'ordre du tour.
                sorts[s] = composants;
            }

            JouerTour(ctx, sorts);
        }

        FinManche(ctx);

        var vivants = ctx.Sorciers.Where(s => s.EstVivant).ToList();
        // Mémorise le vainqueur pour les effets différés de la manche suivante (Doigt Magique). null = cas
        // improbable sans survivant (un suicide qui tue le dernier adversaire ; le jeton est tout de même décerné).
        ctx.VainqueurDerniereManche = vivants.Count == 1 ? vivants[0] : null;
        return ctx.VainqueurDerniereManche;
    }

    // Joue une PARTIE complète : enchaîne les manches jusqu'à ce qu'un sorcier atteigne le seuil de victoire
    // (2 jetons Dernier Survivant par défaut, [[constantes-de-jeu]]). Le Sang et les jetons persistent d'une
    // manche à l'autre (seuls les PV sont remis à zéro par DebutManche). Renvoie le champion (ou null).
    public Sorcier? JouerPartie(GameContext ctx, int jetonsPourGagner = 2, int tailleMain = 8)
    {
        ctx.MelangerPioche();   // setup de partie : melange initial de la pioche (une seule fois)

        // Garde-fou anti-boucle infinie (parties sans fin si personne ne peut tuer) : plafond de manches.
        const int plafondManches = 100;
        var manches = 0;
        while (!ctx.Sorciers.Any(s => s.JetonsDernierSurvivant >= jetonsPourGagner) && manches++ < plafondManches)
            JouerManche(ctx, tailleMain);

        return ctx.Sorciers.FirstOrDefault(s => s.JetonsDernierSurvivant >= jetonsPourGagner);
    }

    // Fin de manche ([[creatures-gardees]]) : chaque sorcier défausse sa main, ses Trésors et ses Créatures.
    // Il GARDE en revanche ses cartes Sorcier crevé (et son Sang, ses jetons). Trésors remis sous leur pile.
    private static void FinManche(GameContext ctx)
    {
        foreach (var s in ctx.Sorciers)
        {
            ctx.Defausse.AddRange(s.Main);
            ctx.Defausse.AddRange(s.Creatures);
            ctx.Defausse.AddRange(s.SousBuffet);
            ctx.PiocheTresor.AddRange(s.Tresors);
            s.Main.Clear();
            s.Creatures.Clear();
            s.SousBuffet.Clear();
            s.Tresors.Clear();
        }
    }

    // Début de manche : le Donjon est remis au centre (personne ne le contrôle), le compteur avance, les
    // sorciers REVIENNENT à la vie (PV de départ ; Sang/jetons/crevés persistent) et les effets différés
    // « au début de la prochaine manche » se déclenchent ([[donjon-controle]], tranche E).
    public void DebutManche(GameContext ctx)
    {
        ctx.ControleurDonjon = null;
        ctx.Manche++;
        ctx.PrimesEnJeu = 0;                         // les primes (Avis de Recherche) ne survivent pas à la manche
        ctx.Prediction = null;                       // la prédiction (Pièces du Destin) ne survit pas à la manche
        ctx.ReinitialiserJetonDernierSurvivant();   // nouvelle bataille → un nouveau jeton Dernier Survivant en jeu

        // Réveil : chaque sorcier repart à PV de départ (les morts reviennent pour la nouvelle manche) et son
        // statut « premier tué » est remis à zéro.
        foreach (var s in ctx.Sorciers)
        {
            s.PointsDeVie = Sorcier.PvDepart;
            s.EstPremierMortCetteManche = false;
        }

        // Effets différés (Sorciers crevés MancheSuivante, etc.) — joués sur des sorciers désormais vivants.
        ctx.DeclencherEffetsDifferes();
    }
    
    // Initiative du sort = celle de sa Destination ; 0 sans Destination. Une Magie féroce a Type == null :
    // elle n'est jamais reconnue comme Destination, donc une Magie féroce en emplacement Destination (aucune
    // vraie Destination dans le sort) tombe sur le ?? 0 → Initiative 0, conforme au rulebook (pages 6 et 16).
    // C'est pour cela que l'ordonnancement précède le remplacement des Magie féroce (cf. JouerTour / JouerManche).
    private static int InitiativeDe(List<CarteSort> sort) =>
        sort.FirstOrDefault(c => c.Type == TypeComposant.Destination)?.Initiative ?? 0;
}
