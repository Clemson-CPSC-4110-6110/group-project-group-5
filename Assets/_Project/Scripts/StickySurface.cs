using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickySurface : MonoBehaviour
{
    private ScaleAwayOnHit[] scalingScripts;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;

        Rigidbody collidingRb = collision.rigidbody;

        if (collidingRb == null) return;

        collidingRb.constraints = RigidbodyConstraints.FreezeAll;

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

        collidingRb.constraints = RigidbodyConstraints.None;

        foreach (ScaleAwayOnHit script in scalingScripts)
        {
            script.isOnAnvil = false;
        }
    }
}