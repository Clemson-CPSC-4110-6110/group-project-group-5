using UnityEngine;

public class SwordTipScaleMatcher : MonoBehaviour
{
    [SerializeField] GameObject tipLeftEdge;
    [SerializeField] GameObject tipRightEdge;
    [SerializeField] GameObject tipTopEdge;
    [SerializeField] GameObject tipBottomEdge;
    [SerializeField] GameObject tipTopLeftCorner;
    [SerializeField] GameObject tipTopRightCorner;
    [SerializeField] GameObject tipBottomLeftCorner;
    [SerializeField] GameObject tipBottomRightCorner;
    [SerializeField] GameObject tip;

    [SerializeField] Transform bodyLeftEdge;
    [SerializeField] Transform bodyRightEdge;
    [SerializeField] Transform bodyTopEdge;
    [SerializeField] Transform bodyBottomEdge;
    [SerializeField] Transform bodyTopLeftCorner;
    [SerializeField] Transform bodyTopRightCorner;
    [SerializeField] Transform bodyBottomLeftCorner;
    [SerializeField] Transform bodyBottomRightCorner;
    [SerializeField] Transform bodyBaseCap;

    public void UpdateTipScale()
    {
        Vector3 newScale = tipLeftEdge.transform.localScale;
        newScale = new(bodyLeftEdge.localScale.x, bodyLeftEdge.localScale.y, newScale.z);
        tipLeftEdge.transform.localScale = newScale;
        Vector3 newPosition = bodyLeftEdge.localPosition;
        newPosition = new(newPosition.x, tipLeftEdge.transform.localPosition.y, newPosition.z);
        tipLeftEdge.transform.localPosition = newPosition;

        newScale = tipRightEdge.transform.localScale;
        newScale = new(bodyRightEdge.localScale.x, bodyRightEdge.localScale.y, newScale.z);
        tipRightEdge.transform.localScale = newScale;
        newPosition = bodyRightEdge.localPosition;
        newPosition = new(newPosition.x, tipRightEdge.transform.localPosition.y, newPosition.z);
        tipRightEdge.transform.localPosition = newPosition;

        newScale = tipTopEdge.transform.localScale;
        newScale = new(bodyTopEdge.localScale.x, bodyTopEdge.localScale.y, newScale.z);
        tipTopEdge.transform.localScale = newScale;
        newPosition = bodyTopEdge.localPosition;
        newPosition = new(newPosition.x, tipTopEdge.transform.localPosition.y, newPosition.z);
        tipTopEdge.transform.localPosition = newPosition;

        newScale = tipBottomEdge.transform.localScale;
        newScale = new(bodyBottomEdge.localScale.x, bodyBottomEdge.localScale.y, newScale.z);
        tipBottomEdge.transform.localScale = newScale;
        newPosition = bodyBottomEdge.localPosition;
        newPosition = new(newPosition.x, tipBottomEdge.transform.localPosition.y, newPosition.z);
        tipBottomEdge.transform.localPosition = newPosition;

        newScale = tipTopLeftCorner.transform.localScale;
        newScale = new(bodyTopLeftCorner.localScale.x, bodyTopLeftCorner.localScale.y, newScale.z);
        tipTopLeftCorner.transform.localScale = newScale;
        newPosition = bodyTopLeftCorner.localPosition;
        newPosition = new(newPosition.x, tipTopLeftCorner.transform.localPosition.y, newPosition.z);
        tipTopLeftCorner.transform.localPosition = newPosition;

        newScale = tipTopRightCorner.transform.localScale;
        newScale = new(bodyTopRightCorner.localScale.x, bodyTopRightCorner.localScale.y, newScale.z);
        tipTopRightCorner.transform.localScale = newScale;
        newPosition = bodyTopRightCorner.localPosition;
        newPosition = new(newPosition.x, tipTopRightCorner.transform.localPosition.y, newPosition.z);
        tipTopRightCorner.transform.localPosition = newPosition;

        newScale = tipBottomLeftCorner.transform.localScale;
        newScale = new(bodyBottomLeftCorner.localScale.x, bodyBottomLeftCorner.localScale.y, newScale.z);
        tipBottomLeftCorner.transform.localScale = newScale;
        newPosition = bodyBottomLeftCorner.localPosition;
        newPosition = new(newPosition.x, tipBottomLeftCorner.transform.localPosition.y, newPosition.z);
        tipBottomLeftCorner.transform.localPosition = newPosition;

        newScale = tipBottomRightCorner.transform.localScale;
        newScale = new(bodyBottomRightCorner.localScale.x, bodyBottomRightCorner.localScale.y, newScale.z);
        tipBottomRightCorner.transform.localScale = newScale;
        newPosition = bodyBottomRightCorner.localPosition;
        newPosition = new(newPosition.x, tipBottomRightCorner.transform.localPosition.y, newPosition.z);
        tipBottomRightCorner.transform.localPosition = newPosition;

        newScale = tip.transform.localScale;
        newScale = new(bodyTopEdge.localScale.x, bodyLeftEdge.localScale.y, newScale.z);
        tip.transform.localScale = newScale;
        newPosition = bodyBaseCap.localPosition;
        newPosition = new(newPosition.x, tip.transform.localPosition.y, newPosition.z);
        tip.transform.localPosition = newPosition;
    }
}
