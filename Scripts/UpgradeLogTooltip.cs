using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UpgradeLogTooltip : MonoBehaviour
{
    public GameObject tooltipPanel;
    public Transform cardsContainer;
    public GameObject tooltipCardPrefab; // Prefab z grafik¹ karty i tekstem

    public Vector3 offset = new Vector3(20f, -20f, 0f);

    void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // Tutaj przypisujesz pozycjê, uwzglêdniaj¹c swój ewentualny offset
            transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }
    }

    public void ShowTooltip(List<Sprite> cardSprites, List<int> cardStacks)
    {
        if (tooltipPanel == null || cardsContainer == null || tooltipCardPrefab == null) return;

        // Czyœcimy stare wpisy
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

        // Generujemy listê kart podpiêtych pod ten buff
        for (int i = 0; i < cardSprites.Count; i++)
        {
            GameObject newEntry = Instantiate(tooltipCardPrefab, cardsContainer);

            Image img = newEntry.GetComponent<Image>();
            TextMeshProUGUI txt = newEntry.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null) img.sprite = cardSprites[i];
            if (txt != null) txt.text = "x" + cardStacks[i];
        }

        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}