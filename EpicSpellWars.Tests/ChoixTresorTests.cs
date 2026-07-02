using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Choix du Trésor volé / défaussé (rulebook + textes) : le VOLEUR choisit ce qu'il prend (Tentaculax, Mains
// Poisseuses « n'importe quel ») ; le PROPRIÉTAIRE choisit ce qu'il défausse (Écrabouillax/Gromago « 1 de SES »).
public class ChoixTresorTests
{
    private static Tresor T(string nom) => new(nom, [], TriggerType.Passif);

    // VolerTresor : c'est le LANCEUR (voleur) qui choisit, parmi les Trésors de la cible.
    [Fact]
    public void Voler_le_voleur_choisit_le_tresor()
    {
        var t = new Table();
        var pepite = T("Pépite"); var caillou = T("Caillou");
        t.Gandalf.Tresors = [caillou, pepite];
        // Le voleur (Merlin) convoite « Pépite » (pas le 1er de la liste).
        t.ChoixTresor = (chooser, ts) => chooser == t.Merlin ? ts.Single(x => x.Nom == "Pépite") : ts[0];

        t.Ctx.Appliquer(new Action { Type = TypeAction.VolerTresor, Cible = Cible.AdversaireGauche });

        Assert.Contains(pepite, t.Merlin.Tresors);        // le voleur a pris celui qu'il voulait
        Assert.Contains(caillou, t.Gandalf.Tresors);      // l'autre reste
        Assert.DoesNotContain(pepite, t.Gandalf.Tresors);
    }

    // DefausserTresor : c'est le PROPRIÉTAIRE (la cible) qui choisit lequel de SES Trésors il défausse.
    [Fact]
    public void Defausser_le_proprietaire_choisit_le_tresor()
    {
        var t = new Table();
        var precieux = T("Précieux"); var camelote = T("Camelote");
        t.Gandalf.Tresors = [precieux, camelote];
        // Le propriétaire (Gandalf) jette « Camelote » (garde le précieux).
        t.ChoixTresor = (chooser, ts) => chooser == t.Gandalf ? ts.Single(x => x.Nom == "Camelote") : ts[0];

        t.Ctx.Appliquer(new Action { Type = TypeAction.DefausserTresor, Cible = Cible.AdversaireGauche });

        Assert.Contains(precieux, t.Gandalf.Tresors);       // il garde le meilleur
        Assert.DoesNotContain(camelote, t.Gandalf.Tresors);
        Assert.Contains(camelote, t.Ctx.PiocheTresor);      // remis sous la pile
    }
}
