namespace EpicSpellWars.Domain.Enums;

public enum TypeComposant
{
    Source,
    Qualite,
    Destination,
}

public enum Glyphe
{
    Arcane,
    Tenebres,
    Elementaire,
    Illusion,
    Primaire,
    Aucun,
}

public enum TriggerType
{
    Immediat,
    MancheSuivante,
    Passif,
    SurInitiative
}

// Phases du tour portant des clauses de Trésor (pipeline ordonné dans OrdonnanceurDeTour) :
//   DebutTour = au tour d'Initiative du porteur, AVANT son sort (ex. « début de votre tour », « si premier à jouer »).
//   FinTour   = une fois, après que tous les sorciers ont joué (ex. « si dernier à jouer », « si le plus faible »).
// La phase Standby (activation payante) et la résolution du sort sont des étapes à part, intercalées par
// l'ordonnanceur. Les déclencheurs d'ÉVÉNEMENT (mort/kill/pioche de crevé) restent au goulot, hors pipeline.
public enum PhaseTour
{
    DebutTour,
    FinTour
}
