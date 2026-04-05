using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] GameObject swordBladeScaleTarget;
    [SerializeField] SwordTipScaleHandler swordTipScaleHandler;

    public void SetBladeScale(Vector3 newScale)
    {
        Vector3 oldScale = swordBladeScaleTarget.transform.localScale;
        swordBladeScaleTarget.transform.localScale = newScale;
        swordTipScaleHandler.MoveTipWhenSwordScales(oldScale, newScale);
        swordTipScaleHandler.ScaleXZToMatchSwordScale(oldScale, newScale);
    }
}
