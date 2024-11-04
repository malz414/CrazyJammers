using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class Character : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;
    private List<BurnEffect> activeBurns = new List<BurnEffect>();
    private ParalysisEffect paralysisEffect;

    [SerializeField] private Animator animator;

    private const float DAMAGE_ANIM_DELAY_DURATION = .35f;

    private CharacterStatusUpdateEvent statusUpdateEvent;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        statusUpdateEvent = new CharacterStatusUpdateEvent();
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(DoHitRoutine());
    }

    public void DoAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    private IEnumerator DoHitRoutine()
    {
        yield return new WaitForSeconds(DAMAGE_ANIM_DELAY_DURATION);

        if(currentHealth <= 0)
        {
            DoDeathAnimation();
        } else
        {
            DoHitAnimation();
        }

        EventBus.Publish(statusUpdateEvent);

    }

    public void DoHitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    public void DoDeathAnimation()
    {
        animator.SetTrigger("Die");
    }


    protected abstract void Die();

    public virtual bool IsAlive()
    {
        return currentHealth > 0;
    }

     public void ApplyBurn(int damagePerTurn, int duration)
    {
        BurnEffect burnEffect = new BurnEffect(damagePerTurn, duration);
        activeBurns.Add(burnEffect);
    }

       public void ApplyParalysis(int duration)
    {
        paralysisEffect = new ParalysisEffect(duration);
    }

    public bool CanAct()
    {
        return paralysisEffect == null || !paralysisEffect.IsActivatedThisTurn;
    }

    
    public void UpdateEffects()
    {
    
        paralysisEffect?.CheckForActivation();

        foreach (var burn in activeBurns.ToArray())
        {
            burn.ApplyBurn(this);
            if (burn.turnsRemaining <= 0)
            {
                activeBurns.Remove(burn);
            }
        }
    }
    
}
