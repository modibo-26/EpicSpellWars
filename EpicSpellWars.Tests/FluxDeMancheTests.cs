using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Flux de manche (tranche F1) : la boucle de manche (OrdonnanceurDeTour.JouerManche) enchaîne des tours
// jusqu'au dernier survivant. Chaque tour : les vivants complètent leur main à 8, les morts piochent un
// Sorcier crevé, chacun déclare son sort, résolution. Fin de manche : défausse mains/Trésors/Créatures
// (crevés gardés). DebutManche réveille les morts (PV → départ) et déclenche les effets différés.
public class FluxDeMancheTests
{
    private static SorcierCreve Creve(string nom) => SorciersCreves.Toutes().Single(c => c.Nom == nom);

    // Source qui inflige `montant` dégâts à une cible donnée.
    private static CarteSort Frappe(Cible cible, int montant) => new("Frappe", TypeComposant.Source, Glyphe.Arcane)
    {
        Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = cible, Valeur = new ValeurFixe(montant) }] }],
    };

    // Pioche de remplissage (cartes neutres) pour compléter les mains.
    private static List<Carte> Remplissage(int n) =>
        [.. Enumerable.Range(0, n).Select(i => (Carte)new CarteSort($"R{i}", TypeComposant.Qualite, Glyphe.Arcane))];

    // Une manche se termine quand il ne reste qu'un survivant : il gagne le jeton, et la fin de manche
    // défausse mains/Trésors/Créatures (les crevés restent).
    [Fact]
    public void JouerManche_se_termine_au_dernier_survivant_et_nettoie()
    {
        var t = new Table();
        var foudre = Frappe(Cible.TousAdversaires, 25);   // létal sur les deux adversaires en un tour
        t.Merlin.Main = [foudre];
        t.Gandalf.Tresors = [new Tresor("T", [], TriggerType.Passif)];
        t.Ctx.PiochePrincipale = Remplissage(30);
        t.Declaration = s => s == t.Merlin ? [foudre] : [];

        var vainqueur = new OrdonnanceurDeTour().JouerManche(t.Ctx);

        Assert.Same(t.Merlin, vainqueur);
        Assert.Equal(1, t.Merlin.JetonsDernierSurvivant);
        Assert.False(t.Gandalf.EstVivant);
        Assert.False(t.Saroumane.EstVivant);
        Assert.Empty(t.Merlin.Main);        // main défaussée en fin de manche
        Assert.Empty(t.Gandalf.Tresors);    // Trésors défaussés en fin de manche
    }

    // Début de tour : on complète la main à 8 cartes depuis la pioche principale.
    [Fact]
    public void CompleterMain_complete_jusqu_a_huit()
    {
        var t = new Table();
        t.Merlin.Main = [Frappe(Cible.Soi, 0)];   // 1 carte déjà en main
        t.Ctx.PiochePrincipale = Remplissage(20);

        t.Ctx.CompleterMain(t.Merlin, 8);

        Assert.Equal(8, t.Merlin.Main.Count);
        Assert.Equal(13, t.Ctx.PiochePrincipale.Count);   // 7 cartes tirées
    }

    // Un sorcier mort pioche un nouveau Sorcier crevé au DÉBUT de chaque tour (en plus de celui de sa mort).
    [Fact]
    public void Un_mort_pioche_un_creve_au_debut_de_chaque_tour()
    {
        var t = new Table();
        // Deux « Tournée d'Adieu » (+2 🩸 immédiat) : 1 piochée à la mort, 1 au début du tour suivant.
        t.Ctx.PiocheSorcierCreve = [Creve("Tournée d'Adieu"), Creve("Tournée d'Adieu")];
        t.Ctx.PiochePrincipale = Remplissage(40);

        var tueGauche = Frappe(Cible.AdversaireGauche, 25);   // Gandalf
        var tueDroite = Frappe(Cible.AdversaireDroite, 25);   // Saroumane
        t.Merlin.Main = [tueGauche, tueDroite];
        // Tour 1 : tue Gandalf (reste Merlin + Saroumane) ; tour 2 : tue Saroumane.
        t.Declaration = s => s != t.Merlin ? [] : t.Gandalf.EstVivant ? [tueGauche] : [tueDroite];

        new OrdonnanceurDeTour().JouerManche(t.Ctx);

        // Gandalf : crevé à la mort (tour 1) + crevé au début du tour 2 = 2 crevés, +2 🩸 chacun.
        Assert.Equal(2, t.Gandalf.SorciersCreves.Count);
        Assert.Equal(4, t.Gandalf.Sang);
    }

    // Ronronne en Paix (crevé MancheSuivante) : à la mort, différé ; au début de la manche suivante, révèle
    // la pioche jusqu'à une Créature et la met en jeu devant le propriétaire (le reste révélé est défaussé).
    [Fact]
    public void Ronronne_en_paix_met_une_creature_en_jeu_au_debut_de_la_manche_suivante()
    {
        var t = new Table();
        t.Ctx.PiocheSorcierCreve = [Creve("Ronronne en Paix")];
        var source = new CarteSort("S", TypeComposant.Source, Glyphe.Arcane);
        var creature = new CarteSort("Bête", TypeComposant.Destination, Glyphe.Arcane);
        t.Ctx.PiochePrincipale = [source, creature];
        t.Merlin.PointsDeVie = 2;

        // Merlin meurt → pioche Ronronne (différé, rien tout de suite).
        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });
        Assert.Empty(t.Merlin.Creatures);

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.Contains(creature, t.Merlin.Creatures);   // Créature mise en jeu
        Assert.Contains(source, t.Ctx.Defausse);         // carte révélée non retenue → défaussée
    }

    // DebutManche réveille les sorciers morts (PV → départ) et déclenche les effets différés.
    [Fact]
    public void DebutManche_reveille_les_morts_et_lance_les_effets_differes()
    {
        var t = new Table();
        t.Gandalf.PointsDeVie = 0;          // mort
        t.Merlin.PointsDeVie = 5;
        // Effet différé observable : +2 🩸 à Merlin au début de la manche.
        t.Ctx.EffetsDifferes.Add(([new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerSang, Cible = Cible.Soi, Valeur = new ValeurFixe(2) }] }], t.Merlin));

        new OrdonnanceurDeTour().DebutManche(t.Ctx);

        Assert.Equal(Sorcier.PvDepart, t.Gandalf.PointsDeVie);   // réveillé
        Assert.True(t.Gandalf.EstVivant);
        Assert.Equal(Sorcier.PvDepart, t.Merlin.PointsDeVie);    // remis au départ
        Assert.Equal(2, t.Merlin.Sang);                          // effet différé joué
        Assert.Empty(t.Ctx.EffetsDifferes);
    }
}
