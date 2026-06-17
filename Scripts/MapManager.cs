using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("Wszystkie Wêz³y Mapy")]
    public List<MapNode> allNodes;

    [Header("Pule Losowañ")]
    public List<PathData> level1Pool;
    public List<PathData> level2PlusPool;
    public PathData bossPath;

    [Header("Wygl¹d Ukrytych/Zablokowanych")]
    public Sprite hiddenIcon;
    public Sprite lockedIcon;
    public Sprite startIcon;

    [Header("Przejœcia i Ekrany")]
    public GameObject loadingPanel;

    private void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);

        InitializeMap();
    }

    private void InitializeMap()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave == null)
        {
            SaveManager.instance.LoadFromFile();
        }

        if (SaveManager.instance != null && SaveManager.instance.currentSave == null)
        {
            SaveManager.instance.currentSave = new SaveData();
        }

        if (SaveManager.instance == null || SaveManager.instance.currentSave == null) return;
        SaveData data = SaveManager.instance.currentSave;

        if (data.mapNodes == null || data.mapNodes.Count == 0)
        {
            GenerateNewMap(data);
        }
        else
        {
            RestoreMap(data);
        }

        UpdateMapVisibility(data);

        // Odœwie¿amy EQ i wczytujemy obiekty PO zaktualizowaniu danych!
        DelayedEQRefresh();
    }

    private void GenerateNewMap(SaveData data)
    {
        Debug.Log("Generujê i zapisujê now¹ siatkê mapy!");
        data.mapNodes = new List<SavedNodeData>();
        data.currentNodeID = 0;

        foreach (MapNode node in allNodes)
        {
            SavedNodeData savedNode = new SavedNodeData();
            savedNode.nodeID = node.nodeID;
            savedNode.isCleared = false;

            if (node.nodeID == 0)
            {
                savedNode.contentName = "START";
                savedNode.isCleared = true;
            }
            else if (node.connectedNodes.Count == 0 && node.nodeLevel > 1)
            {
                if (bossPath != null) savedNode.contentName = bossPath.name;
            }
            else if (node.nodeLevel == 1)
            {
                if (level1Pool != null && level1Pool.Count > 0)
                {
                    PathData randomPath = level1Pool[Random.Range(0, level1Pool.Count)];
                    savedNode.contentName = randomPath.name;
                }
            }
            else
            {
                if (level2PlusPool != null && level2PlusPool.Count > 0)
                {
                    PathData randomPath = level2PlusPool[Random.Range(0, level2PlusPool.Count)];
                    savedNode.contentName = randomPath.name;
                }
            }

            data.mapNodes.Add(savedNode);
        }
        SaveManager.instance.SaveToFile(data);
        RestoreMap(data);
    }

    private void RestoreMap(SaveData data)
    {
        foreach (MapNode node in allNodes)
        {
            SavedNodeData savedNode = data.mapNodes.Find(n => n.nodeID == node.nodeID);
            if (savedNode != null)
            {
                if (savedNode.contentName == "START") continue;

                PathData foundPath = null;
                if (level1Pool != null) foundPath = level1Pool.Find(p => p.name == savedNode.contentName);
                if (foundPath == null && level2PlusPool != null) foundPath = level2PlusPool.Find(p => p.name == savedNode.contentName);
                if (foundPath == null && bossPath != null && bossPath.name == savedNode.contentName) foundPath = bossPath;

                node.assignedPath = foundPath;

                node.nodeButton.onClick.RemoveAllListeners();
                node.nodeButton.onClick.AddListener(() => OnNodeClicked(node, savedNode, data));
            }
        }
    }

    private void UpdateMapVisibility(SaveData data)
    {
        MapNode currentNode = allNodes.Find(n => n.nodeID == data.currentNodeID);
        List<MapNode> availableNodes = currentNode != null ? currentNode.connectedNodes : new List<MapNode>();

        foreach (MapNode node in allNodes)
        {
            SavedNodeData savedNode = data.mapNodes.Find(n => n.nodeID == node.nodeID);
            if (savedNode == null) continue;

            bool isBoss = (node.connectedNodes.Count == 0 && node.nodeLevel > 1);

            if (node.nodeID == 0)
            {
                node.iconImage.sprite = startIcon;
                node.nodeButton.interactable = false;
                continue;
            }

            if (savedNode.isCleared)
            {
                if (node.assignedPath != null && node.assignedPath.pathIcon != null)
                    node.iconImage.sprite = node.assignedPath.pathIcon;

                node.iconImage.color = Color.gray;
                node.nodeButton.interactable = false;
            }
            else if (availableNodes.Contains(node))
            {
                if (node.assignedPath != null && node.assignedPath.pathIcon != null)
                    node.iconImage.sprite = node.assignedPath.pathIcon;

                node.iconImage.color = Color.white;
                node.nodeButton.interactable = true;
            }
            else if (node.nodeLevel <= currentNode.nodeLevel)
            {
                node.iconImage.sprite = lockedIcon;
                node.iconImage.color = new Color(1f, 1f, 1f, 0.5f);
                node.nodeButton.interactable = false;
            }
            else if (isBoss)
            {
                if (node.assignedPath != null && node.assignedPath.pathIcon != null)
                    node.iconImage.sprite = node.assignedPath.pathIcon;

                node.iconImage.color = Color.white;
                node.nodeButton.interactable = false;
            }
            else
            {
                node.iconImage.sprite = hiddenIcon;
                node.iconImage.color = Color.white;
                node.nodeButton.interactable = false;
            }
        }
    }

    private void OnNodeClicked(MapNode clickedNode, SavedNodeData savedNode, SaveData data)
    {
        Debug.Log("Wybrano: " + clickedNode.assignedPath.name);
        data.currentNodeID = clickedNode.nodeID;

        // POPRAWKA: Pokoje walki zaznaczaj¹ siê jako ukoñczone DOPIERO, gdy naprawdê j¹ wygrasz!
        if (clickedNode.assignedPath.typeOfPath != PathData.PathType.Combat)
        {
            savedNode.isCleared = true;
        }

        data.currentMapLevel = clickedNode.nodeLevel;

        if (clickedNode.assignedPath.typeOfPath == PathData.PathType.Combat && clickedNode.assignedPath.waveToLoad != null)
        {
            data.targetWaveName = clickedNode.assignedPath.waveToLoad.name;
            data.savedMobIndex = 0;
        }

        SaveManager.instance.SaveToFile(data);

        foreach (MapNode node in allNodes) node.nodeButton.interactable = false;

        if (clickedNode.assignedPath.typeOfPath == PathData.PathType.Combat)
            StartCoroutine(LoadSceneRoutine("CombatScene"));
        else if (clickedNode.assignedPath.typeOfPath == PathData.PathType.Blacksmith)
            StartCoroutine(LoadSceneRoutine("Blacksmith"));
        else if (clickedNode.assignedPath.typeOfPath == PathData.PathType.Mystery)
            StartCoroutine(LoadSceneRoutine("MysteryScene"));
        else if (clickedNode.assignedPath.typeOfPath == PathData.PathType.Tavern)
            StartCoroutine(LoadSceneRoutine("TavernScene"));
    }

    private System.Collections.IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;
    }

    private void DelayedEQRefresh()
    {
        InventoryUI invUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (invUI != null)
        {
            if (SaveManager.instance != null && SaveManager.instance.currentSave != null && invUI.eqManager != null)
            {
                invUI.eqManager.LoadEquipment(SaveManager.instance.currentSave);
            }
            invUI.RefreshUI();
        }
    }
}