using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche C (Donjon — cycle) : +1 Sang fin de tour au contrôleur, reset début de manche,
// et bonus « Donjon : » inline (via EffetConditionnel / effets sur-mesure) sur Vishnakrax, Peutidardus,
// Volcanino, Chancedecocus.
public class DonjonCycleTests
{
    private static CarteSort Carte(string nom) =>
        Sources.Toutes().Concat(Qualites.Toutes()).Concat(Destinations.Toutes()).Single(c => c.Nom == nom);

    private static CarteSort Simple(string nom, TypeComposant type) => new(nom, type, Glyphe.Arcane);

    private static Dictionary<Sorcier, List<CarteSort>> Sort(Sorcier s, CarteSort c) => new() { [s] = [c] };

    // Fin de tour : le contrôleur du Donjon gagne +1 🩸.
    [Fact]
    public void Fin_de_tour_le_controleur_gagne_1_sang()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sort(t.Merlin, Simple("S", TypeComposant.Source)));

        Assert.Equal(1, t.Merlin.Sang);
    }

    // Fin de tour sans contrôleur : personne ne gagne de 🩸.
    [Fact]
    public void Fin_de_tour_sans_controleur_aucun_gain()
    {
        var t = new Table();

        new OrdonnanceurDeTour().JouerTour(t.Ctx, Sort(t.Merlin, Simple("S", TypeComposant.Source)));

        Assert.Equal(0, t.Merlin.Sang);
        Assert.Equal(0, t.Gandalf.Sang);
        Assert.Equal(0, t.Saroumane.Sang);
    }

    // Début de manche : le Donjon est remis au centre (personne ne le contrôle) et le compteur avance.
    [Fact]
    public void Debut_manche_remet_le_donjon_au_centre()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Gandalf;

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.Null(t.Ctx.ControleurDonjon);
        Assert.Equal(1, t.Ctx.Manche);
    }

    // Vishnakrax « Donjon : » ajoute 1 carte supplémentaire (2 au lieu d'1).
    [Fact]
    public void Vishnakrax_donjon_ajoute_une_carte_de_plus()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;
        t.Merlin.Main = [Simple("a", TypeComposant.Source), Simple("b", TypeComposant.Source), Simple("c", TypeComposant.Source)];
        t.Ctx.SortEnCours = [Carte("Vishnakrax")];

        t.Ctx.ResoudreSort();

        Assert.Single(t.Merlin.Main);   // 3 - 2 ajoutées au sort
    }

    // Vishnakrax sans Donjon : 1 seule carte ajoutée.
    [Fact]
    public void Vishnakrax_sans_donjon_ajoute_une_seule_carte()
    {
        var t = new Table();
        t.Merlin.Main = [Simple("a", TypeComposant.Source), Simple("b", TypeComposant.Source), Simple("c", TypeComposant.Source)];
        t.Ctx.SortEnCours = [Carte("Vishnakrax")];

        t.Ctx.ResoudreSort();

        Assert.Equal(2, t.Merlin.Main.Count);   // 3 - 1
    }

    // Peutidardus « Donjon : » révèle 2 Créatures au lieu d'une.
    [Fact]
    public void Peutidardus_donjon_ajoute_deux_creatures()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;
        t.Ctx.PiochePrincipale = [Simple("c1", TypeComposant.Destination), Simple("c2", TypeComposant.Destination), Simple("s", TypeComposant.Source)];
        t.Ctx.SortEnCours = [Carte("Peutidardus")];

        t.Ctx.ResoudreSort();

        Assert.Equal(2, t.Ctx.SortEnCours.Count(c => c.EstCreature));
    }

    // Peutidardus sans Donjon : 1 seule Créature.
    [Fact]
    public void Peutidardus_sans_donjon_ajoute_une_creature()
    {
        var t = new Table();
        t.Ctx.PiochePrincipale = [Simple("c1", TypeComposant.Destination), Simple("c2", TypeComposant.Destination), Simple("s", TypeComposant.Source)];
        t.Ctx.SortEnCours = [Carte("Peutidardus")];

        t.Ctx.ResoudreSort();

        Assert.Equal(1, t.Ctx.SortEnCours.Count(c => c.EstCreature));
    }

    // Volcanino « Donjon : » frappe aussi un autre adversaire (1 dé), en plus du plus fort.
    [Fact]
    public void Volcanino_donjon_frappe_aussi_un_autre_adversaire()
    {
        var t = new Table { ProchainDe = 2 };
        t.Ctx.ControleurDonjon = t.Merlin;
        t.Ctx.SortEnCours = [Carte("Volcanino")];

        t.Ctx.ResoudreSort();

        Assert.Equal(18, t.Gandalf.PointsDeVie);     // le plus fort (base) : 20 - 2
        Assert.Equal(18, t.Saroumane.PointsDeVie);   // autre adversaire (Donjon) : 20 - 2
    }

    // Volcanino sans Donjon : seul le plus fort est touché.
    [Fact]
    public void Volcanino_sans_donjon_ne_touche_que_le_plus_fort()
    {
        var t = new Table { ProchainDe = 2 };
        t.Ctx.SortEnCours = [Carte("Volcanino")];

        t.Ctx.ResoudreSort();

        Assert.Equal(18, t.Gandalf.PointsDeVie);
        Assert.Equal(20, t.Saroumane.PointsDeVie);
    }

    // Chancedecocus « Donjon : » chaque AUTRE adversaire subit aussi son propre résultat de dé.
    [Fact]
    public void Chancedecocus_donjon_frappe_aussi_les_autres()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;
        var des = new Queue<int>([5, 2]);   // Gandalf = 5, Saroumane = 2
        t.Ctx.LancerDe = () => des.Dequeue();

        new EffetChancedecocus().Execute(t.Ctx);

        Assert.Equal(15, t.Gandalf.PointsDeVie);     // gagnant : 20 - 5
        Assert.Equal(1, t.Gandalf.Sang);             // +1 🩸
        Assert.Equal(18, t.Saroumane.PointsDeVie);   // autre (Donjon) : 20 - 2
    }

    // Roulépélax « Donjon : » applique aussi la proposition au voisin de DROITE (les deux voisins donnent).
    [Fact]
    public void Roulepelax_donjon_applique_l_effet_aux_deux_voisins()
    {
        var t = new Table();   // ProchainChoix = true par défaut → chaque voisin accepte de donner sa carte Arcane
        t.Ctx.ControleurDonjon = t.Merlin;
        var gauche = new CarteSort("AG", TypeComposant.Source, Glyphe.Arcane);
        var droite = new CarteSort("AD", TypeComposant.Source, Glyphe.Arcane);
        t.Gandalf.Main = [gauche];
        t.Saroumane.Main = [droite];
        t.Ctx.SortEnCours = [Carte("Roulepélax")];

        t.Ctx.ResoudreSort();

        Assert.Contains(gauche, t.Merlin.Main);   // voisin de gauche
        Assert.Contains(droite, t.Merlin.Main);   // voisin de droite (Donjon)
    }

    // Roulépélax sans Donjon : seul le voisin de gauche est affecté.
    [Fact]
    public void Roulepelax_sans_donjon_n_affecte_que_la_gauche()
    {
        var t = new Table();
        var gauche = new CarteSort("AG", TypeComposant.Source, Glyphe.Arcane);
        var droite = new CarteSort("AD", TypeComposant.Source, Glyphe.Arcane);
        t.Gandalf.Main = [gauche];
        t.Saroumane.Main = [droite];
        t.Ctx.SortEnCours = [Carte("Roulepélax")];

        t.Ctx.ResoudreSort();

        Assert.Contains(gauche, t.Merlin.Main);
        Assert.DoesNotContain(droite, t.Merlin.Main);
        Assert.Contains(droite, t.Saroumane.Main);   // gardé : la droite n'est pas touchée
    }

    // Groclonar « Donjon : » fait les DEUX options (pair puis impair) au lieu d'en choisir une.
    [Fact]
    public void Groclonar_donjon_fait_les_deux_options()
    {
        var t = new Table();
        t.Ctx.ControleurDonjon = t.Merlin;
        t.Gandalf.PointsDeVie = 20;    // pair
        t.Saroumane.PointsDeVie = 15;  // impair
        t.Ctx.SortEnCours = [Carte("Groclonar")];

        t.Ctx.ResoudreSort();

        Assert.Equal(18, t.Gandalf.PointsDeVie);     // pair → -2
        Assert.Equal(12, t.Saroumane.PointsDeVie);   // impair → -3
    }
}
