using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMob", menuName = "NPC/Mob")]
public class MobData : ScriptableObject
{
    public enum MobCategory { Undead, Beast, Demon, Humanoid, Vampire }
    public enum DeckGenerationType { Fixed, Random, Hybrid }

    [Header("Podstawowe Informacje")]
    public string mobName;
    public MobCategory category;

    [Header("Statystyki")]
    public int maxHP;
    public int startArmor; // <--- DODANE: Pocz¹tkowy pancerz potwora
    public int attackBonus;
    public int level;
    public int minCoins;
    public int maxCoins;

    [Header("Zdolnoœci Specjalne")]
    public bool hasLifesteal = false;
    [Range(0.1f, 2f)]
    [Tooltip("Ile procent zadanych obra¿eñ w HP wraca jako leczenie (np. 0.5 = 50%, 1.0 = 100%)")]
    public float lifestealPercentage = 1.0f;

    [Header("Konfiguracja Talii")]
    public DeckGenerationType deckType;
    public List<Card> fixedDeck;
    public List<Card> randomCardPool;
    public int randomCardCount;

    [Header("Wizualia i Animacje")]
    public Sprite mobGraphic;
    public AudioClip attackSound;
    public AudioClip damageSound;
    public RuntimeAnimatorController animatorController;
}