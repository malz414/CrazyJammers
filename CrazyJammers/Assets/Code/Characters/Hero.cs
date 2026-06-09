using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : Character
{
    public CombinedAttackSO currentAttack;
    public List<AttackSO> allAttacks;
    public float bideLevel = 1.0f;
    public float bideLevelBase = 1.0f;
    public int bideUses = 0; 
    public bool dead = false;
    public float originalY;
    public Coroutine lerpCoroutine;
    
    public Vector3 startPosition;
    public Quaternion startRotation;
  
    protected override void Start()
    {
        base.Start();
        originalY = transform.position.y;
        startPosition = transform.position;
        startRotation = transform.rotation;
        allAttacks = new List<AttackSO>(); 
    }

    public void CombineAttacks(AttackSO attack1, AttackSO attack2)
    {
        currentAttack.Combine(attack1, attack2);
    }

    public void RemoveHeroBurns()
    {
        this.burning = 0;
        if (burnIcon != null) burnIcon.SetActive(false);
    }

    public void RemoveHeroParalysis()
    {
        paralysisEffect = null;  
    }
    
    public int GetParalysisTurnsRemaining()
    {
        return paralysisEffect != null ? paralysisEffect.TurnsRemaining : 0;
    }

    public bool UseBide()
    {
        if (bideUses < 2)
        {
            bideLevel += 0.5f;
            bideUses++;
            return true;
        }
        return false;
    }

    public int GetDamage()
    {
        if (currentAttack != null)
        {
            return currentAttack.GetDamage();
        }
        return 0;
    }
    
    protected override void Die()
    {
        dead = true;
        
        if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
        lerpCoroutine = StartCoroutine(LerpYPosition(transform.position.y, transform.position.y - 1f, .5f, .5f)); 
        
        TurnManager.Instance.EndGame(false);
    }

    public IEnumerator LerpYPosition(float fromY, float toY, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float time = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, toY, startPos.z);

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

    public IEnumerator ResetTransformRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        float time = 0f;
        float duration = 0.25f;
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        while (time < duration)
        {
            float t = time / duration;
            transform.position = Vector3.Lerp(currentPos, new Vector3(startPosition.x, transform.position.y, startPosition.z), t);
            transform.rotation = Quaternion.Lerp(currentRot, startRotation, t);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = new Vector3(startPosition.x, transform.position.y, startPosition.z);
        transform.rotation = startRotation;
    }
}