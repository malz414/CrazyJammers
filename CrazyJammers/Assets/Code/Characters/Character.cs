using System.Collections;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;

    [SerializeField] private Animator animator;

    private const float DAMAGE_ANIM_DELAY_DURATION = .35f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
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
}
