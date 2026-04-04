using UnityEngine;
using UnityEngine.Events;

public class ScaleAwayOnHit : MonoBehaviour
{
    [Header("Target")]
    public Collider targetCollider; // Assign your child MeshCollider here

    [Header("Scaling Settings")]
    public float shrinkFactor = 0.95f;
    public float growFactor = 1.05f;

    [Header("Clamp Settings")]
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new Vector3(3f, 3f, 3f);

    [Header("Scale Target")]
    public Transform scaleTarget; // What actually gets scaled

    [Header("Events")]
    public UnityEvent<Vector3, Vector3> onScaleChanged;

    void Start()
    {
        if (scaleTarget == null)
            scaleTarget = transform;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) return;

        ContactPoint contact = collision.GetContact(0);

        // Only react if this specific collider was hit
        if (contact.thisCollider != targetCollider)
            return;

        Vector3 worldNormal = contact.normal;

        HandleDirectionalScale(worldNormal);
    }

    void HandleDirectionalScale(Vector3 worldNormal)
    {
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new Vector3(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        Vector3 oldScale = scaleTarget.localScale;
        Vector3 newScale = oldScale;

        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        {
            newScale.x *= shrinkFactor;
            newScale.y *= growFactor;
            newScale.z *= growFactor;
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        {
            newScale.y *= shrinkFactor;
            newScale.x *= growFactor;
            newScale.z *= growFactor;
        }
        else
        {
            newScale.z *= shrinkFactor;
            newScale.x *= growFactor;
            newScale.y *= growFactor;
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