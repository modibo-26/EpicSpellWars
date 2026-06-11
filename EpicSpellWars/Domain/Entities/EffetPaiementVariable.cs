using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

// « Payez X 🩸 : <effet utilisant X> » (Pèredodux). Le lanceur choisit X, on debite, et X est place
// dans DerniereQuantite pour qu'une Valeur le lise (ValeurQuantiteChoisie). Rien si X = 0.
public class EffetPaiementVariable : IEffet
{
    public string Libelle { get; set; } = "";
    public List<Action> SiPaye { get; set; } = [];

    public void Execute(GameContext context)
    {
        var x = context.TenterPayerVariable(Libelle);
        context.DerniereQuantite = x;
        if (x <= 0)
            return;
        foreach (var action in SiPaye)
            context.Appliquer(action);
    }
}
