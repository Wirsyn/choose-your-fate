using UnityEngine;

public enum EquipmentSlot { Weapon, Shield, Helmet, Armor, Trinket, Ring, Potion, Throwable, SmallItem, Boots, Gloves }

[CreateAssetMenu(fileName = "NewItem", menuName = "Player/Item")]
public class ItemData : ScriptableObject
{
    [Header("Informacje o przedmiocie")]
    public string itemName;
    [TextArea] public string description;
    public Sprite itemIcon;
    public EquipmentSlot slotType;

    [Tooltip("Ile kratek w plecaku zajmuje ten przedmiot")]
    public int slotsTaken = 1;

    [Header("Zarz¹dzanie stertami (Stacking)")]
    public bool isStackable = false;
    public int maxStack = 99;
    public int currentStack = 1;

    [Header("Wytrzyma³oœæ (Durability)")]
    public int maxDurability = 100;
    public int currentDurability = 100;

    [Header("Ekonomia Sklepu i Kowala")]
    public int buyPrice = 50;
    public int sellPrice = 25;
    [Tooltip("Czy ten przedmiot mo¿e byæ ulepszany u Kowala?")]
    public bool isUpgradable = true; // DODANE: Blokada ulepszania
    public int baseUpgradeCost = 100;
    public int upgradeLevel = 0;

    [Header("Bonusy do Statystyk (Pasywne)")]
    public int bonusAttackTokens = 0;
    public int bonusDefenseTokens = 0;
    public int bonusMaxHP = 0;
    public int bonusMaxBet = 0;
    public int bonusBaseDamage = 0;
    [Range(0f, 2f)] public float bonusGoldMultiplier = 0f;

    [Header("Zdolnoœci Specjalne Trinketów")]
    [Tooltip("Daje 50% szans na przetrwanie œmiertelnego ciosu (niszczy trinket)")]
    public bool hasDeathDefiance = false; // DODANE: Zdolnoœæ trinketu

    [Header("Przedmioty U¿ytkowe (Mikstury)")]
    public int restoreHP = 0;
    public int restoreArmor = 0;
    [Tooltip("DŸwiêk odtwarzany podczas zu¿ycia/wypicia przedmiotu")]
    public AudioClip consumeSound; // DODANE: DŸwiêk mikstury
}