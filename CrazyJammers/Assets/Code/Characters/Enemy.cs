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
    if (possibleAttacks.Count == 0) return null;

    // 1. Efficiently grab the list of living enemies directly from your TurnManager
    List<Enemy> activeEnemies = TurnManager.Instance.aliveEnemies.Where(e => !e.dead).ToList();

    // 2. Evaluate the board state
    bool isAnyoneBelow20 = activeEnemies.Any(e => e.currentHealth <= (e.maxHealth * 0.2f));
    bool allEnemiesFullHealth = activeEnemies.All(e => e.currentHealth >= e.maxHealth);

    // Default to all possible attacks
    List<AttackSO> validAttacks = new List<AttackSO>(possibleAttacks);

    // 3. Smart AI Filtering based on Attributes (not names)
    if (isAnyoneBelow20)
    {
        // Panic Mode: Try to filter down to ONLY healing moves
        List<AttackSO> healingAttacks = possibleAttacks.Where(a => a.attributes.Contains("Heal") || a.attributes.Contains("Field")).ToList();
        
        // If this enemy actually owns a healing move, restrict their choices to ONLY heals
        if (healingAttacks.Count > 0)
        {
            validAttacks = healingAttacks; 
        }
    }
    else if (allEnemiesFullHealth)
    {
        // Chill Mode: Remove healing moves so we don't waste them
        validAttacks = possibleAttacks.Where(a => !a.attributes.Contains("Heal") && !a.attributes.Contains("Field")).ToList();
        
        // Failsafe: Just in case an enemy *only* has healing moves, don't leave the list empty
        if (validAttacks.Count == 0) validAttacks = possibleAttacks; 
    }

    // 4. Roll the dice from our intelligently filtered list
    int randomIndex = Random.Range(0, validAttacks.Count);
    AttackSO chosenAttack = validAttacks[randomIndex];

    // 5. Execute
    int damage = Mathf.RoundToInt(chosenAttack.GetDamage() * multiplier);
    attacksUsed.Add(chosenAttack);
    DoAttackAnimation();
    
    return chosenAttack; 
}


    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }
}
