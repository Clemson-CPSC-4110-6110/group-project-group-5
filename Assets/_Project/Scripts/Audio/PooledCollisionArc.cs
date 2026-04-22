using UnityEngine;
using UnityEngine.Audio;

public class PooledCollisionArc : MonoBehaviour
{
    public static PooledCollisionArc Instance;
    [System.Serializable]
    public class MaterialSound
    {
        public string materialTag;
        public AudioResource arc;
    }

    public MaterialSound[] materials;
    public AudioResource defaultARC;
    public AudioPool audioPool;
    public float maxVelocity = 5f;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    AudioResource GetARCForTag(string tag)
    {
        foreach (var mat in materials)
        {
            if (mat.materialTag == tag)
                return mat.arc;
        }
        return defaultARC;
    }

    public void PlayCollision(Collision collision)
    {
        if (collision.gameObject.tag == "anvilSocketable" || collision.gameObject.tag == "hammer") return;

        AudioResource arc = GetARCForTag(collision.gameObject.tag);
        if (arc == null) return;

        float volume = Mathf.Clamp01(
            collision.relativeVelocity.magnitude / maxVelocity
        );

        audioPool.PlayARCAt(
            arc,
            collision.GetContact(0).point,
            volume
        );
    }
}
