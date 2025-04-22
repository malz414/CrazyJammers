using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : Character
{
    public CombinedAttackSO currentAttack;
    public List<AttackSO> allAttacks;
    public float bideLevel = 1.0f;
    public int bideUses = 0; 
    public bool dead = false;
    public float originalY;
    public Coroutine lerpCoroutine;
  

    protected override void Start()
    {
        base.Start();
        originalY = transform.position.y;
        allAttacks = new List<AttackSO>(); 
    }

    public void CombineAttacks(AttackSO attack1, AttackSO attack2)
    {
        currentAttack.Combine(attack1, attack2);
    }

    public void RemoveHeroBurns()
    {
        this.burning = 0;
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
            Debug.Log($"Bide used. Current bide level: {bideLevel}");
            return true;
        }
        else
        {
            Debug.Log("Bide can only be used twice.");
            return false;
        }
    }

    // Method to calculate damage
    public int GetDamage()
    {
        if (currentAttack != null)
        {
            int damage = (currentAttack.GetDamage());
              return damage;
              
        }
        return 0;
    }
    

    protected override void Die()
    {
        dead = true;
        Debug.Log($"{characterName} has been defeated!");
        
        if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
        lerpCoroutine = StartCoroutine(LerpYPosition(transform.position.y, transform.position.y - 1f, .5f, .5f)); // sink 2 units down
        
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

}