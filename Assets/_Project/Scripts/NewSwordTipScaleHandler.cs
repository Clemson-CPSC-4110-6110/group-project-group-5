using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] GameObject tip;
    [SerializeField] BoxCollider tipCollider;
    [SerializeField] List<GameObject> tipObjects;
    [SerializeField] List<GameObject> bodyObjects;


    [Header("Clamp Settings")]
    [SerializeField] Vector3 minScale = new(0.2f, 0.2f, 0.2f);
    public Vector3 maxScale = new(2f, 2f, 2f);

    [SerializeField] double minTipScale = 0.001f;
    [SerializeField] double minBodyScale = 0.001f;


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
    public UnityEvent onLengthScaleChanged;
    Vector3 scaledTipSize;
    float tipScaledLength;
    float tipUnscaledLength;
    float bodyScaledLength;
    // Vector3 area;
    float tipArea;
    float bodyArea;
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
        Vector3 unscaledTipSize = new (
            tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x,
            tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.y,
            tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z + tip.GetComponent<MeshFilter>().sharedMesh.bounds.size.z
        );
        scaledTipSize = new (
            unscaledTipSize.x * tipObjects[0].transform.localScale.x,
            unscaledTipSize.y * tipObjects[0].transform.localScale.z,
            unscaledTipSize.z * tipObjects[0].transform.localScale.y
        );
        tipUnscaledLength = tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z + tip.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;
        tipScaledLength = tipUnscaledLength * tipObjects[0].transform.localScale.z; 
        bodyScaledLength = bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * bodyObjects[0].transform.localScale.z;
        tipArea = scaledTipSize[0] * scaledTipSize[1];
        bodyArea = bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * bodyObjects[0].transform.localScale.x * 
                   bodyObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.y * bodyObjects[0].transform.localScale.z;

        Debug.Log("bodyArea: " + bodyArea);
    }

    void FixComponentPositions()
    {
        onLengthScaleChanged.Invoke();
        tip.transform.localPosition = new (
            tip.transform.localPosition.x,
            tipObjects[0].transform.localPosition.y + tipObjects[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * tipObjects[0].transform.localScale.z,
            tip.transform.localPosition.z
        );
        tipCollider.size = new Vector3(scaledTipSize.x, scaledTipSize.z, scaledTipSize.y);
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
            Debug.Log("volumeShiftedOnHit: " + volumeShiftedOnHit);
            double change_in_tip_length = volumeShiftedOnHit / tipArea;
            double change_in_tip_length_scale = (tipScaledLength + change_in_tip_length) / tipScaledLength;

            double change_in_body_length = volumeShiftedOnHit / bodyArea;
            double change_in_body_length_scale = (bodyScaledLength - change_in_body_length) / bodyScaledLength;

            Debug.Log(
                "XZ HIT" + 
                "\nvolume shifted: " + volumeShiftedOnHit +
                "\ntipArea: " + tipArea +
                "\nbodyArea: " + bodyArea +
                "\nchange_in_tip_length: " + change_in_tip_length + 
                "\nchange_in_tip_length_scale: " + change_in_tip_length_scale + 
                "\nchange_in_body_length: " + change_in_body_length + 
                "\nchange_in_body_length_scale: " + change_in_body_length_scale
            );

            if (IsNewScaleWithinBounds(
                tipObjects[0].transform.localScale.z * change_in_tip_length_scale, 
                bodyObjects[0].transform.localScale.z * change_in_tip_length_scale
            ))
            {
                foreach (GameObject component in tipObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0], 
                        newComponentScale[1], 
                        newComponentScale[2] * (float)change_in_tip_length_scale
                    );
                }
                Vector3 tipComponentScale = tip.transform.localScale;
                tip.transform.localScale = new(
                    tipComponentScale[0], 
                    tipComponentScale[1], 
                    tipComponentScale[2] * (float)change_in_tip_length_scale
                );
                foreach (GameObject component in bodyObjects)
                {
                    Vector3 newComponentScale = component.transform.localScale;
                    component.transform.localScale = new(
                        newComponentScale[0], 
                        newComponentScale[1], 
                        newComponentScale[2] * (float)change_in_body_length_scale
                    );
                }
            }
            RecalculateMeasurements();
            FixComponentPositions();
        }
        else
        {
            Debug.Log("Y Hit Detected");
        }
        
        Debug.Log("Scale change rejected: newScale out of bounds");
    }

    bool IsNewScaleWithinBounds(double tipScale, double bodyScale)
    {
        return tipScale >= minScale.x / 10 && tipScale <= maxScale.x * 10 && 
               bodyScale >= minScale.x / 10;
    }
}
