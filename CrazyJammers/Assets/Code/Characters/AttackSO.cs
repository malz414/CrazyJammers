using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Battle/Attack")]
public class AttackSO : ScriptableObject
{
    public string attackName;
    public string attackDesctiption;
    public int baseDamage;
    public int upgradeLevel;
    public List<string> attributes;

    public int GetDamage()
    {
        return baseDamage + (upgradeLevel * 5); 
    }
}