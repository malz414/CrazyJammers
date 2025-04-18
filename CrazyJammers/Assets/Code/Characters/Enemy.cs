using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : Character
{
    public List<AttackSO> possibleAttacks = new List<AttackSO>();
    public List<AttackSO> attacksUsed = new List<AttackSO>();
    public bool dead;   
    private bool shouldMove = false;
    private float moveThreshold = 0.65f;
    public float walkSpeed = 2f;
    public float multiplier = 1f;
    public string enemyType;
    private AudioClip walkingClip;
    private AudioSource walkingAudioSource;
    //public CharacterSO characterData;

    [SerializeField] public GameObject TargetingIndicator;

    private void Awake()
    {
        //characterName = characterData.characterName;
        //maxHealth = characterData.maxHealth;
        currentHealth = maxHealth; 
    }
    void Start()
    {
        
        
        animator = this.GetComponent<Animator>();

        // Get current animation clip name on spawn
        string currentClipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        // If the clip name contains "Walk", we want to start moving
  
        walkingAudioSource = this.GetComponent<AudioSource>();
        if (walkingAudioSource == null)
        {
            walkingAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (walkingClip == null)
        {
            walkingClip = Resources.Load<AudioClip>("Audio/Walking");
            Debug.Log("Loaded walking clip: " + walkingClip);
        }

        walkingAudioSource.clip = walkingClip;
        walkingAudioSource.time = 2f; 
        walkingAudioSource.loop = true;
        walkingAudioSource.playOnAwake = false;
        walkingAudioSource.spatialBlend = 0f;
        walkingAudioSource.volume = 0.7f;

        // Add pitch variation
        walkingAudioSource.pitch = Random.Range(.95f, 1.05f);
        if (currentClipName.Contains("Walking"))
        {
            shouldMove = true;
            walkingAudioSource.Play(); 
        }

    }


    public void OnMouseOverr()
    {
        if (dead)
            return;

        TurnManager.Instance.SelectEnemyToAttack(this);

        //TargetingIndicator.SetActive(false);
        return;

    }
    public int GetParalysisTurnsRemaining()
    {
        return paralysisEffect != null ? paralysisEffect.TurnsRemaining : 0;
    }

     void Update()
    {
        if (shouldMove)
        {
            transform.position += transform.forward * walkSpeed * Time.deltaTime;

            // Stop moving when the animation is almost finished (95% of the animation)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= moveThreshold && !animator.IsInTransition(0))
            {
                shouldMove = false;
                walkingAudioSource.Stop();
            }
        }
    }

    void OnMouseExit()
    {
//        TargetingIndicator.SetActive(false);
    }

    protected override void Die()
    {
        Debug.Log($"{characterName} has been defeated.");
        dead = true;
        TurnManager.Instance.RemoveEnemy(this);

    }

    public AttackSO PerformRandomAttack()
    {
        if (possibleAttacks.Count > 0)
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        Debug.Log(allEnemies[1]);
        bool allEnemiesFullHealth = allEnemies
            .Where(enemy => !enemy.dead) // Ignore dead enemies
            .All(enemy => enemy.IsFullHealth());

        List<AttackSO> validAttacks = allEnemiesFullHealth 
            ? possibleAttacks.Where(attack => attack.attackName != "Heal" && attack.attackName != "Healing Field" ).ToList() 
            : possibleAttacks;

        if (validAttacks.Count > 0)
        {
      
            int randomIndex = Random.Range(0, validAttacks.Count);
            AttackSO chosenAttack = validAttacks[randomIndex];
            int damage = Mathf.RoundToInt(chosenAttack.GetDamage() * multiplier);
            attacksUsed.Add(chosenAttack);
            DoAttackAnimation();
            return chosenAttack; 
        }
        Debug.LogError($"No available attacks for enemy: {gameObject.name}");
       
    }
     return null;
}


    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }
}
