using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public ItemData item;
    [HideInInspector] public ItemTooltip tooltip;
    [HideInInspector] public InventoryUI inventoryUI;

    [HideInInspector] public bool successfullyEquipped = false;

    private Transform originalParent;
    private int originalSiblingIndex;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex(); // Zapisujemy pozycje w plecaku

        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            transform.SetParent(inventoryUI.inventoryPanel.transform, true);
        }
        else
        {
            transform.SetParent(transform.root, true);
        }

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        if (tooltip != null) tooltip.HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 globalMousePos))
        {
            transform.position = globalMousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (successfullyEquipped)
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup.blocksRaycasts = true;

        // Zwracamy przedmiot na jego pierwotne miejsce
        if (inventoryUI != null && transform.parent == inventoryUI.inventoryPanel.transform)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex); // Przesuwamy go tam gdzie by³
        }
        else if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex); // Przesuwamy go tam gdzie by³
        }

        transform.localScale = Vector3.one; // Zabezpieczenie rozmiaru po puszczeniu
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && item != null && eventData.dragging == false) tooltip.ShowTooltip(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.HideTooltip();
    }
}