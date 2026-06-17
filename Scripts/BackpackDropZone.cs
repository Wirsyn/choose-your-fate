using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackDropZone : MonoBehaviour, IDropHandler
{
    public InventoryUI inventoryUI;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Sprawdzamy, czy upuszczony obiekt przyszed³ z naszego slota ekwipunku
            EquipSlotUI slot = eventData.pointerDrag.GetComponent<EquipSlotUI>();

            if (slot != null && slot.currentItem != null)
            {
                inventoryUI.eqManager.UnequipItem(slot.slotType);
                inventoryUI.SaveEquipmentState();
                inventoryUI.RefreshUI();

                if (slot.tooltip != null) slot.tooltip.HideTooltip();
            }
        }
    }
}