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

    // Chipodada : 🩸 si le résultat du Jet atteint le seuil d'un Trésor du lanceur.
    protected override void ApresLancer(int somme, GameContext context) => context.ApresJetDePuissance(somme);

    // Granoloup : arme le bonus de dégâts du jet (lu par InfligerDegats des Actions de la tranche).
    protected override void AvantActions(GameContext context) =>
        context.BonusDegatsJet = context.Lanceur.Tresors.Sum(t => t.BonusDegatsCreatureJet);

    protected override void ApresTranche(Tranche tranche, GameContext context)
    {
        if (tranche is TrancheJetDePuissance { PeutGarder: true })
            context.GarderCreatureEnCours();
        context.BonusDegatsJet = 0;   // désarme le bonus de dégâts (portée = ce jet)
    }
}
