using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UpgradeLogItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buffIcon;

    [HideInInspector] public List<Sprite> cardSprites = new List<Sprite>();
    [HideInInspector] public List<int> cardStacks = new List<int>();

    private UpgradeLogTooltip tooltip;

    public void Setup(Sprite buffSprite, UpgradeLogTooltip tooltipRef)
    {
        if (buffIcon != null) buffIcon.sprite = buffSprite;
        tooltip = tooltipRef;
    }

    public void AddCardData(Sprite cardSprite, int stack)
    {
        cardSprites.Add(cardSprite);
        cardStacks.Add(stack);
    }

    // Odpala siê, gdy naje¿d¿asz myszk¹
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.ShowTooltip(cardSprites, cardStacks);
    }

    // Odpala siê, gdy myszka zje¿d¿a z ikony
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null) tooltip.HideTooltip();
    }
}