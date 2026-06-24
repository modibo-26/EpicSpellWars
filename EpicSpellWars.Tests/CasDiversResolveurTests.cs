using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Cas divers du résolveur (#4) : les branches payées « hors-modèle » de Bébéfédex (carte au hasard),
// Cadopourrix (payé enveloppant un IEffet), Fourchétix (tuer TOUTES les Créatures) et Sabruledepartoux
// (voisins du contrôleur du Donjon).
public class CasDiversResolveurTests
{
    private static CarteSort Carte(string nom) =>
        Sources.Toutes().Concat(Qualites.Toutes()).Single(c => c.Nom == nom);

    private static CarteSort Simple(string nom, TypeComposant type) => new(nom, type, Glyphe.Arcane);

    // Bébéfédex payé : ajoute 1 carte AU HASARD de la main au sort (index tiré par le hook).
    [Fact]
    public void Bebefedex_paye_ajoute_une_carte_au_hasard()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 3;
        var extra = Simple("Extra", TypeComposant.Source);
        t.Merlin.Main = [extra];
        t.Ctx.SortEnCours = [Carte("Bébéfédex")];

        t.Ctx.ResoudreSort();

        Assert.Empty(t.Merlin.Main);                 // carte retirée de la main
        Assert.Contains(extra, t.Ctx.SortEnCours);   // ajoutée au sort
        Assert.Equal(0, t.Merlin.Sang);              // 3 🩸 payés
    }

    // Bébéfédex non payé : seul l'effet de base (2 au plus fort) ; rien n'est ajouté.
    [Fact]
    public void Bebefedex_non_paye_n_ajoute_rien()
    {
        var t = new Table { ProchainPaye = false };
        t.Merlin.Main = [Simple("Extra", TypeComposant.Source)];
        t.Ctx.SortEnCours = [Carte("Bébéfédex")];

        t.Ctx.ResoudreSort();

        Assert.Single(t.Merlin.Main);              // rien retiré
        Assert.Equal(18, t.Gandalf.PointsDeVie);   // 2 au plus fort
    }

    // Cadopourrix payé : révèle la pioche jusqu'à une Créature et l'ajoute au sort (branche payée = IEffet).
    [Fact]
    public void Cadopourrix_paye_revele_une_creature_et_l_ajoute()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 4;
        var dest = Simple("Dest", TypeComposant.Destination);
        t.Merlin.Main = [dest];
        var source = Simple("S", TypeComposant.Source);
        var creature = Simple("Bête", TypeComposant.Destination);
        t.Ctx.PiochePrincipale = [source, creature];
        t.Ctx.SortEnCours = [Carte("Cadopourrix")];

        t.Ctx.ResoudreSort();

        Assert.Contains(dest, t.Ctx.SortEnCours);       // base : Destination de la main
        Assert.Contains(creature, t.Ctx.SortEnCours);   // payé : Créature révélée
        Assert.Contains(source, t.Ctx.Defausse);        // révélée non retenue
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Cadopourrix non payé : seule la base (1 Destination de la main) ; la pioche n'est pas touchée.
    [Fact]
    public void Cadopourrix_non_paye_n_ajoute_que_la_base()
    {
        var t = new Table { ProchainPaye = false };
        var dest = Simple("Dest", TypeComposant.Destination);
        t.Merlin.Main = [dest];
        var creature = Simple("Bête", TypeComposant.Destination);
        t.Ctx.PiochePrincipale = [creature];
        t.Ctx.SortEnCours = [Carte("Cadopourrix")];

        t.Ctx.ResoudreSort();

        Assert.Contains(dest, t.Ctx.SortEnCours);
        Assert.DoesNotContain(creature, t.Ctx.SortEnCours);   // pioche intacte
        Assert.Single(t.Ctx.PiochePrincipale);
    }

    // Fourchétix payé : tue TOUTES les Créatures de la cible (montant = nb de Créatures), puis 3 dégâts.
    [Fact]
    public void Fourchetix_paye_tue_toutes_les_creatures()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 3;
        t.Gandalf.Creatures = [Simple("C1", TypeComposant.Destination), Simple("C2", TypeComposant.Destination)];
        t.Ctx.SortEnCours = [Carte("Fourchétix")];

        t.Ctx.ResoudreSort();

        Assert.Empty(t.Gandalf.Creatures);                  // toutes tuées
        Assert.Equal(17, t.Gandalf.PointsDeVie);            // 3 dégâts
        Assert.Equal(t.Gandalf, t.Ctx.ControleurDonjon);    // a reçu le Donjon
    }

    // Fourchétix non payé : les Créatures sont épargnées (mais les 3 dégâts s'appliquent).
    [Fact]
    public void Fourchetix_non_paye_epargne_les_creatures()
    {
        var t = new Table { ProchainPaye = false };
        t.Gandalf.Creatures = [Simple("C1", TypeComposant.Destination)];
        t.Ctx.SortEnCours = [Carte("Fourchétix")];

        t.Ctx.ResoudreSort();

        Assert.Single(t.Gandalf.Creatures);
        Assert.Equal(17, t.Gandalf.PointsDeVie);
    }

    // Sabruledepartoux payé : le contrôleur subit 8, ses VOISINS directs (dont le lanceur ici) subissent 4.
    [Fact]
    public void Sabruledepartoux_paye_frappe_le_controleur_et_ses_voisins()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 6;
        t.Ctx.ControleurDonjon = t.Saroumane;   // voisins de Saroumane = Merlin et Gandalf
        t.Ctx.SortEnCours = [Carte("Sabruledepartoux")];

        t.Ctx.ResoudreSort();

        Assert.Equal(12, t.Saroumane.PointsDeVie);   // contrôleur : 8
        Assert.Equal(16, t.Gandalf.PointsDeVie);     // voisin : 4
        Assert.Equal(16, t.Merlin.PointsDeVie);      // voisin (lanceur) : 4
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Sabruledepartoux non payé : seul le contrôleur subit 4 (base).
    [Fact]
    public void Sabruledepartoux_non_paye_ne_frappe_que_le_controleur()
    {
        var t = new Table { ProchainPaye = false };
        t.Ctx.ControleurDonjon = t.Saroumane;
        t.Ctx.SortEnCours = [Carte("Sabruledepartoux")];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Saroumane.PointsDeVie);   // contrôleur : 4
        Assert.Equal(20, t.Gandalf.PointsDeVie);     // voisins épargnés
        Assert.Equal(20, t.Merlin.PointsDeVie);
    }
}
