using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; } 
    public Hero hero;
    public List<Enemy> enemies;
    public GameObject attackSelectionPanel; 
    public Button[] attackButtons; 
    public Button bideButton;
    public AttackSO selectedAttack1;
    public AttackSO selectedAttack2;

    private List<AttackSO> enemyAttacksUsed; 

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
        StartTurn();
    }

    public void StartTurn()
    {
        if (hero.currentHealth <= 0 || enemies.Count == 0)
        {
            EndGame(hero.currentHealth > 0);
            return;
        }

        // Clear previous attacks at the start of each turn
        enemyAttacksUsed = new List<AttackSO>();

        // Enemies attack hero
        foreach (var enemy in enemies)
        {
            AttackSO enemyAttack = enemy.PerformRandomAttack();
            enemyAttacksUsed.Add(enemyAttack);
            hero.TakeDamage(enemyAttack.GetDamage());
            Debug.Log($"Enemy {enemy.name} used {enemyAttack.attackName}, dealing {enemyAttack.GetDamage()} damage to the hero.");
        }

        // Show attack selection UI for the hero
        ShowAttackSelectionUI();
    }

    private void ShowAttackSelectionUI()
    {
        attackSelectionPanel.SetActive(true);

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

        // Set up the Bide button
        bideButton.gameObject.SetActive(true);
        bideButton.onClick.RemoveAllListeners();
        bideButton.onClick.AddListener(OnBideButtonClicked);
        bideButton.GetComponentInChildren<TextMeshProUGUI>().text = "Bide";
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
            heroAttackEnemy();

            enemyAttacksUsed.Clear();
            StartTurn();

         
        }
    }
    
    private void OnBideButtonClicked()
    {
        bool bideSuccessful = hero.UseBide();

        if (bideSuccessful)
        {
            attackSelectionPanel.SetActive(false);
            StartTurn();
        }
    }

    private void heroAttackEnemy()
    {
        if (enemies.Count > 0)
        {
            Enemy targetEnemy = enemies[0]; 
            int damage = hero.GetDamage(); 
            targetEnemy.TakeDamage(damage);
            Debug.Log($"Hero attacked {targetEnemy.name}, dealing {damage} damage.");

            if (targetEnemy.currentHealth <= 0)
            {
                Debug.Log($"{targetEnemy.name} has been defeated!");
                RemoveEnemy(targetEnemy);
            }
        }
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
    }
}
