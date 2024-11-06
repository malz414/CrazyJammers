using UnityEngine;
using System.Collections.Generic;

public class Hero : Character
{
    public CombinedAttackSO currentAttack;
    public List<AttackSO> allAttacks;
    public float bideLevel = 1.0f;
    public int bideUses = 0; 
  
    
    protected override void Start()
    {
        base.Start();
        allAttacks = new List<AttackSO>(); 
    }

    public void CombineAttacks(AttackSO attack1, AttackSO attack2)
    {
        currentAttack.Combine(attack1, attack2);
    }

    public void RemoveHeroBurns()
    {
        this.burning=0;
    }

    public void RemoveHeroParalysis()
    {
        paralysisEffect = null;  
    }

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

    // Method to calculate damage
    public int GetDamage()
    {
        if (currentAttack != null)
        {
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
