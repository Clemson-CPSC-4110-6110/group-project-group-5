using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class HapticOnRelease : MonoBehaviour
{
    [SerializeField] float intensity = 0.5f;
    [SerializeField] float duration = 0.1f;

    XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        PlayHaptic(args);
    }

    void PlayHaptic(SelectExitEventArgs args)
    {
        var interactor = args.interactorObject;

        var hapticPlayer = interactor.transform
            .GetComponentInParent<HapticImpulsePlayer>();

        if (hapticPlayer == null) return;

        hapticPlayer.SendHapticImpulse(intensity, duration);
    }
}