namespace EpicSpellWars.Domain.Entities;

// Gaztoxicus (variante du Jet de puissance — résolution par-dé optionnelle) :
// « Payez 2 🩸 : Vous pouvez résoudre chaque dé de ce Jet individuellement contre des adversaires
//   différents. Cible : Adversaire qui a déjà joué ce tour. »
// - Sans payer (« vous pouvez » = optionnel) : Jet normal (somme des dés → 1 tranche → 1 cible).
// - En payant : pas de somme. Chaque dé est un mini-jet INDÉPENDANT — évalué seul contre les tranches,
//   avec SA propre cible choisie parmi les « a déjà joué ». Le lanceur répartit librement : plusieurs dés
//   peuvent viser le même adversaire (« 2 dés sur A, 1 sur B »). GARDEZ dès qu'AU MOINS un dé tombe dans
//   une tranche PeutGarder (utile ici : le GARDEZ est sur la tranche basse 1-4, perdu si on somme trop).
// Les Actions des tranches portent déjà Cible.ADejaJoue + CibleUnique → un ChoisirCible par dé.
public class EffetGaztoxicus : EffetJetDePuissance
{
    public override void Execute(GameContext context)
    {
        var des = LancerEtAjuster(context);

        // « Vous pouvez » : optionnel (et soumis à la limite 1 paiement de Composant / tour). Pas payé → Jet normal.
        if (!context.TenterPayer(2, "Résolvez chaque dé de ce Jet séparément contre des adversaires différents"))
        {
            ResoudreSomme(des, context);
            return;
        }

        // Mode séparé : pas de ApresLancer (Chipodada) — « le résultat du Jet » n'a pas de sens dé par dé.
        AvantActions(context);   // Granoloup : +1 aux dégâts du jet, armé une fois pour tous les dés.

        var garde = false;
        foreach (var de in des)
        {
            var tranche = Tranches.Where(t => de >= t.Seuil).MaxBy(t => t.Seuil);
            if (tranche is null)
                continue;

            // chaque Action = une INSTANCE distincte ; Cible.ADejaJoue + CibleUnique → ChoisirCible pour CE dé.
            foreach (var action in tranche.Actions)
                context.Appliquer(action);

            if (tranche is TrancheJetDePuissance { PeutGarder: true })
                garde = true;
        }

        if (garde)
            context.GarderCreatureEnCours();
        context.BonusDegatsJet = 0;   // désarme le bonus de dégâts (portée = ce jet)
    }
}
