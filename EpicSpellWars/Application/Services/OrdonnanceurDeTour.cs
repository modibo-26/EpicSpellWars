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

        // Memorise les sorts declares + reset de l'etat de resolution a portee tour (Reactions cross-wizard).
        ctx.PreparerTour(sorts);

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

            // Trésors « SurInitiative » au DÉBUT du tour de leur porteur (Braguette de Cthulhu). Les Trésors
            // SurInitiative conditionnés à l'ORDRE (premier/dernier à jouer) gardent Effets=[] → inertes ici ;
            // ils se brancheront à leur place propre (cf. GAP tranche D).
            foreach (var tresor in lanceur.Tresors.Where(t => t.TriggerType == TriggerType.SurInitiative).ToList())
                ctx.DeclencherTresor(tresor, lanceur);

            ctx.SortEnCours = [..sorts[lanceur]];   // copie : ResoudreSort/nettoyage ne touchent pas l'entree

            ctx.ResoudreSort();

            // Nettoyage fin de sort : composants non gardes → Defausse. Les Creatures gardees sont deja
            // dans Lanceur.Creatures (via GarderCreatureEnCours) ; on ne les defausse pas.
            foreach (var composant in ctx.SortEnCours.Where(composant => !lanceur.Creatures.Contains(composant)))
                ctx.Defausse.Add(composant);
            ctx.SortEnCours = [];

            lanceur.ADejaJoueCeTour = true;
        }

        // Fin de tour : le contrôleur du Donjon le conserve et gagne +1 Sang ([[donjon-controle]]). S'applique
        // même s'il est mort (le Sang persiste). Sorcier sous Terre / Chalisman modifieront ce gain (tranche D/E).
        if (ctx.ControleurDonjon is { } gardien)
            gardien.Sang = Math.Min(gardien.SangMax, gardien.Sang + 1);

        ctx.Tour++;
        return ordre;
    }

    // Début de manche : le Donjon est remis au centre (personne ne le contrôle) et le compteur avance.
    // Reset transverse ([[donjon-controle]]) ; appelé par la boucle de manche (à venir).
    public void DebutManche(GameContext ctx)
    {
        ctx.ControleurDonjon = null;
        ctx.Manche++;
        ctx.ReinitialiserJetonDernierSurvivant();   // nouvelle bataille → un nouveau jeton Dernier Survivant en jeu
        // Effets différés « au début de la prochaine manche » (Sorciers crevés MancheSuivante, etc., tranche E).
        ctx.DeclencherEffetsDifferes();
    }

    // Initiative du sort = celle de sa Destination ; 0 sans Destination.
    // TODO: Magie feroce en Destination = Initiative 0 (a affiner avec le type Magie feroce).
    private static int InitiativeDe(List<CarteSort> sort) =>
        sort.FirstOrDefault(c => c.Type == TypeComposant.Destination)?.Initiative ?? 0;
}
