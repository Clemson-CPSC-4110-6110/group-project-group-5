using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class NewScaleAwayOnHit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform scaleTarget;

    [SerializeField] GameObject left_edge;
    [SerializeField] GameObject right_edge;
    [SerializeField] GameObject top_edge;
    [SerializeField] GameObject bottom_edge;
    [SerializeField] GameObject top_left_corner;
    [SerializeField] GameObject top_right_corner;
    [SerializeField] GameObject bottom_left_corner;
    [SerializeField] GameObject bottom_right_corner;

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new(2f, 2f, 2f);

    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;

    [Header("Events")]
    public UnityEvent<Vector3, Vector3> onScaleChanged;

    Vector3 unscaledSize;
    Vector3 scaledSize;
    Vector3 area;
    float volume;
    float maxVolumeShift;
    private float lastHitTime = 0f;
    List<GameObject> leftComponents;
    List<GameObject> middleXComponents;
    List<GameObject> rightComponents;
    List<GameObject> topComponents;
    List<GameObject> middleYComponents;
    List<GameObject> bottomComponents;
    List<GameObject> allComponents;
    Vector3 leftEdgeSize;
    Vector3 rightEdgeSize;
    Vector3 topEdgeSize;
    Vector3 bottomEdgeSize;

    void Start()
    {
        leftComponents = new() {top_left_corner, left_edge, bottom_left_corner};
        middleXComponents = new() {top_edge, bottom_edge};
        rightComponents = new() {top_right_corner, right_edge, bottom_right_corner};

        topComponents = new() {top_left_corner, top_edge, top_right_corner};
        middleYComponents = new() {left_edge, right_edge};
        bottomComponents = new() {bottom_left_corner, bottom_edge, bottom_right_corner};
        allComponents = leftComponents.Concat(middleXComponents).Concat(rightComponents).ToList();

        leftEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        rightEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        topEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        bottomEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        Debug.Log("Unscaled size: " + unscaledSize);
        RecalculateUnscaledSize();
        FixComponentPositions();
    }

    void RecalculateUnscaledSize()
    {
        leftEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        rightEdgeSize = right_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        topEdgeSize = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        bottomEdgeSize = bottom_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        
        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] += leftEdgeSize[0] + topEdgeSize[0] + rightEdgeSize[0];
        unscaledSize[0] /= scaleTarget.localScale[0];
        unscaledSize[1] += leftEdgeSize[1];
        unscaledSize[1] /= scaleTarget.localScale[1];
        unscaledSize[2] += topEdgeSize[2] + leftEdgeSize[2] + bottomEdgeSize[2];
        unscaledSize[2] /= scaleTarget.localScale[2];
    }

    public void ScaleUpMaxScale(Vector3 modifier)
    {
        maxScale = new(
            maxScale[0] * modifier[0], 
            maxScale[1] * modifier[1], 
            maxScale[2] * modifier[2]
        );
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
        Debug.Log("Collision's velocity magnitude = " + velocityMagnitude);
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;
        // VOLUME SHIFT
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();

        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);
    }

    void RecalculateMeasurements()
    {
        // scaledSize = new(unscaledSize[0] * scaleTarget.localScale[0], unscaledSize[1] * scaleTarget.localScale[1], unscaledSize[2] * scaleTarget.localScale[2]);

        leftEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        rightEdgeSize = right_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        topEdgeSize = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        bottomEdgeSize = bottom_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        scaledSize = new(
            leftEdgeSize[0] + topEdgeSize[0] + rightEdgeSize[0],
            leftEdgeSize[1],
            topEdgeSize[2] + leftEdgeSize[2] + bottomEdgeSize[2]
        );
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);

        // Debug.Log("\nunscaledSize[0] * scaleTarget.localScale[0]: " + unscaledSize[0] * scaleTarget.localScale[0] + 
        //         "\nunscaledSize[1] * scaleTarget.localScale[1]: " + unscaledSize[1] * scaleTarget.localScale[1] + 
        //         "\nunscaledSize[2] * scaleTarget.localScale[2]: " + unscaledSize[2] * scaleTarget.localScale[2]);
    }

    void FixComponentPositions()
    {
        top_left_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] - topEdgeSize[0] / 2, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        Debug.Log("top_edge.transform.localPosition: " + top_edge.transform.localPosition);
        Debug.Log("topEdgeSize: " + topEdgeSize);
        Debug.Log("top_left_corner.transform.localPosition: " + top_left_corner.transform.localPosition);
        Debug.Log("top_edge.transform.localPosition[0]: " + top_edge.transform.localPosition[0] + "\n - topEdgeSize[0] / 2:" + topEdgeSize[0] / 2 + "\n = top_left_corner.transform.localPosition: " + top_left_corner.transform.localPosition);
        left_edge.transform.localPosition = new Vector3(
            top_left_corner.transform.localPosition[0],
            top_left_corner.transform.localPosition[1],
            top_left_corner.transform.localPosition[2] + leftEdgeSize.y / 2 // for some reason box colliders swap z and y
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            left_edge.transform.localPosition[0], 
            left_edge.transform.localPosition[1], 
            left_edge.transform.localPosition[2] - leftEdgeSize.y / 2 // for some reason box colliders swap z and y
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            left_edge.transform.localPosition[0], 
            left_edge.transform.localPosition[1], 
            left_edge.transform.localPosition[2] + leftEdgeSize.y / 2 // for some reason box colliders swap z and y
        );

        top_right_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] + topEdgeSize[0] / 2, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        right_edge.transform.localPosition = new Vector3(
            top_right_corner.transform.localPosition[0],
            top_right_corner.transform.localPosition[1],
            top_right_corner.transform.localPosition[2] + leftEdgeSize.y / 2 // for some reason box colliders swap z and y
        );
        bottom_right_corner.transform.localPosition = new Vector3(
            right_edge.transform.localPosition[0], 
            right_edge.transform.localPosition[1], 
            right_edge.transform.localPosition[2] + leftEdgeSize.y / 2 // for some reason box colliders swap z and y
        );

        /*
                top_left_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] - topEdgeSize[1] / 2, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        Debug.Log("top_edge.transform.localPosition: " + top_edge.transform.localPosition);
        Debug.Log("topEdgeSize: " + topEdgeSize);
        Debug.Log("top_left_corner.transform.localPosition: " + top_left_corner.transform.localPosition);
        Debug.Log("top_edge.transform.localPosition[0]: " + top_edge.transform.localPosition[0] + "\n - topEdgeSize[0] / 2:" + topEdgeSize[0] / 2 + "\n = top_left_corner.transform.localPosition: " + top_left_corner.transform.localPosition);
        left_edge.transform.localPosition = new Vector3(
            top_left_corner.transform.localPosition[0],
            top_left_corner.transform.localPosition[1],
            top_left_corner.transform.localPosition[2] + leftEdgeSize[1] / 2
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            left_edge.transform.localPosition[0], 
            left_edge.transform.localPosition[1], 
            left_edge.transform.localPosition[2] - leftEdgeSize[1] / 2
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            bottom_left_corner.transform.localPosition[0], 
            bottom_left_corner.transform.localPosition[1], 
            bottom_left_corner.transform.localPosition[2] + topEdgeSize[0] / 2
        );

        top_right_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] + topEdgeSize[1] / 2, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        right_edge.transform.localPosition = new Vector3(
            top_right_corner.transform.localPosition[0],
            top_right_corner.transform.localPosition[1],
            top_right_corner.transform.localPosition[2] + leftEdgeSize[1] / 2
        );
        bottom_right_corner.transform.localPosition = new Vector3(
            right_edge.transform.localPosition[0], 
            right_edge.transform.localPosition[1], 
            right_edge.transform.localPosition[2] + leftEdgeSize[1] / 2
        );
        */
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        // Vector3 oldScale = scaleTarget.localScale;
        Vector3 oldScale = new(
            (left_edge.transform.localScale[0] + top_edge.transform.localScale[0] + right_edge.transform.localScale[0]) / 3,
            left_edge.transform.localScale[1],
            (top_edge.transform.localScale[2] + left_edge.transform.localScale[2] + bottom_edge.transform.localScale[2]) / 3
        );
        Vector3 newScale = oldScale;
        // Debug.Log("New Scale: " + newScale);

        Debug.Log("Volume After: " + volume);
        RecalculateMeasurements();
        float hitAxisSizeLost;
        float hitAxisShrinkFactor;
        float preservedFactor;

        // Debug.Log("Volume before: " + volume);
        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        {
            // Debug.Log("Decreasing x");
            hitAxisSizeLost = volumeShiftedOnHit / area[0];
            hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
            newScale.x *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            // newScale.y *= preservedFactor;
            // newScale.z *= preservedFactor;

            float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            newScale.y *= yPreservedFactor;
            newScale.z *= nonYpreservedFactor;
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        {
            // Debug.Log("Decreasing y");
            hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
            hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
            newScale.y *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            newScale.x *= preservedFactor;
            newScale.z *= preservedFactor;
        }
        else
        {
            // Debug.Log("Decreasing z");
            hitAxisSizeLost = volumeShiftedOnHit / area[2];
            hitAxisShrinkFactor = (scaledSize[2] - hitAxisSizeLost) / scaledSize[2];
            newScale.z *= hitAxisShrinkFactor;

            preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
            // float yFactor = 1 + (totalPreservedFactor - 1) * yGrowthScale;
            // float nonYFactor = 1 + (totalPreservedFactor - 1) * (1 - yGrowthScale);
            float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            // Debug.Log("preservedFactor: " + preservedFactor);
            // Debug.Log("yPreservedFactor: " + yPreservedFactor);
            // Debug.Log("nonYpreservedFactor: " + nonYpreservedFactor);

            newScale.x *= nonYpreservedFactor;
            newScale.y *= yPreservedFactor;
        }

        if (newScale.x >= minScale.x && newScale.x <= maxScale.x &&
            newScale.y >= minScale.y && newScale.y <= maxScale.y &&
            newScale.z >= minScale.z && newScale.z <= maxScale.z)
        {
            // scaleTarget.localScale = newScale;
            foreach (GameObject component in leftComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(newComponentScale[0] * newScale.x, newComponentScale[1] * newScale.y, newComponentScale[2] * newScale.z);
                component.transform.localScale = newComponentScale;
            }
            foreach (GameObject component in middleXComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(newComponentScale[0] * newScale.x, newComponentScale[1] * newScale.y, newComponentScale[2] * newScale.z);
                component.transform.localScale = newComponentScale;
            }
            foreach (GameObject component in rightComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(newComponentScale[0] * newScale.x, newComponentScale[1] * newScale.y, newComponentScale[2] * newScale.z);
                component.transform.localScale = newComponentScale;
            }

            onScaleChanged.Invoke(oldScale, newScale);
            FixComponentPositions();
            return;
        }
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    void MultiplyScaleAxis(Transform transform, float mult, char axis)
    {
        Vector3 newScale     = transform.localScale;
        if (axis == 'x')       newScale.x *= mult; 
        else if (axis == 'y')  newScale.y *= mult; 
        else                   newScale.z *= mult;
        transform.localScale = newScale;
    }
}


