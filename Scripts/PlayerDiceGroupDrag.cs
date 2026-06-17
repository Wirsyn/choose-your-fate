using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerDiceGroupDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public MysteryManager mysteryManager;

    private Vector3 originalLocalPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Canvas mainCanvas;
    private bool isInitialized = false;

    // Funkcja gwarantuj¹ca, ¿e zapamiêtamy pozycjê bazow¹ zanim cokolwiek siê zepsuje
    public void Initialize()
    {
        if (isInitialized) return;

        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // Wymuszamy znalezienie Canvasu, nawet jeœli jest wy³¹czony
        mainCanvas = GetComponentInParent<Canvas>(true);
        originalParent = transform.parent;
        originalLocalPosition = rectTransform.localPosition;

        isInitialized = true;
    }

    void Awake()
    {
        Initialize();
    }

    public void ResetPosition()
    {
        Initialize();
        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            rectTransform.localPosition = originalLocalPosition;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Initialize();

        if (mainCanvas != null)
            transform.SetParent(mainCanvas.transform, true);
        else if (originalParent != null)
            transform.SetParent(originalParent.parent, true);

        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
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
        canvasGroup.blocksRaycasts = true;

        if (mysteryManager != null && mysteryManager.IsPointerOverBoard(eventData.position))
        {
            Vector2 dropLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mysteryManager.diceBoardRect,
                eventData.position,
                eventData.pressEventCamera,
                out dropLocalPos
            );

            // KLUCZOWE: Zwracamy grupê na miejsce NATYCHMIAST, zanim rzut rzuci ewentualnym b³êdem!
            ResetPosition();

            // Nastêpnie odpalamy rzut (koœci wyjd¹ z grupy i wejd¹ na stó³)
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            mysteryManager.StartPhysicalDiceRoll(dropLocalPos);
        }
        else
        {
            ResetPosition();
        }
    }
}