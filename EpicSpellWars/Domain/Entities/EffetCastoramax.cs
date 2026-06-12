using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Castoramax (IEffet sur-mesure — 2 dés utilisés SÉPARÉMENT) :
// « Lancez 2 dés. Utilisez-EN UN pour infliger autant de dégâts à votre adversaire de gauche. Subissez
//   autant de dégâts que L'AUTRE dé, sauf si vous contrôlez le Donjon ou si vous payez 1 🩸. »
// Le lanceur choisit quel dé sert d'attaque (hook ChoisirDe) ; l'autre sert à l'auto-dégât évitable.
public class EffetCastoramax : IEffet
{
    public void Execute(GameContext context)
    {
        var d1 = context.LancerDe();
        var d2 = context.LancerDe();
        var offensif = context.ChoisirDe([d1, d2]);
        var autre = offensif == d1 ? d2 : d1;

        context.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(offensif) });

        // Auto-dégât évité si on contrôle le Donjon OU si on paie 1 🩸.
        var evite = context.ControleurDonjon == context.Lanceur || context.TenterPayer(1, "Évitez l'auto-dégât");
        if (!evite)
            context.Appliquer(new Action { Type = TypeAction.AutoDegats, Cible = Cible.Soi, Valeur = new ValeurFixe(autre) });
    }
}
