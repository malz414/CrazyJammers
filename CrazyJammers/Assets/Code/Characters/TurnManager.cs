using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Code.Utility.Events;
using UnityEngine.EventSystems;
using DamageNumbersPro;
using System.Linq;


public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;


    [SerializeField] private GameObject SteadyAttack;
    [SerializeField] private GameObject SteadyHit;

    [SerializeField] private GameObject lungeAttack;
    [SerializeField] private GameObject lungeHit;

    [SerializeField] private GameObject arrowAttack;
    [SerializeField] private GameObject arrowHit;

    [SerializeField] private GameObject slashAttack;
    [SerializeField] private GameObject slashHit;

    [SerializeField] private GameObject iceAttack;
    [SerializeField] private GameObject iceHit;

    [SerializeField] private GameObject fireAttack;
    [SerializeField] private GameObject fireHit;

    [SerializeField] private GameObject barrier1;
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

    [SerializeField] GameObject potionOptions;

    [SerializeField] GameObject targetingHUDParent;

    [SerializeField] GameObject winScreen;

    [SerializeField] GameObject loseScreen;

    [SerializeField] CharacterHUD[] enemyHUDs;

    [SerializeField] CharacterHUD bossHUD;

    [SerializeField] private DamageNumber popupPrefab;

    private List<AttackSO> enemyAttacksByIndex = new List<AttackSO> { null, null, null, null };

    private List<Enemy> selectedEnemies = new List<Enemy>();
    private bool selectingEnemies = false;

    private Hero hero;
    private List<Enemy> enemies;

    public Button[] attackButtons;
    public Button bideButton;

    public Button potionButton;
    public Button panaceaButton;
    public GameObject itemOptions;


    private AttackSO selectedAttack1;
    private AttackSO selectedAttack2;

    private Coroutine bossAttackCoroutine;

    private List<AttackSO> enemyAttacksUsed;

    private GameplayBlurbEvent blurbEvent;

    private bool hasLunged = false;
    private bool hasIced = false;

    private int extraAttacks = 0;
    private bool attackExtra = false;

    public bool TargetingMode => targetingMode;

    private bool targetingMode = false;
    public float critMultiplier = 1.2f;

    public float heroCritRate = .05f;

    private bool bideBuff = false;
    private int bideAttribute = 0;
    private float randomChance = 0f;

    private int Potion = 1;
    private int Panacea = 1;
    private int enemiesDead = 0;


    [SerializeField] private TextMeshProUGUI PotAmount; 
    [SerializeField] private TextMeshProUGUI PanAmount; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    public TextMeshProUGUI Popop; 

    private CharacterStatusUpdateEvent statusUpdateEvent;
    


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
    }

    public void StartBattle()
    {
        SetUpBattle();
    }
    //VFX called with delay for some so attacks go off then theres a delay on the hit more time is given to the duration so with delay + duration it doesnt  cancel early 

    private void ApplyEffectWithDelay(GameObject effectPrefab, Transform target, float delay, float effectDuration = 2f)
    {
        StartCoroutine(DelayedEffectCoroutine(effectPrefab, target, delay, effectDuration));
    }

    private IEnumerator DelayedEffectCoroutine(GameObject effectPrefab, Transform target, float delay, float effectDuration)
    {
        yield return new WaitForSeconds(delay);
        GameObject effect = Instantiate(effectPrefab, target.position, Quaternion.identity);
        effect.transform.SetParent(target); 
        Destroy(effect, delay + effectDuration); // Ensures the effect lasts for delay + effectDuration seconds
    }


    private void SetUpBattle()
    {
        GameObject knightObj = SpawnPrefabAtPosition(knightPrefab, enemySpawns[0]);

        GameObject swordsmanObj = SpawnPrefabAtPosition(swordsmanPrefab, enemySpawns[1]);
        GameObject mageObj = SpawnPrefabAtPosition(magePrefab, enemySpawns[2]);
        GameObject archerObj = SpawnPrefabAtPosition(archerPrefab, enemySpawns[3]);

        enemies = new List<Enemy>();

        enemies.Add(knightObj.GetComponent<Enemy>());
        enemies.Add(swordsmanObj.GetComponent<Enemy>());
        enemies.Add(mageObj.GetComponent<Enemy>());
        enemies.Add(archerObj.GetComponent<Enemy>());

        enemies[0].Init(popupPrefab);
        enemies[1].Init(popupPrefab);
        enemies[2].Init(popupPrefab);
        enemies[3].Init(popupPrefab);

        enemyHUDs[0].Init(enemies[0]);
        enemyHUDs[1].Init(enemies[1]);
        enemyHUDs[2].Init(enemies[2]);
        enemyHUDs[3].Init(enemies[3]);
        



        targetingMode = false;
        targetingHUDParent.SetActive(false);

        GameObject bossObj = SpawnPrefabAtPosition(bossPrefab, bossSpawn);

        hero = bossObj.GetComponent<Hero>();

        foreach(var obj in listOfObjectToDeactivateAtStartOfBattle)
        {
            obj.SetActive(false);
        }
        hero.Init(popupPrefab);

        bossHUD.Init(hero);

        MainUIParent.SetActive(true);

        enemyAttacksUsed = new List<AttackSO>();

        blurbEvent = new GameplayBlurbEvent();

        StartCoroutine(DoBattleStartRoutine());

    }

    private IEnumerator DoBattleStartRoutine()
    {
        yield return new WaitForSeconds(.25f);

        EventBus.Publish(new FadeInEvent(UICategoryEnums.GamePlayUI));

        yield return new WaitForSeconds(2f);

        StartTurn();
    }

    private GameObject SpawnPrefabAtPosition(GameObject prefab, Transform pos)
    {
        GameObject spawned = GameObject.Instantiate(prefab);
        spawned.transform.position = pos.transform.position;
        spawned.transform.rotation = pos.transform.rotation;
        return spawned;
    }

    public void StartTurn()
    {
        hero.UpdateEffects();
        if (hero.burning > 0)
        {
            hero.currentHealth -= (int)(hero.maxHealth*.1);
            ApplyEffectWithDelay(burn, hero.transform, 0f, 3.0f);
            blurbEvent.Set($" You're Burning for {hero.burning} turns!");
            EventBus.Publish(blurbEvent);
            hero.burning--;
            EventBus.Publish(statusUpdateEvent);
            if(hero.currentHealth <= 0) 
            {
                EndGame(false);
                return;
            }

        }

        foreach (var enemy in enemies)
        {
            if (enemy.burning > 0)
            {
                enemy.currentHealth -= (int)(enemy.maxHealth*.1);
                ApplyEffectWithDelay(burn, enemy.transform, 0f, 3.0f);
                if (enemy.currentHealth <= 0)
                {
                    blurbEvent.Set($"{enemy.characterName} has been burned to death!");
                    EventBus.Publish(blurbEvent);
                    RemoveEnemy(enemy);
                }
                else
                {
                    enemy.burning--;
                    blurbEvent.Set($" {enemy.characterName} is burning for {enemy.burning} turns!");
                    EventBus.Publish(blurbEvent);

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

    
        enemyAttacksUsed.Clear();
        if(hero.currentHealth <= 0) 
        {
            EndGame(false);
            return;
        }
        StartCoroutine(DoTurnRoutine());
    }

    private IEnumerator DoTurnRoutine()
    {
        yield return new WaitForSeconds(.5f);
        
        

        // Enemies attack hero
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (!enemy.CanAct())
            {
                blurbEvent.Set($"{enemy.characterName} is paralyzed and cannot act this turn!");
                EventBus.Publish(blurbEvent);
                ApplyEffectWithDelay(para, enemy.transform, 0f, 3.0f);
           
                continue;
            }
            else if (enemy.dead)
            {
                continue;
            }

            else
            {


                
            AttackSO enemyAttack = enemy.PerformRandomAttack();
            enemyAttacksByIndex[i] = enemyAttack;

            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, assigning to slot {i}, dealing {enemyAttack.GetDamage()} damage.");


            if (enemyAttack.attributes.Contains("Barrier"))
            {
                foreach (var enemyBarrier in enemies)
                {
                    if(enemyBarrier.currentHealth <= 0) continue;
                    enemyBarrier.barrierCount += 1;
                    ApplyEffectWithDelay(barrier1, enemyBarrier.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(barrier2, enemyBarrier.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(barrier3, enemyBarrier.transform, 0f, 2.0f);
                    blurbEvent.Set($"The heroes gained a barrier.");
                    EventBus.Publish(blurbEvent);

                }

            continue;
            }
            if (enemyAttack.attributes.Contains("Field"))
            {
                foreach (var enemyHeal in enemies)
                {
                    if(enemyHeal.currentHealth <= 0) continue;
                    enemyHeal.currentHealth += enemyAttack.maxHealth*0.2;
                    if(enemyHeal.currentHealth >= enemyHeal.maxHealth)
                    {
                        enemyHeal.currentHealth = enemyHeal.maxHealth;
                    }
                    enemyHeal.RemoveBurns();
                    enemyHeal.RemoveParalysis();
                    ApplyEffectWithDelay(heal, enemyHeal.transform, 0f, 2.0f);
                    ApplyEffectWithDelay(panaceaAni, enemyHeal.transform, 0f, 2.0f);
                    blurbEvent.Set($"The heroes healed and status cured.");
                    blurbEvent.Set($"The heroes gained a barrier.");
                    EventBus.Publish(blurbEvent);

                }

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
           
            enemyHeal.currentHealth += enemyAttack.maxHealth*0.2;

           
            if (enemyHeal.currentHealth > enemyHeal.maxHealth)
            {
                enemyHeal.currentHealth = enemyHeal.maxHealth;
            }

      
            ApplyEffectWithDelay(heal, enemyHeal.transform, 0f, 2.0f);

           
            blurbEvent.Set($"{enemyHeal.characterName} was healed.");
            EventBus.Publish(blurbEvent);
            EventBus.Publish(statusUpdateEvent);
        }
                
            }

            if (enemyAttack.attributes.Contains("Ice"))
            {
                hero.TakeDamage(enemyAttack.GetDamage());
                ApplyEffectWithDelay(iceAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(iceHit, hero.transform, .5f, 3.0f);
            }

            
            if (enemyAttack.attributes.Contains("Lunge"))
            {
                hero.TakeDamage(enemyAttack.GetDamage());
                ApplyEffectWithDelay(lungeAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(lungeHit, hero.transform, .5f, 3.0f);
            }

            if (enemyAttack.attributes.Contains("Slash"))
            {
                hero.TakeDamage(enemyAttack.GetDamage());
                ApplyEffectWithDelay(slashAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(slashHit, hero.transform, .5f, 3.0f);
            }



            if (enemyAttack.attributes.Contains("Steady"))
            {
                ApplyEffectWithDelay(SteadyAttack, enemy.transform, 0f, 2.0f);
                ApplyEffectWithDelay(SteadyHit, hero.transform, .5f, 3.0f);
                hero.TakeDamage(enemyAttack.GetDamage());
            }

            



            hero.TakeDamage(enemyAttack.GetDamage());
            blurbEvent.Set($"{enemy.characterName} used {enemyAttack.attackName}!");
            EventBus.Publish(blurbEvent);

            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, dealing {enemyAttack.GetDamage()} damage to the hero.");

            if (enemyAttack.attributes.Contains("Burn"))
            {

                if (Random.value <= 0.3f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyBurn(10, 3);
                    blurbEvent.Set($"Boss has been burned by {enemyAttack.attackName}!");
                    EventBus.Publish(blurbEvent);
                    Debug.Log($"Hero has been burned by {enemyAttack.attackName}!");
                }
                ApplyEffectWithDelay(fireAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(fireHit, hero.transform, .5f, 3.0f);
            }

            if (enemyAttack.attributes.Contains("Paralysis"))
            {

                if (Random.value <= 1f && hero.GetParalysisTurnsRemaining() < 1 && hero.burning < 1)
                {
                    hero.ApplyParalysis(5, false);
                    blurbEvent.Set($"Boss has been paralyzed by {enemyAttack.attackName}!");
                     EventBus.Publish(blurbEvent);
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
                ApplyEffectWithDelay(arrowAttack, enemy.transform, 0f, 3.0f);
                ApplyEffectWithDelay(arrowHit, hero.transform, .5f, 3.0f);
            }




            yield return new WaitForSeconds(1.5f);

            if(hero.currentHealth <= 0)
            {
                yield break;
            }
            }
        }

        yield return new WaitForSeconds(1.5f);
        hero.bideBuff = false;

        if (!hero.CanAct())
        {
             blurbEvent.Set($"Boss is paralyzed and cannot act this turn!");
             EventBus.Publish(blurbEvent);
            ApplyEffectWithDelay(para, hero.transform, 0f, 3.0f);
            Debug.Log("Hero is paralyzed and cannot act this turn!");
            StartTurn(); // Skip the hero's turn
        }
        else
        {
            // Show attack selection UI for the hero
            ShowAttackSelectionUI();
        }

    }

    private void ShowAttackSelectionUI()
    {
        for (int i = 0; i < attackButtons.Length; i++)
        {
            if (enemyAttacksByIndex.Capacity > i)
            {
                if (enemyAttacksByIndex[i] != null)
                {
                    attackButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyAttacksByIndex[i].attackName;
                    int index = i;
                    attackButtons[i].onClick.RemoveAllListeners();
                    attackButtons[i].onClick.AddListener(() => OnAttackButtonClicked(index));
                    AddHoverEvents(attackButtons[i], enemyAttacksByIndex[i].attackDesctiption);
                    attackButtons[i].gameObject.SetActive(true);
                }               
            }
            else
            {
                attackButtons[i].gameObject.SetActive(false);
            }
        }
        attackOptionsParent.SetActive(true);
        potionOptions.SetActive(true);
        PanAmount.text = Potion.ToString();
        PotAmount.text = Panacea.ToString();
    }

        // Set up the Bide button
        /*        bideButton.gameObject.SetActive(true);
                bideButton.onClick.RemoveAllListeners();
                bideButton.onClick.AddListener(OnBideButtonClicked);
                bideButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bide";*/

private void AddHoverEvents(Button button, string description)
{
    EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
    if (trigger == null)
    {
        trigger = button.gameObject.AddComponent<EventTrigger>();
    }
    trigger.triggers.Clear();

    EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
    {
        eventID = EventTriggerType.PointerEnter
    };
    pointerEnterEntry.callback.AddListener((_) => ShowDescription(description));
    trigger.triggers.Add(pointerEnterEntry);

    
    EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
    {
        eventID = EventTriggerType.PointerExit
    };
    pointerExitEntry.callback.AddListener((_) => HideDescription());
    trigger.triggers.Add(pointerExitEntry);
}

private void ShowDescription(string description)
{
    
    descriptionText.text = description;
    
}

private void HideDescription()
{
    descriptionText.text = "Select a move";

}

    private void OnAttackButtonClicked(int index)
    {
        if (enemyAttacksByIndex[index] == null) return; // Ensure the selected index has an attack

        var selectedAttack = enemyAttacksByIndex[index];

        
        if (selectedAttack1 == selectedAttack)
        {
             blurbEvent.Set($"Select a different move!");
             EventBus.Publish(blurbEvent);
            return; 
        }

        if (selectedAttack1 == null)
        {
            selectedAttack1 = selectedAttack;
            Debug.Log($"Selected first attack: {selectedAttack1.attackName}");
        }
        else if (selectedAttack2 == null)
        {
            selectedAttack2 = selectedAttack;
            Debug.Log($"Selected second attack: {selectedAttack2.attackName}");

            // Combine selected attacks
            hero.CombineAttacks(selectedAttack1, selectedAttack2);

            // Reset selections
            selectedAttack1 = null;
            selectedAttack2 = null;

            attackOptionsParent.SetActive(false);
            attackOptionsMenu.SetActive(false);
            potionOptions.SetActive(false);
            targetingHUDParent.SetActive(true);
            targetingMode = true;
        }
    }

    public void OnItemOptionsClicked()
    {
        itemOptions.SetActive(true);
        PanAmount.text = Panacea.ToString();
        PotAmount.text = Potion.ToString();
        
    }


    public void SelectEnemyTotAttack(Enemy enemy)
{
    // Check for "Lunge" attribute to determine multi-target behavior
    if(!targetingMode) return;
    if (!combinedAttack.attributes.Contains("Lunge")) 
    {   Debug.Log("ATTACKING NO lunge");
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
    }
    else
    {
        Debug.Log("ATTACKING WITH lunge");
        // Multi-target attack
        if (!selectingEnemies)
        {
            // Start multi-selection mode
            selectedEnemies.Clear();
            selectingEnemies = true;
            blurbEvent.Set("Select two different enemies to attack!");
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

        // If two enemies are selected, start the attack coroutine
        if (selectedEnemies.Count == 2)
        {
            Debug.Log("Two enemies selected. Starting multi-target attack.");
            StartCoroutine(DoBossAttackRoutine(selectedEnemies.ToArray()));

            // Reset multi-selection mode
            selectingEnemies = false;
            targetingHUDParent.SetActive(false);
            targetingMode = false;
        }
    }
}


    public void OnBideButtonClicked()
    {
        bool bideSuccessful = hero.UseBide();

        if (bideSuccessful)
        {
            ApplyEffectWithDelay(bideani, hero.transform, 0f, 2.0f);
            bideAttribute = 3;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            hero.bideBuff = true;
            bideBuff = true;
            StartTurn();
        }
    }

    public void onPotionClicked()
    {
        if(Potion > 0)
        {

            hero.currentHealth += 1000;
            if(hero.currentHealth>hero.maxHealth)
            {
                hero.currentHealth = hero.maxHealth;
            }
             blurbEvent.Set($"Potion Used");
             EventBus.Publish(blurbEvent);
            Potion --;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);
            ApplyEffectWithDelay(potionAni, hero.transform, 0f, 2.0f);
            EventBus.Publish(statusUpdateEvent);
            StartTurn();
        }
        else
        {
             blurbEvent.Set($"No Potion");
             EventBus.Publish(blurbEvent);
        }

    }
    public void onPanaceaClicked()
    {
        if(Panacea > 0)
        {
            hero.RemoveBurns();
            hero.RemoveParalysis();
            hero.RemoveHeroBurns();
            hero.RemoveHeroParalysis();
             blurbEvent.Set($"Status Healed");
             EventBus.Publish(blurbEvent);
            Panacea --;
            attackOptionsParent.SetActive(false);
            potionOptions.SetActive(false);
            itemOptions.SetActive(false);

            ApplyEffectWithDelay(panaceaAni, hero.transform, 0f, 2.0f);
            StartTurn();
        }
        else
        {
             blurbEvent.Set($"No Panacea");
             EventBus.Publish(blurbEvent);
        }
    }



    private IEnumerator DoBossAttackRoutine(params Enemy[] targetEnemies)
    {
        Debug.Log("DoBossAttackRoutine started.");
        Debug.Log(targetEnemies);
        foreach (var targetEnemy in targetEnemies){

        yield return new WaitForSeconds(1f);

        hero.DoAttackAnimation();

        int damage = hero.GetDamage();
        //Crit Damage
        if (combinedAttack.attributes.Contains("Steady"))
            {
                heroCritRate = 0.4f;
                ApplyEffectWithDelay(SteadyAttack, hero.transform, 0f, 2.0f);
                ApplyEffectWithDelay(SteadyHit, targetEnemy.transform, .5f, 3.0f);
                
            }
        
        if(Random.value <= heroCritRate)
        {
            damage = (int)(damage * critMultiplier);
            damage = (int)(damage * 1.2);
             blurbEvent.Set("Critical Hit!");
             EventBus.Publish(blurbEvent);
            targetEnemy.TakeDamage(damage);
        }
        else
        {
            targetEnemy.TakeDamage(damage);
        }



         blurbEvent.Set($"Boss attacked {targetEnemy.characterName}");
         EventBus.Publish(blurbEvent);
        Debug.Log($"Boss attacked {targetEnemy.characterName}, dealing {damage} damage.");


        if (combinedAttack.attributes.Contains("Burn"))
            {   
                randomChance = (bideAttribute > 0) ? 0.4f : 0.2f;
                if (Random.value <= randomChance  && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
                {
                    targetEnemy.ApplyBurn(1000, 3);
                     blurbEvent.Set($"{targetEnemy.characterName} was burned!");
                     EventBus.Publish(blurbEvent);
                    
                }
                ApplyEffectWithDelay(fireAttack, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(fireHit, targetEnemy.transform, .5f, 3.0f);
            }

        if (combinedAttack.attributes.Contains("Paralysis"))
            {

                if (Random.value <= 1f && targetEnemy.GetParalysisTurnsRemaining() < 1 && targetEnemy.burning < 1)
                {
                   
                    targetEnemy.ApplyParalysis(5, true);
                     blurbEvent.Set($"{targetEnemy.characterName} was paralysed!");
                     EventBus.Publish(blurbEvent);
                }
                ApplyEffectWithDelay(arrowAttack, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(arrowHit, targetEnemy.transform, .5f, 3.0f);
            }

        if (combinedAttack.attributes.Contains("Heal"))
            {
                    hero.currentHealth += damage;
                     blurbEvent.Set($"{damage} Health Recovered!");
                     EventBus.Publish(blurbEvent);
                    ApplyEffectWithDelay(heal, hero.transform, 0f, 3.0f);
                    EventBus.Publish(statusUpdateEvent);
                    
             }

        
          if (combinedAttack.attributes.Contains("Barrier"))
            {
                    hero.barrierCount += 1;
                     blurbEvent.Set("Barrier raised");
                     EventBus.Publish(blurbEvent);
                    ApplyEffectWithDelay(barrier1, hero.transform, 0f, 3.0f);
                    ApplyEffectWithDelay(barrier2, hero.transform, 0f, 3.0f);
                    ApplyEffectWithDelay(barrier3, hero.transform, 0f, 3.0f);

            }

        if (combinedAttack.attributes.Contains("Field"))
            {
                    hero.currentHealth += damage;
                     blurbEvent.Set($"{damage} Health Recovered!");
                     EventBus.Publish(blurbEvent);
                    hero.RemoveBurns();
                    hero.RemoveParalysis();
                    hero.RemoveHeroBurns();
                    hero.RemoveHeroParalysis();
                     blurbEvent.Set($"Status Healed");
                     EventBus.Publish(blurbEvent);
                    ApplyEffectWithDelay(heal, hero.transform, 0f, 3.0f);
                    ApplyEffectWithDelay(panaceaAni, hero.transform, 0f, 3.0f);
            }


        if (targetEnemy.currentHealth <= 0)
        {
             blurbEvent.Set($"{targetEnemy.characterName} has been defeated!");
             EventBus.Publish(blurbEvent);
            RemoveEnemy(targetEnemy);
        }

        
        yield return new WaitForSeconds(1f);
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
                heroCritRate = 0.4f;
                ApplyEffectWithDelay(slashAttack, hero.transform, 0f, 3.0f);
                ApplyEffectWithDelay(slashHit, targetEnemy.transform, .5f, 3.0f);
                
            }
        if (combinedAttack.attributes.Contains("Ice") && !hasIced)
        {
            //hasIced = true;
            targetEnemy.TakeDamage(damage);
            
            ApplyEffectWithDelay(iceAttack, hero.transform, 0f, 3.0f);
            ApplyEffectWithDelay(iceHit, targetEnemy.transform, .5f, 3.0f);
            // targetingHUDParent.SetActive(true);
            // targetingMode = true;
             blurbEvent.Set($"YouWW strike again");
             EventBus.Publish(blurbEvent);
            
            
        }

        if (combinedAttack.attributes.Contains("Lunge"))
        {
              blurbEvent.Set($"You strike twice");
            EventBus.Publish(blurbEvent);
            ApplyEffectWithDelay(lungeAttack, hero.transform, 0f, 3.0f);
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
        StartTurn();
    }



    public void RemoveEnemy(Enemy enemy)
        {
        enemy.dead = true;
        
        Debug.Log("Enemies dead = " + enemiesDead);


        StartCoroutine(FadeOut(enemy.gameObject));  
        foreach (var body in enemies)
            {
                if (body.dead)
                {
                    enemiesDead++;
                    if(enemiesDead == 4)
                    {
                         EndGame(true); // Player wins

                    }
                }
            }
        enemiesDead = 0;
        }


    private IEnumerator FadeOut(GameObject enemy)
    {
        Renderer enemyRenderer = enemy.GetComponent<Renderer>();
        Material material = enemyRenderer.material;
        Color startColor = material.color;
        float fadeDuration = 2f; // Time in seconds for the fade out
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpValue = timeElapsed / fadeDuration;
            material.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, lerpValue));
            yield return null;
        }

        
      
    }



    public void EndGame(bool playerWon)
    {
        Debug.Log(playerWon ? "You won!" : "Game Over.");

        StartCoroutine(DoEndGameRoutine(playerWon));
    }

    private IEnumerator DoEndGameRoutine(bool playerWon)
    {

        yield return new WaitForSeconds(1f);

        if (playerWon)
        {
            winScreen.SetActive(true);
            MainUIParent.SetActive(false);
        }
        else
        {
            loseScreen.SetActive(true);
        }
    }
}