using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public EquipmentSlot slotType;
    public InventoryUI inventoryUI;
    public ItemTooltip tooltip;

    [HideInInspector] public ItemData currentItem;

    private GameObject dragGhost;

    // Upuszczanie przedmiotu na slot
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj != null)
        {
            DragDropItem draggedItem = droppedObj.GetComponent<DragDropItem>();

            // Sprawdzamy czy to nie jest nasz w³asny Duch przeci¹gania
            if (draggedItem != null && draggedItem.item != null && droppedObj != dragGhost)
            {
                if (draggedItem.item.slotType == slotType)
                {
                    draggedItem.successfullyEquipped = true;

                    inventoryUI.eqManager.EquipFromBackpack(draggedItem.item);
                    inventoryUI.SaveEquipmentState();
                    inventoryUI.RefreshUI();
                    if (tooltip != null) tooltip.HideTooltip();
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem != null && inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            // Tworzymy czystego Ducha
            dragGhost = new GameObject("DragGhost");
            dragGhost.transform.SetParent(inventoryUI.inventoryPanel.transform, false);
            dragGhost.transform.SetAsLastSibling();

            Image ghostImg = dragGhost.AddComponent<Image>();
            ghostImg.sprite = currentItem.itemIcon;
            ghostImg.raycastTarget = false;
            ghostImg.preserveAspect = true;

            RectTransform rt = dragGhost.GetComponent<RectTransform>();

            // Pobieramy fizyczny rozmiar komórki z siatki plecaka, ¿eby ikona zawsze pasowa³a!
            if (inventoryUI.backpackContainer != null)
            {
                GridLayoutGroup grid = inventoryUI.backpackContainer.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    rt.sizeDelta = grid.cellSize;
                }
                else
                {
                    rt.sizeDelta = new Vector2(80, 80);
                }
            }
            else
            {
                rt.sizeDelta = new Vector2(80, 80);
            }

            // Ustawiamy pozycjê startow¹ dok³adnie pod kursorem myszy
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)inventoryUI.inventoryPanel.transform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 globalMousePos))
            {
                dragGhost.transform.position = globalMousePos;
            }

            // Gwarancja, ¿e obiekt nie zostanie sztucznie sp³aszczony przez rodzica
            dragGhost.transform.localScale = Vector3.one;

            if (tooltip != null) tooltip.HideTooltip();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)dragGhost.transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 globalMousePos))
            {
                dragGhost.transform.position = globalMousePos;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        // Zamiast sztywno narzucaæ bia³y kolor i psuæ przezroczystoœæ, ka¿emy mened¿erowi przeliczyæ stan
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && currentItem != null && !eventData.dragging) tooltip.ShowTooltip(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Unequip (œci¹ganie prawym/lewym klikiem) dzia³a tylko, gdy nie przeci¹gamy
        if (currentItem != null && !eventData.dragging)
        {
            inventoryUI.eqManager.UnequipItem(slotType);
            inventoryUI.SaveEquipmentState();
            inventoryUI.RefreshUI();
            if (tooltip != null) tooltip.HideTooltip();
        }
    }
}