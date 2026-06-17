using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche D (Trésors) : modèle d'abonnement. Immediat (« Lorsque vous gagnez ce Trésor »),
// SurInitiative (au début de votre tour), Passif data-driven (modif au goulot). Représentants : Bébé
// Monstre (Immediat), Braguette de Cthulhu (SurInitiative), Liste du Père Fouettard (passif sur kill).
public class TresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);

    // Liste du Père Fouettard : +1 🩸 EN PLUS des +3 à chaque kill du porteur.
    [Fact]
    public void Liste_du_pere_fouettard_donne_un_sang_de_plus_par_kill()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Liste du Père Fouettard"));
        t.Gandalf.PointsDeVie = 3;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(5) });

        Assert.False(t.Gandalf.EstVivant);
        Assert.Equal(4, t.Merlin.Sang);   // 3 (kill) + 1 (Trésor)
    }

    // Braguette de Cthulhu : au début du tour de son porteur, vole 1 🩸 à un adversaire.
    [Fact]
    public void Braguette_de_cthulhu_vole_un_sang_au_debut_du_tour()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(Tresor("Braguette de Cthulhu"));
        t.Gandalf.Sang = 5;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [new CarteSort("S", TypeComposant.Source, Glyphe.Arcane)],
        });

        Assert.Equal(4, t.Gandalf.Sang);   // -1 volé
        Assert.Equal(1, t.Merlin.Sang);    // +1 volé (pas de Donjon → pas d'autre gain)
    }

    // Bébé Monstre (Immediat) : 1 dégât à chaque sorcier (soi inclus), puis +2 🩸 par sorcier à <= 5 PV.
    [Fact]
    public void Bebe_monstre_immediat_a_l_obtention()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 6;     // → 5 après le dégât (compte)
        t.Saroumane.PointsDeVie = 3;   // → 2 après le dégât (compte)
        t.Ctx.PiocheTresor = [Tresor("Bébé Monstre")];

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });

        Assert.Equal(19, t.Merlin.PointsDeVie);   // soi inclus : 20 - 1
        Assert.Equal(5, t.Gandalf.PointsDeVie);
        Assert.Equal(2, t.Saroumane.PointsDeVie);
        Assert.Equal(4, t.Merlin.Sang);           // 2 sorciers <= 5 PV × 2 🩸
        Assert.Single(t.Merlin.Tresors);
    }

    // Un Trésor SANS effet déclenché (Effets vides) ne fait rien à l'obtention ni au tour.
    [Fact]
    public void Tresor_stub_n_a_aucun_effet()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [Tresor("Divan le Terrible")];   // Passif stub (Effets = [])

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });

        Assert.Single(t.Merlin.Tresors);
        Assert.Equal(20, t.Merlin.PointsDeVie);
        Assert.Equal(0, t.Merlin.Sang);
    }
}
