using UnityEngine;
using System.Collections.Generic;

public class Enemy : Character
{
    public List<AttackSO> possibleAttacks = new List<AttackSO>();
    public List<AttackSO> attacksUsed = new List<AttackSO>(); 
    public CharacterSO characterData;
    
    private void Awake()
    {
        characterName = characterData.characterName;
        maxHealth = characterData.maxHealth;
        currentHealth = maxHealth; 
        }

    protected override void Die()
    {
        Debug.Log($"{characterName} has been defeated.");
        TurnManager.Instance.RemoveEnemy(this);
        Destroy(gameObject);
    }

    public AttackSO PerformRandomAttack()
    {
        if (possibleAttacks.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleAttacks.Count);
            AttackSO chosenAttack = possibleAttacks[randomIndex];
            int damage = chosenAttack.GetDamage();
            attacksUsed.Add(chosenAttack);
            return chosenAttack; 
        }
        return null;
    }
}
