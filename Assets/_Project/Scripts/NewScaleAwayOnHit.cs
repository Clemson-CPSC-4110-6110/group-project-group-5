// using UnityEngine;

// public class NewScaleAwayOnHit : MonoBehaviour
// {
//     [SerializeField] GameObject posXObjects;
//     [SerializeField] GameObject negXObjects;
//     [SerializeField] GameObject posYObjects;
//     [SerializeField] GameObject negYObjects;
//     [SerializeField] GameObject posZObjects;
//     [SerializeField] GameObject negZObjects;
    
//     void OnPosXHit(double shrinkFactor)
//     {
//         hitAxisSizeLost = volumeShiftedOnHit / area[0];
//         hitAxisShrinkFactor = (scaledSize[0] - hitAxisSizeLost) / scaledSize[0];
//         newScale.x *= hitAxisShrinkFactor;

//         preservedFactor = Mathf.Sqrt(volume / (scaledSize[0] * scaledSize[1] * hitAxisShrinkFactor * scaledSize[2]));
//         // newScale.y *= preservedFactor;
//         // newScale.z *= preservedFactor;

//         float yPreservedFactor = 1 + (preservedFactor - 1) * yGrowthScale;
//         float nonYpreservedFactor = preservedFactor * preservedFactor / yPreservedFactor;
//         newScale.y *= yPreservedFactor;
//         newScale.z *= nonYpreservedFactor;
//     }
//     void OnNegXHit()
//     {
        
//     }
// }
