using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Foulremix (IEffet sur-mesure, hors modele par-cible) :
// « Choisissez un type de cartes. Chaque sorcier passe TOUTES les cartes de ce type de sa main à son
//   voisin de gauche, puis subit autant de degats que le nombre de cartes qu'il reçoit. »
// Le passage est SIMULTANE → on fige (snapshot) ce que chacun passe avant tout deplacement.
public class EffetFoulremix : IEffet
{
    // Cout du « Payez N 🩸 : Doublez les dégâts ». 0 = pas d'option payante.
    public int CoutDouble { get; set; }

    public void Execute(GameContext context)
    {
        var type = context.ChoisirTypeComposant(context.Lanceur,
            [TypeComposant.Source, TypeComposant.Qualite, TypeComposant.Destination]);

        // Decision de paiement AVANT de calculer les degats (regles-sang).
        var multiplicateur = CoutDouble > 0 && context.TenterPayer(CoutDouble, "Doublez les dégâts infligés")
            ? 2 : 1;

        var vivants = context.Sorciers.Where(s => s.EstVivant).ToList();
        if (vivants.Count < 2)
            return;

        // 1) Snapshot simultane : ce que chacun passe, AVANT tout retrait/ajout.
        var aPasser = vivants.ToDictionary(s => s, s => s.Main.Where(c => c.Type == type).ToList());

        // 2) Retrait des mains + remise au voisin de gauche ; comptage des cartes recues.
        var recues = vivants.ToDictionary(s => s, _ => 0);
        foreach (var s in vivants)
        {
            foreach (var c in aPasser[s])
                s.Main.Remove(c);

            var voisin = VoisinGauche(context, s);
            if (voisin is null)
                continue;
            voisin.Main.AddRange(aPasser[s]);
            recues[voisin] += aPasser[s].Count;
        }

        // 3) Degats = nb de cartes recues (x multiplicateur), borne a 0.
        foreach (var s in vivants)
            s.PointsDeVie = Math.Max(0, s.PointsDeVie - recues[s] * multiplicateur);
    }

    // Voisin de gauche vivant (case suivante = +1), en sautant les morts. Convention table identique
    // à GameContext. Voisin, mais relative a un sorcier quelconque (pas seulement au lanceur).
    private static Sorcier? VoisinGauche(GameContext context, Sorcier s)
    {
        var n = context.Sorciers.Count;
        var i = context.Sorciers.IndexOf(s);
        for (var pas = 1; pas < n; pas++)
        {
            var v = context.Sorciers[(i + pas) % n];
            if (v != s && v.EstVivant)
                return v;
        }
        return null;
    }
}
