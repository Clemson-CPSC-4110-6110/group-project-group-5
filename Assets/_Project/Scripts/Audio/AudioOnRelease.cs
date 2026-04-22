using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AudioOnRelease : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume = 1f;
    XRGrabInteractable xRGrabInteractable;
    void Awake()
    {
        xRGrabInteractable = GetComponent<XRGrabInteractable>();
        xRGrabInteractable.selectExited.AddListener(
            (args) => SoundFXManager.Instance.PlaySoundFXClip(audioClip, gameObject.transform, volume)
        );
    }
    void OnDisable()
    {
        xRGrabInteractable.selectExited.RemoveListener(
            (args) => SoundFXManager.Instance.PlaySoundFXClip(audioClip, gameObject.transform, volume)
        );
    }
}
