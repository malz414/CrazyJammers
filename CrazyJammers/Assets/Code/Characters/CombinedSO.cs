using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombinedAttack", menuName = "ScriptableObjects/CombinedAttack")]
public class CombinedAttackSO : ScriptableObject
{
    public string attackName;
    public int baseDamage;
    public int upgradeLevel;
    public List<string> attributes = new List<string>();

    // Dictionary to hold attack name mappings based on attack types
    private static readonly Dictionary<string, string> attackNameMapping = new Dictionary<string, string>
    {
        { "Fireball, Slash", "Firesword" },
        { "Arrow Bolt, Slash", "Shock Blade" },
        { "Lunge, Slash", "Jab" },
        { "Heal, Slash", "Zap" },
        { "Barrier, Slash", "Sword Field" },
        //NOTE FIRE BLADE DOESNT WORK IDK WHY
        { "Fireball, Arrow Bolt", "Fire Blade" },
        { "Arrow Bolt, Fireball", "Fire Blade" },
        
        { "Fireball, Lunge", "Flame Jolt" },
        { "Fireball, Barrier", "Flame Gate" },
        { "Fireball, Heal", "Flame Drain" },
        { "Arrow Bolt, Lunge", "Bolt Jab" },
        { "Heal, Lunge", "Lunge Drain" },
        { "Barrier, Arrow Bolt", "Shielding Arrow" },
        { "Heal, Arrow Bolt", "Healing Bolt" }
        
    };

    // Method to combine two attacks
    public void Combine(AttackSO attack1, AttackSO attack2)
    {
        string[] attacks = new[] { attack1.attackName, attack2.attackName };
        System.Array.Sort(attacks);
        string key = string.Join(", ", attacks);

        

        if (attackNameMapping.TryGetValue(key, out string combinedName))
        {
            attackName = combinedName; 
        }
        else
        {
            attackName = $"{attack1.attackName} + {attack2.attackName}"; // Fallback to default name
            Debug.Log($"No combination found for key: {key}. Using fallback: {attackName}"); // Log if no combination is found
        }

        baseDamage = attack1.GetDamage() + attack2.GetDamage();
        upgradeLevel = Mathf.Max(attack1.upgradeLevel, attack2.upgradeLevel);

        attributes.Clear(); // Clear previous attributes if necessary
        attributes.AddRange(attack1.attributes);
        attributes.AddRange(attack2.attributes);
    }

    public int GetDamage()
    {
        return baseDamage + (upgradeLevel * 5); 
    }
}
