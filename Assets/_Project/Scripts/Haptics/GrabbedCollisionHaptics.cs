// More flexible option. Dynamically determines which hand is holding object
// Only triggers when actually held 

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbedCollisionHaptics : MonoBehaviour
{
    public float baseIntensity = 0.5f;
    public float duration = 0.1f;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Only vibrate if someone is holding this object
        if (!grabInteractable.isSelected) return;

        // Get the interactor that's holding us
        var interactor = grabInteractable.interactorsSelecting[0];

        // Find the Haptic Impulse Player on the interactor's GameObject
        var hapticPlayer = interactor.transform
            .GetComponentInParent<HapticImpulsePlayer>();

        if (hapticPlayer == null) return;

        float scaledIntensity = Mathf.Clamp01(
            collision.relativeVelocity.magnitude / 5f * baseIntensity
        );
        hapticPlayer.SendHapticImpulse(scaledIntensity, duration);
    }
}
