using EpicSpellWars.Domain.Enums;
using EpicSpellWars.Domain.Interfaces;

namespace EpicSpellWars.Domain.Entities;

public class SorcierCreve(string nom, List<IEffet> effets, TriggerType triggerType) : Carte(nom, effets)
{
    public TriggerType TriggerType { get; } = triggerType;

    // Passif data-driven (lu au goulot concerne, comme Tresor.BonusSangParKill) :
    //   GainDonjonMortFinTour : si le porteur MORT controle le Donjon en fin de tour, son gain de Sang vaut ce
    //     montant AU LIEU de 1 (Sorcier sous Terre = 4). 0 = pas d'override.
    //   BonusProchainJetCreature : des ajoutes au prochain Jet de Creature du porteur (Petit Ange = 1),
    //     appliques au sorcier des que le crevé est pioché (TriggerType.Passif).
    public int GainDonjonMortFinTour { get; init; }
    public int BonusProchainJetCreature { get; init; }
}
