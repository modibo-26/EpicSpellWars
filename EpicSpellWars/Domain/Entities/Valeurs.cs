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

// « n par jeton Dernier Survivant en jeu » (global, tous sorciers ; ex. Necrophilus, Repos Mérité)
public class ValeurParJeton(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) =>
        parUnite * context.Sorciers.Sum(s => s.JetonsDernierSurvivant);
}

// « n par carte choisie » : lit le nb de cartes du dernier choix de main (DerniereQuantite).
// L'Action de choix (DefausserCartes) doit s'exécuter AVANT pour alimenter le compte.
// Ex. Mortalriktus dégâts = nb défaussé ; Sarabandus soin = 1 PV / carte (parUnite=1).
public class ValeurQuantiteChoisie(int parUnite) : IValeur
{
    public int Calculer(GameContext context, Sorcier cible) => parUnite * context.DerniereQuantite;
}
