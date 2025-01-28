using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class AttackSO : ScriptableObject
{
    public string attackName;
    public string attackDescription;
    public int baseDamage;
    public int upgradeLevel = 0;
    public List<string> attributes;

    public int GetDamage()
    {
        // float multiplier = 1.0f + (upgradeLevel/10f);   
        // return Mathf.RoundToInt(baseDamage * multiplier);
        return baseDamage;
    }
}