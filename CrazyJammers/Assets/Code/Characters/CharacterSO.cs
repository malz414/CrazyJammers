using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Battle/Character")]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public int maxHealth;
    public List<AttackSO> availableAttacks;
}
