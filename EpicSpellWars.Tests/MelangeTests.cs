using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Hook Melanger ([[pioche-non-melangee]]) : melange initial au setup de partie + remelange de la Defausse
// quand la pioche s'epuise. Le hook de Table est un Fisher-Yates SEEDE (fidele ET reproductible).
public class MelangeTests
{
    private static CarteSort Carte(string nom) => new(nom, TypeComposant.Qualite, Glyphe.Arcane);

    // Le melange seede brasse reellement l'ordre (≠ identite) tout en restant deterministe (rejouable).
    [Fact]
    public void Melanger_brasse_l_ordre_et_reste_deterministe()
    {
        var t = new Table();
        var pioche = Enumerable.Range(0, 20).Select(i => (Carte)Carte($"C{i}")).ToList();

        var melangee = t.Ctx.Melanger(pioche);

        Assert.Equal(20, melangee.Count);
        Assert.Equal([.. pioche.OrderBy(c => c.Nom)], [.. melangee.OrderBy(c => c.Nom)]);   // memes cartes
        Assert.NotEqual([.. pioche], [.. melangee]);                                        // ordre change (≠ identite)
    }

    // CompleterMain : pioche vide mais Defausse non vide → la Defausse est remelangee et devient la pioche.
    [Fact]
    public void CompleterMain_remelange_la_defausse_quand_la_pioche_est_vide()
    {
        var t = new Table();
        t.Ctx.PiochePrincipale = [];
        t.Ctx.Defausse = [Carte("D0"), Carte("D1"), Carte("D2")];

        t.Ctx.CompleterMain(t.Merlin, 8);

        Assert.Equal(3, t.Merlin.Main.Count);    // les 3 cartes de la Defausse remelangee sont piochees
        Assert.Empty(t.Ctx.Defausse);            // Defausse videe (versee dans la pioche)
        Assert.Empty(t.Ctx.PiochePrincipale);    // puis entierement piochee
    }

    // Pioche ET Defausse vides → CompleterMain ne tourne pas en boucle et ne pioche rien.
    [Fact]
    public void CompleterMain_s_arrete_si_pioche_et_defausse_vides()
    {
        var t = new Table();
        t.Ctx.PiochePrincipale = [];
        t.Ctx.Defausse = [];

        t.Ctx.CompleterMain(t.Merlin, 8);

        Assert.Empty(t.Merlin.Main);
    }

    // RevelerPiocheJusqua : pioche vide → remelange la Defausse pour y chercher le critere.
    [Fact]
    public void RevelerPioche_remelange_la_defausse()
    {
        var t = new Table();
        var creature = new CarteSort("Bête", TypeComposant.Destination, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [];
        t.Ctx.Defausse = [Carte("D0"), creature, Carte("D1")];

        var trouvee = t.Ctx.RevelerPiocheJusqua(c => c.Type == TypeComposant.Destination);

        Assert.Same(creature, trouvee);
    }

    // JouerPartie melange la pioche au setup (une fois). Avec une pioche letale homogene le vainqueur ne
    // change pas — on verifie surtout que le melange ne casse pas le flux de partie.
    [Fact]
    public void JouerPartie_melange_la_pioche_au_setup()
    {
        var t = new Table();
        var letale = new CarteSort("L", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.TousAdversaires, Valeur = new ValeurFixe(25) }] }],
        };
        t.Ctx.PiochePrincipale = [.. Enumerable.Range(0, 40).Select(_ => (Carte)new CarteSort("L", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = letale.Effets,
        })];
        t.Declaration = s => s == t.Merlin && s.Main.Count > 0 ? [s.Main[0]] : [];

        var champion = new OrdonnanceurDeTour().JouerPartie(t.Ctx);

        Assert.Same(t.Merlin, champion);
    }
}
