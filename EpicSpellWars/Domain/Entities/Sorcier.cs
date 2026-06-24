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

    public List<CarteSort> Main { get; set; } = [];
    public List<CarteSort> Creatures { get; set; } = [];
    public List<Tresor> Tresors { get; set; } = [];

    // Cartes Sorcier creve piochees a la mort (consolation du mort, tenues devant soi).
    public List<SorcierCreve> SorciersCreves { get; set; } = [];
}
