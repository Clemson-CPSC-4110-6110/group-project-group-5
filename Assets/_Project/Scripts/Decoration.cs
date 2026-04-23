using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(XRGrabInteractable))]
public class Decoration : MonoBehaviour
{
    [SerializeField] List<Material> materials = new();
    Rigidbody rb;
    Collider decoCollider;
    XRGrabInteractable grabInteractable;
    protected Transform swordTransform;
    protected Vector3 attachLocalPosition;
    protected Vector3 attachLocalScale = new(1,1,1);
    protected Quaternion attachLocalRotation = Quaternion.identity;
    SwordDecorationHandler swordDecorationHandler;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        decoCollider = GetComponent<Collider>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        GetComponent<Renderer>().material = materials[Random.Range(0, materials.Count)];
    }
    public void SetSwordToAttachTo(Transform newSwordTransform)
    {
        swordTransform = newSwordTransform;
    }
    public void SetLocalAttachPosition(Vector3 newAttachLocalPosition)
    {
        attachLocalPosition = newAttachLocalPosition;
    }
    public void SetLocalAttachScale(Vector3 newAttachLocalScale)
    {
        attachLocalScale = newAttachLocalScale;
    }
    public void SetLocalAttachRotation(Quaternion newAttachLocalRotation)
    {
        attachLocalRotation = newAttachLocalRotation;
    }
    public void SetSwordDecorationHandler(SwordDecorationHandler newSwordDecorationHandler)
    {
        swordDecorationHandler = newSwordDecorationHandler;

        // AttachDecoration();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) { return; }
        if (swordTransform == null) return;
        // AttachDecoration();
        gameObject.transform.SetParent(swordTransform);
        decoCollider.enabled = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        grabInteractable.enabled = false;
        // rb.enabled = false;

        if (!gameObject.CompareTag("handle"))
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 worldNormal = contact.normal;
            Vector3 localNormal = swordTransform.InverseTransformDirection(worldNormal);
            Vector3 absNormal = new(
                Mathf.Abs(localNormal.x),
                Mathf.Abs(localNormal.y),
                Mathf.Abs(localNormal.z)
            );
            Debug.Log("absNormal: " + absNormal);
            float angle = (absNormal.x >= absNormal.z) ? 90 : 0;
            Vector3 newRotationEulers = attachLocalRotation.eulerAngles;
            newRotationEulers.z += angle;
            // attachLocalRotation = Quaternion.Euler(newRotationEulers.x, newRotationEulers.y, newRotationEulers.z);
            gameObject.transform.SetLocalPositionAndRotation(attachLocalPosition, Quaternion.Euler(newRotationEulers.x, newRotationEulers.y, newRotationEulers.z));
        }
        else
        {
            gameObject.transform.SetLocalPositionAndRotation(attachLocalPosition, attachLocalRotation);
            gameObject.transform.localScale = attachLocalScale;
        }

        swordDecorationHandler.OnDecorationAttach(gameObject.tag);
    }

    // void AttachDecoration()
    // {
    //     gameObject.transform.SetParent(swordTransform);
    //     decoCollider.enabled = false;
    //     rb.isKinematic = true;
    //     rb.useGravity = false;
    //     grabInteractable.enabled = false;
    //     // rb.enabled = false;
    //     gameObject.transform.SetLocalPositionAndRotation(attachLocalPosition, attachLocalRotation);
    //     swordDecorationHandler.OnDecorationAttach(gameObject.tag);
    // }
}
