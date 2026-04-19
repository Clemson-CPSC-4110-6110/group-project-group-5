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
    [SerializeField] GameObject hole_cover;

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
    [SerializeField] float squashBias = 0.33f;

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
    Vector3 leftEdgeUnscaledSize;
    Vector3 rightEdgeUnscaledSize;
    Vector3 topEdgeUnscaledSize;
    Vector3 bottomEdgeUnscaledSize;

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
        RecalculateMeasurements();
        float volumeShiftedOnHit = Mathf.Clamp01(1f / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();
        HandleDirectionalScale(new Vector3(1,0,0), volumeShiftedOnHit);

        // Vector3 newScale = CalculateNewScaleOnXHit(volumeShiftedOnHit);
        // if   (IsNewScaleWithinBounds(newScale)) { ChangeScales(newScale, 'x', false); }

        // Vector3 newScale = CalculateNewScaleOnZHit(volumeShiftedOnHit);
        // if   (IsNewScaleWithinBounds(newScale)) { ChangeScales(newScale, 'z', false); }

        // Vector3 newScale = CalculateNewScaleOnYHit(volumeShiftedOnHit);
        // if   (IsNewScaleWithinBounds(newScale)) { ChangeScales(newScale, 'y', false); }

        // else { Debug.Log("Scale change rejected: newScale out of bounds"); }

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
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        // VOLUME SHIFT
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();

        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);
    }

    void RecalculateMeasurements()
    {
        leftEdgeUnscaledSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        rightEdgeUnscaledSize = right_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        topEdgeUnscaledSize = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        bottomEdgeUnscaledSize = bottom_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        scaledSize = new(
            leftEdgeUnscaledSize[0] * left_edge.transform.localScale.x + topEdgeUnscaledSize[0] * top_edge.transform.localScale.x + rightEdgeUnscaledSize[0] * right_edge.transform.localScale.x,
            leftEdgeUnscaledSize[1] * left_edge.transform.localScale.z,
            topEdgeUnscaledSize[2] * left_edge.transform.localScale.y + leftEdgeUnscaledSize[2] * left_edge.transform.localScale.y + bottomEdgeUnscaledSize[2] * left_edge.transform.localScale.y
        );
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        Debug.Log("Volume = " + scaledSize[0] + " * " + scaledSize[1] + " * " + scaledSize[2] + " = " + volume);
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);

        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] += leftEdgeUnscaledSize[0] + topEdgeUnscaledSize[0] + rightEdgeUnscaledSize[0];
        unscaledSize[1] += leftEdgeUnscaledSize[1];
        unscaledSize[2] += topEdgeUnscaledSize[2] + leftEdgeUnscaledSize[2] + bottomEdgeUnscaledSize[2];
    }

    void FixComponentPositions()
    {
        // FOR SOME REASON POS Z = ROT Y
        float halfWidthOfTopEdge = topEdgeUnscaledSize[0] / 2;
        float halfHeightOfLeftEdge = leftEdgeUnscaledSize.y / 2; // for some reason box colliders swap z and y
        Debug.Log("halfWidthOfTopEdge * top_edge.transform.localScale.x: " + halfWidthOfTopEdge * top_edge.transform.localScale.x);
        top_left_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] - halfWidthOfTopEdge * top_edge.transform.localScale.x, 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        left_edge.transform.localPosition = new Vector3(
            top_left_corner.transform.localPosition[0],
            top_left_corner.transform.localPosition[1],
            top_left_corner.transform.localPosition[2] + halfHeightOfLeftEdge * left_edge.transform.localScale.y
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
            top_right_corner.transform.localPosition[2] + halfHeightOfLeftEdge * right_edge.transform.localScale.y
        );
        bottom_right_corner.transform.localPosition = new Vector3(
            right_edge.transform.localPosition[0], 
            right_edge.transform.localPosition[1], 
            right_edge.transform.localPosition[2] + halfHeightOfLeftEdge * right_edge.transform.localScale.y
        );
        hole_cover.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0], 
            0.0025f, 
            top_edge.transform.localPosition[2] + halfHeightOfLeftEdge * hole_cover.transform.localScale.y
        );
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 oldScale = CalculateOldScale();
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
            // newScale = oldScale;
            // float change_in_hit_axis_length = volumeShiftedOnHit / area[0];
            // float change_in_hit_axis_scale = (scaledSize[0] - change_in_hit_axis_length) / scaledSize[0];
            // newScale.x *= change_in_hit_axis_scale;

            // // float c1_scaled_axis_length = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * left_edge.transform.localScale.x;
            // // float change_in_c1_axis_length = c1_scaled_axis_length * change_in_hit_axis_scale - c1_scaled_axis_length;
            // // float change_in_c1_vol = change_in_c1_axis_length * area[0];

            // // float c2_scaled_axis_length = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * top_edge.transform.localScale.x;
            // // float change_in_c2_axis_length = c2_scaled_axis_length * change_in_hit_axis_scale - c2_scaled_axis_length;
            // // float change_in_c2_vol = change_in_c2_axis_length * area[0];

            // // float c3_scaled_axis_length = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * top_edge.transform.localScale.x;
            // // float change_in_c3_axis_length = c3_scaled_axis_length * change_in_hit_axis_scale - c3_scaled_axis_length;
            // // float change_in_c3_vol = change_in_c3_axis_length * area[0];

            // // float biased_change_in_c1_vol = change_in_c1_vol * squashBias;
            // // float biased_change_in_c1_axis_length = biased_change_in_c1_vol / area[0];

            // // float remaining_volume = change_in_c1_vol - biased_change_in_c1_vol;

            // // float biased_change_in_c2_vol = change_in_c2_vol + remaining_volume / 2;
            // // float biased_change_in_c2_axis_length = biased_change_in_c2_vol / area[0];

            // // float biased_change_in_c3_vol = change_in_c3_vol + remaining_volume / 2;
            // // float biased_change_in_c3_axis_length = biased_change_in_c3_vol / area[0];

            // // float biased_change_in_c1_axis_scale = (c1_scaled_axis_length + biased_change_in_c1_axis_length) / c1_scaled_axis_length;
            // // float biased_change_in_c2_axis_scale = (c2_scaled_axis_length + biased_change_in_c2_axis_length) / c2_scaled_axis_length;
            // // float biased_change_in_c3_axis_scale = (c3_scaled_axis_length + biased_change_in_c3_axis_length) / c3_scaled_axis_length;

            // float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * change_in_hit_axis_scale * scaledSize[2]));
            // float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
            // float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;

            // newScale.y *= yPreservedFactor;
            // newScale.z *= nonYpreservedFactor;

            // Debug.Log(
            //     "\nX Hit--------------------------------" + 
            //     "\nvolume shifted: " + volumeShiftedOnHit +
            //     "\nhit axis length change: " + change_in_hit_axis_length +
            //     "\nhit axis scale mult: " + change_in_hit_axis_scale +
            //     "\ny axis scale mult: " + yPreservedFactor +
            //     "\nnon y axis scale mult: " + nonYpreservedFactor
            // );
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'x', localNormal.x < 0);
                // foreach (GameObject component in allComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * newScale.x, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
                return;
                // if (localNormal.x > 0)
                // foreach (GameObject component in leftComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * biased_change_in_c1_axis_scale, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
                // foreach (GameObject component in middleXComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * biased_change_in_c2_axis_scale, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
                // foreach (GameObject component in rightComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * biased_change_in_c3_axis_scale, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
                // return;
            }
        }
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z) { 
            newScale = CalculateNewScaleOnYHit(volumeShiftedOnHit); 
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'y', localNormal.y < 0);
                // foreach (GameObject component in allComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * newScale.x, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
                return;
            }
        }
        else { 
            newScale = CalculateNewScaleOnZHit(volumeShiftedOnHit); 
            if (IsNewScaleWithinBounds(newScale))
            {
                ChangeScales(newScale, 'z', localNormal.z < 0);
                // foreach (GameObject component in allComponents)
                // {
                //     Vector3 newComponentScale = component.transform.localScale;
                //     component.transform.localScale = new(
                //         newComponentScale[0] * newScale.x, 
                //         newComponentScale[1] * newScale.y, 
                //         newComponentScale[2] * newScale.z
                //     );
                // }
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

    void ChangeScales(Vector3 newScale, char hitAxis, bool isNegativeAxis)
    {
        if (hitAxis == 'x')
        {
            foreach (GameObject component in allComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                component.transform.localScale = new(
                    newComponentScale[0] * newScale.x, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
            }
            // List<GameObject> hitComponents = isNegativeAxis ? leftComponents : rightComponents;
            // List<GameObject> oppositeHitComponents = isNegativeAxis ? rightComponents : leftComponents;

            // float component1Length = hitComponents[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * hitComponents[0].transform.localScale.x;
            // float component2Length = middleXComponents[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * middleXComponents[0].transform.localScale.x;
            // float component3Length = oppositeHitComponents[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * oppositeHitComponents[0].transform.localScale.x;

            // float component1Area = hitComponents[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.y * hitComponents[0].transform.localScale.y * hitComponents[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * hitComponents[0].transform.localScale.z;
            // float component2Area = hitComponents[1].GetComponent<MeshFilter>().sharedMesh.bounds.size.y * hitComponents[1].transform.localScale.y * hitComponents[1].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * hitComponents[1].transform.localScale.z;
            // float component3Area = hitComponents[2].GetComponent<MeshFilter>().sharedMesh.bounds.size.y * hitComponents[2].transform.localScale.y * hitComponents[2].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * hitComponents[2].transform.localScale.z;

            // float scaleComponent1 = newScale.x * squashBias;
            // float scaleComponent2 = (newScale.x * 3 - scaleComponent1) / 2;

            // Debug.Log("Volume Lost From Component 1: " + component1Area * component1Length * (1 - scaleComponent1));
            // Debug.Log("Volume Lost From Component 2: " + component2Area * component2Length * (1 - scaleComponent1));
            // Debug.Log("Volume Lost From Component 3: " + component3Area * component3Length * (1 - scaleComponent1));
            // if (scaleComponent1 + scaleComponent2 + scaleComponent2 != newScale.x) { Debug.Log("Incorrect scale calculations"); }
            // Debug.Log("newScale.x = " + newScale.x + " | scaleComponent1: " + scaleComponent1 + " | scaleComponent2: " + scaleComponent2);
            // // if (scaleComponent1 * scaleComponent2 * scaleComponent2 != newScale.x) { Debug.Log("Incorrect scale calculations"); }

            // foreach (GameObject component in hitComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * scaleComponent1, 
            //         newComponentScale[1] * newScale.y, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
            // foreach (GameObject component in middleXComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * scaleComponent2, 
            //         newComponentScale[1] * newScale.y, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
            // foreach (GameObject component in oppositeHitComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * scaleComponent2, 
            //         newComponentScale[1] * newScale.y, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
        }
        else if (hitAxis == 'y')
        {
            foreach (GameObject component in allComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                component.transform.localScale = new(
                    newComponentScale[0] * newScale.x, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
            }
        }
        else
        {
            foreach (GameObject component in allComponents)
            {
                Vector3 newComponentScale = component.transform.localScale;
                component.transform.localScale = new(
                    newComponentScale[0] * newScale.x, 
                    newComponentScale[1] * newScale.y, 
                    newComponentScale[2] * newScale.z
                );
            }
            // float scaleComponent1 = newScale.z * squashBias;
            // float scaleComponent2 = (newScale.z * 3 - scaleComponent1) / 2;
            // if (scaleComponent1 + scaleComponent2 + scaleComponent2 != newScale.z) { Debug.Log("Incorrect scale calculations"); }
            // // if (scaleComponent1 * scaleComponent2 * scaleComponent2 != newScale.z) { Debug.Log("Incorrect scale calculations"); }
            // List<GameObject> hitComponents = isNegativeAxis ? bottomComponents : topComponents;
            // List<GameObject> oppositeHitComponents = isNegativeAxis ? topComponents : bottomComponents;

            // foreach (GameObject component in hitComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * newScale.x, 
            //         newComponentScale[1] * scaleComponent1, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
            // foreach (GameObject component in middleYComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * newScale.x, 
            //         newComponentScale[1] * scaleComponent2, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
            // foreach (GameObject component in oppositeHitComponents)
            // {
            //     Vector3 newComponentScale = component.transform.localScale;
            //     component.transform.localScale = new(
            //         newComponentScale[0] * newScale.x, 
            //         newComponentScale[1] * scaleComponent2, 
            //         newComponentScale[2] * newScale.z
            //     );
            // }
        }
        hole_cover.transform.localScale = new(
            top_edge.transform.localScale.x,
            left_edge.transform.localScale.y,
            1
        );
        // onScaleChanged.Invoke(oldScale, newScale);
        FixComponentPositions();
    }

    Vector3 CalculateOldScale()
    {
        return new(
            (left_edge.transform.localScale[0] + top_edge.transform.localScale[0] + right_edge.transform.localScale[0]) / 3,
            left_edge.transform.localScale[1],
            (top_edge.transform.localScale[2] + left_edge.transform.localScale[2] + bottom_edge.transform.localScale[2]) / 3
        );
    }

    /// <summary>
    /// SHRINK X SCALE. GROW X AND Y SCALE.
    /// </summary>
    /// <param name="volumeShiftedOnHit"></param>
    /// <returns></returns>
    Vector3 CalculateNewScaleOnXHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float change_in_hit_axis_length = volumeShiftedOnHit / area[0];
        float change_in_hit_axis_scale = (scaledSize[0] - change_in_hit_axis_length) / scaledSize[0];
        newScale.x *= change_in_hit_axis_scale;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * change_in_hit_axis_scale * scaledSize[2]));
        float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;

        newScale.y *= yPreservedFactor;
        newScale.z *= nonYpreservedFactor;
        Debug.Log(
            "\nX Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + change_in_hit_axis_length +
            "\nhit axis scale mult: " + change_in_hit_axis_scale +
            "\ny axis scale mult: " + yPreservedFactor +
            "\nnon y axis scale mult: " + nonYpreservedFactor
        );
        return newScale;
    }

    /// <summary>
    /// SHRINK Y SCALE. GROW X AND Z SCALE.
    /// </summary>
    /// <param name="volumeShiftedOnHit"></param>
    /// <returns></returns>
    Vector3 CalculateNewScaleOnZHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float change_in_hit_axis_length = volumeShiftedOnHit * yGrowthScale / area[1];
        float change_in_hit_axis_scale = (scaledSize[1] - change_in_hit_axis_length) / scaledSize[1];
        newScale.y *= change_in_hit_axis_scale;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * change_in_hit_axis_scale * scaledSize[2]));

        newScale.x *= preservedFactor;
        newScale.z *= preservedFactor;

        Debug.Log(
            "\nY Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + change_in_hit_axis_length +
            "\nhit axis scale mult: " + change_in_hit_axis_scale +
            "\nother axis scale mult: " + preservedFactor
        );
        return newScale;
    }

    /// <summary>
    /// SHRINK Z SCALE. GROW X AND Y SCALE.
    /// </summary>
    /// <param name="volumeShiftedOnHit"></param>
    /// <returns></returns>
    Vector3 CalculateNewScaleOnYHit(float volumeShiftedOnHit)
    {
        Vector3 newScale = CalculateOldScale();
        float change_in_hit_axis_length = volumeShiftedOnHit / area[2];
        float change_in_hit_axis_scale = (scaledSize[2] - change_in_hit_axis_length) / scaledSize[2];
        newScale.z *= change_in_hit_axis_scale;

        float preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * change_in_hit_axis_scale * scaledSize[2]));
        float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
        float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;

        newScale.x *= nonYpreservedFactor;
        newScale.y *= yPreservedFactor;

        Debug.Log(
            "\nZ Hit--------------------------------" + 
            "\nvolume shifted: " + volumeShiftedOnHit +
            "\nhit axis length change: " + change_in_hit_axis_length +
            "\nhit axis scale mult: " + change_in_hit_axis_scale +
            "\ny axis scale mult: " + yPreservedFactor +
            "\nnon y axis scale mult: " + nonYpreservedFactor
        );

        return newScale;
    }
}
