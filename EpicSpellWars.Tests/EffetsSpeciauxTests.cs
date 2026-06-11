using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Effets hors-modèle par-cible : Mortalriktus (type au choix), Foulremix (passage simultané),
// EffetRevelerPioche (révélation de la pioche).
public class EffetsSpeciauxTests
{
    // 10) Mortalriktus — choisit un type (Source), défausse ces cartes, dégâts = nombre défaussé.
    [Fact]
    public void Mortalriktus_defausse_le_type_choisi_et_inflige_le_compte()
    {
        var t = new Table { ProchainType = TypeComposant.Source };
        t.Gandalf.PointsDeVie = 10;
        var halo = new CarteSort("Halo", TypeComposant.Qualite, Glyphe.Arcane);
        t.Merlin.Main.AddRange(
        [
            new CarteSort("Flamme", TypeComposant.Source, Glyphe.Elementaire),
            new CarteSort("Givre", TypeComposant.Source, Glyphe.Elementaire),
            halo,
        ]);

        new EffetSimple
        {
            Actions =
            [
                new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, TypeAuChoix = true },
                new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurQuantiteChoisie(1) },
            ],
        }.Execute(t.Ctx);

        Assert.Equal(2, t.Ctx.DerniereQuantite);
        Assert.Equal(8, t.Gandalf.PointsDeVie);     // 10 - 2 cartes Source défaussées
        Assert.Equal([halo], t.Merlin.Main);        // la Qualité reste
    }

    // 11) Foulremix — chacun passe ses Source au voisin de gauche ; dégâts = cartes reçues.
    //     Merlin[1]→Gandalf, Gandalf[2]→Saroumane, Saroumane[0]→Merlin. Dégâts 0/1/2.
    [Fact]
    public void Foulremix_passe_a_gauche_et_inflige_les_recues()
    {
        var t = new Table { ProchainType = TypeComposant.Source };
        var mSource = new CarteSort("M-Source", TypeComposant.Source, Glyphe.Primaire);
        var gSource1 = new CarteSort("G-Source 1", TypeComposant.Source, Glyphe.Primaire);
        var gSource2 = new CarteSort("G-Source 2", TypeComposant.Source, Glyphe.Primaire);
        var sQualite = new CarteSort("S-Qualité", TypeComposant.Qualite, Glyphe.Illusion);
        t.Merlin.Main.Add(mSource);
        t.Gandalf.Main.AddRange([gSource1, gSource2]);
        t.Saroumane.Main.Add(sQualite);

        new EffetFoulremix().Execute(t.Ctx);

        Assert.Equal(20, t.Merlin.PointsDeVie);      // reçoit 0
        Assert.Equal(19, t.Gandalf.PointsDeVie);     // reçoit 1 (de Merlin)
        Assert.Equal(18, t.Saroumane.PointsDeVie);   // reçoit 2 (de Gandalf)
        Assert.Equal([mSource], t.Gandalf.Main);
        Assert.Equal([sQualite, gSource1, gSource2], t.Saroumane.Main);
        Assert.Empty(t.Merlin.Main);
    }

    // 13) Peutidardus — révèle la pioche jusqu'à une Créature ; la garde au sort, défausse les autres.
    [Fact]
    public void RevelerPioche_garde_la_creature_et_defausse_le_reste()
    {
        var t = new Table();
        var creature = new CarteSort("Pioche-Créature", TypeComposant.Destination, Glyphe.Tenebres);
        var source = new CarteSort("Pioche-Source", TypeComposant.Source, Glyphe.Elementaire);
        var qualite = new CarteSort("Pioche-Qualité", TypeComposant.Qualite, Glyphe.Illusion);
        var reste = new CarteSort("Pioche-reste", TypeComposant.Source, Glyphe.Primaire);
        t.Ctx.PiochePrincipale = [source, qualite, creature, reste];

        new EffetRevelerPioche { Critere = c => c.EstCreature, Nombre = 1 }.Execute(t.Ctx);

        Assert.Equal([creature], t.Ctx.SortEnCours);
        Assert.Equal([source, qualite], t.Ctx.Defausse);
        Assert.Equal([reste], t.Ctx.PiochePrincipale);
    }
}
