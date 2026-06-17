using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TavernManager : MonoBehaviour
{
    [Header("Managery")]
    public EquipmentManager eqManager;
    public TextMeshProUGUI playerCoinsText;
    public GameObject loadingPanel;
    public ItemTooltip tooltipManager;

    [Header("Panele UI")]
    public GameObject mainMenuPanel;
    public GameObject shopPanel;
    public GameObject sleepPanel;
    public GameObject eatPanel;
    public GameObject craftPanel;

    [Header("Sklep")]
    public List<ItemData> shopItemPool;
    public Button[] shopItemButtons;
    public TextMeshProUGUI[] shopItemNames;
    public Image[] shopItemIcons;
    public TextMeshProUGUI[] shopItemPrices;

    [Header("Sen")]
    public int sleepCost = 20;
    public int sleepHealAmount = 15;
    public TextMeshProUGUI sleepCostText;

    [Header("Jedzenie")] 
    public List<MealData> tavernMealsPool;
    public Button[] mealButtons;
    public TextMeshProUGUI[] mealNames;
    public Image[] mealIcons;
    public TextMeshProUGUI[] mealPrices;

    private void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (tooltipManager != null) tooltipManager.HideTooltip();

        OpenMainMenu();
        UpdateCoinsUI();
        GenerateShop();
        GenerateMealsMenu();

        if (sleepCostText != null) sleepCostText.text = $"{sleepCost} Coins";
    }

    public void OpenMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (shopPanel) shopPanel.SetActive(false);
        if (sleepPanel) sleepPanel.SetActive(false);
        if (eatPanel) eatPanel.SetActive(false);
        if (craftPanel) craftPanel.SetActive(false);

        // Ukrywamy tooltip przy zmianie zak³adki
        if (tooltipManager != null) tooltipManager.HideTooltip();
    }

    private void UpdateCoinsUI()
    {
        if (playerCoinsText != null && SaveManager.instance != null)
        {
            playerCoinsText.text = SaveManager.instance.currentSave.savedPlayerCoins.ToString();
        }
    }

    // ================= SKLEP PRZEDMIOTÓW =================
    public void OpenShop()
    {
        HideAllPanels();
        if (shopPanel) shopPanel.SetActive(true);
    }

    private void GenerateShop()
    {
        if (shopItemPool == null || shopItemPool.Count == 0) return;

        for (int i = 0; i < shopItemButtons.Length; i++)
        {
            if (i < shopItemPool.Count)
            {
                ItemData item = shopItemPool[i];
                shopItemButtons[i].gameObject.SetActive(true);

                if (shopItemNames[i] != null) shopItemNames[i].text = item.itemName;
                if (shopItemIcons[i] != null) shopItemIcons[i].sprite = item.itemIcon;
                if (shopItemPrices[i] != null) shopItemPrices[i].text = item.buyPrice.ToString();

                int index = i;
                shopItemButtons[i].onClick.RemoveAllListeners();
                shopItemButtons[i].onClick.AddListener(() => BuyItem(index));

                // --- DODANE: Automatyczne podpiêcie Tooltipa pod przycisk ---
                ItemData localItem = item; // Zabezpieczenie pêtli
                EventTrigger trigger = shopItemButtons[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = shopItemButtons[i].gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enter.callback.AddListener((data) => { if (tooltipManager != null) tooltipManager.ShowTooltip(localItem); });
                trigger.triggers.Add(enter);

                EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exit.callback.AddListener((data) => { if (tooltipManager != null) tooltipManager.HideTooltip(); });
                trigger.triggers.Add(exit);
            }
            else
            {
                shopItemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void BuyItem(int index)
    {
        if (index < 0 || index >= shopItemPool.Count) return;
        ItemData itemToBuy = shopItemPool[index];

        if (SaveManager.instance.currentSave.savedPlayerCoins >= itemToBuy.buyPrice)
        {
            if (eqManager != null && eqManager.AddToBackpack(itemToBuy))
            {
                SaveManager.instance.currentSave.savedPlayerCoins -= itemToBuy.buyPrice;
                UpdateCoinsUI();
                SaveGame();
                if (tooltipManager != null) tooltipManager.HideTooltip(); // Ukrywamy po zakupie
            }
        }
    }

    // ================= SPANIE =================
    public void OpenSleep()
    {
        HideAllPanels();
        if (sleepPanel) sleepPanel.SetActive(true);
    }

    public void RestInBed()
    {
        if (SaveManager.instance.currentSave.savedPlayerCoins >= sleepCost)
        {
            if (SaveManager.instance.currentSave.savedPlayerHP >= SaveManager.instance.currentSave.savedPlayerMaxHP)
            {
                Debug.Log("You are fully healed! No need to sleep.");
                return;
            }

            SaveManager.instance.currentSave.savedPlayerCoins -= sleepCost;
            SaveManager.instance.currentSave.savedPlayerHP += sleepHealAmount;

            if (SaveManager.instance.currentSave.savedPlayerHP > SaveManager.instance.currentSave.savedPlayerMaxHP)
                SaveManager.instance.currentSave.savedPlayerHP = SaveManager.instance.currentSave.savedPlayerMaxHP;

            UpdateCoinsUI();
            SaveGame();
            Debug.Log("You slept well. HP restored!");
            OpenMainMenu();
        }
    }

    // ================= JEDZENIE =================
    public void OpenEat()
    {
        HideAllPanels();
        if (eatPanel) eatPanel.SetActive(true);
    }

    private void GenerateMealsMenu()
    {
        if (tavernMealsPool == null || tavernMealsPool.Count == 0) return;

        for (int i = 0; i < mealButtons.Length; i++)
        {
            if (i < tavernMealsPool.Count)
            {
                MealData meal = tavernMealsPool[i];
                mealButtons[i].gameObject.SetActive(true);

                if (mealNames[i] != null) mealNames[i].text = meal.mealName;
                if (mealIcons[i] != null) mealIcons[i].sprite = meal.mealIcon;
                if (mealPrices[i] != null) mealPrices[i].text = $"{meal.buyPrice} Coins";

                int index = i;
                mealButtons[i].onClick.RemoveAllListeners();
                mealButtons[i].onClick.AddListener(() => BuyMeal(index));

                // --- DODANE: Automatyczne podpiêcie Tooltipa pod Posi³ki ---
                MealData localMeal = meal; // Zabezpieczenie pêtli
                EventTrigger trigger = mealButtons[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = mealButtons[i].gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                enter.callback.AddListener((data) => { if (tooltipManager != null) tooltipManager.ShowTooltipMeal(localMeal); });
                trigger.triggers.Add(enter);

                EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exit.callback.AddListener((data) => { if (tooltipManager != null) tooltipManager.HideTooltip(); });
                trigger.triggers.Add(exit);
            }
            else
            {
                mealButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void BuyMeal(int index)
    {
        if (index < 0 || index >= tavernMealsPool.Count) return;
        MealData chosenMeal = tavernMealsPool[index];

        if (SaveManager.instance.currentSave.savedPlayerCoins >= chosenMeal.buyPrice)
        {
            SaveManager.instance.currentSave.savedPlayerCoins -= chosenMeal.buyPrice;
            SaveManager.instance.currentSave.activeTavernBuff = chosenMeal.name;

            UpdateCoinsUI();
            SaveGame();
            if (tooltipManager != null) tooltipManager.HideTooltip(); // Ukrywamy po zakupie
            Debug.Log($"You ate {chosenMeal.mealName}! Buff registered for the next fight.");
            OpenMainMenu();
        }
    }

    // ================= CRAFTING =================
    public void OpenCrafting()
    {
        HideAllPanels();
        if (craftPanel) craftPanel.SetActive(true);
    }

    // ================= WYJŒCIE =================
    public void LeaveTavern()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        SceneManager.LoadScene("WorldMap");
    }

    private void SaveGame()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            if (eqManager != null) eqManager.SaveEquipment(SaveManager.instance.currentSave);
            SaveManager.instance.SaveToFile(SaveManager.instance.currentSave);
        }
    }
}