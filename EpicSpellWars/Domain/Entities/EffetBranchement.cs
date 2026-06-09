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

        var tranche = Tranches.Where(t => somme >= t.Seuil).MaxBy(t => t.Seuil);
        if (tranche is null)
            return;

        // chaque Action = une INSTANCE distincte (cf. blocage Creature, rulebook p.9)
        foreach (var action in tranche.Actions)
            context.Appliquer(action);

        ApresTranche(tranche, context);
    }

    // Nb de des a lancer. Fixe par defaut ; le Jet de puissance le redefinit via les Glyphes.
    protected virtual int CalculerNbDes(GameContext context) => NbDes;

    // Hook post-tranche. No-op de base ; le Jet de puissance y traite GARDEZ.
    protected virtual void ApresTranche(Tranche tranche, GameContext context) { }
}
