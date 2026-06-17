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
    SansTresor,        // adversaires sans aucun Tresor (ensemble)
    ATresor,           // adversaires possedant >= 1 Tresor (ensemble)
    PlusDeSang,
    ACreature,
    SansCreature,
    PlusDeCreatures,
    ADonjon,
    SansDonjon,
    AJetonDernierSurvivant,
    TousAdversaires,
    AutreAdversaire,   // un adversaire different de la derniere cible affectee ce sort
    AutresAdversaires, // TOUS les adversaires sauf la derniere cible (« chaque autre adversaire »)
    MemeCible,         // la derniere cible affectee ce sort (« cet adversaire »)
    AdversaireAuChoix, // « choisissez un adversaire » : designe par le lanceur (via ChoisirCible)
    DesigneParDe,
    ControleurDonjon,  // le sorcier qui controle le Donjon (PEUT etre le lanceur ; 0 ou 1 ; Sabruledepartoux)
    TousSorciers,      // TOUS les sorciers vivants, lanceur INCLUS (≠ TousAdversaires qui l'exclut)
    Soi
}

public enum TypeAction
{
    Degats,
    Soin,
    GagnerSang,
    VolerSang,
    PerdreSang,
    DonnerSang,        // transfert lanceur → cible (inverse de VolerSang)
    GagnerTresor,
    VolerTresor,
    DefausserTresor,
    PrendreDonjon,
    DonnerDonjon,      // donne le Donjon à la cible (≠ PrendreDonjon qui le donne au lanceur)
    RevelerMain,
    DefausserCartes,
    PasserCartes,
    GagnerCarte,
    TuerCreature,
    AutoDegats,
    PvAUn,
    LancerDeMemorise,  // lance 1 dé et mémorise le résultat dans DernierDe (réutilisable via ValeurDernierDe)
    AjouterBonusDe,    // ajoute `montant` dé(s) aux Jets de puissance du lanceur ce tour (BonusDesJetCreature)
    Garder             // GARDEZ la Créature en cours hors tranche (ex. Gracula « Payez 1 🩸 : GARDEZ »)
}

// Ou va une carte revelee retenue par EffetRevelerPioche (Peutidardus/Cadopourrix → Sort).
public enum DestinationRevele
{
    Sort,
    Main
}

// Sorcier de reference d'une Valeur (ex. ValeurParSang : « votre » Sang vs « son » Sang).
//   Lanceur       = celui qui lance le sort.
//   Cible         = la cible de l'Action en cours.
//   DerniereCible = la derniere cible affectee ce sort (« cet adversaire »), pour les renvois croises.
public enum SourceSorcier
{
    Lanceur,
    Cible,
    DerniereCible
}
