using UnityEngine;

public class ParalysisEffect
{
    public int TurnsRemaining => turnsRemaining;
    public bool IsActivatedThisTurn => isActivatedThisTurn;

    private int turnsRemaining;
    private bool isActivatedThisTurn;

    private const float PARALYSIS_EFFECT_CHANCE = .15f;

    public ParalysisEffect(int duration)
    {
        turnsRemaining = duration;
        isActivatedThisTurn = Random.value <= PARALYSIS_EFFECT_CHANCE;
    }


    public void CheckForActivation()
    {
        if (turnsRemaining > 0)
        {
            isActivatedThisTurn = Random.value <= PARALYSIS_EFFECT_CHANCE;
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
