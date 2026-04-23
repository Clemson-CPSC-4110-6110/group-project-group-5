using UnityEngine;
using UnityEngine.Audio;

public class HammerCollisionAudio : MonoBehaviour
{
    public AudioPool audioPool;
    public float maxVelocity = 2f;
    public float baseVolume = 1f;
    public AudioResource arc;
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("anvilSocketable")) return;
        float volume = baseVolume * Mathf.Clamp01(
            collision.relativeVelocity.magnitude / maxVelocity
        );

        audioPool.PlayARCAt(
            arc,
            collision.GetContact(0).point,
            volume
        );
    }
}
