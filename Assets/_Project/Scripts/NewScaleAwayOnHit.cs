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
        // Debug.Log("Unscaled size: " + unscaledSize);
        RecalculateMeasurements();
        FixComponentPositions();

        // StartCoroutine(TestHit());
    }

    IEnumerator TestHit()
    {
        yield return new WaitForSeconds(2);
        RecalculateMeasurements();
        float volumeShiftedOnHit = Mathf.Clamp01(1f / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();
        HandleDirectionalScale(new Vector3(0,1,0), volumeShiftedOnHit);
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
        // Debug.Log("Sword Body Collision Detected");
        if (!anvilAttachable.isOnAnvil) { return; }
        if (collision.contactCount == 0 || !collision.gameObject.CompareTag("hammer")) { return; }
        ContactPoint contact = collision.GetContact(0);
        if (contact.thisCollider.name != targetName) { return; }
        // Debug.Log("contact name: " + contact.thisCollider.name);
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

        unscaledSize = new Vector3(0,0,0);
        unscaledSize[0] = leftEdgeUnscaledSize[0] + topEdgeUnscaledSize[0] + rightEdgeUnscaledSize[0];
        unscaledSize[1] = leftEdgeUnscaledSize[1];
        unscaledSize[2] = topEdgeUnscaledSize[2] + leftEdgeUnscaledSize[2] + bottomEdgeUnscaledSize[2];

        scaledSize = new(
            leftEdgeUnscaledSize.x * left_edge.transform.localScale.x + topEdgeUnscaledSize.x * top_edge.transform.localScale.x + rightEdgeUnscaledSize.x * right_edge.transform.localScale.x,
            topEdgeUnscaledSize.y * left_edge.transform.localScale.z + leftEdgeUnscaledSize.y * left_edge.transform.localScale.z + bottomEdgeUnscaledSize.y * left_edge.transform.localScale.z,
            topEdgeUnscaledSize.z * left_edge.transform.localScale.y
        );
        volume = scaledSize[0] * scaledSize[1] * scaledSize[2];
        if (originalVolume == 0f) { originalVolume = volume; }
        area = new(scaledSize[1] * scaledSize[2], scaledSize[0] * scaledSize[2], scaledSize[0] * scaledSize[1]);
    }

    void FixComponentPositions()
    {
        // FOR SOME REASON POS Z = ROT Y
        float halfWidthOfTopEdge = topEdgeUnscaledSize[0] / 2;
        float halfHeightOfLeftEdge = leftEdgeUnscaledSize.y / 2; // for some reason box colliders swap z and y
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

        // TODO: VERY CONFUSING THAT THE SWORD AND COVER HAVE DIFFERENT AXISES
        hole_cover.transform.localScale = new Vector3(
            top_edge.transform.localScale.x, 
            left_edge.transform.localScale.z,
            1
        );
        hole_cover.transform.localPosition = new Vector3(
            top_edge.transform.localPosition.x, 
            0, 
            left_edge.transform.localPosition.z
        );

        swordBodyCollider.size = new Vector3(scaledSize.x, scaledSize.z, scaledSize.y);
        swordBodyCollider.center = new Vector3(0, scaledSize.z / 2, 0);

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
        Debug.Log(
            "\nabsNormal: " + absNormal + 
            "\nabsNormal.x: " + absNormal.x + 
            "\nabsNormal.y: " + absNormal.y + 
            "\nabsNormal.z: " + absNormal.z
        );

        RecalculateMeasurements();
        if (absNormal.x >= absNormal.y && absNormal.x >= absNormal.z) { 

            float change_in_hit_axis_length = volumeShiftedOnHit / area[0];
            float change_in_hit_axis_scale = (scaledSize[0] - change_in_hit_axis_length) / scaledSize[0];
            newScale.x *= (float)change_in_hit_axis_scale;

            GameObject c1Object;
            GameObject c2Object = top_edge;
            GameObject c3Object;
            List<GameObject> c1Objects;
            List<GameObject> c2Objects = middleXComponents;
            List<GameObject> c3Objects;
            if (localNormal.x > 0)
            {
                c1Object = left_edge;
                c3Object = right_edge;
                c1Objects = leftComponents;
                c3Objects = rightComponents;
            }
            else
            {
                c1Object = right_edge;
                c3Object = left_edge;
                c1Objects = rightComponents;
                c3Objects = leftComponents;
            }
            
            float c1_scaled_axis_length = c1Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * c1Object.transform.localScale.x;
            float change_in_c1_axis_length = c1_scaled_axis_length * change_in_hit_axis_scale - c1_scaled_axis_length;
            float change_in_c1_vol = change_in_c1_axis_length * area[0];

            float c2_scaled_axis_length = c2Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * c2Object.transform.localScale.x;
            float change_in_c2_axis_length = c2_scaled_axis_length * change_in_hit_axis_scale - c2_scaled_axis_length;
            float change_in_c2_vol = change_in_c2_axis_length * area[0];

            float c3_scaled_axis_length = c3Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * c3Object.transform.localScale.x;

            float biased_change_in_c1_vol = change_in_c1_vol * squashBias;
            float biased_change_in_c1_axis_length = biased_change_in_c1_vol / area[0];

            float remaining_volume = change_in_c1_vol - biased_change_in_c1_vol;

            float biased_change_in_c2_vol = change_in_c2_vol + remaining_volume / 2;
            float biased_change_in_c2_axis_length = biased_change_in_c2_vol / area[0];

            float biased_change_in_c3_vol = -volumeShiftedOnHit - biased_change_in_c1_vol - biased_change_in_c2_vol;
            float biased_change_in_c3_axis_length = biased_change_in_c3_vol / area[0];

            float biased_change_in_c1_axis_scale = (c1_scaled_axis_length + biased_change_in_c1_axis_length) / c1_scaled_axis_length;
            float biased_change_in_c2_axis_scale = (c2_scaled_axis_length + biased_change_in_c2_axis_length) / c2_scaled_axis_length;
            float biased_change_in_c3_axis_scale = (c3_scaled_axis_length + biased_change_in_c3_axis_length) / c3_scaled_axis_length;

            float preservedFactor = Mathf.Sqrt(volume / (
                scaledSize[0] * change_in_hit_axis_scale * 
                scaledSize[1] *
                scaledSize[2]
            ));
            newScale.y *= (float)preservedFactor;
            newScale.z *= (float)preservedFactor;

            if (
                IsNewScaleWithinBounds(
                    newScale, 
                    c1Objects[0].transform.localScale[0] * (float)biased_change_in_c1_axis_scale, 
                    c2Objects[0].transform.localScale[0] * (float)biased_change_in_c2_axis_scale, 
                    c3Objects[0].transform.localScale[0] * (float)biased_change_in_c3_axis_scale
                )
            )
            {
                foreach (GameObject component in c1Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * (float)biased_change_in_c1_axis_scale, 
                        newComponentScale[1] * newScale.y, 
                        newComponentScale[2] * newScale.z
                    );
                }
                foreach (GameObject component in c2Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * (float)biased_change_in_c2_axis_scale, 
                        newComponentScale[1] * newScale.y, 
                        newComponentScale[2] * newScale.z
                    );
                }
                foreach (GameObject component in c3Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * (float)biased_change_in_c3_axis_scale, 
                        newComponentScale[1] * newScale.y, 
                        newComponentScale[2] * newScale.z
                    );
                }

                RecalculateMeasurements();
                FixComponentPositions();
                Debug.Log(
                    "X HIT - Volume: " + volume + 
                    "\nvolume shifted: " + volumeShiftedOnHit +
                    "\narea: " + area +
                    "\nbiased_change_in_c1_axis_scale: " + biased_change_in_c1_axis_scale + 
                    "\n\tc1_scaled_axis_length: " + c1_scaled_axis_length + 
                    "\n\tbiased_change_in_c1_vol: " + biased_change_in_c1_vol + 

                    "\nbiased_change_in_c2_axis_scale: " + biased_change_in_c2_axis_scale + 
                    "\n\tc2_scaled_axis_length: " + c2_scaled_axis_length + 
                    "\n\tbiased_change_in_c2_vol: " + biased_change_in_c2_vol + 

                    "\nbiased_change_in_c3_axis_scale: " + biased_change_in_c3_axis_scale +
                    "\n\tc3_scaled_axis_length: " + c3_scaled_axis_length +
                    "\n\tbiased_change_in_c3_vol: " + biased_change_in_c3_vol
                );
                return;
            }
        }
        else if (absNormal.z >= absNormal.x && absNormal.z >= absNormal.y) { 

            float change_in_hit_axis_length = volumeShiftedOnHit / area[1];
            float change_in_hit_axis_scale = (scaledSize[2] - change_in_hit_axis_length) / scaledSize[2];
            newScale.y *= (float)change_in_hit_axis_scale;

            GameObject c1Object;
            GameObject c2Object = top_edge;
            GameObject c3Object;
            List<GameObject> c1Objects;
            List<GameObject> c2Objects = middleYComponents;
            List<GameObject> c3Objects;

            // TODO: I GOT NO IDEA WHY THIS HAS TO BE FLIPPED
            if (localNormal.z < 0)
            {
                c1Object = top_edge;
                c3Object = bottom_edge;
                c1Objects = topComponents;
                c3Objects = bottomComponents;
            }
            else
            {
                c1Object = bottom_edge;
                c3Object = top_edge;
                c1Objects = bottomComponents;
                c3Objects = topComponents;
            }
            float c1_scaled_axis_length = c1Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * c1Object.transform.localScale.y;
            float change_in_c1_axis_length = c1_scaled_axis_length * change_in_hit_axis_scale - c1_scaled_axis_length;
            float change_in_c1_vol = change_in_c1_axis_length * area[1];

            float c2_scaled_axis_length = c2Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * c2Object.transform.localScale.y;
            float change_in_c2_axis_length = c2_scaled_axis_length * change_in_hit_axis_scale - c2_scaled_axis_length;
            float change_in_c2_vol = change_in_c2_axis_length * area[1];

            float c3_scaled_axis_length = c3Object.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * c3Object.transform.localScale.y;

            float biased_change_in_c1_vol = change_in_c1_vol * squashBias;
            float biased_change_in_c1_axis_length = biased_change_in_c1_vol / area[1];

            float remaining_volume = change_in_c1_vol - biased_change_in_c1_vol;

            float biased_change_in_c2_vol = change_in_c2_vol + remaining_volume / 2;
            float biased_change_in_c2_axis_length = biased_change_in_c2_vol / area[1];

            float biased_change_in_c3_vol = -volumeShiftedOnHit - biased_change_in_c1_vol - biased_change_in_c2_vol;
            float biased_change_in_c3_axis_length = biased_change_in_c3_vol / area[1];

            float biased_change_in_c1_axis_scale = (c1_scaled_axis_length + biased_change_in_c1_axis_length) / c1_scaled_axis_length;
            float biased_change_in_c2_axis_scale = (c2_scaled_axis_length + biased_change_in_c2_axis_length) / c2_scaled_axis_length;
            float biased_change_in_c3_axis_scale = (c3_scaled_axis_length + biased_change_in_c3_axis_length) / c3_scaled_axis_length;

            float preservedFactor = Mathf.Sqrt(volume / (
                scaledSize[0] * change_in_hit_axis_scale * 
                scaledSize[1] *
                scaledSize[2]
            ));
            newScale.x *= (float)preservedFactor;
            newScale.z *= (float)preservedFactor;

            if (
                IsNewScaleWithinBounds(
                    newScale, 
                    c1Objects[0].transform.localScale[1] * (float)biased_change_in_c1_axis_scale, 
                    c2Objects[0].transform.localScale[1] * (float)biased_change_in_c2_axis_scale, 
                    c3Objects[0].transform.localScale[1] * (float)biased_change_in_c3_axis_scale
                )
            )
            {
                foreach (GameObject component in c1Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * newScale.x, 
                        newComponentScale[1] * (float)biased_change_in_c1_axis_scale, 
                        newComponentScale[2] * newScale.z
                    );
                }
                foreach (GameObject component in c2Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * newScale.x, 
                        newComponentScale[1] * (float)biased_change_in_c2_axis_scale, 
                        newComponentScale[2] * newScale.z
                    );
                }
                foreach (GameObject component in c3Objects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0] * newScale.x, 
                        newComponentScale[1] * (float)biased_change_in_c3_axis_scale, 
                        newComponentScale[2] * newScale.z
                    );
                }

                RecalculateMeasurements();
                FixComponentPositions();
                Debug.Log(
                    "Z HIT - Volume: " + volume + 
                    "\nvolume shifted: " + volumeShiftedOnHit +
                    "\narea: " + area +
                    "\nbiased_change_in_c1_axis_scale: " + biased_change_in_c1_axis_scale + 
                    "\n\tc1_scaled_axis_length: " + c1_scaled_axis_length + 
                    "\n\tbiased_change_in_c1_vol: " + biased_change_in_c1_vol + 

                    "\nbiased_change_in_c2_axis_scale: " + biased_change_in_c2_axis_scale + 
                    "\n\tc2_scaled_axis_length: " + c2_scaled_axis_length + 
                    "\n\tbiased_change_in_c2_vol: " + biased_change_in_c2_vol + 

                    "\nbiased_change_in_c3_axis_scale: " + biased_change_in_c3_axis_scale +
                    "\n\tc3_scaled_axis_length: " + c3_scaled_axis_length +
                    "\n\tbiased_change_in_c3_vol: " + biased_change_in_c3_vol
                );
                return;
            }
        }
        else
        {
            Debug.Log("Y Hit Detected");
        }
        
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(Vector3 newScale, float c1Scale, float c2Scale, float c3Scale)
    {
        return newScale.x >= minScale.x && newScale.x <= maxScale.x &&
            newScale.y >= minScale.y && newScale.y <= maxScale.y &&
            newScale.z >= minScale.z && newScale.z <= maxScale.z &&

            // TODO: MAKE A CUSTOM MIN MAX FOR COMPONENTS
            c1Scale >= minScale.x / 10 && c1Scale <= maxScale.x * 10 && 
            c2Scale >= minScale.x / 10 && c2Scale <= maxScale.x * 10 && 
            c3Scale >= minScale.x / 10 && c3Scale <= maxScale.x * 10;
    }
}
