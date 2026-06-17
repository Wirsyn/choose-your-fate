using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DraggableToken : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform originalParent; 
    private CanvasGroup canvasGroup;
    public DeckManager.BetType tokenType;
    private Canvas mainCanvas;

    [SerializeField] private Image tokenImage; 
    [SerializeField] private Sprite attackSprite; 
    [SerializeField] private Sprite defenseSprite; 

    
    public void Setup(DeckManager.BetType type)
    {
        tokenType = type;
        if (type == DeckManager.BetType.Attack)
            tokenImage.sprite = attackSprite;
        else
            tokenImage.sprite = defenseSprite;
    }
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(mainCanvas.transform); 
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // T³umaczymy piksele myszki na przestrzeñ 3D kamery
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

        if (transform.parent == mainCanvas.transform)
        {
            transform.SetParent(originalParent);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent as RectTransform);
    }
}