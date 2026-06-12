using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Application.Services;

// Ordonnanceur de tour : ordonne les sorts du tour puis les resout un par un via
// GameContext.ResoudreSort, en nettoyant la fin de chaque sort.
//
// TRANCHE FINE volontaire : tri + boucle + nettoyage. Les recompenses (Sang, +3 au kill, vol du
// Donjon, jeton, bonus « Donjon : » de fin de tour) relevent du systeme de declencheurs (2e pilier)
// et ne sont PAS traitees ici — elles se brancheront par-dessus dans un commit dedie.
public class OrdonnanceurDeTour
{
    // Joue un tour : chaque sorcier ayant un sort le resout dans l'ordre reglementaire
    //   1. moins de Composants d'abord ;
    //   2. egalite → Initiative de la Destination la plus haute ;
    //   3. egalite → depart au de (plus haut resultat d'abord).
    // Renvoie l'ordre effectif de resolution (les sorciers qui ont joue).
    public IReadOnlyList<Sorcier> JouerTour(GameContext ctx, IReadOnlyDictionary<Sorcier, List<CarteSort>> sorts)
    {
        foreach (var s in ctx.Sorciers)
        {
            s.ADejaJoueCeTour = false;
            s.ADejaPayeCeTour = false;
        }

        var aJouer = ctx.Sorciers
            .Where(s => s.EstVivant && sorts.TryGetValue(s, out var sort) && sort.Count > 0)
            .ToList();

        // De de depart tire UNE seule fois par sorcier (sinon OrderBy reevaluerait la cle).
        var deDepart = aJouer.ToDictionary(s => s, _ => ctx.LancerDe());

        var ordre = aJouer
            .OrderBy(s => sorts[s].Count)
            .ThenByDescending(s => InitiativeDe(sorts[s]))
            .ThenByDescending(s => deDepart[s])
            .ToList();

        foreach (var lanceur in ordre)
        {
            if (!lanceur.EstVivant)
                continue;   // tue par un sort precedent ce tour → ne resout pas le sien

            ctx.Lanceur = lanceur;
            ctx.CreatureEnCours = null;
            ctx.BonusDesJetCreature = 0;   // bonus de dés (Shub payé) à portée du tour du lanceur
            ctx.SortEnCours = [..sorts[lanceur]];   // copie : ResoudreSort/nettoyage ne touchent pas l'entree

            ctx.ResoudreSort();

            // Nettoyage fin de sort : composants non gardes → Defausse. Les Creatures gardees sont deja
            // dans Lanceur.Creatures (via GarderCreatureEnCours) ; on ne les defausse pas.
            foreach (var composant in ctx.SortEnCours.Where(composant => !lanceur.Creatures.Contains(composant)))
                ctx.Defausse.Add(composant);
            ctx.SortEnCours = [];

            lanceur.ADejaJoueCeTour = true;
        }

        ctx.Tour++;
        return ordre;
    }

    // Initiative du sort = celle de sa Destination ; 0 sans Destination.
    // TODO: Magie feroce en Destination = Initiative 0 (a affiner avec le type Magie feroce).
    private static int InitiativeDe(List<CarteSort> sort) =>
        sort.FirstOrDefault(c => c.Type == TypeComposant.Destination)?.Initiative ?? 0;
}
