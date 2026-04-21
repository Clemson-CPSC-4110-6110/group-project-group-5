using UnityEngine;


public class HammerHit : MonoBehaviour
{
    private AudioSource audioSource;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check if the hammer is currently being held
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // 2. Only play if we hit hard enough (optional, prevents tiny micro-sounds)
            if (collision.relativeVelocity.magnitude > 0.5f)
            {
                // 3. Randomize pitch slightly so every hit sounds unique
                // grabInteractable.interactorsSelecting[0].GetComponent<XRBaseController>().SendHapticImpulse(0.5f, 0.1f);
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.Play();
            }
        }
    }
}