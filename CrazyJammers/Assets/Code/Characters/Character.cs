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

        //Dont need activeburns
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
            
          if(barrierCount > 0 && bideBuff)
            {
                barrierCount--;
                bideBuff = false;
                damage = Mathf.RoundToInt(damage * 0.70f);  
                currentHealth -= damage;
            }
            else if(barrierCount > 0)
            {
                barrierCount--;
                damage = Mathf.RoundToInt(damage * 0.80f);  
                currentHealth -= damage;
            }
            else if(bideBuff)
            {
                damage = Mathf.RoundToInt(damage * 0.90f); 
                currentHealth -= damage;
            }
            else
            {
                currentHealth -= damage;
            }


            if (currentHealth <= 0)
            {
                Die();
            }
            //Barrier Animation?
            StartCoroutine(DoHitRoutine(damage));
        }

    

        public virtual void HealDamage(int damage)
        {
         
                currentHealth += damage;
            

            //Barrier Animation?
            StartCoroutine(DoHealRoutine(damage));
        }

        public void RemoveBurns()
        {
            this.burning = 0;
        }
        public void RemoveParalysis()
        {
            paralysisEffect = null;  
        }
        public void DoAttackAnimation()
        {
            animator.SetTrigger("Attack");
            Debug.Log(this.name);
        }

        private IEnumerator DoHitRoutine(int damage)
        {
            yield return new WaitForSeconds(DAMAGE_ANIM_DELAY_DURATION);

            DamageNumber newPopup = popupPrefab.Spawn(gameObject.transform.position + new Vector3(0, 0.25f, -1), damage); //Spawn DamageNumber     <-----     [REQUIRED]
            // newPopup.color = yellow;
            //var newpreset = Resources.Load<DamageNumberSettings>("Blood Thick");
            //newPopup.settings = newpreset; 
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

            DamageNumber newPopup = popupPrefab.Spawn(gameObject.transform.position + new Vector3(0, 0.25f, -1), damage); //Spawn DamageNumber     <-----     [REQUIRED]
            // newPopup.color = green;
            //var newpreset = Resources.Load<DamageNumberSettings>("Blood Thick");
            //newPopup.settings = newpreset; 
            newPopup.permanent = false;
            newPopup.lifetime = DAMAGE_POPUP_LIFETIME;

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


        //DAMAGEPER TURN AND DURATION DONT MATTER
        public void ApplyBurn(int damagePerTurn, int duration)
        {
            // BurnEffect burnEffect = new BurnEffect(damagePerTurn, duration);
            // activeBurns.Add(burnEffect);
            burning = 4;
        }

        public void ApplyParalysis(int duration, bool enhanced)
        {
            paralysisEffect = new ParalysisEffect(duration, enhanced);
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