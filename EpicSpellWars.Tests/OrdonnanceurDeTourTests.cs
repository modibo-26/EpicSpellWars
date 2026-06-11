using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Ordonnanceur de tour (tranche fine) : ordre de résolution, nettoyage fin de sort, flags ADejaJoue.
public class OrdonnanceurDeTourTests
{
    // 14) Deux sorts de tailles différentes : Merlin (1 composant) joue AVANT Gandalf (2 composants).
    //     Nettoyage : composants non gardés → Défausse ; Créature gardée conservée.
    [Fact]
    public void Resout_les_sorts_du_plus_petit_au_plus_grand_et_nettoie()
    {
        var t = new Table { ProchainDe = 10 };

        var mSource = new CarteSort("M-Source", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }] }],
        };
        var gSource = new CarteSort("G-Source", TypeComposant.Source, Glyphe.Arcane);
        var gCreature = new CarteSort("G-Créature", TypeComposant.Destination, Glyphe.Arcane, initiative: 5)
        {
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches = [new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) }] }],
                },
            ],
        };

        var ordre = new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [mSource],
            [t.Gandalf] = [gSource, gCreature],
        });

        Assert.Equal([t.Merlin, t.Gandalf], ordre);          // moins de composants d'abord
        Assert.Equal(17, t.Gandalf.PointsDeVie);             // Merlin inflige 3
        Assert.Equal(16, t.Saroumane.PointsDeVie);           // Créature de Gandalf inflige 4 à sa gauche
        Assert.Contains(mSource, t.Ctx.Defausse);            // composant non gardé
        Assert.Contains(gSource, t.Ctx.Defausse);
        Assert.Contains(gCreature, t.Gandalf.Creatures);     // GARDEZ : pas défaussée
        Assert.DoesNotContain(gCreature, t.Ctx.Defausse);
        Assert.True(t.Merlin.ADejaJoueCeTour);
        Assert.True(t.Gandalf.ADejaJoueCeTour);
        Assert.False(t.Saroumane.ADejaJoueCeTour);
    }
}
