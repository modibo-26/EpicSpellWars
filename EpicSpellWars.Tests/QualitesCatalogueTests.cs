using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Cartes Qualités encodées (parties exprimables ; les // GAP / // TODO restent non couverts).
public class QualitesCatalogueTests
{
    private static CarteSort Qualite(string nom) => Qualites.Toutes().Single(c => c.Nom == nom);

    private static void Jouer(Table t, CarteSort carte)
    {
        t.Ctx.DerniereCible = null;
        t.Ctx.DerniereQuantite = 0;
        foreach (var effet in carte.Effets)
            effet.Execute(t.Ctx);
    }

    private static Tresor Tres() => new("Trésor", [], TriggerType.Passif);

    // Gromago — Merlin gagne 2 Trésors ; l'adversaire qui n'a pas joué en gagne 1 ; payé : celui qui
    // a joué en défausse 1.
    [Fact]
    public void Gromago_distribue_et_defausse_des_tresors()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        t.Ctx.PiocheTresor = [Tres(), Tres(), Tres(), Tres()];
        t.Saroumane.ADejaJoueCeTour = true;       // a déjà joué → défaussera
        t.Saroumane.Tresors.Add(Tres());          // possède 1 Trésor
        // Gandalf : ADejaJoueCeTour = false (défaut) → n'a pas joué → gagne 1

        Jouer(t, Qualite("Gromago"));

        Assert.Equal(2, t.Merlin.Tresors.Count);      // a gagné 2
        Assert.Single(t.Gandalf.Tresors);     // n'a pas joué → +1
        Assert.Empty(t.Saroumane.Tresors);            // a joué → a défaussé son Trésor
        Assert.Equal(0, t.Merlin.Sang);               // 2 🩸 débités
    }

    // Mortalriktus — défausse le type choisi (Source), dégâts = nombre défaussé ; payé = doublé.
    [Fact]
    public void Mortalriktus_degats_egal_nombre_defausse()
    {
        var t = new Table { ProchainType = TypeComposant.Source, ProchainPaye = false };
        t.Gandalf.PointsDeVie = 10;
        t.Merlin.Main.AddRange(
        [
            new CarteSort("Flamme", TypeComposant.Source, Glyphe.Elementaire),
            new CarteSort("Givre", TypeComposant.Source, Glyphe.Elementaire),
            new CarteSort("Halo", TypeComposant.Qualite, Glyphe.Arcane),
        ]);

        Jouer(t, Qualite("Mortalriktus"));

        Assert.Equal(2, t.Ctx.DerniereQuantite);
        Assert.Equal(8, t.Gandalf.PointsDeVie);   // 10 - 2
    }

    [Fact]
    public void Mortalriktus_paye_double_les_degats()
    {
        var t = new Table { ProchainType = TypeComposant.Source, ProchainPaye = true };
        t.Merlin.Sang = 4;
        t.Gandalf.PointsDeVie = 10;
        t.Merlin.Main.AddRange(
        [
            new CarteSort("Flamme", TypeComposant.Source, Glyphe.Elementaire),
            new CarteSort("Givre", TypeComposant.Source, Glyphe.Elementaire),
        ]);

        Jouer(t, Qualite("Mortalriktus"));

        Assert.Equal(6, t.Gandalf.PointsDeVie);   // 10 - (2 cartes × 2)
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Peutidardus — révèle la pioche jusqu'à une Créature, l'ajoute au sort.
    [Fact]
    public void Peutidardus_ajoute_une_creature_de_la_pioche_au_sort()
    {
        var t = new Table();
        var creature = new CarteSort("Bestiole", TypeComposant.Destination, Glyphe.Tenebres);
        t.Ctx.PiochePrincipale =
        [
            new CarteSort("Carte-Source", TypeComposant.Source, Glyphe.Arcane),
            creature,
            new CarteSort("Carte-reste", TypeComposant.Qualite, Glyphe.Illusion),
        ];

        Jouer(t, Qualite("Peutidardus"));

        Assert.Contains(creature, t.Ctx.SortEnCours);
        Assert.Single(t.Ctx.PiochePrincipale);   // « Carte-reste » seule restante
    }
}
