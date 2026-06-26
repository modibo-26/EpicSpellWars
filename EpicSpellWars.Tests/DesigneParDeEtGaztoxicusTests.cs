using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;

namespace EpicSpellWars.Tests;

// Deux cartes longtemps gelées par Cible.DesigneParDe :
//  - Oulacécho : « 1 dé de dégâts à un adversaire DÉSIGNÉ PAR LE DÉ » = cible aléatoire (ChoisirIndexAuHasard).
//  - Gaztoxicus : « Payez 2 🩸 : résoudre chaque dé du Jet séparément contre des adversaires différents ».
public class DesigneParDeEtGaztoxicusTests
{
    private static CarteSort Qualite(string nom) => Qualites.Toutes().Single(c => c.Nom == nom);
    private static CarteSort Destination(string nom) => Destinations.Toutes().Single(c => c.Nom == nom);

    // Oulacécho sans Donjon : le dé (4) frappe l'adversaire tiré au hasard (index 0 = Gandalf). Pas de clause Donjon.
    [Fact]
    public void Oulacecho_designe_un_adversaire_au_hasard()
    {
        var t = new Table { ProchainDe = 4, ProchainIndexHasard = 0 };   // hasard → 1er adversaire (Gandalf)
        t.Ctx.SortEnCours = [Qualite("Oulacécho")];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // désigné par le dé : 20 - 4
        Assert.Equal(20, t.Saroumane.PointsDeVie); // pas désigné, pas de Donjon
    }

    // Oulacécho avec Donjon : dé (3) sur l'adversaire tiré (index 1 = Saroumane), PUIS 2 dégâts à un AUTRE.
    [Fact]
    public void Oulacecho_donjon_ajoute_deux_degats_a_un_autre_adversaire()
    {
        var t = new Table { ProchainDe = 3, ProchainIndexHasard = 1 };   // hasard → 2e adversaire (Saroumane)
        t.Ctx.ControleurDonjon = t.Merlin;                               // Lanceur contrôle le Donjon
        t.Ctx.SortEnCours = [Qualite("Oulacécho")];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Saroumane.PointsDeVie); // désigné par le dé : 20 - 3
        Assert.Equal(18, t.Gandalf.PointsDeVie);   // « un autre adversaire » (≠ DerniereCible) : 20 - 2
    }

    // Gaztoxicus payé : 3 dés [3,7,8] résolus séparément, répartis librement (Gandalf, Gandalf, Saroumane).
    // Le dé en 1-4 (3) déclenche GARDEZ. Plusieurs dés peuvent viser le même adversaire.
    [Fact]
    public void Gaztoxicus_paye_resout_chaque_de_separement_et_garde()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        t.Gandalf.ADejaJoueCeTour = true;
        t.Saroumane.ADejaJoueCeTour = true;

        var des = new Queue<int>([3, 7, 8]);
        t.Ctx.LancerDe = () => des.Dequeue();
        var cibles = new Queue<Sorcier>([t.Gandalf, t.Gandalf, t.Saroumane]);   // répartition par dé
        t.Ctx.ChoisirCible = _ => cibles.Dequeue();

        var gaztoxicus = Destination("Gaztoxicus");
        // 3 cartes Illusion dans le sort → CompterGlyphes(Illusion) = 3 dés.
        t.Ctx.SortEnCours =
        [
            gaztoxicus,
            new CarteSort("Brume", TypeComposant.Source, Glyphe.Illusion),
            new CarteSort("Mirage", TypeComposant.Source, Glyphe.Illusion),
        ];

        t.Ctx.ResoudreSort();

        Assert.Equal(17, t.Gandalf.PointsDeVie);            // dé 3 → 1 dégât, dé 7 → 2 dégâts : 20 - 3
        Assert.Equal(18, t.Saroumane.PointsDeVie);          // dé 8 → 2 dégâts : 20 - 2
        Assert.Contains(gaztoxicus, t.Merlin.Creatures);    // GARDEZ (un dé en 1-4)
        Assert.Equal(0, t.Merlin.Sang);                     // 2 🩸 payés
    }

    // Gaztoxicus NON payé : Jet normal. 3 dés [3,7,8] = somme 18 → tranche 10+ → 4 dégâts à UNE cible
    // (ADejaJoue, CibleUnique → ChoisirCible = 1er = Gandalf). Pas de GARDEZ en 10+.
    [Fact]
    public void Gaztoxicus_non_paye_somme_normale_une_seule_cible()
    {
        var t = new Table();   // ProchainPaye = false
        t.Gandalf.ADejaJoueCeTour = true;
        t.Saroumane.ADejaJoueCeTour = true;

        var des = new Queue<int>([3, 7, 8]);
        t.Ctx.LancerDe = () => des.Dequeue();

        var gaztoxicus = Destination("Gaztoxicus");
        t.Ctx.SortEnCours =
        [
            gaztoxicus,
            new CarteSort("Brume", TypeComposant.Source, Glyphe.Illusion),
            new CarteSort("Mirage", TypeComposant.Source, Glyphe.Illusion),
        ];

        t.Ctx.ResoudreSort();

        Assert.Equal(16, t.Gandalf.PointsDeVie);            // somme 18 → 10+ → 4 dégâts (cible unique)
        Assert.Equal(20, t.Saroumane.PointsDeVie);          // pas touché
        Assert.DoesNotContain(gaztoxicus, t.Merlin.Creatures); // pas de GARDEZ en 10+
    }
}
