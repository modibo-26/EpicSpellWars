using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Tests;

// Fixture de test : une table de 3 sorciers (Merlin lanceur, Gandalf à gauche, Saroumane à droite)
// avec un GameContext branche sur des hooks deterministes et pilotables.
//   - ProchainDe : valeur renvoyee par LancerDe
//   - ProchainChoix : reponse oui/non de ChoisirOption
//   - ProchainType : type renvoye par ChoisirTypeComposant
//   - ChoisirCible prend le 1er candidat ; ChoisirCartes prend les `max` premieres cartes.
internal sealed class Table
{
    public readonly Sorcier Merlin = new("Merlin");
    public readonly Sorcier Gandalf = new("Gandalf");
    public readonly Sorcier Saroumane = new("Saroumane");
    public readonly GameContext Ctx;

    public int ProchainDe = 1;
    public bool ProchainChoix = true;
    public TypeComposant ProchainType = TypeComposant.Source;
    public bool ProchainPaye = false;   // reponse de ChoisirPayer
    public int ProchainMontant = 0;     // montant de ChoisirMontant (« Payez X »)
    public int ProchainOption = 0;      // index renvoye par ChoisirOptionLanceur (« choisissez une option »)

    // Strategie de declaration de sort (flux de manche) : par defaut, aucun sort. Un test la surcharge pour
    // faire jouer des composants depuis la main d'un sorcier (l'ordonnanceur les retire de la main).
    public Func<Sorcier, IReadOnlyList<CarteSort>> Declaration = _ => [];

    public int ProchainIndexHasard = 0;   // index renvoye par ChoisirIndexAuHasard (effets « au hasard »)

    // Hasard SEEDE (Fisher-Yates) pour Melanger : l'ordre est reellement brasse (fidele) ET reproductible
    // (rejouable) — JAMAIS l'identite ([[pioche-non-melangee]]). Graine fixe → tests deterministes.
    private readonly Random _rng = new(20260630);

    // Strategie d'activation des Tresors (tour d'Initiative) : par defaut, aucune activation. Un test la
    // surcharge (ex. activer le 1er Tresor activable).
    public Func<Sorcier, IReadOnlyList<Tresor>, Tresor?> ActivationTresor = (_, _) => null;

    public Table()
    {
        Ctx = new GameContext
        {
            Lanceur = Merlin,
            Sorciers = [Merlin, Gandalf, Saroumane],   // ordre de table : « gauche » = case suivante
            ChoisirCible = candidats => candidats.First(),
            LancerDe = () => ProchainDe,
            ChoisirDe = des => des.Max(),   // Castoramax : dé offensif = le plus grand (optimal)
            ChoisirCartes = (candidats, _, max) => candidats.Take(max).ToList(),
            ChoisirOption = (_, _) => ProchainChoix,
            ChoisirOptionLanceur = (_, _) => ProchainOption,
            ChoisirTypeComposant = (_, types) => types.Contains(ProchainType) ? ProchainType : types.First(),
            ChoisirPayer = (_, _, _) => ProchainPaye,
            ChoisirMontant = _ => ProchainMontant,
            DeclarerSort = s => Declaration(s),
            ChoisirIndexAuHasard = _ => ProchainIndexHasard,
            ChoisirActivationTresor = (s, ts) => ActivationTresor(s, ts),
            Melanger = cartes => MelangerSeed(cartes, _rng),
        };
    }

    // Fisher-Yates seede : brassage reel et reproductible (cf. _rng).
    private static List<Carte> MelangerSeed(IReadOnlyList<Carte> cartes, Random rng)
    {
        var liste = cartes.ToList();
        for (var i = liste.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (liste[i], liste[j]) = (liste[j], liste[i]);
        }
        return liste;
    }
}
