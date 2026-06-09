using EpicSpellWars.Domain.Entities;

namespace EpicSpellWars.Domain.Interfaces;

public interface IEffet
{
    void Execute(GameContext context);
}
