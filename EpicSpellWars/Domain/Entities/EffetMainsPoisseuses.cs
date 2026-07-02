using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Mains Poisseuses ! » (clause DebutTour, si premier à jouer) : échangez CE Trésor contre n'importe quel
// Trésor d'un adversaire. Le porteur donne Mains Poisseuses (TresorClauseEnCours) à un adversaire et prend l'un
// de ses Trésors — « n'importe quel » → c'est le porteur (voleur) qui choisit lequel (ChoisirTresor).
public class EffetMainsPoisseuses : IEffet
{
    public void Execute(GameContext context)
    {
        if (context.TresorClauseEnCours is not { } cetteCarte)
            return;

        var porteur = context.Lanceur;
        var adversaires = context.Sorciers.Where(s => s != porteur && s.EstVivant && s.Tresors.Count > 0).ToList();
        if (adversaires.Count == 0)
            return;

        var cible = context.ChoisirCible(adversaires);
        var convoite = context.ChoisirTresor(porteur, cible.Tresors);   // le porteur choisit le Trésor convoité

        cible.Tresors.Remove(convoite);
        porteur.Tresors.Remove(cetteCarte);
        porteur.Tresors.Add(convoite);
        cible.Tresors.Add(cetteCarte);
    }
}
