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
        if (swordTransform != null)
        {
            AttachDecoration();
        }
    }

    void AttachDecoration()
    {
        gameObject.transform.SetParent(swordTransform);
        decoCollider.enabled = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        grabInteractable.enabled = false;
        // rb.enabled = false;
        gameObject.transform.SetLocalPositionAndRotation(attachLocalPosition, attachLocalRotation);
        swordDecorationHandler.OnDecorationAttach(gameObject.tag);
    }
}
