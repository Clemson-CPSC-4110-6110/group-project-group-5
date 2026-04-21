using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class HandleScaleHandler : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform scaleTarget;
    [SerializeField] string targetName = "Handle Pivot";

    // [SerializeField] GameObject handleLeftEdge;
    // [SerializeField] GameObject handleRightEdge;
    // [SerializeField] GameObject handleTopEdge;
    // [SerializeField] GameObject handleBottomEdge;
    // [SerializeField] GameObject handleTopLeftCorner;
    // [SerializeField] GameObject handleTopRightCorner;
    // [SerializeField] GameObject handleBottomLeftCorner;
    // [SerializeField] GameObject handleBottomRightCorner;
    // [SerializeField] GameObject handleHandle;
    [SerializeField] BoxCollider handleCollider;
    // [SerializeField] GameObject handleTopLeftCorner;
    // [SerializeField] GameObject handleTopRightCorner;
    // [SerializeField] GameObject handleBottomRightCorner;

    [SerializeField] GameObject handleObject;

    // [SerializeField] List<GameObject> handleObjects;
    [SerializeField] List<GameObject> bodyObjects;

    readonly int handleLeftRightScaleAxisIndex = 0;
    readonly int handleBackForthScaleAxisIndex = 1;
    readonly int handleUpDownScaleAxisIndex = 2;
    
    readonly int handleLeftRightBoundsAxisIndex = 0;
    readonly int handleBackForthBoundsAxisIndex = 1;
    readonly int handleUpDownBoundsAxisIndex = 2;

    readonly int bodyLeftRightScaleAxisIndex = 0;
    readonly int bodyBackForthScaleAxisIndex = 2;
    readonly int bodyUpDownScaleAxisIndex = 1;

    readonly int bodyLeftRightBoundsAxisIndex = 0;
    readonly int bodyBackForthBoundsAxisIndex = 2;
    readonly int bodyUpDownBoundsAxisIndex = 1;

    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new(2f, 2f, 2f);

    [SerializeField] float minHandleScale = 0.001f;
    [SerializeField] float minBodyScale = 0.001f;


    [Header("Dials")]
    [SerializeField] float yGrowthScale = 0.7f; 
    [SerializeField] float volumeShiftModifier = 1f;
    [SerializeField] float minVelocity = 0.01f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] AnvilAttachable anvilAttachable;
    [SerializeField] float hitCooldown = 0.5f; // cooldown in seconds
    [SerializeField] TemperatureScript temperatureScript;
    [SerializeField] float percentSquashDown = 0.33f;

    [Header("Events")]
    public UnityEvent onLengthScaleChanged;
    Vector3 scaledHandleSize;
    float handleScaledLength;
    float handleUnscaledLength;
    float bodyScaledLength;
    // Vector3 area;
    float handleArea;
    float volume;
    float maxVolumeShift;
    private float lastHitTime = 0f;

    void Start()
    {

        maxVolumeShift = 0.002f * volumeShiftModifier; // Upper limit
        // Debug.Log("Unscaled size: " + unscaledHandleSize);
        RecalculateMeasurements();
        FixComponentPositions();

        StartCoroutine(TestHit());
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
        handleUnscaledLength = handleObject.GetComponent<MeshFilter>().sharedMesh.bounds.size[handleBackForthBoundsAxisIndex];
        handleScaledLength = handleUnscaledLength * handleObject.transform.localScale[handleBackForthScaleAxisIndex];
        scaledHandleSize = new ( 0.1f, 0.1f, handleScaledLength );

        bodyScaledLength = bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyBackForthBoundsAxisIndex]
                             * bodyObjects[0].transform.localScale[bodyBackForthScaleAxisIndex];
        handleArea = scaledHandleSize[0] * scaledHandleSize[1];
        volume = handleArea * handleScaledLength;
        Debug.Log("Bounds: " + handleObject.GetComponent<MeshFilter>().sharedMesh.bounds.size);
    }

    void FixComponentPositions()
    {
        onLengthScaleChanged.Invoke();
        // handleHandle.transform.localPosition = new (
        //     handleHandle.transform.localPosition.x,
        //     handleObjects[0].transform.localPosition.y + handleObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size[handleBackForthBoundsAxisIndex] * handleObjects[0].transform.localScale.z,
        //     handleHandle.transform.localPosition.z
        // );
        handleCollider.size = new Vector3(scaledHandleSize.x * 0.6f, scaledHandleSize.z, scaledHandleSize.y * 0.6f);
        handleCollider.center = new Vector3(0, scaledHandleSize.z / 2, 0);
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
            float change_in_handle_length = volumeShiftedOnHit / handleArea;
            float change_in_handle_length_scale = (handleScaledLength + change_in_handle_length) / handleScaledLength;

            float change_in_body_length = volumeShiftedOnHit / handleArea;
            float change_in_body_length_scale = (bodyScaledLength - change_in_body_length) / bodyScaledLength;

            Debug.Log(
                "XZ HIT" + 
                "\nvolume shifted: " + volumeShiftedOnHit +
                "\nhandleArea: " + handleArea +
                "\nchange_in_handle_length: " + change_in_handle_length + 
                "\nchange_in_handle_length_scale: " + change_in_handle_length_scale + 
                "\nchange_in_body_length: " + change_in_body_length + 
                "\nchange_in_body_length_scale: " + change_in_body_length_scale
            );

            if (IsNewScaleWithinBounds(
                handleObject.transform.localScale.z * change_in_handle_length_scale, 
                bodyObjects[0].transform.localScale.z * change_in_body_length_scale
            ))
            {
                Vector3 handleComponentScale = handleObject.transform.localScale;
                handleComponentScale[handleBackForthScaleAxisIndex] *= change_in_handle_length_scale;
                handleObject.transform.localScale = handleComponentScale;

                foreach (GameObject component in bodyObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[bodyBackForthScaleAxisIndex] *= change_in_body_length_scale;
                    component.transform.localScale = newComponentScale;
                }
            }
            else
            {
                Debug.Log("New scale not within bounds");
            }
            onLengthScaleChanged.Invoke();
            RecalculateMeasurements();
            FixComponentPositions();
            return;
        }
        else
        {
            float change_in_handle_length = volumeShiftedOnHit / handleArea;
            float change_in_handle_length_scale = (handleScaledLength - change_in_handle_length) / handleScaledLength;

            float change_in_body_length = volumeShiftedOnHit / handleArea;
            float change_in_body_length_scale = (bodyScaledLength + change_in_body_length) / bodyScaledLength;

            float change_in_hit_axis_length = volumeShiftedOnHit / handleArea;
            float biased_change_in_hit_axis_length = change_in_hit_axis_length * percentSquashDown;
            float biased_change_in_hit_axis_scale = (bodyScaledLength + biased_change_in_hit_axis_length) / bodyScaledLength;

            float volume_to_spread_outward = (1 - percentSquashDown) * change_in_hit_axis_length * handleArea;
            float volume_with_incoming_length_change = handleArea * bodyScaledLength * biased_change_in_hit_axis_scale;

            float preservedFactor = Mathf.Sqrt( (volume_with_incoming_length_change + volume_to_spread_outward) / volume_with_incoming_length_change );

            Debug.Log(
                "Y HIT" + 
                "\nvolume shifted: " + volumeShiftedOnHit +
                "\nhandleArea: " + handleArea +
                "\nchange_in_handle_length: " + change_in_handle_length + 
                "\nchange_in_handle_length_scale: " + change_in_handle_length_scale + 
                "\nchange_in_body_length: " + change_in_body_length + 
                "\nchange_in_body_length_scale: " + change_in_body_length_scale +
                "\nbiased_change_in_hit_axis_length: " + biased_change_in_hit_axis_length +
                "\nbiased_change_in_hit_axis_scale: " + biased_change_in_hit_axis_scale +
                "\npreservedFactor: " + preservedFactor
            );

            if (IsNewScaleWithinBounds(
                handleObject.transform.localScale[handleBackForthScaleAxisIndex] * biased_change_in_hit_axis_scale, 
                bodyObjects[0].transform.localScale[bodyBackForthScaleAxisIndex] * biased_change_in_hit_axis_scale
            ))
            {
                Vector3 handleComponentScale = handleObject.transform.localScale;
                // handleComponentScale[handleLeftRightScaleAxisIndex] *= preservedFactor;
                // handleComponentScale[handleUpDownScaleAxisIndex] *= preservedFactor; 
                handleComponentScale[handleBackForthScaleAxisIndex] *= change_in_handle_length_scale;
                handleObject.transform.localScale = handleComponentScale;

                foreach (GameObject component in bodyObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    newComponentScale[handleLeftRightScaleAxisIndex] *= preservedFactor;
                    newComponentScale[handleUpDownScaleAxisIndex] *= preservedFactor; 
                    newComponentScale[handleBackForthScaleAxisIndex] *= biased_change_in_hit_axis_scale;
                    component.transform.localScale = newComponentScale;
                }
            }
            else
            {
                Debug.Log("New scale not within bounds");
            }
            onLengthScaleChanged.Invoke();
            RecalculateMeasurements();
            FixComponentPositions();
            return;
        }
        
        // Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(float handleScale, float bodyScale)
    {
        return handleScale >= minScale.x && handleScale <= maxScale.x && 
               bodyScale >= minScale.x;
    }
}
