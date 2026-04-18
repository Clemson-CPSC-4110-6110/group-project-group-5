using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class NewSwordTipScaleHandler : MonoBehaviour
{
    [Header("Target")]
    // [SerializeField] Transform scaleTarget; // What actually gets scaled
    [SerializeField] Transform bodyScaleTarget; // What actually gets scaled
    [SerializeField] Transform tipScaleTarget; // What actually gets scaled
    [SerializeField] List<MeshFilter> xMeshFilters;
    [SerializeField] List<MeshFilter> yMeshFilters;
    [SerializeField] List<MeshFilter> zMeshFilters;

    [Header("Clamp Settings")]
    [SerializeField] double minBodyScale = 0.2;
    [SerializeField] double maxBodyScale = 3;
    [SerializeField] double minTipScale = 0.2;
    [SerializeField] double maxTipScale = 3;

    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;

    Vector3 unscaledSize;
    Vector3 scaledSize;
    Vector3 area;
    float volume;
    float maxVolumeShift;
    private float lastHitTime = 0f;

    void Start()
    {
        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        RecalculateUnscaledSize();
    }

    void RecalculateUnscaledSize()
    {
        unscaledSize = new Vector3(0,0,0);
        foreach (MeshFilter meshFilter in xMeshFilters)
        {
            unscaledSize[0] += meshFilter.sharedMesh.bounds.size[0];
        }
        unscaledSize[0] /= bodyScaleTarget.localScale[0];
        foreach (MeshFilter meshFilter in yMeshFilters)
        {
            unscaledSize[1] += meshFilter.sharedMesh.bounds.size[1];
        }
        unscaledSize[1] /= bodyScaleTarget.localScale[1];
        foreach (MeshFilter meshFilter in zMeshFilters)
        {
            unscaledSize[2] += meshFilter.sharedMesh.bounds.size[2];
        }
        unscaledSize[2] /= bodyScaleTarget.localScale[2];
    }

    public void ScaleUpMaxScale(Vector3 modifier)
    {
        maxBodyScale *= modifier.y;
        minBodyScale *= modifier.y;
        // maxScale = new(
        //     maxScale[0] * modifier[0], 
        //     maxScale[1] * modifier[1], 
        //     maxScale[2] * modifier[2]
        // );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) { return; }
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) { return; }
        
        ContactPoint contact = collision.GetContact(0);
        Vector3 worldNormal = contact.normal;
        RecalculateMeasurements();

        // VELOCITY
        float velocityMagnitude;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;

        velocityMagnitude = hammerRb.linearVelocity.magnitude;
        if (velocityMagnitude < minVelocity || velocityMagnitude > maxVelocity) return;

        // Debug.Log("Collision's velocity magnitude = " + velocityMagnitude);s
        if (Time.time - lastHitTime < hitCooldown) return;

        lastHitTime = Time.time;
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();
        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);
    }

    void RecalculateMeasurements()
    {
        scaledSize = new(unscaledSize[0] * bodyScaleTarget.localScale[0], unscaledSize[1] * bodyScaleTarget.localScale[1], unscaledSize[2] * bodyScaleTarget.localScale[2]);
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = tipScaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        // Vector3 newScale = oldScale;
        float new_body_scale_y = bodyScaleTarget.localScale.y;
        float new_tip_scale_y = tipScaleTarget.localScale.y;
        // Debug.Log("New Scale: " + newScale);

        // Debug.Log("Volume After: " + volume);
        RecalculateMeasurements();
        float hitAxisSizeLost;
        float hitAxisShrinkFactor;
        float preservedFactor;

        // Debug.Log("Volume before: " + volume);

        // Flatten Tip
        if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        {
            // Debug.Log("Decreasing y");
            hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
            hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
            preservedFactor = volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]);
            
            new_body_scale_y *= preservedFactor / hitAxisShrinkFactor;
            new_tip_scale_y *= hitAxisShrinkFactor / (preservedFactor / hitAxisShrinkFactor);

        }
        // Lengthen tip
        else
        {
            // Debug.Log("Decreasing y");
            hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
            hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
            preservedFactor = volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]);

            new_body_scale_y *= hitAxisShrinkFactor;
            new_tip_scale_y *= preservedFactor / hitAxisShrinkFactor;
        }

        if (new_tip_scale_y >= minTipScale && new_tip_scale_y <= maxTipScale &&
            new_body_scale_y >= minBodyScale && new_body_scale_y <= maxBodyScale)
        {
            Vector3 oldBodyScale = bodyScaleTarget.localScale;
            oldBodyScale.y += new_body_scale_y;
            bodyScaleTarget.localScale = oldBodyScale;

            Vector3 oldTipScale = tipScaleTarget.localScale;
            oldTipScale.y += new_tip_scale_y;
            tipScaleTarget.localScale = oldTipScale;
            // scaleTarget.localScale = newScale;
            // onScaleChanged.Invoke(oldScale, newScale);
            return;
        }
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    void ShiftVolumeFromBodyToTip()
    {
        
    }
    void ShiftVolumeFromTipToBody()
    {
        
    }
}