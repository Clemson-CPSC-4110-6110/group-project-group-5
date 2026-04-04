using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TongsController : MonoBehaviour
{
    public Transform pivotTransform;
    public XRGrabInteractable grabInteractable;

    public float maxAngle = 19f;
    public float minAngle = 0f;
    public float minDistance = 0.1f;
    public float maxDistance = 0.2f;

    [Header("Grabbing")]
    public Transform grabPoint;            // point between tong tips
    public float grabRadius = 0.05f;
    public LayerMask grabbableLayer;
    public float breakForce = 200f;

    public GameObject[] ignoreObjects;

    private Quaternion startRotation = Quaternion.identity;
    private FixedJoint currentJoint;
    private Rigidbody grabbedRb;
    private Transform grabbedObjParent;
    private bool grabbedRbIsKinematic;
    private bool grabbedRbUseGravity;

    void Update()
    {
        if (grabInteractable.interactorsSelecting.Count == 2)
        {
            IXRSelectInteractor first = grabInteractable.interactorsSelecting[0];
            IXRSelectInteractor second = grabInteractable.interactorsSelecting[1];

            float dist = Vector3.Distance(first.transform.position,
                                          second.transform.position);

            float t = 1 - Mathf.InverseLerp(minDistance, maxDistance, dist);
            float angle = Mathf.Lerp(minAngle, maxAngle, t);

            pivotTransform.localRotation =
                startRotation * Quaternion.Euler(0f, angle, 0f);

            HandleGrab(t);
        }
    }

    void HandleGrab(float t)
    {
        // Fully closed threshold
        if (t < 0.1f)
        {
            if (currentJoint == null)
            {
                TryGrab();
            }
        }
        else
        {
            Release();
        }
    }

void TryGrab()
{
    Collider[] hits = Physics.OverlapSphere(grabPoint.position, grabRadius, grabbableLayer);

    foreach (Collider hit in hits)
    {
        bool shouldIgnore = false;

        foreach (GameObject obj in ignoreObjects)
        {
            if (hit.transform.IsChildOf(obj.transform))
            {
                shouldIgnore = true;
                break;
            }
        }

        if (shouldIgnore) continue;

        Rigidbody rb = hit.attachedRigidbody;

        if (rb != null)
        {
            // Debug.Log("Attempting to grab: " + rb.gameObject.name);

            grabbedRb = rb;

            // currentJoint = rb.gameObject.AddComponent<FixedJoint>();
            // currentJoint.connectedBody = GetComponent<Rigidbody>();
            // currentJoint.breakForce = breakForce;
            // currentJoint.breakTorque = breakForce;
            grabbedRbIsKinematic = rb.isKinematic;
            grabbedRbUseGravity = rb.useGravity;
            grabbedObjParent = rb.gameObject.transform.parent;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.gameObject.transform.SetParent(gameObject.transform);

            return; // stop after grabbing first valid object
        }
    }
}

    void Release()
    {
        if (grabbedRb != null)
        {
            Debug.Log("Releasing: " + grabbedRb.gameObject.name);

            // Destroy(currentJoint);
            // currentJoint = null;
            grabbedRb.isKinematic = grabbedRbIsKinematic;
            grabbedRb.useGravity = grabbedRbUseGravity;
            grabbedRb.gameObject.transform.SetParent(grabbedObjParent);
            grabbedRb = null;

        }
    }

    // Optional: visualize grab area
    void OnDrawGizmos()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
        }
    }
}