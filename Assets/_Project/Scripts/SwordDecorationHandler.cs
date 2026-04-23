using System.Collections.Generic;
using UnityEngine;

public class SwordDecorationHandler : MonoBehaviour
{
    string targetName = "Sword Handle";
    [SerializeField] GameObject sword;
    [SerializeField] GameObject handle;
    // readonly string targetName = "Sword Handle";
    bool isGuardAttached = false;
    bool isHandleAttached = false;
    bool isPommelAttached = false;
    readonly int handleBackForthBoundsAxisIndex = 1;
    readonly int handleBackForthScaleAxisIndex = 1;
    List<Decoration> contactingDecorations = new();

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Collision detected");
        if (collision.contactCount == 0 || !collision.gameObject.TryGetComponent(out Decoration decoration)) { return; }
        // Debug.Log("Collision contains decoration");
        ContactPoint contact = collision.GetContact(0);
        // Debug.Log("Collision name: " + contact.thisCollider.name);
        if (contact.thisCollider.name != targetName) { return; }

        // Debug.Log("Collision was with handle");

        // Debug.Log("Name: " + collision.gameObject.name);
        // Debug.Log("Tag: " + collision.gameObject.tag);
        // foreach (var contact in collision.contacts)
        // {
        //     Debug.Log("Other collider tag: " + contact.otherCollider.tag);
        // }
        if (collision.gameObject.CompareTag("guard"))
        {
            if (isGuardAttached) return;
            // Debug.Log("Guard is touching handle");
            contactingDecorations.Add(decoration);
            decoration.SetSwordToAttachTo(sword.transform);
            decoration.SetLocalAttachPosition(new(0,0,0));
            // Vector3 worldNormal = contact.normal;
            // Vector3 localNormal = handle.transform.InverseTransformDirection(worldNormal);
            // Vector3 absNormal = new(
            //     Mathf.Abs(localNormal.x),
            //     Mathf.Abs(localNormal.y),
            //     Mathf.Abs(localNormal.z)
            // );
            // Debug.Log("absNormal: " + absNormal);
            // float angle = (absNormal.x >= absNormal.y) ? 0 : 90;
            decoration.SetLocalAttachRotation(Quaternion.Euler(-90f, 0f, 0f));
            decoration.SetSwordDecorationHandler(this);
        }
        else if (collision.gameObject.CompareTag("handle"))
        {
            if (isHandleAttached) return;
            // Debug.Log("Handle is touching handle");
            contactingDecorations.Add(decoration);
            decoration.SetSwordToAttachTo(sword.transform);
            decoration.SetLocalAttachPosition(new(0,-0.02f,0));
            float scaledHandleLength = handle.GetComponent<MeshFilter>().sharedMesh.bounds.size[handleBackForthBoundsAxisIndex] * handle.transform.localScale[handleBackForthScaleAxisIndex];
            float scaledHandleDecoLength = scaledHandleLength - 0.02f;
            float handleDecoScale = 1 + (scaledHandleDecoLength - 0.18f) / 0.18f;
            Debug.Log("scaledHandleDecoLength: " + scaledHandleDecoLength);
            decoration.SetLocalAttachScale(new(1,handleDecoScale,1));
            decoration.SetSwordDecorationHandler(this);
        }
        else if (collision.gameObject.CompareTag("pommel"))
        {
            if (isPommelAttached) return;
            // Debug.Log("Pommel is touching handle");
            contactingDecorations.Add(decoration);
            decoration.SetSwordToAttachTo(sword.transform);
            // Vector3 worldNormal = contact.normal;
            // Vector3 localNormal = handle.transform.InverseTransformDirection(worldNormal);
            // Vector3 absNormal = new(
            //     Mathf.Abs(localNormal.x),
            //     Mathf.Abs(localNormal.y),
            //     Mathf.Abs(localNormal.z)
            // );
            // float angle = (absNormal.x >= absNormal.y) ? 0 : 90;
            decoration.SetLocalAttachRotation(Quaternion.Euler(-90f, 0f, 0f));
            decoration.SetLocalAttachPosition(new(0,-(0.2f * handle.transform.localScale[handleBackForthScaleAxisIndex]),0));
            decoration.SetSwordDecorationHandler(this);
        }

        // if (
        //     ((decoration is SwordGuard) && !isGuardAttached) ||
        //     ((decoration is SwordGuard) && !isHandleAttached) ||
        //     ((decoration is SwordGuard) && !isPommelAttached)
        // )
        // {
        //     decoration.SetSwordToAttachTo(gameObject.transform);
        // }
    }

    void OnCollisionExit(Collision collision)
    {
        Decoration decoration = collision.gameObject.GetComponent<Decoration>();
        if (!contactingDecorations.Contains(decoration)) { return; }
        Debug.Log("Decoration is leaving handle");
        contactingDecorations.Remove(decoration);
        decoration.SetSwordToAttachTo(null);
    }

    public void OnDecorationAttach(string tag)
    {
        if (tag == "guard") isGuardAttached = true;
        if (tag == "handle") isHandleAttached = true;
        if (tag == "pommel") isPommelAttached = true;
    }
}
