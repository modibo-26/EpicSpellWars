using EpicSpellWars.Domain.Entities;
using EpicSpellWars.Domain.Enums;
using Action = EpicSpellWars.Domain.Entities.Action;

namespace EpicSpellWars.Tests;

// Boucle de résolution d'un sort (GameContext. ResoudreSort) : ordre de lecture Source→Qualité→Destination
// et carte ajoutée en cours de résolution résolue à son rang de type.
public class ResoudreSortTests
{
    // 12) Un Sort donné en désordre [Destination, Qualité, Source]. La Qualité ajoute une Source de la main
    //     (GagnerCarte) qui se résout AVANT la Destination. Jet 10+ : 4 dégâts à gauche + GARDEZ.
    //     Dégâts à Gandalf : Source 2 + Source ajoutée 1 + Destination 4 = 7.
    [Fact]
    public void Resout_dans_l_ordre_de_type_y_compris_les_cartes_ajoutees()
    {
        var t = new Table { ProchainDe = 10 };

        var source0 = new CarteSort("Source-A", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(2) }] }],
        };
        var bonusSource = new CarteSort("Source-bonus", TypeComposant.Source, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(1) }] }],
        };
        t.Merlin.Main.Add(bonusSource);   // tirée dans le sort par la Qualité
        var qualite = new CarteSort("Qualité-A", TypeComposant.Qualite, Glyphe.Arcane)
        {
            Effets = [new EffetSimple { Actions = [new Action { Type = TypeAction.GagnerCarte, Cible = Cible.Soi, MinCartes = 1, FiltreCarte = c => c.Type == TypeComposant.Source }] }],
        };
        var destination = new CarteSort("Créature-A", TypeComposant.Destination, Glyphe.Arcane, initiative: 8)
        {
            Effets =
            [
                new EffetJetDePuissance
                {
                    Glyphe = Glyphe.Arcane,
                    Tranches = [new TrancheJetDePuissance { Seuil = 10, PeutGarder = true, Actions = [new Action { Type = TypeAction.Degats, Cible = Cible.AdversaireGauche, Valeur = new ValeurFixe(4) }] }],
                },
            ],
        };
        t.Ctx.SortEnCours = [destination, qualite, source0];   // désordre volontaire

        t.Ctx.ResoudreSort();

        Assert.Equal(13, t.Gandalf.PointsDeVie);              // 20 - (2 + 1 + 4)
        Assert.Contains(destination, t.Merlin.Creatures);     // GARDEZ
        Assert.Empty(t.Merlin.Main);                          // la Source bonus a été tirée dans le sort
    }
}
