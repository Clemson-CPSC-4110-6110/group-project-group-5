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

        ScaleAwayOnHit[] scaleAwayOnHits = GetComponents<ScaleAwayOnHit>();
        foreach (ScaleAwayOnHit script in scaleAwayOnHits)
        {
            script.maxScale = new(script.maxScale[0] * newScale[0], script.maxScale[1] * newScale[1], script.maxScale[2] * newScale[2]);
        }
    }
}
