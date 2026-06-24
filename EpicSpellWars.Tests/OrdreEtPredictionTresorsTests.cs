using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Derniers Trésors : Pièces du Destin (prédiction du prochain tué), Mains Poisseuses (premier à jouer → échange
// ce Trésor), Vers Pas Solitaires (égalité d'initiative → payer 1 pour la remporter).
public class OrdreEtPredictionTresorsTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);
    private static CarteSort Simple(string nom, TypeComposant type) => new(nom, type, Glyphe.Arcane);

    private static void DesSequence(Table t, params int[] valeurs)
    {
        var file = new Queue<int>(valeurs);
        t.Ctx.LancerDe = () => file.Dequeue();
    }

    // Pièces du Destin : à l'obtention, on prédit le prochain tué (ici soi-même, 1er vivant via le hook) ; si la
    // prédiction se réalise → +2 🩸 (même s'il s'agit de soi).
    [Fact]
    public void Pieces_du_destin_prediction_juste_donne_deux()
    {
        var t = new Table();
        t.Ctx.PiocheTresor = [Tresor("Pièces du Destin")];

        t.Ctx.Appliquer(new Action { Type = TypeAction.GagnerTresor, Cible = Cible.Soi });
        Assert.Equal(t.Merlin, t.Ctx.Prediction!.Value.Predit);   // 1er vivant = Merlin (lui-même)

        t.Merlin.PointsDeVie = 2;
        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(2, t.Merlin.Sang);        // prédiction juste → +2
        Assert.Null(t.Ctx.Prediction);         // consommée
    }

    // Pièces du Destin : prédiction sur un autre, qui meurt bien → +2 (mort par suicide pour isoler le bonus).
    [Fact]
    public void Pieces_du_destin_predit_un_autre_qui_meurt()
    {
        var t = new Table();
        t.Ctx.Prediction = (t.Merlin, t.Gandalf);
        t.Gandalf.PointsDeVie = 2;
        t.Ctx.Lanceur = t.Gandalf;   // suicide de Gandalf (pas de récompense de kill qui fausserait le test)

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(2, t.Merlin.Sang);
        Assert.Null(t.Ctx.Prediction);
    }

    // Pièces du Destin : si c'est un AUTRE sorcier qui meurt, la prédiction est ratée (consommée, pas de 🩸).
    [Fact]
    public void Pieces_du_destin_prediction_fausse_ne_donne_rien()
    {
        var t = new Table();
        t.Ctx.Prediction = (t.Merlin, t.Saroumane);   // prédit Saroumane
        t.Gandalf.PointsDeVie = 2;
        t.Ctx.Lanceur = t.Gandalf;   // c'est Gandalf qui meurt (suicide), pas Saroumane

        t.Ctx.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(5) });

        Assert.Equal(0, t.Merlin.Sang);
        Assert.Null(t.Ctx.Prediction);   // « le prochain tué » → consommée même si fausse
    }

    // Mains Poisseuses : premier à jouer → échange ce Trésor contre un Trésor d'un adversaire.
    [Fact]
    public void Mains_poisseuses_premier_a_jouer_echange_le_tresor()
    {
        var t = new Table();
        var mains = Tresor("Mains Poisseuses !");
        var butin = new Tresor("Butin", [], TriggerType.Passif);
        t.Merlin.Tresors.Add(mains);
        t.Gandalf.Tresors.Add(butin);

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("S", TypeComposant.Source)],   // seul à jouer → premier
        });

        Assert.Contains(butin, t.Merlin.Tresors);        // pris à l'adversaire
        Assert.DoesNotContain(mains, t.Merlin.Tresors);  // ce Trésor donné
        Assert.Contains(mains, t.Gandalf.Tresors);       // reçu par l'adversaire
    }

    // Mains Poisseuses : si le porteur n'est PAS le premier à jouer, pas d'échange.
    [Fact]
    public void Mains_poisseuses_pas_premier_ne_echange_pas()
    {
        var t = new Table();
        var mains = Tresor("Mains Poisseuses !");
        var butin = new Tresor("Butin", [], TriggerType.Passif);
        t.Merlin.Tresors.Add(mains);
        t.Gandalf.Tresors.Add(butin);

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Gandalf] = [Simple("G", TypeComposant.Source)],                                  // 1 comp → premier
            [t.Merlin] = [Simple("M1", TypeComposant.Source), Simple("M2", TypeComposant.Qualite)],   // 2 comp → second
        });

        Assert.Contains(mains, t.Merlin.Tresors);    // gardé (pas premier)
        Assert.Contains(butin, t.Gandalf.Tresors);
    }

    // Vers Pas Solitaires : à égalité d'initiative, payer 1 🩸 fait jouer avant les autres.
    [Fact]
    public void Vers_pas_solitaires_paye_remporte_l_egalite()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 1;
        DesSequence(t, 1, 6);   // dés de départage : Merlin=1, Gandalf=6 (Gandalf gagnerait sans le Trésor)
        t.Merlin.Tresors.Add(Tresor("Vers Pas Solitaires"));

        var ordre = new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],     // 1 comp, Initiative 0
            [t.Gandalf] = [Simple("G", TypeComposant.Source)],    // 1 comp, Initiative 0 → égalité
        });

        Assert.Equal(t.Merlin, ordre[0]);   // remporte l'égalité
        Assert.Equal(0, t.Merlin.Sang);     // 1 payé
    }

    // Vers Pas Solitaires non payé : le départage au dé s'applique (Gandalf joue avant).
    [Fact]
    public void Vers_pas_solitaires_non_paye_laisse_le_de_departager()
    {
        var t = new Table { ProchainPaye = false };
        DesSequence(t, 1, 6);
        t.Merlin.Tresors.Add(Tresor("Vers Pas Solitaires"));

        var ordre = new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [Simple("M", TypeComposant.Source)],
            [t.Gandalf] = [Simple("G", TypeComposant.Source)],
        });

        Assert.Equal(t.Gandalf, ordre[0]);   // dé 6 > 1
    }
}
