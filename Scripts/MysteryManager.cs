using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MysteryLootItem
{
    public ItemData item;
    [Range(0f, 100f)] public float dropWeight;
}

public class MysteryManager : MonoBehaviour
{
    [Header("Wymagane Mened¿ery")]
    public EquipmentManager eqManager;
    public InventoryUI inventoryUI;
    public GameObject canvasUI;

    [Header("Stan gracza")]
    public int playerMaxHP;
    public int playerHP;
    public Transform heartContainer;
    public GameObject draggableHeartPrefab;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Ekrany i Przejœcia")]
    public GameObject loadingPanel;

    [Header("Wydarzenie 1: Nieznajomy (Start)")]
    public GameObject strangerPanel;

    [Header("Minigra w Koœci - Referencje UI")]
    public GameObject diceMinigamePanel;
    public RectTransform diceBoardRect;
    public PlayerDiceGroupDrag playerDiceDragGroup;
    public Image[] playerDiceImages = new Image[2];
    public Image[] strangerDiceImages = new Image[2];

    [Header("Ustawienia Fizyki Rzutu")]
    [Tooltip("Bazowa prêdkoœæ z jak¹ koœci wystrzel¹ w losowych kierunkach")]
    public float throwForce = 800f;
    [Tooltip("Wspó³czynnik tarcia sto³u. Im mniejszy, tym szybciej koœci siê zatrzymaj¹ (np. 0.98f)")]
    public float tableFriction = 0.982f;
    [Tooltip("Jak szybko (w sekundach) koœci zmieniaj¹ klatki podczas turlania (np. 0.05)")]
    public float spriteChangeInterval = 0.06f;

    [Header("Wymiary Granic Sto³u (Local Space)")]
    [Tooltip("Po³owa szerokoœci wewnêtrznego pola sto³u, od którego koœci siê odbij¹")]
    public float boardHalfWidth = 320f;
    [Tooltip("Po³owa wysokoœci wewnêtrznego pola sto³u, od którego koœci siê odbij¹")]
    public float boardHalfHeight = 180f;

    [Header("Grafiki Kostek")]
    public Sprite[] playerDiceSprites = new Sprite[6];
    public Sprite[] strangerDiceSprites = new Sprite[6];

    [Header("Wynik Koœci i Nagroda")]
    public GameObject diceResultPanel;
    public TextMeshProUGUI diceResultText;

    public List<MysteryLootItem> strangerRewardPool;

    public GameObject afterRewardLeaveButton;

    [Header("Kara Nieznajomego - Zap³ata")]
    public GameObject strangerPenaltyPanel;
    public MysteryDropZone penaltyDropZone;
    public Sprite emptySlotIcon;
    public Image penaltySlotIcon;
    public Button confirmPenaltyButton;

    [Header("Wydarzenie 2: Skrzynka")]
    public GameObject chestPanel;
    public GameObject chestResultPanel;
    public TextMeshProUGUI chestResultText;
    public Image chestRewardImage;

    [Range(0, 100)] public float mimicChance = 20f;
    public List<MysteryLootItem> chestLootPool;

    private ItemData stagedItem = null;
    private bool isStagingHeart = false;
    private bool stagedHeartIsFull = false;

