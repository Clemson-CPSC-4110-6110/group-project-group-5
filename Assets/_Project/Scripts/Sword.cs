using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] GameObject entireSwordScaler;
    [SerializeField] NewScaleAwayOnHit swordBodyScaleHandler;
    // [SerializeField] SwordTipScaleHandler swordTipScaleHandler;
    // [SerializeField] ScaleAwayOnHit scaleAwayOnHit;
    public SmithingMaterial smithingMaterial;
    public void SetBladeScale(Vector3 initialScale)
    {
        entireSwordScaler.transform.localScale = initialScale;
        swordBodyScaleHandler.onScaleChanged.Invoke();
    }

    // public void SetBladeVolume(float initialVolume)
    // {
    //     float handleVolume = 0.02f * 0.02f * 0.2f;
    //     float baseBodyVolume = 0.1f * 0.1f * 0.6f;
    //     float tipVolume = 0.1f * 0.01f * 0.078f;

    //     float baseVolume = handleVolume + baseBodyVolume + tipVolume;
    //     float volumeScale = (initialVolume - baseVolume) / baseVolume;

    //     // Vector3 oldScale = swordBladeScaleTarget.transform.localScale;
    //     entireSwordScaler.transform.localScale = new(volumeScale, volumeScale, volumeScale);
    //     swordBodyScaleHandler.onScaleChanged.Invoke();

    //     // swordTipScaleHandler.MoveTipWhenSwordScales(oldScale, newScale);
    //     // swordTipScaleHandler.ScaleXZToMatchSwordScale(oldScale, newScale);
    //     // scaleAwayOnHit.ScaleUpMaxScale(newScale);
    // }
}
