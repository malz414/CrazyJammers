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
    // Existing Combinations
    { "Fireball, Slash", "Firesword" },
    { "Arrow Bolt, Slash", "Shock Blade" },
    { "Lunge, Slash", "Jab" },
    { "Heal, Slash", "Zap" },
    { "Barrier, Slash", "Sword Field" },
    { "Fireball, Arrow Bolt", "Fire Blade" },
    { "Arrow Bolt, Fireball", "Fire Blade" },
    { "Fireball, Lunge", "Flame Jolt" },
    { "Fireball, Barrier", "Flame Gate" },
    { "Fireball, Heal", "Flame Drain" },
    { "Arrow Bolt, Lunge", "Bolt Jab" },
    { "Heal, Lunge", "Lunge Drain" },
    { "Barrier, Arrow Bolt", "Shielding Arrow" },
    { "Barrier, Lunge", "Spear Shield" },
    { "Heal, Arrow Bolt", "Healing Bolt" },
    
    // New Combinations with Ice Bullet
    { "Ice Bullet, Slash", "Frostsword" },
    { "Ice Bullet, Fireball", "Steam Bullets" },
    { "Ice Bullet, Arrow Bolt", "Ice Arrow" },
    { "Ice Bullet, Lunge", "Bulltet Jab" },
    { "Ice Bullet, Heal", "Frosty Heal" },
    { "Ice Bullet, Barrier", "Frost Field" },
    { "Ice Bullet, Steady Blade", "Steady Bullets" },
    { "Ice Bullet, Healing Field", "Pure Snow" },
    { "Heal, Barrier", "Holy Shield" },

    // New Combinations with Steady Blade
    { "Steady Blade, Slash", "Great Sword" },
    { "Steady Blade, Fireball", "Flame Blade" },
    { "Steady Blade, Arrow Bolt", "Steady Arrow" },
    { "Steady Blade, Lunge", "Steady Jab" },
    { "Steady Blade, Heal", "Steady Drain" },
    { "Steady Blade, Barrier", "Steady Barrier" },
    { "Steady Blade, Healing Field", "Steady Field" },

    // New Combinations with Healing Field
    { "Healing Field, Slash", "Great Sword" },
    { "Healing Field, Fireball", "Flame Aura" },
    { "Healing Field, Arrow Bolt", "Regen Bolt" },
    { "Healing Field, Lunge", "S Jab" },
    { "Healing Field, Heal", "Enhanced Healing" },
    { "Healing Field, Barrier", "Sanctuary" },

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
