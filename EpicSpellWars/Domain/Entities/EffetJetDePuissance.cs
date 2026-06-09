using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Domain.Entities;

// Jet de puissance : specialisation du branchement.
// Nb de des = nb de cartes du sort ayant le meme Glyphe (self + creatures gardees) + modificateurs.
// Seul cas a porter GARDEZ (via TrancheJetDePuissance.PeutGarder).
// La cible n'est PAS portee ici : chaque Action porte sa propre Cible.
public class EffetJetDePuissance : EffetBranchement
{
    public Glyphe Glyphe { get; set; }

    protected override int CalculerNbDes(GameContext context) =>
        context.CompterGlyphes(Glyphe) + context.BonusDesJet(this);

    protected override void ApresTranche(Tranche tranche, GameContext context)
    {
        if (tranche is TrancheJetDePuissance { PeutGarder: true })
            context.GarderCreatureEnCours();
    }
}
