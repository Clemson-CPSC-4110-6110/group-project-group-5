using UnityEngine;
using UnityEngine.Events;

public class SharpenOnCollision : MonoBehaviour
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
    // [SerializeField] GameObject hole_cover;
    [SerializeField] NewScaleAwayOnHit bodyScaleScript;
    [SerializeField] MeshRenderer tipRenderer;
    // public UnityEvent onSharpen;

    readonly int colliderLeftRightAxisIndex = 0;
    readonly int colliderBackForthAxisIndex = 1;
    readonly int colliderUpDownAxisIndex = 2;

    readonly int bodyLeftRightPosAxisIndex = 0;
    readonly int bodyBackForthPosAxisIndex = 1;
    readonly int bodyUpDownPosAxisIndex = 2;

    readonly int bodyLeftRightScaleAxisIndex = 0;
    readonly int bodyBackForthScaleAxisIndex = 2;
    readonly int bodyUpDownScaleAxisIndex = 1;

    readonly int bodyLeftRightBoundsAxisIndex = 0;
    readonly int bodyBackForthBoundsAxisIndex = 2;
    readonly int bodyUpDownBoundsAxisIndex = 1;

    bool isUpDownSideSharpened = false;

    void Update()
    {
        SharpenLeftRightSide(0.001f);
    }

    void SharpenLeftRightSide(float length_lost)
    {
        if (isUpDownSideSharpened) return;
        // float change_in_scale = 0.9f;
        float old_edge_length = left_edge.GetComponent<MeshFilter>().sharedMesh.bounds.size[bodyUpDownBoundsAxisIndex] * left_edge.transform.localScale[bodyUpDownScaleAxisIndex];
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

        Vector3 newEdgeScale = left_edge.transform.localScale;
        newEdgeScale[bodyUpDownScaleAxisIndex] *= change_in_scale;
        left_edge.transform.localScale = newEdgeScale;
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

    
}
