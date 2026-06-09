using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class SorcierCreve(string nom, List<IEffet> effets, TriggerType triggerType) : Carte(nom, effets)
{
    public TriggerType TriggerType { get; } = triggerType;
}
