using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class CombatManager : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float cardAnimDuration = 0.7f;
    [Header("Opóźnienia Dźwięków")]
    [Range(0f, 2f)]
    public float shuffleSoundDelay = 0f;
    [Range(0f, 2f)]
    public float dealCardSoundDelay = 0.2f;

    public List<Card> PlayerDeck;
    public List<Card> DealerDeck;
    public CardDisplay cardPrefab;
    public List<CardDisplay> PlayerCardsOnTable = new List<CardDisplay>();
    public List<CardDisplay> DealerCardsOnTable = new List<CardDisplay>();
    public Transform PlayerArea;
    public Transform DealerArea;
    public float offset = 1.5f;
    public List<Card> StartingPlayerDeck;
    public List<Card> StartingDealerDeck;

    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI dealerScoreText;
    public TextMeshProUGUI dealerHPText;
    public TextMeshProUGUI mobInfoText;

    public int currentAttackBet = 0;
    public int currentDefenseBet = 0;
    public int maxBetAmount;

    public int playerHP;
    public int dealerHP;
    public int tokensAttack;
    public int tokensDefense;
    public int playerMaxHP;
    public int dealerMaxHP;
    public int playerArmor;
    public int dealerArmor;

    public GameObject hitButton;
    public GameObject standButton;
    public GameObject betConfirmButton;
    public GameObject restartButton;

    public GameObject heartPrefab;
    public Transform playerHeartArea;
    public Transform dealerHeartArea;
    public List<GameObject> playerHearts = new List<GameObject>();
    public List<GameObject> dealerHearts = new List<GameObject>();
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    public WaveData currentWave;
    public int currentMobIndex = 0;
    public List<WaveData> allPossibleWaves;

    public SpriteRenderer mobGraphicImage;
    public GameObject winPanel;
    public GameObject losePanel;

    public GameObject uiDealerDeck;
    public GameObject uiPlayerDeck;
    public GameObject tableObject;

    public AudioSource audioSource;
    public AudioClip loseSound;
    public AudioClip shuffleSound;
    public AudioClip dealCardSound;

    public DraggableToken tokenPrefab;
    public Transform attackZone;
    public Transform defenseZone;
    public Transform bettingZoneTransform;

    public int playerCoins;
    public int minCoins;
    public int maxCoins;
    public TextMeshProUGUI playerCoinsText;

    public PlayerAvatar playerAvatar;
    public List<PlayerClassData> allPossibleClasses;

    public EquipmentManager eqManager;
    public GameObject loadingPanel;

    public GameObject armorIconPrefab;
    public Transform playerArmorArea;
    public Transform dealerArmorArea;
    public List<GameObject> playerArmorIcons = new List<GameObject>();
    public List<GameObject> dealerArmorIcons = new List<GameObject>();

    public Animator mobAnimator;

    public SpriteRenderer backgroundRenderer;
    public Animator backgroundAnimator;
    public List<MapThemeData> availableThemes;
    public GameObject combatUIContainer;

    [Header("System Ulepszeń Kart")]
    public List<CardUpgradeData> allAvailableUpgrades;

    [Header("Panel Logów Ulepszeń Kart")]
    public GameObject upgradesLogPanel;
    public Transform upgradesLogContainer;
    public GameObject upgradeLogItemPrefab;
    public List<Card> allReferenceCards;
    public UpgradeLogTooltip upgradeTooltip;

    [Header("Miejsca wyświetlania animacji (Spawn Points)")]
    public Transform playerEffectSpawnPoint;
    public Transform enemyEffectSpawnPoint;

    [Header("Wizualne Efekty Standardowe")]
    public GameObject healEffectPrefab;
    public GameObject armorEffectPrefab;

    [Header("Podsumowanie Fali (Wave Summary)")]
    public GameObject waveSummaryPanel;
    public Transform summaryMobsContainer;
    public GameObject summaryMobPrefab;
    public Transform summaryLootContainer;
    public GameObject summaryLootPrefab;
    public TextMeshProUGUI summaryTotalCoinsText;

    public int poisonStacksOnEnemy = 0;
    public int bloodStacksOnEnemy = 0;
    public int freezeTurnsOnEnemy = 0;

    public GameObject avatarObject;
    public GameObject playerHealthBar;
    public GameObject dealerHealthBar;

    private HashSet<string> playedDrawEffectsThisAction = new HashSet<string>();
    private int coinsGainedThisWave = 0;
    public List<MealData> allPossibleMeals;

    void Start()
    {
        // --- POPRAWKA: GWARANCJA WCZYTANIA ZAPISU (DLA CMS) ---
        if (SaveManager.instance == null)
        {
            SaveManager existingSM = FindAnyObjectByType<SaveManager>();
            if (existingSM != null)
            {
                SaveManager.instance = existingSM;
            }
            else
            {
                GameObject smObj = new GameObject("SaveManager");
                SaveManager.instance = smObj.AddComponent<SaveManager>();
            }
            SaveManager.instance.LoadFromFile(); // Awaryjne wczytanie pliku wygenerowanego przez CMS
        }
        else if (SaveManager.instance.currentSave == null)
        {
            SaveManager.instance.LoadFromFile();
        }

        coinsGainedThisWave = 0;
        LoadEnvironmentTheme();
        Shuffle(PlayerDeck);
        StartingPlayerDeck = new List<Card>(PlayerDeck);

        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            ApplyLoadedData();
        }
        else
        {
            if (allPossibleClasses != null && allPossibleClasses.Count > 0)
            {
                PlayerClassData startingClass = allPossibleClasses[0];
                if (eqManager != null) eqManager.InitializeStartingEquipment(startingClass);
                LoadClassVisuals(startingClass.classType);

                playerMaxHP = startingClass.baseMaxHP + (eqManager != null ? eqManager.GetTotalMaxHPBonus() : 0);
                tokensAttack = startingClass.baseAttackTokens + (eqManager != null ? eqManager.GetTotalAttackBonus() : 0);
                tokensDefense = startingClass.baseDefenseTokens + (eqManager != null ? eqManager.GetTotalDefenseBonus() : 0);
            }
            else
            {
                playerMaxHP = 15 + (eqManager != null ? eqManager.GetTotalMaxHPBonus() : 0);
                tokensAttack = 3 + (eqManager != null ? eqManager.GetTotalAttackBonus() : 0);
                tokensDefense = 0 + (eqManager != null ? eqManager.GetTotalDefenseBonus() : 0);
            }

            playerHP = playerMaxHP;
            playerArmor = 0;
            currentMobIndex = 0;
        }

        ChangeUIState(true, false, false);
        CheckAndRestoreBaseTokens();
        GenerateHearts(playerMaxHP, playerHeartArea, playerHearts);
        LoadNextMob();
        GenerateTokens();

        if (maxBetAmount <= 0) maxBetAmount = 2 + (eqManager != null ? eqManager.GetTotalMaxBetBonus() : 0);
        Invoke(nameof(DelayedEQRefresh), 0.1f);

        if (waveSummaryPanel != null) waveSummaryPanel.SetActive(false);
    }

    public void CheckAndRestoreBaseTokens()
    {
        PlayerClassData currentClass = null;
        if (allPossibleClasses != null && allPossibleClasses.Count > 0 && SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            currentClass = allPossibleClasses.Find(c => c.classType == SaveManager.instance.currentSave.savedClassType);
        }
        if (currentClass == null && allPossibleClasses != null && allPossibleClasses.Count > 0) currentClass = allPossibleClasses[0];

        int baseAtk = currentClass != null ? currentClass.baseAttackTokens : 3;
        int baseDef = currentClass != null ? currentClass.baseDefenseTokens : 0;

        int expectedAtk = baseAtk + (eqManager != null ? eqManager.GetTotalAttackBonus() : 0);
        int expectedDef = baseDef + (eqManager != null ? eqManager.GetTotalDefenseBonus() : 0);

        if (tokensAttack < expectedAtk) tokensAttack = expectedAtk;
        if (tokensDefense < expectedDef) tokensDefense = expectedDef;
    }

    void Shuffle(List<Card> deck)
    {
        if (audioSource != null && shuffleSound != null)
        {
            StartCoroutine(PlaySoundDelayed(shuffleSound, shuffleSoundDelay));
        }
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randi = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[randi];
            deck[randi] = temp;
        }
    }

    public Card DrawCard(List<Card> deck)
    {
        if (deck.Count == 0) return null;
        int i = deck.Count - 1;
        Card temp = deck[i];
        deck.RemoveAt(i);
        return temp;
    }

    private IEnumerator DealInitialCardsSequence()
    {
        ChangeUIState(false, false, false);

        Card p1 = DrawCard(PlayerDeck);
        SpawnCard(p1, PlayerArea, PlayerCardsOnTable, uiPlayerDeck.transform.position);
        yield return new WaitForSeconds(0.2f);
        if (dealerHP <= 0) { DetermineWinner(); yield break; }

        Card p2 = DrawCard(PlayerDeck);
        SpawnCard(p2, PlayerArea, PlayerCardsOnTable, uiPlayerDeck.transform.position);
        yield return new WaitForSeconds(0.2f);
        if (dealerHP <= 0) { DetermineWinner(); yield break; }

        Card d1 = DrawCard(DealerDeck);
        SpawnCard(d1, DealerArea, DealerCardsOnTable, uiDealerDeck.transform.position);
        yield return new WaitForSeconds(0.2f);

        Card d2 = DrawCard(DealerDeck);
        SpawnCard(d2, DealerArea, DealerCardsOnTable, uiDealerDeck.transform.position);
        DealerCardsOnTable[1].SetFaceUp(false);

        yield return new WaitForSeconds(0.5f);
        UpdateUI(false);

        if (CalculateScore(PlayerCardsOnTable) >= 21)
            StartCoroutine(DealerTurn());
        else
            ChangeUIState(false, true, false);
    }

    void SpawnCard(Card drawnCard, Transform area, List<CardDisplay> tableList, Vector3 startWorldPos)
    {
        CardDisplay newCard = Instantiate(cardPrefab, area);
        float currentOffset = offset * tableList.Count;
        int initialOrder = 20;
        newCard.CardRender.sortingOrder = initialOrder + tableList.Count;

        Vector3 targetPosition = new Vector3(currentOffset, 0f, 0f);
        Vector3 localStartPosition = area.InverseTransformPoint(startWorldPos);

        newCard.transform.localPosition = localStartPosition;
        newCard.DisplayCard(drawnCard);

        tableList.Add(newCard);

        PlayDealCardSound();
        StartCoroutine(AnimateCardSpawn(newCard.transform, localStartPosition, targetPosition, cardAnimDuration));

        if (area == PlayerArea)
        {
            ApplyOnDrawEffects(drawnCard);
        }
    }

    public void PlayerHit()
    {
        playedDrawEffectsThisAction.Clear();
        Card newPlayerCard = DrawCard(PlayerDeck);
        SpawnCard(newPlayerCard, PlayerArea, PlayerCardsOnTable, uiPlayerDeck.transform.position);
        UpdateUI(false);

        if (dealerHP <= 0) { DetermineWinner(); return; }

        int score = CalculateScore(PlayerCardsOnTable);

        if (score > 21)
        {
            ChangeUIState(false, false, false);
            DetermineWinner();
        }
        else if (score == 21)
        {
            ChangeUIState(false, false, false);
            StartCoroutine(DealerTurn());
        }
    }

    public int CalculateScore(List<CardDisplay> hand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (CardDisplay cardOnTable in hand)
        {
            total += cardOnTable.cardValue.value;
            if (cardOnTable.cardValue.isAce == true) aceCount++;
        }
        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }
        return total;
    }

    public IEnumerator DealerTurn()
    {
        DealerCardsOnTable[1].SetFaceUp(true);
        UpdateUI(true);
        yield return new WaitForSeconds(1f);

        while (CalculateScore(DealerCardsOnTable) < 17)
        {
            if (dealerHP <= 0)
            {
                DetermineWinner();
                yield break;
            }
            Card newDealerCard = DrawCard(DealerDeck);
            SpawnCard(newDealerCard, DealerArea, DealerCardsOnTable, uiDealerDeck.transform.position);
            UpdateUI(true);
            yield return new WaitForSeconds(cardAnimDuration + 0.3f);
        }

        DetermineWinner();
    }

    public void PlayEffect(GameObject prefab, Transform target, float duration = 1.5f)
    {
        if (prefab != null && target != null)
        {
            GameObject fx = Instantiate(prefab, target);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = Vector3.one;

            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
            {
                Renderer[] fxRenderers = fx.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in fxRenderers)
                {
                    r.sortingLayerID = targetRenderer.sortingLayerID;
                    r.sortingOrder = targetRenderer.sortingOrder + 1;
                }
            }

            Destroy(fx, duration);
        }
    }

    private void PlayerWinsRound(int baseDamageMultiplier, int armorMultiplier)
    {
        int bonusDmg = 0;
        int healAmt = 0;
        float lifestealPct = 0f;
        int critMult = 1;
        int bonusArmor = 0;

        CalculateWinBonuses(out bonusDmg, out healAmt, out lifestealPct, out critMult, out bonusArmor);

        int weaponDamage = 0;
        if (currentAttackBet > 0 && eqManager != null)
        {
            weaponDamage = eqManager.GetTotalBaseDamageBonus();
        }

        int baseDmg = currentAttackBet * baseDamageMultiplier;
        int finalDamage = ((baseDmg + weaponDamage) * critMult) + bonusDmg;

        if (finalDamage > 0)
        {
            StartCoroutine(MobDamageFlash());
            PlayMobDamageSound();
            DealDamageToMob(finalDamage);
        }

        int lifestealAmt = 0;
        if (lifestealPct > 0f && finalDamage > 0)
        {
            lifestealAmt = Mathf.Max(1, Mathf.FloorToInt(finalDamage * lifestealPct));
        }

        int totalHeal = healAmt + lifestealAmt;

        if (totalHeal > 0)
        {
            playerHP += totalHeal;
            if (playerHP > playerMaxHP) playerHP = playerMaxHP;
            Transform spawnTarget = playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform);
            PlayEffect(healEffectPrefab, spawnTarget);
        }

        int totalArmorGained = (currentDefenseBet * armorMultiplier) + bonusArmor;
        if (totalArmorGained > 0)
        {
            playerArmor += totalArmorGained;
            Transform spawnTarget = playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform);
            PlayEffect(armorEffectPrefab, spawnTarget);
        }

        tokensAttack += (currentAttackBet * (baseDamageMultiplier + 1));
        tokensDefense += (currentDefenseBet * (armorMultiplier + 1));
    }

    private void EnemyWinsRound(int baseDamageMultiplier)
    {
        PlayMobAttackSound();
        dealerArmor += currentDefenseBet;
        TakeDamage(baseDamageMultiplier);
    }

    private void HandleEndCombatDurability()
    {
        if (eqManager != null)
        {
            List<string> brokenItems = eqManager.DecreaseDurabilityAndGetDestroyed();
            if (brokenItems.Count > 0)
            {
                Debug.Log("Items destroyed due to lack of durability: " + string.Join(", ", brokenItems));
            }
        }
    }

    public void DetermineWinner()
    {
        if (DealerCardsOnTable.Count > 1) DealerCardsOnTable[1].SetFaceUp(true);

        int playerScore = CalculateScore(PlayerCardsOnTable);
        int dealerScore = CalculateScore(DealerCardsOnTable);

        bool playerHasBJ = HasBlackjack(PlayerCardsOnTable);
        bool dealerHasBJ = HasBlackjack(DealerCardsOnTable);

        if (dealerHP <= 0)
        {
            PlayerWinsRound(1, 1);
        }
        else if (playerHasBJ && dealerHasBJ)
        {
            tokensAttack += currentAttackBet;
            tokensDefense += currentDefenseBet;
        }
        else if (playerHasBJ)
        {
            PlayerWinsRound(2, 2);
        }
        else if (dealerHasBJ)
        {
            EnemyWinsRound(2);
        }
        else if (playerScore > 21)
        {
            EnemyWinsRound(1);
        }
        else if (dealerScore > 21)
        {
            PlayerWinsRound(1, 1);
        }
        else if (playerScore > dealerScore)
        {
            PlayerWinsRound(1, 1);
        }
        else if (dealerScore > playerScore)
        {
            EnemyWinsRound(1);
        }
        else
        {
            tokensAttack += currentAttackBet;
            tokensDefense += currentDefenseBet;
        }

        if (playerHP <= 0)
        {
            if (HandleDeathDefiance())
            {
                currentAttackBet = 0;
                currentDefenseBet = 0;

                if (!HasAnyDamageSource()) ChangeUIState(false, false, true);
                else ChangeUIState(false, false, true);

                UpdateUI(true);
                return;
            }

            HandleEndCombatDurability();
            if (SaveManager.instance != null) SaveManager.instance.DeleteSaveData();
            losePanel.SetActive(true);
            ChangeUIState(false, false, true);
            if (audioSource != null && loseSound != null)
            {
                audioSource.PlayOneShot(loseSound);
            }
        }
        else if (dealerHP <= 0)
        {
            HandleEndCombatDurability();
            GiveRandomCoins(minCoins, maxCoins);
            currentMobIndex++;
            currentAttackBet = 0;
            currentDefenseBet = 0;

            PlayerDeck = new List<Card>(StartingPlayerDeck);
            Shuffle(PlayerDeck);

            foreach (CardDisplay card in PlayerCardsOnTable) Destroy(card.gameObject);
            PlayerCardsOnTable.Clear();
            foreach (CardDisplay card in DealerCardsOnTable) Destroy(card.gameObject);
            DealerCardsOnTable.Clear();

            if (currentWave != null && currentMobIndex >= currentWave.mobsInWave.Count)
            {
                ShowWaveSummary();
            }
            else
            {
                StartCoroutine(NextMobOrWinSequence());
            }
        }
        else
        {
            currentAttackBet = 0;
            currentDefenseBet = 0;

            if (!HasAnyDamageSource())
            {
                HandleEndCombatDurability();
                if (SaveManager.instance != null) SaveManager.instance.DeleteSaveData();
                if (losePanel != null) losePanel.SetActive(true);
                ChangeUIState(false, false, true);
                if (audioSource != null && loseSound != null) audioSource.PlayOneShot(loseSound);
            }
            else
            {
                ChangeUIState(false, false, true);
            }
        }
        UpdateUI(true);
    }

    public void TakeDamage(int baseDamageAmount)
    {
        if (freezeTurnsOnEnemy > 0)
        {
            freezeTurnsOnEnemy--;
            return;
        }

        MobData currentMob = currentWave.mobsInWave[currentMobIndex];
        int totalDamage = baseDamageAmount + currentMob.attackBonus;
        int actualDamageToHP = 0;

        if (playerArmor >= totalDamage)
        {
            playerArmor -= totalDamage;
        }
        else
        {
            actualDamageToHP = totalDamage - playerArmor;
            playerArmor = 0;
            playerHP -= actualDamageToHP;
        }

        if (mobAnimator != null) mobAnimator.SetTrigger("AttackTrigger");
        playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
        playerAvatar.PlayHurtAnimation();

        if (currentMob.hasLifesteal && actualDamageToHP > 0)
        {
            int healAmount = Mathf.Max(1, Mathf.FloorToInt(actualDamageToHP * currentMob.lifestealPercentage));

            dealerHP += healAmount;
            if (dealerHP > dealerMaxHP) dealerHP = dealerMaxHP;

            Transform spawnTarget = enemyEffectSpawnPoint != null ? enemyEffectSpawnPoint : (mobGraphicImage != null ? mobGraphicImage.transform : transform);
            PlayEffect(healEffectPrefab, spawnTarget);
        }
    }

    public void PlayerStand()
    {
        ChangeUIState(false, false, false);
        StartCoroutine(DealerTurn());
    }

    void UpdateUI(bool showDealerScore)
    {
        playerScoreText.text = CalculateScore(PlayerCardsOnTable).ToString();

        if (showDealerScore) dealerScoreText.text = CalculateScore(DealerCardsOnTable).ToString();
        else dealerScoreText.text = "?";

        for (int i = 0; i < playerHearts.Count; i++)
        {
            Image heartImage = playerHearts[i].GetComponent<Image>();
            heartImage.sprite = (i < playerHP) ? fullHeartSprite : emptyHeartSprite;
            playerHearts[i].SetActive(true);
        }

        if (dealerMaxHP > 100 || (currentWave != null && currentMobIndex < currentWave.mobsInWave.Count && (currentWave.mobsInWave[currentMobIndex].mobName == "Kukla" || currentWave.mobsInWave[currentMobIndex].mobName == "Dummy")))
        {
            if (dealerHPText != null)
            {
                dealerHPText.gameObject.SetActive(true);
                dealerHPText.text = dealerHP.ToString();
            }
            foreach (GameObject heart in dealerHearts) heart.SetActive(false);
        }
        else
        {
            if (dealerHPText != null) dealerHPText.gameObject.SetActive(false);
            for (int i = 0; i < dealerHearts.Count; i++)
            {
                Image heartImage = dealerHearts[i].GetComponent<Image>();
                heartImage.sprite = (i < dealerHP) ? fullHeartSprite : emptyHeartSprite;
                dealerHearts[i].SetActive(true);
            }
        }

        UpdateArmorVisuals(playerArmor, playerArmorArea, playerArmorIcons);
        UpdateArmorVisuals(dealerArmor, dealerArmorArea, dealerArmorIcons);
    }

    public void UpdateCoinsUI()
    {
        if (playerCoinsText != null) playerCoinsText.text = playerCoins.ToString();
    }

    public void StartNewRound()
    {
        bool mobDiedFromEffects = false;

        if (bloodStacksOnEnemy > 0)
        {
            DealDamageToMob(1);
            StartCoroutine(MobDamageFlash());
            bloodStacksOnEnemy--;
            if (dealerHP <= 0) mobDiedFromEffects = true;
        }

        if (!mobDiedFromEffects && poisonStacksOnEnemy > 0)
        {
            dealerHP -= 1;
            if (mobAnimator != null) mobAnimator.SetTrigger("HitTrigger");
            StartCoroutine(MobDamageFlash());
            poisonStacksOnEnemy--;
            if (dealerHP <= 0) mobDiedFromEffects = true;
        }

        if (mobDiedFromEffects)
        {
            HandleEndCombatDurability();
            GiveRandomCoins(minCoins, maxCoins);
            currentMobIndex++;
            currentAttackBet = 0;
            currentDefenseBet = 0;

            foreach (CardDisplay card in PlayerCardsOnTable) Destroy(card.gameObject);
            PlayerCardsOnTable.Clear();
            foreach (CardDisplay card in DealerCardsOnTable) Destroy(card.gameObject);
            DealerCardsOnTable.Clear();

            if (currentWave != null && currentMobIndex >= currentWave.mobsInWave.Count)
            {
                ShowWaveSummary();
            }
            else
            {
                StartCoroutine(NextMobOrWinSequence());
            }
            return;
        }

        foreach (CardDisplay card in PlayerCardsOnTable) Destroy(card.gameObject);
        PlayerCardsOnTable.Clear();
        foreach (CardDisplay card in DealerCardsOnTable) Destroy(card.gameObject);
        DealerCardsOnTable.Clear();

        DealerDeck = new List<Card>(StartingDealerDeck);
        PlayerDeck = new List<Card>(StartingPlayerDeck);

        Shuffle(DealerDeck);
        Shuffle(PlayerDeck);

        currentAttackBet = 0;
        currentDefenseBet = 0;
        ChangeUIState(true, false, false);

        PlayerClassData currentClass = null;
        if (allPossibleClasses != null && allPossibleClasses.Count > 0 && SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            currentClass = allPossibleClasses.Find(c => c.classType == SaveManager.instance.currentSave.savedClassType);
        }
        if (currentClass == null && allPossibleClasses != null && allPossibleClasses.Count > 0) currentClass = allPossibleClasses[0];

        if (currentClass != null)
        {
            int baseMaxHP = currentClass.baseMaxHP;
            int baseMaxBet = currentClass.baseMaxBet;

            playerMaxHP = baseMaxHP + (eqManager != null ? eqManager.GetTotalMaxHPBonus() : 0);
            maxBetAmount = baseMaxBet + (eqManager != null ? eqManager.GetTotalMaxBetBonus() : 0);

            if (playerHP > playerMaxHP) playerHP = playerMaxHP;
            if (playerAvatar != null) playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
        }

        UpdateUI(false);
        CheckAndRestoreBaseTokens();
        GenerateTokens();
        RefreshBetButton();
    }

    public bool HasBlackjack(List<CardDisplay> hand)
    {
        return CalculateScore(hand) == 21 && hand.Count == 2;
    }

    public void ChangeUIState(bool showBets, bool showPlay, bool showRestart)
    {
        attackZone.gameObject.SetActive(showBets);
        defenseZone.gameObject.SetActive(showBets);
        if (bettingZoneTransform != null) bettingZoneTransform.gameObject.SetActive(showBets);

        hitButton.SetActive(showPlay);
        standButton.SetActive(showPlay);
        restartButton.SetActive(showRestart);
        betConfirmButton.SetActive(showBets);

        playerScoreText.gameObject.SetActive(!showBets);
        dealerScoreText.gameObject.SetActive(!showBets);
    }

    void GenerateHearts(int maxHP, Transform area, List<GameObject> heartList)
    {
        for (int i = 0; i < maxHP; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, area);
            heartList.Add(newHeart);
        }
    }

    public void LoadNextMob()
    {
        if (currentWave == null) return;

        if (currentMobIndex >= currentWave.mobsInWave.Count)
        {
            StartCoroutine(NextMobOrWinSequence());
            return;
        }

        MobData currentMob = currentWave.mobsInWave[currentMobIndex];

        if (mobInfoText != null)
        {
            int displayAttack = 1 + currentMob.attackBonus;
            mobInfoText.text = $"{currentMob.mobName} - Lvl {currentMob.level} (DMG: {displayAttack})";
        }

        foreach (GameObject heart in dealerHearts) Destroy(heart);
        dealerHearts.Clear();
        foreach (GameObject armorIcon in dealerArmorIcons) Destroy(armorIcon);
        dealerArmorIcons.Clear();

        dealerMaxHP = currentMob.maxHP;
        dealerHP = dealerMaxHP;
        dealerArmor = currentMob.startArmor;
        minCoins = currentMob.minCoins;
        maxCoins = currentMob.maxCoins;

        GenerateHearts(dealerMaxHP, dealerHeartArea, dealerHearts);

        DealerDeck = new List<Card>();

        if (currentMob.deckType == MobData.DeckGenerationType.Fixed || currentMob.deckType == MobData.DeckGenerationType.Hybrid)
        {
            DealerDeck.AddRange(currentMob.fixedDeck);
        }

        if (currentMob.deckType == MobData.DeckGenerationType.Random || currentMob.deckType == MobData.DeckGenerationType.Hybrid)
        {
            if (currentMob.randomCardPool.Count > 0)
            {
                for (int i = 0; i < currentMob.randomCardCount; i++)
                {
                    int randomIndex = Random.Range(0, currentMob.randomCardPool.Count);
                    DealerDeck.Add(currentMob.randomCardPool[randomIndex]);
                }
            }
        }

        StartingDealerDeck = new List<Card>(DealerDeck);
        Shuffle(DealerDeck);

        if (mobAnimator != null)
        {
            if (currentMob.animatorController != null)
            {
                mobAnimator.runtimeAnimatorController = currentMob.animatorController;
                mobAnimator.enabled = true;
            }
            else
            {
                mobAnimator.runtimeAnimatorController = null;
                mobAnimator.enabled = false;
            }
        }

        if (mobGraphicImage != null && currentMob.mobGraphic != null)
        {
            mobGraphicImage.sprite = currentMob.mobGraphic;
            mobGraphicImage.gameObject.SetActive(true);
        }

        UpdateUI(false);
    }

    public void RestartGame()
    {
        coinsGainedThisWave = 0;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        currentMobIndex = 0;

        PlayerClassData currentClass = null;
        if (allPossibleClasses != null && allPossibleClasses.Count > 0 && SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            currentClass = allPossibleClasses.Find(c => c.classType == SaveManager.instance.currentSave.savedClassType);
        }
        if (currentClass == null && allPossibleClasses != null && allPossibleClasses.Count > 0) currentClass = allPossibleClasses[0];

        tokensAttack = (currentClass != null ? currentClass.baseAttackTokens : 3) + (eqManager != null ? eqManager.GetTotalAttackBonus() : 0);
        tokensDefense = (currentClass != null ? currentClass.baseDefenseTokens : 0) + (eqManager != null ? eqManager.GetTotalDefenseBonus() : 0);

        playerArmor = 0;
        playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);

        foreach (GameObject armorIcon in playerArmorIcons) Destroy(armorIcon);
        playerArmorIcons.Clear();
        foreach (CardDisplay card in PlayerCardsOnTable) Destroy(card.gameObject);
        PlayerCardsOnTable.Clear();
        foreach (CardDisplay card in DealerCardsOnTable) Destroy(card.gameObject);
        DealerCardsOnTable.Clear();

        PlayerDeck = new List<Card>(StartingPlayerDeck);
        Shuffle(PlayerDeck);

        currentAttackBet = 0;
        currentDefenseBet = 0;
        ChangeUIState(true, false, false);
        LoadNextMob();
        GenerateTokens();
    }

    public void FinishGameAndClearSave()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.DeleteSaveData();
        }
        if (loadingPanel != null) loadingPanel.SetActive(true);
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveGame()
    {
        SaveData data;

        if (SaveManager.instance != null)
        {
            if (SaveManager.instance.currentSave == null) SaveManager.instance.LoadFromFile();

            if (SaveManager.instance.currentSave != null)
                data = SaveManager.instance.currentSave;
            else
                data = new SaveData();
        }
        else
        {
            data = new SaveData();
        }

        data.savedPlayerHP = playerHP;
        data.savedPlayerMaxHP = playerMaxHP;
        data.savedPlayerArmor = playerArmor;
        data.savedTokensAttack = tokensAttack;
        data.savedTokensDefense = tokensDefense;
        data.savedMobIndex = currentMobIndex;
        data.savedPlayerCoins = playerCoins;

        if (eqManager != null) eqManager.SaveEquipment(data);
        if (currentWave != null) data.savedWaveName = currentWave.name;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.currentSave = data;
            SaveManager.instance.SaveToFile(data);
        }
    }

    public void ApplyLoadedData()
    {
        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            SaveData data = SaveManager.instance.currentSave;
            if (eqManager != null) eqManager.LoadEquipment(data);

            PlayerClassData currentClass = null;
            if (allPossibleClasses != null && allPossibleClasses.Count > 0)
            {
                currentClass = allPossibleClasses.Find(c => c.classType == data.savedClassType);
                if (currentClass == null) currentClass = allPossibleClasses[0];
            }

            int dynamicMaxHP = (currentClass != null ? currentClass.baseMaxHP : 15) + (eqManager != null ? eqManager.GetTotalMaxHPBonus() : 0);
            maxBetAmount = (currentClass != null ? currentClass.baseMaxBet : 2) + (eqManager != null ? eqManager.GetTotalMaxBetBonus() : 0);

            if (data.savedPlayerMaxHP == 999)
            {
                playerMaxHP = 999;
                playerHP = 999;
                playerArmor = data.savedPlayerArmor;
                tokensAttack = data.savedTokensAttack;
                tokensDefense = data.savedTokensDefense;
                currentMobIndex = data.savedMobIndex;
                playerCoins = data.savedPlayerCoins;
            }
            else if (data.savedPlayerMaxHP > 0)
            {
                int hpDiff = dynamicMaxHP - data.savedPlayerMaxHP;
                playerHP = data.savedPlayerHP + (hpDiff > 0 ? hpDiff : 0);

                playerMaxHP = dynamicMaxHP;
                playerArmor = data.savedPlayerArmor;
                tokensAttack = data.savedTokensAttack;
                tokensDefense = data.savedTokensDefense;
                currentMobIndex = data.savedMobIndex;
                playerCoins = data.savedPlayerCoins;
            }
            else
            {
                if (currentClass != null && eqManager != null) eqManager.InitializeStartingEquipment(currentClass);

                playerMaxHP = dynamicMaxHP;
                playerHP = playerMaxHP;
                playerArmor = 0;
                currentMobIndex = 0;
                playerCoins = 0;

                tokensAttack = (currentClass != null ? currentClass.baseAttackTokens : 3) + (eqManager != null ? eqManager.GetTotalAttackBonus() : 0);
                tokensDefense = (currentClass != null ? currentClass.baseDefenseTokens : 0) + (eqManager != null ? eqManager.GetTotalDefenseBonus() : 0);
            }

            if (!string.IsNullOrEmpty(data.targetWaveName) && allPossibleWaves != null)
            {
                WaveData targetWave = allPossibleWaves.Find(w => w.name == data.targetWaveName);
                if (targetWave != null)
                {
                    currentWave = targetWave;
                }
            }

            if (!string.IsNullOrEmpty(data.activeTavernBuff))
            {
                MealData activeMeal = allPossibleMeals.Find(m => m.name == data.activeTavernBuff);
                if (activeMeal != null)
                {
                    playerArmor += activeMeal.restoreArmor;
                    playerHP += activeMeal.restoreHP;
                    if (playerHP > playerMaxHP) playerHP = playerMaxHP;

                    tokensAttack += activeMeal.bonusAttackTokens;
                    tokensDefense += activeMeal.bonusDefenseTokens;

                    Debug.Log($"[Tavern Buff] Consumed {activeMeal.mealName}. Restored HP: {activeMeal.restoreHP}, Armor: {activeMeal.restoreArmor}. ATK Tokens Bonus: +{activeMeal.bonusAttackTokens}");
                }

                data.activeTavernBuff = "";
                SaveManager.instance.SaveToFile(data);
            }
            LoadClassVisuals(data.savedClassType);
            UpdateCoinsUI();
            RefreshUpgradesLogUI();
        }
    }

    public void RefreshUpgradesLogUI()
    {
        if (upgradesLogContainer == null || upgradeLogItemPrefab == null) return;

        foreach (Transform child in upgradesLogContainer) Destroy(child.gameObject);

        if (SaveManager.instance == null || SaveManager.instance.currentSave == null) return;

        Dictionary<string, Dictionary<string, int>> buffGroups = new Dictionary<string, Dictionary<string, int>>();

        foreach (SavedCardUpgrade savedUpgrade in SaveManager.instance.currentSave.upgradedCards)
        {
            foreach (string upgName in savedUpgrade.upgradeNames)
            {
                if (!buffGroups.ContainsKey(upgName))
                    buffGroups[upgName] = new Dictionary<string, int>();

                if (!buffGroups[upgName].ContainsKey(savedUpgrade.cardName))
                    buffGroups[upgName][savedUpgrade.cardName] = 0;

                buffGroups[upgName][savedUpgrade.cardName]++;
            }
        }

        foreach (var buffEntry in buffGroups)
        {
            string buffName = buffEntry.Key;
            var cardsWithThisBuff = buffEntry.Value;

            CardUpgradeData upgData = allAvailableUpgrades.Find(u => u.name == buffName);
            if (upgData == null) continue;

            GameObject logItem = Instantiate(upgradeLogItemPrefab, upgradesLogContainer);
            UpgradeLogItemUI uiScript = logItem.GetComponent<UpgradeLogItemUI>();

            if (uiScript != null)
            {
                uiScript.Setup(upgData.icon, upgradeTooltip);

                foreach (var cardEntry in cardsWithThisBuff)
                {
                    string cardName = cardEntry.Key;
                    int stacks = cardEntry.Value;

                    Sprite cardSprite = null;
                    if (allReferenceCards != null)
                    {
                        Card refCard = allReferenceCards.Find(c => GetCleanCardName(c.name) == cardName);
                        if (refCard != null) cardSprite = refCard.graphic;
                    }

                    uiScript.AddCardData(cardSprite, stacks);
                }
            }
        }
    }

    private bool HasAnyDamageSource()
    {
        if (tokensAttack <= 0 && tokensDefense <= 0) return false;

        if (tokensAttack > 0 || tokensDefense > 0) return true;

        if (poisonStacksOnEnemy > 0 || bloodStacksOnEnemy > 0) return true;
        if (eqManager != null && eqManager.equippedWeapon != null && eqManager.equippedWeapon.bonusBaseDamage > 0) return true;

        if (SaveManager.instance != null && SaveManager.instance.currentSave != null && allAvailableUpgrades != null)
        {
            foreach (var savedCard in SaveManager.instance.currentSave.upgradedCards)
            {
                foreach (string upgName in savedCard.upgradeNames)
                {
                    CardUpgradeData data = allAvailableUpgrades.Find(u => u.name == upgName);
                    if (data != null)
                    {
                        if (data.upgradeType == CardUpgradeData.UpgradeType.Fire ||
                            data.upgradeType == CardUpgradeData.UpgradeType.Poison ||
                            data.upgradeType == CardUpgradeData.UpgradeType.Blood ||
                            data.upgradeType == CardUpgradeData.UpgradeType.Attack)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void LoadClassVisuals(PlayerClassType type)
    {
        if (allPossibleClasses == null || allPossibleClasses.Count == 0) return;

        PlayerClassData selectedData = allPossibleClasses.Find(c => c.classType == type);
        if (selectedData == null) selectedData = allPossibleClasses[0];
        if (playerAvatar == null) return;

        playerAvatar.currentClass = selectedData;
        playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
    }

    public void PlayMobAttackSound()
    {
        if (audioSource != null && currentWave != null)
        {
            MobData currentMob = currentWave.mobsInWave[currentMobIndex];
            if (currentMob.attackSound != null) audioSource.PlayOneShot(currentMob.attackSound);
        }
    }

    public void PlayMobDamageSound()
    {
        if (audioSource != null && currentWave != null)
        {
            MobData currentMob = currentWave.mobsInWave[currentMobIndex];
            if (currentMob.damageSound != null) audioSource.PlayOneShot(currentMob.damageSound);
        }
    }

    public void GenerateTokens()
    {
        foreach (Transform child in attackZone) Destroy(child.gameObject);
        foreach (Transform child in defenseZone) Destroy(child.gameObject);

        for (int i = 0; i < tokensAttack; i++)
        {
            DraggableToken newToken = Instantiate(tokenPrefab, attackZone);
            newToken.Setup(DeckManager.BetType.Attack);
            newToken.transform.SetAsLastSibling();
        }
        for (int i = 0; i < tokensDefense; i++)
        {
            DraggableToken newToken = Instantiate(tokenPrefab, defenseZone);
            newToken.Setup(DeckManager.BetType.Defense);
            newToken.transform.SetAsLastSibling();
        }
        RefreshBetButton();
    }

    public void ConfirmBet()
    {
        playedDrawEffectsThisAction.Clear();
        if (bettingZoneTransform != null)
        {
            foreach (Transform child in bettingZoneTransform) Destroy(child.gameObject);
        }
        StartCoroutine(DealInitialCardsSequence());
    }

    public void RefreshBetButton()
    {
        if (betConfirmButton != null)
        {
            bool canBet = (currentAttackBet + currentDefenseBet) > 0;
            betConfirmButton.GetComponent<Button>().interactable = canBet;
        }
    }

    private IEnumerator MobDamageFlash()
    {
        if (mobGraphicImage != null)
        {
            Color originalColor = mobGraphicImage.color;
            mobGraphicImage.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            if (mobGraphicImage != null) mobGraphicImage.color = originalColor;
        }
    }

    public void GiveRandomCoins(int minRange, int maxRange)
    {
        int randomCoins = Random.Range(minRange, maxRange + 1);

        float multiplier = 1f;
        if (eqManager != null) multiplier = eqManager.GetTotalGoldMultiplier();

        int finalCoins = Mathf.RoundToInt(randomCoins * multiplier);

        playerCoins += finalCoins;
        coinsGainedThisWave += finalCoins;
        UpdateCoinsUI();
    }

    private IEnumerator AnimateCardSpawn(Transform cardTransform, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = cardTransform.localScale;
        cardTransform.localScale = Vector3.zero;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            if (cardTransform != null)
            {
                cardTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                cardTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (cardTransform != null)
        {
            cardTransform.localPosition = endPos;
            cardTransform.localScale = originalScale;
        }
    }

    public void PlayDealCardSound()
    {
        if (audioSource != null && dealCardSound != null)
        {
            StartCoroutine(PlaySoundDelayed(dealCardSound, dealCardSoundDelay));
        }
    }

    private IEnumerator PlaySoundDelayed(AudioClip clip, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    public void DealDamageToMob(int damageAmount)
    {
        if (damageAmount <= 0) return;

        if (mobAnimator != null) mobAnimator.SetTrigger("HitTrigger");
        if (dealerArmor >= damageAmount)
        {
            dealerArmor -= damageAmount;
        }
        else
        {
            int remainingDamage = damageAmount - dealerArmor;
            dealerArmor = 0;
            dealerHP -= remainingDamage;
        }
    }

    private void UpdateArmorVisuals(int currentArmor, Transform area, List<GameObject> iconList)
    {
        if (armorIconPrefab == null || area == null) return;
        while (iconList.Count < currentArmor)
        {
            GameObject newIcon = Instantiate(armorIconPrefab, area);
            iconList.Add(newIcon);
        }
        for (int i = 0; i < iconList.Count; i++)
        {
            iconList[i].SetActive(i < currentArmor);
        }
    }

    private void LoadEnvironmentTheme()
    {
        if (SaveManager.instance == null || SaveManager.instance.currentSave == null) return;

        string savedTheme = SaveManager.instance.currentSave.currentMapTheme;
        MapThemeData currentTheme = availableThemes.Find(t => t.themeID == savedTheme);

        if (currentTheme != null)
        {
            if (backgroundRenderer != null && currentTheme.staticBackground != null)
                backgroundRenderer.sprite = currentTheme.staticBackground;

            if (backgroundAnimator != null && currentTheme.themeAnimator != null)
                backgroundAnimator.runtimeAnimatorController = currentTheme.themeAnimator;
        }
    }

    public void HideCombatUI()
    {
        if (combatUIContainer != null) combatUIContainer.SetActive(false);

        if (tableObject != null) tableObject.SetActive(false);
        if (uiPlayerDeck != null) uiPlayerDeck.SetActive(false);
        if (uiDealerDeck != null) uiDealerDeck.SetActive(false);

        if (mobGraphicImage != null) mobGraphicImage.gameObject.SetActive(false);
        if (avatarObject != null) avatarObject.SetActive(false);
        if (playerHealthBar != null) playerHealthBar.SetActive(false);
        if (dealerHealthBar != null) dealerHealthBar.SetActive(false);

        if (upgradesLogPanel != null) upgradesLogPanel.SetActive(false);
    }

    public void ShowCombatUI()
    {
        if (combatUIContainer != null) combatUIContainer.SetActive(true);

        if (tableObject != null) tableObject.SetActive(true);
        if (uiPlayerDeck != null) uiPlayerDeck.SetActive(true);
        if (uiDealerDeck != null) uiDealerDeck.SetActive(true);

        if (mobGraphicImage != null) mobGraphicImage.gameObject.SetActive(true);
        if (avatarObject != null) avatarObject.SetActive(true);
        if (playerHealthBar != null) playerHealthBar.SetActive(true);
        if (dealerHealthBar != null) dealerHealthBar.SetActive(true);

        if (upgradesLogPanel != null) upgradesLogPanel.SetActive(true);
    }

    public void ShowWaveSummary()
    {
        ChangeUIState(false, false, false);
        HideCombatUI();

        if (waveSummaryPanel != null) waveSummaryPanel.SetActive(true);

        if (summaryMobsContainer != null && summaryMobPrefab != null)
        {
            foreach (Transform child in summaryMobsContainer) Destroy(child.gameObject);

            foreach (MobData mob in currentWave.mobsInWave)
            {
                GameObject mobObj = Instantiate(summaryMobPrefab, summaryMobsContainer);
                TextMeshProUGUI txt = mobObj.GetComponentInChildren<TextMeshProUGUI>();

                if (txt != null)
                {
                    if (!string.IsNullOrEmpty(mob.mobName))
                    {
                        txt.text = $"{mob.mobName} - Lvl {mob.level}";
                    }
                    else
                    {
                        txt.text = $"<color=red>MISSING NAME</color> - Lvl {mob.level}";
                    }
                }
            }
        }

        if (summaryLootContainer != null && summaryLootPrefab != null)
        {
            foreach (Transform child in summaryLootContainer) Destroy(child.gameObject);

            if (currentWave.possibleLootPool != null && currentWave.possibleLootPool.Count > 0)
            {
                int lootAmount = Random.Range(currentWave.minLootItems, currentWave.maxLootItems + 1);
                for (int i = 0; i < lootAmount; i++)
                {
                    ItemData rolledItem = currentWave.possibleLootPool[Random.Range(0, currentWave.possibleLootPool.Count)];

                    if (eqManager != null) eqManager.AddToBackpack(rolledItem);

                    GameObject lootObj = Instantiate(summaryLootPrefab, summaryLootContainer);

                    Image[] images = lootObj.GetComponentsInChildren<Image>();

                    Image targetImg = null;
                    foreach (var img in images)
                    {
                        if (img.gameObject.name == "ItemIcon" || img.gameObject.name == "Icon")
                        {
                            targetImg = img;
                            break;
                        }
                    }
                    if (targetImg == null && images.Length > 1) targetImg = images[1];
                    else if (targetImg == null && images.Length > 0) targetImg = images[0];

                    if (targetImg != null) targetImg.sprite = rolledItem.itemIcon;

                    TextMeshProUGUI[] texts = lootObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length > 0 && texts[0] != null)
                    {
                        if (!string.IsNullOrEmpty(rolledItem.itemName))
                        {
                            texts[0].text = rolledItem.itemName;
                        }
                        else
                        {
                            texts[0].text = "<color=red>MISSING NAME</color>";
                        }
                    }
                    if (texts.Length > 1 && texts[1] != null)
                    {
                        texts[1].text = rolledItem.description;
                    }
                }
            }
        }

        if (summaryTotalCoinsText != null)
        {
            summaryTotalCoinsText.text = "+" + coinsGainedThisWave.ToString();
        }
    }

    public void CloseWaveSummary()
    {
        if (waveSummaryPanel != null) waveSummaryPanel.SetActive(false);
        coinsGainedThisWave = 0;
        StartCoroutine(NextMobOrWinSequence());
    }

    private IEnumerator NextMobOrWinSequence()
    {
        CheckAndRestoreBaseTokens();
        SaveGame();
        HideCombatUI();

        if (backgroundAnimator != null) backgroundAnimator.SetTrigger("WinTransition");

        float waitTime = 2f;

        if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
        {
            MapThemeData theme = availableThemes.Find(t => t.themeID == SaveManager.instance.currentSave.currentMapTheme);
            if (theme != null) waitTime = theme.transitionDuration;
        }

        yield return new WaitForSeconds(waitTime);

        if (currentWave == null || currentMobIndex >= currentWave.mobsInWave.Count)
        {
            if (SaveManager.instance != null && SaveManager.instance.currentSave != null)
            {
                var data = SaveManager.instance.currentSave;
                var node = data.mapNodes.Find(n => n.nodeID == data.currentNodeID);
                if (node != null) node.isCleared = true;
            }

            if (backgroundAnimator != null) backgroundAnimator.Play("Idle");

            if (currentWave != null && currentWave.isFinalBossWave)
            {
                if (winPanel != null) winPanel.SetActive(true);
            }
            else
            {
                SaveGame();
                if (loadingPanel != null) loadingPanel.SetActive(true);
                SceneManager.LoadScene("WorldMap");
            }
        }
        else
        {
            if (backgroundAnimator != null) backgroundAnimator.Play("Idle");

            ShowCombatUI();
            ChangeUIState(true, false, false);
            CheckAndRestoreBaseTokens();
            GenerateTokens();
            LoadNextMob();
        }
    }

    private string GetCleanCardName(string rawName)
    {
        string clean = rawName.Replace("Card_", "");
        string[] stringsToRemove = { "pik", "karo", "kier", "trefl", "Pik", "Karo", "Kier", "Trefl", "_", " " };

        foreach (string s in stringsToRemove)
        {
            clean = clean.Replace(s, "");
        }
        return clean.Trim();
    }

    private void ApplyOnDrawEffects(Card drawnCard)
    {
        if (SaveManager.instance == null || SaveManager.instance.currentSave == null) return;
        if (allAvailableUpgrades == null) return;

        string cleanCardName = GetCleanCardName(drawnCard.name);
        var savedCard = SaveManager.instance.currentSave.upgradedCards.Find(u => u.cardName == cleanCardName);
        if (savedCard == null) return;

        foreach (string upgName in savedCard.upgradeNames)
        {
            CardUpgradeData data = allAvailableUpgrades.Find(u => u.name == upgName);

            if (data != null && data.phase == CardUpgradeData.ActivationPhase.OnDraw)
            {
                if (!playedDrawEffectsThisAction.Contains(data.name))
                {
                    Transform fxTarget = (data.upgradeType == CardUpgradeData.UpgradeType.Heal || data.upgradeType == CardUpgradeData.UpgradeType.Defense) ? (playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform)) : (enemyEffectSpawnPoint != null ? enemyEffectSpawnPoint : (mobGraphicImage != null ? mobGraphicImage.transform : transform));
                    PlayEffect(data.effectPrefab, fxTarget, data.effectDuration);

                    if (data.effectSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(data.effectSound);
                    }

                    playedDrawEffectsThisAction.Add(data.name);
                }

                switch (data.upgradeType)
                {
                    case CardUpgradeData.UpgradeType.Fire:
                        DealDamageToMob(data.effectValue);
                        StartCoroutine(MobDamageFlash());
                        PlayMobDamageSound();
                        break;
                    case CardUpgradeData.UpgradeType.Defense:
                        playerArmor += data.effectValue;
                        break;
                    case CardUpgradeData.UpgradeType.Heal:
                        playerHP += data.effectValue;
                        if (playerHP > playerMaxHP) playerHP = playerMaxHP;
                        if (playerAvatar != null) playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
                        UpdateUI(false);
                        break;
                }
            }
        }
        UpdateUI(false);
    }

    private void CalculateWinBonuses(out int bonusDamage, out int healAmount, out float lifestealPct, out int critMult, out int bonusArmor)
    {
        bonusDamage = 0;
        healAmount = 0;
        lifestealPct = 0f;
        critMult = 1;
        bonusArmor = 0;

        if (SaveManager.instance == null || SaveManager.instance.currentSave == null) return;
        if (allAvailableUpgrades == null) return;

        HashSet<string> playedWinFx = new HashSet<string>();

        foreach (CardDisplay cardDisplay in PlayerCardsOnTable)
        {
            string cleanName = GetCleanCardName(cardDisplay.cardValue.name);
            var savedCard = SaveManager.instance.currentSave.upgradedCards.Find(u => u.cardName == cleanName);
            if (savedCard == null) continue;

            foreach (string upgName in savedCard.upgradeNames)
            {
                CardUpgradeData data = allAvailableUpgrades.Find(u => u.name == upgName);
                if (data != null && data.phase == CardUpgradeData.ActivationPhase.OnWin)
                {
                    if (!playedWinFx.Contains(data.name))
                    {
                        Transform fxTarget = (data.upgradeType == CardUpgradeData.UpgradeType.Attack || data.upgradeType == CardUpgradeData.UpgradeType.Lifesteal || data.upgradeType == CardUpgradeData.UpgradeType.Crit) ? (enemyEffectSpawnPoint != null ? enemyEffectSpawnPoint : (mobGraphicImage != null ? mobGraphicImage.transform : transform)) : (playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform));
                        PlayEffect(data.effectPrefab, fxTarget, data.effectDuration);

                        if (data.effectSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(data.effectSound);
                        }

                        playedWinFx.Add(data.name);
                    }

                    switch (data.upgradeType)
                    {
                        case CardUpgradeData.UpgradeType.Attack:
                            bonusDamage += data.effectValue;
                            break;
                        case CardUpgradeData.UpgradeType.Heal:
                            healAmount += data.effectValue;
                            break;
                        case CardUpgradeData.UpgradeType.Lifesteal:
                            lifestealPct += (data.effectValue * 0.1f);
                            break;
                        case CardUpgradeData.UpgradeType.Crit:
                            critMult += data.effectValue;
                            break;
                        case CardUpgradeData.UpgradeType.Defense:
                            bonusArmor += data.effectValue;
                            break;
                    }
                }
            }
        }
    }

    public void UseQuickslotPotion()
    {
        if (eqManager != null && eqManager.equippedPotion != null)
        {
            ItemData potion = eqManager.equippedPotion;

            if (potion.consumeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(potion.consumeSound);
            }

            if (potion.restoreHP > 0)
            {
                playerHP += potion.restoreHP;
                if (playerHP > playerMaxHP) playerHP = playerMaxHP;
                Transform spawnTarget = playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform);
                PlayEffect(healEffectPrefab, spawnTarget);
            }
            if (potion.restoreArmor > 0)
            {
                playerArmor += potion.restoreArmor;
                Transform spawnTarget = playerEffectSpawnPoint != null ? playerEffectSpawnPoint : (avatarObject != null ? avatarObject.transform : transform);
                PlayEffect(armorEffectPrefab, spawnTarget);
            }

            eqManager.ConsumePotion();

            UpdateUI(false);
            playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
            InventoryUI invUI = FindAnyObjectByType<InventoryUI>();
            if (invUI != null)
            {
                invUI.RefreshUI();
            }
        }
    }

    private void DelayedEQRefresh()
    {
        InventoryUI invUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (invUI != null) invUI.RefreshUI();
    }

    private bool HandleDeathDefiance()
    {
        if (eqManager != null && eqManager.equippedTrinket != null && eqManager.equippedTrinket.hasDeathDefiance)
        {
            if (Random.value <= 0.5f)
            {
                float healPct = Random.Range(0.1f, 0.5f);
                playerHP = Mathf.Max(1, Mathf.FloorToInt(playerMaxHP * healPct));

                StartCoroutine(DeathDefianceAnimation(eqManager.equippedTrinket.itemIcon));

                eqManager.UnequipItem(EquipmentSlot.Trinket, false);

                playerAvatar.UpdateAvatarState(playerHP, playerMaxHP);
                DelayedEQRefresh();

                Debug.Log("DEATH DEFIED! The trinket shattered and saved your life.");
                return true;
            }
        }
        return false;
    }

    private IEnumerator DeathDefianceAnimation(Sprite icon)
    {
        GameObject defyObj = new GameObject("DeathDefianceIcon");
        defyObj.transform.SetParent(combatUIContainer != null ? combatUIContainer.transform : transform, false);

        Image img = defyObj.AddComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;

        RectTransform rt = defyObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(150, 150);

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 2.5f, t);
            img.color = new Color(1, 1, 1, 1 - t);
            yield return null;
        }
        Destroy(defyObj);
    }
}