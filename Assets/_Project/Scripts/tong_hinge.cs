using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class TongsController : MonoBehaviour
{
    // public Transform upperArm;
    public Transform pivotTransform;
    // public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor firstHand;
    // public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor secondHand;
    public XRGrabInteractable grabInteractable;
    public float maxAngle = 19f;
    public float minAngle = 0f;
    public float minDistance = 0f;
    public float maxDistance = 0.2f;
private Quaternion startRotation = Quaternion.identity;

    void Update()
    {
        if (grabInteractable.interactorsSelecting.Count == 2)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor first = grabInteractable.interactorsSelecting[0];
            UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor second = grabInteractable.interactorsSelecting[1];

            float dist = Vector3.Distance(first.transform.position,
                                          second.transform.position);

            // Convert distance to 0-1 range
            float t = 1 - Mathf.InverseLerp(minDistance, maxDistance, dist);

            // Map to minAngle → maxAngle
            float angle = Mathf.Lerp(minAngle, maxAngle, t);

            pivotTransform.localRotation =
                startRotation * Quaternion.Euler(0f, angle, 0f);
        }
    }
}