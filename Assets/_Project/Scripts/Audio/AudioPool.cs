using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioPool : MonoBehaviour {
    public AudioSource prefab;
    private ObjectPool<AudioSource> pool;

    void Awake() {
        pool = new ObjectPool<AudioSource>(
            createFunc:      () => Instantiate(prefab),
            actionOnGet:     (src) => src.gameObject.SetActive(true),
            actionOnRelease: (src) => {
                src.Stop();
                src.resource = null;
                src.gameObject.SetActive(false);
            },
            actionOnDestroy: (src) => Destroy(src.gameObject),
            maxSize: 20
        );
    }

    public void PlayClipAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        var src = pool.Get();
        src.transform.position = position;
        src.volume = volume;
        src.PlayOneShot(clip);
        StartCoroutine(ReleaseWhenDone(src, clip.length));
    }

    IEnumerator ReleaseWhenDone(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(src);
    }

    // Method for Audio Random Containers
    public void PlayARCAt(AudioResource arc, Vector3 position, float volume = 1f)
    {
        var src = pool.Get();
        src.transform.position = position;
        src.volume = volume;
        src.resource = arc;
        // Debug.Log($"Source enabled: {src.enabled}, GameObject active: {src.gameObject.activeInHierarchy}");
        src.Play();
        // Debug.Log($"[AudioPool] Playing ARC '{arc.name}' at {position} with volume {volume}");
        StartCoroutine(ReleaseWhenStopped(src));
    }

    // Poll isPlaying since we don't know the ARC clip length
    IEnumerator ReleaseWhenStopped(AudioSource src)
    {
        // Wait one frame so isPlaying has time to become true
        yield return null;
        while (src.isPlaying)
        {
            yield return null;
        }
        pool.Release(src);
    }

}