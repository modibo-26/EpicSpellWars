using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// Valeur fixe : « 3 dégâts »
public class ValeurFixe(int montant) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) => montant;
}

// Valeur au dé : « lancez X dés, sommez » (ex. castoramax : 2 dés)
public class ValeurDe(int nbDes) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible)
    {
        var total = 0;
        for (var i = 0; i < nbDes; i++)
            total += context.LancerDe();
        return total;
    }
}

// « n par Créature de la cible »
public class ValeurParCreature(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) => parUnite * cible.Creatures.Count;
}

// « n par sorcier mort »
public class ValeurParMort(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * context.Sorciers.Count(s => !s.EstVivant);
}

// « n par sorcier VIVANT dont les PV sont <= seuil » (Bébé Monstre : +2 🩸 par sorcier à 5 PV ou moins).
public class ValeurParSorcierFaible(int parUnite, int seuil) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * context.Sorciers.Count(s => s.EstVivant && s.PointsDeVie <= seuil);
}

// « n par jeton Dernier Survivant en jeu » (global, tous sorciers ; ex. Necrophilus, Repos Mérité)
public class ValeurParJeton(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * context.Sorciers.Sum(s => s.JetonsDernierSurvivant);
}

// « n par Trésor en jeu » (global, tous sorciers ; ex. Trésormodix payé)
public class ValeurParTresorEnJeu(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * context.Sorciers.Sum(s => s.Tresors.Count);
}

// « n par niveau de 🩸 » d'un sorcier de référence (Asphixis : votre Sang = Lanceur ; son Sang = DerniereCible).
public class ValeurParSang(int parUnite, SourceSorcier source) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * source switch
        {
            SourceSorcier.Lanceur => context.Lanceur.Sang,
            SourceSorcier.DerniereCible => context.DerniereCible?.Sang ?? 0,
            _ => cible.Sang,
        };
}

// « n par Créature dans la MAIN de la cible » (≠ ValeurParCreature qui compte les Créatures EN JEU ; ex. Poilcramus)
public class ValeurParCreatureEnMain(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * cible.Main.Count(c => c.EstCreature);
}

// « n par point du dé mémorisé » : relit DernierDe (posé par LancerDeMemorise) — un seul tirage,
// plusieurs lectures (ex. Trankilus : gain de Sang = dé, puis soin = même dé).
public class ValeurDernierDe(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) => parUnite * context.DernierDe;
}

// « n par carte choisie » : lit le nb de cartes du dernier choix de main (DerniereQuantite).
// L'Action de choix (DefausserCartes) doit s'exécuter AVANT pour alimenter le compte.
// Ex. Mortalriktus dégâts = nb défaussé ; Sarabandus soin = 1 PV / carte (parUnite=1).
public class ValeurQuantiteChoisie(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) => parUnite * context.DerniereQuantite;
}

// « <valeur A> ou <valeur B> si <condition> » : montant conditionnel selon l'état de jeu (≠ EffetConditionnel
// qui branche des Actions ; ici on branche la seule VALEUR d'une Action, à l'intérieur d'une tranche par ex.).
// Beeeh-zerker 10+ : « 3 dégâts ou 6 si c'est votre dernier adversaire ».
public class ValeurConditionnelle(Predicate<GameContext> condition, IValeur siVrai, IValeur siFaux) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        (condition(context) ? siVrai : siFaux).Calculer(context, cible);
}