    private void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);

        if (strangerPanel != null) strangerPanel.SetActive(false);
        if (diceMinigamePanel != null) diceMinigamePanel.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (diceResultPanel != null) diceResultPanel.SetActive(false);
        if (strangerPenaltyPanel != null) strangerPenaltyPanel.SetActive(false);
        if (chestResultPanel != null) chestResultPanel.SetActive(false);

        if (penaltySlotIcon != null) penaltySlotIcon.gameObject.SetActive(false);
        if (confirmPenaltyButton != null) confirmPenaltyButton.interactable = false;

        LoadPlayerData();
        RollMysteryEvent();
    }

    private void LoadPlayerData()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            SaveData data = SaveManager.instance.currentSave;
            if (eqManager != null) eqManager.LoadEquipment(data);

            playerHP = data.savedPlayerHP;
            playerMaxHP = data.savedPlayerMaxHP;

            RefreshHeartsUI();
            DelayedEQRefresh();
        }
    }

    private void RefreshHeartsUI()
    {
        if (heartContainer == null) return;

        foreach (Transform child in heartContainer) Destroy(child.gameObject);

        for (int i = 0; i < playerMaxHP; i++)
        {
            GameObject heart = Instantiate(draggableHeartPrefab, heartContainer);
            Image img = heart.GetComponent<Image>();
            DraggableHeart dragScript = heart.GetComponent<DraggableHeart>();

            bool isFull = (i < playerHP);
            if (img != null) img.sprite = isFull ? fullHeartSprite : emptyHeartSprite;
            if (dragScript != null)
            {
                dragScript.mysteryManager = this;
                dragScript.isFull = isFull;
            }
        }
    }

    private void RollMysteryEvent()
    {
        int roll = Random.Range(0, 2);
        if (roll == 0 && strangerPanel != null) strangerPanel.SetActive(true);
        else if (chestPanel != null) chestPanel.SetActive(true);
    }

    public void OpenDiceMinigame()
    {
        if (strangerPanel != null) strangerPanel.SetActive(false);
        if (canvasUI != null) canvasUI.SetActive(false);
        if (diceMinigamePanel != null) diceMinigamePanel.SetActive(true);

        if (playerDiceDragGroup != null)
        {
            playerDiceDragGroup.mysteryManager = this;
            playerDiceDragGroup.ResetPosition();

            if (playerDiceImages != null && playerDiceImages.Length >= 2)
            {
                if (playerDiceImages[0] != null)
                {
                    playerDiceImages[0].transform.SetParent(playerDiceDragGroup.transform, false);
                    playerDiceImages[0].gameObject.SetActive(true);
                    playerDiceImages[0].rectTransform.localPosition = new Vector3(-45f, 0f, 0f);
                }
                if (playerDiceImages[1] != null)
                {
                    playerDiceImages[1].transform.SetParent(playerDiceDragGroup.transform, false);
                    playerDiceImages[1].gameObject.SetActive(true);
                    playerDiceImages[1].rectTransform.localPosition = new Vector3(45f, 0f, 0f);
                }
            }

            if (strangerDiceImages != null && strangerDiceImages.Length >= 2)
            {
                if (strangerDiceImages[0] != null)
                {
                    strangerDiceImages[0].gameObject.SetActive(true);
                    strangerDiceImages[0].rectTransform.anchoredPosition = new Vector2(240f, 140f);
                }
                if (strangerDiceImages[1] != null)
                {
                    strangerDiceImages[1].gameObject.SetActive(true);
                    strangerDiceImages[1].rectTransform.anchoredPosition = new Vector2(320f, 140f);
                }
            }
        }
    }

    public bool IsPointerOverBoard(Vector2 screenPosition)
    {
        if (diceBoardRect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(diceBoardRect, screenPosition);
    }

    public void StartPhysicalDiceRoll(Vector2 localDropPosition)
    {
        StartCoroutine(PhysicalRollSequence(localDropPosition));
    }

    private IEnumerator PhysicalRollSequence(Vector2 startPlayerPos)
    {
        int p1Result = Random.Range(1, 7);
        int p2Result = Random.Range(1, 7);
        int s1Result = Random.Range(1, 7);
        int s2Result = Random.Range(1, 7);

        RectTransform[] diceRects = new RectTransform[4];
        if (playerDiceImages[0] != null) diceRects[0] = playerDiceImages[0].rectTransform;
        if (playerDiceImages[1] != null) diceRects[1] = playerDiceImages[1].rectTransform;
        if (strangerDiceImages[0] != null) diceRects[2] = strangerDiceImages[0].rectTransform;
        if (strangerDiceImages[1] != null) diceRects[3] = strangerDiceImages[1].rectTransform;

        if (diceRects[0] != null && diceBoardRect != null) diceRects[0].SetParent(diceBoardRect, false);
        if (diceRects[1] != null && diceBoardRect != null) diceRects[1].SetParent(diceBoardRect, false);

        Vector2[] velocities = new Vector2[4];

        if (diceRects[0] != null) diceRects[0].anchoredPosition = startPlayerPos + new Vector2(-25f, 0f);
        if (diceRects[1] != null) diceRects[1].anchoredPosition = startPlayerPos + new Vector2(25f, 0f);

        if (diceRects[2] != null) diceRects[2].anchoredPosition = new Vector2(-100f, boardHalfHeight - 20f);
        if (diceRects[3] != null) diceRects[3].anchoredPosition = new Vector2(100f, boardHalfHeight - 20f);

        for (int i = 0; i < 4; i++)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            velocities[i] = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * throwForce;
        }

        float animTimer = 0f;
        bool simulationRunning = true;

        while (simulationRunning)
        {
            simulationRunning = false;
            animTimer += Time.deltaTime;
            bool changeSpritesThisFrame = (animTimer >= spriteChangeInterval);
            if (changeSpritesThisFrame) animTimer = 0f;

            for (int i = 0; i < 4; i++)
            {
                if (diceRects[i] == null) continue;

                velocities[i] *= tableFriction;

                if (velocities[i].magnitude > 15f)
                {
                    simulationRunning = true;
                    Vector2 newPos = diceRects[i].anchoredPosition + (velocities[i] * Time.deltaTime);

                    if (newPos.x < -boardHalfWidth) { newPos.x = -boardHalfWidth; velocities[i].x = -velocities[i].x; }
                    else if (newPos.x > boardHalfWidth) { newPos.x = boardHalfWidth; velocities[i].x = -velocities[i].x; }

                    if (newPos.y < -boardHalfHeight) { newPos.y = -boardHalfHeight; velocities[i].y = -velocities[i].y; }
                    else if (newPos.y > boardHalfHeight) { newPos.y = boardHalfHeight; velocities[i].y = -velocities[i].y; }

                    diceRects[i].anchoredPosition = newPos;

                    if (changeSpritesThisFrame)
                    {
                        if (i == 0 && playerDiceImages[0] != null) playerDiceImages[0].sprite = playerDiceSprites[Random.Range(0, 6)];
                        else if (i == 1 && playerDiceImages[1] != null) playerDiceImages[1].sprite = playerDiceSprites[Random.Range(0, 6)];
                        else if (i == 2 && strangerDiceImages[0] != null) strangerDiceImages[0].sprite = strangerDiceSprites[Random.Range(0, 6)];
                        else if (i == 3 && strangerDiceImages[1] != null) strangerDiceImages[1].sprite = strangerDiceSprites[Random.Range(0, 6)];
                    }
                }
            }
            yield return null;
        }

        if (playerDiceImages[0] != null) playerDiceImages[0].sprite = playerDiceSprites[p1Result - 1];
        if (playerDiceImages[1] != null) playerDiceImages[1].sprite = playerDiceSprites[p2Result - 1];
        if (strangerDiceImages[0] != null) strangerDiceImages[0].sprite = strangerDiceSprites[s1Result - 1];
        if (strangerDiceImages[1] != null) strangerDiceImages[1].sprite = strangerDiceSprites[s2Result - 1];

        yield return new WaitForSeconds(1.5f);

        if (afterRewardLeaveButton != null) afterRewardLeaveButton.SetActive(false);
        if (diceMinigamePanel != null) diceMinigamePanel.SetActive(false);
        if (canvasUI != null) canvasUI.SetActive(true);
        if (diceResultPanel != null) diceResultPanel.SetActive(true);

        int playerRoll = p1Result + p2Result;
        int strangerRoll = s1Result + s2Result;

        if (playerRoll > strangerRoll)
        {
            if (strangerRewardPool != null && strangerRewardPool.Count > 0)
            {
                ItemData reward = GetRandomStrangerReward();
                if (reward != null)
                {
                    if (eqManager != null) eqManager.AddToBackpack(reward);
                    // POPRAWKA: Angielski tekst
                    if (diceResultText != null) diceResultText.text = $"You won! ({playerRoll} vs {strangerRoll})\nYou received: {reward.itemName}";
                }
            }
            if (afterRewardLeaveButton != null) afterRewardLeaveButton.SetActive(true);
            SaveGame();
        }
        else
        {
            // POPRAWKA: Angielski tekst
            if (diceResultText != null) diceResultText.text = $"You lost... ({playerRoll} vs {strangerRoll})\nPay your debt!";
            if (strangerPenaltyPanel != null) strangerPenaltyPanel.SetActive(true);
            if (canvasUI != null) canvasUI.SetActive(true);
            if (diceMinigamePanel != null) diceMinigamePanel.SetActive(false);
            SaveGame();
        }
    }

    private ItemData GetRandomStrangerReward()
    {
        if (strangerRewardPool == null || strangerRewardPool.Count == 0) return null;
        float totalWeight = 0; foreach (var item in strangerRewardPool) totalWeight += item.dropWeight;
        float randomVal = Random.Range(0, totalWeight); float currentWeight = 0;
        foreach (var item in strangerRewardPool) { currentWeight += item.dropWeight; if (randomVal <= currentWeight) return item.item; }
        return strangerRewardPool[0].item;
    }

    public void StageItemForPenalty(ItemData item)
    {
        if (isStagingHeart) { playerMaxHP++; if (stagedHeartIsFull) playerHP++; isStagingHeart = false; }
        if (stagedItem != null && eqManager != null) eqManager.AddToBackpack(stagedItem);
        stagedItem = item;
        if (eqManager != null) eqManager.RemoveItem(item);
        if (penaltySlotIcon != null) { penaltySlotIcon.sprite = item.itemIcon; penaltySlotIcon.gameObject.SetActive(true); }
        if (confirmPenaltyButton != null) confirmPenaltyButton.interactable = true;
        RefreshHeartsUI(); DelayedEQRefresh();
    }

    public void StageHeartForPenalty(DraggableHeart heart)
    {
        if (isStagingHeart) { playerMaxHP++; if (stagedHeartIsFull) playerHP++; }
        if (stagedItem != null && eqManager != null) { eqManager.AddToBackpack(stagedItem); stagedItem = null; }
        isStagingHeart = true; stagedHeartIsFull = heart.isFull;
        playerMaxHP--; if (stagedHeartIsFull) playerHP--;
        if (penaltySlotIcon != null) { penaltySlotIcon.sprite = stagedHeartIsFull ? fullHeartSprite : emptyHeartSprite; penaltySlotIcon.gameObject.SetActive(true); }
        if (confirmPenaltyButton != null) confirmPenaltyButton.interactable = true;
        RefreshHeartsUI(); DelayedEQRefresh();
    }

    public void ConfirmPenalty()
    {
        if (stagedItem != null) { stagedItem = null; SaveGame(); LeaveScene(); }
        else if (isStagingHeart) { isStagingHeart = false; SaveGame(); if (playerMaxHP <= 0) { if (SaveManager.instance != null) SaveManager.instance.DeleteSaveData(); SceneManager.LoadScene("MainMenu"); } else LeaveScene(); }
    }

    public void CancelPenalty()
    {
        if (isStagingHeart) { playerMaxHP++; if (stagedHeartIsFull) playerHP++; isStagingHeart = false; RefreshHeartsUI(); }
        if (stagedItem != null && eqManager != null) { eqManager.AddToBackpack(stagedItem); stagedItem = null; }
        if (confirmPenaltyButton != null) confirmPenaltyButton.interactable = false;
        if (penaltySlotIcon != null) penaltySlotIcon.sprite = emptySlotIcon;
        DelayedEQRefresh();
    }

    public void OpenChest()
    {
        if (chestResultPanel != null) chestResultPanel.SetActive(true);
        float roll = Random.Range(0f, 100f);

        if (roll <= mimicChance)
        {
            StealRandomItems();
            // POPRAWKA: Angielski tekst
            if (chestResultText != null) chestResultText.text = "It was a Mimic!\nIt ate some of your equipment.";
            if (chestRewardImage != null) chestRewardImage.gameObject.SetActive(false);
        }
        else
        {
            ItemData reward = GetRandomChestReward();
            if (reward != null)
            {
                if (eqManager != null) eqManager.AddToBackpack(reward);
                // POPRAWKA: Angielski tekst
                if (chestResultText != null) chestResultText.text = "You found:\n" + reward.itemName;
                if (chestRewardImage != null) { chestRewardImage.sprite = reward.itemIcon; chestRewardImage.gameObject.SetActive(true); }
            }
        }
        SaveGame();
    }

    private ItemData GetRandomChestReward()
    {
        if (chestLootPool == null || chestLootPool.Count == 0) return null;
        float totalWeight = 0; foreach (var item in chestLootPool) totalWeight += item.dropWeight;
        float randomVal = Random.Range(0, totalWeight); float currentWeight = 0;
        foreach (var item in chestLootPool) { currentWeight += item.dropWeight; if (randomVal <= currentWeight) return item.item; }
        return chestLootPool[0].item;
    }

    private void StealRandomItems()
    {
        if (eqManager == null) return;
        if (eqManager.backpack.Count > 0) { int r = Random.Range(0, eqManager.backpack.Count); eqManager.backpack.RemoveAt(r); }
        List<EquipmentSlot> equippedSlots = new List<EquipmentSlot>();
        if (eqManager.equippedWeapon != null) equippedSlots.Add(EquipmentSlot.Weapon);
        if (eqManager.equippedShield != null) equippedSlots.Add(EquipmentSlot.Shield);
        if (eqManager.equippedHelmet != null) equippedSlots.Add(EquipmentSlot.Helmet);
        if (eqManager.equippedArmor != null) equippedSlots.Add(EquipmentSlot.Armor);
        if (eqManager.equippedTrinket != null) equippedSlots.Add(EquipmentSlot.Trinket);
        if (eqManager.equippedRing != null) equippedSlots.Add(EquipmentSlot.Ring);
        if (eqManager.equippedBoots != null) equippedSlots.Add(EquipmentSlot.Boots);
        if (eqManager.equippedGloves != null) equippedSlots.Add(EquipmentSlot.Gloves);
        if (equippedSlots.Count > 0) { EquipmentSlot randomSlot = equippedSlots[Random.Range(0, equippedSlots.Count)]; eqManager.UnequipItem(randomSlot, false); }
    }

    public void LeaveScene() { if (loadingPanel != null) loadingPanel.SetActive(true); SceneManager.LoadScene("WorldMap"); }
    private void SaveGame() { if (SaveManager.instance != null && SaveManager.instance.currentSave != null) { SaveData data = SaveManager.instance.currentSave; data.savedPlayerHP = playerHP; data.savedPlayerMaxHP = playerMaxHP; if (eqManager != null) eqManager.SaveEquipment(data); SaveManager.instance.SaveToFile(data); } }
    private void DelayedEQRefresh() { if (inventoryUI != null) inventoryUI.RefreshUI(); }
}