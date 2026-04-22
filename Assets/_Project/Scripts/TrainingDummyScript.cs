using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TrainingDummyScript : MonoBehaviour
{
    Animator anim;
    [SerializeField] float minSmallHitVelocity = 5f;
    [SerializeField] float maxSmallHitVelocity = 10f;
    [SerializeField] float hitCooldown = 0.5f;
    private float lastHitTime = 0f;
    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.SetTrigger("BigHit");
    }

    void OnCollisionEnter(Collision collision)
    {
        float velocityMagnitude;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;
        velocityMagnitude = hammerRb.linearVelocity.magnitude;
        Debug.Log("Velocity Magnitude: " + velocityMagnitude);
        if (velocityMagnitude < maxSmallHitVelocity && velocityMagnitude > minSmallHitVelocity)
        {
            anim.SetTrigger("SmallHit");
        }
        if (velocityMagnitude >= maxSmallHitVelocity)
        {
            anim.SetTrigger("BigHit");
        }
    }

}
