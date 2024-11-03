using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;

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
    }

    protected abstract void Die();

    public virtual bool IsAlive()
    {
        return currentHealth > 0;
    }
}
