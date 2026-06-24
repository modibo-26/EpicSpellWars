using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche E : Sorciers crevés piochés à la mort (Immediat tout de suite / MancheSuivante différé)
// + joker Magie féroce (révèle la pioche jusqu'au type remplacé).
public class SorciersCrevesEtMagieFeroceTests
{
    private static SorcierCreve Creve(string nom) => SorciersCreves.Toutes().Single(c => c.Nom == nom);
    private static CarteSort Simple(string nom, TypeComposant type) => new(nom, type, Glyphe.Arcane);

    // À la mort, la victime pioche un Sorcier crevé Immédiat qui se résout aussitôt (Tournée d'Adieu = +2 🩸).
    [Fact]
    public void Mort_pioche_un_sorcier_creve_immediat_qui_se_resout()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Tournée d'Adieu")];
        t.Merlin.PointsDeVie = 2;

        // Suicide : Merlin meurt → pioche le crevé Immédiat.
        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(4, t.Merlin.Sang);                  // Tournée d'Adieu : +2 🩸 + 2 🩸 (premier/seul tué de la manche)
        Assert.Single(t.Merlin.SorciersCreves);
    }

    // Bilan Sanguin (Immédiat) : le mort vole 1 🩸 à chaque sorcier vivant.
    [Fact]
    public void Bilan_sanguin_vole_du_sang_aux_vivants_a_la_mort()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Bilan Sanguin")];
        t.Gandalf.Sang = 3;
        t.Saroumane.Sang = 4;
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(2, t.Gandalf.Sang);   // -1
        Assert.Equal(3, t.Saroumane.Sang); // -1
        Assert.Equal(2, t.Merlin.Sang);    // +1 +1 volés (mort, mais le Sang persiste)
    }

    // Un Sorcier crevé MancheSuivante est différé puis joué au début de la manche suivante.
    [Fact]
    public void Sorcier_creve_manche_suivante_est_differe_jusqu_a_debut_manche()
    {
        var t = new Table();
        // Crevé MancheSuivante porteur d'un effet observable (+2 🩸).
        var differe = new SorcierCreve("Différé-test",
            [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurFixe(2) }] }],
            TriggerType.MancheSuivante);
        t.Ctx.PiocheSorcierCreve = [differe];
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(0, t.Merlin.Sang);              // pas encore : différé
        Assert.Single(t.Ctx.EffetsDifferes);

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.Equal(2, t.Merlin.Sang);              // joué au début de la manche
        Assert.Empty(t.Ctx.EffetsDifferes);
    }

    // Pas de pile de crevés → la mort ne pioche rien (pas d'exception).
    [Fact]
    public void Mort_sans_pile_de_creves_ne_pioche_rien()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Empty(t.Merlin.SorciersCreves);
    }

    // Joker Magie féroce : révèle la pioche jusqu'à une carte du type remplacé et la renvoie.
    [Fact]
    public void Magie_feroce_revele_jusqu_au_type_remplace()
    {
        var t = new Table();
        var qualite = Simple("Q", TypeComposant.Qualite);
        t.Ctx.PiochePrincipale = [Simple("S1", TypeComposant.Source), Simple("S2", TypeComposant.Source), qualite, Simple("D", TypeComposant.Destination)];
        var joker = new MagieFeroce { TypeRemplace = TypeComposant.Qualite };

        var trouvee = t.Ctx.ResoudreMagieFeroce(joker);

        Assert.Same(qualite, trouvee);                 // 1re Qualité de la pioche
        Assert.Equal(2, t.Ctx.Defausse.Count);         // les 2 Sources révélées et défaussées
        Assert.Single(t.Ctx.PiochePrincipale);         // reste la Destination
    }
}
