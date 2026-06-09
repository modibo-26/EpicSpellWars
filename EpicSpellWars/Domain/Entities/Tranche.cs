namespace EpicSpellWars.Domain.Entities;

// Une tranche de resultat de des : on retient la tranche au plus grand Seuil <= somme.
// La borne haute est ouverte par nature (pas de max artificiel).
public class Tranche
{
    public int Seuil { get; set; }   // borne basse atteinte : 1, puis 5, puis 10... (Depipax : 1, 3, 5)
    public List<Action> Actions { get; set; } = [];
}
