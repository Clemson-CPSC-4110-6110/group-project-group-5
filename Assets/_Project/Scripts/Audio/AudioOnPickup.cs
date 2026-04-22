using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class AudioOnPickup : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume = 1f;
    XRGrabInteractable xRGrabInteractable;
    void Awake()
    {
        xRGrabInteractable = GetComponent<XRGrabInteractable>();
        xRGrabInteractable.selectEntered.AddListener(
            (args) => SoundFXManager.Instance.PlaySoundFXClip(audioClip, gameObject.transform, volume)
        );
    }
        void OnDisable()
    {
        xRGrabInteractable.selectEntered.RemoveListener(
            (args) => SoundFXManager.Instance.PlaySoundFXClip(audioClip, gameObject.transform, volume)
        );
    }
}
