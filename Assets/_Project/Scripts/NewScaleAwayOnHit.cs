using System;
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
    [SerializeField] string targetName = "Center Pivot";

    readonly int colliderLeftRightAxisIndex = 0;
    readonly int colliderBackForthAxisIndex = 1;
    readonly int colliderUpDownAxisIndex = 2;

    readonly int bodyLeftRightPosAxisIndex = 0;
    // readonly int bodyBackForthPosAxisIndex = 1;
    readonly int bodyUpDownPosAxisIndex = 2;

    readonly int bodyLeftRightScaleAxisIndex = 0;
    readonly int bodyBackForthScaleAxisIndex = 2;
    readonly int bodyUpDownScaleAxisIndex = 1;

    readonly int bodyLeftRightBoundsAxisIndex = 0;
    readonly int bodyBackForthBoundsAxisIndex = 2;
    readonly int bodyUpDownBoundsAxisIndex = 1;

    [SerializeField] GameObject left_edge;
    [SerializeField] GameObject right_edge;
    [SerializeField] GameObject top_edge;
    [SerializeField] GameObject bottom_edge;
    [SerializeField] GameObject top_left_corner;
    [SerializeField] GameObject top_right_corner;
    [SerializeField] GameObject bottom_left_corner;
    [SerializeField] GameObject bottom_right_corner;
    [SerializeField] GameObject hole_cover;
    [SerializeField] BoxCollider swordBodyCollider;

    [Header("Clamp Settings")]
    Vector3 minScale = new(0.01f, 0.01f, 0.01f);

    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.5f; 
    [SerializeField] float volumeShiftModifier = 1f;
    float minVelocity = 0.7f;
    float maxVelocity = 1.1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;
    [SerializeField] float squashBias = 0.33f;

    [Header("Events")]
    public UnityEvent onScaleChanged;

    Vector3 unscaledSize;
    Vector3 scaledSize;
    Vector3 area;
    float volume;
    float originalVolume = 0f;
    float maxVolumeShift;
    private float lastHitTime = 0f;
    List<GameObject> leftComponents;
    List<GameObject> middleXComponents;
    List<GameObject> rightComponents;
    List<GameObject> topComponents;
    List<GameObject> middleYComponents;
    List<GameObject> bottomComponents;
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

        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        // Debug.Log("Unscaled size: " + unscaledSize);
        RecalculateMeasurements();
        FixComponentPositions();

        // StartCoroutine(TestHit());
    }

    IEnumerator TestHit()
    {
        yield return new WaitForSeconds(2);
        RecalculateMeasurements();
        float volumeShiftedOnHit = Mathf.Clamp01(1f / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemp();
        // Debug.Log("temperatureScript.GetPercentMaxTemp(): " + temperatureScript.GetPercentMaxTemp());
        HandleDirectionalScale(new Vector3(0,1,0), volumeShiftedOnHit);
        StartCoroutine(TestHit());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!anvilAttachable.isOnAnvil) { return; }
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) { return; }
        ContactPoint contact = collision.GetContact(0);
        if (contact.thisCollider.name != targetName) { return; }
        Vector3 worldNormal = contact.normal;

        // VELOCITY
        float velocityMagnitude;
        Rigidbody hammerRb = collision.rigidbody;
        if (hammerRb == null) return;
        velocityMagnitude = hammerRb.linearVelocity.magnitude;
        if (velocityMagnitude < minVelocity || velocityMagnitude > maxVelocity) return;
        Debug.Log("velocityMagnitude: " + velocityMagnitude);

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        // VOLUME SHIFT
        float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemp();

        HandleDirectionalScale(worldNormal, volumeShiftedOnHit);
    }

    public void RecalculateMeasurements()
    {
        leftEdgeUnscaledSize = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        rightEdgeUnscaledSize = right_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        topEdgeUnscaledSize = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        bottomEdgeUnscaledSize = bottom_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] = leftEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex]
                         + topEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex]
                         + rightEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex];
        unscaledSize[1] = topEdgeUnscaledSize[bodyUpDownBoundsAxisIndex]
                         + leftEdgeUnscaledSize[bodyUpDownBoundsAxisIndex]
                         + bottomEdgeUnscaledSize[bodyUpDownBoundsAxisIndex];
        unscaledSize[2] = leftEdgeUnscaledSize[bodyBackForthBoundsAxisIndex];
        // Debug.Log("Unscaled Size: " + unscaledSize);

        // x = leftright y = topdown z = backforth
        scaledSize = new(
            leftEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex] * left_edge.transform.localScale[bodyLeftRightScaleAxisIndex]
             + topEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex] * top_edge.transform.localScale[bodyLeftRightScaleAxisIndex]
             + rightEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex] * right_edge.transform.localScale[bodyLeftRightScaleAxisIndex],
             
            topEdgeUnscaledSize[bodyUpDownBoundsAxisIndex] * top_edge.transform.localScale[bodyUpDownScaleAxisIndex]
             + leftEdgeUnscaledSize[bodyUpDownBoundsAxisIndex] * left_edge.transform.localScale[bodyUpDownScaleAxisIndex]
             + bottomEdgeUnscaledSize[bodyUpDownBoundsAxisIndex] * bottom_edge.transform.localScale[bodyUpDownScaleAxisIndex],

            topEdgeUnscaledSize[bodyBackForthBoundsAxisIndex] * top_edge.transform.localScale[bodyBackForthScaleAxisIndex]
        );
        Debug.Log("Scaled Size: " + scaledSize);

        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        if (originalVolume == 0f) { originalVolume = volume; }
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);
    }

    public void FixComponentPositions()
    {
        // FOR SOME REASON POS Z = ROT Y
        float halfWidthOfTopEdge = topEdgeUnscaledSize[bodyLeftRightBoundsAxisIndex] / 2;
        float halfHeightOfLeftEdge = leftEdgeUnscaledSize[bodyUpDownBoundsAxisIndex] / 2; // for some reason box colliders swap z and y
        
        top_left_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] - halfWidthOfTopEdge * top_edge.transform.localScale[bodyLeftRightScaleAxisIndex], 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        left_edge.transform.localPosition = new Vector3(
            top_left_corner.transform.localPosition[0],
            top_left_corner.transform.localPosition[1],
            top_left_corner.transform.localPosition[2] + halfHeightOfLeftEdge * left_edge.transform.localScale[bodyUpDownScaleAxisIndex]
        );
        bottom_left_corner.transform.localPosition = new Vector3(
            left_edge.transform.localPosition[0], 
            left_edge.transform.localPosition[1], 
            left_edge.transform.localPosition[2] + halfHeightOfLeftEdge * left_edge.transform.localScale[bodyUpDownScaleAxisIndex]
        );

        bottom_edge.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0], 
            bottom_left_corner.transform.localPosition[1], 
            bottom_left_corner.transform.localPosition[2]
        );

        top_right_corner.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[0] + halfWidthOfTopEdge * top_edge.transform.localScale[bodyLeftRightScaleAxisIndex], 
            top_edge.transform.localPosition[1], 
            top_edge.transform.localPosition[2]
        );
        right_edge.transform.localPosition = new Vector3(
            top_right_corner.transform.localPosition[0],
            top_right_corner.transform.localPosition[1],
            top_right_corner.transform.localPosition[2] + halfHeightOfLeftEdge * right_edge.transform.localScale[bodyUpDownScaleAxisIndex]
        );
        bottom_right_corner.transform.localPosition = new Vector3(
            right_edge.transform.localPosition[0], 
            right_edge.transform.localPosition[1], 
            right_edge.transform.localPosition[2] + halfHeightOfLeftEdge * right_edge.transform.localScale[bodyUpDownScaleAxisIndex]
        );

        // TODO: x = left right, y = top down, z = back forth
        hole_cover.transform.localScale = new Vector3(
            top_edge.transform.localScale[bodyLeftRightScaleAxisIndex], 
            left_edge.transform.localScale[bodyUpDownScaleAxisIndex],
            1
        );
        hole_cover.transform.localPosition = new Vector3(
            top_edge.transform.localPosition[bodyLeftRightPosAxisIndex], 
            0, 
            left_edge.transform.localPosition[bodyUpDownPosAxisIndex]
        );

        Vector3 newColliderSize = new();
        newColliderSize[colliderLeftRightAxisIndex] = scaledSize[bodyLeftRightScaleAxisIndex];
        newColliderSize[colliderUpDownAxisIndex] = scaledSize[bodyUpDownScaleAxisIndex];
        newColliderSize[colliderBackForthAxisIndex] = scaledSize[bodyBackForthScaleAxisIndex] * 0.95f;
        swordBodyCollider.size = newColliderSize;
        // swordBodyCollider.size = new Vector3(scaledSize.x, scaledSize.z, scaledSize.y);
        Vector3 newColliderPos = new(0,0,0);
        newColliderPos[colliderBackForthAxisIndex] = scaledSize[bodyBackForthScaleAxisIndex] / 2;
        swordBodyCollider.center = newColliderPos;

        onScaleChanged.Invoke();

    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 newScale = new Vector3(1,1,1);
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);

        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        if (absNormal.x >= absNormal.y && absNormal.x >= absNormal.z) { 
            int area_index = 0;

            float change_in_hit_axis_length = volumeShiftedOnHit / area[area_index];
            float change_in_hit_axis_scale = (scaledSize[area_index] - change_in_hit_axis_length) / scaledSize[area_index];
            newScale[bodyLeftRightScaleAxisIndex] *= change_in_hit_axis_scale;

            List<GameObject> c1Objects;
            List<GameObject> c2Objects = middleXComponents;
            List<GameObject> c3Objects;
            if (localNormal.x > 0)
            {
                c1Objects = leftComponents;
                c3Objects = rightComponents;
            }
            else
            {
                c1Objects = rightComponents;
                c3Objects = leftComponents;
            }
            GameObject c1Object = c1Objects[0];
            GameObject c2Object = c2Objects[0];
            GameObject c3Object = c3Objects[0];

            Vector3 biasedScales = GetBiasedScales(
                c1Object,
                c2Object,
                c3Object,
                area_index,
                bodyLeftRightBoundsAxisIndex,
                bodyLeftRightScaleAxisIndex,
                volumeShiftedOnHit,
                squashBias,
                0.1f
            );

            float biased_change_in_c1_axis_scale = biasedScales[0];
            float biased_change_in_c2_axis_scale = biasedScales[1];
            float biased_change_in_c3_axis_scale = biasedScales[2];
            
            float preservedFactor = Mathf.Sqrt(volume / (
                scaledSize[0] * change_in_hit_axis_scale * 
                scaledSize[1] *
                scaledSize[2]
            ));
            float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            newScale[bodyBackForthScaleAxisIndex] *= yPreservedFactor;
            newScale[bodyUpDownScaleAxisIndex] *= nonYpreservedFactor;
            // newScale[bodyUpDownScaleAxisIndex] *= preservedFactor;
            // newScale[bodyBackForthScaleAxisIndex] *= preservedFactor;

            if (
                IsNewScaleWithinBounds(
                    newScale, 
                    c1Objects[0].transform.localScale[0] * biased_change_in_c1_axis_scale, 
                    c2Objects[0].transform.localScale[0] * biased_change_in_c2_axis_scale, 
                    c3Objects[0].transform.localScale[0] * biased_change_in_c3_axis_scale
                )
            )
            {
                foreach (GameObject component in c1Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= biased_change_in_c1_axis_scale;
                    newComponentScale[bodyUpDownScaleAxisIndex] *= newScale[bodyUpDownScaleAxisIndex]; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * biased_change_in_c1_axis_scale, 
                    //     newComponentScale[1] * newScale[bodyUpDownScaleAxisIndex], 
                    //     newComponentScale[2] * newScale[bodyBackForthScaleAxisIndex]
                    // );
                }
                foreach (GameObject component in c2Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= biased_change_in_c2_axis_scale;
                    newComponentScale[bodyUpDownScaleAxisIndex] *= newScale[bodyUpDownScaleAxisIndex]; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * biased_change_in_c2_axis_scale, 
                    //     newComponentScale[1] * newScale[bodyUpDownScaleAxisIndex], 
                    //     newComponentScale[2] * newScale[bodyBackForthScaleAxisIndex]
                    // );
                }
                foreach (GameObject component in c3Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= biased_change_in_c3_axis_scale;
                    newComponentScale[bodyUpDownScaleAxisIndex] *= newScale[bodyUpDownScaleAxisIndex]; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * biased_change_in_c3_axis_scale, 
                    //     newComponentScale[1] * newScale.y, 
                    //     newComponentScale[2] * newScale.z
                    // );
                }

                RecalculateMeasurements();
                FixComponentPositions();
                // Debug.Log(
                //     "X HIT - Volume: " + volume + 
                //     "\nvolume shifted: " + volumeShiftedOnHit +
                //     "\narea: " + area +
                //     "\nbiased_change_in_c1_axis_scale: " + biased_change_in_c1_axis_scale + 
                //     "\nbiased_change_in_c2_axis_scale: " + biased_change_in_c2_axis_scale + 
                //     "\nbiased_change_in_c3_axis_scale: " + biased_change_in_c3_axis_scale
                // );
                return;
            }
        }
        else if (absNormal.z >= absNormal.x && absNormal.z >= absNormal.y) { 
            int area_index = 1;

            float change_in_hit_axis_length = volumeShiftedOnHit / area[area_index];
            float change_in_hit_axis_scale = (scaledSize[area_index] - change_in_hit_axis_length) / scaledSize[area_index];
            newScale[bodyUpDownScaleAxisIndex] *= change_in_hit_axis_scale;

            List<GameObject> c1Objects;
            List<GameObject> c2Objects = middleYComponents;
            List<GameObject> c3Objects;

            // TODO: I GOT NO IDEA WHY THIS HAS TO BE FLIPPED
            if (localNormal.z > 0)
            {
                c1Objects = topComponents;
                c3Objects = bottomComponents;
            }
            else
            {
                c1Objects = bottomComponents;
                c3Objects = topComponents;
            }
            GameObject c1Object = c1Objects[0];
            GameObject c2Object = c2Objects[0];
            GameObject c3Object = c3Objects[0];

            Vector3 biasedScales = GetBiasedScales(
                c1Object,
                c2Object,
                c3Object,
                area_index,
                bodyUpDownBoundsAxisIndex,
                bodyUpDownScaleAxisIndex,
                volumeShiftedOnHit,
                squashBias,
                0.1f
            );

            float biased_change_in_c1_axis_scale = biasedScales[0];
            float biased_change_in_c2_axis_scale = biasedScales[1];
            float biased_change_in_c3_axis_scale = biasedScales[2];

            float preservedFactor = Mathf.Sqrt(volume / (
                scaledSize[0] * change_in_hit_axis_scale * 
                scaledSize[1] *
                scaledSize[2]
            ));
            float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
            float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
            newScale[bodyBackForthScaleAxisIndex] *= yPreservedFactor;
            newScale[bodyLeftRightScaleAxisIndex] *= nonYpreservedFactor;
            // newScale[bodyLeftRightScaleAxisIndex] *= preservedFactor;
            // newScale[bodyBackForthScaleAxisIndex] *= preservedFactor;

            if (
                IsNewScaleWithinBounds(
                    newScale, 
                    c1Objects[0].transform.localScale[bodyUpDownScaleAxisIndex] * biased_change_in_c1_axis_scale, 
                    c2Objects[0].transform.localScale[bodyUpDownScaleAxisIndex] * biased_change_in_c2_axis_scale, 
                    c3Objects[0].transform.localScale[bodyUpDownScaleAxisIndex] * biased_change_in_c3_axis_scale
                )
            )
            {
                foreach (GameObject component in c1Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= newScale[bodyLeftRightScaleAxisIndex];
                    newComponentScale[bodyUpDownScaleAxisIndex] *= biased_change_in_c1_axis_scale; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * newScale.x, 
                    //     newComponentScale[1] * biased_change_in_c1_axis_scale, 
                    //     newComponentScale[2] * newScale.z
                    // );
                }
                foreach (GameObject component in c2Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= newScale[bodyLeftRightScaleAxisIndex];
                    newComponentScale[bodyUpDownScaleAxisIndex] *= biased_change_in_c2_axis_scale; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * newScale.x, 
                    //     newComponentScale[1] * biased_change_in_c2_axis_scale, 
                    //     newComponentScale[2] * newScale.z
                    // );
                }
                foreach (GameObject component in c3Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyLeftRightScaleAxisIndex] *= newScale[bodyLeftRightScaleAxisIndex];
                    newComponentScale[bodyUpDownScaleAxisIndex] *= biased_change_in_c3_axis_scale; 
                    newComponentScale[bodyBackForthScaleAxisIndex] *= newScale[bodyBackForthScaleAxisIndex];
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * newScale.x, 
                    //     newComponentScale[1] * biased_change_in_c3_axis_scale, 
                    //     newComponentScale[2] * newScale.z
                    // );
                }

                RecalculateMeasurements();
                FixComponentPositions();
                // Debug.Log(
                //     "Z HIT - Volume: " + volume + 
                //     "\nvolume shifted: " + volumeShiftedOnHit +
                //     "\narea: " + area +
                //     "\nbiased_change_in_c1_axis_scale: " + biased_change_in_c1_axis_scale + 
                //     "\nbiased_change_in_c2_axis_scale: " + biased_change_in_c2_axis_scale + 
                //     "\nbiased_change_in_c3_axis_scale: " + biased_change_in_c3_axis_scale
                // );
                return;
            }
        }
        // else
        // {
        //     Debug.Log("Y Hit Detected");
        // }
        
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(Vector3 newScale, float c1Scale, float c2Scale, float c3Scale)
    {
        return newScale.x >= minScale.x &&
            newScale.y >= minScale.y &&
            newScale.z >= minScale.z &&

            // TODO: MAKE A CUSTOM MIN MAX FOR COMPONENTS
            c1Scale >= minScale.x / 10 && 
            c2Scale >= minScale.x / 10 && 
            c3Scale >= minScale.x / 10;
    }

    Vector3 GetBiasedScales(
        GameObject c1Object,
        GameObject c2Object,
        GameObject c3Object,
        int area_index,
        int bounds_axis_index,
        int scale_axis_index,
        float volumeShiftedOnHit,
        float bonusPercentVolumeLostToC1, // should be >1 to represent more volume lost
        float percentRemainingVolumeLostToC2 // should be <1. 0.5 for perfectly split between c2 and c3. >0.5 for larger c2.
    )
    {
        float area_in_axis = area[area_index];
        float change_in_hit_axis_length = volumeShiftedOnHit / area[area_index];
        float change_in_hit_axis_scale = (scaledSize[area_index] - change_in_hit_axis_length) / scaledSize[area_index];

        float c1_scaled_axis_length = c1Object.GetComponent<MeshFilter>().sharedMesh.bounds.size[bounds_axis_index] * c1Object.transform.localScale[scale_axis_index];
        float change_in_c1_axis_length = c1_scaled_axis_length * change_in_hit_axis_scale - c1_scaled_axis_length;
        float change_in_c1_vol = change_in_c1_axis_length * area_in_axis;

        float c2_scaled_axis_length = c2Object.GetComponent<MeshFilter>().sharedMesh.bounds.size[bounds_axis_index] * c2Object.transform.localScale[scale_axis_index];
        float change_in_c2_axis_length = c2_scaled_axis_length * change_in_hit_axis_scale - c2_scaled_axis_length;
        float change_in_c2_vol = change_in_c2_axis_length * area_in_axis;

        float c3_scaled_axis_length = c3Object.GetComponent<MeshFilter>().sharedMesh.bounds.size[bounds_axis_index] * c3Object.transform.localScale[scale_axis_index];

        float biased_change_in_c1_vol = change_in_c1_vol * bonusPercentVolumeLostToC1;
        float biased_change_in_c1_axis_length = biased_change_in_c1_vol / area_in_axis;
        
        // THIS WILL BE POSITIVE
        float remaining_volume = change_in_c1_vol - biased_change_in_c1_vol;

        float biased_change_in_c2_vol = change_in_c2_vol + remaining_volume * percentRemainingVolumeLostToC2;
        // Debug.Log(
        //     "POObiased_change_in_c1_vol " + biased_change_in_c1_vol + 
        //     "\n = change_in_c2_vol: " + change_in_c2_vol + 
        //     "\n + remaining_volume: " + remaining_volume + 
        //     "\n * percentRemainingVolumeLostToC2: " + percentRemainingVolumeLostToC2
        // );
        float biased_change_in_c2_axis_length = biased_change_in_c2_vol / area_in_axis;

        float biased_change_in_c3_vol = -volumeShiftedOnHit - biased_change_in_c1_vol - biased_change_in_c2_vol;
        float biased_change_in_c3_axis_length = biased_change_in_c3_vol / area_in_axis;

        float biased_change_in_c1_axis_scale = (c1_scaled_axis_length + biased_change_in_c1_axis_length) / c1_scaled_axis_length;
        float biased_change_in_c2_axis_scale = (c2_scaled_axis_length + biased_change_in_c2_axis_length) / c2_scaled_axis_length;
        float biased_change_in_c3_axis_scale = (c3_scaled_axis_length + biased_change_in_c3_axis_length) / c3_scaled_axis_length;
        // Debug.Log(
        //     "\nbiased_change_in_c1_vol: " + biased_change_in_c1_vol + 
        //     "\nbiased_change_in_c2_vol: " + biased_change_in_c2_vol + 
        //     "\nbiased_change_in_c3_vol: " + biased_change_in_c3_vol
        // );
        return new (
            biased_change_in_c1_axis_scale,
            biased_change_in_c2_axis_scale,
            biased_change_in_c3_axis_scale
        );
    }
}
