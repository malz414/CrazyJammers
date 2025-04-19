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

    [SerializeField] private List<GameObject> easyEnemies;
    [SerializeField] private List<GameObject> mediumEnemies;
    [SerializeField] private List<GameObject> hardEnemies;
    private int easyIndex = 0;
    private int mediumIndex = 0;
    private int hardIndex = 0;
    private enum Difficulty { Easy, Medium, Hard }
    private Difficulty currentDifficulty = Difficulty.Easy;
    private int totalEnemiesKilled = 0;
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;
    


    [SerializeField] private GameObject SteadyAttack;
    [SerializeField] private GameObject SteadyHit;

    [SerializeField] private GameObject lungeAttack;
    [SerializeField] private GameObject lungeHit;
    
    private List<Enemy> upcomingEnemies = new List<Enemy>();


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

    [SerializeField] private GameObject para;

    [SerializeField] private GameObject burn;

    [SerializeField] private GameObject bideani;

    [SerializeField] private GameObject potionAni;

    [SerializeField] private GameObject panaceaAni;

    public CombinedAttackSO combinedAttack;

    [SerializeField] private Transform[] enemySpawns;

    [SerializeField] private GameObject bossPrefab;

    [SerializeField] private Transform bossSpawn;

    [SerializeField] GameObject MainUIParent;

    [SerializeField] GameObject attackOptionsParent;
    [SerializeField] GameObject attackOptionsMenu;
    [SerializeField] private TMP_Dropdown attackDropdown;
    [SerializeField] private TMP_Dropdown attackDropdown2;

    [SerializeField] GameObject potionOptions;

    [SerializeField] GameObject targetingHUDParent;
  

        
    public List<Enemy> aliveEnemies = new List<Enemy>();
    private List<Enemy> deadEnemies = new List<Enemy>();
    [SerializeField] GameObject winScreen;

    [SerializeField] GameObject loseScreen;

    [SerializeField] CharacterHUD[] enemyHUDs;

    [SerializeField] CharacterHUD bossHUD;

    [SerializeField] private DamageNumber popupPrefabNeutral;
    [SerializeField] private DamageNumber popupPrefab;
     [SerializeField] private DamageNumber popupPrefabfire;
     [SerializeField] private DamageNumber popupPrefabgreen;

    public List<AttackSO> enemyAttacksByIndex = new List<AttackSO>();
    public List<AttackSO> enemyAttacksByIndexPerm = new List<AttackSO> { null, null, null, null };
    
    public List<AttackSO> previousTurnMoves;
    private List<Enemy> selectedEnemies = new List<Enemy>();
    private int selectedEnemyNum = -1;
    private bool selectingEnemies = false;

    private Hero hero;
    public List<Enemy> enemies;

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

    private int extraAttacks = 0;
    private bool attackExtra = false;

    public bool TargetingMode => targetingMode;

    private bool targetingMode = false;
    public float critMultiplier = 1.2f;

    public float heroCritRate = .05f;

    private bool bideBuff = false;
    public int bideAttribute = 0;
    private float randomChance = 0f;


    public int enemiesDead = 0;


    [SerializeField] private TextMeshProUGUI PotAmount; 
    [SerializeField] private TextMeshProUGUI PanAmount; 
    [SerializeField] private TextMeshProUGUI descriptionText;
     [SerializeField] private TextMeshProUGUI descriptionTextPotion;
    [SerializeField] private TextMeshProUGUI usedMove;
    [SerializeField] private TextMeshProUGUI usedMove1;
    [SerializeField] private GameObject usedMoveGO;
    public TextMeshProUGUI Popop; 

    private CharacterStatusUpdateEvent statusUpdateEvent;

    
    public AttackSO selectedAttack1;
    public AttackSO selectedAttack2;

    public bool drop1Opened;
    public bool drop2Opened;
    


    [SerializeField] GameObject[] listOfObjectToDeactivateAtStartOfBattle;

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
            case Difficulty.Medium:
                pool = mediumEnemies;
                break;
            case Difficulty.Hard:
                pool = hardEnemies;
                break;
            default:
                pool = easyEnemies;
                break;
        }

        var existingNames = aliveEnemies.Select(e => e.characterName).ToList();
        if (upcoming != null)
            existingNames.AddRange(upcoming.Select(e => e.characterName));

        var available = pool.Where(prefab =>
        {
            var meta = prefab.GetComponent<Enemy>();
            return meta != null && !existingNames.Contains(meta.characterName);
        }).ToList();

        if (available.Count == 0)
            return pool[Random.Range(0, pool.Count)];

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
    enemies = new List<Enemy>();
    aliveEnemies = new List<Enemy>();

    if (currentGameMode == GameMode.Standard)
    {
        // Standard mode: fixed enemy spawns
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

    // Reset upgrades
    foreach (var attack in attacksReset)
    {
        attack.upgradeLevel = 0;
        Debug.Log($"Attack: {attack.attackName}, Upgrade Level: {attack.upgradeLevel}");
    }

    // Boss setup
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


    //VFX called with delay for some so attacks go off then theres a delay on the hit more time is given to the duration so with delay + duration it doesnt  cancel early 

private void ApplyEffectWithDelay(GameObject effectPrefab, Transform target, float delay, float effectDuration, bool? xRotationEffect = null, bool raiseEffect = false, float? yRotationOffset = null) 
{
    StartCoroutine(DelayedEffectCoroutine(effectPrefab, target, delay, effectDuration, raiseEffect, xRotationEffect, yRotationOffset));
}

private IEnumerator DelayedEffectCoroutine(GameObject effectPrefab, Transform target, float delay, float effectDuration, bool raiseEffect, bool? xRotationEffect, float? yRotationOffset = null)
{
    yield return new WaitForSeconds(delay);

    // Default to the target's normal position
    Vector3 effectPosition = target.position;

    // If raiseEffect is true, modify the Y-axis
    if (raiseEffect)
    {
        effectPosition += new Vector3(0f, 1.5f, 0f);
    }

      // Instantiate the effect at the appropriate position
    GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.identity); // neutral rotation

    effect.transform.SetParent(target);
    effect.transform.SetParent(null);
    
    if (yRotationOffset.HasValue)
    {       Vector3 direction = Quaternion.Euler(0f, yRotationOffset.Value, 0f) * Vector3.forward;
          effect.transform.rotation = Quaternion.LookRotation(direction);
            HS_ProjectileMover mover = effect.GetComponent<HS_ProjectileMover>();
        if (mover != null)
        {
            mover.moveDirection = direction;
        }
    }


    if (xRotationEffect.HasValue)
    {
        float xRotation = xRotationEffect.Value ? 90f : -90f;
        effect.transform.Rotate(0f, xRotation, 0f);
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
        if (prefab.name.Contains("Boss"))
        {
            spawned.tag = "Boss";
        }
        spawned.transform.position = pos.transform.position;
        spawned.transform.rotation = pos.transform.rotation;
        return spawned;
    }

    public IEnumerator StartTurn()
    {
        drop1Opened = false;
        drop2Opened = false;

       yield return new WaitForSeconds(1.5f);
        foreach (var enemy in enemies)
        {
            usedMoveGO.SetActive(true);
            if (enemy.burning > 0)
            {
                enemy.Init(popupPrefabfire);
                enemy.TakeDamage((int)(enemy.maxHealth*.1));
          
       
                ApplyEffectWithDelay(burn, enemy.transform, 0f, 3.0f);
                if (enemy.currentHealth <= 0)
                {
                    blurbEvent.Set($"{enemy.characterName} has been burned to death!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{enemy.characterName} has been burned to death!";
                    RemoveEnemy(enemy);
                }
                else
                {
                    enemy.burning--;
                    blurbEvent.Set($" {enemy.characterName} is burning for {enemy.burning} turns!");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"{enemy.characterName} is burning for {enemy.burning} turns!";

                }
                
          
            }
         
        }
        bideAttribute--;
        if (bideAttribute == 0)
        {
            hero.bideLevel = 1.0f;
            hero.bideUses = 0;
        }
        foreach (var enemy in enemies)
        {
            enemy.UpdateEffects();
        }

    
        //   if (hero.bideUses == 2) 
        // {
        //     List<AttackSO> previousTurnMoves = new List<AttackSO>(enemyAttacksByIndex);
        //     enemyAttacksByIndex.Clear();

        //     enemyAttacksByIndex.AddRange(previousTurnMoves);
        // }
        // else
        // {
        //     enemyAttacksByIndex.Clear();
        // }

   
        if(hero.currentHealth <= 0) 
        {
            EndGame(false);
            yield return null;
        }
         
    
        StartCoroutine(DoTurnRoutine());
    }






    private IEnumerator DoTurnRoutine()
    {
       
      
        
        yield return new WaitForSeconds(1f);
        enemyAttacksByIndex.Clear();
        
        while (enemyAttacksByIndexPerm.Count < enemies.Count)
        {
            enemyAttacksByIndexPerm.Add(null);
        }

        // Enemies attack hero
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            enemy.Init(popupPrefab);
            hero.Init(popupPrefab);
            if (!enemy.CanAct() && !enemy.dead)
            {
                usedMoveGO.SetActive(true);
                if(!enemyAttacksByIndex.Contains(enemyAttacksByIndexPerm[i]))
                 {
                     enemyAttacksByIndex.Add(enemyAttacksByIndexPerm[i]);
                 }
           
                blurbEvent.Set($"{enemy.characterName} is paralyzed and cannot act this turn!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"{enemy.characterName} is paralyzed and cannot act this turn!";
                ApplyEffectWithDelay(para, enemy.transform, 0f, 3.0f);
                  yield return new WaitForSeconds(1.5f);
                continue;
            }
            else if (enemy.dead)
            {
               if (!enemyAttacksByIndex.Contains(enemyAttacksByIndexPerm[i]))
                {
                    enemyAttacksByIndex.Add(enemyAttacksByIndexPerm[i]);
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

            if (!enemyAttacksByIndex.Contains(enemyAttack))
            {
                enemyAttacksByIndex.Add(enemyAttack);
            }

            if (!previousTurnMoves.Contains(enemyAttack))
            {
                previousTurnMoves.Add(enemyAttack);
            }
                
            

            if (hero.bideUses == 2) 
            {
                enemyAttacksByIndex.Clear();
                enemyAttacksByIndex.AddRange(previousTurnMoves);
            }

            


            if (enemyAttack.attributes.Contains("Barrier"))
            {
                // List<Enemy> alive = enemies.FindAll(enemy => enemy.currentHealth >0);
                // int randomRange = Random.Range(0, alive.Count);
                // Enemy enemyBarrier = alive[randomRange];
                Vector3 newPosition = enemy.transform.position;
                newPosition.y += 1f;

                // Create a temporary game object with the new position
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;

                // Destroy the temporary game object after 5 seconds
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
                    if(enemyHeal.currentHealth >= enemyHeal.maxHealth)
                    {
                        enemyHeal.currentHealth = enemyHeal.maxHealth;
                    }
                    enemyHeal.RemoveBurns();
                    enemyHeal.RemoveParalysis();
                    ApplyEffectWithDelay(healfield, enemyHeal.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(panaceaAni, enemyHeal.transform, 0f, 2.0f);
                    blurbEvent.Set($"The heroes healed and status cured.");
                    blurbEvent.Set($"The heroes gained a barrier.");
                     usedMove1.text = $"The heroes gained a barrier.";
                      usedMove1.text = $"The heroes healed and status cured.";
                    EventBus.Publish(blurbEvent);
                  
                    yield return new WaitForSeconds(.2f);

                }
                  yield return new WaitForSeconds(.5f);

            continue;
            }

        if (enemyAttack.attributes.Contains("Heal"))
        {
             var enemyHeal = enemies
            .Where(e => e.currentHealth > 0) 
            .OrderBy(e => e.currentHealth) 
            .FirstOrDefault(); 

        if (enemyHeal != null)
        {
           
            enemyHeal.Init(popupPrefabgreen);
            //enemyHeal.currentHealth += (int)(enemy.maxHealth*0.2);
            enemyHeal.HealDamage((int)(enemy.maxHealth*0.2));

           
            if (enemyHeal.currentHealth > enemyHeal.maxHealth)
            {
                enemyHeal.currentHealth = enemyHeal.maxHealth;
            }

      
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
                ApplyEffectWithDelay(lungeAttack, enemy.transform, 0f, 3.0f, false, false);
                ApplyEffectWithDelay(lungeHit, hero.transform, .5f, 3.0f);
            }

            if (enemyAttack.attributes.Contains("Slash"))
            {
                Vector3 newPosition = hero.transform.position;
                newPosition.y += 3f;

                // Create a temporary game object with the new position
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;

                // Destroy the temporary game object after 5 seconds
                Destroy(tempGameObject, 5f);

                ApplyEffectWithDelay(slashAttack, enemy.transform, 0f, 3.0f, null, true);
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
                   // cancels previous trigger if still active
                   
                    yield return new WaitForSeconds(.1f);
                }
                hero.StartCoroutine(MoveBackToPosition(hero.transform, originalPos, 0.2f));
                yield return new WaitForSeconds(.5f);
                
            }
              if (enemyAttack.attributes.Contains("Triple"))
            {
                hero.TakeDamage(enemyAttack.GetDamage());
                hero.TakeDamage(enemyAttack.GetDamage());
           
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyParalysis(5, false);
                    blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                    EventBus.Publish(blurbEvent);
                      usedMove1.text =$"Boss has been paralyzed by {enemyAttack.attackName}!";
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyParalysis(5, false);
                    blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                      usedMove1.text =$"Boss has been paralyzed by {enemyAttack.attackName}!";
                    EventBus.Publish(blurbEvent);
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyParalysis(5, false);
                    blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                       usedMove1.text =$"Boss has been paralyzed by {enemyAttack.attackName}!";
                    EventBus.Publish(blurbEvent);
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
                ApplyEffectWithDelay(tripleAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(tripleHit, hero.transform, .5f, 3.0f, null, true);
            }

            



            hero.TakeDamage(Mathf.RoundToInt(enemyAttack.GetDamage() * enemy.multiplier));
            
            blurbEvent.Set($"{enemy.characterName} used {enemyAttack.attackName}!");
            EventBus.Publish(blurbEvent);

            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, dealing {enemyAttack.GetDamage()} damage to the hero.");

            if (enemyAttack.attributes.Contains("Burn"))
            {

                if (Random.value <= .3f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyBurn(10, 3);
                    blurbEvent.Set($"Boss has been burned by {enemyAttack.attackName}!");
                    EventBus.Publish(blurbEvent);
                       usedMove1.text =$"Boss has been burned by {enemyAttack.attackName}!";
                    Debug.Log($"Hero has been burned by {enemyAttack.attackName}!");
                }
                ApplyEffectWithDelay(fireAttack, enemy.transform, 0f, 3.0f, false, true);
                ApplyEffectWithDelay(fireHit, hero.transform, .5f, 3.0f);
            }

            if (enemyAttack.attributes.Contains("Paralysis"))
            {
                
                Debug.Log("selectedEnemyNum is " + i + " PARA IS CALLED");
                if (Random.value <= 1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyParalysis(5, false);
                    blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                     EventBus.Publish(blurbEvent);
                     usedMove1.text = $"Boss has been paralyzed by {enemyAttack.attackName}!";
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
                float yRotationOffset = -90f;
                switch (i)
                {
                    case 0: yRotationOffset = -70f; break;
                    case 1: yRotationOffset = -90f; break;
                    case 2: yRotationOffset = -90f; break;
                    case 3: yRotationOffset = -110f; break;
                }
                
                 Debug.Log("selectedEnemyNum is" + i + "So rotation is " + yRotationOffset);
                Vector3 newPosition = enemy.transform.position;
                newPosition.y += 0f;

                    // Create a temporary game object with the new position
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
             
                // Apply visual effects using the temp object's transform
                Quaternion customRot = Quaternion.Euler(0, yRotationOffset, 0);
                    
                    
                    ApplyEffectWithDelay(arrowAttack1, enemy.transform, 0f, 3.0f, null, true, yRotationOffset);
                    ApplyEffectWithDelay(arrowHit, hero.transform, .5f, 3.0f);
                }




            yield return new WaitForSeconds(1.5f);

            if(hero.currentHealth <= 0)
            {
                yield break;
            }
            }
        }

        yield return new WaitForSeconds(.5f);
        hero.bideBuff = false;
            hero.UpdateEffects();
        if (hero.burning > 0)
        { 
            hero.Init(popupPrefabfire);
            hero.TakeDamage((int)(hero.maxHealth*.1));
          
            usedMoveGO.SetActive(true);
            ApplyEffectWithDelay(burn, hero.transform, 0f, 3.0f);
            blurbEvent.Set($" You're Burning for {hero.burning} turns!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = $" You're Burning for {hero.burning} turns!";
            hero.burning--;
            EventBus.Publish(statusUpdateEvent);
            yield return new WaitForSeconds(1f);
            if(hero.currentHealth <= 0) 
            {
                EndGame(false);
                yield return null;
            }

        }
       
        if (!hero.CanAct())
        {
            
            blurbEvent.Set($"Boss is paralyzed and cannot act this turn!");
            EventBus.Publish(blurbEvent);
            usedMove1.text = $"Boss is paralyzed and cannot act this turn!";
            ApplyEffectWithDelay(para, hero.transform, 0f, 3.0f);
            Debug.Log("Hero is paralyzed and cannot act this turn!");
            yield return new WaitForSeconds(1f);
            usedMove1.text = "";
            usedMoveGO.SetActive(false);
            StartCoroutine(StartTurn()); // Skip the hero's turn
        }
        else
        {
             usedMoveGO.SetActive(false);
            // Show attack selection UI for the hero
            ShowAttackSelectionUI();
        }

    }

private void SetupAttackDropdowns()
{
    // Add listeners to dropdowns for when selection changes
    attackDropdown.onValueChanged.AddListener(OnAttack1Selected);
    attackDropdown2.onValueChanged.AddListener(OnAttack2Selected);
}

private void OnAttack1Selected(int index)
{
    if (index < 0 || index >= enemyAttacksByIndex.Count) return;

    selectedAttack1 = enemyAttacksByIndex[index];
    Debug.Log($"First attack selected: {selectedAttack1.attackName}");
}

private void OnAttack2Selected(int index)
{
    if (index < 0 || index >= enemyAttacksByIndex.Count) return;

    selectedAttack2 = enemyAttacksByIndex[index];
    Debug.Log($"Second attack selected: {selectedAttack2.attackName}");
}


private void ShowAttackSelectionUI()
{
    // Prepare new options and descriptions for dropdowns
    List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
    List<string> descriptions = new List<string>();

    // Iterate through the list of enemy attacks
    for (int i = 0; i < enemyAttacksByIndex.Count; i++)
    {
        var attack = enemyAttacksByIndex[i];

        bool found = false;
           
          // If not found, add it to the dropdown options
        if (!found)
        {
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(attack.attackName);
            dropdownOptions.Add(newOption);
            descriptions.Add(attack.attackDescription);
        }
        

        // Check if the attack is already in the dropdown
        foreach (var existingOption in dropdownOptions)
        
         
        {

            int index = dropdownOptions.IndexOf(existingOption);
        if (index <= 3)
            {
            var enemy = enemies[index]; 
            if (!enemy.dead && existingOption.text == attack.attackName && bideAttribute >= 1)
            {
                // If found, increment the upgrade level
                Debug.Log("Attack reinforced: " + attack.attackName);
                attack.upgradeLevel++;
                
                // Cap the upgrade level at 2
                if (attack.upgradeLevel > 2)
                    attack.upgradeLevel = 2;

                Debug.Log("Attack level: " + attack.upgradeLevel);
                found = true;
                break;
            }
        }

      
    }
    }

    // Clear existing options and add updated ones
    attackDropdown.ClearOptions();
    attackDropdown2.ClearOptions();

    attackDropdown.AddOptions(dropdownOptions);
    attackDropdown2.AddOptions(dropdownOptions);

    // Add hover events for descriptions
    AddHoverEvents(attackDropdown, descriptions);
    AddHoverEvents(attackDropdown2, descriptions);

    // Set default values for dropdowns
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

    // Set UI elements active
    attackOptionsParent.SetActive(true);
    potionOptions.SetActive(true);

    // Update potion and panacea amounts
     PanAmount.text = PotionData.Instance.Potion.ToString();;
    PotAmount.text = PotionData.Instance.Panacea.ToString();;


}





        // Set up the Bide button
        /*        bideButton.gameObject.SetActive(true);
                bideButton.onClick.RemoveAllListeners();
                bideButton.onClick.AddListener(OnBideButtonClicked);
                bideButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bide";*/

private void AddHoverEvents(TMP_Dropdown dropdown, List<string> descriptions)
{
    // Access the dropdown template and its content
    Transform dropdownContent = dropdown.transform.Find("Dropdown List/Viewport/Content");

    if (dropdownContent == null)
    {
        Debug.LogError("Dropdown content not found. Ensure the template is set up correctly.");
        return;
    }

    // Get all option objects inside the dropdown content
    var optionObjects = dropdownContent.GetComponentsInChildren<Transform>(true);

    int optionIndex = 0;

    // Iterate over each option and add hover events
    foreach (Transform option in optionObjects)
    {
        // Skip non-option objects such as the label, background, or spacer
        if (option.gameObject.name.Contains("Label") || option.gameObject.name.Contains("Background") || option.gameObject.name.Contains("Spacer"))
            continue;

        // Ensure we're within the bounds of the descriptions
        if (optionIndex < descriptions.Count)
        {
            string description = descriptions[optionIndex];

            // Add EventTrigger to the current option GameObject
            EventTrigger trigger = option.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = option.gameObject.AddComponent<EventTrigger>();
            }

            // Clear any previous triggers to avoid duplicates
            trigger.triggers.Clear();

            // PointerEnter event to show the description
            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener((_) => ShowDescription(description));
            trigger.triggers.Add(pointerEnterEntry);

            // PointerExit event to hide the description
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener((_) => HideDescription());
            trigger.triggers.Add(pointerExitEntry);
        }

        optionIndex++;
    }
}



private void ShowDescription(string description)
{
    
    if (descriptionText != null)
    {
        descriptionText.text = description;
        Debug.Log("DESCIPBPOS");
    }
    else 
    {
        Debug.Log("NO DESCIPBPOS");
    }
    
}

private void HideDescription()
{
    descriptionText.text = "Select a move";

}

    public void OnConfirmButtonClicked()
{
     usedMoveGO.SetActive(true);
    // Ensure both attacks are selected
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

    // Combine the selected attacks
    hero.CombineAttacks(selectedAttack1, selectedAttack2);
     usedMove1.text = ($"Choose a target");

    // Reset selections after combining
    selectedAttack1 = null;
    selectedAttack2 = null;

    // Hide UI elements after combining
    attackOptionsParent.SetActive(false);
    attackOptionsMenu.SetActive(false);
    potionOptions.SetActive(false);
    targetingHUDParent.SetActive(true);
  

    targetingMode = true;
}


    public void OnItemOptionsClicked()
    {
        itemOptions.SetActive(true);
        PanAmount.text = PotionData.Instance.Panacea.ToString();
        PotAmount.text = PotionData.Instance.Potion.ToString();
        
    }


    public void SelectEnemyToAttack(Enemy enemy)
{
    if (!targetingMode) return;

    // Determine the number of targets based on the highest-priority attribute
    int maxTargets = GetMaxTargetsForAttack(combinedAttack.attributes);

    // Count alive enemies
    int aliveEnemies = enemies.Count(e => !e.dead);

    // Determine the actual number of targets based on alive enemies
    int targets = System.Math.Min(maxTargets, aliveEnemies);

    // Single-target attack if only 1 target is available or the attack is not multi-target
    if (targets == 1 || !IsMultiTargetAttack(combinedAttack.attributes))
    {
        Debug.Log("ATTACKING SINGLE TARGET");
        blurbEvent.Set("Attacking");
        EventBus.Publish(blurbEvent);

        // Stop any existing attack coroutine
        if (bossAttackCoroutine != null)
        {
            StopCoroutine(bossAttackCoroutine);
        }

        // Single-target attack
        targetingMode = false;
        targetingHUDParent.SetActive(false);
        Debug.Log($"Single-target attack on {enemy.characterName}");
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
            // Start multi-selection mode
            selectedEnemies.Clear();
            selectingEnemies = true;
            blurbEvent.Set($"Select up to {targets} different enemies to attack!");
            EventBus.Publish(blurbEvent);
        }

        // Prevent selecting the same enemy twice
        if (selectedEnemies.Contains(enemy))
        {
            blurbEvent.Set("Enemy already selected! Choose a different enemy.");
            EventBus.Publish(blurbEvent);
            Debug.Log($"Enemy {enemy.characterName} already selected!");
            return;
        }

        // Add the selected enemy
        selectedEnemies.Add(enemy);
        blurbEvent.Set($"Selected: {enemy.characterName}");
        EventBus.Publish(blurbEvent);
        Debug.Log($"Enemy {enemy.characterName} selected. Total selected: {selectedEnemies.Count}");

        // If the required number of enemies is selected, start the attack coroutine
        if (selectedEnemies.Count == targets)
        {
            Debug.Log($"{targets} enemies selected. Starting multi-target attack.");
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

            // Reset multi-selection mode
            selectingEnemies = false;
            targetingHUDParent.SetActive(false);
            targetingMode = false;
        }
    }
}

// Helper method to determine the maximum number of targets based on attributes
private int GetMaxTargetsForAttack(List<string> attributes)
{
    if (attributes.Contains("Field"))
    {
        Debug.Log("aattacks is 4" );
        return 4;
    }
    else if (attributes.Contains("Ice"))
    {
            Debug.Log("taattacks is 3" );
        return 3; 
    }
    else if (attributes.Contains("Lunge"))
    {
            Debug.Log("taattacks is 2" );
        return 2;
    }
    else
    {
            Debug.Log("taattacks is 1" );
        return 1; 
    }
}

// Helper method to check if an attack is multi-target
private bool IsMultiTargetAttack(List<string> attributes)
{
    return GetMaxTargetsForAttack(attributes) > 1;
}

    public void OnBideButtonClicked()
    {
        bool bideSuccessful = hero.UseBide();

        if (bideSuccessful)
        {
            ApplyEffectWithDelay(bideani, hero.transform, 0f, 2.0f);
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

    public void onPotionClicked()
    {
        if(  PotionData.Instance.Potion > 0)
        {
            hero.Init(popupPrefabgreen);
            hero.HealDamage((int)(hero.maxHealth*.4));
            if(hero.currentHealth>hero.maxHealth)
            {
                hero.currentHealth = hero.maxHealth;
            }
             usedMoveGO.SetActive(true);
             blurbEvent.Set($"Potion Used");
             EventBus.Publish(blurbEvent);
             usedMove1.text = "Potion Used";
                PotionData.Instance.Potion--;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);
            ApplyEffectWithDelay(potionAni, hero.transform, 0f, 2.0f);
            EventBus.Publish(statusUpdateEvent);
            StartCoroutine(StartTurn());
        }
        else
        {
            descriptionTextPotion.text = "No Potion Left.";
              usedMove1.text = "No Potion";
             
        }

    }
    public void onPanaceaClicked()
    {
        if(PotionData.Instance.Panacea > 0)
        {
            hero.RemoveBurns();
            hero.RemoveParalysis();
            hero.RemoveHeroBurns();
            hero.RemoveHeroParalysis();
            usedMoveGO.SetActive(true);
            blurbEvent.Set($"Status Healed");
            EventBus.Publish(blurbEvent);
            usedMove1.text = "Status Healed";
            PotionData.Instance.Panacea--;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);

            ApplyEffectWithDelay(panaceaAni, hero.transform, 0f, 2.0f);
            StartCoroutine(StartTurn());
        }
        else
        {
            descriptionTextPotion.text = "No Panacea left";
              usedMove1.text = "No Panacea";
            
        }
    }



    private IEnumerator DoBossAttackRoutine(params Enemy[] targetEnemies)
    {
        Debug.Log("DoBossAttackRoutine started.");
        Debug.Log(targetEnemies);
        foreach (var targetEnemy in targetEnemies){

        yield return new WaitForSeconds(0f);

        if(!heroAniPlayed)
        {
            hero.DoAttackAnimation();
            heroAniPlayed = true;
        }
        int damage;
        if(bideAttribute <= 0)
        {
            damage = hero.GetDamage();
        }
        else
        {
            float multiplier = 1.0f + (combinedAttack.upgradeLevel/30f);   
            damage = Mathf.RoundToInt(hero.GetDamage() * multiplier);
        }
        
        //Crit Damage
        if (combinedAttack.attributes.Contains("Steady"))
            {
                 hero.Init(popupPrefab);
                heroCritRate = 0.4f;
                ApplyEffectWithDelay(SteadyAttack, hero.transform, 0f, 2.0f);
                ApplyEffectWithDelay(SteadyHit, targetEnemy.transform, .5f, 3.0f);
                
            }
        
        if(Random.value <= heroCritRate)
        {
            damage = (int)(damage * critMultiplier);
            
             blurbEvent.Set("Critical Hit!");
             EventBus.Publish(blurbEvent);
              usedMove1.text = "Critical Hit";
            targetEnemy.TakeDamage(damage);
            Debug.Log("CRITICAL HHIT");
        }
        else
        {
            targetEnemy.TakeDamage(damage);
        }



         blurbEvent.Set($"Boss attacked {targetEnemy.characterName}");
         EventBus.Publish(blurbEvent);
         usedMove1.text = $"Boss attacked {targetEnemy.characterName} with {combinedAttack.attackName}";
 


        if (combinedAttack.attributes.Contains("Burn"))
        {
            hero.Init(popupPrefab);
            randomChance = (bideAttribute > 0) ? 0.2f : 0.2f;

            if (Random.value <= randomChance && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
            {
                targetEnemy.ApplyBurn(1000, 3);
                blurbEvent.Set($"{targetEnemy.characterName} was burned!");
                EventBus.Publish(blurbEvent);
                usedMove1.text = $"{targetEnemy.characterName} was burned!";
            }

            // Determine rotation based on selected enemy position
            float yRotationOffset = 0f;
            Debug.Log("selectedEnemyNum is" + selectedEnemyNum);
            switch (selectedEnemyNum)
            {
                case 0: yRotationOffset = 30f; break;
                case 1: yRotationOffset = 5f; break;
                case 2: yRotationOffset = 0f; break;
                case 3: yRotationOffset = -25f; break;
            }

            // Create and position temporary GameObject
            Vector3 newPosition = hero.transform.position;
            GameObject tempGameObject = new GameObject();
            tempGameObject.transform.position = newPosition;
       
            Debug.Log("Rotation set to: " + tempGameObject.transform.rotation.eulerAngles);

            // Apply visual effects using the temp object's transform
            Quaternion customRot = Quaternion.Euler(0, yRotationOffset, 0);
            ApplyEffectWithDelay(fireAttack, tempGameObject.transform, 0f, 3.0f, true, true, yRotationOffset);
            
            ApplyEffectWithDelay(fireHit, targetEnemy.transform, 0.5f, 3.0f);
        }

        if (combinedAttack.attributes.Contains("Paralysis"))
            {
                 hero.Init(popupPrefab);

                if (Random.value <= 1f && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
                {
                   
                    targetEnemy.ApplyParalysis(5, true);
                     blurbEvent.Set($"{targetEnemy.characterName} was paralysed!");
                     EventBus.Publish(blurbEvent);
                     
                     usedMove1.text = $"{targetEnemy.characterName} was paralysed!";
                }

                    // Determine rotation based on selected enemy position
            float yRotationOffset = 0f;
            Vector3 newPosition = hero.transform.position;
            newPosition.y += 0f;

                // Create a temporary game object with the new position
            GameObject tempGameObject = new GameObject();
            tempGameObject.transform.position = newPosition;
            Debug.Log("selectedEnemyNum is" + selectedEnemyNum);
            switch (selectedEnemyNum)
            {
                   case 0: yRotationOffset = 115f; break;
                    case 1: yRotationOffset = 95f; break;
                    case 2: yRotationOffset = 75f; break;
                    case 3: yRotationOffset = 55f; break;
            }

            Debug.Log("Rotation set to: " + tempGameObject.transform.rotation.eulerAngles);

            // Apply visual effects using the temp object's transform
            Quaternion customRot = Quaternion.Euler(0, yRotationOffset, 0);
   

            

                // Destroy the temporary game object after 5 seconds
                
            Destroy(tempGameObject, 5f);
            ApplyEffectWithDelay(arrowAttack, tempGameObject.transform, 0f, 3.0f,true, true, yRotationOffset);
            ApplyEffectWithDelay(arrowHit, targetEnemy.transform, .5f, 3.0f);
            }

        if (combinedAttack.attributes.Contains("Heal"))
            {
                
                    
                    hero.Init(popupPrefabgreen);
                    hero.HealDamage(damage);
                    blurbEvent.Set($"{damage} Health Recovered!");
                    EventBus.Publish(blurbEvent);
                    
                     usedMove1.text = $"{damage} Health Recovered!";
                    
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



                // Create a temporary game object with the new position
                GameObject tempGameObject = new GameObject();
                tempGameObject.transform.position = newPosition;
                tempGameObject.transform.localScale = newScale;

                // Destroy the temporary game object after 5 seconds
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
                    usedMove1.text = $"{damage} Health Recovered!";
                    hero.RemoveBurns();
                    hero.RemoveParalysis();
                    hero.RemoveHeroBurns();
                    hero.RemoveHeroParalysis();
                    blurbEvent.Set($"Status Healed");
                    EventBus.Publish(blurbEvent);
                    usedMove1.text = $"Status Healed";
                    ApplyEffectWithDelay(healfield, hero.transform, 0f, 3.0f);
                    ApplyEffectWithDelay(panaceaAni, hero.transform, 0f, 3.0f);
                    
            }


        if (targetEnemy.currentHealth <= 0)
        {
             blurbEvent.Set($"{targetEnemy.characterName} has been defeated!");
             EventBus.Publish(blurbEvent);
                  usedMove1.text = $"{targetEnemy.characterName} has been defeated! with {combinedAttack.attackName}";
            RemoveEnemy(targetEnemy);
        }

        
        // if (combinedAttack.attributes.Contains("Lunge"))
        // {
        //     if(attackExtra == false)
        //     {
        //         extraAttacks = 1;
        //     }
        //     else if(attackExtra == true && extraAttacks == 0)
        //     {
        //         attackExtra = false;
        //         StartTurn();
        //     }
        // }

        if (combinedAttack.attributes.Contains("Slash"))
            {
                 hero.Init(popupPrefab);
               
                Vector3 newPosition = targetEnemy.transform.position;
                newPosition.y += 3f;

                // Create a temporary transform with the new position
                Transform tempTransform = new GameObject().transform;
                tempTransform.position = newPosition;

                ApplyEffectWithDelay(slashAttack, hero.transform, 0f, 3.0f, null, true);
                ApplyEffectWithDelay(slashHit, tempTransform, .5f, 3.0f);
                ApplyEffectWithDelay(slashCrater, targetEnemy.transform , .8f, 3.0f);
                
            }

            if (combinedAttack.attributes.Contains("Triple"))
            {
                 hero.Init(popupPrefab);
                targetEnemy.TakeDamage(damage);
                targetEnemy.TakeDamage(damage);
        
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, true);
                    blurbEvent.Set($"{targetEnemy.characterName} was paralysed!");
                    EventBus.Publish(blurbEvent);
                    
                  usedMove1.text = $"{targetEnemy.characterName} was paralysed!";
                    
                }
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    targetEnemy.ApplyParalysis(5, true);
                    blurbEvent.Set($"{targetEnemy.characterName} was paralysed!");
                    EventBus.Publish(blurbEvent);
                      usedMove1.text = $"{targetEnemy.characterName} was paralysed!";
                }
                if (Random.value <= .1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    
                    targetEnemy.ApplyParalysis(5, true);
                    blurbEvent.Set($"{targetEnemy.characterName} was paralysed!");
                    EventBus.Publish(blurbEvent);
                      usedMove1.text = $"{targetEnemy.characterName} was paralysed!";
                }
                ApplyEffectWithDelay(tripleAttack, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(tripleHit, targetEnemy.transform, .5f, 3.0f, null, true);
            }
             
        if (combinedAttack.attributes.Contains("Ice") && !hasIced)
        {
            //hasIced = true;
          
             hero.Init(popupPrefab);
            ApplyEffectWithDelay(iceAttack, hero.transform, 0f, 3.0f);
            ApplyEffectWithDelay(iceHit, targetEnemy.transform, .5f, 3.0f);
            // targetingHUDParent.SetActive(true);
            // targetingMode = true;
             blurbEvent.Set($"You strike again");
             EventBus.Publish(blurbEvent);
             
            
            
        }

        if (combinedAttack.attributes.Contains("Lunge"))
        {
             hero.Init(popupPrefab);
            blurbEvent.Set($"You strike twice");
            EventBus.Publish(blurbEvent);

            ApplyEffectWithDelay(lungeAttack, hero.transform, 0f, 3.0f, true, false);
            ApplyEffectWithDelay(lungeHit, targetEnemy.transform, .5f, 3.0f);
          
            
            
        }

        
        // if (combinedAttack.attributes.Contains("Lunge") && !hasLunged)
        // {
        //     attackExtra = true;
        //     extraAttacks--;
        //     hasLunged = true;
        //     blurbEvent.Set($"You prepare to strike again");
        //     EventBus.Publish(blurbEvent);
        //     targetingMode = true;
        //     yield break;
        // }
        // else if(extraAttacks == 0)
        // {
        //     extraAttacks--;
        //     StartTurn();
        // }

        heroCritRate = 0.05f;
        hasIced = false;
        
        hasLunged = false;
        }
    
        heroAniPlayed = false;
        StartCoroutine(StartTurn());
    }


    public void RemoveEnemy(Enemy enemy)
{
    enemy.dead = true;
    enemy.RemoveParalysis();
    enemy.RemoveBurns();

    if (currentGameMode == GameMode.Endless)
    {
        totalEnemiesKilled++;
        if (totalEnemiesKilled % 4 == 0)
            IncreaseDifficulty();

        int spawnIndex = enemies.IndexOf(enemy);
        if (spawnIndex < 0) return;

        // Set the enemy slot to null instead of removing
        enemies[spawnIndex] = null;
        aliveEnemies.Remove(enemy);

        StartCoroutine(ReplaceEnemyAfterDelay(enemy, spawnIndex));
    }
    else // Standard mode
    {
        foreach (var body in enemies)
        {
            if (body.dead)
            {
                enemiesDead++;
            }
        }

        if (enemiesDead == 4)
        {
            EndGame(true); // Player wins
        }

        enemiesDead = 0;
    }
}

private IEnumerator ReplaceEnemyAfterDelay(Enemy deadEnemy, int spawnIndex)
{
    var spawnPoint = enemySpawns[spawnIndex];

    StartCoroutine(FadeOut(deadEnemy.gameObject));
    Destroy(deadEnemy.gameObject, 1f);

    yield return new WaitForSeconds(1f);

    var newPrefab = GetNextEnemyFromCurrentDifficulty(upcomingEnemies);
    if (newPrefab == null)
        yield break;

    // Use SpawnPrefabAtPosition to ensure any necessary setup
    GameObject newObj = SpawnPrefabAtPosition(newPrefab, spawnPoint);
    var newEnemy = newObj.GetComponent<Enemy>();
    newEnemy.Init(popupPrefab);

    // Assign new enemy to the correct index in enemies list
    enemies[spawnIndex] = newEnemy;
    aliveEnemies.Add(newEnemy);

    // Update the corresponding HUD
    enemyHUDs[spawnIndex].Init(newEnemy);
}


private GameObject GetRandomEnemyPrefab(Enemy deadEnemy)
{
    return GetNextEnemyFromCurrentDifficulty();
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


private int GetDeadEnemySpawnIndex(Enemy deadEnemy)
{
    return enemies.IndexOf(deadEnemy);  // Find the index in the enemies list and spawn at that position
}



    private IEnumerator FadeOut(GameObject enemy)
    {
        yield return new WaitForSeconds(.5f);
        float duration = 1.5f;
        float time = 0f;
        Vector3 startScale = enemy.transform.localScale;
        
        while (time < duration)
        {
            float t = time / duration;
            enemy.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(enemy);
            
      
    }



    public void EndGame(bool playerWon)
    {
        Debug.Log(playerWon ? "You won!" : "Game Over.");

        StartCoroutine(DoEndGameRoutine(playerWon));
        CrazySDK.Game.GameplayStop();
    }
        public void ReviveBoss()
    {
        //Todo Create a separate button for crazy games ad ALSO implement add check (if add is complete revive boss else ??)
      CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, () => // or CrazyAdType.Rewarded
        {
            // ad started
        }, (error) =>
        {
            // ad error
        }, () =>
        {
                hero.currentHealth = hero.maxHealth;
                hero.animator.SetTrigger("Revive");
                //TODO check performance of this 
                GameObject boss = GameObject.FindGameObjectWithTag("Boss");
                if (boss != null)
                {
                    Vector3 pos = boss.transform.position;
                    pos.y = 0;
                    boss.transform.position = pos;
                }

                ApplyEffectWithDelay(bideani, hero.transform, 0f, 2.0f);
                StartCoroutine(StartTurn());
                loseScreen.SetActive(false);
                MainUIParent.SetActive(true);
                CrazySDK.Game.GameplayStart();
            // ad finished, for rewarded ads give reward here
        });
    
        
    }
    

    private IEnumerator DoEndGameRoutine(bool playerWon)
    {

        yield return new WaitForSeconds(1f);

        if (playerWon)
        {
            if (SceneManager.GetActiveScene().name == "LVL5")
            {
                //Todo Make a congrats you win scene?
                //CrazySDK.Game.HappyTime();

            }
            else
            winScreen.SetActive(true);
            MainUIParent.SetActive(false);
        }
        else
        {
            loseScreen.SetActive(true);
        }
    }
}