using UnityEngine;
using System.Collections.Generic;

public class Hero : Character
{
    public AttackSO currentAttack; 
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
        // Combine attacks logic
        AttackSO combinedAttack = ScriptableObject.CreateInstance<AttackSO>();
        combinedAttack.attackName = $"{attack1.attackName} + {attack2.attackName}";
        combinedAttack.baseDamage = attack1.GetDamage() + attack2.GetDamage();
        combinedAttack.upgradeLevel = Mathf.Max(attack1.upgradeLevel, attack2.upgradeLevel); 
        combinedAttack.attributes = new List<string>(attack1.attributes);
        combinedAttack.attributes.AddRange(attack2.attributes);

        if (!allAttacks.Contains(combinedAttack)) 
        {
            allAttacks.Add(combinedAttack);
            Debug.Log($"Combined attack created: {combinedAttack.attackName} with damage: {combinedAttack.GetDamage()}");
        }
    }

    public AttackSO RandomlySelectAttack()
    {
        if (allAttacks.Count > 0)
        {
            int randomIndex = Random.Range(0, allAttacks.Count);
            currentAttack = allAttacks[randomIndex];
            Debug.Log($"Randomly selected attack: {currentAttack.attackName} with damage: {currentAttack.GetDamage()}");
            return currentAttack;
        }
        else
        {
            Debug.Log("No attacks available to select.");
            currentAttack = null; 
            return null;
        }
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
       
    }
}
