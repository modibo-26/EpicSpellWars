using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Spiralex (IEffet sur-mesure) : « 1 dégât à votre adversaire de droite, puis 2 à son adversaire de
// droite, puis 3 au suivant... jusqu'à ce que chaque adversaire ait subi des dégâts. »
// Parcours vers la DROITE (sens -1) depuis le lanceur ; le montant croît de 1 à chaque adversaire
// VIVANT touché (les morts sont sautés sans incrémenter). « Payez 2 🩸 : Doublez les dégâts ».
public class EffetSpiralex : IEffet
{
    public int CoutDouble { get; set; }   // 0 = pas d'option payante

    public void Execute(GameContext context)
    {
        var multiplicateur = CoutDouble > 0 && context.TenterPayer(CoutDouble, "Doublez les dégâts infligés")
            ? 2 : 1;

        var n = context.Sorciers.Count;
        var i = context.Sorciers.IndexOf(context.Lanceur);
        var degats = 1;
        for (var pas = 1; pas < n; pas++)
        {
            var s = context.Sorciers[((i - pas) % n + n) % n];   // -1 = droite
            if (s == context.Lanceur || !s.EstVivant)
                continue;
            context.InfligerDegats(s, degats * multiplicateur);
            degats++;
        }
    }
}
