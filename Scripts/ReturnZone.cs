using UnityEngine;
using UnityEngine.EventSystems;

public class ReturnZone : MonoBehaviour, IDropHandler
{
    [Header("Zarz¹dcy (Wykrywani automatycznie)")]
    public DeckManager deckManager;
    public CombatManager combatManager;

    private void Start()
    {
        // Automatycznie szukamy managera obecnego na danej scenie
        if (deckManager == null) deckManager = FindAnyObjectByType<DeckManager>();
        if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableToken droppedToken = eventData.pointerDrag.GetComponent<DraggableToken>();

        if (droppedToken != null)
        {
            // POPRAWKA: U¿ywamy originalParent
            if (droppedToken.originalParent == transform) return;

            // ----------------------------------------------------
            // SCENARIUSZ 1: Jesteœmy w Tutorialu (DeckManager)
            // ----------------------------------------------------
            if (deckManager != null)
            {
                // POPRAWKA: U¿ywamy originalParent do wykrycia, czy ¿eton uciek³ ze strefy zak³adów
                if (droppedToken.originalParent.GetComponent<BettingZone>() != null)
                {
                    if (droppedToken.tokenType == DeckManager.BetType.Attack)
                    {
                        deckManager.currentAttackBet--;
                        deckManager.tokensAttack++;
                    }
                    else if (droppedToken.tokenType == DeckManager.BetType.Defense)
                    {
                        deckManager.currentDefenseBet--;
                        deckManager.tokensDefense++;
                    }
                }

                droppedToken.transform.SetParent(transform);
                droppedToken.transform.SetAsLastSibling();

                Debug.Log($"[Tutorial] Zwrócono ¿eton. Aktualny zak³ad - Atak: {deckManager.currentAttackBet}, Obrona: {deckManager.currentDefenseBet}");
                deckManager.RefreshBetButton();
            }
            // ----------------------------------------------------
            // SCENARIUSZ 2: Jesteœmy w G³ównej Walce (CombatManager)
            // ----------------------------------------------------
            else if (combatManager != null)
            {
                if (droppedToken.originalParent.GetComponent<BettingZone>() != null)
                {
                    if (droppedToken.tokenType == DeckManager.BetType.Attack)
                    {
                        combatManager.currentAttackBet--;
                        combatManager.tokensAttack++;
                    }
                    else if (droppedToken.tokenType == DeckManager.BetType.Defense)
                    {
                        combatManager.currentDefenseBet--;
                        combatManager.tokensDefense++;
                    }
                }

                droppedToken.transform.SetParent(transform);
                droppedToken.transform.SetAsLastSibling();

                Debug.Log($"[Walka] Zwrócono ¿eton. Aktualny zak³ad - Atak: {combatManager.currentAttackBet}, Obrona: {combatManager.currentDefenseBet}");
                combatManager.RefreshBetButton();
            }
        }
    }
}