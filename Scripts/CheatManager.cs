using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitializeGlobalCheats()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameObject cheatObj = new GameObject("[DEV] Global Cheat Manager");
        DontDestroyOnLoad(cheatObj); 
        cheatObj.AddComponent<CheatManager>();
#endif
    }

    void Update()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (Keyboard.current == null) return;

        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        CombatManager combat = FindAnyObjectByType<CombatManager>();
        DeckManager deck = FindAnyObjectByType<DeckManager>();
        BlacksmithManager blacksmith = FindAnyObjectByType<BlacksmithManager>();

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            if (combat != null) { combat.dealerHP = 0; combat.SendMessage("DetermineWinner", SendMessageOptions.DontRequireReceiver); }
            if (deck != null) { deck.dealerHP = 0; deck.SendMessage("DetermineWinner", SendMessageOptions.DontRequireReceiver); }
            Debug.Log("DEV CHEAT: Insta-Kill Moba!");
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            if (combat != null) { combat.playerCoins += 100; combat.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); combat.SendMessage("UpdateCoinsUI", SendMessageOptions.DontRequireReceiver); }
            if (deck != null) { deck.playerCoins += 100; deck.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); deck.SendMessage("UpdateCoinsUI", SendMessageOptions.DontRequireReceiver); }
            
            // --- NAPRAWA CHEATA U KOWALA ---
            if (blacksmith != null) 
            {
                if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
                {
                    SaveManager.instance.currentSave.savedPlayerCoins += 100;
                    SaveManager.instance.SaveToFile(SaveManager.instance.currentSave);
                }
                blacksmith.RefreshCoinsFromSave();
            }

            Debug.Log("DEV CHEAT: +100 Monet!");
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            Time.timeScale = Time.timeScale == 1f ? 3f : 1f;
            Debug.Log("DEV CHEAT: Prêdkoœæ gry x" + Time.timeScale);
        }

        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            int currentHP = 0, maxHP = 0;

            if (combat != null) { combat.playerHP = combat.playerMaxHP; currentHP = combat.playerHP; maxHP = combat.playerMaxHP; combat.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); }
            if (deck != null) { deck.playerHP = deck.playerMaxHP; currentHP = deck.playerHP; maxHP = deck.playerMaxHP; deck.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); }
            
            PlayerAvatar avatar = FindAnyObjectByType<PlayerAvatar>();
            if (avatar != null && maxHP > 0) avatar.UpdateAvatarState(currentHP, maxHP);

            Debug.Log("DEV CHEAT: Uleczono do pe³na!");
        }

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            if (combat != null) combat.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            if (deck != null) deck.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            SceneManager.LoadScene("Blacksmith");
            Debug.Log("DEV CHEAT: Teleport do Kowala!");
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            if (combat != null) combat.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            if (deck != null) deck.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            Debug.Log("DEV CHEAT: Wymuszono zapis!");
        }

        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            if (combat != null) { combat.tokensAttack += 5; combat.tokensDefense += 5; combat.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); }
            if (deck != null) { deck.tokensAttack += 5; deck.tokensDefense += 5; deck.SendMessage("UpdateUI", false, SendMessageOptions.DontRequireReceiver); }
            Debug.Log("DEV CHEAT: Dodano +5 ¯etonów Ataku i +5 ¯etonów Obrony!");
        }
        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            if (combat != null) combat.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            if (deck != null) deck.SendMessage("SaveGame", SendMessageOptions.DontRequireReceiver);
            SceneManager.LoadScene("MysteryScene");
            Debug.Log("DEV CHEAT: Teleport do MysteryScene!");
        }
#endif
    }
}