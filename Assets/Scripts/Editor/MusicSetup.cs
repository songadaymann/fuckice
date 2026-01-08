using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to set up the music manager with the game's soundtrack.
/// </summary>
public class MusicSetup : Editor
{
    [MenuItem("Tools/Setup Music")]
    public static void SetupMusic()
    {
        // Check if music manager already exists
        MusicManager existing = FindObjectOfType<MusicManager>();
        if (existing != null)
        {
            Debug.Log("MusicManager already exists! Updating audio clip...");
            UpdateMusicClip(existing);
            Selection.activeGameObject = existing.gameObject;
            return;
        }
        
        // Create music manager object
        GameObject musicObj = new GameObject("MusicManager");
        
        // Add audio source
        AudioSource audioSource = musicObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        
        // Add music manager script
        MusicManager manager = musicObj.AddComponent<MusicManager>();
        
        // Find and assign the music track
        UpdateMusicClip(manager);
        
        Selection.activeGameObject = musicObj;
        
        EditorUtility.DisplayDialog("Music Setup Complete!",
            "MusicManager created!\n\n" +
            "✓ Will play 'fuckICE.mp3' on loop\n" +
            "✓ Persists across scene reloads\n" +
            "✓ Fades in smoothly\n\n" +
            "Adjust volume in the Inspector if needed.",
            "Rock on! 🎸");
    }
    
    private static void UpdateMusicClip(MusicManager manager)
    {
        // Find the music file
        string[] guids = AssetDatabase.FindAssets("fuckICE t:AudioClip");
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            if (clip != null)
            {
                SerializedObject so = new SerializedObject(manager);
                so.FindProperty("musicTrack").objectReferenceValue = clip;
                so.FindProperty("volume").floatValue = 0.5f;
                so.FindProperty("playOnStart").boolValue = true;
                so.FindProperty("loop").boolValue = true;
                so.FindProperty("fadeInDuration").floatValue = 1f;
                so.ApplyModifiedProperties();
                
                Debug.Log($"Found and assigned music: {path}");
            }
        }
        else
        {
            Debug.LogWarning("Could not find 'fuckICE' audio clip! Make sure it's in Assets/Sound/Song/");
        }
    }
}

