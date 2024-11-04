using UnityEngine;

public class ParalysisEffect
{
    public int turnsRemaining;
    public bool isActivatedThisTurn;

    public ParalysisEffect(int duration)
    {
        turnsRemaining = duration;
        isActivatedThisTurn = false;
    }


    public void CheckForActivation()
    {
        if (turnsRemaining > 0)
        {
            isActivatedThisTurn = Random.value <= 0.6f;
            turnsRemaining--;
        }
        else
        {
            isActivatedThisTurn = false; 
        }
    }

    public bool IsEffectActive()
    {
        return turnsRemaining > 0;
    }
}
