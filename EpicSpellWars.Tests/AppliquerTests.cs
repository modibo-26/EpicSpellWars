using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Resolveur d'actions (GameContext.Appliquer) : effets de base, valeurs, et manipulation de main (groupes A/B).
public class AppliquerTests
{
    // 1) Taléboulas — 3 dégâts à l'adversaire de gauche.
    [Fact]
    public void Degats_cible_adversaire_gauche()
    {
        var t = new Table();
        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) });

        Assert.Equal(17, t.Gandalf.PointsDeVie);
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // 2) Flaminus — 1 dégât à chaque adversaire.
    [Fact]
    public void Degats_tous_adversaires()
    {
        var t = new Table();
        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(1) });

        Assert.Equal(19, t.Gandalf.PointsDeVie);
        Assert.Equal(19, t.Saroumane.PointsDeVie);
        Assert.Equal(20, t.Merlin.PointsDeVie);
    }

    // 3) Pèredodux — soin de 1 PV par Créature en jeu (ValeurParCreature).
    [Fact]
    public void Soin_par_creature()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 18;
        t.Merlin.Creatures.Add(new CarteSort("Familier 1", TypeComposant.Destination, Glyphe.Primaire));
        t.Merlin.Creatures.Add(new CarteSort("Familier 2", TypeComposant.Destination, Glyphe.Primaire));

        t.Ctx.Appliquer(new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurParCreature(1) });

        Assert.Equal(20, t.Merlin.PointsDeVie);
    }

    // 4) Cubengelus — Jet de puissance Arcane (1 dé forcé à 10 → tranche 10+) : 4 dégâts gauche + Trésor + GARDEZ.
    [Fact]
    public void JetDePuissance_tranche_haute_degats_tresor_et_gardez()
    {
        var t = new Table { ProchainDe = 10 };
        var cubengelus = new CarteSort("Cubengelus", TypeComposant.Destination, Glyphe.Arcane, initiative: 12);
        t.Ctx.SortEnCours = [cubengelus];          // 1 carte Arcane → 1 dé
        t.Ctx.CreatureEnCours = cubengelus;
        t.Ctx.PiocheTresor = [new Tresor("Trésor démo", [], TriggerType.Passif)];

        new EffetJetDePuissance
        {
            Glyphe = Glyphe.Arcane,
            Tranches =
            [
                new Tranche { Seuil = 1, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) }] },
                new Tranche { Seuil = 5, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }] },
                new TrancheJetDePuissance
                {
                    Seuil = 10, PeutGarder = true,
                    Actions =
                    [
                        new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) },
                        new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi },
                    ],
                },
            ],
        }.Execute(t.Ctx);

        Assert.Equal(16, t.Gandalf.PointsDeVie);            // 4 dégâts
        Assert.Single(t.Merlin.Tresors);                    // 1 Trésor gagné
        Assert.Contains(cubengelus, t.Merlin.Creatures);    // GARDEZ
    }

    // 5) Necrophilus — Prenez le Donjon, puis 2 dégâts par jeton Dernier Survivant en jeu à chaque adversaire.
    [Fact]
    public void PrendreDonjon_puis_degats_par_jeton()
    {
        var t = new Table();
        t.Saroumane.JetonsDernierSurvivant = 1;   // 1 jeton en jeu

        t.Ctx.Appliquer(new Action { Type = TypeAction.PrendreDonjon, Cible = Cible.Soi });
        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurParJeton(2) });

        Assert.Same(t.Merlin, t.Ctx.ControleurDonjon);
        Assert.Equal(18, t.Gandalf.PointsDeVie);
        Assert.Equal(18, t.Saroumane.PointsDeVie);
    }

    // 6) Vishnakrax — ajoute 1 Créature de la main au sort (GagnerCarte, groupe A).
    [Fact]
    public void GagnerCarte_deplace_la_creature_vers_le_sort()
    {
        var t = new Table();
        var bestiole = new CarteSort("Bestiole", TypeComposant.Destination, Glyphe.Tenebres);
        var etincelle = new CarteSort("Étincelle", TypeComposant.Source, Glyphe.Elementaire);
        t.Merlin.Main.AddRange([bestiole, etincelle]);

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.EstCreature });

        Assert.Contains(bestiole, t.Ctx.SortEnCours);
        Assert.Equal([etincelle], t.Merlin.Main);   // seule la Source reste en main
    }

    // 7) Sarabandus — défausse jusqu'à 3 cartes non Primaires, soin = 1 PV / carte (ValeurQuantiteChoisie).
    [Fact]
    public void DefausserCartes_alimente_la_quantite_choisie()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 10;
        var brume = new CarteSort("Brume", TypeComposant.Qualite, Glyphe.Illusion);
        var foudre = new CarteSort("Foudre", TypeComposant.Source, Glyphe.Elementaire);
        var caillou = new CarteSort("Caillou", TypeComposant.Source, Glyphe.Primaire);
        t.Merlin.Main.AddRange([brume, foudre, caillou]);

        new EffetSimple
        {
            Actions =
            [
                new Action { Type = TypeAction.DefausserCartes, Cible = Cible.Soi, Valeur = new ValeurFixe(3), FiltreCarte = c => c.Glyphe != Glyphe.Primaire },
                new Action { Type = TypeAction.Soin, Cible = Cible.Soi, Valeur = new ValeurQuantiteChoisie(1) },
            ],
        }.Execute(t.Ctx);

        Assert.Equal(2, t.Ctx.DerniereQuantite);
        Assert.Equal(12, t.Merlin.PointsDeVie);       // 10 + 2 cartes défaussées
        Assert.Equal([caillou], t.Merlin.Main);       // la Primaire reste
        Assert.Contains(brume, t.Ctx.Defausse);
        Assert.Contains(foudre, t.Ctx.Defausse);
    }
}
