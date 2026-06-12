using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class Action
{
    public TypeAction Type { get; set; }
    public Cible Cible { get; set; }
    public IValeur? Valeur { get; set; }   // null pour les actions sans quantité (PrendreDonjon, TuerCreature...)

    // « Adversaire qui... » au SINGULIER : le lanceur choisit UN seul match du filtre Cible (via ChoisirCible),
    // au lieu de tout l'ensemble. Pour les Destinations dont la Cible principale est une filtrante (ADejaJoue,
    // SansDonjon, SansCreature...). Sans effet si le filtre renvoie déjà 0 ou 1 cible.
    public bool CibleUnique { get; set; }

    // Choix de cartes dans une main (GagnerCarte / DefausserCartes) :
    // - FiltreCarte : cartes éligibles (ex. Cadopourrix → c.Type == Destination ; Shub → c.EstCreature). null = toutes.
    // - MinCartes : borne basse du choix ; le MAX est le montant issu de Valeur (défaut 1).
    //   « ajoutez 1 carte » → Min=1, Valeur=null (max 1) ; « jusqu'à 3 » → Min=0, Valeur=ValeurFixe(3).
    public Predicate<CarteSort>? FiltreCarte { get; set; }
    public int MinCartes { get; set; }

    // Mortalriktus : demande d'abord un TypeComposant (hook ChoisirTypeComposant) et ne garde que
    // les cartes de ce type ; le max devient « toutes les cartes du type » (montant ignore).
    public bool TypeAuChoix { get; set; }
}
