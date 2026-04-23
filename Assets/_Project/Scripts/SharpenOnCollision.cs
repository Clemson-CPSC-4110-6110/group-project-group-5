using UnityEngine;
using UnityEngine.Events;

public class SharpenOnCollision : MonoBehaviour
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
    // [SerializeField] GameObject hole_cover;
    [SerializeField] NewScaleAwayOnHit bodyScaleScript;
    [SerializeField] MeshRenderer tipRenderer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume;

    // public UnityEvent onSharpen;

    // readonly int colliderLeftRightAxisIndex = 0;
    // readonly int colliderBackForthAxisIndex = 1;
    // readonly int colliderUpDownAxisIndex = 2;

    // readonly int bodyLeftRightPosAxisIndex = 0;
    // readonly int bodyBackForthPosAxisIndex = 1;
    // readonly int bodyUpDownPosAxisIndex = 2;

    readonly int bodyLeftRightScaleAxisIndex = 0;
    // readonly int bodyBackForthScaleAxisIndex = 2;
    readonly int bodyUpDownScaleAxisIndex = 1;

    readonly int bodyLeftRightBoundsAxisIndex = 0;
    // readonly int bodyBackForthBoundsAxisIndex = 2;
    readonly int bodyUpDownBoundsAxisIndex = 1;

    bool isUpDownSideSharpened = false;
    bool isLeftRightSideSharpened = false;

    // void Update()
    // {
    //     SharpenLeftRightSide(0.00001f);
    // }

    void Awake()
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("grinder")) { return; }
        // audioSource.Play(audioClip, volume);
        audioSource.Play();
    }

    void OnTriggerStay(Collider other)
    {
        // Debug.Log("Trigger detected");
        if (!other.gameObject.CompareTag("grinder")) { return; }
        // Debug.Log("Trigger matches grinder tag");

        Vector3 worldNormal = (other.transform.position - transform.position).normalized;
        // ContactPoint contact = collision.GetContact(0);
        // Vector3 worldNormal = contact.normal;

        Vector3 localNormal = scaleTarget.InverseTransformDirection(worldNormal);
        Vector3 absNormal = new(
            Mathf.Abs(localNormal.x),
            Mathf.Abs(localNormal.y),
            Mathf.Abs(localNormal.z)
        );
        if (absNormal.x >= absNormal.z) { 
            SharpenLeftRightSide(0.00001f);
        }
        else
        {
            SharpenUpDownSide(0.00001f);
        }

        // // VELOCITY
        // float velocityMagnitude;
        // Rigidbody hammerRb = collision.rigidbody;
        // if (hammerRb == null) return;
        // velocityMagnitude = hammerRb.linearVelocity.magnitude;
        // if (velocityMagnitude < minVelocity || velocityMagnitude > maxVelocity) return;

        // if (Time.time - lastHitTime < hitCooldown) return;
        // lastHitTime = Time.time;

        // // VOLUME SHIFT
        // float volumeShiftedOnHit = Mathf.Clamp01(velocityMagnitude / maxVelocity) * maxVolumeShift * temperatureScript.GetPercentMaxTemp();

        // HandleDirectionalScale(worldNormal, volumeShiftedOnHit);
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("grinder")) { return; }
        audioSource.Stop();
    }

    void SharpenLeftRightSide(float length_lost)
    {
        if (isLeftRightSideSharpened) return;
        // float change_in_scale = 0.9f;
        float old_edge_length = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * left_edge.transform.localScale[bodyUpDownScaleAxisIndex];
        float new_edge_length;
        if (old_edge_length - length_lost <= 0)
        {
            new_edge_length = 0;
            isLeftRightSideSharpened = true;
            tipRenderer.enabled = false;
        }
        else
        {
            new_edge_length = old_edge_length - length_lost;
        }
        float change_in_scale = new_edge_length / old_edge_length;

        Vector3 newEdgeScale = left_edge.transform.localScale;
        newEdgeScale[bodyUpDownScaleAxisIndex] *= change_in_scale;
        left_edge.transform.localScale = newEdgeScale;

        newEdgeScale = right_edge.transform.localScale;
        newEdgeScale[bodyUpDownScaleAxisIndex] *= change_in_scale;
        right_edge.transform.localScale = newEdgeScale;

        // float new_edge_length = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * left_edge.transform.localScale[bodyUpDownScaleAxisIndex];
        // float length_lost = old_edge_length - new_edge_length;
        // float length_lost = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * (1 - change_in_scale);

        float top_change_up_down_length = top_left_corner.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex]
                                          * top_left_corner.transform.localScale[bodyUpDownScaleAxisIndex];
        float top_change_scale_up_down = (top_change_up_down_length + length_lost / 2) / top_change_up_down_length;
        
        float bottom_change_up_down_length = bottom_left_corner.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex]
                                             * bottom_left_corner.transform.localScale[bodyUpDownScaleAxisIndex];
        float bottom_change_scale_up_down = (bottom_change_up_down_length + length_lost / 2) / bottom_change_up_down_length;

        Vector3 newTopLeftCornerScale = top_left_corner.transform.localScale;
        newTopLeftCornerScale[bodyUpDownScaleAxisIndex] *= top_change_scale_up_down;
        top_left_corner.transform.localScale = newTopLeftCornerScale;

        Vector3 newTopEdgeScale = top_edge.transform.localScale;
        newTopEdgeScale[bodyUpDownScaleAxisIndex] *= top_change_scale_up_down;
        top_edge.transform.localScale = newTopEdgeScale;

        Vector3 newTopRightCornerScale = top_right_corner.transform.localScale;
        newTopRightCornerScale[bodyUpDownScaleAxisIndex] *= top_change_scale_up_down;
        top_right_corner.transform.localScale = newTopRightCornerScale;

        Vector3 newBottomLeftCornerScale = bottom_left_corner.transform.localScale;
        newBottomLeftCornerScale[bodyUpDownScaleAxisIndex] *= bottom_change_scale_up_down;
        bottom_left_corner.transform.localScale = newBottomLeftCornerScale;

        Vector3 newBottomEdgeScale = bottom_edge.transform.localScale;
        newBottomEdgeScale[bodyUpDownScaleAxisIndex] *= bottom_change_scale_up_down;
        bottom_edge.transform.localScale = newBottomEdgeScale;

        Vector3 newBottomRightCornerScale = bottom_right_corner.transform.localScale;
        newBottomRightCornerScale[bodyUpDownScaleAxisIndex] *= bottom_change_scale_up_down;
        bottom_right_corner.transform.localScale = newBottomRightCornerScale;

        bodyScaleScript.RecalculateMeasurements();
        bodyScaleScript.FixComponentPositions();
    }

    void SharpenUpDownSide(float length_lost)
    {
        if (isUpDownSideSharpened) return;
        // float change_in_scale = 0.9f;
        float old_edge_length = top_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyLeftRightBoundsAxisIndex] * top_edge.transform.localScale[bodyLeftRightScaleAxisIndex];
        float new_edge_length;
        if (old_edge_length - length_lost <= 0)
        {
            new_edge_length = 0;
            isUpDownSideSharpened = true;
            tipRenderer.enabled = false;
        }
        else
        {
            new_edge_length = old_edge_length - length_lost;
        }
        float change_in_scale = new_edge_length / old_edge_length;

        Vector3 newEdgeScale = top_edge.transform.localScale;
        newEdgeScale[bodyLeftRightScaleAxisIndex] *= change_in_scale;
        top_edge.transform.localScale = newEdgeScale;

        newEdgeScale = bottom_edge.transform.localScale;
        newEdgeScale[bodyLeftRightScaleAxisIndex] *= change_in_scale;
        bottom_edge.transform.localScale = newEdgeScale;

        // float new_edge_length = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * left_edge.transform.localScale[bodyUpDownScaleAxisIndex];
        // float length_lost = old_edge_length - new_edge_length;
        // float length_lost = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * (1 - change_in_scale);

        float left_change_left_right_length = top_left_corner.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyLeftRightBoundsAxisIndex]
                                          * top_left_corner.transform.localScale[bodyLeftRightScaleAxisIndex];
        float left_change_scale_left_right = (left_change_left_right_length + length_lost / 2) / left_change_left_right_length;
        
        float right_change_left_right_length = top_right_corner.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyLeftRightBoundsAxisIndex]
                                             * top_right_corner.transform.localScale[bodyLeftRightScaleAxisIndex];
        float right_change_scale_left_right = (right_change_left_right_length + length_lost / 2) / right_change_left_right_length;

        Vector3 newTopLeftCornerScale = top_left_corner.transform.localScale;
        newTopLeftCornerScale[bodyLeftRightScaleAxisIndex] *= left_change_scale_left_right;
        top_left_corner.transform.localScale = newTopLeftCornerScale;

        Vector3 newTopEdgeScale = left_edge.transform.localScale;
        newTopEdgeScale[bodyLeftRightScaleAxisIndex] *= left_change_scale_left_right;
        left_edge.transform.localScale = newTopEdgeScale;

        Vector3 newTopRightCornerScale = bottom_left_corner.transform.localScale;
        newTopRightCornerScale[bodyLeftRightScaleAxisIndex] *= left_change_scale_left_right;
        bottom_left_corner.transform.localScale = newTopRightCornerScale;

        Vector3 newBottomLeftCornerScale = top_right_corner.transform.localScale;
        newBottomLeftCornerScale[bodyLeftRightScaleAxisIndex] *= right_change_scale_left_right;
        top_right_corner.transform.localScale = newBottomLeftCornerScale;

        Vector3 newBottomEdgeScale = right_edge.transform.localScale;
        newBottomEdgeScale[bodyLeftRightScaleAxisIndex] *= right_change_scale_left_right;
        right_edge.transform.localScale = newBottomEdgeScale;

        Vector3 newBottomRightCornerScale = bottom_right_corner.transform.localScale;
        newBottomRightCornerScale[bodyLeftRightScaleAxisIndex] *= right_change_scale_left_right;
        bottom_right_corner.transform.localScale = newBottomRightCornerScale;

        bodyScaleScript.RecalculateMeasurements();
        bodyScaleScript.FixComponentPositions();
    }

    
}
