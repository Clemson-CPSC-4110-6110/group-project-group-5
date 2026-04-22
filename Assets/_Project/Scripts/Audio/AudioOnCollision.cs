using UnityEngine;

public class AudioOnCollision : MonoBehaviour
{
    // [SerializeField] AudioClip audioClip;
    // [SerializeField] float volume = 1f;
    // void OnCollisionEnter(Collision collision)
    // {
    //     if (!audioClip) return;
    //     SoundFXManager.Instance.PlaySoundFXClip(audioClip, collision.transform, volume);
    // }
    void OnCollisionEnter(Collision collision)
    {
        PooledCollisionArc.Instance.PlayCollision(collision);
    }
}
