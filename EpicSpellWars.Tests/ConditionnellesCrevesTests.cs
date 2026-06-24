using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche E — conditionnelles des Sorciers crevés : Tournée d'Adieu (premier tué), Sorcier sous
// Terre (Donjon mort en fin de tour), Repos Mérité (Trésor différé si aucun jeton), Petit Ange (Passif : +1
// dé au prochain Jet de Créature).
public class ConditionnellesCrevesTests
{
    private static SorcierCreve Creve(string nom) => SorciersCreves.Toutes().Single(c => c.Nom == nom);

    private static void Suicide(Table t, Sorcier qui)
    {
        qui.PointsDeVie = 2;
        t.Ctx.Lanceur = qui;
        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });
    }

    // Tournée d'Adieu : le PREMIER (seul) tué de la manche gagne +2 +2 = 4 🩸.
    [Fact]
    public void Tournee_d_adieu_premier_tue_gagne_quatre()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Tournée d'Adieu")];

        Suicide(t, t.Merlin);

        Assert.Equal(4, t.Merlin.Sang);
    }

    // Tournée d'Adieu : un sorcier tué APRÈS un autre n'a que +2 🩸 (pas le premier).
    [Fact]
    public void Tournee_d_adieu_pas_premier_tue_gagne_deux()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Tournée d'Adieu")];
        t.Gandalf.PointsDeVie = 0;   // déjà mort avant → Saroumane ne sera pas le premier

        Suicide(t, t.Saroumane);

        Assert.Equal(2, t.Saroumane.Sang);
    }

    // Sorcier sous Terre : un contrôleur du Donjon MORT gagne 4 🩸 (au lieu de 1) en fin de tour.
    [Fact]
    public void Sorcier_sous_terre_controleur_mort_gagne_quatre_en_fin_de_tour()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 0;                       // mort
        t.Gandalf.SorciersCreves = [Creve("Sorcier sous Terre")];
        t.Ctx.ControleurDonjon = t.Gandalf;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>());

        Assert.Equal(4, t.Gandalf.Sang);
    }

    // Sorcier sous Terre : un contrôleur VIVANT (même porteur du crevé) ne gagne que 1 🩸 (override = si mort).
    [Fact]
    public void Sorcier_sous_terre_controleur_vivant_gagne_un()
    {
        var t = new Table();
        t.Gandalf.SorciersCreves = [Creve("Sorcier sous Terre")];   // vivant
        t.Ctx.ControleurDonjon = t.Gandalf;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>());

        Assert.Equal(1, t.Gandalf.Sang);
    }

    // Repos Mérité sans jeton : +0 🩸 maintenant, mais 1 Trésor au début de la manche suivante (différé).
    [Fact]
    public void Repos_merite_sans_jeton_gagne_un_tresor_la_manche_suivante()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Repos Mérité")];
        t.Ctx.PiocheTresor = [new Tresor("T", [], TriggerType.Passif)];

        Suicide(t, t.Merlin);   // 0 jeton

        Assert.Equal(0, t.Merlin.Sang);        // +1 par jeton, 0 jeton → 0
        Assert.Empty(t.Merlin.Tresors);        // pas encore
        Assert.Single(t.Ctx.EffetsDifferes);

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.Single(t.Merlin.Tresors);       // Trésor gagné au début de la manche
    }

    // Repos Mérité avec jetons : +1 🩸 par jeton, et AUCUN Trésor différé.
    [Fact]
    public void Repos_merite_avec_jeton_gagne_du_sang_sans_tresor()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Repos Mérité")];
        t.Merlin.JetonsDernierSurvivant = 2;

        Suicide(t, t.Merlin);

        Assert.Equal(2, t.Merlin.Sang);        // +1 par jeton
        Assert.Empty(t.Ctx.EffetsDifferes);    // a des jetons → pas de Trésor différé
    }

    // Petit Ange (Passif) : à la pioche, arme +1 dé pour le prochain Jet de Créature du porteur.
    [Fact]
    public void Petit_ange_pioche_arme_le_bonus_de_des()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Petit Ange Parti Trop Tôt")];

        Suicide(t, t.Merlin);

        Assert.Equal(1, t.Merlin.BonusProchainJetCreature);
        Assert.Single(t.Merlin.SorciersCreves);
    }

    // Petit Ange : le bonus ajoute 1 dé au prochain Jet de Créature, puis est consommé.
    [Fact]
    public void Petit_ange_ajoute_un_de_au_prochain_jet_puis_se_consomme()
    {
        var t = new Table { ProchainDe = 5 };
        t.Merlin.BonusProchainJetCreature = 1;   // armé
        var creature = new CarteSort("Bête", TypeComposant.Destination, Glyphe.Arcane)
        {
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches = [new TrancheJetDePuissance { Seuil = 10, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(7) }] }],
                },
            ],
        };
        t.Ctx.SortEnCours = [creature];

        t.Ctx.ResoudreSort();

        // 1 Glyphe Arcane + 1 dé Petit Ange = 2 dés × 5 = 10 ≥ seuil → 7 dégâts (sans le bonus : 1 dé = 5 < 10).
        Assert.Equal(13, t.Gandalf.PointsDeVie);
        Assert.Equal(0, t.Merlin.BonusProchainJetCreature);   // consommé
    }
}
