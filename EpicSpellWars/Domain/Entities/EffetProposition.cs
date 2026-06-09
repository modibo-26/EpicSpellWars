using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Groupe C : on propose un choix oui/non a une cible, puis on execute la branche correspondante.
// Le branchement vit ICI (pas dans GameContext.Appliquer, qui reste par-action) ; chaque branche
// est une liste d'Actions reutilisant les TypeAction existants.
//   Roulepélax : « donner 1 carte Arcane ? » oui → PasserCartes(filtre Arcane) ; non → VolerTresor sur la cible.
//   Boucledorus : « donner 1 Créature ? »   oui → GagnerCarte(filtre Créature) ; non → Degats (1 dé) sur la cible.
// Les Actions des branches ciblent la cible designee via Cible.MemeCible (memorisee avant le choix).
public class EffetProposition : IEffet
{
    public Cible Cible { get; set; }             // a qui on propose (le decideur)
    public string Proposition { get; set; } = "";
    public List<Action> SiAccepte { get; set; } = [];
    public List<Action> SiRefuse { get; set; } = [];

    public void Execute(GameContext context)
    {
        foreach (var decideur in context.ResoudreCible(Cible).ToList())
        {
            // Cale les cibles relatives (MemeCible/AutreAdversaire) sur le decideur pour les branches.
            if (decideur != context.Lanceur)
                context.DerniereCible = decideur;

            var branche = context.ChoisirOption(decideur, Proposition) ? SiAccepte : SiRefuse;
            foreach (var action in branche)
                context.Appliquer(action);
        }
    }
}
