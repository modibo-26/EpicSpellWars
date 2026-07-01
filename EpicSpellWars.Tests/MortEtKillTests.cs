using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche A (mort & récompenses au kill) : le goulot InfligerDegats détecte la transition
// vivant→mort et déclenche OnMort. Tueur = Lanceur ; +3 Sang (sauf suicide), vol du Donjon, jeton
// Dernier Survivant quand il ne reste qu'un vivant.
public class MortEtKillTests
{
    private static Action Degats(Cible cible, int montant) =>
        new() { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) };

    // Kill : le tueur (Lanceur) gagne +3 🩸.
    [Fact]
    public void Tuer_un_adversaire_donne_3_sang_au_tueur()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.False(t.Gandalf.EstVivant);
        Assert.Equal(3, t.Merlin.Sang);   // +3 au kill
    }

    // Dégâts non létaux : aucune récompense.
    [Fact]
    public void Degats_non_letaux_ne_donnent_pas_de_sang()
    {
        var t = new Table();

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.Equal(15, t.Gandalf.PointsDeVie);
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Suicide (AutoDegats) : pas de +3 🩸 (règles-sang : pas de Sang si on se tue soi-même).
    [Fact]
    public void Se_suicider_ne_donne_pas_de_sang()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Vol du Donjon : tuer son contrôleur en transfère le contrôle au tueur.
    [Fact]
    public void Tuer_le_controleur_du_donjon_le_vole()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Gandalf;
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
    }

    // Tuer un sorcier qui ne contrôle PAS le Donjon ne change pas le contrôleur.
    [Fact]
    public void Tuer_un_non_controleur_ne_change_pas_le_donjon()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Saroumane;
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.Equal(t.Saroumane, t.Ctx.ControleurDonjon);
    }

    // Jeton Dernier Survivant : le dernier vivant après un kill l'emporte (et le tueur gagne aussi +3 🩸).
    [Fact]
    public void Dernier_survivant_apres_kill_gagne_un_jeton()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 0;   // déjà mort → ne reste que Merlin et Gandalf
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));

        Assert.Equal(1, t.Merlin.JetonsDernierSurvivant);
        Assert.Equal(3, t.Merlin.Sang);   // +3 au kill
    }

    // Jeton aussi si le dernier survivant le devient par le suicide d'un autre (sans +3 pour le suicidé).
    [Fact]
    public void Dernier_survivant_par_suicide_d_un_autre_gagne_un_jeton()
    {
        var t = new Table();
        t.Saroumane.PointsDeVie = 0;   // ne reste que Merlin et Gandalf
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Equal(1, t.Gandalf.JetonsDernierSurvivant);
        Assert.Equal(0, t.Merlin.Sang);   // suicide → pas de Sang
    }

    // Nettoyage à la mort (rulebook p.10/11) : un mort défausse sa main + ses Créatures, et rend ses Trésors
    // sous leur pile. Il ne garde que ses crevés (+ Sang/jetons). Fait dès OnMort, pas seulement au FinManche.
    [Fact]
    public void Un_mort_defausse_main_creatures_et_rend_ses_tresors()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 3;
        var carteMain = new CarteSort("M", TypeComposant.Source, Glyphe.Arcane);
        var creature = new CarteSort("Bête", TypeComposant.Destination, Glyphe.Arcane);
        var tresor = new Tresor("T", [], TriggerType.Passif);
        t.Gandalf.Main = [carteMain];
        t.Gandalf.Creatures = [creature];
        t.Gandalf.Tresors = [tresor];

        t.Ctx.Appliquer(Degats(Cible.AdversaireGauche, 5));   // Merlin tue Gandalf

        Assert.False(t.Gandalf.EstVivant);
        Assert.Empty(t.Gandalf.Main);
        Assert.Empty(t.Gandalf.Creatures);        // invariant pour Q3 : un mort n'a plus de Créatures
        Assert.Empty(t.Gandalf.Tresors);
        Assert.Contains(carteMain, t.Ctx.Defausse);
        Assert.Contains(creature, t.Ctx.Defausse);
        Assert.Contains(tresor, t.Ctx.PiocheTresor);   // Trésor remis sous sa pile (comme FinManche)
    }

    // Le nettoyage vaut aussi pour un suicide (la mort est confirmée quel que soit le tueur).
    [Fact]
    public void Un_suicide_defausse_aussi_les_biens_du_mort()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 2;
        var tresor = new Tresor("T", [], TriggerType.Passif);
        t.Merlin.Main = [new CarteSort("M", TypeComposant.Source, Glyphe.Arcane)];
        t.Merlin.Tresors = [tresor];

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.False(t.Merlin.EstVivant);
        Assert.Empty(t.Merlin.Main);
        Assert.Empty(t.Merlin.Tresors);
        Assert.Contains(tresor, t.Ctx.PiocheTresor);
    }
}
