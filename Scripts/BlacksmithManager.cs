using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class BlacksmithManager : MonoBehaviour
{
    [Header("UI Gracza")]
    public TextMeshProUGUI playerCoinsText;
    public TextMeshProUGUI playerCoinsTextInv;
    private int currentCoins;
    public Sprite emptySlotIcon;

    [Header("Przejœcia i Ekrany")]
    public GameObject loadingPanel;
    public EquipmentManager eqManager;

    [Header("Etap 1: Wybór Buffa (Karty)")]
    public GameObject upgradesPanel;
    public List<CardUpgradeData> allPossibleUpgrades;
    public Button[] upgradeButtons;
    public TextMeshProUGUI[] upgradeNames;
    public Image[] upgradeIcons;

    [Header("Etap 2: Wybór Karty")]
    public GameObject cardSelectionPanel;
    public List<Card> allPossibleCards;
    public Button[] cardButtons;
    public TextMeshProUGUI[] cardNames;
    public Image[] cardGraphics;

    [Header("Etap 3: Sklep (Zakup Przedmiotów)")]
    public GameObject shopPanel;
    public List<ItemData> shopItemPool;
    public Button[] shopItemButtons;
    public TextMeshProUGUI[] shopItemNames;
    public Image[] shopItemIcons;
    public TextMeshProUGUI[] shopItemPrices;
    public TextMeshProUGUI[] shopItemStats;

    public int rerollCost = 20;
    public Button rerollButton;

    [Header("Etap 4: Ulepszanie Ekwipunku (Enhance)")]
    public GameObject enhancePanel;
    public Image enhanceSlotIcon;
    public TextMeshProUGUI enhanceCostText;
    public TextMeshProUGUI enhanceStatsBefore;
    public TextMeshProUGUI enhanceStatsAfter;
    public Button confirmEnhanceButton;
    private ItemData itemToEnhance;
    public GameObject enchancePanelCoinIcon;
    public GameObject enchancePanelArrow;
    public TextMeshProUGUI enchanceItemName;

    [Header("Etap 5: Sprzeda¿ Ekwipunku (Sell)")]
    public GameObject sellPanel;
    public Image sellSlotIcon;
    public TextMeshProUGUI sellPriceText;
    public TextMeshProUGUI sellItemName;
    public Button confirmSellButton;
    private ItemData itemToSell;
    public GameObject sellPanelCoin;

    // --- NOWE ETAPY ---
    [Header("Etap 6: Naprawa Ekwipunku (Repair)")]
    public GameObject repairPanel;
    public Image repairSlotIcon;
    public TextMeshProUGUI repairCostText;
    public TextMeshProUGUI repairItemName;
    public Button confirmRepairButton;
    public GameObject repairPanelCoin;
    public TextMeshProUGUI repairDurability;
    private ItemData itemToRepair;
    [Tooltip("Koszt naprawy 1 punktu wytrzyma³oœci")]
    public int repairCostPerPoint = 1;

    private ItemData[] currentShopItems = new ItemData[3];
    private CardUpgradeData selectedUpgrade;
    private Card selectedCard;

    private bool hasRolledUpgrades = false;
    private bool hasRolledShopItems = false;

    private List<Card> lockedCardsForUpgrade = null;
    private bool hasUpgradedCard = false;
    private bool hasEnhancedGear = false;

    

    void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        ClosePanels();

        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            currentCoins = SaveManager.instance.currentSave.savedPlayerCoins;
            if (eqManager != null) eqManager.LoadEquipment(SaveManager.instance.currentSave);
        }

        UpdateUI();
        DelayedEQRefresh();
    }

    public void RefreshCoinsFromSave()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            currentCoins = SaveManager.instance.currentSave.savedPlayerCoins;
            UpdateUI();
            RefreshShopButtonsInteractability();
        }
    }

    public void OpenUpgradesPanel()
    {
        if (hasUpgradedCard) return;
        ClosePanels();
        upgradesPanel.SetActive(true);
        if (!hasRolledUpgrades)
        {
            RollUpgrades();
            hasRolledUpgrades = true;
        }
    }

    public void OpenShopPanel()
    {
        ClosePanels();
        shopPanel.SetActive(true);
        if (!hasRolledShopItems)
        {
            RollShopItems();
            hasRolledShopItems = true;
        }
    }

    public void OpenEnhancePanel()
    {
        ClosePanels();
        enhancePanel.SetActive(true);
    }

    public void OpenSellPanel()
    {
        ClosePanels();
        sellPanel.SetActive(true);
    }

    public void OpenRepairPanel()
    {
        ClosePanels();
        repairPanel.SetActive(true);
    }

    // ----------------------------------------------------
    // LOKIGA REPAIR (NAPRAWA)
    // ----------------------------------------------------
    public void SetItemForRepair(ItemData item)
    {
        if (item == null || item.slotType == EquipmentSlot.Potion) return;

        if (itemToRepair != null) eqManager.AddToBackpack(itemToRepair);

        eqManager.RemoveItem(item);;
        itemToRepair = item;

        repairSlotIcon.sprite = item.itemIcon;
        repairPanelCoin.SetActive(true );

        int damageTaken = item.maxDurability - item.currentDurability;
        int cost = damageTaken * repairCostPerPoint;

        repairCostText.text = damageTaken > 0 ? cost.ToString() : "Fully repaired";
        repairItemName.text = $"{item.itemName}";
        repairDurability.text = $"{item.currentDurability} / {item.maxDurability}";

        if (confirmRepairButton != null) confirmRepairButton.interactable = (currentCoins >= cost && damageTaken > 0);
        DelayedEQRefresh();
    }

    public void ConfirmRepair()
    {
        if (itemToRepair == null) return;

        int damageTaken = itemToRepair.maxDurability - itemToRepair.currentDurability;
        int cost = damageTaken * repairCostPerPoint;

        if (currentCoins >= cost && damageTaken > 0)
        {
            currentCoins -= cost;
            itemToRepair.currentDurability = itemToRepair.maxDurability;

            eqManager.AddToBackpack(itemToRepair);
            itemToRepair = null;

            repairCostText.text = "Repaired!";
            if (confirmRepairButton != null) confirmRepairButton.interactable = false;

            SaveProgress();
            UpdateUI();
            DelayedEQRefresh();
        }
    }

    public void CancelRepair()
    {
        if (itemToRepair != null)
        {
            eqManager.AddToBackpack(itemToRepair);
            itemToRepair = null;
        }
        if (repairCostText != null) repairCostText.text = "";
        if (repairItemName != null) repairItemName.text = "Item name";
        DelayedEQRefresh();
    }

    // ----------------------------------------------------
    // LOKIGA ENHANCE (ULEPSZANIE)
    // ----------------------------------------------------
    public void SetItemForEnhance(ItemData item)
    {
        if (item == null) return;

        // --- DODANE: Zabezpieczenie przedmiotu, którego nie da siê ulepszyæ ---
        if (!item.isUpgradable)
        {
            Debug.Log("This item cannot be upgraded!");
            if (itemToEnhance != null) eqManager.AddToBackpack(itemToEnhance);
            itemToEnhance = null;
            enhanceSlotIcon.sprite = item.itemIcon;
            if (enchanceItemName != null) enchanceItemName.text = item.itemName;
            enhanceCostText.text = "LOCKED";
            enhanceStatsBefore.text = "This item cannot be upgraded.";
            enhanceStatsAfter.text = "";
            if (confirmEnhanceButton != null) confirmEnhanceButton.interactable = false;
            return;
        }

        if (itemToEnhance != null) eqManager.AddToBackpack(itemToEnhance);
        eqManager.RemoveItem(item);

        itemToEnhance = item;
        enhanceSlotIcon.sprite = item.itemIcon;
        if (enchanceItemName != null) enchanceItemName.text = item.itemName;

        int cost = item.baseUpgradeCost * (int)Mathf.Pow(2, item.upgradeLevel);
        enhanceCostText.text = cost.ToString();
        enhanceStatsBefore.text = GetItemStatsString(item);
        if (enchancePanelCoinIcon != null) enchancePanelCoinIcon.SetActive(true);
        if (enchancePanelArrow != null) enchancePanelArrow.SetActive(true);

        ItemData dummy = Instantiate(item);
        eqManager.ApplyUpgradeStats(dummy);
        dummy.upgradeLevel++;
        enhanceStatsAfter.text = GetItemStatsString(dummy);
        Destroy(dummy);

        if (confirmEnhanceButton != null) confirmEnhanceButton.interactable = (currentCoins >= cost) && !hasEnhancedGear;
        DelayedEQRefresh();
    }

    public void ConfirmEnhance()
    {
        if (itemToEnhance == null) return;
        int cost = itemToEnhance.baseUpgradeCost * (int)Mathf.Pow(2, itemToEnhance.upgradeLevel);

        if (currentCoins >= cost && !hasEnhancedGear)
        {
            currentCoins -= cost;
            hasEnhancedGear = true;

            eqManager.ApplyUpgradeStats(itemToEnhance);
            itemToEnhance.upgradeLevel++;

            eqManager.AddToBackpack(itemToEnhance);
            itemToEnhance = null;

            enhanceCostText.text = "Upgraded!";
            if (confirmEnhanceButton != null) confirmEnhanceButton.interactable = false;

            SaveProgress();
            UpdateUI();
            DelayedEQRefresh();
        }
    }

    public void CancelEnhance()
    {
        if (itemToEnhance != null)
        {
            eqManager.AddToBackpack(itemToEnhance);
            itemToEnhance = null;
        }
        if (enhanceStatsBefore != null) enhanceStatsBefore.text = "";
        if (enhanceStatsAfter != null) enhanceStatsAfter.text = "";
        if (enhanceCostText != null) enhanceCostText.text = "";
        if (enchanceItemName != null) enchanceItemName.text = "Item name";
        DelayedEQRefresh();
    }

    // ----------------------------------------------------
    // LOKIGA SELL (SPRZEDA¯)
    // ----------------------------------------------------
    public void SetItemForSell(ItemData item)
    {
        if (item == null) return;

        if (itemToSell != null) eqManager.AddToBackpack(itemToSell);
        eqManager.RemoveItem(item);
        itemToSell = item;

        sellSlotIcon.sprite = item.itemIcon;
        sellSlotIcon.gameObject.SetActive(true);
        if (sellPanelCoin != null) sellPanelCoin.SetActive(true);

        // U¿ywa stosu (currentStack) jeœli to ma³y przedmiot!
        int finalSellPrice = (item.sellPrice + (item.upgradeLevel * (item.baseUpgradeCost / 2))) * item.currentStack;
        sellPriceText.text = "+" + finalSellPrice.ToString();

        string quant = item.currentStack > 1 ? $" (x{item.currentStack})" : "";
        sellItemName.text = item.itemName + (item.upgradeLevel > 0 ? " +" + item.upgradeLevel : "") + quant;

        if (confirmSellButton != null) confirmSellButton.interactable = true;
        DelayedEQRefresh();
    }

    public void ConfirmSell()
    {
        if (itemToSell == null) return;

        int finalSellPrice = (itemToSell.sellPrice + (itemToSell.upgradeLevel * (itemToSell.baseUpgradeCost / 2))) * itemToSell.currentStack;
        currentCoins += finalSellPrice;
        itemToSell = null;

        sellSlotIcon.sprite = emptySlotIcon;
        if (sellPanelCoin != null) sellPanelCoin.SetActive(false);
        sellPriceText.text = "Sold!";
        sellItemName.text = "Item name";
        if (confirmSellButton != null) confirmSellButton.interactable = false;

        SaveProgress();
        UpdateUI();
        DelayedEQRefresh();
    }

    public void CancelSell()
    {
        if (itemToSell != null)
        {
            eqManager.AddToBackpack(itemToSell);
            itemToSell = null;
        }
        if (sellSlotIcon != null) sellSlotIcon.sprite = emptySlotIcon;
        if (sellPriceText != null) sellPriceText.text = "";
        if (sellItemName != null) sellItemName.text = "Item name";
        if (sellPanelCoin != null) sellPanelCoin.SetActive(false);
        DelayedEQRefresh();
    }

    // ----------------------------------------------------
    // POZOSTA£E FUNKCJE KOWALA
    // ----------------------------------------------------

    private void RollUpgrades()
    {
        List<CardUpgradeData> poolCopy = new List<CardUpgradeData>(allPossibleUpgrades);

        for (int i = 0; i < 3; i++)
        {
            if (poolCopy.Count > 0 && i < upgradeButtons.Length)
            {
                int randomIndex = Random.Range(0, poolCopy.Count);
                CardUpgradeData rolledUpg = poolCopy[randomIndex];

                upgradeNames[i].text = rolledUpg.upgradeName;
                upgradeIcons[i].sprite = rolledUpg.icon;

                int indexForClosure = i;
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(rolledUpg));

                poolCopy.RemoveAt(randomIndex);
            }
        }
    }

    private void OnUpgradeSelected(CardUpgradeData upgrade)
    {
        selectedUpgrade = upgrade;
        upgradesPanel.SetActive(false);
        cardSelectionPanel.SetActive(true);
        LoadPlayerDeckForSelection();
    }

    private void LoadPlayerDeckForSelection()
    {
        if (lockedCardsForUpgrade == null)
        {
            List<Card> currentDeck = new List<Card>();

            if (SaveManager.instance != null && SaveManager.instance.currentSave != null && allPossibleCards != null)
            {
                foreach (Card refCard in allPossibleCards)
                {
                    currentDeck.Add(refCard);
                }
            }

            lockedCardsForUpgrade = new List<Card>();
            List<Card> poolCopy = new List<Card>(currentDeck);

            for (int i = 0; i < 3; i++)
            {
                if (poolCopy.Count > 0)
                {
                    int randomIndex = Random.Range(0, poolCopy.Count);
                    lockedCardsForUpgrade.Add(poolCopy[randomIndex]);
                    poolCopy.RemoveAt(randomIndex);
                }
            }
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < lockedCardsForUpgrade.Count)
            {
                Card card = lockedCardsForUpgrade[i];
                cardButtons[i].gameObject.SetActive(true);
                cardNames[i].text = "ALL " + card.name;
                cardGraphics[i].sprite = card.graphic;

                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => OnCardSelected(card));
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnCardSelected(Card card)
    {
        selectedCard = card;
        Debug.Log($"Upgraded cards {selectedCard.name} with effect {selectedUpgrade.upgradeName}!");

        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            string cleanCardName = GetCleanCardName(card.name);
            var savedUpgrades = SaveManager.instance.currentSave.upgradedCards;
            var existingRecord = savedUpgrades.Find(u => u.cardName == cleanCardName);

            if (existingRecord != null)
            {
                existingRecord.upgradeNames.Add(selectedUpgrade.name);
            }
            else
            {
                SavedCardUpgrade newRecord = new SavedCardUpgrade();
                newRecord.cardName = cleanCardName;
                newRecord.upgradeNames.Add(selectedUpgrade.name);
                savedUpgrades.Add(newRecord);
            }
            SaveProgress();
        }

        hasUpgradedCard = true;
        cardSelectionPanel.SetActive(false);
        lockedCardsForUpgrade = null;
    }

    private void RollShopItems()
    {
        List<ItemData> poolCopy = new List<ItemData>(shopItemPool);

        for (int i = 0; i < 3; i++)
        {
            if (poolCopy.Count > 0 && i < shopItemButtons.Length)
            {
                int randomIndex = Random.Range(0, poolCopy.Count);
                ItemData rolledItem = poolCopy[randomIndex];
                currentShopItems[i] = rolledItem;

                shopItemNames[i].text = rolledItem.itemName;
                shopItemIcons[i].sprite = rolledItem.itemIcon;
                shopItemPrices[i].text = rolledItem.buyPrice.ToString();
                shopItemStats[i].text = GetItemStatsString(rolledItem);

                int indexForClosure = i;
                shopItemButtons[i].onClick.RemoveAllListeners();
                shopItemButtons[i].onClick.AddListener(() => OnShopItemClicked(indexForClosure));

                poolCopy.RemoveAt(randomIndex);
            }
        }

        if (rerollButton != null)
        {
            rerollButton.interactable = currentCoins >= rerollCost;
        }

        RefreshShopButtonsInteractability();
    }

    private string GetItemStatsString(ItemData item)
    {
        string stats = "";
        if (item.bonusMaxHP > 0) stats += $"+{item.bonusMaxHP} Max HP\n";
        if (item.bonusAttackTokens > 0) stats += $"+{item.bonusAttackTokens} Attack Tokens\n";
        if (item.bonusDefenseTokens > 0) stats += $"+{item.bonusDefenseTokens} Defense Tokens\n";
        if (item.bonusMaxBet > 0) stats += $"+{item.bonusMaxBet} Max Bet\n";
        if (item.bonusBaseDamage > 0) stats += $"+{item.bonusBaseDamage} Damage\n";
        if (item.restoreHP > 0) stats += $"Restores {item.restoreHP} HP\n";
        if (item.restoreArmor > 0) stats += $"Restores {item.restoreArmor} armor";
        if (item.bonusGoldMultiplier > 0f)
        {
            int goldPercentage = Mathf.RoundToInt(item.bonusGoldMultiplier * 100);
            stats += $"+{goldPercentage}% Gold Gain";
        }
        return stats;
    }

    public void OnShopItemClicked(int index)
    {
        ItemData itemToBuy = currentShopItems[index];

        if (itemToBuy != null && currentCoins >= itemToBuy.buyPrice)
        {
            if (eqManager != null && eqManager.AddToBackpack(itemToBuy))
            {
                currentCoins -= itemToBuy.buyPrice;
                UpdateUI();

                shopItemNames[index].text = "Sold";
                shopItemPrices[index].text = "-";
                shopItemStats[index].text = "";
                shopItemButtons[index].interactable = false;
                currentShopItems[index] = null;

                SaveProgress();
                RefreshShopButtonsInteractability();
                if (rerollButton != null) rerollButton.interactable = currentCoins >= rerollCost;
            }
        }
    }

    public void RerollShop()
    {
        if (currentCoins >= rerollCost)
        {
            currentCoins -= rerollCost;
            SaveProgress();
            RollShopItems();
            UpdateUI();
        }
    }

    private void RefreshShopButtonsInteractability()
    {
        for (int i = 0; i < 3; i++)
        {
            if (currentShopItems[i] != null && shopItemButtons.Length > i && shopItemButtons[i] != null)
            {
                shopItemButtons[i].interactable = currentCoins >= currentShopItems[i].buyPrice;
            }
        }
    }

    private void SaveProgress()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            SaveManager.instance.currentSave.savedPlayerCoins = currentCoins;
            eqManager.SaveEquipment(SaveManager.instance.currentSave);
            SaveManager.instance.SaveToFile(SaveManager.instance.currentSave);
        }
    }

    public void OnLeaveClicked()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        SceneManager.LoadScene("WorldMap");
    }

    public void ClosePanels()
    {
        CancelEnhance();
        CancelSell();
        CancelRepair(); // Od³o¿enie niedokoñczonej naprawy do plecaka

        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (upgradesPanel != null) upgradesPanel.SetActive(false);
        if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (enhancePanel != null) enhancePanel.SetActive(false);
        if (sellPanel != null) sellPanel.SetActive(false);
        if (repairPanel != null) repairPanel.SetActive(false);
        if(enchancePanelCoinIcon != null) enchancePanelCoinIcon.SetActive(false);
        if(enchancePanelArrow != null) enchancePanelArrow.SetActive(false);
        if(sellPanelCoin != null) sellPanelCoin.SetActive(false);
        if(repairPanelCoin != null) repairPanelCoin.SetActive(false);
        if (enhanceSlotIcon != null) enhanceSlotIcon.sprite = emptySlotIcon;
        if(sellSlotIcon != null) sellSlotIcon.sprite = emptySlotIcon;
        if(repairSlotIcon != null) repairSlotIcon.sprite = emptySlotIcon;
        if (repairDurability != null) repairDurability.text = "";
    }

    private string GetCleanCardName(string rawName)
    {
        string clean = rawName.Replace("Card_", "");
        string[] stringsToRemove = { "pik", "karo", "kier", "trefl", "Pik", "Karo", "Kier", "Trefl", "_", " " };

        foreach (string s in stringsToRemove)
        {
            clean = clean.Replace(s, "");
        }
        return clean.Trim();
    }

    private void UpdateUI()
    {
        if (playerCoinsText != null) playerCoinsText.text = currentCoins.ToString();
        if (playerCoinsTextInv != null) playerCoinsTextInv.text = currentCoins.ToString();
    }

    private void DelayedEQRefresh()
    {
        InventoryUI invUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (invUI != null) invUI.RefreshUI();
    }
}