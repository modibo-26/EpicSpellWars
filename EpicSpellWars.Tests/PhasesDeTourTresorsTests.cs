using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pipeline de phases de tour (#1bis) : clauses FinTour / DebutTour des Trésors (Menottes d'Avarice, Smoking
// de Location) + passif Donjon fin de tour (Chalisman). DebutTour est déjà couvert par Braguette (TresorsTests).
public class PhasesDeTourTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static Tresor Butin() => new("Butin", [], TriggerType.Passif);
    private static CarteSort Simple(string nom, TypeComposant type) => new(nom, type, Glyphe.Arcane);

    // Menottes d'Avarice : le DERNIER à jouer dans l'ordre du tour gagne un Trésor (clause FinTour).
    [Fact]
    public void Menottes_d_avarice_dernier_a_jouer_gagne_un_tresor()
    {
        var t = new Table();
        t.Gandalf.Tresors.Add(Tresor("Menottes d'Avarice"));
        t.Ctx.PiocheTresor = [Butin()];

        // Merlin (1 composant) joue AVANT Gandalf (2 composants) → Gandalf est le dernier.
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
            [t.Gandalf] = [Simple("G1", TypeComposant.Source), Simple("G2", TypeComposant.Qualite)],
        });

        Assert.Equal(2, t.Gandalf.Tresors.Count);   // Menottes + Butin gagné
    }

    // Menottes d'Avarice : si le porteur n'est PAS le dernier à jouer, aucun gain.
    [Fact]
    public void Menottes_d_avarice_pas_dernier_ne_gagne_rien()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Menottes d'Avarice"));   // Merlin (1 comp) joue en premier
        t.Ctx.PiocheTresor = [Butin()];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
            [t.Gandalf] = [Simple("G1", TypeComposant.Source), Simple("G2", TypeComposant.Qualite)],
        });

        Assert.Single(t.Merlin.Tresors);   // garde juste Menottes
    }

    // Smoking de Location : le sorcier le plus faible en fin de tour se soigne de 2 PV et gagne un Trésor.
    [Fact]
    public void Smoking_de_location_plus_faible_soigne_et_gagne_tresor()
    {
        var t = new Table();
        t.Merlin.PointsDeVie = 5;   // le plus faible
        t.Merlin.Tresors.Add(Tresor("Smoking de Location"));
        t.Ctx.PiocheTresor = [Butin()];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
        });

        Assert.Equal(7, t.Merlin.PointsDeVie);     // +2
        Assert.Equal(2, t.Merlin.Tresors.Count);   // Smoking + Butin
    }

    // Smoking de Location : si le porteur n'est pas le plus faible, rien.
    [Fact]
    public void Smoking_de_location_pas_plus_faible_ne_fait_rien()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 3;   // Gandalf plus faible que Merlin (20)
        t.Merlin.Tresors.Add(Tresor("Smoking de Location"));
        t.Ctx.PiocheTresor = [Butin()];

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
        });

        Assert.Equal(20, t.Merlin.PointsDeVie);
        Assert.Single(t.Merlin.Tresors);
    }

    // Chalisman : le contrôleur du Donjon gagne 1 🩸 SUPPLÉMENTAIRE en fin de tour (2 au lieu de 1).
    [Fact]
    public void Chalisman_controleur_gagne_un_sang_de_plus_en_fin_de_tour()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Chalisman"));
        t.Ctx.ControleurDonjon = t.Merlin;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
        });

        Assert.Equal(2, t.Merlin.Sang);   // 1 (base) + 1 (Chalisman)
    }

    // Chalisman (Immediat) : à l'obtention, prenez le Donjon.
    [Fact]
    public void Chalisman_immediat_prend_le_donjon()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [Tresor("Chalisman")];

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });

        Assert.Equal(t.Merlin, t.Ctx.ControleurDonjon);
    }
}
