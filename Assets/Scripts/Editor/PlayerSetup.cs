using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to automatically set up the Flurry player character.
/// Run from menu: Tools > Setup Player
/// </summary>
public class PlayerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Player (Flurry)")]
    public static void SetupPlayer()
    {
        // Find the Flurry sprites
        string flurry1Path = "Assets/Characters/Flurry/flurry1.png";
        string flurry2Path = "Assets/Characters/Flurry/flurry2.png";
        
        // Configure sprite import settings
        ConfigureSprite(flurry1Path);
        ConfigureSprite(flurry2Path);
        
        // Load the sprites
        Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(flurry1Path);
        Sprite sprite2 = AssetDatabase.LoadAssetAtPath<Sprite>(flurry2Path);
        
        if (sprite1 == null || sprite2 == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find Flurry sprites at:\n" + flurry1Path + "\n" + flurry2Path, 
                "OK");
            return;
        }
        
        // Create Player GameObject
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        
        // Add SpriteRenderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite1;
        sr.sortingLayerName = "Player";
        
        // Add Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // Add BoxCollider2D - sized for 16x16 sprite at 32 PPU
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.4f, 0.45f);
        col.offset = new Vector2(0f, 0f);
        
        // Create GroundCheck child object
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        
        // Add PlayerController script
        PlayerController controller = player.AddComponent<PlayerController>();
        
        // Use SerializedObject to set private serialized fields
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        serializedController.FindProperty("groundLayer").intValue = LayerMask.GetMask("Ground");
        serializedController.FindProperty("moveSpeed").floatValue = 6f;
        serializedController.FindProperty("jumpForce").floatValue = 12f;
        serializedController.ApplyModifiedProperties();
        
        // Add AnimatedSprite for walk animation
        AnimatedSprite anim = player.AddComponent<AnimatedSprite>();
        SerializedObject serializedAnim = new SerializedObject(anim);
        SerializedProperty framesProperty = serializedAnim.FindProperty("frames");
        framesProperty.arraySize = 2;
        framesProperty.GetArrayElementAtIndex(0).objectReferenceValue = sprite1;
        framesProperty.GetArrayElementAtIndex(1).objectReferenceValue = sprite2;
        serializedAnim.FindProperty("framesPerSecond").floatValue = 8f;
        serializedAnim.FindProperty("loop").boolValue = true;
        serializedAnim.ApplyModifiedProperties();
        
        // Position player in scene
        player.transform.position = new Vector3(0f, 2f, 0f);
        
        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Player.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        
        // Select the player in hierarchy
        Selection.activeGameObject = player;
        
        // Setup camera to follow player
        SetupCamera(player.transform);
        
        EditorUtility.DisplayDialog("Player Setup Complete!", 
            "Flurry player has been created!\n\n" +
            "✓ Player GameObject in scene\n" +
            "✓ Prefab saved to Assets/Prefabs/Player.prefab\n" +
            "✓ Camera set to follow player\n\n" +
            "Make sure your ground tiles are on the 'Ground' layer!\n\n" +
            "Press Play to test (Arrow keys + Space)", 
            "Let's Go!");
        
        Debug.Log("Player setup complete! Prefab saved to: " + prefabPath);
    }
    
    static void ConfigureSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
    
    static void SetupCamera(Transform target)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No Main Camera found!");
            return;
        }
        
        // Add CameraFollow if not present
        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollow>();
        }
        
        // Set target using SerializedObject
        SerializedObject serializedCam = new SerializedObject(follow);
        serializedCam.FindProperty("target").objectReferenceValue = target;
        serializedCam.FindProperty("smoothSpeed").floatValue = 5f;
        serializedCam.FindProperty("offset").vector3Value = new Vector3(0f, 1f, -10f);
        serializedCam.ApplyModifiedProperties();
        
        // Set camera size for pixel art
        cam.orthographicSize = 5f;
    }
    
    [MenuItem("Tools/Setup Ground Layer")]
    public static void SetupGroundLayer()
    {
        // Find Tilemap and set its layer
        var tilemaps = Object.FindObjectsOfType<UnityEngine.Tilemaps.Tilemap>();
        int groundLayer = LayerMask.NameToLayer("Ground");
        
        if (groundLayer == -1)
        {
            EditorUtility.DisplayDialog("Layer Not Found", 
                "The 'Ground' layer doesn't exist.\n\n" +
                "It should already be set up, but if not:\n" +
                "1. Go to Edit > Project Settings > Tags and Layers\n" +
                "2. Add 'Ground' to one of the User Layers", 
                "OK");
            return;
        }
        
        int count = 0;
        foreach (var tilemap in tilemaps)
        {
            tilemap.gameObject.layer = groundLayer;
            
            // Also add TilemapCollider2D if not present
            if (tilemap.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() == null)
            {
                tilemap.gameObject.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            }
            
            count++;
        }
        
        EditorUtility.DisplayDialog("Ground Setup Complete", 
            $"Set {count} tilemap(s) to Ground layer and added colliders!", 
            "OK");
    }
}

