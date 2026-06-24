using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class GameContext
{
    public List<Sorcier> Sorciers { get; set; } = [];
    public required Sorcier Lanceur { get; set; }
    public Sorcier? ControleurDonjon { get; set; }

    // Le lanceur controle-t-il le Donjon ? Condition des bonus « Donjon : <effet> » (appliques seulement
    // si on controle le Donjon au moment de jouer la carte). Ex. EffetConditionnel(ctx => ctx.LanceurControleDonjon).
    public bool LanceurControleDonjon => ControleurDonjon == Lanceur;

    // Ordre de jeu effectif du tour (sorciers ayant joué, dans l'ordre), renseigné par OrdonnanceurDeTour.
    // Sert aux clauses « si vous êtes premier/dernier à jouer ».
    public IReadOnlyList<Sorcier> OrdreDuTour { get; set; } = [];

    // Le lanceur est-il le premier / le dernier à jouer ce tour ? (clauses Mains Poisseuses / Menottes d'Avarice.)
    public bool LanceurEstPremierAJouer => OrdreDuTour.Count > 0 && OrdreDuTour[0] == Lanceur;
    public bool LanceurEstDernierAJouer => OrdreDuTour.Count > 0 && OrdreDuTour[^1] == Lanceur;

    // Le lanceur est-il (à égalité) le sorcier vivant le plus faible en PV ? (clause Smoking de Location.)
    public bool LanceurEstLePlusFaible =>
        Lanceur.EstVivant && Sorciers.Where(s => s.EstVivant).Min(s => s.PointsDeVie) == Lanceur.PointsDeVie;

    public List<Carte> PiochePrincipale { get; set; } = [];
    public List<Carte> Defausse { get; set; } = [];
    public List<Tresor> PiocheTresor { get; set; } = [];
    public List<SorcierCreve> PiocheSorcierCreve { get; set; } = [];

    public int Manche { get; set; }
    public int Tour { get; set; }

    // Vainqueur (dernier survivant) de la manche qui vient de s'achever. Renseigne par OrdonnanceurDeTour en
    // fin de JouerManche ; lu par les effets differes qui ciblent « le vainqueur de cette manche » (Doigt Magique).
    public Sorcier? VainqueurDerniereManche { get; set; }

    // Primes « Avis de Recherche » placees au milieu : le prochain tueur en consomme une et gagne 3 🩸 (OnMort).
    // Reinitialise au debut de chaque manche (les Tresors sont defausses en fin de manche).
    public int PrimesEnJeu { get; set; }

    // Sort en cours de resolution (composants du lanceur ce tour) et creature en cours.
    public List<CarteSort> SortEnCours { get; set; } = [];
    public CarteSort? CreatureEnCours { get; set; }

    // Derniere cible adversaire affectee ce sort (pour Cible.AutreAdversaire / Cible.MemeCible).
    // À reinitialiser au debut de chaque sort.
    public Sorcier? DerniereCible { get; set; }

    // Nb de cartes du dernier choix de main ce sort (alimente ValeurQuantiteChoisie).
    // À reinitialiser au debut de chaque sort (comme DerniereCible).
    public int DerniereQuantite { get; set; }

    // Resultat du dernier de memorise ce sort (LancerDeMemorise → ValeurDernierDe : un meme tirage
    // alimente plusieurs effets, ex. Trankilus). À reinitialiser au debut de chaque sort.
    public int DernierDe { get; set; }

    // Des ajoutes aux Jets de puissance (pour une Creature) du lanceur CE TOUR (Shub-Niggurath paye).
    // Portee = tour du lanceur : remis a 0 par l'ordonnanceur au debut de chaque resolution de sort.
    public int BonusDesJetCreature { get; set; }

    // Hooks de decision (injectes par la couche qui pilote la partie)
    public required Func<IEnumerable<Sorcier>, Sorcier> ChoisirCible { get; set; }
    public required Func<int> LancerDe { get; set; }

    // Castoramax : « utilisez-EN UN » — le lanceur choisit, parmi les des lances, lequel sert d'attaque.
    public required Func<IReadOnlyList<int>, int> ChoisirDe { get; set; }

    // Choix de cartes dans une main : recoit les candidats DEJA filtres + les bornes [min...max],
    // renvoie la selection (longueur dans [min...max]). Injecte par la couche qui pilote la partie.
    public required Func<IReadOnlyList<CarteSort>, int, int, IReadOnlyList<CarteSort>> ChoisirCartes { get; set; }

    // Decision oui/non prise par UN sorcier face a une proposition (groupe C : don ou repli).
    // « Est-ce que ce sorcier accepte cette proposition ? » Ex. Roulepélax, Boucledorus, Nyarlaprizdetep.
    public required Func<Sorcier, string, bool> ChoisirOption { get; set; }

    // Choix d'une option par le LANCEUR parmi N libelles (« choisissez une option : ... ou ... »).
    // Renvoie l'index de l'option retenue. Distinct de ChoisirOption (decision oui/non d'une CIBLE).
    // Ex. Brikébix, Groclonar.
    public required Func<Sorcier, IReadOnlyList<string>, int> ChoisirOptionLanceur { get; set; }

    // Choix d'un type de composant (Source/Qualite/Destination) par UN sorcier, parmi une liste.
    // Ex. Mortalriktus (puis defausse de ce type), Foulremix (puis passage de ce type).
    public required Func<Sorcier, IReadOnlyList<TypeComposant>, TypeComposant> ChoisirTypeComposant { get; set; }

    // Injection du hasard : etant donne un nombre de candidats, renvoie un index dans [0, nb). Sert aux
    // effets « au hasard » (Bébéfédex : ajoute 1 carte au hasard de la main). Deterministe en test.
    public required Func<int, int> ChoisirIndexAuHasard { get; set; }

    // Au tour d'Initiative d'un porteur, quelle capacite de Tresor activer (ou null = aucune) parmi ses
    // Tresors activables. Une seule activation PAYANTE par tour ([[tresors-effets-speciaux]]).
    public required Func<Sorcier, IReadOnlyList<Tresor>, Tresor?> ChoisirActivationTresor { get; set; }

    // Declaration du sort d'un sorcier : renvoie les composants qu'il joue ce tour DEPUIS sa main
    // (l'ordonnanceur les retire de la main et les pose en SortEnCours). La SELECTION releve de la couche
    // qui pilote la partie (Console/ASP.NET) ; l'integration Magie feroce dans la declaration viendra ici.
    public required Func<Sorcier, IReadOnlyList<CarteSort>> DeclarerSort { get; set; }

    // Le lanceur decide-t-il de payer ce cout en Sang ? (« Payez N 🩸 : … »). libelle = effet propose.
    public required Func<Sorcier, int, string, bool> ChoisirPayer { get; set; }
    // Montant choisi pour un cout variable « Payez X 🩸 » (sera borne au Sang dispo).
    public required Func<Sorcier, int> ChoisirMontant { get; set; }

    // Nb de cartes du sort en cours (+ creatures gardees) portant ce Glyphe.
    public int CompterGlyphes(Glyphe glyphe) =>
        SortEnCours.Count(c => c.Glyphe == glyphe)
        + Lanceur.Creatures.Count(c => c.Glyphe == glyphe);

    // Des ajoutes a CE Jet de puissance par les modificateurs actifs : BonusDesJetCreature (Shub-Niggurath
    // paye, portee tour) + le bonus garde du Lanceur (Petit Ange, Passif). Tous les Jets de puissance sont
    // « pour une Creature » → le bonus garde est CONSOMME ici (« jusqu'a votre prochain Jet pour une Creature »).
    public int BonusDesJet(EffetJetDePuissance jet)
    {
        var bonus = BonusDesJetCreature + Lanceur.BonusProchainJetCreature;
        Lanceur.BonusProchainJetCreature = 0;
        return bonus;
    }

    // GOULOT UNIQUE de tous les degats du jeu : inflige `montant` a `cible` (borne a 0) et detecte la
    // transition vivant→mort. Toutes les sources de degats passent ici (Actions Degats/AutoDegats + effets
    // sur-mesure Spiralex/Foulremix/Chancedecocus), pour que les declencheurs (recompenses au kill, et plus
    // tard Reactions/Tresors/Sorciers creves) se branchent en UN seul endroit (OnMort).
    public void InfligerDegats(Sorcier cible, int montant)
    {
        var avant = cible.PointsDeVie;
        cible.PointsDeVie = Math.Max(0, cible.PointsDeVie - montant);
        if (avant > 0 && cible.PointsDeVie == 0)
            OnMort(cible, avant);   // avant = PV d'avant-coup, pour qu'une Réaction d'absorption restaure pleinement
    }

    // Declencheurs a la mort d'un sorcier (pilier 2). Le TUEUR = Lanceur courant (celui dont l'effet a
    // porte le coup fatal). Suicide (victime == Lanceur) → aucune recompense (regles-sang). Branche dans
    // l'ordre : B Reactions (peuvent empecher la mort), A recompenses au kill (+ passif Tresor), E crevé.
    private void OnMort(Sorcier victime, int pvAvant)
    {
        // Le TUEUR = Lanceur courant (celui dont l'effet a porte le coup fatal). Capture AVANT les Reactions :
        // une Reaction agit du point de vue de la victime (Lanceur temporairement = victime), donc on fige le
        // tueur ici, pour Cible.Tueur (Fukushimax) comme pour les recompenses au kill.
        var tueur = Lanceur;

        // Tranche B : Reactions. Sort de la victime = SortsDeclares[victime] ; pour le lanceur qui se resout,
        // c'est SortEnCours (memes instances). Seules tirent les Reactions des composants NON resolus
        // (_composantsResolus, portee TOUR) — ce qui couvre les 3 situations SANS cas special :
        //   victime pas encore jouee → aucun resolu → toutes ses Reactions ; victime == lanceur en cours →
        //   seuls les composants pas encore passes ; victime a deja joue ce tour → tous resolus → aucune.
        // Une Reaction peut EMPECHER la mort (Gonzofungus PV→1) → on re-teste EstVivant ensuite.
        var sortVictime = victime == Lanceur ? SortEnCours : SortsDeclares.GetValueOrDefault(victime) ?? [];
        DeclencherReactions(sortVictime, victime, tueur, pvAvant);

        if (victime.EstVivant)
            return;   // une Reaction a empeche la mort → pas de recompense au kill

        // Premier tue de la manche ? (la victime est le seul mort a cet instant). Fige le statut pour la
        // conditionnelle de Tournee d'Adieu, valable meme si le crevé est pioche plus tard dans la manche.
        victime.EstPremierMortCetteManche = Sorciers.Count(s => !s.EstVivant) == 1;

        if (tueur != victime)
        {
            // +3 Sang au kill, + bonus passif des Tresors du tueur (Liste du Père Fouettard, tranche D).
            var bonus = tueur.Tresors.Sum(t => t.BonusSangParKill);
            tueur.Sang = Math.Min(tueur.SangMax, tueur.Sang + 3 + bonus);
            if (ControleurDonjon == victime)
                ControleurDonjon = tueur;                            // vol du Donjon au kill

            // Avis de Recherche : une prime au milieu → le 1er tueur gagne 3 🩸 ; la prime est consommée.
            if (PrimesEnJeu > 0)
            {
                tueur.Sang = Math.Min(tueur.SangMax, tueur.Sang + 3);
                PrimesEnJeu--;
            }
        }

        // Jeton Dernier Survivant : s'il ne reste qu'un sorcier vivant, il l'emporte (victoire a 2 jetons).
        // UNE seule fois par manche (la bataille s'arrete des qu'il reste un survivant) : sinon une cascade de
        // morts en chaine (riposte mortelle d'une Reaction → OnMort imbrique) re-testerait « 1 vivant » a
        // chaque niveau et decernerait plusieurs jetons. Reset par DebutManche.
        var vivants = Sorciers.Where(s => s.EstVivant).ToList();
        if (vivants.Count == 1 && !_jetonDernierSurvivantDecerne)
        {
            vivants[0].JetonsDernierSurvivant++;
            _jetonDernierSurvivantDecerne = true;
        }

        // Coupe du Tocard : si la mort de la victime met fin à la manche (≤ 1 survivant), elle gagne 3 🩸 (passif).
        if (vivants.Count <= 1)
        {
            var bonusFin = victime.Tresors.Sum(t => t.BonusSangMortFinManche);
            if (bonusFin > 0)
                victime.Sang = Math.Min(victime.SangMax, victime.Sang + bonusFin);
        }

        // Tranche E : la victime pioche un Sorcier creve (consolation du mort).
        PiocherSorcierCreve(victime);
    }

    // Pioche le Sorcier creve du sommet de la pile pour `victime`. Appele a la mort (OnMort) ET au debut de
    // chaque nouveau tour pour un sorcier deja mort (rulebook : « au debut de chaque nouveau tour de jeu, ce
    // dernier pioche une nouvelle carte Sorcier creve »). Immediat → effet tout de suite (du point de vue de
    // la victime ; morte, mais Sang/Donjon persistent) ; MancheSuivante → differe ; Passif (Petit Ange) →
    // simplement conserve (son modificateur s'appliquera a son moment propre, GAP BonusDesJet).
    public void PiocherSorcierCreve(Sorcier victime)
    {
        if (PiocheSorcierCreve.Count == 0)
            return;

        var creve = PiocheSorcierCreve[0];
        PiocheSorcierCreve.RemoveAt(0);
        victime.SorciersCreves.Add(creve);

        switch (creve.TriggerType)
        {
            case TriggerType.Immediat:
                DeclencherEffets(creve.Effets, victime);
                break;
            case TriggerType.MancheSuivante:
                EffetsDifferes.Add((creve.Effets, victime));
                break;
            case TriggerType.Passif:
                // Petit Ange : garde des dés pour le prochain Jet de Créature du porteur (appliqués maintenant).
                victime.BonusProchainJetCreature += creve.BonusProchainJetCreature;
                break;
        }

        // Bouclier Anti-Fiente : quand un sorcier pioche un crevé, chaque AUTRE porteur lance un dé et se
        // soigne de 1 PV sur 5-6.
        foreach (var porteur in Sorciers)
            if (porteur != victime && porteur.Tresors.Any(t => t.SoigneSurPiocheCreveAdverse) && LancerDe() >= 5)
                porteur.PointsDeVie = Math.Min(porteur.PointsDeVieMax, porteur.PointsDeVie + 1);
    }

    // Declenche les Reactions des composants NON resolus du sort de la victime. La Reaction agit du POINT DE
    // VUE de la victime (Lanceur temporaire) : ainsi une riposte mortelle (Fukushimax) est attribuee a la
    // victime, et le tueur est expose via _tueur (Cible.Tueur). Save/restore Lanceur/_tueur/DerniereCible
    // pour ne pas polluer le sort en cours (cross-wizard : on est au milieu du sort du tueur). Le composant
    // en cours de resolution est deja dans _composantsResolus → sa propre Reaction ne se declenche pas
    // (timing fin SELF : « si votre Destination vous tue, sa Reaction et celles deja resolues ne s'appliquent
    // pas »). Une Reaction ne se declenche qu'UNE fois (_reactionsDeclenchees).
    private void DeclencherReactions(IReadOnlyList<CarteSort> sortVictime, Sorcier victime, Sorcier tueur, int pvAvant)
    {
        var ancienLanceur = Lanceur;
        var ancienTueur = _tueur;
        var ancienneCible = DerniereCible;
        var ancienSortReaction = _sortEnReaction;
        var ancienPvAvantMort = _pvAvantMort;
        Lanceur = victime;
        _tueur = tueur;
        DerniereCible = null;
        _sortEnReaction = sortVictime;   // sort de la victime, pour qu'une Réaction y trouve sa Créature (Brademinus)
        _pvAvantMort = pvAvant;          // PV d'avant-coup, pour une absorption qui annule pleinement le coup

        foreach (var composant in sortVictime.Where(c => !_composantsResolus.Contains(c)).ToList())
        {
            var aReagi = false;
            foreach (var reaction in composant.Effets.OfType<EffetReaction>())
                if (_reactionsDeclenchees.Add(reaction))
                {
                    reaction.Execute(this);
                    aReagi = true;
                }

            // La carte qui a reagi est consommee (« defaussez-la ») : si une Reaction empeche la mort
            // (Gonzofungus), la resolution reprend mais ce composant ne resout PAS son effet principal.
            if (aReagi)
                _composantsResolus.Add(composant);
        }

        Lanceur = ancienLanceur;
        _tueur = ancienTueur;
        DerniereCible = ancienneCible;
        _sortEnReaction = ancienSortReaction;
        _pvAvantMort = ancienPvAvantMort;
    }

    // Tueur courant, valide uniquement pendant la resolution d'une Reaction (cf. DeclencherReactions) ;
    // lu par Cible.Tueur. Null hors contexte de Reaction.
    private Sorcier? _tueur;

    // Sort de la victime + PV d'avant-coup, valides uniquement pendant une Reaction (save/restore dans
    // DeclencherReactions). Lus par TypeAction.CreatureEncaisse (Brademinus) : une Créature présente dans
    // le sort encaisse le coup fatal → on restaure _pvAvantMort (« vous ne mourez pas »).
    private IReadOnlyList<CarteSort> _sortEnReaction = [];
    private int _pvAvantMort;

    // Jeton Dernier Survivant deja decerne dans la manche en cours (une bataille = un seul jeton). Reset au
    // debut de manche par ReinitialiserJetonDernierSurvivant ; garde-fou anti double-comptage en cascade (OnMort).
    private bool _jetonDernierSurvivantDecerne;

    // Rearme le jeton Dernier Survivant au debut d'une nouvelle manche (appele par OrdonnanceurDeTour.DebutManche).
    public void ReinitialiserJetonDernierSurvivant() => _jetonDernierSurvivantDecerne = false;

    // Effets « au debut de la prochaine manche » (differes) : Sorciers creves MancheSuivante, Repos Merite/
    // Dépipax (TODO). Empiles avec leur proprietaire, vides par DeclencherEffetsDifferes (appele a DebutManche).
    public List<(List<IEffet> Effets, Sorcier Proprietaire)> EffetsDifferes { get; } = [];

    // Execute une liste d'effets du POINT DE VUE d'un proprietaire (Lanceur temporaire). Sauvegarde/restaure
    // Lanceur+DerniereCible pour ne pas polluer un sort en cours (Tresor Immediat / crevé piochés en cours).
    private void DeclencherEffets(List<IEffet> effets, Sorcier proprietaire)
    {
        var ancienLanceur = Lanceur;
        var ancienneCible = DerniereCible;
        Lanceur = proprietaire;
        DerniereCible = null;
        foreach (var effet in effets)
            effet.Execute(this);
        Lanceur = ancienLanceur;
        DerniereCible = ancienneCible;
    }

    // Declenche les effets d'un Tresor (Immediat « Lorsque vous gagnez ce Tresor ») du point de vue du proprietaire.
    public void DeclencherTresor(Tresor tresor, Sorcier proprietaire) => DeclencherEffets(tresor.Effets, proprietaire);

    // Pipeline de phases : declenche les clauses d'une PHASE de tour pour une liste de porteurs. Chaque clause
    // s'execute du POINT DE VUE de son porteur (Lanceur temporaire, comme DeclencherEffets) si sa Condition est
    // remplie. Appele par OrdonnanceurDeTour (DebutTour par sorcier ; FinTour une fois pour tous).
    public void DeclencherClausesPhase(PhaseTour phase, IEnumerable<Sorcier> porteurs)
    {
        foreach (var porteur in porteurs.ToList())
            foreach (var clause in porteur.Tresors.SelectMany(t => t.Clauses).Where(c => c.Phase == phase).ToList())
            {
                var ancienLanceur = Lanceur;
                var ancienneCible = DerniereCible;
                Lanceur = porteur;
                DerniereCible = null;
                if (clause.Condition?.Invoke(this) ?? true)
                    clause.Effet.Execute(this);
                Lanceur = ancienLanceur;
                DerniereCible = ancienneCible;
            }
    }

    // Vide la file des effets differes (debut de manche) : chacun joue du point de vue de son proprietaire.
    public void DeclencherEffetsDifferes()
    {
        foreach (var (effets, proprietaire) in EffetsDifferes)
            DeclencherEffets(effets, proprietaire);
        EffetsDifferes.Clear();
    }

    // Joker Magie feroce ([[magie-feroce]]) : revele la pioche principale jusqu'a trouver une carte du TYPE
    // remplace par le joker ; cette carte rejoint le sort (l'appelant la place), le joker et les revelees non
    // retenues sont defaussees (RevelerPiocheJusqua s'en charge). null si type non declare / pioche epuisee.
    public CarteSort? ResoudreMagieFeroce(MagieFeroce joker) =>
        joker.TypeRemplace is { } type ? RevelerPiocheJusqua(c => c.Type == type) : null;

    // À la déclaration ([[magie-feroce]]) : remplace IN PLACE chaque joker Magie féroce du sort par une vraie
    // carte révélée de la pioche (du type déclaré), et défausse le joker. Pioche épuisée / type non déclaré →
    // le joker disparaît sans remplacement. Appelé avant la résolution, pour que l'ordre du tour et le Jet
    // portent sur de vraies cartes. NB : la nuance « joker en Destination = Initiative 0 » n'est pas modélisée
    // (l'ordre utilise l'Initiative de la carte révélée) — simplification assumée.
    public void ResoudreJokersDuSort(List<CarteSort> sort)
    {
        for (var i = 0; i < sort.Count; i++)
            if (sort[i] is MagieFeroce joker)
            {
                var reelle = ResoudreMagieFeroce(joker);
                Defausse.Add(joker);
                if (reelle is not null)
                    sort[i] = reelle;
                else
                    sort.RemoveAt(i--);
            }
    }

    // Baisse de Tension : ajoute la 1re carte de la pioche principale au sort. Si c'est un joker Magie féroce,
    // le proprietaire la PREND EN MAIN et c'est la carte SUIVANTE de la pioche qui rejoint le sort.
    public void AugmenterSortDepuisPioche(List<CarteSort> sort, Sorcier proprietaire)
    {
        if (PiochePrincipale.Count == 0)
            return;

        var premiere = PiochePrincipale[0];
        PiochePrincipale.RemoveAt(0);

        if (premiere is MagieFeroce joker)
        {
            proprietaire.Main.Add(joker);   // « prenez-la en main »
            if (PiochePrincipale.Count == 0)
                return;
            var suivante = PiochePrincipale[0];
            PiochePrincipale.RemoveAt(0);
            if (suivante is CarteSort cs)
                sort.Add(cs);
            else
                Defausse.Add(suivante);
        }
        else if (premiere is CarteSort carte)
        {
            sort.Add(carte);
        }
    }

    // Garde la creature en cours en jeu (resultat GARDEZ d'un Jet de puissance).
    public void GarderCreatureEnCours()
    {
        if (CreatureEnCours is not null && !Lanceur.Creatures.Contains(CreatureEnCours))
            Lanceur.Creatures.Add(CreatureEnCours);
    }

    // Applique une Action : resolution de la Cible, du montant (Valeur) puis de l'effet (TypeAction).
    // Chaque cible = une INSTANCE distincte (on ne somme jamais : une Creature ne bloque qu'une instance).
    // Les cibles sont figees AVANT application (snapshot) pour ne pas etre affectees par les morts en cours.
    public void Appliquer(Action action)
    {
        var cibles = ResoudreCible(action.Cible).ToList();
        // « Adversaire qui... » au singulier : le lanceur tranche entre les matchs du filtre.
        if (action.CibleUnique && cibles.Count > 1)
            cibles = [ChoisirCible(cibles)];

        foreach (var cible in cibles)
        {
            // Quantite par defaut = 1 (« gagnez un Tresor »...). Les effets sans quantite l'ignorent.
            var montant = action.Valeur?.Calculer(this, cible) ?? 1;
            AppliquerEffet(action, cible, montant);
            if (cible != Lanceur)
                DerniereCible = cible;   // « cet adversaire » / « un autre adversaire » se calent dessus
        }
    }

    // Coût « Payez N 🩸 » d'un Composant : demande la decision au lanceur, et si oui debite N Sang.
    // Renvoie true si paye. Regles-sang : decider AVANT de resoudre (l'appelant n'execute l'effet
    // qu'apres ce true), 1x/tour pour un Composant, et impossible si pas assez de Sang.
    public bool TenterPayer(int cout, string libelle)
    {
        if (Lanceur.ADejaPayeCeTour || Lanceur.Sang < cout || !ChoisirPayer(Lanceur, cout, libelle))
            return false;
        Lanceur.Sang -= cout;
        Lanceur.ADejaPayeCeTour = true;
        return true;
    }

    // Coût « Payez N 🩸 » d'une capacite activee de TRESOR. Identique a TenterPayer mais sur la limite 1x/tour
    // SEPAREE des Composants (ADejaPayeTresorCeTour) : rulebook « le cout d'un Tresor ne peut etre paye qu'une
    // fois par tour, a votre tour d'Initiative ». Decider avant de resoudre l'effet ; impossible si pas assez.
    public bool TenterPayerTresor(int cout, string libelle)
    {
        if (Lanceur.ADejaPayeTresorCeTour || Lanceur.Sang < cout || !ChoisirPayer(Lanceur, cout, libelle))
            return false;
        Lanceur.Sang -= cout;
        Lanceur.ADejaPayeTresorCeTour = true;
        return true;
    }

    // Coût variable « Payez X 🩸 » : le lanceur choisit X (borne au Sang dispo), on debite, on renvoie X
    // (0 si rien paye / pas de Sang / deja paye ce tour). X alimente une Valeur via DerniereQuantite.
    public int TenterPayerVariable(string libelle)
    {
        if (Lanceur.ADejaPayeCeTour)
            return 0;
        var x = Math.Clamp(ChoisirMontant(Lanceur), 0, Lanceur.Sang);
        if (x <= 0)
            return 0;
        Lanceur.Sang -= x;
        Lanceur.ADejaPayeCeTour = true;
        return x;
    }

    // Complete la main du sorcier jusqu'a `taille` cartes en piochant le sommet de la pioche principale
    // (s'arrete si la pioche est epuisee). Appele au debut de chaque tour ([[constantes-de-jeu]] : main = 8).
    // TODO: pioche epuisee → remelanger la Defausse (regle de reconstitution), comme RevelerPiocheJusqua.
    public void CompleterMain(Sorcier sorcier, int taille)
    {
        // Reduction one-shot (Doigt Magique) : on vise `taille - reduction`, puis on consomme la reduction.
        var cible = Math.Max(0, taille - sorcier.ReductionPiocheProchainTour);
        sorcier.ReductionPiocheProchainTour = 0;

        while (sorcier.Main.Count < cible && PiochePrincipale.Count > 0)
        {
            var carte = PiochePrincipale[0];
            PiochePrincipale.RemoveAt(0);
            if (carte is CarteSort cs)
                sorcier.Main.Add(cs);
            else
                Defausse.Add(carte);   // carte non-sort (ne devrait pas figurer dans la pioche principale)
        }
    }

    // Revele les cartes du sommet de la pioche principale jusqu'a en trouver une qui matche `critere`
    // (ou pioche epuisee). Les cartes revelees NON retenues vont a la Defausse ; la carte trouvee est
    // retiree de la pioche et RENVOYEE (l'appelant la place : sort, main...). null si rien trouve.
    // Routine automatique (Peutidardus, Cadopourrix paye, Magie Feroce...) — pas un choix de main.
    // TODO: pioche epuisee → remelanger la Defausse (regle de reconstitution) ; ici on s'arrete.
    public CarteSort? RevelerPiocheJusqua(Predicate<CarteSort> critere)
    {
        while (PiochePrincipale.Count > 0)
        {
            var carte = PiochePrincipale[0];
            PiochePrincipale.RemoveAt(0);
            if (carte is CarteSort cs && critere(cs))
                return cs;
            Defausse.Add(carte);   // revelee mais non retenue
        }
        return null;
    }

    // Resout le sort en cours (SortEnCours) dans l'ordre de lecture Source → Qualite → Destination.
    // Une carte ajoutee pendant la resolution (GagnerCarte) rejoint le pool et se resout a son rang de
    // type : on prend a chaque etape le composant NON resolu de plus petit rang, ce qui realise la regle
    // « finir le composant courant, puis reprendre au type le plus bas non resolu » (rulebook).
    // DerniereCible/DerniereQuantite sont a l'echelle du SORT (« cet adversaire » porte sur tout le sort).
    // Composants resolus / Reactions declenchees sont a PORTEE TOUR (reset par PreparerTour, PAS par sort) :
    // ainsi, quand un sorcier meurt pendant le tour d'un autre, on sait si SES composants sont resolus
    // (= a-t-il deja joue ce tour) sans tracker l'etat par sorcier. Instances distinctes par sorcier → un
    // seul set plat suffit ([[reaction-timing]]).
    private readonly HashSet<CarteSort> _composantsResolus = [];
    private readonly HashSet<IEffet> _reactionsDeclenchees = [];

    // Sort declare de chaque sorcier ce tour (alimente par PreparerTour). Sert aux Reactions CROSS-WIZARD :
    // un sorcier peut mourir pendant le tour d'un autre, il faut alors retrouver son sort declare.
    public Dictionary<Sorcier, List<CarteSort>> SortsDeclares { get; set; } = [];

    // Debut de tour (appele par l'ordonnanceur) : memorise les sorts declares et remet a zero l'etat de
    // resolution a portee TOUR. DerniereCible/DernierDe restent a portee SORT (reset par ResoudreSort).
    public void PreparerTour(IReadOnlyDictionary<Sorcier, List<CarteSort>> sorts)
    {
        SortsDeclares = sorts.ToDictionary(kv => kv.Key, kv => kv.Value);
        _composantsResolus.Clear();
        _reactionsDeclenchees.Clear();
    }

    public void ResoudreSort()
    {
        DerniereCible = null;
        DerniereQuantite = 0;
        DernierDe = 0;

        while (true)
        {
            // Egalite de rang (plusieurs non resolus du meme type) : ordre de SortEnCours.
            // TODO: laisser le joueur ordonner (hook) ; ne survient que via cartes ajoutees.
            var composant = SortEnCours
                .Where(c => !_composantsResolus.Contains(c))
                .OrderBy(RangType)
                .FirstOrDefault();
            if (composant is null)
                break;

            _composantsResolus.Add(composant);
            ResoudreComposant(composant);

            // Lanceur mort en cours de sort (et aucune Reaction n'a empeche la mort) → le sort s'arrete :
            // les composants restants ne se resolvent pas. Une Reaction preventive (Gonzofungus PV→1) laisse
            // EstVivant a true → la resolution continue.
            if (!Lanceur.EstVivant)
                break;
        }

        CreatureEnCours = null;
    }

    // Resout un composant : execute ses Effets. Pour une Destination-Creature, pose CreatureEnCours
    // autour (le Jet de puissance s'en sert pour GARDEZ). Restaure l'ancienne valeur (composants imbriques).
    private void ResoudreComposant(CarteSort composant)
    {
        var precedente = CreatureEnCours;
        if (composant.EstCreature)
            CreatureEnCours = composant;

        // Les EffetReaction ne s'executent PAS en resolution normale : seulement a la mort (DeclencherReactions).
        foreach (var effet in composant.Effets)
            if (effet is not EffetReaction)
                effet.Execute(this);

        if (composant.EstCreature)
            CreatureEnCours = precedente;
    }

    // Rang de lecture d'un composant dans le sort. Type absent (cas hors-sort) en dernier.
    private static int RangType(CarteSort c) => c.Type switch
    {
        TypeComposant.Source => 0,
        TypeComposant.Qualite => 1,
        TypeComposant.Destination => 2,
        _ => 3,
    };

    // Demande au hook de choisir entre `min` et `max` cartes de `main` parmi celles qui passent
    // `filtre` (null = toutes), memorise le nombre choisi dans DerniereQuantite, et renvoie la selection.
    // Le max effectif est borne par le nombre de candidats eligibles.
    private IReadOnlyList<CarteSort> ChoisirEtMemoriser(List<CarteSort> main, int min, int max, Predicate<CarteSort>? filtre)
    {
        var candidats = (filtre is null ? main : main.Where(c => filtre(c))).ToList();
        var maxEffectif = Math.Min(max, candidats.Count);
        var choix = candidats.Count == 0 ? [] : ChoisirCartes(candidats, Math.Min(min, maxEffectif), maxEffectif);
        DerniereQuantite = choix.Count;
        return choix;
    }

    // Resout le choix de cartes d'une Action de manipulation de main. Si action.TypeAuChoix, demande
    // d'abord un TypeComposant (Mortalriktus) et restreint le filtre a ce type, max = TOUTES les cartes
    // du type. Sinon comportement standard (filtre fixe, max = montant).
    private IReadOnlyList<CarteSort> ChoisirCartesPourAction(Action action, Sorcier proprietaire, int max)
    {
        var filtre = action.FiltreCarte;
        if (action.TypeAuChoix)
        {
            var presents = proprietaire.Main
                .Where(c => c.Type.HasValue && (filtre is null || filtre(c)))
                .Select(c => c.Type!.Value).Distinct().ToList();
            if (presents.Count == 0)
            {
                DerniereQuantite = 0;
                return [];
            }
            var type = ChoisirTypeComposant(proprietaire, presents);
            var filtreBase = filtre;
            filtre = c => c.Type == type && (filtreBase is null || filtreBase(c));
            max = proprietaire.Main.Count(c => filtre(c));   // « cartes du même type » → toutes celles du type
        }
        return ChoisirEtMemoriser(proprietaire.Main, action.MinCartes, max, filtre);
    }

    // Adversaires vivants du lanceur, dans l'ordre de la table.
    private IEnumerable<Sorcier> Adversaires => Sorciers.Where(s => s != Lanceur && s.EstVivant);

    // Voisin vivant du lanceur dans un sens donne (+1 = gauche, -1 = droite), en sautant les morts.
    private Sorcier? Voisin(int sens) => Voisin(sens, Lanceur);

    // Voisin vivant d'un sorcier de REFERENCE dans un sens donne (+1 = gauche, -1 = droite), en sautant les
    // morts. Convention : Sorciers est dans l'ordre de table ; « gauche » = case suivante. Sert au ciblage
    // relatif a un autre que le lanceur (Sabruledepartoux : voisins du controleur du Donjon).
    private Sorcier? Voisin(int sens, Sorcier reference)
    {
        var n = Sorciers.Count;
        var i = Sorciers.IndexOf(reference);
        for (var pas = 1; pas < n; pas++)
        {
            var v = Sorciers[((i + sens * pas) % n + n) % n];
            if (v != reference && v.EstVivant)
                return v;
        }
        return null;
    }

    // Superlatif parmi les adversaires (max/min d'une cle) ; egalite tranchee par le lanceur.
    private Sorcier? Superlatif(Func<Sorcier, int> cle, bool max)
    {
        var advs = Adversaires.ToList();
        if (advs.Count == 0)
            return null;
        var seuil = max ? advs.Max(cle) : advs.Min(cle);
        var exaequo = advs.Where(s => cle(s) == seuil).ToList();
        return exaequo.Count == 1 ? exaequo[0] : ChoisirCible(exaequo);
    }

    // Resout une Cible en l'ensemble des sorciers visés. Les cibles conditionnelles
    // renvoient TOUS les adversaires qui matchent (« chaque adversaire qui... »).
    public IEnumerable<Sorcier> ResoudreCible(Cible cible)
    {
        switch (cible)
        {
            case Cible.Soi: return [Lanceur];
            case Cible.TousAdversaires: return Adversaires;
            // Tous les sorciers vivants, lanceur inclus (« tous les sorciers subissent... », Sabruledepartoux).
            case Cible.TousSorciers: return Sorciers.Where(s => s.EstVivant);
            // Le controleur du Donjon (0 ou 1 ; peut etre le lanceur). Le filtrage vivant/non est laisse
            // a l'appelant (Sabruledepartoux branche dessus via EffetConditionnel).
            case Cible.ControleurDonjon: return Enumerable1(ControleurDonjon);
            // Les deux voisins directs vivants du controleur du Donjon (peut inclure le lanceur). Sabruledepartoux paye.
            case Cible.VoisinsControleurDonjon:
                return ControleurDonjon is { } ctrl
                    ? new[] { Voisin(+1, ctrl), Voisin(-1, ctrl) }.OfType<Sorcier>().Distinct()
                    : [];
            case Cible.AdversaireGauche: return Enumerable1(Voisin(+1));
            case Cible.AdversaireDroite: return Enumerable1(Voisin(-1));
            case Cible.DeuxVoisins: return new[] { Voisin(+1), Voisin(-1) }.OfType<Sorcier>().Distinct();

            // Superlatifs (cible unique)
            case Cible.PlusFort: return Enumerable1(Superlatif(s => s.PointsDeVie, max: true));
            case Cible.PlusFaible: return Enumerable1(Superlatif(s => s.PointsDeVie, max: false));
            case Cible.PlusDeTresors: return Enumerable1(Superlatif(s => s.Tresors.Count, max: true));
            case Cible.PlusDeSang: return Enumerable1(Superlatif(s => s.Sang, max: true));
            case Cible.PlusDeCreatures: return Enumerable1(Superlatif(s => s.Creatures.Count, max: true));

            // Filtres conditionnels (ensemble des adversaires qui matchent)
            case Cible.ACreature: return Adversaires.Where(s => s.Creatures.Count > 0);
            case Cible.SansCreature: return Adversaires.Where(s => s.Creatures.Count == 0);
            case Cible.ATresor: return Adversaires.Where(s => s.Tresors.Count > 0);
            case Cible.SansTresor: return Adversaires.Where(s => s.Tresors.Count == 0);
            case Cible.ADonjon: return Adversaires.Where(s => s == ControleurDonjon);
            case Cible.SansDonjon: return Adversaires.Where(s => s != ControleurDonjon);
            case Cible.AJetonDernierSurvivant: return Adversaires.Where(s => s.JetonsDernierSurvivant > 0);
            case Cible.PlusFortQueMoi: return Adversaires.Where(s => s.PointsDeVie > Lanceur.PointsDeVie);
            case Cible.PlusFaibleQueMoi: return Adversaires.Where(s => s.PointsDeVie < Lanceur.PointsDeVie);
            // « chaque adversaire avec un nombre PAIR / IMPAIR de PV » (Groclonar).
            case Cible.PvPair: return Adversaires.Where(s => s.PointsDeVie % 2 == 0);
            case Cible.PvImpair: return Adversaires.Where(s => s.PointsDeVie % 2 != 0);

            // Le sorcier ayant porte le coup fatal (contexte Reaction a la mort ; null hors de ce contexte).
            case Cible.Tueur: return Enumerable1(_tueur);

            // Cibles relatives au sort en cours
            case Cible.MemeCible: return Enumerable1(DerniereCible);
            case Cible.AutreAdversaire:
                var autres = Adversaires.Where(s => s != DerniereCible).ToList();
                return autres.Count == 0 ? [] : [ChoisirCible(autres)];

            // « Chaque autre adversaire » = TOUS sauf la derniere cible (ensemble, ≠ AutreAdversaire qui en designe UN).
            case Cible.AutresAdversaires: return Adversaires.Where(s => s != DerniereCible);

            // « Choisissez un adversaire » : cible unique designee par le lanceur (via ChoisirCible).
            case Cible.AdversaireAuChoix:
                var advs = Adversaires.ToList();
                return advs.Count == 0 ? [] : [ChoisirCible(advs)];

            case Cible.ADejaJoue: return Adversaires.Where(s => s.ADejaJoueCeTour);
            case Cible.NaPasJoue: return Adversaires.Where(s => !s.ADejaJoueCeTour);

            // Designation par de : a brancher plus tard (necessite le tirage du de en amont).
            case Cible.DesigneParDe:
                throw new NotImplementedException($"Cible {cible} : designation par de a brancher.");

            default:
                throw new NotImplementedException($"Cible {cible} non geree.");
        }
    }

    private static IEnumerable<Sorcier> Enumerable1(Sorcier? s) => s is null ? [] : [s];

    // Applique l'effet d'un TypeAction à une cible. Bornes : PV [0...max], Sang [0...max].
    // La mort et ses recompenses (Sang au kill, vol du Donjon, jeton) relevent du systeme
    // de declencheurs (2e pilier) — PAS du resolveur. Ici, on ne fait que modifier l'etat brut.
    private void AppliquerEffet(Action action, Sorcier cible, int montant)
    {
        switch (action.Type)
        {
            case TypeAction.Degats:
                InfligerDegats(cible, montant);
                break;
            case TypeAction.AutoDegats:
                InfligerDegats(Lanceur, montant);
                break;
            case TypeAction.Soin:
                cible.PointsDeVie = Math.Min(cible.PointsDeVieMax, cible.PointsDeVie + montant);
                break;
            case TypeAction.PvAUn:
                cible.PointsDeVie = 1;
                break;
            case TypeAction.LancerDeMemorise:
                DernierDe = LancerDe();
                break;
            case TypeAction.AjouterBonusDe:
                BonusDesJetCreature += montant;
                break;
            case TypeAction.Garder:
                GarderCreatureEnCours();
                break;
            case TypeAction.CreatureEncaisse:
                // Réaction Brademinus : si une Créature non résolue est présente dans le sort de la victime,
                // elle encaisse le coup fatal. On restaure les PV d'avant-coup (« vous ne mourez pas ») et la
                // Créature est consommée (marquée résolue → ni effet de Destination ni GARDEZ ; le nettoyage de
                // fin de tour la défausse). Sans Créature, la Réaction est sans effet → la mort tient.
                var bouclier = _sortEnReaction.FirstOrDefault(c => c.EstCreature && !_composantsResolus.Contains(c));
                if (bouclier is not null)
                {
                    cible.PointsDeVie = _pvAvantMort;
                    _composantsResolus.Add(bouclier);
                }
                break;
            case TypeAction.GagnerSang:
                cible.Sang = Math.Min(cible.SangMax, cible.Sang + montant);
                break;
            case TypeAction.PerdreSang:
                cible.Sang = Math.Max(0, cible.Sang - montant);
                break;
            case TypeAction.VolerSang:
                var vol = Math.Min(montant, cible.Sang);
                cible.Sang -= vol;
                Lanceur.Sang = Math.Min(Lanceur.SangMax, Lanceur.Sang + vol);
                break;
            case TypeAction.DonnerSang:
                // Transfert lanceur → cible, borne au Sang dispo du lanceur (« si vous en avez »).
                var don = Math.Min(montant, Lanceur.Sang);
                Lanceur.Sang -= don;
                cible.Sang = Math.Min(cible.SangMax, cible.Sang + don);
                break;
            case TypeAction.PrendreDonjon:
                ControleurDonjon = Lanceur;

                break;
            case TypeAction.DonnerDonjon:
                ControleurDonjon = cible;
                break;
            case TypeAction.GagnerTresor:
                // Pioche `montant` Tresors du sommet vers la cible (s'arrete si la pioche est vide).
                for (var i = 0; i < montant && PiocheTresor.Count > 0; i++)
                {
                    var tresor = PiocheTresor[0];
                    PiocheTresor.RemoveAt(0);
                    cible.Tresors.Add(tresor);
                    // « Lorsque vous gagnez ce Tresor » (Immediat) : effet one-shot du point de vue du gagnant.
                    if (tresor.TriggerType == TriggerType.Immediat)
                        DeclencherTresor(tresor, cible);
                }
                break;
            case TypeAction.VolerTresor:
                // Vole `montant` Tresors de la cible vers le lanceur.
                // TODO choix : pour l'instant on prend le 1er ; le voleur devrait pouvoir choisir.
                for (var i = 0; i < montant && cible.Tresors.Count > 0; i++)
                {
                    var t = cible.Tresors[0];
                    cible.Tresors.RemoveAt(0);
                    Lanceur.Tresors.Add(t);
                }
                break;
            case TypeAction.DefausserTresor:
                // La cible defausse `montant` Tresors (remis sous la pioche Tresor).
                // TODO choix : la cible devrait choisir lesquels ; pour l'instant le 1er.
                for (var i = 0; i < montant && cible.Tresors.Count > 0; i++)
                {
                    var t = cible.Tresors[0];
                    cible.Tresors.RemoveAt(0);
                    PiocheTresor.Add(t);
                }
                
                
                break;

            case TypeAction.TuerCreature:
                // Tue `montant` Creatures de la cible (defaussees).
                // NB : « tuez TOUTES ses Creatures » (Fourchétix) n'est pas exprimable avec un nombre
                // fixe → necessitera une Valeur dediee (cf. design à trancher).
                for (var i = 0; i < montant && cible.Creatures.Count > 0; i++)
                {
                    Defausse.Add(cible.Creatures[0]);
                    cible.Creatures.RemoveAt(0);
                }
                break;
            case TypeAction.RevelerMain:
                // Informatif : les effets qui suivent lisent le contenu/compte de la main directement.
                // Aucun changement d'etat ici (hook de visibilite à ajouter si l'UI en a besoin).
                break;

            case TypeAction.GagnerCarte:
                // « Ajoutez à votre sort N carte(s) de votre main » (Vishnakrax, Cadopourrix, Shub 10+).
                // On DEPLACE la carte choisie de la main vers SortEnCours ; sa resolution propre
                // (effets / Jet) sera le travail de la future boucle de resolution de sort.
                foreach (var c in ChoisirCartesPourAction(action, cible, montant))
                {
                    cible.Main.Remove(c);
                    SortEnCours.Add(c);
                }
                break;
            case TypeAction.GagnerCarteAuHasard:
                // « Ajoutez au sort N carte(s) AU HASARD de la main » (Bébéfédex payé) : l'index est tiré par
                // le hook (injection du hasard), ≠ GagnerCarte qui laisse le joueur CHOISIR.
                for (var i = 0; i < montant && cible.Main.Count > 0; i++)
                {
                    var idx = ChoisirIndexAuHasard(cible.Main.Count);
                    var carte = cible.Main[idx];
                    cible.Main.RemoveAt(idx);
                    SortEnCours.Add(carte);
                }
                break;

            case TypeAction.DefausserCartes:
                // « Defaussez (jusqu'a) N carte(s) de votre main » (Sarabandus...). Le nombre defausse
                // est memorise dans DerniereQuantite pour alimenter une Valeur suivante (cf. ValeurQuantiteChoisie).
                foreach (var c in ChoisirCartesPourAction(action, cible, montant))
                {
                    cible.Main.Remove(c);
                    Defausse.Add(c);
                }
                break;

            case TypeAction.PasserCartes:
                // « (La cible) vous donne N carte(s) de sa main » → cible.Main vers la main du Lanceur
                // (Roulepélax). La cible choisit lesquelles via ChoisirCartes. Le don VERS LE SORT
                // (Boucledorus) passe lui par GagnerCarte (cible.Main → SortEnCours).
                foreach (var c in ChoisirCartesPourAction(action, cible, montant))
                {
                    cible.Main.Remove(c);
                    Lanceur.Main.Add(c);
                }
                break;

            default:
                throw new NotImplementedException($"TypeAction {action.Type} non geree.");
        }
    }
}
