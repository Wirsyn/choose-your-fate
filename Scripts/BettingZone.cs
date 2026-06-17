using UnityEngine;
using UnityEngine.EventSystems;

public class BettingZone : MonoBehaviour, IDropHandler
{
    [Header("Zarz¹dcy (Wybierz jednego)")]
    public DeckManager deckManager;
    public CombatManager combatManager;

    private void Start()
    {
        // Automatyczne wykrywanie managera na scenie, ¿ebyœ nie musia³ za ka¿dym razem przypisywaæ go rêcznie!
        if (deckManager == null) deckManager = FindAnyObjectByType<DeckManager>();
        if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableToken droppedToken = eventData.pointerDrag.GetComponent<DraggableToken>();

        if (droppedToken != null)
        {
            // POPRAWKA: Sprawdzamy originalParent, bo podczas przeci¹gania token jest w Canvasie!
            if (droppedToken.originalParent == transform) return;

            // ----------------------------------------------------
            // SCENARIUSZ 1: Jesteœmy w Tutorialu (DeckManager)
            // ----------------------------------------------------
            if (deckManager != null)
            {
                if ((deckManager.currentAttackBet + deckManager.currentDefenseBet) >= deckManager.maxBetAmount)
                {
                    Debug.Log("Osi¹gniêto limit zak³adów!");
                    return;
                }

                droppedToken.transform.SetParent(transform);
                droppedToken.transform.SetAsLastSibling();

                if (droppedToken.tokenType == DeckManager.BetType.Attack)
                {
                    deckManager.currentAttackBet++;
                    deckManager.tokensAttack--;
                }
                else if (droppedToken.tokenType == DeckManager.BetType.Defense)
                {
                    deckManager.currentDefenseBet++;
                    deckManager.tokensDefense--;
                }

                Debug.Log($"Zak³ad zaktualizowany! Atak: {deckManager.currentAttackBet}, Obrona: {deckManager.currentDefenseBet}");
                deckManager.RefreshBetButton();
            }
            // ----------------------------------------------------
            // SCENARIUSZ 2: Jesteœmy w G³ównej Walce (CombatManager)
            // ----------------------------------------------------
            else if (combatManager != null)
            {
                if ((combatManager.currentAttackBet + combatManager.currentDefenseBet) >= combatManager.maxBetAmount)
                {
                    Debug.Log("Osi¹gniêto limit zak³adów!");
                    return;
                }

                droppedToken.transform.SetParent(transform);
                droppedToken.transform.SetAsLastSibling();

                if (droppedToken.tokenType == DeckManager.BetType.Attack)
                {
                    combatManager.currentAttackBet++;
                    combatManager.tokensAttack--;
                }
                else if (droppedToken.tokenType == DeckManager.BetType.Defense)
                {
                    combatManager.currentDefenseBet++;
                    combatManager.tokensDefense--;
                }

                Debug.Log($"Zak³ad zaktualizowany! Atak: {combatManager.currentAttackBet}, Obrona: {combatManager.currentDefenseBet}");
                combatManager.RefreshBetButton();
            }
        }
    }
}