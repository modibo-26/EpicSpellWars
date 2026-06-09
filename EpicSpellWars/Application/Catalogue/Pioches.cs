using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Application.Catalogue;

// Assemble les pioches du jeu a partir des catalogues.
// Toutes les cartes sont des instances FRAICHES (un exemplaire physique = un objet distinct).
public static class Pioches
{
    private const int ExemplairesParCarteSort = 2;
    private const int NbMagieFeroce = 8;

    // Pioche principale : 60 cartes sort uniques x2 (= 120) + 8 Magie féroce.
    public static List<Carte> Principale()
    {
        var pioche = new List<Carte>();

        for (var i = 0; i < ExemplairesParCarteSort; i++)
        {
            pioche.AddRange(SourcesCatalogue.Creer());
            pioche.AddRange(QualitesCatalogue.Creer());
            pioche.AddRange(DestinationsCatalogue.Creer());
        }

        for (var i = 0; i < NbMagieFeroce; i++)
            pioche.Add(new MagieFeroce());

        return pioche;
    }

    // Pioche Trésor : 25 cartes uniques, 1 exemplaire chacune.
    public static List<Tresor> Tresor() => TresorsCatalogue.Creer();

    // Pioche Sorcier crevé : 8 uniques avec leurs multiplicités (25 cartes).
    public static List<SorcierCreve> SorcierCreve() => SorciersCreveCatalogue.Creer();
}
