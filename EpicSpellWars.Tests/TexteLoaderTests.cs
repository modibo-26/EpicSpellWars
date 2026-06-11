using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Verifie le pont catalogue C# <-> data/*.json : TexteLoader charge les JSON embarques et rattache
// le texte verbatim par Id. Sert aussi de garde anti-divergence (un Id du catalogue sans entree JSON
// laisserait un Texte vide -> echec ici).
public class TexteLoaderTests
{
    [Fact]
    public void PiochePrincipale_rattache_le_texte_par_id()
    {
        var pioche = Catalogue.PiochePrincipale();

        // Carte temoin : Foulremix (EP2-002) porte bien son texte verbatim.
        var foulremix = pioche.First(c => c.Id == "EP2-002");
        Assert.StartsWith("Choisissez un type de cartes", foulremix.Texte);
    }

    [Fact]
    public void Toutes_les_cartes_encodees_ont_un_texte_non_vide()
    {
        var pioche = Catalogue.PiochePrincipale();

        var sansTexte = pioche.Where(c => string.IsNullOrEmpty(c.Texte)).Select(c => $"{c.Id} {c.Nom}").Distinct();
        Assert.Empty(sansTexte);
    }
}
