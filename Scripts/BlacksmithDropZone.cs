using UnityEngine;
using UnityEngine.EventSystems;

public class BlacksmithDropZone : MonoBehaviour, IDropHandler
{
    public BlacksmithManager blacksmithManager;

    public enum BlacksmithZoneType { Enhance, Sell, Repair }

    [Tooltip("Wybierz do jakiego okienka podpiêty jest ten slot.")]
    public BlacksmithZoneType zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragDropItem draggedItem = eventData.pointerDrag.GetComponent<DragDropItem>();

            if (draggedItem != null && draggedItem.item != null)
            {
                if (zoneType == BlacksmithZoneType.Enhance)
                {
                    blacksmithManager.SetItemForEnhance(draggedItem.item);
                }
                else if (zoneType == BlacksmithZoneType.Sell)
                {
                    blacksmithManager.SetItemForSell(draggedItem.item);
                }
                else if (zoneType == BlacksmithZoneType.Repair)
                {
                    blacksmithManager.SetItemForRepair(draggedItem.item);
                }

                draggedItem.successfullyEquipped = true;
            }
        }
    }
}