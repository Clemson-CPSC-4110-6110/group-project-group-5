using UnityEngine;

public class Anvil : MonoBehaviour
{
    [Header("Sticky Settings")]
    public float breakForceThreshold = 5f; // upward velocity threshold to escape
    public bool lockAngular = false;       // optional: lock rotation if needed

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        // Calculate upward velocity relative to world
        float upwardVelocity = Vector3.Dot(rb.linearVelocity, Vector3.up);

        // Only lock Y if upward velocity is below threshold
        if (upwardVelocity < breakForceThreshold)
        {
            // Lock Y position to surface
            Vector3 pos = rb.position;
            pos.y = transform.position.y;
            rb.position = pos;

            // Zero out Y velocity if small enough
            Vector3 vel = rb.linearVelocity;
            if (vel.y < breakForceThreshold)
                vel.y = 0;
            rb.linearVelocity = vel;
        }

        // Optionally lock rotation
        if (lockAngular)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
}
