using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Code.Utility.Events;
using DamageNumbersPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;
    public CombinedAttackSO combinedAttack;

    [SerializeField] private Transform[] enemySpawns;

    [SerializeField] private GameObject bossPrefab;

    [SerializeField] private Transform bossSpawn;

    [SerializeField] GameObject MainUIParent;

    [SerializeField] GameObject attackOptionsParent;

    [SerializeField] GameObject targetingHUDParent;

    [SerializeField] GameObject winScreen;

    [SerializeField] GameObject loseScreen;

    [SerializeField] CharacterHUD[] enemyHUDs;

    [SerializeField] CharacterHUD bossHUD;

    [SerializeField] private DamageNumber popupPrefab;

    private Hero hero;
    private List<Enemy> enemies;

    public Button[] attackButtons; 
    public Button bideButton;

    private AttackSO selectedAttack1;
    private AttackSO selectedAttack2;

    private List<AttackSO> enemyAttacksUsed;

    public bool TargetingMode => targetingMode;

    private bool targetingMode = false;

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

    public void StartBattle()
    {
        SetUpBattle();
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

        hero.Init(popupPrefab);

        bossHUD.Init(hero);

        MainUIParent.SetActive(true);

        enemyAttacksUsed = new List<AttackSO>();

        StartCoroutine(DoBattleStartRoutine());

    }

    private IEnumerator DoBattleStartRoutine()
    {
        yield return new WaitForSeconds(.25f);

        EventBus.Publish(new FadeInEvent());

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
        foreach (var enemy in enemies)
        {
            enemy.UpdateEffects();
        }

        if (hero.currentHealth <= 0 || enemies.Count == 0)
        {
            return;
        }

        // Clear previous attacks at the start of each turn
        enemyAttacksUsed.Clear();

        StartCoroutine(DoTurnRoutine());
    }

    private IEnumerator DoTurnRoutine()
    {
        yield return new WaitForSeconds(.5f);

        // Enemies attack hero
        foreach (var enemy in enemies)
        {
            if (!enemy.CanAct())
            {
                Debug.Log("ENEMY is paralyzed and cannot act this turn!");
                continue;
            }
                
            else
            {
            Debug.Log("not para !");
            AttackSO enemyAttack = enemy.PerformRandomAttack();
            enemyAttacksUsed.Add(enemyAttack);
            hero.TakeDamage(enemyAttack.GetDamage());
            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, dealing {enemyAttack.GetDamage()} damage to the hero.");
            if (enemyAttack.attributes.Contains("Burn"))
            {
                
                if (Random.value <= 0.3f) 
                {
                    hero.ApplyBurn(10, 3);
                    Debug.Log($"Hero has been burned by {enemyAttack.attackName}!");
                }
            }
            
            if (enemyAttack.attributes.Contains("Paralysis"))
            {
               
                if (Random.value <= 1f) 
                {
                    hero.ApplyParalysis(5, false); 
                    Debug.Log($"Hero has been paralyzed by {enemyAttack.attackName}!");
                }
            }
        

            yield return new WaitForSeconds(1.5f);

            if(hero.currentHealth <= 0)
            {
                yield break;
            }
            }
        }

        yield return new WaitForSeconds(1.5f);

        if (!hero.CanAct())
        {
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
            if (i < enemyAttacksUsed.Count)
            {
                attackButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyAttacksUsed[i].attackName; 
                int index = i;
                attackButtons[i].onClick.RemoveAllListeners();
                attackButtons[i].onClick.AddListener(() => OnAttackButtonClicked(index));
                attackButtons[i].gameObject.SetActive(true);
            }
            else
            {
                attackButtons[i].gameObject.SetActive(false);
            }
        }

        attackOptionsParent.SetActive(true);

        // Set up the Bide button
        /*        bideButton.gameObject.SetActive(true);
                bideButton.onClick.RemoveAllListeners();
                bideButton.onClick.AddListener(OnBideButtonClicked);
                bideButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bide";*/
    }


    private void OnAttackButtonClicked(int index)
    {
        if (selectedAttack1 == null)
        {
            selectedAttack1 = enemyAttacksUsed[index];
            Debug.Log($"Selected first attack: {selectedAttack1.attackName}");
        }
        else if (selectedAttack2 == null)
        {
            selectedAttack2 = enemyAttacksUsed[index];
            Debug.Log($"Selected second attack: {selectedAttack2.attackName}");

            // Combine selected attacks
            hero.CombineAttacks(selectedAttack1, selectedAttack2);

            // Reset selections
            selectedAttack1 = null;
            selectedAttack2 = null;

            
            //hero.RandomlySelectAttack();

            attackOptionsParent.SetActive(false);
            targetingHUDParent.SetActive(true);
            targetingMode = true;

            enemyAttacksUsed.Clear();
            /*            heroAttackEnemy();

                        StartTurn();*/


        }
    }

    public void SelectEnemyTotAttack(Enemy enemy)
    {
        targetingMode = false;
        targetingHUDParent.SetActive(false);
        StartCoroutine(DoBossAttackRoutine(enemy));
    }
    
    private void OnBideButtonClicked()
    {
        bool bideSuccessful = hero.UseBide();

        if (bideSuccessful)
        {
            attackOptionsParent.SetActive(false);
            StartTurn();
        }
    }

    private IEnumerator DoBossAttackRoutine(Enemy targetEnemy)
    {
        yield return new WaitForSeconds(1f);

        hero.DoAttackAnimation();

        int damage = hero.GetDamage();
        targetEnemy.TakeDamage(damage);
        Debug.Log($"Hero attacked {targetEnemy.name}, dealing {damage} damage.");
        if (combinedAttack.attributes.Contains("Burn"))
            {
                if (Random.value <= 0.3f) 
                {
                    targetEnemy.ApplyBurn(10, 3);
                    Debug.Log($"Hero has burned {targetEnemy.name}!");
                }
            }

        if (combinedAttack.attributes.Contains("Paralysis"))
            {
               
                if (Random.value <= 1f) 
                {
                    targetEnemy.ApplyParalysis(5, true); 
                    Debug.Log($"Hero has paralyzed by {targetEnemy.name}!");
                }
            }

        if (combinedAttack.attributes.Contains("Heal"))
            {
               
                
                    hero.currentHealth += damage; 
                    Debug.Log($"Health is no {hero.currentHealth}!");
            
            }


        if (targetEnemy.currentHealth <= 0)
        {
            Debug.Log($"{targetEnemy.name} has been defeated!");
            RemoveEnemy(targetEnemy);
        }

        yield return new WaitForSeconds(1f);

        StartTurn();
    }



    public void RemoveEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        if (enemies.Count == 0)
        {
            EndGame(true); // Player wins
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
        }
        else
        {
            loseScreen.SetActive(true);
        }
    }
}
