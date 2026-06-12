using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Étape 1 (manques résolveur — Cibles) : ATresor/SansTresor (ensembles), AutresAdversaires (tous sauf la
// dernière cible) et CibleUnique (« Adversaire qui... » au singulier = un seul match du filtre via ChoisirCible).
public class CiblesFiltrantesTests
{
    // ATresor = chaque adversaire qui possède >= 1 Trésor (ensemble).
    [Fact]
    public void ATresor_touche_seulement_les_possesseurs()
    {
        var t = new Table();
        t.Gandalf.Tresors.Add(new Tresor("Babiole", [], TriggerType.Passif));

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.ATresor, Valeur = new ValeurFixe(2) });

        Assert.Equal(18, t.Gandalf.PointsDeVie);     // a un Trésor → touché
        Assert.Equal(20, t.Saroumane.PointsDeVie);   // pas de Trésor → épargné
    }

    // SansTresor = chaque adversaire sans aucun Trésor (ensemble), complément de ATresor.
    [Fact]
    public void SansTresor_touche_seulement_les_demunis()
    {
        var t = new Table();
        t.Gandalf.Tresors.Add(new Tresor("Babiole", [], TriggerType.Passif));

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.SansTresor, Valeur = new ValeurFixe(2) });

        Assert.Equal(20, t.Gandalf.PointsDeVie);     // a un Trésor → épargné
        Assert.Equal(18, t.Saroumane.PointsDeVie);   // sans Trésor → touché
    }

    // AutresAdversaires = tous les adversaires SAUF la dernière cible affectée (≠ TousAdversaires qui l'inclut).
    [Fact]
    public void AutresAdversaires_exclut_la_derniere_cible()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 18;

        // 1re action : frappe Gandalf → DerniereCible = Gandalf.
        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(1) });
        // 2e : soigne chaque AUTRE adversaire → Saroumane seulement.
        t.Ctx.Appliquer(new Action { Type = TypeAction.Soin, Cible = Cible.AutresAdversaires, Valeur = new ValeurFixe(1) });

        Assert.Equal(19, t.Gandalf.PointsDeVie);     // cible → pas soigné
        Assert.Equal(19, t.Saroumane.PointsDeVie);   // 18 + 1 → autre adversaire soigné
        Assert.Equal(20, t.Merlin.PointsDeVie);      // lanceur jamais visé
    }

    // CibleUnique : deux adversaires matchent le filtre, le lanceur n'en frappe qu'UN (ChoisirCible = 1er).
    [Fact]
    public void CibleUnique_reduit_le_filtre_a_un_seul_match()
    {
        var t = new Table();
        t.Gandalf.ADejaJoueCeTour = true;
        t.Saroumane.ADejaJoueCeTour = true;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, CibleUnique = true, Valeur = new ValeurFixe(3) });

        Assert.Equal(17, t.Gandalf.PointsDeVie);     // choisi (1er candidat)
        Assert.Equal(20, t.Saroumane.PointsDeVie);   // épargné : une seule cible
    }

    // Sans le flag, la même filtrante touche TOUT l'ensemble (« chaque adversaire qui... »).
    [Fact]
    public void Sans_CibleUnique_la_filtrante_touche_tout_lensemble()
    {
        var t = new Table();
        t.Gandalf.ADejaJoueCeTour = true;
        t.Saroumane.ADejaJoueCeTour = true;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.ADejaJoue, Valeur = new ValeurFixe(3) });

        Assert.Equal(17, t.Gandalf.PointsDeVie);
        Assert.Equal(17, t.Saroumane.PointsDeVie);
    }
}
