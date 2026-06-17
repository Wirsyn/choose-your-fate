using UnityEngine;
using UnityEngine.EventSystems;

public class MysteryDropZone : MonoBehaviour, IDropHandler
{
    public MysteryManager mysteryManager;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Sprawdzamy czy to przedmiot
            DragDropItem draggedItem = eventData.pointerDrag.GetComponent<DragDropItem>();
            if (draggedItem != null && draggedItem.item != null)
            {
                mysteryManager.StageItemForPenalty(draggedItem.item);
                draggedItem.successfullyEquipped = true;
                return;
            }

            // Sprawdzamy czy to serce
            DraggableHeart draggedHeart = eventData.pointerDrag.GetComponent<DraggableHeart>();
            if (draggedHeart != null)
            {
                mysteryManager.StageHeartForPenalty(draggedHeart);
                Destroy(draggedHeart.gameObject);
            }
        }
    }
}