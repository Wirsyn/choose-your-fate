using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public EquipmentManager eqManager;
    public GameObject inventoryPanel;
    public ItemTooltip tooltipManager;

    [Header("Skrypty Slotów Ekwipunku (T³a/Ramki)")]
    public EquipSlotUI weaponSlot;
    public EquipSlotUI shieldSlot;
    public EquipSlotUI helmetSlot;
    public EquipSlotUI armorSlot;
    public EquipSlotUI trinketSlot;
    public EquipSlotUI ringSlot;
    public EquipSlotUI potionSlot;
    public EquipSlotUI bootsSlot;
    public EquipSlotUI glovesSlot; // DODANE

    [Header("Ikonki Za³o¿onych Przedmiotów (Obiekty-Dzieci)")]
    public Image weaponIcon;
    public Image shieldIcon;
    public Image helmetIcon;
    public Image armorIcon;
    public Image trinketIcon;
    public Image ringIcon;
    public Image potionIcon;
    public Image bootsIcon;
    public Image glovesIcon; // DODANE

    [Header("Wygl¹d Zwyk³ego Plecaka")]
    public Sprite emptySlotSprite;

    [Header("G³ówny Plecak")]
    public Transform backpackContainer;
    public GameObject backpackItemPrefab;
    public TextMeshProUGUI capacityText;

    [Header("Ma³y Plecak")]
    public Transform smallBackpackContainer;
    public TextMeshProUGUI smallCapacityText;

    [Header("Waluta")]
    public TextMeshProUGUI coinsText;

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (tooltipManager != null) tooltipManager.HideTooltip();

        // --- POPRAWKA: Automatyczne przypisywanie referencji ---
        // Dziêki temu nie musisz rêcznie przypisywaæ InventoryUI oraz Tooltipa do ka¿dego slota w edytorze.
        // Skrypt sam je uzupe³ni, zapobiegaj¹c b³êdom NullReferenceException!
        EquipSlotUI[] allSlots = {
            weaponSlot, shieldSlot, helmetSlot, armorSlot,
            trinketSlot, ringSlot, potionSlot, bootsSlot, glovesSlot
        };

        foreach (EquipSlotUI slot in allSlots)
        {
            if (slot != null)
            {
                slot.inventoryUI = this;
                slot.tooltip = tooltipManager;
            }
        }
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        if (inventoryPanel.activeSelf) RefreshUI();
        else if (tooltipManager != null) tooltipManager.HideTooltip();
    }

    public void RefreshUI()
    {
        if (eqManager == null) return;

        UpdateSlot(weaponIcon, eqManager.equippedWeapon, weaponSlot);
        UpdateSlot(shieldIcon, eqManager.equippedShield, shieldSlot);
        UpdateSlot(helmetIcon, eqManager.equippedHelmet, helmetSlot);
        UpdateSlot(armorIcon, eqManager.equippedArmor, armorSlot);
        UpdateSlot(trinketIcon, eqManager.equippedTrinket, trinketSlot);
        UpdateSlot(ringIcon, eqManager.equippedRing, ringSlot);
        UpdateSlot(potionIcon, eqManager.equippedPotion, potionSlot);
        UpdateSlot(bootsIcon, eqManager.equippedBoots, bootsSlot);
        UpdateSlot(glovesIcon, eqManager.equippedGloves, glovesSlot);

        if (capacityText != null) capacityText.text = $"Slots: {eqManager.GetCurrentBackpackLoad()} / {eqManager.maxBackpackSlots}";
        if (smallCapacityText != null) smallCapacityText.text = $"Small Items: {eqManager.smallBackpack.Count} / {eqManager.maxSmallBackpackSlots}";
        if (coinsText != null && SaveManager.instance != null && SaveManager.instance.currentSave != null)
            coinsText.text = SaveManager.instance.currentSave.savedPlayerCoins.ToString();

        // Rysowanie G³ównego Plecaka
        if (backpackContainer != null)
        {
            foreach (Transform child in backpackContainer) Destroy(child.gameObject);
            for (int i = 0; i < eqManager.maxBackpackSlots; i++)
            {
                GameObject newObj = Instantiate(backpackItemPrefab, backpackContainer);
                newObj.transform.localScale = Vector3.one;
                Image img = newObj.GetComponent<Image>();
                DragDropItem dragScript = newObj.GetComponent<DragDropItem>();
                TextMeshProUGUI stackText = newObj.GetComponentInChildren<TextMeshProUGUI>(true);

                if (i < eqManager.backpack.Count)
                {
                    ItemData item = eqManager.backpack[i];
                    if (img != null) { img.sprite = item.itemIcon; img.color = Color.white; }
                    if (dragScript != null) { dragScript.item = item; dragScript.tooltip = tooltipManager; dragScript.inventoryUI = this; dragScript.enabled = true; }
                    if (stackText != null)
                    {
                        if (item.isStackable && item.currentStack > 1) { stackText.text = item.currentStack.ToString(); stackText.gameObject.SetActive(true); }
                        else stackText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (img != null) { img.sprite = emptySlotSprite; img.color = new Color(1f, 1f, 1f, 0.5f); }
                    if (dragScript != null) { dragScript.item = null; dragScript.enabled = false; }
                    if (stackText != null) stackText.gameObject.SetActive(false);
                }
            }
        }

        // Rysowanie Ma³ego Plecaka
        if (smallBackpackContainer != null)
        {
            foreach (Transform child in smallBackpackContainer) Destroy(child.gameObject);
            for (int i = 0; i < eqManager.maxSmallBackpackSlots; i++)
            {
                GameObject newObj = Instantiate(backpackItemPrefab, smallBackpackContainer);
                newObj.transform.localScale = Vector3.one;
                Image img = newObj.GetComponent<Image>();
                DragDropItem dragScript = newObj.GetComponent<DragDropItem>();
                TextMeshProUGUI stackText = newObj.GetComponentInChildren<TextMeshProUGUI>(true);

                if (i < eqManager.smallBackpack.Count)
                {
                    ItemData item = eqManager.smallBackpack[i];
                    if (img != null) { img.sprite = item.itemIcon; img.color = Color.white; }
                    if (dragScript != null) { dragScript.item = item; dragScript.tooltip = tooltipManager; dragScript.inventoryUI = this; dragScript.enabled = true; }
                    if (stackText != null)
                    {
                        if (item.isStackable && item.currentStack > 1) { stackText.text = item.currentStack.ToString(); stackText.gameObject.SetActive(true); }
                        else stackText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (img != null) { img.sprite = emptySlotSprite; img.color = new Color(1f, 1f, 1f, 0.5f); }
                    if (dragScript != null) { dragScript.item = null; dragScript.enabled = false; }
                    if (stackText != null) stackText.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateSlot(Image iconImage, ItemData item, EquipSlotUI slotScript)
    {
        if (slotScript != null) slotScript.currentItem = item;
        if (iconImage == null || slotScript == null) return;

        Image frameImage = slotScript.GetComponent<Image>();
        bool skipFrameLogic = (slotScript.slotType == EquipmentSlot.Potion);

        if (item != null)
        {
            if (frameImage != null && !skipFrameLogic)
            {
                Color c = frameImage.color;
                c.a = 1f;
                frameImage.color = c;
            }

            iconImage.sprite = item.itemIcon;
            iconImage.color = Color.white;
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            if (frameImage != null && !skipFrameLogic)
            {
                Color c = frameImage.color;
                c.a = 0f;
                frameImage.color = c;
            }

            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
    }

    public void SaveEquipmentState()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            eqManager.SaveEquipment(SaveManager.instance.currentSave);
            SaveManager.instance.SaveToFile(SaveManager.instance.currentSave);
        }
    }
}