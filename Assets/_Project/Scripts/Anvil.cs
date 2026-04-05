using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickySurface : MonoBehaviour
{
    [SerializeField] private float stickForce = 5000f;
    private Rigidbody rb;
    private ScaleAwayOnHit[] scalingScripts;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;

        Rigidbody collidingRb = collision.rigidbody;
        if (collidingRb == null) return;

        // Avoid duplicate joints of this type
        if (collidingRb.TryGetComponent<FixedJoint>(out _)) return;

        FixedJoint joint = collidingRb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = rb;
        joint.breakForce = stickForce;
        joint.breakTorque = stickForce;

        scalingScripts = collidingRb.gameObject.GetComponents<ScaleAwayOnHit>();
        foreach (ScaleAwayOnHit script in scalingScripts)
        {
            script.isOnAnvil = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        foreach (ScaleAwayOnHit script in scalingScripts)
        {
            script.isOnAnvil = true;
        }
    }
}