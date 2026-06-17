using UnityEngine;
using System.Collections.Generic;

public enum PlayerClassType { None, Warrior, Rogue, Cleric }

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Player/Class")]
public class PlayerClassData : ScriptableObject
{
    [Header("Informacje o Klasie")]
    public string className;
    public PlayerClassType classType;
    public Sprite classIcon;

    [Header("Statystyki Bazowe Klasy")]
    public int baseMaxHP = 5;
    public int baseAttackTokens = 3;
    public int baseDefenseTokens = 0;
    public int baseMaxBet = 2;

    [Header("Ekwipunek Startowy (Za³o¿ony)")]
    public ItemData startingWeapon;
    public ItemData startingShield;
    public ItemData startingHelmet;
    public ItemData startingArmor;
    public ItemData startingTrinket;
    public ItemData startingRing;
    public ItemData startingPotion;
    public ItemData startingBoots;
    public ItemData startingGloves;

    [Header("Startowy Plecak (Dodatkowe przedmioty)")]
    [Tooltip("Dodaj tu przedmioty, które klasa ma mieæ w plecaku (np. 3x Mikstura)")]
    public List<ItemData> startingBackpack = new List<ItemData>();

    [Header("Faza 1: Zdrowy (Powy¿ej 50% HP)")]
    public Sprite idleSprite;
    public Sprite blinkSprite;
    public Sprite hurtSprite;

    [Header("Faza 2: Ranny (Poni¿ej 50% HP)")]
    public Sprite woundedIdleSprite;
    public Sprite woundedBlinkSprite;
    public Sprite woundedHurtSprite;

    [Header("Faza 3: Œmieræ (0 HP)")] // DODANE: Miejsce na grafikê po przegranej
    public Sprite deadSprite;
}