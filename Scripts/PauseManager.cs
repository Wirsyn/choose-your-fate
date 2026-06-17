using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Panele UI")]
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;

    private bool isPaused = false;

    void Start()
    {
        // Upewniamy siê, ¿e na starcie panele s¹ wy³¹czone
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // Tê funkcjê podepnij pod ma³y przycisk pauzy na ekranie gry
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);

        // Zatrzymujemy lub wznawiamy up³yw czasu
        Time.timeScale = isPaused ? 0f : 1f;
    }

    // Tê funkcjê podepnij pod przycisk "Resume"
    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Tê funkcjê podepnij pod przycisk "Options"
    public void OpenOptions()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Tê funkcjê podepnij pod przycisk powrotu w panelu opcji
    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    // Tê funkcjê podepnij pod przycisk "Save & Exit"
    public void SaveAndExit()
    {
        // BARDZO WA¯NE: Przywracamy czas przed zmian¹ sceny!
        // Inaczej nowa scena wczyta siê "zamarzniêta".
        Time.timeScale = 1f;

        // Szukamy odpowiedniego mened¿era na obecnej scenie i wymuszamy zapis
        CombatManager combatManager = FindAnyObjectByType<CombatManager>();
        if (combatManager != null) combatManager.SaveGame();

        DeckManager deckManager = FindAnyObjectByType<DeckManager>();
        if (deckManager != null) deckManager.SaveGame();

        // Jeœli z jakiegoœ powodu jesteœmy na scenie bez mened¿era walki:
        if (combatManager == null && deckManager == null)
        {
            if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
            {
                SaveManager.instance.SaveToFile(SaveManager.instance.currentSave);
            }
        }

        // £adujemy Menu G³ówne
        SceneManager.LoadScene("MainMenu");
    }
}