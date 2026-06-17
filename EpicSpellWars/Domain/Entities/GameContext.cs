using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Domain.Entities;

public class GameContext
{
    public List<Sorcier> Sorciers { get; set; } = [];
    public required Sorcier Lanceur { get; set; }
    public Sorcier? ControleurDonjon { get; set; }

    public List<Carte> PiochePrincipale { get; set; } = [];
    public List<Carte> Defausse { get; set; } = [];
    public List<Tresor> PiocheTresor { get; set; } = [];
    public List<SorcierCreve> PiocheSorcierCreve { get; set; } = [];

    public int Manche { get; set; }
    public int Tour { get; set; }

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

    // Choix d'un type de composant (Source/Qualite/Destination) par UN sorcier, parmi une liste.
    // Ex. Mortalriktus (puis defausse de ce type), Foulremix (puis passage de ce type).
    public required Func<Sorcier, IReadOnlyList<TypeComposant>, TypeComposant> ChoisirTypeComposant { get; set; }

    // Le lanceur decide-t-il de payer ce cout en Sang ? (« Payez N 🩸 : … »). libelle = effet propose.
    public required Func<Sorcier, int, string, bool> ChoisirPayer { get; set; }
    // Montant choisi pour un cout variable « Payez X 🩸 » (sera borne au Sang dispo).
    public required Func<Sorcier, int> ChoisirMontant { get; set; }

    // Nb de cartes du sort en cours (+ creatures gardees) portant ce Glyphe.
    public int CompterGlyphes(Glyphe glyphe) =>
        SortEnCours.Count(c => c.Glyphe == glyphe)
        + Lanceur.Creatures.Count(c => c.Glyphe == glyphe);

    // Des ajoutes a CE Jet de puissance par les modificateurs actifs (Shub-Niggurath paye ; plus tard
    // Petit Ange et autres via le pilier declencheurs). Tous les Jets de puissance sont « pour une Creature ».
    public int BonusDesJet(EffetJetDePuissance jet) => BonusDesJetCreature;

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
    public void ResoudreSort()
    {
        DerniereCible = null;
        DerniereQuantite = 0;
        DernierDe = 0;

        var resolus = new HashSet<CarteSort>();
        while (true)
        {
            // Egalite de rang (plusieurs non resolus du meme type) : ordre de SortEnCours.
            // TODO: laisser le joueur ordonner (hook) ; ne survient que via cartes ajoutees.
            var composant = SortEnCours
                .Where(c => !resolus.Contains(c))
                .OrderBy(RangType)
                .FirstOrDefault();
            if (composant is null)
                break;

            resolus.Add(composant);
            ResoudreComposant(composant);
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

        foreach (var effet in composant.Effets)
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

    // Voisin vivant dans un sens donne (+1 = gauche, -1 = droite), en sautant les morts.
    // Convention : Sorciers est dans l'ordre de table ; « gauche » = case suivante.
    private Sorcier? Voisin(int sens)
    {
        var n = Sorciers.Count;
        var i = Sorciers.IndexOf(Lanceur);
        for (var pas = 1; pas < n; pas++)
        {
            var v = Sorciers[((i + sens * pas) % n + n) % n];
            if (v != Lanceur && v.EstVivant)
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
                cible.PointsDeVie = Math.Max(0, cible.PointsDeVie - montant);
                break;
            case TypeAction.AutoDegats:
                Lanceur.PointsDeVie = Math.Max(0, Lanceur.PointsDeVie - montant);
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
                    cible.Tresors.Add(PiocheTresor[0]);
                    PiocheTresor.RemoveAt(0);
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
