using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Xunit;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Groupe C : décision oui/non d'une cible (EffetProposition + hook ChoisirOption), don ou repli.
public class GroupeCTests
{
    // 8) Roulepélax — la cible accepte → elle donne 1 carte Arcane (PasserCartes vers la main du lanceur).
    [Fact]
    public void Proposition_acceptee_la_cible_donne_une_carte()
    {
        var t = new Table { ProchainChoix = true };
        var rune = new CarteSort("Rune", TypeComposant.Qualite, Glyphe.Arcane);
        t.Gandalf.Main.Add(rune);

        new EffetProposition
        {
            Cible = Cible.AdversaireGauche,
            Proposition = "Donner 1 carte Arcane ?",
            SiAccepte = [new Action { Type = TypeAction.PasserCartes, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.Glyphe == Glyphe.Arcane }],
            SiRefuse = [new Action { Type = TypeAction.VolerTresor, Cible = Cible.MemeCible }],
        }.Execute(t.Ctx);

        Assert.Contains(rune, t.Merlin.Main);
        Assert.Empty(t.Gandalf.Main);
    }

    // 9) Boucledorus — la cible refuse → branche de repli : 1 dé de dégâts (dé forcé à 4).
    [Fact]
    public void Proposition_refusee_branche_de_repli()
    {
        var t = new Table { ProchainChoix = false, ProchainDe = 4 };

        new EffetProposition
        {
            Cible = Cible.AdversaireGauche,
            Proposition = "Donner 1 Créature ?",
            SiAccepte = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.MemeCible, MinCartes = 1, FiltreCarte = c => c.EstCreature }],
            SiRefuse = [new Action { Type = TypeAction.Degats, Cible = Cible.MemeCible, Valeur = new ValeurDe(1) }],
        }.Execute(t.Ctx);

        Assert.Equal(16, t.Gandalf.PointsDeVie);   // 20 - 4
    }
}
