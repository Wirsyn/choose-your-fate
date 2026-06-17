using UnityEngine;

public enum CardSuit { Pik, Kier, Trefl, Karo }
public enum CardEffect { Brak, Atak, Obrona, Leczenie }

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Card")]
public class Card : ScriptableObject
{
    public int value;
    public CardSuit color;
    public CardEffect effect;
    public Sprite graphic;
    public bool isAce;
}