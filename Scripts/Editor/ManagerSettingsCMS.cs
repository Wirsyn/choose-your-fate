using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;

public class ManagerSettingsCMS : EditorWindow
{
    private Vector2 listScrollPos;
    private Vector2 editorScrollPos;

    // Struktura pomocnicza by przechować dane o menedżerach z ZAMKNIĘTYCH scen
    private struct SceneManagerRef
    {
        public string scenePath;
        public string sceneName;
        public string gameObjectName;
        public System.Type componentType;
    }

    private List<Component> prefabManagers = new List<Component>();
    // Grupowanie lokalnych menedżerów po nazwie sceny, z której pochodzą
    private Dictionary<string, List<SceneManagerRef>> sceneManagersGrouped = new Dictionary<string, List<SceneManagerRef>>();

    private Component selectedManager = null;
    private Editor currentEditor = null;

    [MenuItem("Narzędzia Developera/⚙️ Konfigurator Menedżerów (Ustawienia)")]
    public static void ShowWindow()
    {
        ManagerSettingsCMS window = GetWindow<ManagerSettingsCMS>("Ustawienia Menedżerów");
        window.minSize = new Vector2(850, 550);
    }

    private void OnEnable()
    {
        RefreshManagers();
    }

    private System.Type[] GetKnownManagerTypes()
    {
        return new System.Type[]
        {
            typeof(CombatManager),
            typeof(DeckManager),
            typeof(BlacksmithManager),
            typeof(LaboratoryManager),
            typeof(MainMenuManager),
            typeof(MapManager),
            typeof(MysteryManager),
            typeof(PauseManager),
            typeof(EquipmentManager),
            typeof(OptionsManager),
            typeof(SaveManager),
            typeof(CheatManager),
            typeof(TavernManager)
        };
    }

    private void RefreshManagers()
    {
        prefabManagers.Clear();
        sceneManagersGrouped.Clear();
        selectedManager = null;
        if (currentEditor != null) DestroyImmediate(currentEditor);

        System.Type[] typesToFind = GetKnownManagerTypes();

        // 1. SZUKANIE W PREFABACH
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            foreach (System.Type type in typesToFind)
            {
                Component[] comps = prefab.GetComponentsInChildren(type, true);
                foreach (Component c in comps) prefabManagers.Add(c);
            }
        }
        prefabManagers.Sort((a, b) => string.Compare(a.GetType().Name, b.GetType().Name));

        // 2. SZUKANIE NA WSZYSTKICH SCENACH W TLE
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            string sName = System.IO.Path.GetFileNameWithoutExtension(path);

            // Jeśli gra jest włączona, nie pozwalamy na skanowanie innych scen w tle (by jej nie zepsuć)
            if (Application.isPlaying && path != activeScene.path) continue;

            EditorUtility.DisplayProgressBar("Skanowanie Projektu", $"Przeszukuję scenę: {sName}", (float)i / sceneGuids.Length);

            bool isCurrentlyOpen = (path == activeScene.path);
            UnityEngine.SceneManagement.Scene tempScene;

            if (!isCurrentlyOpen) tempScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            else tempScene = activeScene;

            List<SceneManagerRef> managersInThisScene = new List<SceneManagerRef>();

            foreach (System.Type type in typesToFind)
            {
                foreach (GameObject root in tempScene.GetRootGameObjects())
                {
                    Component[] comps = root.GetComponentsInChildren(type, true);
                    foreach (Component c in comps)
                    {
                        if (!PrefabUtility.IsPartOfPrefabAsset(c.gameObject))
                        {
                            managersInThisScene.Add(new SceneManagerRef()
                            {
                                scenePath = path,
                                sceneName = sName,
                                gameObjectName = c.gameObject.name,
                                componentType = type
                            });
                        }
                    }
                }
            }

            if (managersInThisScene.Count > 0)
            {
                managersInThisScene.Sort((a, b) => string.Compare(a.componentType.Name, b.componentType.Name));
                sceneManagersGrouped[sName] = managersInThisScene;
            }

