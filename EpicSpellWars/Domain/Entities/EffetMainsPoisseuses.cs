using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Mains Poisseuses ! » (clause DebutTour, si premier à jouer) : échangez CE Trésor contre n'importe quel
// Trésor d'un adversaire. Le porteur donne Mains Poisseuses (TresorClauseEnCours) à un adversaire et prend l'un
// de ses Trésors. Simplification : on prend le 1er Trésor de l'adversaire choisi (comme VolerTresor).
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
        var convoite = cible.Tresors[0];   // TODO choix réel ; 1er Trésor (cf. VolerTresor)

        cible.Tresors.Remove(convoite);
        porteur.Tresors.Remove(cetteCarte);
        porteur.Tresors.Add(convoite);
        cible.Tresors.Add(cetteCarte);
    }
}
