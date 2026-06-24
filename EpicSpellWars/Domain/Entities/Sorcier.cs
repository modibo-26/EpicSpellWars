namespace EpicSpellWars.Domain.Entities;

public class Sorcier(string nom)
{
    // PV de depart d'une manche (jeton crane sur la case 20) ; plafond = 25 ([[constantes-de-jeu]]).
    public const int PvDepart = 20;

    public string Nom { get; set; } = nom;
    public int PointsDeVie { get; set; } = PvDepart;
    public int PointsDeVieMax { get; } = 25;
    public int Sang { get; set; }
    public int SangMax { get; } = 25;

    // Jetons Dernier Survivant gagnes (victoire a 2). A ne pas confondre avec le jeton PV (marqueur de vie).
    public int JetonsDernierSurvivant { get; set; }

    public bool EstVivant => PointsDeVie > 0;

    // A deja resolu son sort ce tour (remis a false en debut de tour).
    public bool ADejaJoueCeTour { get; set; }

    // A deja paye un cout de Composant ce tour (regles-sang : 1x/tour). Remis a false en debut de tour.
    public bool ADejaPayeCeTour { get; set; }

    // A deja paye un cout de TRESOR ce tour (capacite activee). Limite 1x/tour SEPAREE des Composants
    // (rulebook : « le cout d'un Tresor ne peut etre paye qu'une fois par tour, a votre tour d'Initiative »).
    public bool ADejaPayeTresorCeTour { get; set; }

    // A deja relance un Jet de puissance entier ce tour (Manuel de Cryptozoic, 1x/tour). Reset en debut de tour.
    public bool ADejaRelanceJetCeTour { get; set; }

    // Reduction one-shot du nombre de cartes a piocher au prochain CompleterMain (Doigt Magique : le vainqueur
    // d'une manche pioche 2 cartes de moins au debut de la suivante). Consommee (remise a 0) des qu'appliquee.
    public int ReductionPiocheProchainTour { get; set; }

    // Armé par Baisse de Tension (crevé MancheSuivante) : le PREMIER sort joué la manche suivante est augmenté
    // de la 1re carte de la pioche. Consommé (remis a false) des que ce premier sort est declare.
    public bool AugmenterPremierSort { get; set; }

    // Ce sorcier est-il le PREMIER tué de la manche en cours ? Fixe au moment de sa mort (OnMort), remis a
    // false en debut de manche. Lu par la conditionnelle de Tournee d'Adieu (« si premier tue : +2 🩸 »).
    public bool EstPremierMortCetteManche { get; set; }

    // Des supplementaires gardes pour le PROCHAIN Jet de puissance pour une Creature (Petit Ange, Passif).
    // Persiste tant qu'il n'est pas consomme (meme d'une manche a l'autre) ; lu+consomme par GameContext.BonusDesJet.
    public int BonusProchainJetCreature { get; set; }

    public List<CarteSort> Main { get; set; } = [];
    public List<CarteSort> Creatures { get; set; } = [];
    public List<Tresor> Tresors { get; set; } = [];

    // Cartes glissées sous « Buffet à Volonté » : leur Glyphe compte dans chacun de vos sorts (CompterGlyphes).
    // Défaussées en fin de manche avec les Trésors.
    public List<CarteSort> SousBuffet { get; set; } = [];

    // Cartes Sorcier creve piochees a la mort (consolation du mort, tenues devant soi).
    public List<SorcierCreve> SorciersCreves { get; set; } = [];
}
