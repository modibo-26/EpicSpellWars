using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Étape 2 (manques résolveur — Valeurs) : ParTresorEnJeu (global), ParSang (Lanceur/Cible/DerniereCible),
// ParCreatureEnMain (Créatures dans la main de la cible, ≠ ParCreature en jeu).
public class ValeursTests
{
    // ParTresorEnJeu = somme des Trésors de TOUS les sorciers en jeu (Trésormodix payé).
    [Fact]
    public void ParTresorEnJeu_compte_tous_les_tresors_en_jeu()
    {
        var t = new Table();
        t.Merlin.Tresors.Add(new Tresor("A", [], TriggerType.Passif));
        t.Gandalf.Tresors.Add(new Tresor("B", [], TriggerType.Passif));
        t.Gandalf.Tresors.Add(new Tresor("C", [], TriggerType.Passif));   // 3 Trésors en jeu au total

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireDroite, Valeur = new ValeurParTresorEnJeu(2) });

        Assert.Equal(14, t.Saroumane.PointsDeVie);   // 20 - 2×3
    }

    // ParSang Lanceur = « votre niveau de 🩸 ».
    [Fact]
    public void ParSang_lanceur_lit_le_sang_du_lanceur()
    {
        var t = new Table();
        t.Merlin.Sang = 4;

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurParSang(1, SourceSorcier.Lanceur) });

        Assert.Equal(16, t.Gandalf.PointsDeVie);     // 20 - 4 (Sang du lanceur)
    }

    // Asphixis payé : la cible subit VOTRE Sang (Lanceur), vous subissez SON Sang (DerniereCible).
    [Fact]
    public void ParSang_croise_lanceur_et_derniere_cible()
    {
        var t = new Table();
        t.Merlin.Sang = 3;
        t.Gandalf.Sang = 7;

        // « Il subit votre Sang » → Gandalf -3, et DerniereCible = Gandalf.
        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurParSang(1, SourceSorcier.Lanceur) });
        // « vous subissez son Sang » → Merlin -7.
        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurParSang(1, SourceSorcier.DerniereCible) });

        Assert.Equal(17, t.Gandalf.PointsDeVie);     // 20 - 3
        Assert.Equal(13, t.Merlin.PointsDeVie);      // 20 - 7
    }

    // ParCreatureEnMain = Créatures dans la MAIN de la cible (Poilcramus), pas celles en jeu.
    [Fact]
    public void ParCreatureEnMain_compte_les_creatures_de_la_main_de_la_cible()
    {
        var t = new Table();
        t.Gandalf.Main.Add(new CarteSort("Bête 1", TypeComposant.Destination, Glyphe.Primaire));   // Créature
        t.Gandalf.Main.Add(new CarteSort("Une Source", TypeComposant.Source, Glyphe.Primaire));     // pas une Créature
        t.Gandalf.Main.Add(new CarteSort("Bête 2", TypeComposant.Destination, Glyphe.Arcane));      // Créature

        t.Ctx.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurParCreatureEnMain(2) });

        Assert.Equal(16, t.Gandalf.PointsDeVie);     // 20 - 2×2 (deux Créatures en main)
    }
}
