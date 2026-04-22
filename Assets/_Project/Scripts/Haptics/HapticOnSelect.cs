using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class HapticOnSelect : MonoBehaviour
{
    [SerializeField] float intensity = 0.5f;
    [SerializeField] float duration = 0.1f;
    XRGrabInteractable grabInteractable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(PlayHaptic);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(PlayHaptic);
    }

    void PlayHaptic(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject;

        var hapticPlayer = interactor.transform
            .GetComponentInParent<HapticImpulsePlayer>();

        if (hapticPlayer == null) return;

        hapticPlayer.SendHapticImpulse(intensity, duration);
    }
}