            if (!isCurrentlyOpen) EditorSceneManager.CloseScene(tempScene, true);
        }

        EditorUtility.ClearProgressBar();
    }

    private void OnGUI()
    {
        DrawTopToolbar();

        EditorGUILayout.BeginHorizontal();
        DrawLeftColumn();
        DrawRightColumn();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTopToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("🔄 Skanuj Cały Projekt (Odśwież Listę)", EditorStyles.toolbarButton, GUILayout.Width(250)))
        {
            RefreshManagers();
        }

        GUILayout.FlexibleSpace();

        int totalSceneManagers = sceneManagersGrouped.Values.Sum(list => list.Count);
        EditorGUILayout.LabelField($"Znaleziono: {prefabManagers.Count} w Prefabach | {totalSceneManagers} na wszystkich Scenach", EditorStyles.miniLabel);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftColumn()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(280));
        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos, GUI.skin.box);

        // --- SEKCJA PREFABÓW ---
        GUI.color = new Color(0.8f, 0.9f, 1f);
        EditorGUILayout.LabelField("📦 PREFABY (Zapis Globalny)", EditorStyles.boldLabel);
        GUI.color = Color.white;

        if (prefabManagers.Count == 0) EditorGUILayout.HelpBox("Brak menedżerów w prefabach.", MessageType.None);

        foreach (Component comp in prefabManagers)
        {
            if (comp == null) continue;
            GUIStyle style = (selectedManager == comp) ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
            if (GUILayout.Button($"{comp.GetType().Name}  ({comp.gameObject.name})", style))
            {
                SelectManager(comp);
            }
        }

        EditorGUILayout.Space(15);

        // --- SEKCJA KATEGORII SCEN ---
        if (sceneManagersGrouped.Count == 0) EditorGUILayout.HelpBox("Brak menedżerów na zeskanowanych scenach.", MessageType.None);

        foreach (var kvp in sceneManagersGrouped)
        {
            GUI.color = new Color(1f, 0.9f, 0.8f);
            EditorGUILayout.LabelField($"🌍 SCENA: {kvp.Key}", EditorStyles.boldLabel);
            GUI.color = Color.white;

            foreach (var smr in kvp.Value)
            {
                // Sprawdzamy czy to ten aktualnie przez nas edytowany (z użyciem specjalnej flagi)
                bool isSelected = (selectedManager != null && selectedManager.GetType() == smr.componentType && selectedManager.gameObject.name == smr.gameObjectName);
                GUIStyle style = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

                if (GUILayout.Button($"{smr.componentType.Name}  ({smr.gameObjectName})", style))
                {
                    HandleSceneManagerClick(smr);
                }
            }
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SelectManager(Component comp)
    {
        selectedManager = comp;
        if (currentEditor != null) DestroyImmediate(currentEditor);
        currentEditor = Editor.CreateEditor(selectedManager);
        GUI.FocusControl(null);
    }

    private void HandleSceneManagerClick(SceneManagerRef smr)
    {
        // Krok 1: Sprawdzamy czy Scena, z której pochodzi Menedżer jest otwarta. Jeśli nie - otwieramy.
        if (EditorSceneManager.GetActiveScene().path != smr.scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(smr.scenePath);
            }
            else
            {
                return; // Użytkownik zrezygnował ze skoku
            }
        }

        // Krok 2: Scena jest już otwarta. Szukamy naszego menedżera. (TUTAJ JEST POPRAWKA)
        Object[] foundObjects = FindObjectsByType(smr.componentType, FindObjectsInactive.Include);

        foreach (Object o in foundObjects)
        {
            Component c = o as Component;
            if (c != null && c.gameObject.name == smr.gameObjectName)
            {
                SelectManager(c);
                return;
            }
        }

        Debug.LogError($"Nie udało się znaleźć Menedżera {smr.componentType.Name} w obiekcie {smr.gameObjectName}. Zrób Odśwież Listę!");
    }

    private void DrawRightColumn()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

        if (selectedManager != null && currentEditor != null)
        {
            bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(selectedManager.gameObject);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"⚙️ Edytujesz: {selectedManager.GetType().Name}", EditorStyles.largeLabel);

            GUI.color = isPrefab ? new Color(0.3f, 0.6f, 1f) : new Color(1f, 0.6f, 0.3f);
            GUILayout.Label(isPrefab ? "[ZAPISYWANE W PREFABIE]" : "[ZAPISYWANE W SCENIE]", EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            currentEditor.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedManager);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pokaż obiekt w Hierarchii / Projekcie", GUILayout.Height(30)))
            {
                Selection.activeGameObject = selectedManager.gameObject;
                EditorGUIUtility.PingObject(selectedManager.gameObject);
            }

            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("💾 Wymuś Zapis Zmian", GUILayout.Height(30)))
            {
                if (isPrefab)
                {
                    PrefabUtility.SavePrefabAsset(selectedManager.gameObject.transform.root.gameObject);
                    Debug.Log("Zapisano zmiany w Prefabie!");
                }
                else
                {
                    EditorSceneManager.MarkSceneDirty(selectedManager.gameObject.scene);
                    EditorSceneManager.SaveScene(selectedManager.gameObject.scene);
                    Debug.Log("Zapisano zmiany na Scenie!");
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Wybierz Menedżera z lewej listy, aby skonfigurować jego zmienne (np. szanse na drop, siłę rzutu, czasy animacji).", MessageType.Info);
            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}