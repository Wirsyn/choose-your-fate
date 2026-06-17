using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panele UI")]
    public GameObject mainMenuPanel;
    public GameObject classSelectionPanel;
    public GameObject loadingPanel;

    [Header("G³ówne Przyciski")]
    public Button continueButton;

    [Header("Konfiguracja Klas")]
    public Transform classGridContainer;
    public GameObject classButtonPrefab;
    public GameObject fillerPrefab;
    public List<PlayerClassData> availableClasses;

    [Header("Pasek £adowania")]
    public Slider loadingBar;

    [Header("Pominiêcie Tutorialu")]
    public Toggle skipTutorialToggle;
    [Tooltip("Tarcza, któr¹ gracz dostanie automatycznie przy pominiêciu tutorialu")]
    public ItemData tutorialRewardShield;

    private bool gridPopulated = false;

    private void Start()
    {
        CheckSaveFile();

        if (skipTutorialToggle != null)
        {
            bool hasCompletedTutorial = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
            skipTutorialToggle.gameObject.SetActive(hasCompletedTutorial);
            skipTutorialToggle.isOn = false;
        }
    }

    private void CheckSaveFile()
    {
        if (continueButton != null)
        {
            string savePath = Application.persistentDataPath + "/savegame.sav";
            continueButton.interactable = File.Exists(savePath);
        }
    }

    public void OpenClassSelection()
    {
        mainMenuPanel.SetActive(false);
        classSelectionPanel.SetActive(true);

        if (!gridPopulated)
        {
            PopulateClassGrid();
        }
    }

    public void CloseClassSelection()
    {
        classSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void PopulateClassGrid()
    {
        foreach (Transform child in classGridContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerClassData classData in availableClasses)
        {
            GameObject btnObj = Instantiate(classButtonPrefab, classGridContainer);

            Image img = btnObj.GetComponent<Image>();
            if (img != null) img.sprite = classData.classIcon;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => StartNewGame(classData));
            }
        }

        int fillerCount = 3 - (availableClasses.Count % 3);
        if (fillerCount < 3)
        {
            for (int i = 0; i < fillerCount; i++)
            {
                Instantiate(fillerPrefab, classGridContainer);
            }
        }
        gridPopulated = true;
    }

    public void StartNewGame(PlayerClassData selectedClass)
    {
        SaveData newGameData = new SaveData();
        newGameData.savedClassType = selectedClass.classType;

        bool skipTutorial = (skipTutorialToggle != null && skipTutorialToggle.gameObject.activeSelf && skipTutorialToggle.isOn);

        if (skipTutorial)
        {
            int totalMaxHP = selectedClass.baseMaxHP;
            int totalAtk = selectedClass.baseAttackTokens;
            int totalDef = selectedClass.baseDefenseTokens;

            ItemData[] startingGear = new ItemData[] {
                selectedClass.startingWeapon,
                selectedClass.startingShield,
                selectedClass.startingHelmet,
                selectedClass.startingArmor,
                selectedClass.startingTrinket,
                selectedClass.startingRing,
                selectedClass.startingBoots,
                selectedClass.startingGloves // <--- DODANE
            };

            if (tutorialRewardShield != null)
            {
                startingGear[1] = tutorialRewardShield;
            }

            foreach (var item in startingGear)
            {
                if (item != null)
                {
                    totalMaxHP += item.bonusMaxHP;
                    totalAtk += item.bonusAttackTokens;
                    totalDef += item.bonusDefenseTokens;
                }
            }

            newGameData.savedWeaponName = startingGear[0] != null ? startingGear[0].itemName + "#0#" + startingGear[0].maxDurability : "";
            newGameData.savedShieldName = startingGear[1] != null ? startingGear[1].itemName + "#0#" + startingGear[1].maxDurability : "";
            newGameData.savedHelmetName = startingGear[2] != null ? startingGear[2].itemName + "#0#" + startingGear[2].maxDurability : "";
            newGameData.savedArmorName = startingGear[3] != null ? startingGear[3].itemName + "#0#" + startingGear[3].maxDurability : "";
            newGameData.savedTrinketName = startingGear[4] != null ? startingGear[4].itemName + "#0#" + startingGear[4].maxDurability : "";
            newGameData.savedRing = startingGear[5] != null ? startingGear[5].itemName + "#0#" + startingGear[5].maxDurability : "";
            newGameData.savedBootsName = startingGear[6] != null ? startingGear[6].itemName + "#0#" + startingGear[6].maxDurability : "";
            newGameData.savedGlovesName = startingGear[7] != null ? startingGear[7].itemName + "#0#" + startingGear[7].maxDurability : ""; // DODANE
            newGameData.savedPotion = selectedClass.startingPotion != null ? selectedClass.startingPotion.itemName + "#0#" + selectedClass.startingPotion.maxDurability : "";

            foreach (var item in selectedClass.startingBackpack)
            {
                if (item != null)
                    newGameData.savedBackpack.Add(item.itemName + "#0#" + item.maxDurability);
            }

            newGameData.savedPlayerMaxHP = totalMaxHP;
            newGameData.savedPlayerHP = totalMaxHP;
            newGameData.savedTokensAttack = totalAtk;
            newGameData.savedTokensDefense = totalDef;
            newGameData.savedPlayerArmor = 0;
            newGameData.savedPlayerCoins = 0;
            newGameData.savedMobIndex = 0;
        }

        SaveManager.instance.currentSave = newGameData;
        SaveManager.instance.SaveToFile(newGameData);

        classSelectionPanel.SetActive(false);

        if (skipTutorial)
        {
            StartCoroutine(LoadSceneAsync("WorldMap"));
        }
        else
        {
            StartCoroutine(LoadSceneAsync("Tutorial"));
        }
    }

    public void ContinueGame()
    {
        SaveManager.instance.LoadFromFile();

        if (SaveManager.instance.currentSave != null)
        {
            StartCoroutine(LoadSceneAsync("WorldMap"));
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingBar != null) loadingBar.value = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (loadingBar.value < 1f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = Mathf.MoveTowards(loadingBar.value, targetProgress, 0.5f * Time.deltaTime);
            yield return null;
        }

        operation.allowSceneActivation = true;
    }

    public void DeleteSaveAndQuit()
    {
        string savePath = Application.persistentDataPath + "/savegame.sav";
        if (System.IO.File.Exists(savePath))
        {
            System.IO.File.Delete(savePath);
        }
        Debug.Log("Zapis usuniêty! Zamykam grê...");
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        Application.Quit();
    }

    public void OpenLogFolder()
    {
        string path = Application.persistentDataPath;
        Application.OpenURL("file://" + path);
    }

    public void OpenLaboratory()
    {
        // NAPRAWIONE: U¿ycie StartCoroutine dla ³adowania sceny!
        StartCoroutine(LoadSceneAsync("Laboratory"));
    }

    // DODANE: Reset ustawieñ do domyœlnych prosto z G³ównego Menu
    public void ResetSettingsToDefault()
    {
        AudioListener.volume = 1f;
        PlayerPrefs.SetFloat("VolumePreference", 1f);

        Screen.SetResolution(1920, 1080, true);
        PlayerPrefs.SetInt("FullscreenPreference", 1);

        PlayerPrefs.Save();
        Debug.Log("Zresetowano ustawienia do 1920x1080 i pe³nego ekranu z poziomu Main Menu!");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}