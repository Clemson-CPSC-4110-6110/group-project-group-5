// Most straightforward approach. Directly calls haptic impulse player when a collision occurs. Scales based on strength of collision.
// Hard-codes which hand vibrates based on which haptic player you assign

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class CollisionHaptics : MonoBehaviour
{
    public HapticImpulsePlayer hapticPlayer;
    public float intensity = 0.5f;
    public float duration = 0.1f;

    void OnCollisionEnter(Collision collision)
    {
        // Scale intensity by collision force
        float scaledIntensity = Mathf.Clamp01(
            collision.relativeVelocity.magnitude / 5f * intensity
        );
        hapticPlayer.SendHapticImpulse(scaledIntensity, duration);
    }
}
