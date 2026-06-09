namespace EpicSpellWars.Domain.Entities;

// Tranche d'un Jet de puissance : peut porter GARDEZ.
// Rangee telle quelle dans EffetBranchement.Tranches (List<Tranche>) — pas de redeclaration.
public class TrancheJetDePuissance : Tranche
{
    public bool PeutGarder { get; set; }
}
