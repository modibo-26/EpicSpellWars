using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Destinations encodées : Jet de puissance (paliers 1-4 / 5-9 / 10+) + GARDEZ. On résout via
// ResoudreSort pour que le composant soit posé comme Créature en cours (CompterGlyphes => 1 dé,
// CreatureEnCours => GARDEZ). ProchainDe pilote le palier (1 dé = somme = ProchainDe).
// Les // GAP (cible filtrante au singulier, « chaque autre adversaire », paiements) restent non couverts.
public class DestinationsCatalogueTests
{
    private static CarteSort Destination(string nom) => Destinations.Toutes().Single(c => c.Nom == nom);

    private static void Resoudre(Table t, CarteSort destination, int de)
    {
        t.ProchainDe = de;
        t.Ctx.SortEnCours = [destination];
        t.Ctx.ResoudreSort();
    }

    // Cubengelus 10+ : 4 dégâts à gauche, gagnez 1 Trésor, GARDEZ.
    [Fact]
    public void Cubengelus_10plus_frappe_gagne_un_tresor_et_garde()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [new Tresor("Trésor", [], Domain.Enums.TriggerType.Passif)];
        var cubengelus = Destination("Cubengelus");

        Resoudre(t, cubengelus, de: 10);

        Assert.Equal(16, t.Gandalf.PointsDeVie);          // gauche : 20 - 4
        Assert.Equal(20, t.Saroumane.PointsDeVie);        // pas touché
        Assert.Single(t.Merlin.Tresors);                  // gagné 1 Trésor
        Assert.Contains(cubengelus, t.Merlin.Creatures);  // GARDEZ
    }

    // Cubengelus 1-4 : 2 dégâts seulement, pas de Trésor, pas de GARDEZ.
    [Fact]
    public void Cubengelus_1a4_frappe_seulement()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [new Tresor("Trésor", [], Domain.Enums.TriggerType.Passif)];
        var cubengelus = Destination("Cubengelus");

        Resoudre(t, cubengelus, de: 2);

        Assert.Equal(18, t.Gandalf.PointsDeVie);          // gauche : 20 - 2
        Assert.Empty(t.Merlin.Tresors);                   // pas de Trésor en 1-4
        Assert.DoesNotContain(cubengelus, t.Merlin.Creatures);
    }

    // Barbaryaga 5-9 : la cible (le plus de 🩸) subit 2 dégâts puis perd 1 🩸 (MemeCible).
    [Fact]
    public void Barbaryaga_5a9_degats_puis_perte_de_sang_sur_la_meme_cible()
    {
        var t = new Table();
        t.Gandalf.Sang = 5;        // le plus de 🩸 parmi les adversaires
        t.Saroumane.Sang = 0;

        Resoudre(t, Destination("Barbaryaga"), de: 5);

        Assert.Equal(18, t.Gandalf.PointsDeVie);   // 20 - 2
        Assert.Equal(4, t.Gandalf.Sang);           // 5 - 1
        Assert.Equal(20, t.Saroumane.PointsDeVie); // pas la cible
    }

    // Coco-Cocoricus 1-4 : GARDEZ seul, aucun dégât.
    [Fact]
    public void CocoCocoricus_1a4_garde_sans_degats()
    {
        var t = new Table();
        var coco = Destination("Coco-Cocoricus");

        Resoudre(t, coco, de: 3);

        Assert.Contains(coco, t.Merlin.Creatures);          // GARDEZ
        Assert.Equal(20, t.Gandalf.PointsDeVie);            // aucun dégât
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // Logocrypto 10+ : 4 dégâts + vol de 1 🩸 sur la cible (le plus de 🩸), GARDEZ.
    [Fact]
    public void Logocrypto_10plus_frappe_vole_du_sang_et_garde()
    {
        var t = new Table();
        t.Gandalf.Sang = 6;        // le plus de 🩸
        t.Saroumane.Sang = 1;
        var logo = Destination("Logocrypto");

        Resoudre(t, logo, de: 10);

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // 20 - 4
        Assert.Equal(5, t.Gandalf.Sang);           // 6 - 1 volé
        Assert.Equal(1, t.Merlin.Sang);            // +1 volé
        Assert.Contains(logo, t.Merlin.Creatures); // GARDEZ
    }
}
