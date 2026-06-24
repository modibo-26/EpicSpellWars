using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Lance NbDes des, somme les resultats, et execute la seule Tranche correspondante.
// Cas hors Jet de puissance (ex. Depipax) : NbDes fixe, pas de Glyphes, pas de GARDEZ.
public class EffetBranchement : IEffet
{
    public int NbDes { get; set; } = 1;
    public List<Tranche> Tranches { get; set; } = [];

    public void Execute(GameContext context)
    {
        var somme = 0;
        var nbDes = CalculerNbDes(context);
        for (var i = 0; i < nbDes; i++)
            somme += context.LancerDe();

        ApresLancer(somme, context);   // hook sur le RÉSULTAT du jet (Chipodada : 🩸 si >= 13)

        var tranche = Tranches.Where(t => somme >= t.Seuil).MaxBy(t => t.Seuil);
        if (tranche is null)
            return;

        AvantActions(context);   // hook avant les dégâts de la tranche (Granoloup : +1 aux dégâts du jet)

        // chaque Action = une INSTANCE distincte (cf. blocage Creature, rulebook p.9)
        foreach (var action in tranche.Actions)
            context.Appliquer(action);

        ApresTranche(tranche, context);
    }

    // Nb de des a lancer. Fixe par defaut ; le Jet de puissance le redefinit via les Glyphes.
    protected virtual int CalculerNbDes(GameContext context) => NbDes;

    // Hook sur le resultat du jet (somme), AVANT le branchement de tranche. No-op de base ; le Jet de
    // puissance y applique les modificateurs lies au resultat (Chipodada). N'est PAS appele pour un simple
    // branchement-par-de (Depipax) qui n'est pas un Jet de puissance.
    protected virtual void ApresLancer(int somme, GameContext context) { }

    // Hook juste AVANT les Actions de la tranche. No-op de base ; le Jet de puissance y arme le bonus de
    // degats du jet (Granoloup).
    protected virtual void AvantActions(GameContext context) { }

    // Hook post-tranche. No-op de base ; le Jet de puissance y traite GARDEZ (et desarme le bonus de degats).
    protected virtual void ApresTranche(Tranche tranche, GameContext context) { }
}
