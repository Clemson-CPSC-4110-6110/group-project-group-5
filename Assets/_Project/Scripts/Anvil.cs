using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickySurface : MonoBehaviour
{
    [SerializeField] private float stickForce = 5000f;
    private ScaleAwayOnHit[] scalingScripts;
    [SerializeField] private float linearDamping = 500f;  // Adjust for desired movement resistance
    [SerializeField] private float angularDamping = 500f; // Adjust for desired rotation resistance
    Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;

        // collidingRb = collision.gameObject.GetComponentInParent<Rigidbody>();
        Rigidbody collidingRb = collision.rigidbody;

        if (collidingRb == null) return;

        // Debug.Log("Collision RigidBody found");

        // // Avoid duplicate joints of this type
        // if (collidingRb.TryGetComponent<FixedJoint>(out _)) return;

        // collidingRb.mass = 50;
        
        collidingRb.constraints = RigidbodyConstraints.FreezePositionX |
                                  RigidbodyConstraints.FreezePositionY |
                                  RigidbodyConstraints.FreezePositionZ |
                                  RigidbodyConstraints.FreezeRotationX |
                                  RigidbodyConstraints.FreezeRotationY |
                                  RigidbodyConstraints.FreezeRotationZ ;
        // collidingRb.dra
        // collidingRb.angularDamping = 0;
        // collidingRb.linearDamping = 0.05f;

        // FixedJoint joint = collidingRb.gameObject.AddComponent<FixedJoint>();

        // joint.connectedBody = rb;
        // joint.breakForce = stickForce;
        // joint.breakTorque = stickForce;
        // joint.enableCollision = true;


        // collidingRb.isKinematic = true;

        scalingScripts = collidingRb.gameObject.GetComponents<ScaleAwayOnHit>();
        foreach (ScaleAwayOnHit script in scalingScripts)
        {
            script.isOnAnvil = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;
        Rigidbody collidingRb = collision.rigidbody;
        // Debug.Log(collidingRb.gameObject.name + " is leaving anvil");

        // collidingRb.isKinematic = false;
        collidingRb.constraints = RigidbodyConstraints.None;

        foreach (ScaleAwayOnHit script in scalingScripts)
        {
            script.isOnAnvil = false;
        }
    }
    // void OnTriggerEnter(Collider other)
    // {
    //     if (!other.gameObject.CompareTag("anvilSocketable")) return;

    //     // collidingRb = collision.gameObject.GetComponentInParent<Rigidbody>();
    //     Rigidbody collidingRb = other.GetComponentInParent<Rigidbody>();

    //     if (collidingRb == null) return;

    //     Debug.Log("Collision RigidBody found");

    //     // // Avoid duplicate joints of this type
    //     // if (collidingRb.TryGetComponent<FixedJoint>(out _)) return;

    //     FixedJoint joint = collidingRb.gameObject.AddComponent<FixedJoint>();
    //     joint.connectedBody = rb;
    //     joint.breakForce = stickForce;
    //     joint.breakTorque = stickForce;

    //     // collidingRb.isKinematic = true;

    //     scalingScripts = collidingRb.gameObject.GetComponents<ScaleAwayOnHit>();
    //     foreach (ScaleAwayOnHit script in scalingScripts)
    //     {
    //         script.isOnAnvil = true;
    //     }
    // }

    // void OnTriggerExit(Collider other)
    // {
    //     if (!other.gameObject.CompareTag("anvilSocketable")) return;
    //     Rigidbody collidingRb = other.GetComponentInParent<Rigidbody>();
    //     Debug.Log(collidingRb.gameObject.name + " is leaving anvil");

    //     // collidingRb.isKinematic = false;
    //     foreach (ScaleAwayOnHit script in scalingScripts)
    //     {
    //         script.isOnAnvil = false;
    //     }
    // }
}