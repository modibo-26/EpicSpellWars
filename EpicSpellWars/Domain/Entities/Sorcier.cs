namespace EpicSpellWars.Domain.Entities;

public class Sorcier(string nom)
{
    public string Nom { get; set; } = nom;
    public int PointsDeVie { get; set; } = 20;
    public int PointsDeVieMax { get; } = 25;
    public int Sang { get; set; } = 0;
    public int SangMax { get; } = 25;

    // Jetons Dernier Survivant gagnes (victoire a 2). A ne pas confondre avec le jeton PV (marqueur de vie).
    public int JetonsDernierSurvivant { get; set; } = 0;

    public bool EstVivant => PointsDeVie > 0;

    // A deja resolu son sort ce tour (remis a false en debut de tour).
    public bool ADejaJoueCeTour { get; set; } = false;

    public List<CarteSort> Main { get; set; } = [];
    public List<CarteSort> Creatures { get; set; } = [];
    public List<Tresor> Tresors { get; set; } = [];
}
