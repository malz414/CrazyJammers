using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Code.Utility.Events;
using UnityEngine.EventSystems;
using DamageNumbersPro;
using System.Linq;
using static System.Math;
#if USE_CRAZYGAMES
using CrazyGames;
#endif
using UnityEngine.SceneManagement;
using EasyTransition;

public class TurnManager : MonoBehaviour
{
    public TransitionSettings sequenceTransition;

    public enum GameMode
    {
        Standard,
        Endless
    }

    // --- NEW ENDLESS WAVE FRAMEWORK DATA STRUCTURES ---
    [System.Serializable]
    public class EnemyPool
    {
        [Tooltip("Unique name for this category (e.g., Easy, Medium, Hard, Maddening).")]
        public string poolName;
        [Tooltip("Enemy prefabs belonging to this difficulty category.")]
        public List<GameObject> enemyPrefabs = new List<GameObject>();
    }

    [System.Serializable]
    public class WavePoolRule
    {
        [Tooltip("Must match a Pool Name defined in Enemy Pools.")]
        public string poolName;
        [Tooltip("How many enemies must die in THIS wave before this pool becomes active (0 = active from start of wave).")]
        public int startAfterKillsInWave = 0;
        [Tooltip("Optional: Stop drawing from this pool after X kills in this wave (-1 = stays active for the rest of the wave).")]
        public int stopAfterKillsInWave = -1;
    }

    [System.Serializable]
    public class WaveConfig
    {
        [Tooltip("Optional label for organization in the Inspector (e.g., 'Wave 1 - Easy Only').")]
        public string waveLabel;
        [Tooltip("Pools that can appear during this wave and when they unlock.")]
        public List<WavePoolRule> poolRules = new List<WavePoolRule>();
    }

    public GameMode currentGameMode = GameMode.Standard;
    public static TurnManager Instance { get; private set; }

    [Header("Legacy Enemy Lists (Auto-migrated if custom pools are empty)")]
    [SerializeField] private List<GameObject> easyEnemies;
    [SerializeField] private List<GameObject> mediumEnemies;
    [SerializeField] private List<GameObject> hardEnemies;

    [Header("Endless Mode - Custom Pools & Waves")]
    [Tooltip("Define as many enemy categories/pools as you want (Easy, Medium, Hard, Maddening, etc.).")]
    [SerializeField] private List<EnemyPool> enemyPools = new List<EnemyPool>();
    [Tooltip("Configure specific waves in order (Element 0 = Wave 1, Element 1 = Wave 2, etc.).")]
    [SerializeField] private List<WaveConfig> waveConfigs = new List<WaveConfig>();

    [Header("Endless Mode - Wave & Automation Settings")]
    [Tooltip("Total enemies per wave (Default: 12). Only 4 are fightable at once.")]
    public int enemiesPerWave = 12;
    [Tooltip("Break time in seconds between waves (Default: 1.0s).")]
    public float waveBreakDuration = 1.0f;
    [Tooltip("Wave number where waves become completely automated to the highest difficulty (e.g., 100).")]
    public int automateToHighestDifficultyWave = 100;
    [Tooltip("Name of the highest difficulty pool for automation (e.g., 'Maddening'). If blank, uses the last pool in Enemy Pools.")]
    public string highestDifficultyPoolName = "Maddening";

    [Header("Endless Mode - Run-In Animation Settings")]
    [Tooltip("Offset from the spawn point where newly spawned enemies start running from.")]
    public Vector3 spawnRunInOffset = new Vector3(-4f, 0f, 0f);
    [Tooltip("How long enemies take to run in at the start of a wave.")]
    public float waveSpawnRunDuration = 0.5f;
    [Tooltip("How quickly a replacement enemy runs up when an enemy dies mid-wave.")]
    public float replacementRunDuration = 0.2f;
    [Tooltip("Animator bool or trigger parameter name for the running animation.")]
    public string runAnimParameter = "Run";
    [Tooltip("Duration of the dying enemy fade-out animation.")]
    public float enemyFadeOutDuration = 0.5f;

    [Header("Endless Mode - Scoring & UI")]
    public int scorePerDefeat = 100;
    public int scorePerWaveBonus = 1000;
    public int currentScore = 0;
    public int currentGald => currentScore / 4;
    public int currentWave = 1;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI galdText;

    // Runtime Wave Tracking
    private int enemiesSpawnedInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0;
    private int totalEnemiesKilled = 0;
    private int pendingReplacementsCount = 0;
    private bool isWaveTransitioning = false;

    private List<Enemy> enemies;
    public Hero hero;
    private string attackNamesText;
    public int currentTurn = 0;

