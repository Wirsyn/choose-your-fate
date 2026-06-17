using UnityEngine;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI itemStatsText;

    [Header("Pole Wytrzyma³oœci")]
    public TextMeshProUGUI itemDurabilityText;

    void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (itemDurabilityText != null) itemDurabilityText.gameObject.SetActive(false);
    }

    public void ShowTooltip(ItemData item)
    {
        if (item == null || tooltipPanel == null) return;

        string upgString = item.upgradeLevel > 0 ? $" +{item.upgradeLevel}" : "";
        itemNameText.text = item.itemName + upgString;
        itemDescText.text = item.description;

        string stats = "";

        if (item.isStackable && item.currentStack > 1) stats += $"Quantity: {item.currentStack}\n";

        if (item.slotType != EquipmentSlot.Potion && item.slotType != EquipmentSlot.SmallItem && itemDurabilityText != null)
        {
            itemDurabilityText.text = $"{item.currentDurability} / {item.maxDurability}";
            itemDurabilityText.gameObject.SetActive(true);
        }
        else if (itemDurabilityText != null)
        {
            itemDurabilityText.gameObject.SetActive(false);
        }

        if (item.bonusAttackTokens > 0) stats += $"Attack Tokens: +{item.bonusAttackTokens}\n";
        if (item.bonusDefenseTokens > 0) stats += $"Defense Tokens: +{item.bonusDefenseTokens}\n";
        if (item.bonusMaxHP > 0) stats += $"Max HP: +{item.bonusMaxHP}\n";
        if (item.bonusMaxBet > 0) stats += $"Max Bet: +{item.bonusMaxBet}\n";
        if (item.bonusBaseDamage > 0) stats += $"Weapon Damage: +{item.bonusBaseDamage}\n";
        if (item.restoreHP > 0) stats += $"Restores: {item.restoreHP} HP\n";
        if (item.restoreArmor > 0) stats += $"Restores: {item.restoreArmor} Armor\n";
        if (item.bonusGoldMultiplier > 0f)
        {
            int goldPercentage = Mathf.RoundToInt(item.bonusGoldMultiplier * 100);
            stats += $"Gold Gain: +{goldPercentage}%\n";
        }

        // --- DODANE: Opisy dla nowych mechanik ---
        if (item.hasDeathDefiance) stats += $"<color=#FFD700>Death Defiance (50% Chance)</color>\n";
        if (!item.isUpgradable) stats += $"<color=#FF5555>Not Upgradable</color>\n";

        itemStatsText.text = stats;
        tooltipPanel.SetActive(true);
    }

    public void ShowTooltipMeal(MealData meal)
    {
        if (meal == null || tooltipPanel == null) return;

        itemNameText.text = meal.mealName;
        itemDescText.text = meal.description;

        if (itemDurabilityText != null) itemDurabilityText.gameObject.SetActive(false);

        string stats = "";

        if (meal.restoreHP > 0) stats += $"Restores: {meal.restoreHP} HP\n";
        if (meal.restoreArmor > 0) stats += $"Restores: {meal.restoreArmor} Armor\n";
        if (meal.bonusAttackTokens > 0) stats += $"Bonus Attack Tokens: +{meal.bonusAttackTokens}\n";
        if (meal.bonusDefenseTokens > 0) stats += $"Bonus Defense Tokens: +{meal.bonusDefenseTokens}\n";
        if (meal.bonusBaseDamage > 0) stats += $"Bonus Damage: +{meal.bonusBaseDamage}\n";

        itemStatsText.text = stats;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (itemDurabilityText != null) itemDurabilityText.gameObject.SetActive(false);
    }
}