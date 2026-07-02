using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Départage même-type (rulebook p.8) : quand plusieurs Composants NON résolus ont le même type (via cartes
// ajoutées en cours de sort), le lanceur choisit l'ordre de résolution (hook ChoisirComposant).
public class DepartageMemeTypeTests
{
    // Source qui marque son passage en poussant son nom dans une liste partagée (observe l'ordre de résolution).
    private static CarteSort Marqueur(string nom, List<string> journal) =>
        new(nom, TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetInline(_ => journal.Add(nom))],
        };

    // Deux Sources ex aequo : le lanceur choisit de résoudre B avant A (≠ ordre de la liste [A, B]).
    [Fact]
    public void Le_lanceur_ordonne_deux_composants_du_meme_type()
    {
        var journal = new List<string>();
        var t = new Table();
        var a = Marqueur("A", journal);
        var b = Marqueur("B", journal);
        t.Ctx.SortEnCours = [a, b];
        // À rang égal, le lanceur préfère B d'abord.
        t.ChoixComposant = (_, cs) => cs.First(c => c.Nom == "B");

        t.Ctx.ResoudreSort();

        Assert.Equal(["B", "A"], journal);   // ordre choisi par le lanceur, pas l'ordre de la liste
    }

    // Par défaut (pas de préférence), l'ordre reste celui de SortEnCours.
    [Fact]
    public void Par_defaut_l_ordre_est_celui_de_la_liste()
    {
        var journal = new List<string>();
        var t = new Table();
        t.Ctx.SortEnCours = [Marqueur("A", journal), Marqueur("B", journal)];

        t.Ctx.ResoudreSort();

        Assert.Equal(["A", "B"], journal);
    }
}

// Effet de test minimal : exécute une action arbitraire (journalisation) à la résolution.
file sealed class EffetInline(System.Action<GameContext> action) : EpicSpellWars.Domain.Interfaces.IEffet
{
    public void Execute(GameContext context) => action(context);
}
