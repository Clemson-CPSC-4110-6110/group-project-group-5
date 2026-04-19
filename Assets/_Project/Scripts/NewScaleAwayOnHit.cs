using System.Collections;
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

        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        Debug.Log("Unscaled size: " + unscaledSize);
        RecalculateMeasurements();
        FixComponentPositions();

        StartCoroutine(TestHit());
    }

    IEnumerator TestHit()
    {
        yield return new WaitForSeconds(2);
        float volumeShiftedOnHit = Mathf.Clamp01(1f / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();
        Vector3 newScale = CalculateNewScaleOnXHit(volumeShiftedOnHit);

        if   (IsNewScaleWithinBounds(newScale)) { ChangeScales(newScale, 'x'); }
        else { Debug.Log("Scale change rejected: newScale out of bounds"); }

        StartCoroutine(TestHit());
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

        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] += leftEdgeSize[0] + topEdgeSize[0] + rightEdgeSize[0];
        unscaledSize[0] /= scaleTarget.localScale[0];
        unscaledSize[1] += leftEdgeSize[1];
        unscaledSize[1] /= scaleTarget.localScale[1];
        unscaledSize[2] += topEdgeSize[2] + leftEdgeSize[2] + bottomEdgeSize[2];
        unscaledSize[2] /= scaleTarget.localScale[2];

        // Debug.Log("\nunscaledSize[0] * scaleTarget.localScale[0]: " + unscaledSize[0] * scaleTarget.localScale[0] + 
        //         "\nunscaledSize[1] * scaleTarget.localScale[1]: " + unscaledSize[1] * scaleTarget.localScale[1] + 
        //         "\nunscaledSize[2] * scaleTarget.localScale[2]: " + unscaledSize[2] * scaleTarget.localScale[2]);
    }

    void FixComponentPositions()
    {
        // FOR SOME REASON POS Z = ROT Y
        float halfWidthOfTopEdge = topEdgeSize[0] / 2;
        float halfHeightOfLeftEdge = leftEdgeSize.y / 2; // for some reason box colliders swap z and y



        top_left_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] - halfWidthOfTopEdge * top_edge.transform.localScale.x, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        left_edge.transform.localPosition = new Vector3(
            top_left_corner.transform.localPosition[0],
            top_left_corner.transform.localPosition[1],
            top_left_corner.transform.localPosition[2] + halfHeightOfLeftEdge * top_left_corner.transform.localScale.y
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            left_edge.transform.localPosition[0], 
            left_edge.transform.localPosition[1], 
            left_edge.transform.localPosition[2] + halfHeightOfLeftEdge * left_edge.transform.localScale.y
        );

        bottom_edge.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0], 
            bottom_left_corner.transform.localPosition[1], 
            bottom_left_corner.transform.localPosition[2]
        );

        top_right_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] + halfWidthOfTopEdge * top_edge.transform.localScale.x, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        right_edge.transform.localPosition = new Vector3(
            top_right_corner.transform.localPosition[0],
            top_right_corner.transform.localPosition[1],
            top_right_corner.transform.localPosition[2] + halfHeightOfLeftEdge * top_right_corner.transform.localScale.y
        );
        bottom_right_corner.transform.localPosition = new Vector3(
            right_edge.transform.localPosition[0], 
            right_edge.transform.localPosition[1], 
            right_edge.transform.localPosition[2] + halfHeightOfLeftEdge * right_edge.transform.localScale.y
        );
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        Vector3 newScale;
        Debug.Log("Volume After: " + volume);
        RecalculateMeasurements();
        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z) { 
            newScale = CalculateNewScaleOnXHit(volumeShiftedOnHit); 
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'x');
                return;
            }
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z) { 
            newScale = CalculateNewScaleOnYHit(volumeShiftedOnHit); 
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'z');
                return;
            }
        }
        else { 
            newScale = CalculateNewScaleOnZHit(volumeShiftedOnHit); 
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'y');
                return;
            }
        }
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(Vector3 newScale)
    {
        return newScale.x >= minScale.x && newScale.x <= maxScale.x &&
               newScale.y >= minScale.y && newScale.y <= maxScale.y &&
               newScale.z >= minScale.z && newScale.z <= maxScale.z;
    }

    void ChangeScales(Vector3 newScale, char hitAxis)
    {
        Vector3 oldScale = CalculateOldScale();
        // foreach (GameObject component in allComponents)
        // {
        //     Vector3 newComponentScale = component.transform.localScale;
        //     newComponentScale = new(
        //         newComponentScale[0] * newScale.x, 
        //         newComponentScale[1] * newScale.y, 
        //         newComponentScale[2] * newScale.z
        //     );
        //     component.transform.localScale = newComponentScale;
        // }
        
        if (hitAxis == 'x')
        {
            float scaleXComponent1 = (newScale.x - 1) * 0.95f + 1;
            float scaleXComponent2 = Mathf.Sqrt(newScale.x / scaleXComponent1);
            if (scaleXComponent1 * scaleXComponent2 * scaleXComponent2 != newScale.x) { Debug.Log("Incorrect scale calculations"); }
            foreach (GameObject component in leftComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(
                    newComponentScale[0] * scaleXComponent1, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
                component.transform.localScale = newComponentScale;
            }
            foreach (GameObject component in middleXComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(
                    newComponentScale[0] * scaleXComponent2, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
                component.transform.localScale = newComponentScale;
            }
            foreach (GameObject component in rightComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                newComponentScale = new(
                    newComponentScale[0] * scaleXComponent2, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
                component.transform.localScale = newComponentScale;
            }
        }

        onScaleChanged.Invoke(oldScale, newScale);
        FixComponentPositions();
    }

    void MultiplyScaleAxis(Transform transform, float mult, char axis)
    {
        Vector3 newScale     = transform.localScale;
        if (axis == 'x')       newScale.x *= mult; 
        else if (axis == 'y')  newScale.y *= mult; 
        else                   newScale.z *= mult;
        transform.localScale = newScale;
    }

    Vector3 CalculateOldScale()
    {
        return new(
            (left_edge.transform.localScale[0] + top_edge.transform.localScale[0] + right_edge.transform.localScale[0]) / 3,
            left_edge.transform.localScale[1],
            (top_edge.transform.localScale[2] + left_edge.transform.localScale[2] + bottom_edge.transform.localScale[2]) / 3
        );
    }

    Vector3 CalculateNewScaleOnXHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float hitAxisSizeLost = volumeShiftedOnHit / area[0];
        float hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
        newScale.x *= hitAxisShrinkFactor;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
        float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;

        newScale.y *= yPreservedFactor;
        newScale.z *= nonYpreservedFactor;
        Debug.Log(
            "\nX Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + hitAxisSizeLost +
            "\nhit axis scale mult: " + hitAxisShrinkFactor +
            "\ny axis scale mult: " + yPreservedFactor +
            "\nnon y axis scale mult: " + nonYpreservedFactor
        );
        return newScale;
    }

    Vector3 CalculateNewScaleOnYHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float hitAxisSizeLost = volumeShiftedOnHit * yGrowthScale / area[1];
        float hitAxisShrinkFactor = (scaledSize[1] - hitAxisSizeLost) / scaledSize[1];
        newScale.y *= hitAxisShrinkFactor;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));

        newScale.x *= preservedFactor;
        newScale.z *= preservedFactor;

        Debug.Log(
            "\nY Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + hitAxisSizeLost +
            "\nhit axis scale mult: " + hitAxisShrinkFactor +
            "\nother axis scale mult: " + preservedFactor
        );
        return newScale;
    }

    Vector3 CalculateNewScaleOnZHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float hitAxisSizeLost = volumeShiftedOnHit / area[2];
        float hitAxisShrinkFactor = (scaledSize[2] - hitAxisSizeLost) / scaledSize[2];
        newScale.z *= hitAxisShrinkFactor;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
        float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;

        newScale.x *= nonYpreservedFactor;
        newScale.y *= yPreservedFactor;

        Debug.Log(
            "\nZ Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + hitAxisSizeLost +
            "\nhit axis scale mult: " + hitAxisShrinkFactor +
            "\ny axis scale mult: " + yPreservedFactor +
            "\nnon y axis scale mult: " + nonYpreservedFactor
        );

        return newScale;
    }
}
