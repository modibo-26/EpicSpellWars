namespace EpicSpellWars.Domain.Enums;

public enum Cible
{
    AdversaireGauche,
    AdversaireDroite,
    DeuxVoisins,
    PlusFort,
    PlusFaible,
    PlusFortQueMoi,
    PlusFaibleQueMoi,
    ADejaJoue,
    NaPasJoue,
    PlusDeTresors,
    PlusDeSang,
    ACreature,
    SansCreature,
    PlusDeCreatures,
    ADonjon,
    SansDonjon,
    AJetonDernierSurvivant,
    TousAdversaires,
    AutreAdversaire,   // un adversaire different de la derniere cible affectee ce sort
    MemeCible,         // la derniere cible affectee ce sort (« cet adversaire »)
    AdversaireAuChoix, // « choisissez un adversaire » : designe par le lanceur (via ChoisirCible)
    DesigneParDe,
    Soi
}

public enum TypeAction
{
    Degats,
    Soin,
    GagnerSang,
    VolerSang,
    PerdreSang,
    GagnerTresor,
    VolerTresor,
    DefausserTresor,
    PrendreDonjon,
    RevelerMain,
    DefausserCartes,
    PasserCartes,
    GagnerCarte,
    TuerCreature,
    AutoDegats,
    PvAUn
}

// Ou va une carte revelee retenue par EffetRevelerPioche (Peutidardus/Cadopourrix → Sort).
public enum DestinationRevele
{
    Sort,
    Main
}
