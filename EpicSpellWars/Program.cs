namespace EpicSpellWars;

// Point d'entree. Le moteur (Domain + Application) est exerce par le projet de tests EpicSpellWars.Tests
// (les anciennes demos console y ont ete converties en tests assertes). La vraie boucle de jeu / UI
// viendra brancher OrdonnanceurDeTour ici quand le systeme de declencheurs et le loader seront prets.
internal static class Program
{
    private static void Main() => Console.WriteLine("EpicSpellWars — moteur prêt. Voir EpicSpellWars.Tests pour les scénarios.");
}
