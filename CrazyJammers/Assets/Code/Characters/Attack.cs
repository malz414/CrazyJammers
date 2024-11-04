using UnityEngine;
using System.Collections.Generic;

public class Attack
{
    public string Name { get; private set; }
    public int Damage { get; private set; }
    public List<string> Attributes { get; private set; }

    public Attack(string name, int damage, List<string> attributes)
    {
        Name = name;
        Damage = damage;
        Attributes = attributes;
    }
}