/*
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class NewScaleAwayOnHit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform centerPivot; // scales all parts

    [SerializeField] GameObject left_edge;
    [SerializeField] GameObject right_edge;
    [SerializeField] GameObject top_edge;
    [SerializeField] GameObject bottom_edge;
    [SerializeField] GameObject top_left_corner;
    [SerializeField] GameObject top_right_corner;
    [SerializeField] GameObject bottom_left_corner;
    [SerializeField] GameObject bottom_right_corner;

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new(2f, 2f, 2f);

    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;

    [Header("Events")]
    public UnityEvent<Vector3, Vector3> onScaleChanged;

    Vector3 unscaledSize;
    Vector3 scaledSize;
    Vector3 area;
    float volume;
    float maxVolumeShift;
    private float lastHitTime = 0f;

    void Start()
    {
        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        Debug.Log("Unscaled size: " + unscaledSize);
        RecalculateUnscaledSize();
    }

    void RecalculateUnscaledSize()
    {
        Vector3 leftEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 rightEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 topEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 bottomEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] += leftEdgeSize[0] + topEdgeSize[0] + rightEdgeSize[0];
        unscaledSize[0] /= centerPivot.localScale[0];
        unscaledSize[1] += leftEdgeSize[1];
        unscaledSize[1] /= centerPivot.localScale[1];
        unscaledSize[2] += topEdgeSize[2] + leftEdgeSize[2] + bottomEdgeSize[2];
        unscaledSize[2] /= centerPivot.localScale[2];
    }

    public void ScaleUpMaxScale(Vector3 modifier)
    {
        maxScale = new(
            maxScale[0] * modifier[0], 
            maxScale[1] * modifier[1], 
            maxScale[2] * modifier[2]
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) 
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
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;
        // VOLUME SHIFT
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();

        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);

        // pendingNormal = normal;
        // pendingVolumeShift = volumeShiftedOnHit;
        // pendingScale = true;
    }

    void RecalculateMeasurements()
    {
        // scaledSize = new(unscaledSize[0] * scaleTarget.localScale[0], unscaledSize[1] * scaleTarget.localScale[1], unscaledSize[2] * scaleTarget.localScale[2]);
        Vector3 leftEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 rightEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 topEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        Vector3 bottomEdgeSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        scaledSize = new(
            leftEdgeSize[0] + topEdgeSize[0] + rightEdgeSize[0],
            leftEdgeSize[1],
            topEdgeSize[2] + leftEdgeSize[2] + bottomEdgeSize[2]
        );
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);

        // Debug.Log("\nunscaledSize[0] * scaleTarget.localScale[0]: " + unscaledSize[0] * scaleTarget.localScale[0] + 
        //         "\nunscaledSize[1] * scaleTarget.localScale[1]: " + unscaledSize[1] * scaleTarget.localScale[1] + 
        //         "\nunscaledSize[2] * scaleTarget.localScale[2]: " + unscaledSize[2] * scaleTarget.localScale[2]);
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = centerPivot.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        // Vector3 oldScale = scaleTarget.localScale;
        // Vector3 newScale = oldScale;
        // Debug.Log("New Scale: " + newScale);

        // Debug.Log("Volume After: " + volume);
        RecalculateMeasurements();
        // float hitAxisSizeLost;
        // float hitAxisShrinkFactor;
        // float preservedFactor;

        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        {
            HandlePosXHit(volumeShiftedOnHit);
        }

        // // Debug.Log("Volume before: " + volume);
        // if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
        // {
        //     // Debug.Log("Decreasing x");
        //     hitAxisSizeLost = volumeShiftedOnHit / area[0];
        //     hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
        //     newScale.x *= hitAxisShrinkFactor;

        //     preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
        //     // newScale.y *= preservedFactor;
        //     // newScale.z *= preservedFactor;

        //     float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        //     float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
        //     newScale.y *= yPreservedFactor;
        //     newScale.z *= nonYpreservedFactor;
        // }
        // else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
        // {
        //     // Debug.Log("Decreasing y");
        //     hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
        //     hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
        //     newScale.y *= hitAxisShrinkFactor;

        //     preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
        //     newScale.x *= preservedFactor;
        //     newScale.z *= preservedFactor;
        // }
        // else
        // {
        //     // Debug.Log("Decreasing z");
        //     hitAxisSizeLost = volumeShiftedOnHit / area[2];
        //     hitAxisShrinkFactor = (scaledSize[2] - hitAxisSizeLost) / scaledSize[2];
        //     newScale.z *= hitAxisShrinkFactor;

        //     preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
        //     // float yFactor = 1 + (totalPreservedFactor - 1) * yGrowthScale;
        //     // float nonYFactor = 1 + (totalPreservedFactor - 1) * (1 - yGrowthScale);
        //     float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        //     float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
        //     // Debug.Log("preservedFactor: " + preservedFactor);
        //     // Debug.Log("yPreservedFactor: " + yPreservedFactor);
        //     // Debug.Log("nonYpreservedFactor: " + nonYpreservedFactor);

        //     newScale.x *= nonYpreservedFactor;
        //     newScale.y *= yPreservedFactor;
        // }

        // if (newScale.x >= minScale.x && newScale.x <= maxScale.x &&
        //     newScale.y >= minScale.y && newScale.y <= maxScale.y &&
        //     newScale.z >= minScale.z && newScale.z <= maxScale.z)
        // {
        //     scaleTarget.localScale = newScale;
        //     onScaleChanged.Invoke(oldScale, newScale);
        //     return;
        // }
        // Debug.Log("Scale change rejected: newScale out of bounds");
    }

    void HandlePosXHit(float volumeShiftedOnHit)
    {
        // Debug.Log("Decreasing x");
        float change_in_hit_axis_length = volumeShiftedOnHit / area[0];
        float mult_to_hit_axis_scale = (scaledSize[0] - change_in_hit_axis_length) / scaledSize[0];
        ChangeScaleRight(mult_to_hit_axis_scale);

        float mult_to_other_axis_scale = volume / (scaledSize[0] * scaledSize[1] * mult_to_hit_axis_scale * scaledSize[2]);
        float mult_to_each_axis_scale = Mathf.Sqrt(mult_to_other_axis_scale);

        float mult_to_y_axis_scale = 1 + (mult_to_each_axis_scale - 1) * yGrowthScale;
        float mult_to_non_y_axis_scale = mult_to_each_axis_scale * mult_to_each_axis_scale / mult_to_y_axis_scale;
        MultiplyScaleAxis(centerPivot, mult_to_y_axis_scale, 'y');
        // ChangeSca
    }

    void MultiplyScaleAxis(Transform transform, float mult, char axis)
    {
        Vector3 newScale     = transform.localScale;
        if (axis == 'x')       newScale.x *= mult; 
        else if (axis == 'y')  newScale.y *= mult; 
        else                   newScale.z *= mult;
        transform.localScale = newScale;
    }

    void ChangeScaleRight(float mult)
    {
        MultiplyScaleAxis(top_right_corner.transform, mult, 'x');
        MultiplyScaleAxis(right_edge.transform, mult, 'x');
        MultiplyScaleAxis(bottom_right_corner.transform, mult, 'x');
    }
    void ChangeScaleLeft(float mult)
    {
        MultiplyScaleAxis(top_left_corner.transform, mult, 'x');
        MultiplyScaleAxis(left_edge.transform, mult, 'x');
        MultiplyScaleAxis(bottom_left_corner.transform, mult, 'x');
    }
    void ChangeScaleTop(float mult)
    {
        MultiplyScaleAxis(top_left_corner.transform, mult, 'z');
        MultiplyScaleAxis(top_edge.transform, mult, 'z');
        MultiplyScaleAxis(top_right_corner.transform, mult, 'z');
    }
    void ChangeScaleBottom(float mult)
    {
        MultiplyScaleAxis(bottom_left_corner.transform, mult, 'z');
        MultiplyScaleAxis(bottom_edge.transform, mult, 'z');
        MultiplyScaleAxis(bottom_right_corner.transform, mult, 'z');
    }
}
*/