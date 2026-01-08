using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to create checkpoint prefab and place checkpoints in the scene.
/// </summary>
public class CheckpointSetup : Editor
{
    [MenuItem("Tools/Checkpoints/Create Checkpoint Prefab")]
    public static void CreateCheckpointPrefab()
    {
        // Create checkpoint object
        GameObject checkpoint = new GameObject("Checkpoint");
        
        // Add a visible sprite (simple flag/pole shape)
        SpriteRenderer sr = checkpoint.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Objects";
        sr.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray when inactive
        
        // Create a simple flag sprite procedurally
        CreateFlagSprite(sr);
        
        // Add trigger collider
        BoxCollider2D col = checkpoint.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 2f); // Tall trigger area
        col.offset = new Vector2(0, 0.5f);
        
        // Add checkpoint script
        Checkpoint cp = checkpoint.AddComponent<Checkpoint>();
        
        // Configure via SerializedObject
        SerializedObject so = new SerializedObject(cp);
        so.FindProperty("activeColor").colorValue = new Color(0.2f, 1f, 0.2f, 1f); // Bright green
        so.FindProperty("inactiveColor").colorValue = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray
        so.FindProperty("showVisualFeedback").boolValue = true;
        so.ApplyModifiedProperties();
        
        // Create prefab folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Checkpoint.prefab";
        PrefabUtility.SaveAsPrefabAsset(checkpoint, prefabPath);
        
        // Position in scene
        checkpoint.transform.position = GetSpawnPosition();
        
        Selection.activeGameObject = checkpoint;
        
        EditorUtility.DisplayDialog("Checkpoint Created!",
            "Checkpoint prefab created!\n\n" +
            "✓ Prefab saved to Assets/Prefabs/Checkpoint.prefab\n" +
            "✓ One placed in scene\n\n" +
            "Drag the prefab into the scene to add more checkpoints.\n" +
            "Place them at safe spots where you want the player to respawn.",
            "Got it!");
    }
    
    [MenuItem("Tools/Checkpoints/Place Checkpoint Here")]
    public static void PlaceCheckpointAtSceneView()
    {
        // Load prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Checkpoint.prefab");
        
        if (prefab == null)
        {
            // Create it first
            CreateCheckpointPrefab();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Checkpoint.prefab");
        }
        
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = GetSpawnPosition();
            
            // Name it based on count
            Checkpoint[] existing = FindObjectsOfType<Checkpoint>();
            instance.name = $"Checkpoint_{existing.Length}";
            
            Selection.activeGameObject = instance;
            
            // Focus scene view on it
            SceneView.lastActiveSceneView?.Frame(new Bounds(instance.transform.position, Vector3.one * 3f), false);
        }
    }
    
    private static Vector3 GetSpawnPosition()
    {
        // Try to spawn at scene view camera position
        if (SceneView.lastActiveSceneView != null)
        {
            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            if (sceneCam != null)
            {
                Vector3 pos = sceneCam.transform.position;
                pos.z = 0; // Keep at z=0 for 2D
                return pos;
            }
        }
        
        // Fallback: spawn near player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position + Vector3.right * 3f;
        }
        
        return Vector3.zero;
    }
    
    private static void CreateFlagSprite(SpriteRenderer sr)
    {
        // Create a simple checkpoint flag sprite
        int width = 16;
        int height = 32;
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        
        Color transparent = new Color(0, 0, 0, 0);
        Color pole = new Color(0.4f, 0.25f, 0.1f); // Brown pole
        Color flag = new Color(1f, 0.8f, 0.2f); // Yellow flag
        Color flagDark = new Color(0.8f, 0.6f, 0.1f);
        
        // Clear to transparent
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, transparent);
        
        // Draw pole (vertical line in center)
        for (int y = 0; y < height - 4; y++)
        {
            tex.SetPixel(7, y, pole);
            tex.SetPixel(8, y, pole);
        }
        
        // Draw flag (triangle pointing right at top)
        for (int y = height - 12; y < height - 2; y++)
        {
            int flagWidth = (y - (height - 12)) / 2 + 1;
            for (int x = 9; x < 9 + flagWidth && x < width; x++)
            {
                tex.SetPixel(x, y, y % 2 == 0 ? flag : flagDark);
            }
        }
        
        // Base (small rectangle at bottom)
        for (int x = 5; x < 11; x++)
        {
            tex.SetPixel(x, 0, pole);
            tex.SetPixel(x, 1, pole);
        }
        
        tex.Apply();
        
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 16f);
    }
}

