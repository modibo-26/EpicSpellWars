using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Chancedecocus (IEffet sur-mesure) : « Chaque adversaire lance 1 dé. Celui qui obtient le plus grand
// résultat gagne 1 🩸 et subit autant de dégâts que son résultat. »
// Égalité au sommet tranchée par le lanceur (ChoisirCible, comme les superlatifs).
// TODO Donjon (pilier déclencheurs) : « chaque AUTRE adversaire subit aussi autant de dégâts que son résultat ».
public class EffetChancedecocus : IEffet
{
    public void Execute(GameContext context)
    {
        var advs = context.Sorciers.Where(s => s != context.Lanceur && s.EstVivant).ToList();
        if (advs.Count == 0)
            return;

        var resultats = advs.ToDictionary(s => s, _ => context.LancerDe());
        var max = resultats.Values.Max();
        var gagnants = advs.Where(s => resultats[s] == max).ToList();
        var gagnant = gagnants.Count == 1 ? gagnants[0] : context.ChoisirCible(gagnants);

        gagnant.Sang = Math.Min(gagnant.SangMax, gagnant.Sang + 1);
        gagnant.PointsDeVie = Math.Max(0, gagnant.PointsDeVie - resultats[gagnant]);
    }
}
