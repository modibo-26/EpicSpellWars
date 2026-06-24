using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors « modif de Glyphes » : Buffet à Volonté (un Composant glissé sous le Trésor : son Glyphe compte
// dans chaque sort → CompterGlyphes) et Fusil à Triple Canon (+1 🩸 par série de 3 Glyphes identiques dans le sort).
public class ModifGlyphesTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static CarteSort Source(string nom, Glyphe glyphe) => new(nom, TypeComposant.Source, glyphe);

    private static CarteSort CreatureJet(Glyphe glyphe, int seuil, params Action[] actions) =>
        new("Bête", TypeComposant.Destination, glyphe)
        {
            Effets = [new EffetJetDePuissance { Glyphe = glyphe, Tranches = [new TrancheJetDePuissance { Seuil = seuil, Actions = [.. actions] }] }],
        };

    // Buffet à Volonté (Immediat) : glisse un Composant de la main sous le Trésor ; son Glyphe compte ensuite.
    [Fact]
    public void Buffet_a_volonte_glisse_une_carte_dont_le_glyphe_compte()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [Tresor("Buffet à Volonté")];
        var carte = Source("Tenebreux", Glyphe.Tenebres);
        t.Merlin.Main = [carte];

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });

        Assert.Contains(carte, t.Merlin.SousBuffet);                  // glissée sous le Trésor
        Assert.Empty(t.Merlin.Main);                                  // retirée de la main
        Assert.Equal(1, t.Ctx.CompterGlyphes(Glyphe.Tenebres));      // compte dans les sorts
    }

    // Buffet à Volonté : la carte glissée ajoute un dé au Jet de puissance du même Glyphe.
    [Fact]
    public void Buffet_a_volonte_ajoute_un_de_au_jet_du_meme_glyphe()
    {
        var t = new Table { ProchainDe = 5 };
        t.Merlin.SousBuffet.Add(Source("A", Glyphe.Arcane));   // déjà glissée
        t.Ctx.SortEnCours = [CreatureJet(Glyphe.Arcane, 10, new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) })];

        t.Ctx.ResoudreSort();

        // 1 Glyphe Arcane (Créature) + 1 (Buffet) = 2 dés × 5 = 10 ≥ seuil → 4 dégâts.
        Assert.Equal(16, t.Gandalf.PointsDeVie);
    }

    // Fusil à Triple Canon : +1 🩸 par série complète de 3 Glyphes identiques.
    [Fact]
    public void Fusil_a_triple_canon_donne_un_sang_par_serie_de_trois()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Fusil à Triple Canon"));
        t.Ctx.SortEnCours =
        [
            Source("a1", Glyphe.Arcane), Source("a2", Glyphe.Arcane), Source("a3", Glyphe.Arcane),
            Source("t1", Glyphe.Tenebres), Source("t2", Glyphe.Tenebres), Source("t3", Glyphe.Tenebres),
        ];

        t.Ctx.ResoudreSort();

        Assert.Equal(2, t.Merlin.Sang);   // 2 séries complètes (3 Arcane + 3 Ténèbres)
    }

    // Fusil à Triple Canon : les séries incomplètes (< 3) ne comptent pas.
    [Fact]
    public void Fusil_a_triple_canon_ignore_les_series_incompletes()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Fusil à Triple Canon"));
        t.Ctx.SortEnCours =
        [
            Source("a1", Glyphe.Arcane), Source("a2", Glyphe.Arcane), Source("a3", Glyphe.Arcane),
            Source("t1", Glyphe.Tenebres), Source("t2", Glyphe.Tenebres),
        ];

        t.Ctx.ResoudreSort();

        Assert.Equal(1, t.Merlin.Sang);   // 3 Arcane = 1 série ; 2 Ténèbres = 0
    }
}
