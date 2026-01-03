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
using CrazyGames;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public enum GameMode
    {
        Standard,
        Endless
    }

    public GameMode currentGameMode = GameMode.Standard;
    public static TurnManager Instance { get; private set; }

    [Header("Enemies")]
    [SerializeField] private List<GameObject> easyEnemies;
    [SerializeField] private List<GameObject> mediumEnemies;
    [SerializeField] private List<GameObject> hardEnemies;
    
    private List<Enemy> enemies;
    public Hero hero;
    private string attackNamesText;

    private enum Difficulty { Easy, Medium, Hard }
    private Difficulty currentDifficulty = Difficulty.Easy;
    private int totalEnemiesKilled = 0;

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

    public bool isFirstMatch = false;

    [Tooltip("Time in seconds to wait before showing the win/lose screen.")]
    public float endGameDelay = 3.0f; 
    private bool isBattleActive = false; // Controls if effects/turns should continue

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        statusUpdateEvent = new CharacterStatusUpdateEvent();
        SetupAttackDropdowns(); 
    }

    private GameObject GetNextEnemyFromCurrentDifficulty(List<Enemy> upcoming = null)
    {
        List<GameObject> pool;
        switch (currentDifficulty)
        {
            case Difficulty.Medium: pool = mediumEnemies; break;
            case Difficulty.Hard: pool = hardEnemies; break;
            default: pool = easyEnemies; break;
        }

        var existingNames = aliveEnemies.Select(e => e.characterName).ToList();
        
        // Filter out enemies that are already alive
        List<GameObject> available = pool.Where(prefab => {
             Enemy meta = prefab.GetComponent<Enemy>();
             return meta != null && !existingNames.Contains(meta.characterName);
        }).ToList();

        if (available.Count == 0) return pool[Random.Range(0, pool.Count)];
        return available[Random.Range(0, available.Count)];
    }

    private GameObject GetNextEnemyFromCurrentDifficulty(Difficulty fallback)
    {
        currentDifficulty = fallback;
        return GetNextEnemyFromCurrentDifficulty();
    }

    public IEnumerator MoveBackToPosition(Transform target, Vector3 startPos, float duration)
    {
        float elapsed = 0f;
        Vector3 currentPos = target.position;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.position = Vector3.Lerp(currentPos, startPos, elapsed / duration);
            yield return null;
        }
        target.position = startPos;
    }

    public void StartBattle()
    {
        isBattleActive = true;
        enemies = new List<Enemy>();
        aliveEnemies = new List<Enemy>();

        if (currentGameMode == GameMode.Standard)
        {
            GameObject knightObj = SpawnPrefabAtPosition(knightPrefab, enemySpawns[0]);
            GameObject swordsmanObj = SpawnPrefabAtPosition(swordsmanPrefab, enemySpawns[1]);
            GameObject mageObj = SpawnPrefabAtPosition(magePrefab, enemySpawns[2]);
            GameObject archerObj = SpawnPrefabAtPosition(archerPrefab, enemySpawns[3]);

            enemies.Add(knightObj.GetComponent<Enemy>());
            enemies.Add(swordsmanObj.GetComponent<Enemy>());
            enemies.Add(mageObj.GetComponent<Enemy>());
            enemies.Add(archerObj.GetComponent<Enemy>());
        }
        else // Endless mode
        {
            List<GameObject> initialEnemies = GetUniqueRandomEnemiesFromPool(easyEnemies, 4);
            for (int i = 0; i < 4; i++)
            {
                GameObject enemyObj = SpawnPrefabAtPosition(initialEnemies[i], enemySpawns[i]);
                enemies.Add(enemyObj.GetComponent<Enemy>());
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].Init(popupPrefab);
            enemyHUDs[i].Init(enemies[i]);
            aliveEnemies.Add(enemies[i]);
        }

        foreach (var attack in attacksReset)
        {
            attack.upgradeLevel = 0;
            Debug.Log($"Attack: {attack.attackName}, Upgrade Level: {attack.upgradeLevel}");
        }

        targetingMode = false;
        targetingHUDParent.SetActive(false);

        GameObject bossObj = SpawnPrefabAtPosition(bossPrefab, bossSpawn);
        hero = bossObj.GetComponent<Hero>();
        hero.Init(popupPrefab);
        bossHUD.Init(hero);

        foreach (var obj in listOfObjectToDeactivateAtStartOfBattle)
        {
            obj.SetActive(false);
        }

        MainUIParent.SetActive(true);
        enemyAttacksUsed = new List<AttackSO>();
        blurbEvent = new GameplayBlurbEvent();
    }

    private void ApplyEffectWithDelay(GameObject effectPrefab, Transform target, float delay, float effectDuration, bool? xRotationEffect = null, float raiseAmount = 0f, float? yRotationOffset = null, bool? standUpright = null) 
    {
        StartCoroutine(DelayedEffectCoroutine(effectPrefab, target, delay, effectDuration, raiseAmount, xRotationEffect, yRotationOffset, standUpright));
    }

    private IEnumerator DelayedEffectCoroutine(GameObject effectPrefab, Transform target, float delay, float effectDuration, float raiseAmount, bool? xRotationEffect, float? yRotationOffset = null, bool? standUpright = null)
    {
        yield return new WaitForSeconds(delay);
        
        // Stop visuals if game is over
        if (!isBattleActive) yield break;

        Vector3 effectPosition = target.position;
        if (raiseAmount > 0f) effectPosition += new Vector3(0f, raiseAmount, 0f);

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
    {
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
        // Stop turn logic if game is over
        if (!isBattleActive) yield break;

        drop1Opened = false;
        drop2Opened = false;

        yield return new WaitForSeconds(1.5f);

        if (!isBattleActive) yield break;

        foreach (var enemy in enemies)
        {
            if (!isBattleActive) break;

            usedMoveGO.SetActive(true);
            if (enemy.burning > 0)
            {
                enemy.Init(popupPrefabfire);
                enemy.TakeDamage((int)(enemy.maxHealth*.1));
            
                ApplyEffectWithDelay(burn, enemy.transform, 0f, 3.0f);
                if (enemy.currentHealth <= 0)
                {
                    enemy.currentHealth = 0; 
                    RemoveEnemy(enemy); 
                }
                else
                {
                    blurbEvent.Set($" {enemy.characterName} is burning for {enemy.burning} turns!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{enemy.characterName} is burning for {enemy.burning} turns!";
                }
            }
        }

        foreach (var enemy in enemies)
        {
            enemy.UpdateEffects();
        }
    
        if(hero.currentHealth <= 0) 
        {
            EndGame(false);
            yield return null;
        }
            
        // Only continue if the game is still running
        if(isBattleActive) StartCoroutine(DoTurnRoutine());
    }

    private IEnumerator DoTurnRoutine()
    {
        if (!isBattleActive) yield break;

        yield return new WaitForSeconds(1f);

        if (!isBattleActive) yield break;

        enemyAttacksByIndex.Clear();
        while (enemyAttacksByIndexPerm.Count < enemies.Count)
        {
            enemyAttacksByIndexPerm.Add(null);
        }

        
        if (!isBattleActive) yield break;

        // Enemies attack hero
        for (int i = 0; i < enemies.Count; i++)
        {
            if (!isBattleActive) break;

            Enemy enemy = enemies[i];
            enemy.Init(popupPrefab);
            hero.Init(popupPrefab);

            if (!enemy.CanAct() && !enemy.dead)
            {
                AttackSO skippedAttack = enemyAttacksByIndexPerm[i];
                if(skippedAttack != null && !enemyAttacksByIndex.Contains(skippedAttack))
                {
                     enemyAttacksByIndex.Add(skippedAttack);
                }

                 // --- ADDED: Individual paralysis text and delay ---
                blurbEvent.Set($"{enemy.characterName} is paralyzed and cannot act this turn!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"{enemy.characterName} is paralyzed and cannot act this turn!";
                usedMoveGO.SetActive(true);              
                ApplyEffectWithDelay(paralysisVFX, enemy.transform, 0f, 3.0f);
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            else if (enemy.dead)
            {
                AttackSO lastUsedAttack = enemyAttacksByIndexPerm[i];
                if (lastUsedAttack != null && !enemyAttacksByIndex.Contains(lastUsedAttack))
                {
                    enemyAttacksByIndex.Add(lastUsedAttack);
                }
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

                if (!enemyAttacksByIndex.Contains(enemyAttack)) enemyAttacksByIndex.Add(enemyAttack);
                if (!previousTurnMoves.Contains(enemyAttack)) previousTurnMoves.Add(enemyAttack);
                            
                if (hero.bideUses == 2) 
                {
                    enemyAttacksByIndex.Clear();
                    enemyAttacksByIndex.AddRange(previousTurnMoves);
                }

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
                        enemyHeal.Init(popupPrefabgreen);
                        if(enemyHeal.currentHealth <= 0) continue;
                        
                        enemyHeal.HealDamage((int)(enemy.maxHealth*0.2));
                        if(enemyHeal.currentHealth >= enemyHeal.maxHealth) enemyHeal.currentHealth = enemyHeal.maxHealth;

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
                    var enemyHeal = enemies.Where(e => e.currentHealth > 0).OrderBy(e => e.currentHealth).FirstOrDefault(); 
                    if (enemyHeal != null)
                    {
                        enemyHeal.Init(popupPrefabgreen);
                        enemyHeal.HealDamage((int)(enemy.maxHealth*0.2));
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
                    ApplyEffectWithDelay(iceAttack, enemy.transform, 0f, 3.0f);
                    ApplyEffectWithDelay(iceHit, hero.transform, .5f, 3.0f);
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
                        hero.StartCoroutine(hero.CallDoHitRoutine(enemyAttack.GetDamage()/10, .1f, hero.gameObject));
                        yield return new WaitForSeconds(.1f);
                    }
                    hero.StartCoroutine(MoveBackToPosition(hero.transform, originalPos, 0.2f));
                    yield return new WaitForSeconds(.5f);
                }

                if (enemyAttack.attributes.Contains("Triple"))
                {
                    hero.TakeDamage(enemyAttack.GetDamage());
                    hero.TakeDamage(enemyAttack.GetDamage());
                    
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                    {
                        hero.ApplyParalysis(5, true);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        EventBus.Publish(blurbEvent);
                        usedMove1.text =$"Boss has been paralyzed by {enemyAttack.attackName}!";
                    }
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                    {
                        hero.ApplyParalysis(5, true);
                        blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                        usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                        EventBus.Publish(blurbEvent);
                    }
                    if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
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
                    if (UnityEngine.Random.value <= .3f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                    {
                        hero.ApplyBurn(10, 3);
                        blurbEvent.Set($"Boss has been burned by {enemyAttack.attackName}!");
                        EventBus.Publish(blurbEvent);
                        usedMove1.text =$"Boss has been burned by {enemyAttack.attackName}!";
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
                    if (UnityEngine.Random.value <= 1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
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
                    ApplyEffectWithDelay(arrowAttack1, enemy.transform, 0f, 3.0f, null, 1.5f, yRotationOffset);
                    ApplyEffectWithDelay(arrowHit, hero.transform, .5f, 3.0f);
                }

                yield return new WaitForSeconds(1.5f);

                if(hero.currentHealth <= 0) yield break;
            }
        }

        yield return new WaitForSeconds(.5f);
        if (!isBattleActive) yield break;

        hero.bideBuff = false;
        hero.UpdateEffects();

        if (hero.burning > 0)
        { 
            hero.Init(popupPrefabfire);
            hero.TakeDamage((int)(hero.maxHealth*.1));
            
            ApplyEffectWithDelay(burn, hero.transform, 0f, 3.0f);
            if (hero.currentHealth <= 0)
            {
                EndGame(false);
                yield return null;
            }
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
            if(isBattleActive) StartCoroutine(StartTurn());
        }
        else 
        {
            if (bideAttribute > 0) bideAttribute--;
            if (bideAttribute == 0)
            {
                hero.bideLevel = 1.0f;
                hero.bideUses = 0;
            }
            usedMoveGO.SetActive(false);
            if(isBattleActive) ShowAttackSelectionUI();
        }
    }

    private void SetupAttackDropdowns()
    {
        attackDropdown.onValueChanged.AddListener(OnAttack1Selected);
        attackDropdown2.onValueChanged.AddListener(OnAttack2Selected);
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
        List<string> descriptions = new List<string>();

        for (int i = 0; i < enemyAttacksByIndex.Count; i++)
        {
            var attack = enemyAttacksByIndex[i];
            bool found = false;
            
            if (!found)
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(attack.attackName);
                dropdownOptions.Add(newOption);
                descriptions.Add(attack.attackDescription);
            }

            foreach (var existingOption in dropdownOptions)
            {
                int index = dropdownOptions.IndexOf(existingOption);
                if (index <= 3)
                {
                    var enemy = enemies[index]; 
                    if (!enemy.dead && existingOption.text == attack.attackName && bideAttribute >= 1)
                    {
                        attack.upgradeLevel++;
                        if (attack.upgradeLevel > 2) attack.upgradeLevel = 2;
                        found = true;
                        break;
                    }
                }
            }
        }

        attackDropdown.ClearOptions();
        attackDropdown2.ClearOptions();
        attackDropdown.AddOptions(dropdownOptions);
        attackDropdown2.AddOptions(dropdownOptions);

        AddHoverEvents(attackDropdown, descriptions);
        AddHoverEvents(attackDropdown2, descriptions);

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
        panaceaAmountText.text = PotionData.Instance.Panacea.ToString();;
        potionAmountText.text = PotionData.Instance.Potion.ToString();;
    }

    private void AddHoverEvents(TMP_Dropdown dropdown, List<string> descriptions)
    {
        Transform dropdownContent = dropdown.transform.Find("Dropdown List/Viewport/Content");
        if (dropdownContent == null) return;

        var optionObjects = dropdownContent.GetComponentsInChildren<Transform>(true);
        int optionIndex = 0;

        foreach (Transform option in optionObjects)
        {
            if (option.gameObject.name.Contains("Label") || option.gameObject.name.Contains("Background") || option.gameObject.name.Contains("Spacer"))
                continue;

            if (optionIndex < descriptions.Count)
            {
                string description = descriptions[optionIndex];
                EventTrigger trigger = option.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = option.gameObject.AddComponent<EventTrigger>();
                
                trigger.triggers.Clear();

                EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                pointerEnterEntry.callback.AddListener((_) => ShowDescription(description));
                trigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExitEntry.callback.AddListener((_) => HideDescription());
                trigger.triggers.Add(pointerExitEntry);
            }
            optionIndex++;
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
        usedMove1.text = ($"Choose a target");

        selectedAttack1 = null;
        selectedAttack2 = null;

        attackOptionsParent.SetActive(false);
        attackOptionsMenu.SetActive(false);
        potionOptions.SetActive(false);
        targetingHUDParent.SetActive(true);
        
        targetingMode = true;
    }

    public void OnItemOptionsClicked()
    {
        itemOptions.SetActive(true);
        panaceaAmountText.text = PotionData.Instance.Panacea.ToString();
        potionAmountText.text = PotionData.Instance.Potion.ToString();
    }

    // --- RESTORED: OnBideButtonClicked ---
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

    // --- RESTORED: OnPotionClicked ---
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
                hero.bideLevel = 1.0f;
                hero.bideUses = 0;
            }
            usedMoveGO.SetActive(true);
            blurbEvent.Set($"Potion Used: Health Restored!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = "Potion Used";
            PotionData.Instance.Potion--;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);
            
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

    // --- RESTORED: OnPanaceaClicked ---
    public void OnPanaceaClicked()
    {
        if (!isBattleActive) return;

        if (PotionData.Instance.Panacea > 0)
        {
            // Check if hero has any status effects to remove (burning or paralysis)
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
                hero.bideLevel = 1.0f;
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
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);

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
        if (!targetingMode) return;

        int maxTargets = GetMaxTargetsForAttack(combinedAttack.attributes);
        int aliveEnemies = enemies.Count(e => !e.dead);
        int targets = System.Math.Min(maxTargets, aliveEnemies);

        if (targets == 1 || !IsMultiTargetAttack(combinedAttack.attributes))
        {
            Debug.Log("ATTACKING SINGLE TARGET");
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
            Debug.Log("ATTACKING MULTI-TARGET");
            usedMove1.text = ($"Choose another target");

            if (!selectingEnemies)
            {
                selectedEnemies.Clear();
                selectingEnemies = true;
                blurbEvent.Set($"Select up to {targets} different enemies to attack!");
                EventBus.Publish(blurbEvent);
            }

            if (selectedEnemies.Contains(enemy))
            {
                blurbEvent.Set("Enemy already selected! Choose a different enemy.");
                EventBus.Publish(blurbEvent);
                return;
            }

            selectedEnemies.Add(enemy);
            blurbEvent.Set($"Selected: {enemy.characterName}");
            EventBus.Publish(blurbEvent);

            if (selectedEnemies.Count == targets)
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
        Debug.Log("DoBossAttackRoutine started.");

        if (!isBattleActive) yield break;

        // Group Attacked Text
        List<string> allTargetNames = new List<string>();
        foreach (var t in targetEnemies)
        {
            if(t != null) allTargetNames.Add(t.characterName);
        }

        if (allTargetNames.Count > 0)
        {
            attackNamesText = "";
            if (allTargetNames.Count == 1)
            {
                attackNamesText = allTargetNames[0];
            }
            else
            {
                attackNamesText = string.Join(", ", allTargetNames.Take(allTargetNames.Count - 1)) + " and " + allTargetNames.Last();
            }

            string fullAttackMsg = $"Boss attacked {attackNamesText} with {combinedAttack.attackName}";
            blurbEvent.Set(fullAttackMsg);
            EventBus.Publish(blurbEvent);
            usedMove1.text = fullAttackMsg;
        }

        List<string> paralyzedNames = new List<string>();
        List<string> defeatedNames = new List<string>(); 

        foreach (var targetEnemy in targetEnemies)
        {
            if (!isBattleActive) yield break;

            // Reset Popup Color
            targetEnemy.Init(popupPrefab); 

            yield return new WaitForSeconds(0f);

            if (!heroAniPlayed)
            {
                hero.DoAttackAnimation();
                heroAniPlayed = true;
            }

            int damage;
            if (bideAttribute <= 0)
            {
                damage = hero.GetDamage();
            }
            else
            {
                float multiplier = 1.0f + (combinedAttack.upgradeLevel / 30f);
                damage = Mathf.RoundToInt(hero.GetDamage() * multiplier);
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

            if (combinedAttack.attributes.Contains("Burn"))
            {
                hero.Init(popupPrefab);
                randomChance = (bideAttribute > 0) ? 0.2f : 0.2f;

                if (UnityEngine.Random.value <= randomChance && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
                {
                    targetEnemy.ApplyBurn(1000, 3);
                    blurbEvent.Set($"{targetEnemy.characterName} was burned!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{targetEnemy.characterName} was burned!";
                }

                float yRotationOffset = 0f;
                switch (selectedEnemyNum)
                {
                    case 0: yRotationOffset = 30f; break;
                    case 1: yRotationOffset = 5f; break;
                    case 2: yRotationOffset = 0f; break;
                    case 3: yRotationOffset = -25f; break;
                }

                Vector3 newPosition = hero.transform.position;
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
                Quaternion customRot = Quaternion.Euler(0, yRotationOffset, 0);
                ApplyEffectWithDelay(fireAttack, tempGameObject.transform, 0f, 3.0f, true, 1.5f, yRotationOffset);
                ApplyEffectWithDelay(fireHit, targetEnemy.transform, 0.5f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Paralysis"))
            {
                hero.Init(popupPrefab);

                if (UnityEngine.Random.value <= 1f && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, true);
                    if(!paralyzedNames.Contains(targetEnemy.characterName))
                    {
                        paralyzedNames.Add(targetEnemy.characterName);
                    }
                }

                float yRotationOffset = 0f;
                Vector3 newPosition = hero.transform.position;
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
                switch (selectedEnemyNum)
                {
                    case 0: yRotationOffset = 115f; break;
                    case 1: yRotationOffset = 95f; break;
                    case 2: yRotationOffset = 75f; break;
                    case 3: yRotationOffset = 60f; break;
                }
                Destroy(tempGameObject, 5f);
                ApplyEffectWithDelay(arrowAttack, tempGameObject.transform, 0f, 3.0f, true, 1.5f, yRotationOffset);
                ApplyEffectWithDelay(arrowHit, targetEnemy.transform, .5f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Heal"))
            {
                hero.Init(popupPrefabgreen);
                hero.HealDamage(damage);
                blurbEvent.Set($"{damage} Health Recovered!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"Boss attacked {attackNamesText} with {combinedAttack.attackName} and recovered {damage} Health!";
                ApplyEffectWithDelay(heal, hero.transform, 0f, 3.0f);
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
                ApplyEffectWithDelay(healfield, hero.transform, 0f, 3.0f, true, 0.10f, null, true);
                ApplyEffectWithDelay(panaceaVFX, hero.transform, 0f, 3.0f);
            }

            if (targetEnemy.currentHealth <= 0)
            {
                if(!defeatedNames.Contains(targetEnemy.characterName))
                {
                    defeatedNames.Add(targetEnemy.characterName);
                }
                RemoveEnemy(targetEnemy);
            }

            if (combinedAttack.attributes.Contains("Slash"))
            {
                hero.Init(popupPrefab);
                Vector3 newPosition = targetEnemy.transform.position;
                newPosition.y += 3f;
                Transform tempTransform = new GameObject().transform;
                tempTransform.position = newPosition;

                ApplyEffectWithDelay(slashAttack, hero.transform, 0f, 3.0f, null, 1.5f); 
                ApplyEffectWithDelay(slashHit, tempTransform, .5f, 3.0f);
                ApplyEffectWithDelay(slashCrater, targetEnemy.transform, .8f, 3.0f);
            }

            if (combinedAttack.attributes.Contains("Triple"))
            {
                hero.Init(popupPrefab);
                targetEnemy.TakeDamage(damage);
                targetEnemy.TakeDamage(damage);

                if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if(!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }
                if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if(!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }
                if (UnityEngine.Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, false);
                    if(!paralyzedNames.Contains(targetEnemy.characterName)) paralyzedNames.Add(targetEnemy.characterName);
                }

                float yRotationOffset = 0f;
                switch (selectedEnemyNum)
                {
                    case 0: yRotationOffset = 115f; break;
                    case 1: yRotationOffset = 110f; break;
                    case 2: yRotationOffset = 90f; break;
                    case 3: yRotationOffset = 70f; break;
                }

                Vector3 newPosition = hero.transform.position;
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
                ApplyEffectWithDelay(tripleAttack, hero.transform, 0f, 3.0f, null, 0f, yRotationOffset, null);
                ApplyEffectWithDelay(tripleHit, targetEnemy.transform, .5f, 3.0f, null, 1.5f);
            }

            if (combinedAttack.attributes.Contains("Ice") && !hasIced)
            {
                hero.Init(popupPrefab);
                ApplyEffectWithDelay(iceAttack, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(iceHit, targetEnemy.transform, .5f, 3.0f);
                blurbEvent.Set($"You strike again");
                EventBus.Publish(blurbEvent);
            }

            if (combinedAttack.attributes.Contains("Lunge"))
            {
                hero.Init(popupPrefab);
                blurbEvent.Set($"You strike twice");
                EventBus.Publish(blurbEvent);
                ApplyEffectWithDelay(lungeAttack, hero.transform, 0f, 3.0f, true, 0f);
                ApplyEffectWithDelay(lungeHit, targetEnemy.transform, .5f, 3.0f);
            }

            heroCritRate = 0.05f;
            hasIced = false;
            hasLunged = false;
        } 

        // Grouped Text Display
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
            yield return new WaitForSeconds(1.5f); 
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

        heroAniPlayed = false;
        
        // CHECK IF BATTLE IS ACTIVE BEFORE CONTINUING
        if(isBattleActive) StartCoroutine(StartTurn());
    }

    public void RemoveEnemy(Enemy enemy)
    {
        enemy.dead = true;
        enemy.RemoveParalysis();
        enemy.RemoveBurns();

        if (currentGameMode == GameMode.Endless)
        {
            totalEnemiesKilled++;
            if (totalEnemiesKilled % 4 == 0) IncreaseDifficulty();

            int spawnIndex = enemies.IndexOf(enemy);
            if (spawnIndex < 0) return;

            enemies[spawnIndex] = null;
            aliveEnemies.Remove(enemy);
            StartCoroutine(ReplaceEnemyAfterDelay(enemy, spawnIndex));
        }
        else // Standard mode
        {
            enemiesDead = 0;
            foreach (var body in enemies)
            {
                if (body.dead) enemiesDead++;
            }

            if (enemiesDead == 4)
            {
                EndGame(true);
            }
        }
    }

    private IEnumerator ReplaceEnemyAfterDelay(Enemy deadEnemy, int spawnIndex)
    {
        var spawnPoint = enemySpawns[spawnIndex];
        StartCoroutine(FadeOut(deadEnemy.gameObject));
        Destroy(deadEnemy.gameObject, 1f);

        yield return new WaitForSeconds(1f);

        if (!isBattleActive) yield break;

        var newPrefab = GetNextEnemyFromCurrentDifficulty(upcomingEnemies);
        if (newPrefab == null) yield break;

        GameObject newObj = SpawnPrefabAtPosition(newPrefab, spawnPoint);
        var newEnemy = newObj.GetComponent<Enemy>();
        newEnemy.Init(popupPrefab);

        enemies[spawnIndex] = newEnemy;
        aliveEnemies.Add(newEnemy);
        enemyHUDs[spawnIndex].Init(newEnemy);
    }

    private void IncreaseDifficulty()
    {
        if (currentDifficulty == Difficulty.Easy)
        {
            currentDifficulty = Difficulty.Medium;
            ReplaceEasyEnemiesWith(mediumEnemies);
            Debug.Log("Difficulty increased to Medium!");
        }
        else if (currentDifficulty == Difficulty.Medium)
        {
            currentDifficulty = Difficulty.Hard;
            ReplaceEasyEnemiesWith(hardEnemies);
            Debug.Log("Difficulty increased to Hard!");
        }
    }

    private void ReplaceEasyEnemiesWith(List<GameObject> newList)
    {
        easyEnemies = new List<GameObject>(newList);
    }

    private List<GameObject> GetUniqueRandomEnemiesFromPool(List<GameObject> pool, int count)
    {
        List<GameObject> shuffled = new List<GameObject>(pool);
        for (int i = 0; i < shuffled.Count; i++)
        {
            GameObject temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        return shuffled.Take(count).ToList();
    }
    
    // --- RESTORED Wrapper for compatibility ---
    private GameObject GetRandomEnemyPrefab(Enemy deadEnemy)
    {
        return GetNextEnemyFromCurrentDifficulty();
    }

    private int GetDeadEnemySpawnIndex(Enemy deadEnemy)
    {
        return enemies.IndexOf(deadEnemy); 
    }

    private IEnumerator FadeOut(GameObject enemy)
    {
        yield return new WaitForSeconds(.5f);
        float duration = 1f;
        float time = 0f;
        Vector3 initialScale = enemy.transform.localScale;

        while (time < duration)
        {
            enemy.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(enemy);
    }

    public void EndGame(bool playerWon)
    {
        isBattleActive = false; // Stop all ongoing effects/logic immediately
        Debug.Log(playerWon ? "You won!" : "Game Over.");
        StartCoroutine(DoEndGameRoutine(playerWon));
    }

    public void ReviveBoss()
    {
        CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, () => 
        {
            // ad started
        }, (error) =>
        {
            // ad error
        }, () =>
        {
             hero.currentHealth = hero.maxHealth;
             hero.animator.SetTrigger("Revive");
             revived = true;
             GameObject boss = GameObject.FindGameObjectWithTag("Boss");
             if (boss != null)
             {
                 Hero hero = boss.GetComponent<Hero>();
                 if (hero != null)
                 {
                     if (hero.lerpCoroutine != null) StopCoroutine(hero.lerpCoroutine);
                     hero.lerpCoroutine = StartCoroutine(hero.LerpYPosition(boss.transform.position.y, hero.originalY, .21f, .5f));
                 }
             }

             isBattleActive = true; // Restart logic

             ApplyEffectWithDelay(bideVFX, hero.transform, 0f, 2.0f,true,0f, null, false);
             StartCoroutine(StartTurn());
             loseScreen.SetActive(false);
             MainUIParent.SetActive(true);
             CrazySDK.Game.GameplayStart();
        });
    }
    

    private IEnumerator DoEndGameRoutine(bool playerWon)
    {
        yield return new WaitForSeconds(endGameDelay);
        MusicManager.Instance.ChangeSong(6);


        if (playerWon)
        {
            if (SceneManager.GetActiveScene().name == "LVL5")
            {
                //Todo Make a congrats you win scene?
            }
            else
                winScreen.SetActive(true);
            MainUIParent.SetActive(false);
        }
        else
        {
            loseScreen.SetActive(true);
            MainUIParent.SetActive(false);
            if (revived)
            {
                reviveButton.SetActive(false);
            }
        }
    }
}