using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Flux de manche — tranche F4 : intégration de la Magie féroce dans la déclaration (les jokers d'un sort
// déclaré sont remplacés par de vraies cartes révélées de la pioche) + Baisse de Tension (crevé MancheSuivante
// qui augmente le 1er sort de la manche suivante avec la 1re carte de la pioche, joker pris en main).
public class MagieFeroceDeclarationTests
{
    private static SorcierCreve Creve(string nom) => SorciersCreves.Toutes().Single(c => c.Nom == nom);

    private static CarteSort Frappe(Cible cible, int montant) => new("Frappe", TypeComposant.Source, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) }] }],
    };

    // Cartes neutres pour garnir une main (Qualité sans effet).
    private static List<CarteSort> Fillers(int n) =>
        [.. Enumerable.Range(0, n).Select(i => new CarteSort($"F{i}", TypeComposant.Qualite, Glyphe.Arcane))];

    // ResoudreJokersDuSort : un joker est remplacé in place par la 1re carte du type déclaré, et défaussé.
    [Fact]
    public void Joker_est_remplace_par_une_vraie_carte_et_defausse()
    {
        var t = new Table();
        var source = new CarteSort("S", TypeComposant.Source, Glyphe.Arcane);
        var qualite = new CarteSort("Q", TypeComposant.Qualite, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [source, qualite];
        var joker = new MagieFeroce { TypeRemplace = TypeComposant.Qualite };
        var sort = new List<CarteSort> { joker };

        t.Ctx.ResoudreJokersDuSort(sort);

        Assert.Equal([qualite], sort);              // joker → 1re Qualité révélée
        Assert.Contains(joker, t.Ctx.Defausse);     // joker défaussé
        Assert.Contains(source, t.Ctx.Defausse);    // Source révélée non retenue défaussée
    }

    // AugmenterSortDepuisPioche : la 1re carte (normale) de la pioche rejoint le sort.
    [Fact]
    public void Augmenter_ajoute_la_premiere_carte_au_sort()
    {
        var t = new Table();
        var sup = new CarteSort("Sup", TypeComposant.Qualite, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [sup];
        var sort = new List<CarteSort> { new("Base", TypeComposant.Source, Glyphe.Arcane) };

        t.Ctx.AugmenterSortDepuisPioche(sort, t.Merlin);

        Assert.Contains(sup, sort);
        Assert.Equal(2, sort.Count);
    }

    // AugmenterSortDepuisPioche : si la 1re carte est un joker, il est PRIS EN MAIN et la carte suivante
    // rejoint le sort.
    [Fact]
    public void Augmenter_joker_pris_en_main_et_carte_suivante_au_sort()
    {
        var t = new Table();
        var joker = new MagieFeroce();
        var suivante = new CarteSort("Suiv", TypeComposant.Qualite, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [joker, suivante];
        var sort = new List<CarteSort>();

        t.Ctx.AugmenterSortDepuisPioche(sort, t.Merlin);

        Assert.Contains(joker, t.Merlin.Main);   // joker pris en main
        Assert.Equal([suivante], sort);          // carte suivante ajoutée au sort
    }

    // Baisse de Tension (crevé MancheSuivante) : pioché à la mort, puis au début de la manche suivante il ARME
    // le propriétaire (AugmenterPremierSort), sans rien ajouter encore.
    [Fact]
    public void Baisse_de_tension_arme_le_proprietaire_au_debut_de_la_manche()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Baisse de Tension")];
        t.Merlin.PointsDeVie = 2;

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });
        Assert.False(t.Merlin.AugmenterPremierSort);   // différé : pas encore armé

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.True(t.Merlin.AugmenterPremierSort);    // armé au début de la manche suivante
    }

    // Intégration : un sorcier armé voit son PREMIER sort augmenté de la 1re carte de la pioche pendant la
    // manche (carte létale ajoutée → kill observable), et le drapeau est consommé.
    [Fact]
    public void Baisse_de_tension_augmente_le_premier_sort_en_jeu()
    {
        var t = new Table();
        var baseSource = Frappe(Cible.AdversaireGauche, 5);   // Gandalf -5 (non létal seul)
        var augSource = Frappe(Cible.TousAdversaires, 25);    // ajout létal sur tous
        t.Merlin.Main = [baseSource, .. Fillers(7)];          // mains pleines → la complétion ne pioche rien
        t.Gandalf.Main = Fillers(8);
        t.Saroumane.Main = Fillers(8);
        t.Ctx.PiochePrincipale = [augSource];                 // au sommet : l'augmentation
        t.Merlin.AugmenterPremierSort = true;
        t.Declaration = s => s == t.Merlin ? [baseSource] : [];

        var vainqueur = new OrdonnanceurDeTour().JouerManche(t.Ctx);

        Assert.Same(t.Merlin, vainqueur);
        Assert.False(t.Saroumane.EstVivant);            // tué par l'augmentation (baseSource ne touche que Gandalf)
        Assert.False(t.Merlin.AugmenterPremierSort);    // drapeau consommé
    }

    // Intégration : un joker déclaré dans un sort est résolu en vraie carte puis joué pendant la manche.
    [Fact]
    public void Joker_declare_est_resolu_et_joue_pendant_la_manche()
    {
        var t = new Table();
        var joker = new MagieFeroce { TypeRemplace = TypeComposant.Source };
        var letale = Frappe(Cible.TousAdversaires, 25);   // vraie Source létale dans la pioche
        t.Merlin.Main = [joker, .. Fillers(7)];
        t.Gandalf.Main = Fillers(8);
        t.Saroumane.Main = Fillers(8);
        t.Ctx.PiochePrincipale = [letale];
        t.Declaration = s => s == t.Merlin ? [joker] : [];

        var vainqueur = new OrdonnanceurDeTour().JouerManche(t.Ctx);

        Assert.Same(t.Merlin, vainqueur);            // le joker → Source létale → kill
        Assert.Contains(joker, t.Ctx.Defausse);      // joker défaussé
    }
}
