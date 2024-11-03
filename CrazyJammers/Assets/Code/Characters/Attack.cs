using UnityEngine;

[System.Serializable]
public class Attack
{
    public string attackName;
    public int power;

    // Executes the attack, dealing damage to the target
    public void Execute(Character target)
    {
        target.TakeDamage(power);
        Debug.Log($"{attackName} dealt {power} damage to {target.characterName}");
    }

    // Combines two attacks to create a stronger one
    public static Attack CombineAttacks(Attack attack1, Attack attack2)
    {
        return new Attack
        {
            attackName = $"{attack1.attackName}-{attack2.attackName}",
            power = attack1.power + attack2.power
        };
    }
}
