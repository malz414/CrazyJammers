using UnityEngine;

public class BurnEffect
{
    public int damagePerTurn;
    public int duration;
    public int turnsRemaining;

    public BurnEffect(int damagePerTurn, int duration)
    {
        this.damagePerTurn = damagePerTurn;
        this.duration = duration;
        this.turnsRemaining = duration;
    }

    
    public void ApplyBurn(Character target)
    {
        if (turnsRemaining > 0)
        {
            
            turnsRemaining--;
        }
    }
}
