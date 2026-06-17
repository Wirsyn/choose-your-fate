using UnityEngine;

[CreateAssetMenu(fileName = "NewMeal", menuName = "Player/Meal")]
public class MealData : ScriptableObject
{
    [Header("Podstawowe informacje")]
    public string mealName;
    [TextArea] public string description;
    public Sprite mealIcon;
    public int buyPrice = 15;

    [Header("Statystyki przywracania (na pocz¹tku bitwy)")]
    public int restoreHP = 0;
    public int restoreArmor = 0;

    [Header("Bonusy na nastêpn¹ walkê")]
    public int bonusAttackTokens = 0;
    public int bonusDefenseTokens = 0;
    public int bonusBaseDamage = 0;
}