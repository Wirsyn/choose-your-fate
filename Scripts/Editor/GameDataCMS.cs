using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class GameDataCMS : EditorWindow
{
    // --- STAN INTERFEJSU ---
    private Vector2 listScrollPos;
    private Vector2 editorScrollPos;
    private int selectedTab = 0;
    private string[] tabNames = { "Przedmioty", "Potwory", "Fale", "Ścieżki", "Karty", "Ulepszenia", "Klasy", "Motywy Map", "Posiłki 🍲", "Walidator ⚠️" };

    // --- LISTY DANYCH ---
    private List<ItemData> allItems = new List<ItemData>();
    private List<MobData> allMobs = new List<MobData>();
    private List<WaveData> allWaves = new List<WaveData>();
    private List<PathData> allPaths = new List<PathData>();
    private List<Card> allCards = new List<Card>();
    private List<CardUpgradeData> allUpgrades = new List<CardUpgradeData>();
    private List<PlayerClassData> allClasses = new List<PlayerClassData>();
    private List<MapThemeData> allThemes = new List<MapThemeData>();
    private List<MealData> allMeals = new List<MealData>(); // DODANE

    // --- WALIDATOR ---
    private struct ValidationIssue
    {
        public ScriptableObject asset;
        public string message;
        public MessageType type;
    }
    private List<ValidationIssue> validationIssues = new List<ValidationIssue>();

    // --- ZAZNACZONY ELEMENT ---
    private ScriptableObject activeItem = null;
    private Editor activeEditor = null;

    [MenuItem("Narzędzia Developera/🛠️ Otwórz CMS (Główna Baza Danych)")]
    public static void ShowWindow()
    {
        GameDataCMS window = GetWindow<GameDataCMS>("Game Data CMS");
        window.minSize = new Vector2(1000, 600);
    }

    private void OnEnable()
    {
        RefreshAllData();
    }

    private void RefreshAllData()
    {
        allItems = LoadAssets<ItemData>();
        allMobs = LoadAssets<MobData>();
        allWaves = LoadAssets<WaveData>();
        allPaths = LoadAssets<PathData>();
        allCards = LoadAssets<Card>();
        allUpgrades = LoadAssets<CardUpgradeData>();
        allClasses = LoadAssets<PlayerClassData>();
        allThemes = LoadAssets<MapThemeData>();
        allMeals = LoadAssets<MealData>(); // DODANE

        RunValidation();
    }

    private List<T> LoadAssets<T>() where T : ScriptableObject
    {
        List<T> list = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) list.Add(asset);
        }
        list.Sort((a, b) => string.Compare(a.name, b.name));
        return list;
    }

    private void OnGUI()
    {
        DrawTabs();
        DrawTopToolbar();

        EditorGUILayout.BeginHorizontal();

        if (selectedTab == tabNames.Length - 1)
        {
            DrawValidatorTab();
        }
        else
        {
            DrawLeftColumn();
            DrawRightColumn();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTabs()
    {
        GUILayout.Space(5);
        int previousTab = selectedTab;

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(30));

        if (previousTab != selectedTab && selectedTab != tabNames.Length - 1)
        {
            activeItem = null;
            if (activeEditor != null) DestroyImmediate(activeEditor);
            GUI.FocusControl(null);
        }
        GUILayout.Space(5);
    }

    private void DrawTopToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Odśwież Bazy", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            RefreshAllData();
        }

        GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
        if (GUILayout.Button("Zsynchronizuj Wszystkie Bazy z Menedżerami", EditorStyles.toolbarButton, GUILayout.Width(300)))
        {
            SyncAllDatabases();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();

        if (selectedTab != tabNames.Length - 1)
        {
            string addText = "+ Dodaj " + tabNames[selectedTab];
            if (GUILayout.Button(addText, EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                CreateNewAssetForCurrentTab();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftColumn()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos, GUI.skin.box);

        switch (selectedTab)
        {
            case 0: DrawAssetList(allItems); break;
            case 1: DrawAssetList(allMobs); break;
            case 2: DrawAssetList(allWaves); break;
            case 3: DrawAssetList(allPaths); break;
            case 4: DrawAssetList(allCards); break;
            case 5: DrawAssetList(allUpgrades); break;
            case 6: DrawAssetList(allClasses); break;
            case 7: DrawAssetList(allThemes); break;
            case 8: DrawAssetList(allMeals); break; // DODANE
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawAssetList<T>(List<T> assets) where T : ScriptableObject
    {
        if (assets.Count == 0)
        {
            EditorGUILayout.HelpBox("Brak danych w tej kategorii.", MessageType.Info);
            return;
        }

        foreach (T asset in assets)
        {
            GUIStyle style = (activeItem == asset) ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

            EditorGUILayout.BeginHorizontal();

            Texture2D iconTex = GetIconForAsset(asset);
            if (iconTex != null) GUILayout.Label(iconTex, GUILayout.Width(20), GUILayout.Height(20));

            string displayName = GetDisplayNameForAsset(asset);

            if (GUILayout.Button(displayName, style))
            {
                activeItem = asset;
                if (activeEditor != null) DestroyImmediate(activeEditor);
                activeEditor = Editor.CreateEditor(activeItem);
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawRightColumn()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

        if (activeItem != null && activeEditor != null)
        {
            EditorGUILayout.LabelField($"Edytujesz plik: {activeItem.name}.asset", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            if (activeItem is MobData mob)
            {
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                if (GUILayout.Button("⚔️ Przetestuj Potwora w Grze (God Mode)", GUILayout.Height(40)))
                {
                    QuickTestMob(mob);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }
            else if (activeItem is WaveData wave)
            {
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                if (GUILayout.Button("⚔️ Przetestuj Falę w Grze (God Mode)", GUILayout.Height(40)))
                {
                    LaunchCombatSceneWithWave(wave);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }

            activeEditor.OnInspectorGUI();

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space();
            if (GUILayout.Button("Pokaż plik w oknie Project (Odnajdź na dysku)"))
            {
                Selection.activeObject = activeItem;
                EditorGUIUtility.PingObject(activeItem);
            }
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Wybierz element z listy po lewej stronie, aby go edytować.", MessageType.Info);
            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SyncAllDatabases()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        RefreshAllData();
        bool updatedAny = false;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Synchronizacja Baz", $"Skanowanie prefabów ({i}/{prefabGuids.Length})...", (float)i / prefabGuids.Length);

            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool modified = false;
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component comp in components)
            {
                if (comp != null && UpdateComponentDatabases(comp)) modified = true;
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                updatedAny = true;
            }
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            EditorUtility.DisplayProgressBar("Synchronizacja Baz", $"Skanowanie sceny: {Path.GetFileNameWithoutExtension(scenePath)}...", (float)i / sceneGuids.Length);

            if (currentScene.path != scenePath)
            {
                UnityEngine.SceneManagement.Scene tempScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                bool sceneModified = false;

                foreach (GameObject root in tempScene.GetRootGameObjects())
                {
                    Component[] comps = root.GetComponentsInChildren<Component>(true);
                    foreach (Component c in comps)
                    {
                        if (c != null && UpdateComponentDatabases(c)) sceneModified = true;
                    }
                }

                if (sceneModified)
                {
                    EditorSceneManager.MarkSceneDirty(tempScene);
                    EditorSceneManager.SaveScene(tempScene);
                    updatedAny = true;
                }
                EditorSceneManager.CloseScene(tempScene, true);
            }
            else
            {
                bool sceneModified = false;
                foreach (GameObject root in currentScene.GetRootGameObjects())
                {
                    Component[] comps = root.GetComponentsInChildren<Component>(true);
                    foreach (Component c in comps)
                    {
                        if (c != null && UpdateComponentDatabases(c)) sceneModified = true;
                    }
                }
                if (sceneModified)
                {
                    EditorSceneManager.MarkSceneDirty(currentScene);
                    updatedAny = true;
                }
            }
        }

        EditorUtility.ClearProgressBar();

        if (updatedAny)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("✅ [CMS] Bazy danych zsynchronizowano pomyślnie na WSZYSTKICH Menedżerach!");
        }
    }

    private bool UpdateComponentDatabases(Component comp)
    {
        bool modified = false;

        if (comp is EquipmentManager eq)
        {
            eq.allPossibleItems = new List<ItemData>(allItems);
            modified = true;
        }
        else if (comp is CombatManager cm)
        {
            cm.allPossibleWaves = new List<WaveData>(allWaves);
            cm.allPossibleClasses = new List<PlayerClassData>(allClasses);
            cm.allAvailableUpgrades = new List<CardUpgradeData>(allUpgrades);
            cm.allReferenceCards = new List<Card>(allCards);
            cm.allPossibleMeals = new List<MealData>(allMeals); // DODANE: Synchronizacja posiłków w walce
            modified = true;
        }
        else if (comp is DeckManager dm)
        {
            dm.allPossibleClasses = new List<PlayerClassData>(allClasses);
            dm.allAvailableUpgrades = new List<CardUpgradeData>(allUpgrades);
            modified = true;
        }
        else if (comp is BlacksmithManager bm)
        {
            bm.allPossibleUpgrades = new List<CardUpgradeData>(allUpgrades);
            List<Card> filteredCards = new List<Card>();
            foreach (Card card in allCards)
            {
                if (card != null)
                {
                    string path = AssetDatabase.GetAssetPath(card);
                    if (path.Contains("Assets/Cards/Values") && !card.name.StartsWith("Card_"))
                    {
                        filteredCards.Add(card);
                    }
                }
            }
            bm.allPossibleCards = filteredCards;
            modified = true;
        }
        else if (comp is LaboratoryManager lm)
        {
            lm.allItems = new List<ItemData>(allItems);
            lm.allUpgrades = new List<CardUpgradeData>(allUpgrades);
            lm.referenceCards = new List<Card>(allCards);
            modified = true;
        }
        else if (comp is MainMenuManager mm)
        {
            mm.availableClasses = new List<PlayerClassData>(allClasses);
            modified = true;
        }
        else if (comp is TavernManager tm) // DODANE: Synchronizacja z karczmą
        {
            tm.tavernMealsPool = new List<MealData>(allMeals);
            modified = true;
        }

        return modified;
    }

    private void RunValidation()
    {
        validationIssues.Clear();

        foreach (var item in allItems)
        {
            if (item.itemIcon == null) AddIssue(item, "Brak ikony przedmiotu!", MessageType.Error);
            if (item.maxDurability <= 0) AddIssue(item, "Maksymalna wytrzymałość wynosi 0 lub mniej!", MessageType.Error);
            if (item.buyPrice <= item.sellPrice) AddIssue(item, "Cena sprzedaży jest wyższa (lub równa) niż kupna.", MessageType.Warning);
        }

        foreach (var mob in allMobs)
        {
            if (mob.mobGraphic == null) AddIssue(mob, "Brak przypisanej grafiki potwora!", MessageType.Error);
            if (mob.maxHP <= 0) AddIssue(mob, "Potwór ma 0 lub ujemne Max HP!", MessageType.Error);
        }

        foreach (var wave in allWaves)
        {
            if (wave.mobsInWave == null || wave.mobsInWave.Count == 0) AddIssue(wave, "Fala jest całkowicie pusta!", MessageType.Error);
        }

        foreach (var path in allPaths)
        {
            if (path.pathIcon == null) AddIssue(path, "Ścieżka na mapie nie ma przypisanej ikony!", MessageType.Error);
        }

        foreach (var cls in allClasses)
        {
            if (cls.classIcon == null) AddIssue(cls, "Brak ikony klasy!", MessageType.Error);
        }

        foreach (var theme in allThemes)
        {
            if (string.IsNullOrEmpty(theme.themeID)) AddIssue(theme, "Motyw mapy nie posiada identyfikatora!", MessageType.Error);
        }

        // DODANE: Walidacja posiłków
        foreach (var meal in allMeals)
        {
            if (meal.mealIcon == null) AddIssue(meal, "Posiłek nie ma przypisanej ikony!", MessageType.Error);
            if (meal.buyPrice <= 0) AddIssue(meal, "Cena posiłku wynosi 0 lub mniej!", MessageType.Warning);
        }
    }

    private void AddIssue(ScriptableObject asset, string msg, MessageType type)
    {
        validationIssues.Add(new ValidationIssue() { asset = asset, message = msg, type = type });
    }

    private void DrawValidatorTab()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"🔍 Wynik Skanowania: Znaleziono {validationIssues.Count} problemów", EditorStyles.boldLabel);
        if (GUILayout.Button("Uruchom Skaner Ponownie", GUILayout.Width(200))) RunValidation();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);
        if (validationIssues.Count == 0) EditorGUILayout.HelpBox("Wspaniale! Baza danych jest czysta.", MessageType.Info);
        else
        {
            foreach (var issue in validationIssues)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.HelpBox(issue.message, issue.type);
                EditorGUILayout.BeginVertical(GUILayout.Width(150));
                EditorGUILayout.LabelField(issue.asset.name, EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Napraw to!")) SelectAssetInCMS(issue.asset);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SelectAssetInCMS(ScriptableObject targetAsset)
    {
        if (targetAsset is ItemData) selectedTab = 0;
        else if (targetAsset is MobData) selectedTab = 1;
        else if (targetAsset is WaveData) selectedTab = 2;
        else if (targetAsset is PathData) selectedTab = 3;
        else if (targetAsset is Card) selectedTab = 4;
        else if (targetAsset is CardUpgradeData) selectedTab = 5;
        else if (targetAsset is PlayerClassData) selectedTab = 6;
        else if (targetAsset is MapThemeData) selectedTab = 7;
        else if (targetAsset is MealData) selectedTab = 8; // DODANE

        activeItem = targetAsset;
        if (activeEditor != null) DestroyImmediate(activeEditor);
        activeEditor = Editor.CreateEditor(activeItem);
        GUI.FocusControl(null);
    }

    private void QuickTestMob(MobData mob)
    {
        EnsureFolderExists("Assets/NPC/Waves");
        string devWavePath = "Assets/NPC/Waves/DEV_TestWave.asset";
        WaveData devWave = AssetDatabase.LoadAssetAtPath<WaveData>(devWavePath);
        if (devWave == null)
        {
            devWave = ScriptableObject.CreateInstance<WaveData>();
            AssetDatabase.CreateAsset(devWave, devWavePath);
        }
        devWave.mobsInWave = new List<MobData> { mob };
        devWave.possibleLootPool = new List<ItemData>();
        EditorUtility.SetDirty(devWave);
        AssetDatabase.SaveAssets();
        LaunchCombatSceneWithWave(devWave);
    }

    private void LaunchCombatSceneWithWave(WaveData waveToTest)
    {
        SaveData devData = new SaveData();
        devData.savedPlayerMaxHP = 999;
        devData.savedPlayerHP = 999;
        devData.savedTokensAttack = 15;
        devData.savedTokensDefense = 15;
        devData.savedPlayerCoins = 9999;
        devData.targetWaveName = waveToTest.name;
        devData.savedMobIndex = 0;

        string json = JsonUtility.ToJson(devData);
        string encrypted = EncryptDevSave(json);
        File.WriteAllText(Application.persistentDataPath + "/savegame.sav", encrypted);

        string[] guids = AssetDatabase.FindAssets("CombatScene t:Scene", new[] { "Assets" });
        if (guids.Length == 0) return;
        string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            CombatManager cm = FindAnyObjectByType<CombatManager>();
            if (cm != null)
            {
                if (cm.allPossibleWaves == null) cm.allPossibleWaves = new List<WaveData>();
                if (!cm.allPossibleWaves.Contains(waveToTest))
                {
                    cm.allPossibleWaves.Add(waveToTest);
                    EditorUtility.SetDirty(cm);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            EditorApplication.isPlaying = true;
        }
    }

    private static byte[] GetDynamicKey() { return Encoding.UTF8.GetBytes("Sup3rT4jn3aCk1410!H4sl0_XxJpW1@3"); }
    private static byte[] GetDynamicIV()
    {
        int[] shiftedAscii = new int[] { 77, 111, 113, 55, 21, 12, 102, 13, 99, 8, 1, 11, 102, 44, 49, 42 };
        string result = "";
        for (int i = 0; i < shiftedAscii.Length; i++) result += (char)shiftedAscii[i];
        return Encoding.UTF8.GetBytes(result);
    }
    private string EncryptDevSave(string plainText)
    {
        byte[] bText = Encoding.UTF8.GetBytes(plainText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetDynamicKey(); aes.IV = GetDynamicIV();
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) { cs.Write(bText, 0, bText.Length); }
                return System.Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private void CreateNewAssetForCurrentTab()
    {
        switch (selectedTab)
        {
            case 0: CreateAsset<ItemData>("NowyPrzedmiot", "Assets/Player/Items"); break;
            case 1: CreateAsset<MobData>("NowyPotwor", "Assets/NPC/Mobs"); break;
            case 2: CreateAsset<WaveData>("NowaFala", "Assets/NPC/Waves"); break;
            case 3: CreateAsset<PathData>("NowaSciezka", "Assets/NPC/Paths"); break;
            case 4: CreateAsset<Card>("NowaKarta", "Assets/Cards/Clear_Cards"); break;
            case 5: CreateAsset<CardUpgradeData>("NoweUlepszenie", "Assets/Blacksmith/CardUpgrades"); break;
            case 6: CreateAsset<PlayerClassData>("NowaKlasa", "Assets/Player/Classes"); break;
            case 7: CreateAsset<MapThemeData>("NowyMotyw", "Assets/Environment/MapThemes"); break;
            case 8: CreateAsset<MealData>("NowyPosilek", "Assets/Tavern/Meals"); break; // DODANE
        }
    }

    private void CreateAsset<T>(string defaultName, string defaultFolderPath) where T : ScriptableObject
    {
        EnsureFolderExists(defaultFolderPath);
        string path = EditorUtility.SaveFilePanelInProject("Utwórz " + typeof(T).Name, defaultName, "asset", "Wybierz lokalizację", defaultFolderPath);
        if (string.IsNullOrEmpty(path)) return;

        T newAsset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newAsset, path);
        AssetDatabase.SaveAssets();

        SyncAllDatabases();

        activeItem = newAsset;
        if (activeEditor != null) DestroyImmediate(activeEditor);
        activeEditor = Editor.CreateEditor(activeItem);
    }

    private void EnsureFolderExists(string folderPath)
    {
        string[] folders = folderPath.Split('/');
        string currentPath = folders[0];
        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(nextPath)) AssetDatabase.CreateFolder(currentPath, folders[i]);
            currentPath = nextPath;
        }
    }

    private Texture2D GetIconForAsset(ScriptableObject asset)
    {
        if (asset is ItemData item) return item.itemIcon != null ? item.itemIcon.texture : null;
        if (asset is MobData mob) return mob.mobGraphic != null ? mob.mobGraphic.texture : null;
        if (asset is PathData path) return path.pathIcon != null ? path.pathIcon.texture : null;
        if (asset is Card card) return card.graphic != null ? card.graphic.texture : null;
        if (asset is CardUpgradeData upg) return upg.icon != null ? upg.icon.texture : null;
        if (asset is PlayerClassData cls) return cls.classIcon != null ? cls.classIcon.texture : null;
        if (asset is MapThemeData theme) return theme.staticBackground != null ? theme.staticBackground.texture : null;
        if (asset is MealData meal) return meal.mealIcon != null ? meal.mealIcon.texture : null; // DODANE
        return null;
    }

    private string GetDisplayNameForAsset(ScriptableObject asset)
    {
        if (asset is ItemData item && !string.IsNullOrEmpty(item.itemName)) return item.itemName;
        if (asset is MobData mob && !string.IsNullOrEmpty(mob.mobName)) return mob.mobName;
        if (asset is PathData path && !string.IsNullOrEmpty(path.pathName)) return path.pathName;
        if (asset is PlayerClassData cls && !string.IsNullOrEmpty(cls.className)) return cls.className;
        if (asset is CardUpgradeData upg && !string.IsNullOrEmpty(upg.upgradeName)) return upg.upgradeName;
        if (asset is MapThemeData theme && !string.IsNullOrEmpty(theme.themeID)) return theme.themeID;
        if (asset is MealData meal && !string.IsNullOrEmpty(meal.mealName)) return meal.mealName; // DODANE
        return asset.name;
    }
}