using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class NewSwordTipScaleHandler : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform scaleTarget;
    [SerializeField] string targetName = "Tip Pivot";

    // [SerializeField] GameObject tipLeftEdge;
    // [SerializeField] GameObject tipRightEdge;
    // [SerializeField] GameObject tipTopEdge;
    // [SerializeField] GameObject tipBottomEdge;
    // [SerializeField] GameObject tipTopLeftCorner;
    // [SerializeField] GameObject tipTopRightCorner;
    // [SerializeField] GameObject tipBottomLeftCorner;
    // [SerializeField] GameObject tipBottomRightCorner;
    [SerializeField] GameObject tipTip;
    [SerializeField] BoxCollider tipCollider;
    [SerializeField] GameObject tipTopLeftCorner;
    [SerializeField] GameObject tipTopRightCorner;
    [SerializeField] GameObject tipBottomRightCorner;

    [SerializeField] List<GameObject> tipObjects;
    [SerializeField] List<GameObject> bodyObjects;

    readonly int tipLeftRightScaleAxisIndex = 0;
    readonly int tipBackForthScaleAxisIndex = 2;
    readonly int tipUpDownScaleAxisIndex = 1;
    
    readonly int tipLeftRightBoundsAxisIndex = 0;
    readonly int tipBackForthBoundsAxisIndex = 2;
    readonly int tipUpDownBoundsAxisIndex = 1;

    // readonly int bodyLeftRightScaleAxisIndex = 0;
    readonly int bodyBackForthScaleAxisIndex = 2;
    // readonly int bodyUpDownScaleAxisIndex = 1;

    // readonly int bodyLeftRightBoundsAxisIndex = 0;
    // readonly int bodyBackForthBoundsAxisIndex = 2;
    // readonly int bodyUpDownBoundsAxisIndex = 1;

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new(2f, 2f, 2f);

    // [SerializeField] float minTipScale = 0.001f;
    // [SerializeField] float minBodyScale = 0.001f;


    [Header("Dials")]
    // [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;
    [SerializeField] float percentSquashDown = 0.33f;

    [Header("Events")]
    public UnityEvent onLengthScaleChanged;
    Vector3 scaledTipSize;
    float tipScaledLength;
    float tipUnscaledLength;
    float bodyScaledLength;
    // Vector3 area;
    float tipArea;
    float volume;
    float maxVolumeShift;
    private float lastHitTime = 0f;

    void Start()
    {

        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        // Debug.Log("Unscaled size: " + unscaledTipSize);
        RecalculateMeasurements();
        FixComponentPositions();

        // StartCoroutine(TestHit());
    }

    IEnumerator TestHit()
    {
        yield return new WaitForSeconds(2);
        RecalculateMeasurements();
        float volumeShiftedOnHit = Mathf.Clamp01(1f / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemperature();
        HandleDirectionalScale(new Vector3(0,0,1), volumeShiftedOnHit);
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
        if (contact.thisCollider.name != targetName) { return; }
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
        scaledTipSize = new (
            tipTopLeftCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size        [tipLeftRightBoundsAxisIndex] * tipTopLeftCorner.transform.localScale [tipLeftRightScaleAxisIndex] +
                tipTip.GetComponent<MeshFilter>().sharedMesh.bounds.size              [tipLeftRightBoundsAxisIndex] * tipTip.transform.localScale           [tipLeftRightScaleAxisIndex] + 
                tipTopRightCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size   [tipLeftRightBoundsAxisIndex] * tipTopRightCorner.transform.localScale[tipLeftRightScaleAxisIndex],
                
            tipTopRightCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size       [tipUpDownBoundsAxisIndex] * tipTopRightCorner.transform.localScale   [tipUpDownScaleAxisIndex] +
                tipTip.GetComponent<MeshFilter>().sharedMesh.bounds.size              [tipUpDownBoundsAxisIndex] * tipTip.transform.localScale              [tipUpDownScaleAxisIndex] + 
                tipBottomRightCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size[tipUpDownBoundsAxisIndex] * tipBottomRightCorner.transform.localScale[tipUpDownScaleAxisIndex],

            tipTopRightCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size       [tipBackForthBoundsAxisIndex] * tipTopRightCorner.transform.localScale[tipBackForthScaleAxisIndex] + 
                tipTip.GetComponent<MeshFilter>().sharedMesh.bounds.size              [tipBackForthBoundsAxisIndex] * tipTip.transform.localScale           [tipBackForthScaleAxisIndex]
        );

        tipUnscaledLength = tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex] + tipTip.GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex];
        tipScaledLength = tipTopRightCorner.GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex]
                             * tipTopRightCorner.transform.localScale[tipBackForthScaleAxisIndex] + 
                          tipTip.GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex]
                             * tipTip.transform.localScale[tipBackForthScaleAxisIndex];
        bodyScaledLength = bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex]
                             * bodyObjects[0].transform.localScale[bodyBackForthScaleAxisIndex];
        tipArea = scaledTipSize[0] * scaledTipSize[1];
        // bodyArea = tipArea;

        volume = tipArea * bodyScaledLength;
        // bodyArea = bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyLeftRightBoundsAxisIndex] * bodyObjects[0].transform.localScale[bodyLeftRightScaleAxisIndex] * 
        //            bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * bodyObjects[0].transform.localScale[bodyUpDownScaleAxisIndex];

        // Debug.Log(
        //     "\nTotal Length: " + (tipScaledLength + bodyScaledLength) +
        //     "\ntipScaledLength: " + tipScaledLength +
        //     "\nbodyScaledLength: " + bodyScaledLength
        // );
        // Debug.Log("bodyScaledLength: " + bodyScaledLength);
    }

    void FixComponentPositions()
    {
        onLengthScaleChanged.Invoke();
        tipTip.transform.localPosition = new (
            tipTip.transform.localPosition.x,
            tipObjects[0].transform.localPosition.y + tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[tipBackForthBoundsAxisIndex] * tipObjects[0].transform.localScale.z,
            tipTip.transform.localPosition.z
        );
        tipCollider.size = new Vector3(scaledTipSize.x * 0.6f, scaledTipSize.z, scaledTipSize.y * 0.6f);
        tipCollider.center = new Vector3(0, scaledTipSize.z / 2, 0);
    }

    void HandleDirectionalScale(Vector3 worldNormal, float volumeShiftedOnHit)
    {
        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);
        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );

        RecalculateMeasurements();
        if (
            (absNormal.x >= absNormal.y && absNormal.x >= absNormal.z) || 
            (absNormal.z >= absNormal.x && absNormal.z >= absNormal.y)
        ) { 
            float change_in_tip_length = volumeShiftedOnHit / tipArea;
            float change_in_tip_length_scale = (tipScaledLength + change_in_tip_length) / tipScaledLength;

            float change_in_body_length = volumeShiftedOnHit / tipArea;
            float change_in_body_length_scale = (bodyScaledLength - change_in_body_length) / bodyScaledLength;

            Debug.Log(
                "XZ HIT" + 
                "\nvolume shifted: " + volumeShiftedOnHit +
                "\ntipArea: " + tipArea +
                "\nchange_in_tip_length: " + change_in_tip_length + 
                "\nchange_in_tip_length_scale: " + change_in_tip_length_scale + 
                "\nchange_in_body_length: " + change_in_body_length + 
                "\nchange_in_body_length_scale: " + change_in_body_length_scale
            );

            if (IsNewScaleWithinBounds(
                tipObjects[0].transform.localScale.z * change_in_tip_length_scale, 
                bodyObjects[0].transform.localScale.z * change_in_body_length_scale
            ))
            {
                foreach (GameObject component in tipObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[tipBackForthScaleAxisIndex] *= change_in_tip_length_scale;
                    component.transform.localScale = newComponentScale;
                }
                Vector3 tipComponentScale = tipTip.transform.localScale;
                tipComponentScale[tipBackForthScaleAxisIndex] *= change_in_tip_length_scale;
                tipTip.transform.localScale = tipComponentScale;

                foreach (GameObject component in bodyObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyBackForthScaleAxisIndex] *= change_in_body_length_scale;
                    component.transform.localScale = newComponentScale;
                }
            }
            RecalculateMeasurements();
            FixComponentPositions();
            return;
        }
        else
        {
            float change_in_tip_length = volumeShiftedOnHit / tipArea;
            float change_in_tip_length_scale = (tipScaledLength - change_in_tip_length) / tipScaledLength;

            float change_in_body_length = volumeShiftedOnHit / tipArea;
            float change_in_body_length_scale = (bodyScaledLength + change_in_body_length) / bodyScaledLength;

            float change_in_hit_axis_length = volumeShiftedOnHit / tipArea;
            float biased_change_in_hit_axis_length = change_in_hit_axis_length * percentSquashDown;
            float biased_change_in_hit_axis_scale = (bodyScaledLength + biased_change_in_hit_axis_length) / bodyScaledLength;

            float volume_to_spread_outward = (1 - percentSquashDown) * change_in_hit_axis_length * tipArea;
            float volume_with_incoming_length_change = tipArea * bodyScaledLength * biased_change_in_hit_axis_scale;

            float preservedFactor = Mathf.Sqrt( (volume_with_incoming_length_change + volume_to_spread_outward) / volume_with_incoming_length_change );

            Debug.Log(
                "Y HIT" + 
                "\nvolume shifted: " + volumeShiftedOnHit +
                "\ntipArea: " + tipArea +
                "\nchange_in_tip_length: " + change_in_tip_length + 
                "\nchange_in_tip_length_scale: " + change_in_tip_length_scale + 
                "\nchange_in_body_length: " + change_in_body_length + 
                "\nchange_in_body_length_scale: " + change_in_body_length_scale +
                "\nbiased_change_in_hit_axis_length: " + biased_change_in_hit_axis_length +
                "\nbiased_change_in_hit_axis_scale: " + biased_change_in_hit_axis_scale +
                "\npreservedFactor: " + preservedFactor
            );

            if (IsNewScaleWithinBounds(
                tipObjects[0].transform.localScale[tipBackForthScaleAxisIndex] * biased_change_in_hit_axis_scale, 
                bodyObjects[0].transform.localScale[bodyBackForthScaleAxisIndex] * biased_change_in_hit_axis_scale
            ))
            {
                foreach (GameObject component in tipObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[tipLeftRightScaleAxisIndex] *= preservedFactor;
                    newComponentScale[tipUpDownScaleAxisIndex] *= preservedFactor; 
                    newComponentScale[tipBackForthScaleAxisIndex] *= change_in_tip_length_scale;
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * preservedFactor, 
                    //     newComponentScale[1] * preservedFactor, 
                    //     newComponentScale[2] * biased_change_in_hit_axis_length
                    // );
                }
                Vector3 tipComponentScale = tipTip.transform.localScale;
                tipComponentScale[tipLeftRightScaleAxisIndex] *= preservedFactor;
                tipComponentScale[tipUpDownScaleAxisIndex] *= preservedFactor; 
                tipComponentScale[tipBackForthScaleAxisIndex] *= change_in_tip_length_scale;
                tipTip.transform.localScale = tipComponentScale;

                foreach (GameObject component in bodyObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[tipLeftRightScaleAxisIndex] *= preservedFactor;
                    newComponentScale[tipUpDownScaleAxisIndex] *= preservedFactor; 
                    newComponentScale[tipBackForthScaleAxisIndex] *= biased_change_in_hit_axis_scale;
                    component.transform.localScale = newComponentScale;

                    // component.transform.localScale = new(
                    //     newComponentScale[0] * preservedFactor, 
                    //     newComponentScale[1] * preservedFactor, 
                    //     newComponentScale[2] * biased_change_in_hit_axis_length
                    // );
                }
            }
            else
            {
                Debug.Log("New scale not within bounds");
            }
            RecalculateMeasurements();
            FixComponentPositions();
            return;
        }
        
        // Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(float tipScale, float bodyScale)
    {
        return tipScale >= minScale.x && tipScale <= maxScale.x && 
               bodyScale >= minScale.x;
    }
}
