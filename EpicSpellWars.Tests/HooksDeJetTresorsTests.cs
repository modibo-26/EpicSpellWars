using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors « hooks de Jet de puissance » : Chipodada (+1 🩸 si résultat ≥ 13) et Granoloup (+1 aux dégâts
// d'un Jet d'une Créature). Branchés dans EffetJetDePuissance (ApresLancer / AvantActions), pas pour un
// simple branchement-par-dé.
public class HooksDeJetTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static CarteSort Glyphe2(string nom) => new(nom, TypeComposant.Source, Glyphe.Arcane);

    private static CarteSort CreatureJet(int seuil, params Action[] actions) =>
        new("Bête", TypeComposant.Destination, Glyphe.Arcane)
        {
            Effets = [new EffetJetDePuissance { Glyphe = Glyphe.Arcane, Tranches = [new TrancheJetDePuissance { Seuil = seuil, Actions = [.. actions] }] }],
        };

    // Chipodada : un Jet de puissance dont le résultat atteint 13 fait gagner 1 🩸.
    [Fact]
    public void Chipodada_jet_treize_ou_plus_donne_un_sang()
    {
        var t = new Table { ProchainDe = 5 };   // 3 Glyphes Arcane → 3 dés × 5 = 15 ≥ 13
        t.Merlin.Tresors.Add(Tresor("Chipodada"));
        t.Ctx.SortEnCours = [CreatureJet(1), Glyphe2("A1"), Glyphe2("A2")];

        t.Ctx.ResoudreSort();

        Assert.Equal(1, t.Merlin.Sang);
    }

    // Chipodada : un résultat sous 13 ne donne rien.
    [Fact]
    public void Chipodada_jet_sous_treize_ne_donne_rien()
    {
        var t = new Table { ProchainDe = 4 };   // 3 × 4 = 12 < 13
        t.Merlin.Tresors.Add(Tresor("Chipodada"));
        t.Ctx.SortEnCours = [CreatureJet(1), Glyphe2("A1"), Glyphe2("A2")];

        t.Ctx.ResoudreSort();

        Assert.Equal(0, t.Merlin.Sang);
    }

    // Granoloup : les dégâts infligés par le Jet d'une Créature sont augmentés de 1.
    [Fact]
    public void Granoloup_augmente_les_degats_du_jet_de_un()
    {
        var t = new Table { ProchainDe = 5 };
        t.Merlin.Tresors.Add(Tresor("Granoloup"));
        t.Ctx.SortEnCours = [CreatureJet(1, new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) })];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // 3 + 1
    }

    // Sans Granoloup, les dégâts du Jet ne sont pas augmentés.
    [Fact]
    public void Sans_granoloup_les_degats_du_jet_restent_normaux()
    {
        var t = new Table { ProchainDe = 5 };
        t.Ctx.SortEnCours = [CreatureJet(1, new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) })];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);   // 3
    }

    // Granoloup ne s'applique qu'aux dégâts d'un JET : les dégâts hors jet (une Source) ne sont pas augmentés.
    [Fact]
    public void Granoloup_n_augmente_pas_les_degats_hors_jet()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Granoloup"));
        t.Ctx.SortEnCours =
        [
            new CarteSort("S", TypeComposant.Source, Glyphe.Arcane)
            {
                Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }] }],
            },
        ];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);   // 3 (BonusDegatsJet = 0 hors jet)
    }
}
