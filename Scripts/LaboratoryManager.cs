using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LaboratoryManager : MonoBehaviour
{
    [Header("Bazy Danych (Przeci¹gnij z folderów)")]
    public List<ItemData> allItems;
    public List<CardUpgradeData> allUpgrades;
    public List<Card> referenceCards; // Wszystkie karty w grze
    public WaveData dummyWave;        // Fala z kuk³¹ treningow¹
    public PlayerClassData testClass; // Klasa, któr¹ chcesz testowaæ (np. Wojownik)

    [Header("UI Elementy")]
    public Transform itemsContainer;    // Content ze Scroll View dla przedmiotów
    public Transform upgradesContainer; // Content ze Scroll View dla ulepszeñ
    public GameObject buttonPrefab;     // Prosty prefab: Button z TextMeshPro w œrodku
    public TextMeshProUGUI summaryText; // Tekst podsumowuj¹cy Twój build

    [Header("Ustawienia Startowe Testu")]
    public TMP_InputField playerHpInput;
    public TMP_InputField playerArmorInput;
    public TMP_InputField dummyHpInput;
    public TMP_InputField dummyArmorInput;

    private SaveData testSave;

    void Start()
    {
        ResetBuild();
    }

    public void ResetBuild()
    {
        testSave = new SaveData();
        testSave.savedClassType = testClass != null ? testClass.classType : PlayerClassType.Warrior;
        testSave.savedPlayerHP = testClass != null ? testClass.baseMaxHP : 15;
        testSave.savedPlayerMaxHP = testClass != null ? testClass.baseMaxHP : 15;
        testSave.savedPlayerCoins = 9999;
        testSave.upgradedCards = new List<SavedCardUpgrade>();
        testSave.savedTokensAttack = 3;
        testSave.savedTokensDefense = 1;

        // Czyœcimy kontenery przed ponownym generowaniem
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);
        foreach (Transform child in upgradesContainer) Destroy(child.gameObject);

        GenerateItemButtons();
        GenerateUpgradeButtons();
        UpdateSummary();
    }
    void GenerateItemButtons()
    {
        foreach (Transform child in itemsContainer) Destroy(child.gameObject); // czyœcimy

        foreach (ItemData item in allItems)
        {
            GameObject btnObj = Instantiate(buttonPrefab, itemsContainer);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;

            ItemData capturedItem = item;
            btnObj.GetComponent<Button>().onClick.AddListener(() => EquipTestItem(capturedItem));
        }

        // --- DODAJ TO: Wymuszenie odœwie¿enia uk³adu ---
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemsContainer.GetComponent<RectTransform>());
    }

    void GenerateUpgradeButtons()
    {
        foreach (CardUpgradeData upg in allUpgrades)
        {
            GameObject btnObj = Instantiate(buttonPrefab, upgradesContainer);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = upg.upgradeName;

            CardUpgradeData capturedUpg = upg;
            btnObj.GetComponent<Button>().onClick.AddListener(() => AddTestUpgrade(capturedUpg));
        }
    }

    void EquipTestItem(ItemData item)
    {
        switch (item.slotType)
        {
            case EquipmentSlot.Weapon: testSave.savedWeaponName = item.itemName; break;
            case EquipmentSlot.Shield: testSave.savedShieldName = item.itemName; break;
            case EquipmentSlot.Helmet: testSave.savedHelmetName = item.itemName; break;
            case EquipmentSlot.Armor: testSave.savedArmorName = item.itemName; break;
            case EquipmentSlot.Trinket: testSave.savedTrinketName = item.itemName; break;
            case EquipmentSlot.Ring: testSave.savedRing = item.itemName; break;
            case EquipmentSlot.Potion: testSave.savedPotion = item.itemName; break;
        }
        UpdateSummary();
        Debug.Log("Za³o¿ono: " + item.itemName);
    }

    void AddTestUpgrade(CardUpgradeData upg)
    {
        // MEGA U£ATWIENIE: Nak³adamy ten buff od razu na KA¯D¥ mo¿liw¹ kartê w talii.
        // Dziêki temu od razu przetestujesz efekt, bo ka¿da wylosowana karta go odpali!
        foreach (Card card in referenceCards)
        {
            string cleanName = GetCleanCardName(card.name);
            var existing = testSave.upgradedCards.Find(u => u.cardName == cleanName);

            if (existing != null)
            {
                existing.upgradeNames.Add(upg.name);
            }
            else
            {
                SavedCardUpgrade newUpg = new SavedCardUpgrade();
                newUpg.cardName = cleanName;
                newUpg.upgradeNames.Add(upg.name);
                testSave.upgradedCards.Add(newUpg);
            }
        }
        UpdateSummary();
        Debug.Log("Dodano globalny buff: " + upg.upgradeName);
    }

    void UpdateSummary()
    {
        string s = "<b>Twój Build:</b>\n";
        s += $"Broñ: {testSave.savedWeaponName}\n";
        s += $"Tarcza: {testSave.savedShieldName}\n";
        s += $"Zbroja: {testSave.savedArmorName}\n";
        s += $"He³m: {testSave.savedHelmetName}\n";
        s += $"Trinket: {testSave.savedTrinketName}\n";
        s += $"Sygnet: {testSave.savedRing}\n";
        s += $"Potka: {testSave.savedPotion}\n\n";

        // Zliczamy unikalne nazwy na³o¿onych globalnie buffów (sprawdzamy na pierwszej karcie)
        int buffCount = testSave.upgradedCards.Count > 0 ? testSave.upgradedCards[0].upgradeNames.Count : 0;
        s += $"<b>Globalne ulepszenia kart:</b> {buffCount}";

        summaryText.text = s;
    }

    public void StartTestCombat()
    {
        if (dummyWave != null) testSave.targetWaveName = dummyWave.name;
        testSave.currentMapTheme = "DarkForest"; // Lub inny domyœlny motyw wizualny z CombatManagera
        int pHP = int.TryParse(playerHpInput.text, out int ph) ? ph : 15;
        int pArmor = int.TryParse(playerArmorInput.text, out int pa) ? pa : 0;
        testSave.savedPlayerHP = pHP;
        testSave.savedPlayerMaxHP = pHP;
        testSave.savedPlayerArmor = pArmor;

        testSave.savedDummyHP = int.TryParse(dummyHpInput.text, out int dh) ? dh : 9999;
        testSave.savedDummyArmor = int.TryParse(dummyArmorInput.text, out int da) ? da : 0;
        testSave.savedTokensAttack = 3;
        testSave.savedTokensDefense = 1;


        // Wymuszamy zapis testowy i ³adujemy walkê
        if (SaveManager.instance != null)
        {
            SaveManager.instance.currentSave = testSave;
            SaveManager.instance.SaveToFile(testSave);
        }

        SceneManager.LoadScene("CombatScene");
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private string GetCleanCardName(string rawName)
    {
        string clean = rawName.Replace("Card_", "");
        string[] stringsToRemove = { "pik", "karo", "kier", "trefl", "Pik", "Karo", "Kier", "Trefl", "_", " " };
        foreach (string s in stringsToRemove) clean = clean.Replace(s, "");
        return clean.Trim();
    }
}