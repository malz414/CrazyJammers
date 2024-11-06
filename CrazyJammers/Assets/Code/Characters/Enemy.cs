using UnityEngine;
using System.Collections.Generic;

public class Enemy : Character
{
    public List<AttackSO> possibleAttacks = new List<AttackSO>();
    public List<AttackSO> attacksUsed = new List<AttackSO>();
    public bool dead;
    //public CharacterSO characterData;

    [SerializeField] public GameObject TargetingIndicator;

    private void Awake()
    {
        //characterName = characterData.characterName;
        //maxHealth = characterData.maxHealth;
        currentHealth = maxHealth; 
    }

    void OnMouseOver()
    {
        if (!TurnManager.Instance.TargetingMode || dead)
            return;

        if(Input.GetMouseButtonUp(0))
        {
            TurnManager.Instance.SelectEnemyTotAttack(this);

            TargetingIndicator.SetActive(false);
            return;
        }

        TargetingIndicator.SetActive(true);
    }

    void OnMouseExit()
    {
        TargetingIndicator.SetActive(false);
    }

    protected override void Die()
    {
        Debug.Log($"{characterName} has been defeated.");
        dead = true;
        TurnManager.Instance.RemoveEnemy(this);

    }

    public AttackSO PerformRandomAttack()
    {
        if (possibleAttacks.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleAttacks.Count);
            AttackSO chosenAttack = possibleAttacks[randomIndex];
            int damage = chosenAttack.GetDamage();
            attacksUsed.Add(chosenAttack);
            DoAttackAnimation();
            return chosenAttack; 
        }
        Debug.LogError($"No available attacks for enemy: {gameObject.name}");
        return null;
    }
}
