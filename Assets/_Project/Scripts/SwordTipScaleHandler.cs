using UnityEngine;

public class SwordTipScaleHandler : MonoBehaviour
{
    // [SerializeField] float objectBeingMovedHeight;
    [SerializeField] float swordBladeBaseHeight;
    [SerializeField] Transform swordTipTransform;
    public void MoveTipWhenSwordScales(Vector3 oldScale, Vector3 newScale)
    {
        float heightDifference = swordBladeBaseHeight * (newScale.y - oldScale.y);
        swordTipTransform.localPosition += heightDifference * Vector3.up;
    }
    public void ScaleXZToMatchSwordScale(Vector3 _oldScale, Vector3 newScale)
    {
        Vector3 newSwordTipScale = new(newScale.x, swordTipTransform.localScale.y, newScale.z);
        swordTipTransform.localScale = newSwordTipScale;
    }
}
