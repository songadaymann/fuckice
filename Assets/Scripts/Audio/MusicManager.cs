using UnityEngine;

/// <summary>
/// Simple music manager that plays background music on loop.
/// Persists across scene reloads.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip musicTrack;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    
    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 1f;
    
    private AudioSource audioSource;
    private static MusicManager instance;
    
    private void Awake()
    {
        // Singleton pattern - only one music manager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = musicTrack;
        audioSource.loop = loop;
        audioSource.volume = 0f; // Start silent for fade in
        audioSource.playOnAwake = false;
    }
    
    private void Start()
    {
        if (playOnStart && musicTrack != null)
        {
            PlayMusic();
        }
    }
    
    public void PlayMusic()
    {
        if (audioSource.isPlaying) return;
        
        audioSource.Play();
        StartCoroutine(FadeIn());
    }
    
    public void StopMusic()
    {
        StartCoroutine(FadeOut());
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = volume;
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = volume;
    }
    
    // Static access
    public static void Play() => instance?.PlayMusic();
    public static void Stop() => instance?.StopMusic();
    public static void Volume(float vol) => instance?.SetVolume(vol);
}

