using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ScaleAwayOnHit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]  Collider targetCollider; // Assign your child MeshCollider here
    [SerializeField] Transform scaleTarget; // What actually gets scaled

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    [SerializeField] Vector3 maxScale = new(2f, 2f, 2f);

    [Header("Size")]
    [SerializeField] Vector3 unscaledSize;

    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;


    [Header("Events")]
    public UnityEvent<Vector3, Vector3> onScaleChanged;

    public bool isOnAnvil = false;
    Vector3 scaledSize;
    Vector3 area;
    float volume;
    float maxVolumeShift;

    // bool pendingScale = false;
    // Vector3 pendingNormal;
    // float pendingVolumeShift;

    void Start()
    {
        maxVolumeShift = 0.001f * volumeShiftModifier; // Upper limit
    }

    // void FixedUpdate()
    // {
    //     if (!pendingScale) return;

    //     RecalculateMeasurements();
    //     HandleDirectionalScale(pendingNormal, pendingVolumeShift);

    //     pendingScale = false;
    // }

    void OnCollisionEnter(Collision collision)
    {
        if (!isOnAnvil) 
        {
            // Debug.Log("Weapon is not on anvil. Cannot modify weapon proportions.");
            return;
        }
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) 
        {
            // Debug.Log("No contact detected or collision is not with hammer");
            return;
        }
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 worldNormal = contact.normal;
        RecalculateMeasurements();

        // VELOCITY
        float velocityMagnitude;
        // Rigidbody hammerRb = collision.rigidbody; // the hammer
        // float velocityMagnitude = hammerRb.linearVelocity.magnitude;
        // float velocityMagnitude = collision.impulse.magnitude;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        velocityMagnitude = hammerRb.linearVelocity.magnitude;
        // Vector3 normal = contact.normal;
        // velocityMagnitude = Mathf.Max(
        //     0f,
        //     Vector3.Dot(hammerRb.linearVelocity, -normal)
        // );
        // velocityMagnitude = collision.relativeVelocity.magnitude;
        if (velocityMagnitude < minVelocity || velocityMagnitude > maxVelocity) return;
        Debug.Log("Collision's velocity magnitude = " + velocityMagnitude);

        // VOLUME SHIFT
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift;

        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);

        // pendingNormal = normal;
        // pendingVolumeShift = volumeShiftedOnHit;
        // pendingScale = true;
    }

    void RecalculateMeasurements()
    {
        scaledSize = new(unscaledSize[0] * scaleTarget.localScale[0], unscaledSize[1] * scaleTarget.localScale[1], unscaledSize[2] * scaleTarget.localScale[2]);
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);

        // Debug.Log("\nunscaledSize[0] * scaleTarget.localScale[0]: " + unscaledSize[0] * scaleTarget.localScale[0] + 
        //         "\nunscaledSize[1] * scaleTarget.localScale[1]: " + unscaledSize[1] * scaleTarget.localScale[1] + 
        //         "\nunscaledSize[2] * scaleTarget.localScale[2]: " + unscaledSize[2] * scaleTarget.localScale[2]);
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new Vector3(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        Vector3 oldScale = scaleTarget.localScale;
        Vector3 newScale = oldScale;
        Debug.Log("New Scale: " + newScale);

        Debug.Log("Volume After: " + volume);
        RecalculateMeasurements();
        float hitAxisSizeLost;
        float hitAxisShrinkFactor;
        float preservedFactor;

        // Debug.Log("Volume before: " + volume);
        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        {
            Debug.Log("Decreasing x");
            hitAxisSizeLost = volumeShiftedOnHit / area[0];
            hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
            newScale.x *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            // newScale.y *= preservedFactor;
            // newScale.z *= preservedFactor;

            float yPreservedFactor = 1 + (preservedFactor - 1) / yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            newScale.y *= yPreservedFactor;
            newScale.z *= nonYpreservedFactor;
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        {
            Debug.Log("Decreasing y");
            hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
            hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
            newScale.y *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            newScale.x *= preservedFactor;
            newScale.z *= preservedFactor;
        }
        else
        {
            Debug.Log("Decreasing z");
            hitAxisSizeLost = volumeShiftedOnHit / area[2];
            hitAxisShrinkFactor = (scaledSize[2] - hitAxisSizeLost) / scaledSize[2];
            newScale.z *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            // float yFactor = 1 + (totalPreservedFactor - 1) * yGrowthScale;
            // float nonYFactor = 1 + (totalPreservedFactor - 1) * (1 - yGrowthScale);
            float yPreservedFactor = preservedFactor + (preservedFactor - 1) * yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            Debug.Log("preservedFactor: " + preservedFactor);
            Debug.Log("yPreservedFactor: " + yPreservedFactor);
            Debug.Log("nonYpreservedFactor: " + nonYpreservedFactor);

            newScale.x *= nonYpreservedFactor;
            newScale.y *= yPreservedFactor;
        }

        if (newScale.x >= minScale.x && newScale.x <= maxScale.x &&
            newScale.y >= minScale.y && newScale.y <= maxScale.y &&
            newScale.z >= minScale.z && newScale.z <= maxScale.z)
        {
            scaleTarget.localScale = newScale;
            onScaleChanged.Invoke(oldScale, newScale);
            return;
        }
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    // void OnJointBreak(float breakForce)
    // {
    //     Rigidbody rb = GetComponentInParent<Rigidbody>();

    //     // rb.mass = 1;

    //     // rb.angularDamping = 0.05f;
    //     // rb.linearDamping = 0f;
    //     rb.constraints = RigidbodyConstraints.None;
    //     isOnAnvil = false;
    // }
}