using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Assemble la pioche principale a partir des catalogues par type.
// Encodees : Sources + Qualites + Destinations. MagieFeroce s'ajoutera ici (.Concat(...)).
public static class Catalogue
{
    // ATTENTION : Enumerable.Repeat renvoie la MEME instance N fois (alias). Acceptable tant qu'on ne
    // tire pas reellement la pioche ni ne deplace les cartes entre zones (main/defausse) ; a remplacer
    // par des clones distincts quand le vrai cycle de jeu sera implemente.
    public static List<Carte> PiochePrincipale()
    {
        var modeles = Sources.Toutes()
            .Concat(Qualites.Toutes())
            .Concat(Destinations.Toutes())
            .Cast<Carte>()
            .ToList();

        // Rattache le texte verbatim depuis data/*.json (jointure par Id) avant de tirer les exemplaires.
        TexteLoader.AppliquerTextes(modeles);

        return modeles
            .SelectMany(c => Enumerable.Repeat(c, c.Exemplaires))
            .ToList();
    }

    // Piles SEPAREES de la pioche principale (permanents / pioche de mort / Magie feroce). Meme schema :
    // texte rattache par Id, puis exemplaires tires.
    public static List<Tresor> PiocheTresor() => AvecTextePuisExemplaires(Tresors.Toutes());

    public static List<SorcierCreve> PiocheSorcierCreve() => AvecTextePuisExemplaires(SorciersCreves.Toutes());

    public static List<MagieFeroce> PiocheMagieFeroce() => AvecTextePuisExemplaires(MagiesFeroces.Toutes());

    private static List<T> AvecTextePuisExemplaires<T>(List<T> modeles) where T : Carte
    {
        TexteLoader.AppliquerTextes(modeles);
        return modeles.SelectMany(c => Enumerable.Repeat(c, c.Exemplaires)).ToList();
    }
}
