using UnityEngine;

public class ParalysisEffect
{
    public int TurnsRemaining => turnsRemaining;
    public bool IsActivatedThisTurn => isActivatedThisTurn;

    private int turnsRemaining;
    private bool isActivatedThisTurn;

    private const float PARALYSIS_EFFECT_CHANCE = .15f;
    private const float PARALYSIS_EFFECT_ENHANCED_CHANCE = .6f;

    private bool isEnhancedParalysis = false;

    public ParalysisEffect(int duration, bool enhanced)
    {
        isEnhancedParalysis = enhanced;
        turnsRemaining = duration;
        isActivatedThisTurn = Random.value <= (isEnhancedParalysis ? PARALYSIS_EFFECT_ENHANCED_CHANCE : PARALYSIS_EFFECT_CHANCE);
    }


    public void CheckForActivation()
    {
        if (turnsRemaining > 0)
        {
            isActivatedThisTurn = Random.value <= (isEnhancedParalysis ? PARALYSIS_EFFECT_ENHANCED_CHANCE : PARALYSIS_EFFECT_CHANCE);
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