    [Header("Prefabs")]
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("VFX")]
    [SerializeField] private GameObject SteadyAttack;
    [SerializeField] private GameObject SteadyHit;
    [SerializeField] private GameObject lungeAttack;
    [SerializeField] private GameObject lungeHit;
    [SerializeField] private GameObject tripleHit;
    [SerializeField] private GameObject tripleAttack;
    [SerializeField] private GameObject arrowAttack;
    [SerializeField] private GameObject arrowAttack1;
    [SerializeField] private GameObject arrowHit;
    [SerializeField] private GameObject slashAttack;
    [SerializeField] private GameObject slashHit;
    [SerializeField] private GameObject slashCrater;
    [SerializeField] private GameObject iceAttack;
    [SerializeField] private GameObject iceHit;
    [SerializeField] private GameObject fireAttack;
    [SerializeField] private GameObject fireHit;
    [SerializeField] private GameObject barrier1;
    [SerializeField] private GameObject barrier1Big;
    [SerializeField] private GameObject barrier2;
    [SerializeField] private GameObject barrier3;
    [SerializeField] private GameObject heal;
    [SerializeField] private GameObject healfield;
    [SerializeField] private GameObject zapAttack;
    [SerializeField] private GameObject zapHit;

    [Header("Hybrid Lunge VFX")]
    [SerializeField] private GameObject fireLungeAttack;
    [SerializeField] private GameObject fireLungeHit;
    [SerializeField] private GameObject iceLungeAttack;
    [SerializeField] private GameObject iceLungeHit;
    [SerializeField] private GameObject arrowLungeAttack;
    [SerializeField] private GameObject arrowLungeHit;
    [SerializeField] private GameObject paralysisAttack;
    [SerializeField] private GameObject paralysisHit;

    [Header("Hybrid Elemental VFX")]
    [SerializeField] private GameObject iceBurnAttack;
    [SerializeField] private GameObject iceBurnHit;
    [SerializeField] private GameObject iceParaAttack; // Ice Bullet x Paralysis
    [SerializeField] private GameObject iceParaHit;
    [SerializeField] private GameObject arrowBurnAttack; // Arrow x Burn
    [SerializeField] private GameObject arrowBurnHit;
    
    // Expanded Names
    [SerializeField] private GameObject paralysisVFX; // Was "para"
    [SerializeField] private GameObject burn;
    [SerializeField] private GameObject bideVFX;      // Was "bideani"
    [SerializeField] private GameObject potionVFX;    // Was "potionAni"
    [SerializeField] private GameObject panaceaVFX;   // Was "panaceaAni"

    [Header("Settings & Objects")]
    public CombinedAttackSO combinedAttack;
    [SerializeField] private Transform[] enemySpawns;
    [SerializeField] private Transform bossSpawn;
    [SerializeField] GameObject MainUIParent;
    [SerializeField] GameObject attackOptionsParent;
    [SerializeField] GameObject attackOptionsMenu;
    [SerializeField] private TMP_Dropdown attackDropdown;
    [SerializeField] private TMP_Dropdown attackDropdown2;
    [SerializeField] GameObject potionOptions;
    [SerializeField] GameObject targetingHUDParent;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject thankScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] CharacterHUD[] enemyHUDs;
    [SerializeField] CharacterHUD bossHUD;
    [SerializeField] private DamageNumber popupPrefabNeutral;
    [SerializeField] private DamageNumber popupPrefab;
    [SerializeField] private DamageNumber popupPrefabfire;
    [SerializeField] private DamageNumber popupPrefabgreen;
    [SerializeField] GameObject[] listOfObjectToDeactivateAtStartOfBattle;

    // Logic Variables
    public List<Enemy> aliveEnemies = new List<Enemy>();
    private List<Enemy> deadEnemies = new List<Enemy>();
    public List<AttackSO> enemyAttacksByIndex = new List<AttackSO>();
    public List<AttackSO> enemyAttacksByIndexPerm = new List<AttackSO> { null, null, null, null };
    public List<AttackSO> previousTurnMoves;
    public List<AttackSO> allEncounteredMoves = new List<AttackSO>();
    private List<Enemy> selectedEnemies = new List<Enemy>();
    private List<Enemy> upcomingEnemies = new List<Enemy>();
    
    private int selectedEnemyNum = -1;
    private bool selectingEnemies = false;

    // Dropdown Logic
    public bool drop1Opened;
    public bool drop2Opened;

    [SerializeField] private AttackSO[] attacksReset;
    public Button bideButton;
    public Button potionButton;
    public Button panaceaButton;
    public GameObject itemOptions;

    private Coroutine bossAttackCoroutine;
    private List<AttackSO> enemyAttacksUsed;
    private GameplayBlurbEvent blurbEvent;

    private bool hasLunged = false;
    private bool hasIced = false;
    private bool heroAniPlayed = false;

    public int bideAttribute = 0;
    public float heroBideLevelBase = 1.0f;
    private float randomChance;
    private bool attackExtra = false;

    public bool TargetingMode => targetingMode;
    private bool targetingMode = false;
    
    public float critMultiplier = 1.2f;
    public float heroCritRate = .05f;

    private bool bideBuff = false;

    // Revive Logic
    private bool revived = false;
    public GameObject reviveButton;

    public int enemiesDead = 0;

    // UI Elements with Expanded Names
    [SerializeField] private TextMeshProUGUI potionAmountText;  // Was "PotAmount"
    [SerializeField] private TextMeshProUGUI panaceaAmountText; // Was "PanAmount"
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI descriptionTextPotion;
    [SerializeField] private TextMeshProUGUI usedMove;
    [SerializeField] private TextMeshProUGUI usedMove1;
    [SerializeField] private GameObject usedMoveGO;
    public TextMeshProUGUI popupText; // Was "Popop"

    private CharacterStatusUpdateEvent statusUpdateEvent;
    
    public AttackSO selectedAttack1;
    public AttackSO selectedAttack2;
    private List<AttackSO> currentlyDisplayedAttacks = new List<AttackSO>();

    public bool isFirstMatch = false;

    [Tooltip("Time in seconds to wait before showing the win/lose screen.")]
    public float endGameDelay = 2.0f; 
    private bool isBattleActive = false;
    private bool isEndingGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        EnsureDefaultPoolsInitialized();
    }

    private void Start()
    {
        statusUpdateEvent = new CharacterStatusUpdateEvent();
        
        if (PotionData.Instance != null)
        {
            PotionData.Instance.SaveCheckpoint();
        }

        SetupAttackDropdowns(); 
        if (currentGameMode == GameMode.Standard)
        {
            PlayerPrefs.SetString("SavedLevel", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Ensures legacy easy/medium/hard lists are migrated into the new custom EnemyPool system if none are configured yet.
    /// </summary>
    private void EnsureDefaultPoolsInitialized()
    {
        if (enemyPools == null) enemyPools = new List<EnemyPool>();

        if (enemyPools.Count == 0)
        {
            if (easyEnemies != null && easyEnemies.Count > 0)
                enemyPools.Add(new EnemyPool { poolName = "Easy", enemyPrefabs = new List<GameObject>(easyEnemies) });
            if (mediumEnemies != null && mediumEnemies.Count > 0)
                enemyPools.Add(new EnemyPool { poolName = "Medium", enemyPrefabs = new List<GameObject>(mediumEnemies) });
            if (hardEnemies != null && hardEnemies.Count > 0)
                enemyPools.Add(new EnemyPool { poolName = "Hard", enemyPrefabs = new List<GameObject>(hardEnemies) });
        }

        if (waveConfigs == null) waveConfigs = new List<WaveConfig>();
        if (waveConfigs.Count == 0 && enemyPools.Count > 0)
        {
            // Default Wave 1 fallback if waveConfigs is left empty in the Inspector
            WaveConfig defaultWave1 = new WaveConfig
            {
                waveLabel = "Wave 1 (Default)",
                poolRules = new List<WavePoolRule>
                {
                    new WavePoolRule { poolName = enemyPools[0].poolName, startAfterKillsInWave = 0, stopAfterKillsInWave = -1 }
                }
            };
            waveConfigs.Add(defaultWave1);
        }
    }

    // --- NEW ENDLESS POOL & WAVE SELECTION LOGIC ---
    private List<GameObject> GetEligiblePrefabsForCurrentState()
    {
        EnsureDefaultPoolsInitialized();
        List<GameObject> combinedPool = new List<GameObject>();

        // 1. Check if we've reached the automation wave threshold (e.g. Wave 100+)
        if (currentWave >= automateToHighestDifficultyWave)
        {
            EnemyPool highestPool = enemyPools.FirstOrDefault(p =>
                string.Equals(p.poolName, highestDifficultyPoolName, System.StringComparison.OrdinalIgnoreCase));

            if (highestPool == null && enemyPools.Count > 0)
            {
                highestPool = enemyPools[enemyPools.Count - 1];
            }

            if (highestPool != null && highestPool.enemyPrefabs != null)
            {
                combinedPool.AddRange(highestPool.enemyPrefabs);
            }
        }
        else if (waveConfigs != null && waveConfigs.Count > 0)
        {
            // 2. Use the configured WaveConfig for the current wave (clamped to the last configured wave if exceeded before automation)
            int configIndex = Mathf.Clamp(currentWave - 1, 0, waveConfigs.Count - 1);
            WaveConfig activeConfig = waveConfigs[configIndex];

            if (activeConfig != null && activeConfig.poolRules != null)
            {
                foreach (var rule in activeConfig.poolRules)
                {
                    bool started = enemiesKilledInCurrentWave >= rule.startAfterKillsInWave;
                    bool notStopped = rule.stopAfterKillsInWave < 0 || enemiesKilledInCurrentWave < rule.stopAfterKillsInWave;

                    if (started && notStopped)
                    {
                        EnemyPool matchingPool = enemyPools.FirstOrDefault(p =>
                            string.Equals(p.poolName, rule.poolName, System.StringComparison.OrdinalIgnoreCase));

                        if (matchingPool != null && matchingPool.enemyPrefabs != null)
                        {
                            combinedPool.AddRange(matchingPool.enemyPrefabs);
                        }
                    }
                }
            }
        }

        // Fallback if no rules matched the current kill count in the wave
        if (combinedPool.Count == 0 && enemyPools.Count > 0)
        {
            combinedPool.AddRange(enemyPools[0].enemyPrefabs);
        }

        return combinedPool.Where(p => p != null).Distinct().ToList();
    }

    private GameObject GetNextEnemyForCurrentWave(List<string> extraExcludedNames = null)
    {
        List<GameObject> candidatePrefabs = GetEligiblePrefabsForCurrentState();
        if (candidatePrefabs.Count == 0) return null;

        var existingNames = aliveEnemies
            .Where(e => e != null && !e.dead)
            .Select(e => e.characterName)
            .ToList();

        if (extraExcludedNames != null)
        {
            existingNames.AddRange(extraExcludedNames);
        }

        // Filter out enemies that are already active on the field so individual draws remain unique when possible
        List<GameObject> available = candidatePrefabs.Where(prefab =>
        {
            Enemy meta = prefab.GetComponent<Enemy>();
            return meta != null && !existingNames.Contains(meta.characterName);
        }).ToList();

        if (available.Count == 0)
        {
            return candidatePrefabs[UnityEngine.Random.Range(0, candidatePrefabs.Count)];
        }

        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    private void AddScore(int points)
    {
        currentScore += points;
        UpdateEndlessScoreUI();
    }

    private void UpdateEndlessScoreUI()
    {
        if (waveText != null) waveText.text = $" {currentWave}";
        if (scoreText != null) scoreText.text = $" {currentScore}";
        if (galdText != null) galdText.text = $" {currentGald}";
    }

    public IEnumerator MoveBackToPosition(Transform target, Vector3 startPos, float duration)
    {
        float elapsed = 0f;
        Vector3 currentPos = target.position;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            target.position = Vector3.Lerp(currentPos, startPos, elapsed / duration);
            yield return null;
        }
        if (target != null) target.position = startPos;
    }

    public void StartBattle()
    {
        isEndingGame = false;
        currentTurn = 0; 
        isBattleActive = true;
        enemies = new List<Enemy> { null, null, null, null };
        aliveEnemies = new List<Enemy>();
        allEncounteredMoves.Clear();

        totalEnemiesKilled = 0;
        currentScore = 0;
        currentWave = 1;
        enemiesSpawnedInCurrentWave = 0;
        enemiesKilledInCurrentWave = 0;
        pendingReplacementsCount = 0;
        isWaveTransitioning = false;
        if (loseScreen != null) loseScreen.SetActive(false);
    

        if (currentGameMode == GameMode.Standard)
        {
            GameObject knightObj = SpawnPrefabAtPosition(knightPrefab, enemySpawns[0]);
            GameObject swordsmanObj = SpawnPrefabAtPosition(swordsmanPrefab, enemySpawns[1]);
            GameObject mageObj = SpawnPrefabAtPosition(magePrefab, enemySpawns[2]);
            GameObject archerObj = SpawnPrefabAtPosition(archerPrefab, enemySpawns[3]);

            enemies[0] = knightObj.GetComponent<Enemy>();
            enemies[1] = swordsmanObj.GetComponent<Enemy>();
            enemies[2] = mageObj.GetComponent<Enemy>();
            enemies[3] = archerObj.GetComponent<Enemy>();

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Init(popupPrefab);
                enemyHUDs[i].Init(enemies[i]);
                aliveEnemies.Add(enemies[i]);
            }
        }
        else // Endless mode
        {
            UpdateEndlessScoreUI();
            SpawnInitialWaveEnemies(instant: false);
        }

        foreach (var attack in attacksReset)
        {
            attack.upgradeLevel = 0;
        }

        targetingMode = false;
        targetingHUDParent.SetActive(false);

        GameObject bossObj = SpawnPrefabAtPosition(bossPrefab, bossSpawn);
        hero = bossObj.GetComponent<Hero>();
        hero.bideLevel = heroBideLevelBase;
        hero.bideLevelBase = heroBideLevelBase;
        hero.Init(popupPrefab);
        
        bossHUD.Init(hero);

        foreach (var obj in listOfObjectToDeactivateAtStartOfBattle)
        {
            obj.SetActive(false);
        }

        MainUIParent.SetActive(true);
        enemyAttacksUsed = new List<AttackSO>();
        blurbEvent = new GameplayBlurbEvent();
        #if USE_CRAZYGAMES
        CrazySDK.Game.GameplayStart();
        #endif
    }

    private void SpawnInitialWaveEnemies(bool instant)
    {
        int slotsToSpawn = Mathf.Min(4, enemiesPerWave - enemiesSpawnedInCurrentWave);
        List<string> chosenNames = new List<string>();

        for (int i = 0; i < slotsToSpawn; i++)
        {
            GameObject prefab = GetNextEnemyForCurrentWave(chosenNames);
            if (prefab == null) continue;

            Enemy meta = prefab.GetComponent<Enemy>();
            if (meta != null) chosenNames.Add(meta.characterName);

            enemiesSpawnedInCurrentWave++;
            float runDuration = instant ? 0f : waveSpawnRunDuration;
            StartCoroutine(SpawnAndRunInRoutine(prefab, i, runDuration));
        }
    }

    private IEnumerator SpawnAndRunInRoutine(GameObject prefab, int spawnIndex, float runDuration)
    {
        Transform spawnPoint = enemySpawns[spawnIndex];
        Vector3 targetPos = spawnPoint.position;
        Vector3 startPos = runDuration > 0f ? targetPos + spawnRunInOffset : targetPos;

        GameObject enemyObj = SpawnPrefabAtPosition(prefab, spawnPoint);
        enemyObj.transform.position = startPos;

        Enemy newEnemy = enemyObj.GetComponent<Enemy>();
        newEnemy.Init(popupPrefab);

        enemies[spawnIndex] = newEnemy;
        if (!aliveEnemies.Contains(newEnemy)) aliveEnemies.Add(newEnemy);
        enemyHUDs[spawnIndex].Init(newEnemy);

        if (runDuration > 0f)
        {
            Animator anim = enemyObj.GetComponentInChildren<Animator>();
            SetRunningAnimation(anim, true);

            float elapsed = 0f;
            while (elapsed < runDuration)
            {
                if (enemyObj == null) yield break;
                elapsed += Time.deltaTime;
                enemyObj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / runDuration);
                yield return null;
            }

            if (enemyObj != null)
            {
                enemyObj.transform.position = targetPos;
                SetRunningAnimation(anim, false);
            }
        }
    }

    private void SetRunningAnimation(Animator anim, bool isRunning)
    {
        if (anim == null || string.IsNullOrEmpty(runAnimParameter)) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == runAnimParameter)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                    anim.SetBool(runAnimParameter, isRunning);
                else if (param.type == AnimatorControllerParameterType.Trigger && isRunning)
                    anim.SetTrigger(runAnimParameter);
                break;
            }
        }
    }

    private void ApplyEffectWithDelay(GameObject effectPrefab, Transform target, float delay, float effectDuration, bool? xRotationEffect = null, float raiseAmount = 0f, float? yRotationOffset = null, bool? standUpright = null, float xOffset = 0f, float zOffset = 0f) 
    {
        StartCoroutine(DelayedEffectCoroutine(effectPrefab, target, delay, effectDuration, raiseAmount, xRotationEffect, yRotationOffset, standUpright, xOffset, zOffset));
    }

    private IEnumerator DelayedEffectCoroutine(GameObject effectPrefab, Transform target, float delay, float effectDuration, float raiseAmount, bool? xRotationEffect, float? yRotationOffset = null, bool? standUpright = null, float xOffset = 0f, float zOffset = 0f)
    {
        yield return new WaitForSeconds(delay);
        
        if (target == null) yield break; 

        Vector3 effectPosition = target.position + new Vector3(xOffset, raiseAmount, zOffset);

        GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.identity); 

        effect.transform.SetParent(target);
        effect.transform.SetParent(null);
    
        if (yRotationOffset.HasValue)
        {   
            Vector3 direction = Quaternion.Euler(0f, yRotationOffset.Value, 0f) * Vector3.forward;
            effect.transform.rotation = Quaternion.LookRotation(direction);
            HS_ProjectileMover mover = effect.GetComponent<HS_ProjectileMover>();
            if (mover != null) mover.moveDirection = direction;
        }

        if (xRotationEffect.HasValue)
        {
            float xRotation = xRotationEffect.Value ? 90f : -90f;
            effect.transform.Rotate(0f, xRotation, 0f);
        }
    
        if (standUpright.HasValue)
        {
            float xRotation = standUpright.Value ? 90f : -90f;
            Vector3 currentEuler = effect.transform.rotation.eulerAngles;
            currentEuler.x = xRotation;
            effect.transform.rotation = Quaternion.Euler(currentEuler);
        }
        Destroy(effect, effectDuration);
    }

    public void SetUpBattle()
    {    if (bossHUD != null) bossHUD.gameObject.SetActive(true);

        StartCoroutine(DoBattleStartRoutine());
    }

    private IEnumerator DoBattleStartRoutine()
    {
        yield return new WaitForSeconds(.25f);
        EventBus.Publish(new FadeInEvent(UICategoryEnums.GamePlayUI));
        StartCoroutine(StartTurn());
    }

    private GameObject SpawnPrefabAtPosition(GameObject prefab, Transform pos)
    {
        GameObject spawned = GameObject.Instantiate(prefab);
        if (prefab.name.Contains("Boss")) spawned.tag = "Boss";
        spawned.transform.position = pos.transform.position;
        spawned.transform.rotation = pos.transform.rotation;
        return spawned;
    }

    public IEnumerator StartTurn()
    {
        // Wait if enemies are currently regenerating or transitioning between waves
        if (currentGameMode == GameMode.Endless)
        {
            yield return new WaitUntil(() => pendingReplacementsCount == 0 && !isWaveTransitioning);
        }

        currentTurn++; 
        if (!isBattleActive) yield break;

        drop1Opened = false;
        drop2Opened = false;

        yield return new WaitForSeconds(1.5f);

        if (!isBattleActive) yield break;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!isBattleActive) break;

            var enemy = enemies[i];
            if (enemy == null || enemy.dead) continue;

            usedMoveGO.SetActive(true);
            if (enemy.burning > 0)
            {
                enemy.Init(popupPrefabfire);
                enemy.TakeDamage((int)(enemy.maxHealth * .1));
                
                ApplyEffectWithDelay(burn, enemy.transform, 0f, 3.0f);
                if (enemy.currentHealth <= 0)
                {
                    enemy.currentHealth = 0; 
                    blurbEvent.Set($" {enemy.characterName} has been burnt to death!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{enemy.characterName} has been burnt to death!";    
                    enemy.RemoveBurns();              
                    RemoveEnemy(enemy); 
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    blurbEvent.Set($" {enemy.characterName} is burning for {enemy.burning} turns!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{enemy.characterName} is burning for {enemy.burning} turns!";
                    enemy.burning--;
                    if (enemy.burning <= 0) enemy.burnIcon.SetActive(true);
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        // If a burn death triggered a replacement or wave break, wait for it to finish
        if (currentGameMode == GameMode.Endless)
        {
            yield return new WaitUntil(() => pendingReplacementsCount == 0 && !isWaveTransitioning);
        }

        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.dead)
            {
                enemy.UpdateEffects();
            }
        }
    
        if (hero.currentHealth <= 0) 
        {
            EndGame(false);
            yield return null;
        }
            
        if (isBattleActive) StartCoroutine(DoTurnRoutine());
    }

    private IEnumerator DoTurnRoutine()
    {
        if (!isBattleActive) yield break;

        yield return new WaitForSeconds(1f);

        if (!isBattleActive) yield break;

        enemyAttacksByIndex.Clear();
        previousTurnMoves.Clear(); 
        while (enemyAttacksByIndexPerm.Count < enemies.Count)
        {
            enemyAttacksByIndexPerm.Add(null);
        }

        if (!isBattleActive) yield break;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!isBattleActive) break;

            Enemy enemy = enemies[i];

            if (enemy == null || enemy.dead)
            {
                AttackSO lastUsedAttack = enemyAttacksByIndexPerm[i];
                if (lastUsedAttack != null)
                {
                    if (!enemyAttacksByIndex.Contains(lastUsedAttack))
                    {
                        enemyAttacksByIndex.Add(lastUsedAttack);
                    }
                    if (!previousTurnMoves.Contains(lastUsedAttack))
                    {
                        previousTurnMoves.Add(lastUsedAttack);
                    }
                }
                continue;
            }

            enemy.Init(popupPrefab);
            hero.Init(popupPrefab);

            if (!enemy.CanAct())
            {
                AttackSO skippedAttack = enemyAttacksByIndexPerm[i];
                if (skippedAttack != null && !enemyAttacksByIndex.Contains(skippedAttack))
                {
                     enemyAttacksByIndex.Add(skippedAttack);
                }

                blurbEvent.Set($"{enemy.characterName} is paralyzed and cannot act this turn!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"{enemy.characterName} is paralyzed and cannot act this turn!";
                usedMoveGO.SetActive(true);              
                ApplyEffectWithDelay(paralysisVFX, enemy.transform, 0f, 3.0f);
                yield return new WaitForSeconds(1f);
                continue;
            }
            else
            {
                AttackSO enemyAttack = enemy.PerformRandomAttack();
                usedMove1.text = $"{enemy.characterName} Used {enemyAttack.attackName}";
                usedMoveGO.SetActive(true);
                
                blurbEvent.Set($"{enemy.characterName} Used {enemyAttack.attackName}");
                EventBus.Publish(blurbEvent);
                enemyAttacksByIndexPerm[i] = enemyAttack;
                if (!allEncounteredMoves.Contains(enemyAttack))
                {
                    allEncounteredMoves.Add(enemyAttack);
                }

                if (!enemyAttacksByIndex.Contains(enemyAttack)) enemyAttacksByIndex.Add(enemyAttack);
                if (!previousTurnMoves.Contains(enemyAttack)) previousTurnMoves.Add(enemyAttack);
                            
                if (hero.bideUses == 2) 
                {
                    enemyAttacksByIndex.Clear();
                    enemyAttacksByIndex.AddRange(previousTurnMoves);
                }

                Debug.Log($"[ATTACK DIAGNOSTIC] {enemy.characterName} -> Hero | Move: {enemyAttack.attackName} | Base Dmg: {enemyAttack.GetDamage()} | Enemy Multiplier: {enemy.multiplier}");

                if (enemyAttack.attributes.Contains("Barrier"))
                {
                    Vector3 newPosition = enemy.transform.position;
                    newPosition.y += 1f;
                    GameObject tempGameObject = new GameObject();
                    tempGameObject.transform.position = newPosition;
                    Destroy(tempGameObject, 5f);

                    enemy.barrierCount += 1;
                    ApplyEffectWithDelay(barrier1, tempGameObject.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(barrier2, enemy.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(barrier3, enemy.transform, 0f, 2.0f);
                    blurbEvent.Set($"The Cleric gained a barrier.");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"The Cleric gained a barrier.";
                    yield return new WaitForSeconds(1.5f);
                    continue;
                }

                if (enemyAttack.attributes.Contains("Field"))
                {
                    foreach (var enemyHeal in enemies)
                    {
                        if (enemyHeal == null || enemyHeal.dead || enemyHeal.currentHealth <= 0) continue;
                        enemyHeal.Init(popupPrefabgreen);
                        
                        enemyHeal.HealDamage((int)(enemy.maxHealth * 0.2));
                        if (enemyHeal.currentHealth >= enemyHeal.maxHealth) enemyHeal.currentHealth = enemyHeal.maxHealth;

                        enemyHeal.RemoveBurns();
                        enemyHeal.RemoveParalysis();

                        ApplyEffectWithDelay(healfield, enemyHeal.transform, 0f, .50f, true, 0.10f, null, true);
                        ApplyEffectWithDelay(panaceaVFX, enemyHeal.transform, 0f, 2.0f);
                        blurbEvent.Set($"The heroes healed and status cured.");
                        usedMove1.text = $"The heroes healed and status cured.";
                        EventBus.Publish(blurbEvent);
                        yield return new WaitForSeconds(.2f);
                    }
                    yield return new WaitForSeconds(.5f);
                    continue;
                }

                if (enemyAttack.attributes.Contains("Heal"))
                {
                    var enemyHeal = enemies.Where(e => e != null && !e.dead && e.currentHealth > 0).OrderBy(e => e.currentHealth).FirstOrDefault(); 
                    if (enemyHeal != null)
                    {
                        enemyHeal.Init(popupPrefabgreen);
                        enemyHeal.HealDamage((int)(enemy.maxHealth * 0.2));
                        if (enemyHeal.currentHealth > enemyHeal.maxHealth) enemyHeal.currentHealth = enemyHeal.maxHealth;
                        
                        ApplyEffectWithDelay(heal, enemyHeal.transform, 0f, 2.0f);
                        blurbEvent.Set($"{enemyHeal.characterName} was healed.");
                        usedMove1.text = $"{enemyHeal.characterName} was healed.";
                        EventBus.Publish(blurbEvent);
                        EventBus.Publish(statusUpdateEvent);
                        yield return new WaitForSeconds(1.5f);
                    }
                    continue;
                }

                if (enemyAttack.attributes.Contains("Ice"))
                {   
                    hero.TakeDamage(enemyAttack.GetDamage());
                    hero.TakeDamage(enemyAttack.GetDamage());

                    float yRotationOffset = -90f;
                    switch (i)
                    {
                        case 0: yRotationOffset = -70f; break;
                        case 1: yRotationOffset = -90f; break;
                        case 2: yRotationOffset = -90f; break;
                        case 3: yRotationOffset = -110f; break;
                    }
                    ApplyEffectWithDelay(iceAttack, enemy.transform, 0f, .20f, null, 2f, yRotationOffset);
                    ApplyEffectWithDelay(iceHit, hero.transform, .2f, 3.5f, null, 2f, null, null, 0.7f);
                }

                if (enemyAttack.attributes.Contains("Lunge"))
                {
                    hero.TakeDamage(enemyAttack.GetDamage());
                    ApplyEffectWithDelay(lungeAttack, enemy.transform, 0f, 3.0f, false, 0f);
                    ApplyEffectWithDelay(lungeHit, hero.transform, .5f, 3.0f);
                }

                if (enemyAttack.attributes.Contains("Slash"))
                {
                    Vector3 newPosition = hero.transform.position;
                    newPosition.y += 3f;
                    GameObject tempGameObject = new GameObject();
                    tempGameObject.transform.position = newPosition;
                    Destroy(tempGameObject, 5f);

                    ApplyEffectWithDelay(slashAttack, enemy.transform, 0f, 3.0f, null, 1.5f); 
                    ApplyEffectWithDelay(slashHit, tempGameObject.transform, 0.2f, 3.0f);
                    ApplyEffectWithDelay(slashCrater, hero.transform, 0.4f, 3.0f);
                }

                if (enemyAttack.attributes.Contains("Steady"))
                {
                    Vector3 originalPos = hero.transform.position;
                    ApplyEffectWithDelay(SteadyAttack, enemy.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(SteadyHit, hero.transform, .5f, 3.0f);
                    for (int j = 0; j < 10; j++)
                    {
                        hero.StartCoroutine(hero.CallDoHitRoutine(enemyAttack.GetDamage() / 10, .1f, hero.gameObject));
                        yield return new WaitForSeconds(.1f);
                    }
                    hero.StartCoroutine(MoveBackToPosition(hero.transform, originalPos, 0.2f));
                    yield return new WaitForSeconds(.5f);
                }

                if (enemyAttack.attributes.Contains("Triple"))
                {
                    hero.TakeDamage(enemyAttack.GetDamage());
                    hero.TakeDamage(enemyAttack.GetDamage());
                    
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() <= 0 && hero.burning <= 0)
                    {
                        hero.ApplyParalysis(5, true);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        EventBus.Publish(blurbEvent);
                        usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                    }
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() <= 0 && hero.burning <= 0)
                    {
                        hero.ApplyParalysis(5, true);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                        EventBus.Publish(blurbEvent);
                    }
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() <= 0 && hero.burning <= 0)
                    {
                        hero.ApplyParalysis(5, true);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                        EventBus.Publish(blurbEvent);
                    }

                    float yRotationOffset = -90f;
                    switch (i)
                    {
                        case 0: yRotationOffset = -70f; break;
                        case 1: yRotationOffset = -90f; break;
                        case 2: yRotationOffset = -90f; break;
                        case 3: yRotationOffset = -115f; break;
                    }
                    ApplyEffectWithDelay(tripleAttack, enemy.transform, 0f, 3.0f, null, 0f, yRotationOffset); 
                    ApplyEffectWithDelay(tripleHit, hero.transform, .5f, 3.0f, null, 1.5f);
                }

                hero.TakeDamage(Mathf.RoundToInt(enemyAttack.GetDamage() * enemy.multiplier));
                blurbEvent.Set($"{enemy.characterName} used {enemyAttack.attackName}!");
                EventBus.Publish(blurbEvent);

                if (enemyAttack.attributes.Contains("Burn"))
                {
                    bool canInflictStatus = true;
                    if (isFirstMatch && currentTurn == 1) canInflictStatus = false;

                    if (canInflictStatus && UnityEngine.Random.value <= 1f && hero.GetParalysisTurnsRemaining() <= 0 && hero.burning <= 0)
                    {
                        hero.ApplyBurn(10, 3);
                        blurbEvent.Set($"Boss has been burned by {enemyAttack.attackName}!");
                        EventBus.Publish(blurbEvent);
                        usedMove1.text = $"Boss has been burned by {enemyAttack.attackName}!";
                    }
                    float yRotationOffset = -90f;
                    switch (i)
                    {
                        case 0: yRotationOffset = -70f; break;
                        case 1: yRotationOffset = -90f; break;
                        case 2: yRotationOffset = -90f; break;
                        case 3: yRotationOffset = -110f; break;
                    }
                    ApplyEffectWithDelay(fireAttack, enemy.transform, 0f, 3.0f, null, 1.5f, yRotationOffset); 
                    ApplyEffectWithDelay(fireHit, hero.transform, .5f, 3.0f);
                }

                if (enemyAttack.attributes.Contains("Paralysis"))
                {
                    if (UnityEngine.Random.value <= 1f && hero.GetParalysisTurnsRemaining() <= 0 && hero.burning <= 0)
                    {
                        hero.ApplyParalysis(5, false);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        EventBus.Publish(blurbEvent);
                        usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                    }
                    float yRotationOffset = -90f;
                    switch (i)
                    {
                        case 0: yRotationOffset = -70f; break;
                        case 1: yRotationOffset = -90f; break;
                        case 2: yRotationOffset = -90f; break;
                        case 3: yRotationOffset = -110f; break;
                    }
                    ApplyEffectWithDelay(arrowAttack1, enemy.transform, 0f, .40f, null, 1.5f, yRotationOffset);
                    ApplyEffectWithDelay(arrowHit, hero.transform, .5f, 3.0f);
                }

                yield return new WaitForSeconds(1.5f);

                if (hero.currentHealth <= 0) yield break;
            }
        }

        yield return new WaitForSeconds(.5f);
        if (!isBattleActive) yield break;

        hero.StartCoroutine(hero.ResetTransformRoutine(0f));

        hero.bideBuff = false;
        hero.UpdateEffects();

        if (hero.burning > 0)
        { 
            hero.Init(popupPrefabfire);
            hero.TakeDamage((int)(hero.maxHealth * .1));
            
            ApplyEffectWithDelay(burn, hero.transform, 0f, 3.0f);
            blurbEvent.Set($" Boss is burning for {hero.burning} turns!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = $" Boss is burning for {hero.burning} turns!";
            
            hero.burning--;
            if (hero.burning <= 0) hero.burnIcon.SetActive(false);
            
            yield return new WaitForSeconds(1f);
            if (hero.currentHealth <= 0)
            {
                EndGame(false);
                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }
        
        bool isParalyzedAndSkipping = !hero.CanAct(); 

        if (isParalyzedAndSkipping && hero.paralysisEffect != null && !hero.paralysisEffect.hasHadFirstTurnCheck && isFirstMatch)
        {
            hero.paralysisEffect.hasHadFirstTurnCheck = true; 
            isParalyzedAndSkipping = false; 
        }
        else if (isParalyzedAndSkipping && hero.paralysisEffect != null)
        {
            hero.paralysisEffect.hasHadFirstTurnCheck = true;
        }

        if (isParalyzedAndSkipping) 
        {
            blurbEvent.Set($"Boss is paralyzed and cannot act this turn!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = $"Boss is paralyzed and cannot act this turn!";
            ApplyEffectWithDelay(paralysisVFX, hero.transform, 0f, 3.0f);
            yield return new WaitForSeconds(1f);
            usedMove1.text = "";
            usedMoveGO.SetActive(false);
            if (isBattleActive) StartCoroutine(StartTurn());
        }
        else 
        {
            if (bideAttribute > 0) bideAttribute--;
            if (bideAttribute == 0)
            {
                hero.bideLevel = hero.bideLevelBase;
                hero.bideUses = 0;
            }

            enemyAttacksByIndex.Clear();

            if (bideAttribute > 0 && hero.bideUses == 2)
            {
                enemyAttacksByIndex.AddRange(allEncounteredMoves);
            }
            else 
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemyAttacksByIndexPerm[i] != null) 
                    {
                        if (!enemyAttacksByIndex.Contains(enemyAttacksByIndexPerm[i]))
                        {
                            enemyAttacksByIndex.Add(enemyAttacksByIndexPerm[i]);
                        }
                    }
                }
            }

            usedMoveGO.SetActive(false);
            if (isBattleActive) ShowAttackSelectionUI();
        }
    }

    private void SetupAttackDropdowns()
    {
        attackDropdown.onValueChanged.AddListener(OnAttack1Selected);
        attackDropdown2.onValueChanged.AddListener(OnAttack2Selected);

        AddDropdownOpenListener(attackDropdown);
        AddDropdownOpenListener(attackDropdown2);
    }

    private void AddDropdownOpenListener(TMP_Dropdown dropdown)
    {
        EventTrigger trigger = dropdown.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = dropdown.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((_) => { StartCoroutine(OnDropdownOpened(dropdown)); });
        trigger.triggers.Add(entry);
    }

    private void OnAttack1Selected(int index)
    {
        if (index < 0 || index >= enemyAttacksByIndex.Count) return;
        selectedAttack1 = enemyAttacksByIndex[index];
    }

    private void OnAttack2Selected(int index)
    {
        if (index < 0 || index >= enemyAttacksByIndex.Count) return;
        selectedAttack2 = enemyAttacksByIndex[index];
    }

    private void ShowAttackSelectionUI()
    {
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        List<string> addedAttackNames = new List<string>();

        currentlyDisplayedAttacks.Clear();

        for (int i = 0; i < enemyAttacksByIndex.Count; i++)
        {
            var attack = enemyAttacksByIndex[i];

            if (!addedAttackNames.Contains(attack.attackName))
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(attack.attackName);
                dropdownOptions.Add(newOption);
                addedAttackNames.Add(attack.attackName);
                
                currentlyDisplayedAttacks.Add(attack);

                if (bideAttribute >= 1)
                {
                    attack.upgradeLevel += 1; 
                    if (attack.upgradeLevel > 3) attack.upgradeLevel = 3; 
                }
            }
        }

        attackDropdown.ClearOptions();
        attackDropdown2.ClearOptions();
        attackDropdown.AddOptions(dropdownOptions);
        attackDropdown2.AddOptions(dropdownOptions);

        if (enemyAttacksByIndex.Count > 0)
        {
            attackDropdown.value = 0;
            OnAttack1Selected(0);
        }
        if (enemyAttacksByIndex.Count > 1)
        {
            attackDropdown2.value = 1;
            OnAttack2Selected(1);
        }

        attackOptionsParent.SetActive(true);
        potionOptions.SetActive(true);
        panaceaAmountText.text = PotionData.Instance.Panacea.ToString();
        potionAmountText.text = PotionData.Instance.Potion.ToString();
    }

    private IEnumerator OnDropdownOpened(TMP_Dropdown dropdown)
    {
        Transform dropdownContent = null;
        for (int i = 0; i < 10; i++) 
        {
            dropdownContent = dropdown.transform.Find("Dropdown List/Viewport/Content");
            if (dropdownContent != null) break;
            yield return null;
        }

        if (dropdownContent == null) yield break;

        yield return null;

        int optionIndex = 0;

        foreach (Transform child in dropdownContent)
        {
            if (child.name.StartsWith("Item") && optionIndex < currentlyDisplayedAttacks.Count)
            {
                AttackSO attack = currentlyDisplayedAttacks[optionIndex];

                Transform arrow1 = child.Find("Item Bide Level 1");
                Transform arrow2 = child.Find("Item Bide Level 2");
                Transform arrow3 = child.Find("Item Bide Level 3");

                if (arrow1 != null) arrow1.gameObject.SetActive(attack.upgradeLevel >= 1);
                if (arrow2 != null) arrow2.gameObject.SetActive(attack.upgradeLevel >= 2);
                if (arrow3 != null) arrow3.gameObject.SetActive(attack.upgradeLevel >= 3);

                EventTrigger trigger = child.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = child.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Clear();

                string desc = attack.attackDescription; 
                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                pointerEnterEntry.callback.AddListener((_) => ShowDescription(desc));
                trigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExitEntry.callback.AddListener((_) => HideDescription());
                trigger.triggers.Add(pointerExitEntry);

                optionIndex++;
            }
        }
    }

    private void ShowDescription(string description)
    {
        if (descriptionText != null) descriptionText.text = description;
    }

    private void HideDescription()
    {
        descriptionText.text = "Select a move";
    }

    public void OnConfirmButtonClicked()
    {
        usedMoveGO.SetActive(true);
        if (selectedAttack1 == null || selectedAttack2 == null)
        {
            blurbEvent.Set("Please select both attacks!");
            usedMove1.text = "Please select both attacks!";
            EventBus.Publish(blurbEvent);
            return;
        }

        if (selectedAttack1 == selectedAttack2)
        {
            blurbEvent.Set("Select a different move!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = "Select a different move!";
            return;
        }

        hero.CombineAttacks(selectedAttack1, selectedAttack2);

        int maxTargets = GetMaxTargetsForAttack(combinedAttack.attributes);
        var aliveEnemiesList = enemies.Where(e => e != null && !e.dead).ToList();
        
        if (aliveEnemiesList.Count == 1 || (IsMultiTargetAttack(combinedAttack.attributes) && maxTargets >= aliveEnemiesList.Count && aliveEnemiesList.Count > 0))
        {
            usedMove1.text = $"Boss Used {combinedAttack.attackName}";
            
            selectedAttack1 = null;
            selectedAttack2 = null;
            attackOptionsParent.SetActive(false);
            attackOptionsMenu.SetActive(false);
            potionOptions.SetActive(false);

            if (bossAttackCoroutine != null) StopCoroutine(bossAttackCoroutine);
            
            bossAttackCoroutine = StartCoroutine(DoBossAttackRoutine(aliveEnemiesList.ToArray()));
            return;
        }

        if (maxTargets > 1 && IsMultiTargetAttack(combinedAttack.attributes))
        {
            usedMove1.text = $"Select {maxTargets} targets";
        }
        else
        {
            usedMove1.text = "Choose a target";
        }

        selectedAttack1 = null;
        selectedAttack2 = null;

        attackOptionsParent.SetActive(false);
        attackOptionsMenu.SetActive(false);
        potionOptions.SetActive(false);
        targetingHUDParent.SetActive(true);
        
        targetingMode = true;
        selectingEnemies = false;
        selectedEnemies.Clear();
    }

    public void OnItemOptionsClicked()
    {
        itemOptions.SetActive(true);
        panaceaAmountText.text = PotionData.Instance.Panacea.ToString();
        potionAmountText.text = PotionData.Instance.Potion.ToString();
    }

    public void OnBideButtonClicked()
    {
        if (!isBattleActive) return;

        bool bideSuccessful = hero.UseBide();

        if (bideSuccessful)
        {
            ApplyEffectWithDelay(bideVFX, hero.transform, 0f, 2.0f, true, 0f, null, false);
            bideAttribute = 2;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            hero.bideBuff = true;
            bideBuff = true;
            usedMove1.text = "You bided for a turn";
            usedMoveGO.SetActive(true);
            StartCoroutine(StartTurn());
        }
    }

    public void OnPotionClicked()
    {
        if (!isBattleActive) return;

        if (PotionData.Instance.Potion > 0)
        {
            if (hero.currentHealth >= hero.maxHealth)
            {
                descriptionTextPotion.text = "Hero Health Full.";
                return; 
            }

            hero.Init(popupPrefabgreen);
            hero.HealDamage((int)(hero.maxHealth * .4));
            
            if (hero.currentHealth > hero.maxHealth)
            {
                hero.currentHealth = hero.maxHealth;
            }
            if (bideAttribute > 0)
            {
                bideAttribute = 2;
            }
            if (bideAttribute == 0)
            {
                hero.bideLevel = hero.bideLevelBase;
                hero.bideUses = 0;
            }
            usedMoveGO.SetActive(true);
            blurbEvent.Set($"Potion Used: Health Restored!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = "Potion Used";
            PotionData.Instance.Potion--;
            if (attackOptionsParent != null) attackOptionsParent.SetActive(false);
            if (attackOptionsMenu != null) attackOptionsMenu.SetActive(false); // Fixes the glitch
            if (potionOptions != null) potionOptions.SetActive(false);
            if (itemOptions != null) itemOptions.SetActive(false);
            
            ApplyEffectWithDelay(potionVFX, hero.transform, 0f, 2.0f);
            EventBus.Publish(statusUpdateEvent);
            StartCoroutine(StartTurn());
        }
        else
        {
            descriptionTextPotion.text = "No Potion Left.";
            usedMove1.text = "No Potion";
        }
    }

    public void OnPanaceaClicked()
    {
        if (!isBattleActive) return;

        if (PotionData.Instance.Panacea > 0)
        {
            bool hasStatus = hero.burning > 0 || hero.paralysisEffect != null;

            if (!hasStatus)
            {
                descriptionTextPotion.text = "No Status Effects to Cure.";
                return;
            }
            if (bideAttribute > 0)
            {
                bideAttribute = 2;
            }
            if (bideAttribute == 0)
            {
                hero.bideLevel = hero.bideLevelBase;
                hero.bideUses = 0;
            }

            hero.RemoveBurns();
            hero.RemoveParalysis();
            hero.RemoveHeroBurns();
            hero.RemoveHeroParalysis();
            usedMoveGO.SetActive(true);
            blurbEvent.Set($"Panacea Used: Status Cured!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = "Status Cured";
            PotionData.Instance.Panacea--;
            if (attackOptionsParent != null) attackOptionsParent.SetActive(false);
            if (attackOptionsMenu != null) attackOptionsMenu.SetActive(false); // Fixes the glitch
            if (potionOptions != null) potionOptions.SetActive(false);
            if (itemOptions != null) itemOptions.SetActive(false);

            ApplyEffectWithDelay(panaceaVFX, hero.transform, 0f, 2.0f);
            StartCoroutine(StartTurn());
        }
        else
        {
            descriptionTextPotion.text = "No Panacea left";
            usedMove1.text = "No Panacea";
        }
    }

    public void SelectEnemyToAttack(Enemy enemy)
    {
        if (!targetingMode || enemy == null || enemy.dead) return;

        int maxTargets = GetMaxTargetsForAttack(combinedAttack.attributes);

        if (maxTargets <= 1 || !IsMultiTargetAttack(combinedAttack.attributes))
        {
            blurbEvent.Set("Attacking");
            EventBus.Publish(blurbEvent);

            if (bossAttackCoroutine != null) StopCoroutine(bossAttackCoroutine);

            targetingMode = false;
            targetingHUDParent.SetActive(false);
            bossAttackCoroutine = StartCoroutine(DoBossAttackRoutine(enemy));
            usedMove1.text = ($"Boss Used {combinedAttack.attackName}");
            
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == enemy)
                {
                    selectedEnemyNum = i;
                    break;
                }
            }
        }
        else
        {
            if (!selectingEnemies)
            {
                selectedEnemies.Clear();
                selectingEnemies = true;
                blurbEvent.Set($"Select {maxTargets} targets!");
                EventBus.Publish(blurbEvent);
            }

            if (selectedEnemies.Contains(enemy))
            {
                blurbEvent.Set("Enemy already selected! Choose a different enemy.");
                EventBus.Publish(blurbEvent);
                return;
            }

            selectedEnemies.Add(enemy);
            usedMove1.text = $"Select {maxTargets} targets"; 
            blurbEvent.Set($"Selected: {enemy.characterName} ({selectedEnemies.Count}/{maxTargets})"); 
            EventBus.Publish(blurbEvent);

            if (selectedEnemies.Count == maxTargets)
            {
                usedMove1.text = ($"Boss Used {combinedAttack.attackName}");
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] == selectedEnemies[0])
                    {
                        selectedEnemyNum = i;
                        break;
                    }
                }

                StartCoroutine(DoBossAttackRoutine(selectedEnemies.ToArray()));
                selectingEnemies = false;
                targetingHUDParent.SetActive(false);
                targetingMode = false;
            }
        }
    }

    private int GetMaxTargetsForAttack(List<string> attributes)
    {
        if (attributes.Contains("Field")) return 4;
        else if (attributes.Contains("Ice")) return 3; 
        else if (attributes.Contains("Lunge")) return 2;
        else return 1; 
    }

    private bool IsMultiTargetAttack(List<string> attributes)
    {
        return GetMaxTargetsForAttack(attributes) > 1;
    }

    // --- PLAYER ATTACK ROUTINE ---
    private IEnumerator DoBossAttackRoutine(params Enemy[] targetEnemies)
    {
        if (!isBattleActive) yield break;

        List<string> allTargetNames = new List<string>();
        foreach (var t in targetEnemies)
        {
            if (t != null) allTargetNames.Add(t.characterName);
        }

        List<string> uniqueNames = allTargetNames.Distinct().ToList();

        if (uniqueNames.Count > 0)
        {
            attackNamesText = "";
            if (uniqueNames.Count == 1)
            {
                attackNamesText = uniqueNames[0];
            }
            else
            {
                attackNamesText = string.Join(", ", uniqueNames.Take(uniqueNames.Count - 1)) + " and " + uniqueNames.Last();
            }

            string fullAttackMsg = $"Boss attacked {attackNamesText} with {combinedAttack.attackName}";
            blurbEvent.Set(fullAttackMsg);
            EventBus.Publish(blurbEvent);
            usedMove1.text = fullAttackMsg;
        }

        List<string> paralyzedNames = new List<string>();
        List<string> defeatedNames = new List<string>(); 

        hasIced = false; 
        hasLunged = false;

        foreach (var targetEnemy in targetEnemies)
        {
            if (!isBattleActive) yield break;
            if (targetEnemy == null) continue;

            int currentTargetIndex = enemies.IndexOf(targetEnemy);
            
            bool isBurn = combinedAttack.attributes.Contains("Burn");
            bool isIce = combinedAttack.attributes.Contains("Ice");
            bool isPara = combinedAttack.attributes.Contains("Paralysis");
            bool isLunge = combinedAttack.attributes.Contains("Lunge");
            bool isHybridElements = (isBurn && isIce) || (isBurn && isPara) || (isIce && isPara);

            targetEnemy.Init(popupPrefab); 

            yield return new WaitForSeconds(0.2f);

            if (!heroAniPlayed)
            {
                hero.DoAttackAnimation();
                heroAniPlayed = true;
            }

            int baseDamage = hero.GetDamage();
            int damage = baseDamage;
            float upgradeMultiplier = 1.0f;
            bool isCrit = false;

            if (bideAttribute > 0)
            {
                upgradeMultiplier = 1.0f + (combinedAttack.upgradeLevel / 30f);
                damage = Mathf.RoundToInt(baseDamage * hero.bideLevel * upgradeMultiplier);
            }

            if (combinedAttack.attributes.Contains("Steady"))
            {
                hero.Init(popupPrefab);
                heroCritRate = 0.4f;
                ApplyEffectWithDelay(SteadyAttack, hero.transform, 0f, 2.0f);
                ApplyEffectWithDelay(SteadyHit, targetEnemy.transform, .5f, 3.0f);
            }

            if (UnityEngine.Random.value <= heroCritRate)
            {
                isCrit = true;
                damage = (int)(damage * critMultiplier);
                blurbEvent.Set("Critical Hit!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = "Critical Hit"; 
                targetEnemy.TakeDamage(damage);
            }
            else
            {
                targetEnemy.TakeDamage(damage);
            }

            Debug.Log($"[ATTACK DIAGNOSTIC] Hero -> {targetEnemy.characterName} | Base Dmg: {baseDamage} | Bide Active: {(bideAttribute > 0)} | Bide Multiplier: {hero.bideLevel} | Upgrade Mult: {upgradeMultiplier} | Crit Hit: {isCrit} | Final Outgoing Dmg: {damage}");

            if (isBurn)
            {
                hero.Init(popupPrefab);
                randomChance = (bideAttribute > 0) ? 0.2f : 0.2f;

                if (UnityEngine.Random.value <= 1 && targetEnemy.GetParalysisTurnsRemaining() <= 0 && targetEnemy.burning <= 0)
                {
                    targetEnemy.ApplyBurn(1000, 3);
                    blurbEvent.Set($"{targetEnemy.characterName} was burned!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{targetEnemy.characterName} was burned!";
                }

                if (!isLunge && !isHybridElements)
                {
                    float yRotationOffset = 0f;
                    switch (currentTargetIndex)
                    {
                        case 0: yRotationOffset = 30f; break;
                        case 1: yRotationOffset = 5f; break;
                        case 2: yRotationOffset = 0f; break;
                        case 3: yRotationOffset = -25f; break;
                    }

                    Vector3 newPosition = hero.transform.position;
                    GameObject tempGameObject = new GameObject();
                    tempGameObject.transform.position = newPosition;
                    Destroy(tempGameObject, 5f);
                    ApplyEffectWithDelay(fireAttack, tempGameObject.transform, 0f, 3.0f, true, 1.5f, yRotationOffset);
                    ApplyEffectWithDelay(fireHit, targetEnemy.transform, 0.5f, 3.0f);
                }
            }

            if (isPara)
            {
                hero.Init(popupPrefab);

                if (UnityEngine.Random.value <= 1f && targetEnemy.GetParalysisTurnsRemaining() <= 0 && targetEnemy.burning <= 0)
                {
                    targetEnemy.ApplyParalysis(5, true);
                    if (!paralyzedNames.Contains(targetEnemy.characterName))
                    {
                        paralyzedNames.Add(targetEnemy.characterName);
                    }
                }

                if (!isLunge && !isHybridElements)
                {
                    float yRotationOffset = 0f;
                    Vector3 newPosition = hero.transform.position;
                    GameObject tempGameObject = new GameObject();
                    tempGameObject.transform.position = newPosition;
                    switch (currentTargetIndex)
                    {
                        case 0: yRotationOffset = 115f; break;
                        case 1: yRotationOffset = 95f; break;
                        case 2: yRotationOffset = 75f; break;
                        case 3: yRotationOffset = 60f; break;
                    }
                    Destroy(tempGameObject, 5f);
                    ApplyEffectWithDelay(paralysisAttack, tempGameObject.transform, 0f, 3.0f, true, 1.5f, yRotationOffset);
                    ApplyEffectWithDelay(paralysisHit, targetEnemy.transform, .5f, 3.0f);
                }
            }

            if (combinedAttack.attributes.Contains("Heal"))
            {
                hero.Init(popupPrefabgreen);
                hero.HealDamage(damage);
                blurbEvent.Set($"{damage} Health Recovered!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"Boss attacked {attackNamesText} with {combinedAttack.attackName} and recovered {damage} Health!";
                ApplyEffectWithDelay(heal, hero.transform, 0f, 2.0f);
                EventBus.Publish(statusUpdateEvent);
            }

            if (combinedAttack.attributes.Contains("Barrier"))
            {
                hero.Init(popupPrefab);
                Vector3 newPosition = hero.transform.position;
                Vector3 newScale = hero.transform.localScale;
                newPosition.y += 2.5f;
                newScale.x += 10f;
                newScale.y += 10f;
                newScale.z += 10f;

                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
                tempGameObject.transform.localScale = newScale;

                Destroy(tempGameObject, 5f);
                hero.barrierCount += 1;
                blurbEvent.Set("Barrier raised");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"Barrier Raised";
                ApplyEffectWithDelay(barrier1Big, tempGameObject.transform, 0f, 3.0f);
                ApplyEffectWithDelay(barrier2, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(barrier3, hero.transform, 0f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Field"))
            {
                hero.Init(popupPrefab);
                hero.HealDamage(damage);
                blurbEvent.Set($"{damage} Health Recovered!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"Boss attacked {attackNamesText} with {combinedAttack.attackName} and recovered {damage} Health!";
                hero.RemoveBurns();
                hero.RemoveParalysis();
                hero.RemoveHeroBurns();
                hero.RemoveHeroParalysis();
                blurbEvent.Set($"Status Healed");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"Status Healed";
                ApplyEffectWithDelay(healfield, hero.transform, 0f, 2.0f, true, 0.10f, null, true);
                ApplyEffectWithDelay(panaceaVFX, hero.transform, 0f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Slash"))
            {
                hero.Init(popupPrefab);
                Vector3 newPosition = targetEnemy.transform.position;
                newPosition.y += 3f;
                GameObject tempObj = new GameObject();
                tempObj.transform.position = newPosition;
                Destroy(tempObj, 5f);

                ApplyEffectWithDelay(slashAttack, hero.transform, 0f, 3.0f, null, 1.5f); 
                ApplyEffectWithDelay(slashHit, tempObj.transform, .5f, 3.0f);
                ApplyEffectWithDelay(slashCrater, targetEnemy.transform, .8f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Triple"))
            {
                hero.Init(popupPrefab);
                targetEnemy.TakeDamage(damage);
                targetEnemy.TakeDamage(damage);

                if (UnityEngine.Random.value <= .1f && targetEnemy.GetParalysisTurnsRemaining() <= 0 && targetEnemy.burning <= 0)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if (!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }
                if (UnityEngine.Random.value <= .1f && targetEnemy.GetParalysisTurnsRemaining() <= 0 && targetEnemy.burning <= 0)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if (!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }
                if (UnityEngine.Random.value <= .1f && targetEnemy.GetParalysisTurnsRemaining() <= 0 && targetEnemy.burning <= 0)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if (!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }

                float yRotationOffset = 0f;
                switch (currentTargetIndex)
                {
                    case 0: yRotationOffset = 115f; break;
                    case 1: yRotationOffset = 110f; break;
                    case 2: yRotationOffset = 90f; break;
                    case 3: yRotationOffset = 70f; break;
                }

                ApplyEffectWithDelay(tripleAttack, hero.transform, 0f, 3.0f, null, 0f, yRotationOffset, null);
                ApplyEffectWithDelay(tripleHit, targetEnemy.transform, .5f, 3.0f, null, 1.5f);
            }

            if (isIce)
            {
                hero.Init(popupPrefab);

                if (!isLunge && !isHybridElements)
                {
                    if (!hasIced)
                    {
                        float yRotationOffset = 0f;
                        switch (currentTargetIndex)
                        {
                            case 0: yRotationOffset = 115f; break;
                            case 1: yRotationOffset = 95f; break;
                            case 2: yRotationOffset = 75f; break;
                            case 3: yRotationOffset = 60f; break;
                        }

                        ApplyEffectWithDelay(iceAttack, hero.transform, 0f, 3.0f, null, 1.5f, yRotationOffset); 
                        hasIced = true;
                    }
                    ApplyEffectWithDelay(iceHit, targetEnemy.transform, 0.3f, 4.0f, null, 1f, null, null, 0.25f);
                }
                
                blurbEvent.Set($"You strike again");
                EventBus.Publish(blurbEvent);
            }

            if (!isLunge && isHybridElements) 
            {
                Vector3 newPos = hero.transform.position;
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPos;
                Destroy(tempGameObject, 5f);

                float yRot = -90f;
                switch (currentTargetIndex) 
                { 
                    case 0: yRot = 15f; break; 
                    case 1: yRot = 0f; break; 
                    case 2: yRot = -15f; break; 
                    case 3: yRot = -25f; break; 
                }

                if (isIce && isBurn)
                {
                    ApplyEffectWithDelay(iceBurnAttack, tempGameObject.transform, 0f, .40f, true, 1.5f, yRot);
                    ApplyEffectWithDelay(iceBurnHit, targetEnemy.transform, 0.2f, 4.0f, null, 0.5f, null, null, 0.78f, 0f);
                }
                else if (isIce && isPara)
                {
                    ApplyEffectWithDelay(iceParaAttack, tempGameObject.transform, 0f, .50f, true, 1.5f, yRot);
                    ApplyEffectWithDelay(iceParaHit, targetEnemy.transform, 0.2f, 4.0f, null, 0.5f, null, null, 0.78f, 0f);
                }
                else if (isBurn && isPara)
                {
                    ApplyEffectWithDelay(arrowBurnAttack, tempGameObject.transform, 0f, .50f, true, 1.5f, yRot);
                    ApplyEffectWithDelay(arrowBurnHit, targetEnemy.transform, .5f, 3.0f);
                }
            }

            if (isLunge)
            {
                hero.Init(popupPrefab);
                blurbEvent.Set($"You strike twice");
                EventBus.Publish(blurbEvent);

                GameObject currentLungeAttack = lungeAttack;
                GameObject currentLungeHit = lungeHit;

                if (isBurn && fireLungeAttack != null && fireLungeHit != null)
                {
                    currentLungeAttack = fireLungeAttack;
                    currentLungeHit = fireLungeHit;
                }
                else if (isIce && iceLungeAttack != null && iceLungeHit != null)
                {
                    currentLungeAttack = iceLungeAttack;
                    currentLungeHit = iceLungeHit;
                }
                else if (isPara && arrowLungeAttack != null && arrowLungeHit != null)
                {
                    currentLungeAttack = arrowLungeAttack;
                    currentLungeHit = arrowLungeHit;
                }

                ApplyEffectWithDelay(currentLungeAttack, hero.transform, 0f, 3.0f, true, 0f);
                ApplyEffectWithDelay(currentLungeHit, targetEnemy.transform, .5f, 3.0f);
            }

            // Check death AFTER all multi-hit damage (such as Triple) has resolved
            if (targetEnemy.currentHealth <= 0 && !targetEnemy.dead)
            {
                if (!defeatedNames.Contains(targetEnemy.characterName))
                {
                    defeatedNames.Add(targetEnemy.characterName);
                }
                RemoveEnemy(targetEnemy);
            }

            heroCritRate = 0.05f;
        } 

        if (paralyzedNames.Count > 0)
        {
            string combinedText = "";
            if (paralyzedNames.Count == 1) combinedText = $"{paralyzedNames[0]} was paralyzed by {combinedAttack.attackName}!";
            else
            {
                string names = string.Join(", ", paralyzedNames.Take(paralyzedNames.Count - 1)) + " and " + paralyzedNames.Last();
                combinedText = $"{names} were paralyzed by {combinedAttack.attackName}!";
            }
            blurbEvent.Set(combinedText);
            EventBus.Publish(blurbEvent);
            usedMove1.text = combinedText;
            usedMoveGO.SetActive(true);
            yield return new WaitForSeconds(1f); 
        }

        if (defeatedNames.Count > 0)
        {
            string combinedText = "";
            if (defeatedNames.Count == 1) combinedText = $"{defeatedNames[0]} has been defeated!";
            else
            {
                string names = string.Join(", ", defeatedNames.Take(defeatedNames.Count - 1)) + " and " + defeatedNames.Last();
                combinedText = $"{names} have been defeated!";
            }
            blurbEvent.Set(combinedText);
            EventBus.Publish(blurbEvent);
            usedMove1.text = combinedText;
            usedMoveGO.SetActive(true);
        }

        foreach (var attack in attacksReset)
        {
            if (attack != null) attack.upgradeLevel = 0;
        }

        bideAttribute = 0;
        hero.bideLevel = hero.bideLevelBase;
        hero.bideUses = 0;
        hero.bideBuff = false;

        heroAniPlayed = false;
        
        if (isBattleActive) StartCoroutine(StartTurn());
    }

    // --- ENDLESS ENEMY REMOVAL, REGENERATION & WAVE PROGRESSION ---
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.dead) return;

        enemy.dead = true;
        enemy.RemoveParalysis();
        enemy.RemoveBurns();

        if (currentGameMode == GameMode.Endless)
        {
            totalEnemiesKilled++;
            enemiesKilledInCurrentWave++;

            // +100 Score per defeat (Gald updates automatically as 1/4 of score)
            AddScore(scorePerDefeat);

            int spawnIndex = enemies.IndexOf(enemy);
            if (spawnIndex >= 0)
            {
                enemies[spawnIndex] = null;
            }
            aliveEnemies.Remove(enemy);

            // Fade out the dying enemy
            StartCoroutine(FadeOut(enemy.gameObject));

            // Check if there are still enemies left to spawn in this 12-enemy wave
            if (enemiesSpawnedInCurrentWave < enemiesPerWave && spawnIndex >= 0)
            {
                enemiesSpawnedInCurrentWave++;
                pendingReplacementsCount++;
                StartCoroutine(ReplaceEnemyAfterDelay(spawnIndex));
            }
            else if (enemiesKilledInCurrentWave >= enemiesPerWave)
            {
                // All 12 enemies in this wave have been defeated!
                if (!isWaveTransitioning)
                {
                    StartCoroutine(CompleteWaveAndStartNextRoutine());
                }
            }
        }
        else // Standard mode
        {
            enemiesDead = 0;
            foreach (var body in enemies)
            {
                if (body != null && body.dead) enemiesDead++;
            }

            if (enemiesDead == 4)
            {
                EndGame(true);
            }
        }
    }

    private IEnumerator ReplaceEnemyAfterDelay(int spawnIndex)
    {
        // Wait for the dying enemy's fade out animation before running the new enemy in
        yield return new WaitForSeconds(enemyFadeOutDuration);

        if (!isBattleActive)
        {
            pendingReplacementsCount = Mathf.Max(0, pendingReplacementsCount - 1);
            yield break;
        }

        GameObject newPrefab = GetNextEnemyForCurrentWave();
        if (newPrefab != null)
        {
            // Newly generated enemy runs up very quickly in its place
            yield return StartCoroutine(SpawnAndRunInRoutine(newPrefab, spawnIndex, replacementRunDuration));
        }

        pendingReplacementsCount = Mathf.Max(0, pendingReplacementsCount - 1);
    }

    private IEnumerator CompleteWaveAndStartNextRoutine()
    {
        isWaveTransitioning = true;

        // Wait for the final enemy's fade-out to complete
        yield return new WaitForSeconds(enemyFadeOutDuration);

        if (!isBattleActive)
        {
            isWaveTransitioning = false;
            yield break;
        }

        // +1000 Wave Completion Bonus Score (Gald is 1/4 of score)
        AddScore(scorePerWaveBonus);

        blurbEvent.Set($"Wave {currentWave} Complete! +{scorePerWaveBonus} Score!");
        EventBus.Publish(blurbEvent);
        if (usedMove1 != null) usedMove1.text = $"Wave {currentWave} Complete!";
        if (usedMoveGO != null) usedMoveGO.SetActive(true);

        // Hook for every set of 5 waves (Ad for special items - placeholder for future implementation)
        if (currentWave % 5 == 0)
        {
            OnFiveWaveMilestoneReached(currentWave);
        }

        // Configurable 1-second break between waves
        yield return new WaitForSeconds(waveBreakDuration);

        if (!isBattleActive)
        {
            isWaveTransitioning = false;
            yield break;
        }

        // Advance to next wave and reset wave counters
        currentWave++;
        enemiesSpawnedInCurrentWave = 0;
        enemiesKilledInCurrentWave = 0;
        UpdateEndlessScoreUI();

        blurbEvent.Set($"Wave {currentWave} Starting!");
        EventBus.Publish(blurbEvent);
        if (usedMove1 != null) usedMove1.text = $"Wave {currentWave}";

        // Spawn the initial 4 enemies of the new wave with their running animation
        SpawnInitialWaveEnemies(instant: false);
        yield return new WaitForSeconds(waveSpawnRunDuration);

        isWaveTransitioning = false;
    }

    /// <summary>
    /// Placeholder hook called after every 5th wave (Wave 5, 10, 15, etc.) for special ad items.
    /// </summary>
    protected virtual void OnFiveWaveMilestoneReached(int completedWave)
    {
        Debug.Log($"[Endless Mode] Completed Wave {completedWave} (5-Wave Set). Special item ad option can be triggered here later.");
    }

    private IEnumerator FadeOut(GameObject enemy)
    {
        if (enemy == null) yield break;
        float duration = enemyFadeOutDuration;
        float time = 0f;
        Vector3 initialScale = enemy.transform.localScale;

        while (time < duration)
        {
            if (enemy == null) yield break;
            enemy.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (enemy != null) Destroy(enemy);
    }

    public void EndGame(bool playerWon)
    {
        if (isEndingGame) return;
        isEndingGame = true;

        isBattleActive = false;
        StopAllCoroutines();
        
        if (attackOptionsParent != null) attackOptionsParent.SetActive(false);
        if (potionOptions != null) potionOptions.SetActive(false);
        if (itemOptions != null) itemOptions.SetActive(false);
        if (targetingHUDParent != null) targetingHUDParent.SetActive(false);

        Debug.Log(playerWon ? $"You won! Score: {currentScore}, Gald: {currentGald}" : $"Game Over. Final Score: {currentScore}, Gald: {currentGald}");
        StartCoroutine(DoEndGameRoutine(playerWon));
    }

    public void ReviveBoss()
    {
        if (PotionData.Instance != null)
        {
            PotionData.Instance.RestoreCheckpoint();
        }

#if USE_CRAZYGAMES
        CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, () => 
        {
        }, 
        (error) =>
        {
            Debug.LogWarning($"Rewarded ad failed or was cancelled: {error}");
        }, 
        () =>
        {
            PerformRevival();
            CrazySDK.Game.GameplayStart();
        });
#else
        PerformRevival();
#endif
    }

    private void PerformRevival()
    {
        hero.currentHealth = hero.maxHealth;
        hero.animator.SetTrigger("Revive");
        revived = true;
        
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            Hero heroComp = boss.GetComponent<Hero>();
            if (heroComp != null)
            {
                if (heroComp.lerpCoroutine != null) StopCoroutine(heroComp.lerpCoroutine);
                heroComp.lerpCoroutine = StartCoroutine(heroComp.LerpYPosition(boss.transform.position.y, heroComp.originalY, .21f, .5f));
            }
        }
        isEndingGame = false;
        isBattleActive = true;
        ApplyEffectWithDelay(bideVFX, hero.transform, 0f, 2.0f, true, 0f, null, false);
        StartCoroutine(StartTurn());
        loseScreen.SetActive(false);
        MainUIParent.SetActive(true);
    }

    private IEnumerator DoEndGameRoutine(bool playerWon)
    {
        yield return new WaitForSeconds(1f);

        if (playerWon)
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusicOnce(MusicManager.Instance.jingle12);
            }

            yield return new WaitForSeconds(endGameDelay);
            
            winScreen.SetActive(true);
            MainUIParent.SetActive(false);
        }
        else
        {
            if (TransitionManager.Instance() != null)
            {
                TransitionManager.Instance().Transition(sequenceTransition, 1f);
            }

            yield return new WaitForSeconds(endGameDelay);

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusicOnce(MusicManager.Instance.gameOver);
            }

            loseScreen.SetActive(true);
            MainUIParent.SetActive(false);

            if (revived && reviveButton != null)
            {
                reviveButton.SetActive(false);
            }
        }
    }

    private IEnumerator ShowYouWin()
    {
        TransitionManager.Instance().Transition(sequenceTransition, 1f);
        yield return new WaitForSeconds(endGameDelay);
        PlayerPrefs.SetInt("EndlessModeUnlocked", 1);
        PlayerPrefs.Save();
        thankScreen.SetActive(true);
    }

    public void showWin()
    {
        StartCoroutine(ShowYouWin());
    }

    private void OnDestroy()
    {
        if (attackDropdown != null) attackDropdown.onValueChanged.RemoveListener(OnAttack1Selected);
        if (attackDropdown2 != null) attackDropdown2.onValueChanged.RemoveListener(OnAttack2Selected);

        if (attackDropdown != null)
        {
            EventTrigger trigger1 = attackDropdown.GetComponent<EventTrigger>();
            if (trigger1 != null) trigger1.triggers.Clear();
        }
        
        if (attackDropdown2 != null)
        {
            EventTrigger trigger2 = attackDropdown2.GetComponent<EventTrigger>();
            if (trigger2 != null) trigger2.triggers.Clear();
        }

        StopAllCoroutines();
    }
    public void OnBackFromItemsClicked()
    {
        // Shuts down the item menu and ensures the main battle options stay visible
        if (itemOptions != null) itemOptions.SetActive(false);
        if (attackOptionsParent != null) attackOptionsParent.SetActive(true);
        if (potionOptions != null) potionOptions.SetActive(true);
    }
}