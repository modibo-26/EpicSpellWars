using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Coupéhendus (IEffet sur-mesure — 2 dés utilisés SÉPARÉMENT) :
// « Lancez 2 dés. Choisissez un adversaire et infligez-lui le PLUS PETIT résultat. Payez 2 🩸 :
//   Infligez l'AUTRE résultat à un autre adversaire. »
// La partie sur-mesure = lancer 2 dés et en isoler min / autre ; le reste passe par les Actions standard
// (AdversaireAuChoix pose DerniereCible ; AutreAdversaire = un adversaire différent).
public class EffetCoupehendus : IEffet
{
    public void Execute(GameContext context)
    {
        var d1 = context.LancerDe();
        var d2 = context.LancerDe();
        var (petit, autre) = d1 <= d2 ? (d1, d2) : (d2, d1);

        context.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireAuChoix, Valeur = new ValeurFixe(petit) });

        if (context.TenterPayer(2, "Infligez l'autre résultat à un autre adversaire"))
            context.Appliquer(new Action { Type = TypeAction.Degats, Cible = Cible.AutreAdversaire, Valeur = new ValeurFixe(autre) });
    }
}
