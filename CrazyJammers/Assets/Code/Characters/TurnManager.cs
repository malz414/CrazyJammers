using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Code.Utility.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;

    [SerializeField] private Transform[] enemySpawns;

    [SerializeField] private GameObject bossPrefab;

    [SerializeField] private Transform bossSpawn;

    [SerializeField] GameObject attackOptionsParent;

    [SerializeField] GameObject targetingHUDParent;

    [SerializeField] GameObject winScreen;

    [SerializeField] GameObject loseScreen;

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

    private void Start()
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

        targetingMode = false;
        targetingHUDParent.SetActive(false);

        GameObject bossObj = SpawnPrefabAtPosition(bossPrefab, bossSpawn);

        hero = bossObj.GetComponent<Hero>();

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
            AttackSO enemyAttack = enemy.PerformRandomAttack();
            enemyAttacksUsed.Add(enemyAttack);
            hero.TakeDamage(enemyAttack.GetDamage());
            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, dealing {enemyAttack.GetDamage()} damage to the hero.");
            yield return new WaitForSeconds(1.5f);

            if(hero.currentHealth <= 0)
            {
                yield break;
            }

        }

        yield return new WaitForSeconds(1.5f);

        // Show attack selection UI for the hero
        ShowAttackSelectionUI();
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

            
            hero.RandomlySelectAttack();

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
