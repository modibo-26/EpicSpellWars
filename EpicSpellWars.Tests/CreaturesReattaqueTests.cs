using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Q3 — réattaque forcée des Créatures gardées (rulebook p.10 : « vous serez obligé »). À chaque tour de son
// contrôleur, une Créature gardée d'un tour précédent refait son Jet de puissance APRÈS le sort déclaré ;
// si le résultat n'est pas GARDEZ, elle est défaussée en fin de tour.
public class CreaturesReattaqueTests
{
    // Créature (Destination) avec un Jet à une seule tranche (Seuil 1 → couvre tout tirage) : `garde` = GARDEZ,
    // `degats` infligés à l'adversaire de gauche.
    private static CarteSort Creature(string nom, bool garde, int degats) =>
        new(nom, TypeComposant.Destination, Glyphe.Arcane)
        {
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches =
                    [
                        new TrancheJetDePuissance
                        {
                            Seuil = 1, PeutGarder = garde,
                            Actions = degats > 0 ? [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(degats) }] : [],
                        },
                    ],
                },
            ],
        };

    // Source neutre (sans effet) : sert juste à ce que le lanceur ait un sort déclaré (donc soit dans l'ordre du tour).
    private static CarteSort SortNeutre() => new("Neutre", TypeComposant.Source, Glyphe.Tenebres);

    private static Dictionary<Sorcier, List<CarteSort>> Sorts(Sorcier s, params CarteSort[] c) => new() { [s] = [.. c] };

    // GARDEZ en réattaque : la Créature reste en jeu ET réinflige ses dégâts.
    [Fact]
    public void Creature_gardee_reattaque_et_reste_si_gardez()
    {
        var t = new Table { ProchainDe = 6 };
        var bete = Creature("Bête", garde: true, degats: 4);
        t.Merlin.Creatures = [bete];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sorts(t.Merlin, SortNeutre()));

        Assert.Contains(bete, t.Merlin.Creatures);   // GARDEZ → toujours là
        Assert.Equal(16, t.Gandalf.PointsDeVie);     // réattaque : 4 dégâts à gauche
    }

    // Pas de GARDEZ : la Créature réattaque (inflige quand même ses dégâts) puis est défaussée en fin de tour.
    [Fact]
    public void Creature_gardee_defaussee_si_pas_gardez()
    {
        var t = new Table { ProchainDe = 6 };
        var bete = Creature("Bête", garde: false, degats: 3);
        t.Merlin.Creatures = [bete];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sorts(t.Merlin, SortNeutre()));

        Assert.DoesNotContain(bete, t.Merlin.Creatures);   // pas de GARDEZ → retirée
        Assert.Contains(bete, t.Ctx.Defausse);             // défaussée
        Assert.Equal(17, t.Gandalf.PointsDeVie);           // a quand même frappé (3)
    }

    // Une Créature qui entre en jeu (GARDEZ) CE tour ne réattaque PAS le même tour : ses dégâts ne comptent qu'une fois.
    [Fact]
    public void Creature_gardee_ce_tour_ne_reattaque_pas_le_meme_tour()
    {
        var t = new Table { ProchainDe = 6 };
        var bete = Creature("Bête", garde: true, degats: 5);   // jouée comme Destination ce tour

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sorts(t.Merlin, bete));

        Assert.Contains(bete, t.Merlin.Creatures);   // gardée ce tour
        Assert.Equal(15, t.Gandalf.PointsDeVie);     // 5 dégâts UNE seule fois (pas 10)
    }

    // Plusieurs Créatures gardées : chacune refait son propre Jet.
    [Fact]
    public void Chaque_creature_gardee_refait_son_jet()
    {
        var t = new Table { ProchainDe = 6 };
        var a = Creature("A", garde: true, degats: 2);
        var b = Creature("B", garde: true, degats: 3);
        t.Merlin.Creatures = [a, b];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sorts(t.Merlin, SortNeutre()));

        Assert.Contains(a, t.Merlin.Creatures);
        Assert.Contains(b, t.Merlin.Creatures);
        Assert.Equal(15, t.Gandalf.PointsDeVie);   // 2 + 3 = 5 dégâts à gauche
    }

    // Un kill en réattaque donne les récompenses (le tueur = contrôleur de la Créature gagne +3 🩸).
    [Fact]
    public void Kill_en_reattaque_donne_les_recompenses()
    {
        var t = new Table { ProchainDe = 6 };
        t.Gandalf.PointsDeVie = 3;
        var bete = Creature("Bête", garde: true, degats: 5);   // létal sur Gandalf
        t.Merlin.Creatures = [bete];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sorts(t.Merlin, SortNeutre()));

        Assert.False(t.Gandalf.EstVivant);
        Assert.Equal(3, t.Merlin.Sang);   // +3 au kill via OnMort
    }
}
