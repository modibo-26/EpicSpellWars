using EpicSpellWars.Domain.Enums;

namespace EpicSpellWars.Domain.Entities;

public class CarteSort : Carte
{
    public Glyphe Glyphe { get; set; }

    public int Initiative { get; set; }

    public TypeComposant? Type { get; set; }

    public bool EstCreature => Type == TypeComposant.Destination;


    public CarteSort(string nom, TypeComposant? type, Glyphe glyphe, int initiative = 0) : base(nom)
    {
        Type = type;
        Glyphe = glyphe;
        Initiative = initiative;
    }
}
