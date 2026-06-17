using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Blacksmith/Card Upgrade")]
public class CardUpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    public Sprite icon;
    public Color cardTintColor = Color.white;

    public enum UpgradeType { None, Attack, Blood, Crit, Defense, Fire, Heal, Ice, Lifesteal, Poison }
    public UpgradeType upgradeType;
    public enum ActivationPhase { OnDraw, OnWin }
    public ActivationPhase phase;

    public int effectValue = 1;

    [Header("Efekty Wizualne i DŸwiêkowe")] // Zmieniono nazwê nag³ówka
    public GameObject effectPrefab;
    public float effectDuration = 1.5f;
    public AudioClip effectSound; // <--- DODANE: DŸwiêk ulepszenia
}