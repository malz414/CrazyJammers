using UnityEngine;

public class SpawnWalkMover : MonoBehaviour
{
    public float walkSpeed = 2f;
    private Animator animator;
    private bool shouldMove = false;
    private float moveThreshold = 0.65f; // Stop moving when animation reaches 95%

    void Start()
    {
        animator = GetComponent<Animator>();

        // Get current animation clip name on spawn
        string currentClipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        // If the clip name contains "Walk", we want to start moving
        if (currentClipName.Contains("Walk"))
        {
            shouldMove = true;
        }
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
            }
        }
    }
}
