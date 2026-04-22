using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    [SerializeField] private AudioSource SoundFXObject;

    private void Awake()
    {
        // Check if an instance of GameManager already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate instance
            return;
        }
        Instance = this;  // Set the instance to this object
        // DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // spawn in gameObject
        AudioSource audioSource = Instantiate(SoundFXObject, spawnTransform.position, Quaternion.identity);
        DontDestroyOnLoad(audioSource);
        // assign the audioClip
        audioSource.clip = audioClip;
        // assign volume
        audioSource.volume = volume;
        // play sound
        audioSource.Play();
        // get length of SFX clip
        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);

    }
}
