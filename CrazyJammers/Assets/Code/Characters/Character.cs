using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using DamageNumbersPro;

public abstract class Character : MonoBehaviour
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;
    public int barrierCount;
    public int burning;
    public bool bideBuff = false;
    public float bideDefenseMultiplier = 0.90f;
    public GameObject burnIcon;
    public GameObject paraIcon;

    public List<BurnEffect> activeBurns = new List<BurnEffect>();
    public ParalysisEffect paralysisEffect;
    private GameplayBlurbEvent blurbEvent;

    [SerializeField] public Animator animator;

    private const float DAMAGE_ANIM_DELAY_DURATION = .35f;
    private CharacterStatusUpdateEvent statusUpdateEvent;
    private DamageNumber popupPrefab;
    private const float DAMAGE_POPUP_LIFETIME = 1.5f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        blurbEvent = new GameplayBlurbEvent();
        statusUpdateEvent = new CharacterStatusUpdateEvent();
    }

    public void Init(DamageNumber popupPrefab)
    {
        this.popupPrefab = popupPrefab;
    }

    public virtual void TakeDamage(int damage)
    {
        int incomingDamage = damage;
        bool barrierUsed = false;
        bool bideDefUsed = false;
        float appliedMultiplier = 1f;

        if (barrierCount > 0 && bideBuff)
        {
            barrierCount--;
            bideBuff = false;
            barrierUsed = true;
            bideDefUsed = true;
            
            appliedMultiplier = Mathf.Max(0f, bideDefenseMultiplier - 0.20f);
            damage = Mathf.RoundToInt(damage * appliedMultiplier);  
            currentHealth -= damage;
        }
        else if (barrierCount > 0)
        {
            barrierCount--;
            barrierUsed = true;
            appliedMultiplier = 0.80f;
            
            damage = Mathf.RoundToInt(damage * 0.80f); 
            currentHealth -= damage;
        }
        else if (bideBuff)
        {
            bideDefUsed = true;
            appliedMultiplier = bideDefenseMultiplier;
            
            damage = Mathf.RoundToInt(damage * bideDefenseMultiplier); 
            currentHealth -= damage;
        }
        else
        {
            currentHealth -= damage;
        }

        Debug.Log($"[DEFENSE DIAGNOSTIC] {gameObject.name} | Incoming Dmg: {incomingDamage} | Barrier Applied: {barrierUsed} | Bide Defense Applied: {bideDefUsed} | Mitigation Multiplier: {appliedMultiplier} | Final Dmg Taken: {damage}");

        if (currentHealth <= 0)
        {
            Die();
        }
        StartCoroutine(DoHitRoutine(damage));
    }

    public IEnumerator CallDoHitRoutine(int damage, float delay, GameObject gam)
    {
        yield return new WaitForSeconds(delay);

        DamageNumber newPopup = popupPrefab.Spawn(gam.transform.position + new Vector3(0, 0.25f, -1), damage);
        newPopup.permanent = false;
        newPopup.lifetime = DAMAGE_POPUP_LIFETIME;
        
        if (currentHealth <= 0)
        {
            DoDeathAnimation();
        } 
        else
        {
            DoHitAnimation();
        }

        EventBus.Publish(statusUpdateEvent);
    }

    public virtual void HealDamage(int damage)
    {
        currentHealth += damage;
        StartCoroutine(DoHealRoutine(damage));
    }

    public void RemoveBurns()
    {
        this.burning = 0;
        burnIcon.SetActive(false);
    }

    public void RemoveParalysis()
    {
        paralysisEffect = null;  
        paraIcon.SetActive(false);
    }

    public void DoAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    private IEnumerator DoHitRoutine(int damage)
    {
        yield return new WaitForSeconds(DAMAGE_ANIM_DELAY_DURATION);

        DamageNumber newPopup = popupPrefab.Spawn(gameObject.transform.position + new Vector3(0, 0.25f, -1), damage);
        newPopup.permanent = false;
        newPopup.lifetime = DAMAGE_POPUP_LIFETIME;

        if (currentHealth <= 0)
        {
            DoDeathAnimation();
        } 
        else
        {
            DoHitAnimation();
        }

        EventBus.Publish(statusUpdateEvent);
    }

    private IEnumerator DoHealRoutine(int damage)
    {
        yield return new WaitForSeconds(0f);

        DamageNumber newPopup = popupPrefab.Spawn(gameObject.transform.position + new Vector3(0, 0.25f, -1), damage);
        newPopup.permanent = false;
        newPopup.lifetime = DAMAGE_POPUP_LIFETIME;

        EventBus.Publish(statusUpdateEvent);
    }

    public void DoHitAnimation()
    {
        animator.Play("Hit", 0, 0f);
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
        burning = 4;
        burnIcon.SetActive(true);
    }

    public void ApplyParalysis(int duration, bool enhanced)
    {
        paralysisEffect = new ParalysisEffect(duration, enhanced);
        paraIcon.SetActive(true);
    }

    public bool CanAct()
    {
        return paralysisEffect == null || !paralysisEffect.IsActivatedThisTurn;
    }

    public void UpdateEffects()
    {
        foreach (var burn in activeBurns.ToArray())
        {
            burn.ApplyBurn(this);
            if (burn.turnsRemaining <= 0)
            {
                activeBurns.Remove(burn);
                burnIcon.SetActive(false);
            }
        }

        paralysisEffect?.CheckForActivation();
        if(paralysisEffect != null) 
        {
            if(!paralysisEffect.IsEffectActive())
            {
                RemoveParalysis();
            }
        }
    }
}