using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ScaleAwayOnHit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]  Collider targetCollider; // Assign your child MeshCollider here

    [Header("Scaling Settings")]
    [SerializeField] float shrinkFactor = 0.95f;
    [SerializeField] float growFactor = 1.05f;

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] Vector3 maxScale = new Vector3(3f, 3f, 3f);

    [Header("Scale Target")]
    [SerializeField] Transform scaleTarget; // What actually gets scaled

    [Header("Size")]
    [SerializeField] Vector3 unscaledSize;

    [Header("Events")]
    public UnityEvent<Vector3, Vector3> onScaleChanged;
    float volume;
    Vector3 scaledSize;
    Vector3 area;

    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) return;

        ContactPoint contact = collision.GetContact(0);

        // Only react if this specific collider was hit
        if (contact.thisCollider != targetCollider)
            return;

        Vector3 worldNormal = contact.normal;
        // collision.impulse.magnitude
        HandleDirectionalScale(worldNormal, 0.002f);
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

        RecalculateMeasurements();
        float hitAxisSizeLost;
        float hitAxisShrinkFactor;
        float preservedFactor;

        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        {
            Debug.Log("Volume before: " + volume);
            hitAxisSizeLost = volumeShiftedOnHit / area[0];
            hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
            newScale.x *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            newScale.y *= preservedFactor;
            newScale.z *= preservedFactor;
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        {
            Debug.Log("Volume before: " + volume);
            hitAxisSizeLost = volumeShiftedOnHit / area[1];
            hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
            newScale.y *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            newScale.x *= preservedFactor;
            newScale.z *= preservedFactor;
        }
        else
        {
            Debug.Log("Volume before: " + volume);
            hitAxisSizeLost = volumeShiftedOnHit / area[2];
            hitAxisShrinkFactor = (scaledSize[2] - hitAxisSizeLost) / scaledSize[2];
            newScale.z *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            newScale.x *= preservedFactor;
            newScale.y *= preservedFactor;
        }

        // Clamp scale
        newScale.x = Mathf.Clamp(newScale.x, minScale.x, maxScale.x);
        newScale.y = Mathf.Clamp(newScale.y, minScale.y, maxScale.y);
        newScale.z = Mathf.Clamp(newScale.z, minScale.z, maxScale.z);

        // Apply scale
        scaleTarget.localScale = newScale;

        onScaleChanged.Invoke(oldScale, newScale);
    }
}