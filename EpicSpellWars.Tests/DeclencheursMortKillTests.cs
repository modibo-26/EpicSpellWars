using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Trésors déclenchés par des ÉVÉNEMENTS (mort/kill/pioche de crevé), branchés au goulot (OnMort /
// PiocherSorcierCreve), HORS pipeline de phases : Coupe du Tocard (passif), Avis de Recherche, Bouclier Anti-Fiente.
public class DeclencheursMortKillTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static Tresor Butin() => new("Butin", [], TriggerType.Passif);

    // Coupe du Tocard : si la mort du porteur met fin à la manche (≤ 1 survivant), il gagne 3 🩸.
    [Fact]
    public void Coupe_du_tocard_mort_qui_finit_la_manche_donne_trois()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 0;   // déjà mort → la mort de Merlin laissera 1 seul survivant (Gandalf)
        t.Merlin.PointsDeVie = 3;
        t.Merlin.Tresors.Add(Tresor("Coupe du Tocard"));
        t.Ctx.Lanceur = t.Gandalf;     // Gandalf tue Merlin

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(3, t.Merlin.Sang);   // +3 (le Sang persiste après la mort)
    }

    // Coupe du Tocard : une mort qui NE met PAS fin à la manche (≥ 2 survivants) ne donne rien.
    [Fact]
    public void Coupe_du_tocard_mort_sans_fin_de_manche_ne_donne_rien()
    {
        var t = new Table();   // Saroumane vivant → 2 survivants après la mort de Merlin
        t.Merlin.PointsDeVie = 3;
        t.Merlin.Tresors.Add(Tresor("Coupe du Tocard"));
        t.Ctx.Lanceur = t.Gandalf;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Avis de Recherche (Immediat) : place une prime + gagne un autre Trésor.
    [Fact]
    public void Avis_de_recherche_immediat_place_une_prime_et_gagne_un_tresor()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [Tresor("Avis de Recherche"), Butin()];

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });

        Assert.Equal(1, t.Ctx.PrimesEnJeu);
        Assert.Equal(2, t.Merlin.Tresors.Count);   // Avis (gardé) + Butin gagné
    }

    // Avis de Recherche : le prochain tueur gagne 3 🩸 EN PLUS des +3 du kill, et la prime est consommée.
    [Fact]
    public void Avis_de_recherche_prime_le_prochain_kill()
    {
        var t = new Table();
        t.Ctx.PrimesEnJeu = 1;
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(5) });

        Assert.False(t.Gandalf.EstVivant);
        Assert.Equal(6, t.Merlin.Sang);    // 3 (kill) + 3 (prime)
        Assert.Equal(0, t.Ctx.PrimesEnJeu);
    }

    // Bouclier Anti-Fiente : un adversaire pioche un crevé, le porteur lance 5 ou 6 → soin 1 PV.
    [Fact]
    public void Bouclier_anti_fiente_soigne_sur_pioche_creve_adverse()
    {
        var t = new Table { ProchainDe = 5 };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Tresors.Add(Tresor("Bouclier Anti-Fiente"));
        t.Ctx.PiocheSorcierCreve = [new SorcierCreve("X", [], TriggerType.Passif)];

        t.Ctx.PiocherSorcierCreve(t.Gandalf);

        Assert.Equal(11, t.Merlin.PointsDeVie);
    }

    // Bouclier Anti-Fiente : dé < 5 → pas de soin.
    [Fact]
    public void Bouclier_anti_fiente_petit_de_ne_soigne_pas()
    {
        var t = new Table { ProchainDe = 4 };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Tresors.Add(Tresor("Bouclier Anti-Fiente"));
        t.Ctx.PiocheSorcierCreve = [new SorcierCreve("X", [], TriggerType.Passif)];

        t.Ctx.PiocherSorcierCreve(t.Gandalf);

        Assert.Equal(10, t.Merlin.PointsDeVie);
    }

    // Bouclier Anti-Fiente : ne se déclenche pas quand c'est le PORTEUR lui-même qui pioche (pas un adversaire).
    [Fact]
    public void Bouclier_anti_fiente_pas_sur_sa_propre_pioche()
    {
        var t = new Table { ProchainDe = 6 };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Tresors.Add(Tresor("Bouclier Anti-Fiente"));
        t.Ctx.PiocheSorcierCreve = [new SorcierCreve("X", [], TriggerType.Passif)];

        t.Ctx.PiocherSorcierCreve(t.Merlin);

        Assert.Equal(10, t.Merlin.PointsDeVie);
    }
}
