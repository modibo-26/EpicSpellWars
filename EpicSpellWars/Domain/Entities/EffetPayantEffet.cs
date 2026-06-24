using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Payez N 🩸 : <effets> » au niveau IEffet — analogue de EffetOptionnelPayant / EffetChoixPayant, mais
// les branches sont des IEffet (et non des Actions). Si payé → exécute SiPaye ; sinon → exécute Base.
//   - Base VIDE = la branche payée S'AJOUTE (la base de la carte est portée par un Effet voisin),
//     ex. Cadopourrix (« Payez 4 : révélez une Créature et ajoutez-la au sort » = EffetRevelerPioche).
//   - Base NON VIDE = on joue SOIT la base SOIT la branche payée (à la place),
//     ex. Sabruledepartoux (« 4 au contrôleur » de base, ou « 8 au contrôleur + 4 à ses voisins » si payé).
public class EffetPayantEffet : IEffet
{
    public int Cout { get; set; }
    public string Libelle { get; set; } = "";
    public List<IEffet> Base { get; set; } = [];
    public List<IEffet> SiPaye { get; set; } = [];

    public void Execute(GameContext context)
    {
        var effets = context.TenterPayer(Cout, Libelle) ? SiPaye : Base;
        foreach (var effet in effets)
            effet.Execute(context);
    }
}
