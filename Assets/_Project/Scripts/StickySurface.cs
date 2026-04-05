using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StickySurface : MonoBehaviour
{
    private AnvilAttachable[] anvilAttachables;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;

        Rigidbody collidingRb = collision.rigidbody;

        if (collidingRb == null) return;

        collidingRb.constraints = RigidbodyConstraints.FreezeAll;

        anvilAttachables = collidingRb.gameObject.GetComponents<AnvilAttachable>();
        foreach (AnvilAttachable script in anvilAttachables)
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

        foreach (AnvilAttachable script in anvilAttachables)
        {
            script.isOnAnvil = false;
        }
    }
}