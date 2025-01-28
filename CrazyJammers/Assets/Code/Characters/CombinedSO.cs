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
{ "Slash, Fireball", "Firesword" },

{ "Arrow Bolt, Slash", "Shock Blade" },
{ "Slash, Arrow Bolt", "Shock Blade" },

{ "Lunge, Slash", "Jab" },
{ "Slash, Lunge", "Jab" },

{ "Heal, Slash", "Zap" },
{ "Slash, Heal", "Zap" },

{ "Barrier, Slash", "Sword Field" },
{ "Slash, Barrier", "Sword Field" },

{ "Fireball, Arrow Bolt", "Fire Blade" },
{ "Arrow Bolt, Fireball", "Fire Blade" },

{ "Fireball, Lunge", "Flame Jolt" },
{ "Lunge, Fireball", "Flame Jolt" },

{ "Fireball, Barrier", "Flame Gate" },
{ "Barrier, Fireball", "Flame Gate" },

{ "Fireball, Heal", "Flame Drain" },
{ "Heal, Fireball", "Flame Drain" },

{ "Arrow Bolt, Lunge", "Bolt Jab" },
{ "Lunge, Arrow Bolt", "Bolt Jab" },

{ "Heal, Lunge", "Lunge Drain" },
{ "Lunge, Heal", "Lunge Drain" },

{ "Barrier, Arrow Bolt", "Shielding Arrow" },
{ "Arrow Bolt, Barrier", "Shielding Arrow" },

{ "Barrier, Lunge", "Spear Shield" },
{ "Lunge, Barrier", "Spear Shield" },

{ "Heal, Arrow Bolt", "Healing Bolt" },
{ "Arrow Bolt, Heal", "Healing Bolt" },

// New Combinations with Ice Bullet
{ "Ice Bullet, Slash", "Frostsword" },
{ "Slash, Ice Bullet", "Frostsword" },

{ "Ice Bullet, Fireball", "Steam Bullets" },
{ "Fireball, Ice Bullet", "Steam Bullets" },

{ "Ice Bullet, Arrow Bolt", "Ice Arrow" },
{ "Arrow Bolt, Ice Bullet", "Ice Arrow" },

{ "Ice Bullet, Lunge", "Bulltet Jab" },
{ "Lunge, Ice Bullet", "Bulltet Jab" },

{ "Ice Bullet, Heal", "Frosty Heal" },
{ "Heal, Ice Bullet", "Frosty Heal" },

{ "Ice Bullet, Barrier", "Frost Field" },
{ "Barrier, Ice Bullet", "Frost Field" },

{ "Ice Bullet, Steady Blade", "Steady Bullets" },
{ "Steady Blade, Ice Bullet", "Steady Bullets" },

{ "Ice Bullet, Healing Field", "Pure Snow" },
{ "Healing Field, Ice Bullet", "Pure Snow" },

{ "Heal, Barrier", "Holy Shield" },
{ "Barrier, Heal", "Holy Shield" },

{ "Steady Blade, Slash", "Great Sword" },
{ "Slash, Steady Blade", "Great Sword" },

{ "Steady Blade, Fireball", "Flame Blade" },
{ "Fireball, Steady Blade", "Flame Blade" },

{ "Steady Blade, Arrow Bolt", "Steady Arrow" },
{ "Arrow Bolt, Steady Blade", "Steady Arrow" },

{ "Steady Blade, Lunge", "Steady Jab" },
{ "Lunge, Steady Blade", "Steady Jab" },

{ "Steady Blade, Heal", "Steady Drain" },
{ "Heal, Steady Blade", "Steady Drain" },

{ "Steady Blade, Barrier", "Steady Barrier" },
{ "Barrier, Steady Blade", "Steady Barrier" },

{ "Steady Blade, Healing Field", "Steady Field" },
{ "Healing Field, Steady Blade", "Steady Field" },

{ "Healing Field, Slash", "Healing Sword" },
{ "Slash, Healing Field", "Healing Sword" },

{ "Healing Field, Fireball", "Flame Aura" },
{ "Fireball, Healing Field", "Flame Aura" },

{ "Healing Field, Arrow Bolt", "Regen Bolt" },
{ "Arrow Bolt, Healing Field", "Regen Bolt" },

{ "Healing Field, Lunge", "S Jab" },
{ "Lunge, Healing Field", "S Jab" },

{ "Healing Field, Heal", "Enhanced Healing" },
{ "Heal, Healing Field", "Enhanced Healing" },

{ "Healing Field, Barrier", "Sanctuary" },
{ "Barrier, Healing Field", "Sanctuary" },

{ "Triple Shot, Fireball", "Blazing Shots" },
{ "Fireball, Triple Shot", "Blazing Shots" },

{ "Triple Shot, Barrier", "Defensive Shots" },
{ "Barrier, Triple Shot", "Defensive Shots" },

{ "Triple Shot, Heal", "Healing Shots" },
{ "Heal, Triple Shot", "Healing Shots" },

{ "Triple Shot, Healing Field", "Restoring Volley" },
{ "Healing Field, Triple Shot", "Restoring Volley" },

{ "Triple Shot, Slash", "Cutting Barrage" },
{ "Slash, Triple Shot", "Cutting Barrage" },

{ "Triple Shot, Lunge", "Piercing Volley" },
{ "Lunge, Triple Shot", "Piercing Volley" },

{ "Triple Shot, Arrow Bolt", "Precision Volley" },
{ "Arrow Bolt, Triple Shot", "Precision Volley" },

{ "Triple Shot, Ice Bullet", "Frost Volley" },
{ "Ice Bullet, Triple Shot", "Frost Volley" },

{ "Triple Shot, Steady Blade", "Stable Shots" },
{ "Steady Blade, Triple Shot", "Stable Shots" },

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
        upgradeLevel = Mathf.Max(attack1.upgradeLevel + attack2.upgradeLevel);

        attributes.Clear(); // Clear previous attributes if necessary
        attributes.AddRange(attack1.attributes);
        attributes.AddRange(attack2.attributes);
    }

    public int GetDamage()
    {
        return baseDamage + (upgradeLevel * 5); 
    }
}
