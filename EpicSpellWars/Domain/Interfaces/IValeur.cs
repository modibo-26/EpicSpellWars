using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Domain.Interfaces;

public interface IValeur
{
    int Calculer(GameContext context, Sorcier cible);
}
