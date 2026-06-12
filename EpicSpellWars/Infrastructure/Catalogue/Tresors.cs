using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Infrastructure.Catalogue;

// Les 25 Tresors (permanents face visible, chacun en 1 exemplaire). Texte = data/tresors.json (charge
// par TexteLoader, jointure Id). Effets = [] : AUCUN Tresor n'est un effet de resolution de sort -
// tous sont passifs / au tour d'Initiative / capacites activees (« Payez X 🩸 »), donc du ressort du
// 2e PILIER (declencheurs), pas encore construit. Seul le TriggerType est encode ici.
//   SurInitiative = se declenche a votre tour / sur l'ordre du tour (premier/dernier a jouer, egalite).
//   Immediat      = effet one-shot « Lorsque vous gagnez ce Tresor ».
//   Passif        = reactif/conditionnel permanent ou capacite activee payante.
public static class Tresors
{
    public static List<Tresor> Toutes() =>
    [
        new("Braguette de Cthulhu", [], TriggerType.SurInitiative) { Id = "EP2-146" },
        new("Gang du Gong", [], TriggerType.Passif) { Id = "EP2-154" },
        new("Divan le Terrible", [], TriggerType.Passif) { Id = "EP2-155" },
        new("Dés Pipés", [], TriggerType.Passif) { Id = "EP2-156" },
        new("Chalisman", [], TriggerType.Passif) { Id = "EP2-157" },
        new("Vers Pas Solitaires", [], TriggerType.SurInitiative) { Id = "EP2-158" },
        new("Dissuasion Nucléaire", [], TriggerType.Passif) { Id = "EP2-159" },
        new("Baguette Bicéphale", [], TriggerType.Passif) { Id = "EP2-160" },
        new("Manuel de Cryptozoic", [], TriggerType.SurInitiative) { Id = "EP2-161" },
        new("Pièces du Destin", [], TriggerType.Immediat) { Id = "EP2-162" },
        new("Buffet à Volonté", [], TriggerType.Passif) { Id = "EP2-163" },
        new("Nachos de la Rage", [], TriggerType.Passif) { Id = "EP2-164" },
        new("Bœuf aux Hormones", [], TriggerType.Passif) { Id = "EP2-165" },
        new("Coupe du Tocard", [], TriggerType.Passif) { Id = "EP2-167" },
        new("Bébé Monstre", [], TriggerType.Immediat) { Id = "EP2-168" },
        new("Menottes d'Avarice", [], TriggerType.SurInitiative) { Id = "EP2-169" },
        new("Fusil à Triple Canon", [], TriggerType.Passif) { Id = "EP2-171" },
        new("Avis de Recherche", [], TriggerType.Immediat) { Id = "EP2-172" },
        new("Mains Poisseuses !", [], TriggerType.SurInitiative) { Id = "EP2-173" },
        new("Liste du Père Fouettard", [], TriggerType.Passif) { Id = "EP2-174" },
        new("Chipodada", [], TriggerType.Passif) { Id = "EP2-175" },
        new("Granoloup", [], TriggerType.Passif) { Id = "EP2-176" },
        new("Bouclier Anti-Fiente", [], TriggerType.Passif) { Id = "EP2-177" },
        new("Globe Sacrificiel", [], TriggerType.Passif) { Id = "EP2-178" },
        new("Smoking de Location", [], TriggerType.SurInitiative) { Id = "EP2-179" },
    ];
}
