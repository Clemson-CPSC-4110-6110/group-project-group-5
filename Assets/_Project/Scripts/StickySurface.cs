using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class StickySurface : MonoBehaviour
{
    // private AnvilAttachable[] anvilAttachables;
    private AnvilAttachable anvilAttachable;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;

        Rigidbody collidingRb = collision.rigidbody;

        if (collidingRb == null) return;

        collidingRb.constraints = RigidbodyConstraints.FreezeAll;

        // anvilAttachables = collidingRb.gameObject.GetComponents<AnvilAttachable>();
        // foreach (AnvilAttachable script in anvilAttachables)
        // {
        //     script.isOnAnvil = true;
        // }
        anvilAttachable = collidingRb.gameObject.GetComponent<AnvilAttachable>();
        anvilAttachable.isOnAnvil = true;
        collision.gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.Instantaneous;
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;
        Rigidbody collidingRb = collision.rigidbody;
        // Debug.Log(collidingRb.gameObject.name + " is leaving anvil");

        collidingRb.constraints = RigidbodyConstraints.None;

        // foreach (AnvilAttachable script in anvilAttachables)
        // {
        //     script.isOnAnvil = false;
        // }
        anvilAttachable.isOnAnvil = true;
        collision.gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.VelocityTracking;
    }
}