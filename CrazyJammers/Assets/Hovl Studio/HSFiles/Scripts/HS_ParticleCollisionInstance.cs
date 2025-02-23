using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HS_ParticleCollisionInstance : MonoBehaviour
{
    public GameObject[] EffectsOnCollision;
    public float DestroyTimeDelay = 5;
    public bool UseWorldSpacePosition;
    public float Offset = 0;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public bool useOnlyRotationOffset = true;
    public bool UseFirePointRotation;
    public bool DestoyMainEffect = false;
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        if (part == null)
        {
            Debug.LogError("ParticleSystem component not found on the GameObject.");
        }
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Particle collision detected with: " + other.name);
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        Debug.Log("Number of collision events: " + numCollisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            foreach (var effect in EffectsOnCollision)
            {
                var instance = Instantiate(effect, collisionEvents[i].intersection + collisionEvents[i].normal * Offset, Quaternion.identity);
                Debug.Log("Instantiated object initial rotation: " + instance.transform.rotation.eulerAngles);

                if (!UseWorldSpacePosition) instance.transform.parent = transform;
                if (UseFirePointRotation)
                {
                    instance.transform.LookAt(transform.position);
                }
                else if (rotationOffset != Vector3.zero && useOnlyRotationOffset)
                {
                    instance.transform.rotation = Quaternion.Euler(rotationOffset);
                }
                else
                {
                    instance.transform.LookAt(collisionEvents[i].intersection + collisionEvents[i].normal);
                    instance.transform.rotation *= Quaternion.Euler(rotationOffset);
                }

                Debug.Log("Instantiated object final rotation: " + instance.transform.rotation.eulerAngles);
                Destroy(instance, DestroyTimeDelay);
            }
        }

        if (DestoyMainEffect)
        {
            Destroy(gameObject, DestroyTimeDelay + 0.5f);
        }
    }
}
