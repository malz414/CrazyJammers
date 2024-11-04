using UnityEngine;

public class ParalysisEffect
{
    public int TurnsRemaining => turnsRemaining;
    public bool IsActivatedThisTurn => isActivatedThisTurn;

    private int turnsRemaining;
    private bool isActivatedThisTurn;

    public ParalysisEffect(int duration)
    {
        turnsRemaining = duration;
        isActivatedThisTurn = Random.value <= 0.6f;
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
