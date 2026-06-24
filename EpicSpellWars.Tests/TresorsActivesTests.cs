using EpicSpellWars.Application.Services;
using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Infrastructure.Catalogue;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Pilier 2, tranche D — capacités ACTIVÉES des Trésors (#1) : phase d'activation au tour d'Initiative du
// porteur (1 activation payante par tour, limite SÉPARÉE des Composants). Cartes : Nachos de la Rage,
// Gang du Gong, Bébé Monstre (Payez 4), Coupe du Tocard.
public class TresorsActivesTests
{
    private static Tresor Tresor(string nom) => Tresors.Toutes().Single(t => t.Nom == nom);

    // Fait jouer un tour à Merlin (sort trivial) en activant le 1er Trésor activable s'il y en a un.
    private static void JouerTourMerlin(Table t)
    {
        t.ActivationTresor = (_, ts) => ts.FirstOrDefault();
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [new CarteSort("S", TypeComposant.Source, Glyphe.Arcane)],
        });
    }

    // Nachos de la Rage : Payez 3 🩸 → soin 2, déclenché par la phase d'activation au tour d'Initiative.
    [Fact]
    public void Nachos_de_la_rage_paye_soigne_de_deux()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Sang = 3;
        t.Merlin.Tresors.Add(Tresor("Nachos de la Rage"));

        JouerTourMerlin(t);

        Assert.Equal(12, t.Merlin.PointsDeVie);   // +2
        Assert.Equal(0, t.Merlin.Sang);           // 3 payés
    }

    // Gang du Gong : Payez 2 🩸 → 1 dégât par tranche complète de 5 PV de la cible (14 PV → 2).
    [Fact]
    public void Gang_du_gong_paye_inflige_par_tranche_de_cinq_pv()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 2;
        t.Gandalf.PointsDeVie = 14;   // adversaire ciblé (AdversaireAuChoix → 1er = Gandalf)
        t.Merlin.Tresors.Add(Tresor("Gang du Gong"));

        JouerTourMerlin(t);

        Assert.Equal(12, t.Gandalf.PointsDeVie);   // 14 / 5 = 2 dégâts
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Bébé Monstre : capacité activée Payez 4 🩸 → 2 dégâts à un adversaire.
    [Fact]
    public void Bebe_monstre_active_paye_inflige_deux()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.Sang = 4;
        t.Merlin.Tresors.Add(Tresor("Bébé Monstre"));

        JouerTourMerlin(t);

        Assert.Equal(18, t.Gandalf.PointsDeVie);   // 2 dégâts au 1er adversaire
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Coupe du Tocard : Payez 1 🩸 → soin 1.
    [Fact]
    public void Coupe_du_tocard_paye_soigne_de_un()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Sang = 1;
        t.Merlin.Tresors.Add(Tresor("Coupe du Tocard"));

        JouerTourMerlin(t);

        Assert.Equal(11, t.Merlin.PointsDeVie);
        Assert.Equal(0, t.Merlin.Sang);
    }

    // Le hook décline l'activation (null) : aucun coût payé, aucun effet, même avec un Trésor activable.
    [Fact]
    public void Activation_declinee_ne_fait_rien()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Sang = 3;
        t.Merlin.Tresors.Add(Tresor("Nachos de la Rage"));

        t.ActivationTresor = (_, _) => null;   // décline
        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [new CarteSort("S", TypeComposant.Source, Glyphe.Arcane)],
        });

        Assert.Equal(10, t.Merlin.PointsDeVie);
        Assert.Equal(3, t.Merlin.Sang);
    }

    // Limites SÉPARÉES : on peut payer un Trésor ET un Composant le même tour (rulebook). Merlin active Nachos
    // (Payez 3 → soin 2) ET paie le coût d'un Composant du sort (Payez 1 → 3 dégâts à gauche) le même tour.
    [Fact]
    public void Paiement_tresor_et_composant_sont_independants_le_meme_tour()
    {
        var t = new Table { ProchainPaye = true };
        t.Merlin.PointsDeVie = 10;
        t.Merlin.Sang = 4;
        t.Merlin.Tresors.Add(Tresor("Nachos de la Rage"));
        t.ActivationTresor = (_, ts) => ts.FirstOrDefault();

        var source = new CarteSort("S", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets =
            [
                new EffetOptionnelPayant
                {
                    Cout = 1,
                    Libelle = "3 dégâts à gauche",
                    SiPaye = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(3) }],
                },
            ],
        };

        new OrdonnanceurDeTour().JouerTour(t.Ctx, new Dictionary<Sorcier, List<CarteSort>>
        {
            [t.Merlin] = [source],
        });

        Assert.Equal(12, t.Merlin.PointsDeVie);   // Trésor payé : soin 2
        Assert.Equal(17, t.Gandalf.PointsDeVie);  // Composant payé : 3 dégâts
        Assert.Equal(0, t.Merlin.Sang);           // 3 (Trésor) + 1 (Composant) débités → les deux ont payé
    }
}
