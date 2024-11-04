using UnityEngine;
using System.Collections.Generic;

public class Hero : Character
{
    
    public CombinedAttackSO currentAttack;
    public List<AttackSO> allAttacks;
    public float bideLevel = 1.0f;
    private int bideUses = 0; 
    
    

    protected override void Start()
    {
        base.Start();
        allAttacks = new List<AttackSO>(); 
    }

    public void CombineAttacks(AttackSO attack1, AttackSO attack2)
    {
        currentAttack.Combine(attack1, attack2);
    }

    // public AttackSO RandomlySelectAttack()
    // {
    //     if (allAttacks.Count > 0)
    //     {
    //         int randomIndex = Random.Range(0, allAttacks.Count);
    //         currentAttack = allAttacks[randomIndex];
    //         Debug.Log($"Randomly selected attack: {currentAttack.attackName} with damage: {currentAttack.GetDamage()}");
    //         return currentAttack;
    //     }
    //     else
    //     {
    //         Debug.Log("No attacks available to select.");
    //         currentAttack = null; 
    //         return null;
    //     }
    // }

    public bool UseBide()
    {
        if (bideUses < 2)
        {
            bideLevel += 0.5f;
            bideUses++;
            Debug.Log($"Bide used. Current bide level: {bideLevel}");
            return true;
        }
        else
        {
            Debug.Log("Bide can only be used twice.");
            return false;
        }
    }

    public int GetDamage()
    {
        if (currentAttack != null)
        {
            // Calculate the damage and round to an integer
            int damage = (int)(currentAttack.GetDamage() * bideLevel);
            Debug.Log($"Damage with bide applied: {damage} (Base: {currentAttack.GetDamage()}, Bide Level: {bideLevel})");
            return damage;
        }
        return 0;
    }

    protected override void Die()
    {
        Debug.Log($"{characterName} has been defeated!");
        TurnManager.Instance.EndGame(false);
       
    }
}
