using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Q2 — sacrifice anti-dégâts (rulebook p.9) : quand un défenseur va subir des dégâts d'un adversaire, il peut
// défausser une Créature gardée pour absorber l'instance ENTIÈRE (1 Créature = 1 instance). Décision via le hook
// ChoisirSacrificeCreature dans le goulot InfligerDegats.
public class CreaturesSacrificeTests
{
    private static Action Degats(Cible cible, int montant) =>
        new() { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) };

    private static CarteSort Bete(string nom = "Bête") => new(nom, TypeComposant.Destination, Glyphe.Arcane);

    // Sacrifice : la Créature absorbe toute l'instance → 0 PV perdu, Créature défaussée, signal armé.
    [Fact]
    public void Sacrifier_une_creature_absorbe_toute_l_instance()
    {
        var t = new Table();
        var bete = Bete();
        t.Gandalf.Creatures = [bete];
        t.SacrificeCreature = (_, _, cs) => cs[0];   // Gandalf sacrifie sa Créature

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));   // Merlin frappe Gandalf

        Assert.Equal(20, t.Gandalf.PointsDeVie);          // aucun dégât encaissé
        Assert.Empty(t.Gandalf.Creatures);                // Créature consommée
        Assert.Contains(bete, t.Ctx.Defausse);
        Assert.True(t.Ctx.DerniereInstanceBloquee);
    }

    // Sans sacrifice (hook renvoie null) : les dégâts passent normalement.
    [Fact]
    public void Ne_pas_sacrifier_laisse_passer_les_degats()
    {
        var t = new Table();
        t.Gandalf.Creatures = [Bete()];
        // SacrificeCreature par défaut = null (n'encaisse pas)

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.Equal(15, t.Gandalf.PointsDeVie);
        Assert.Single(t.Gandalf.Creatures);
        Assert.False(t.Ctx.DerniereInstanceBloquee);
    }

    // 1 Créature ne bloque qu'UNE instance : deux actions de dégâts distinctes → seule la 1re est absorbée.
    [Fact]
    public void Une_creature_ne_bloque_qu_une_instance()
    {
        var t = new Table();
        t.Gandalf.Creatures = [Bete()];
        // Sacrifie tant qu'il reste une Créature (le hook reçoit la liste courante).
        t.SacrificeCreature = (_, _, cs) => cs.Count > 0 ? cs[0] : null;

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 4));   // 1re instance : bloquée
        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 4));   // 2e instance : plus de Créature → encaissée

        Assert.Empty(t.Gandalf.Creatures);
        Assert.Equal(16, t.Gandalf.PointsDeVie);   // seule la 2e instance (4) est passée
    }

    // Le sacrifice peut empêcher un kill : la mort n'a pas lieu, donc aucune récompense au tueur.
    [Fact]
    public void Sacrifier_empeche_le_kill()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 3;
        t.Gandalf.Creatures = [Bete()];
        t.SacrificeCreature = (_, _, cs) => cs[0];

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));   // aurait tué Gandalf

        Assert.True(t.Gandalf.EstVivant);
        Assert.Equal(3, t.Gandalf.PointsDeVie);
        Assert.Equal(0, t.Merlin.Sang);   // pas de kill → pas de +3
    }

    // Pas de sacrifice sur l'auto-dégât (la cible est le Lanceur lui-même, pas un adversaire).
    [Fact]
    public void Pas_de_sacrifice_sur_auto_degats()
    {
        var t = new Table();
        t.Merlin.Creatures = [Bete()];
        t.SacrificeCreature = (_, _, cs) => cs.Count > 0 ? cs[0] : null;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(15, t.Merlin.PointsDeVie);   // dégâts subis
        Assert.Single(t.Merlin.Creatures);        // Créature intacte (pas de sacrifice contre soi-même)
    }
}
