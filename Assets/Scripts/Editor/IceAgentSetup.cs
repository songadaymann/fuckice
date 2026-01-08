using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Editor script to set up the IceAgent enemy.
/// Run from menu: Tools > Setup IceAgent Enemy
/// </summary>
public class IceAgentSetup : EditorWindow
{
    // The sprites are 300px tall - use 150 PPU to make him 2 tiles tall
    private const float PIXELS_PER_UNIT = 150f;
    
    [MenuItem("Tools/Setup IceAgent Enemy")]
    public static void SetupIceAgent()
    {
        string basePath = "Assets/Characters/IceAgent";
        
        // Load and configure all sprites
        Sprite[] runSprites = LoadAndConfigureSprites(basePath + "/Run", "Run", 4);
        Sprite[] jumpSprites = LoadAndConfigureSprites(basePath + "/Jump", "Jump", 3);
        Sprite[] shootSprites = LoadAndConfigureSprites(basePath + "/Shoot", "Shoot", 2);
        Sprite[] smooshedSprites = LoadAndConfigureSprites(basePath + "/Smooshed", "Smooshed", 2);
        
        if (runSprites == null || runSprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Could not find IceAgent sprites!", "OK");
            return;
        }
        
        // Create IceAgent GameObject
        GameObject iceAgent = new GameObject("IceAgent");
        iceAgent.tag = "Enemy";
        iceAgent.layer = LayerMask.NameToLayer("Enemies");
        
        // Add SpriteRenderer
        SpriteRenderer sr = iceAgent.AddComponent<SpriteRenderer>();
        sr.sprite = runSprites[0];
        sr.sortingLayerName = "Player";
        sr.flipX = false; // Sprite faces right by default, flip when going left
        
        // Add Rigidbody2D
        Rigidbody2D rb = iceAgent.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add CapsuleCollider2D (better for humanoid shapes)
        CapsuleCollider2D col = iceAgent.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.8f, 1.8f);
        col.offset = new Vector2(0f, 0f);
        col.direction = CapsuleDirection2D.Vertical;
        
        // Create child objects for checks
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = iceAgent.transform;
        groundCheck.transform.localPosition = new Vector3(0.3f, -0.95f, 0f);
        
        GameObject wallCheck = new GameObject("WallCheck");
        wallCheck.transform.parent = iceAgent.transform;
        wallCheck.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = iceAgent.transform;
        firePoint.transform.localPosition = new Vector3(0.6f, 0.3f, 0f);
        
        // Add IceAgentController script
        IceAgentController controller = iceAgent.AddComponent<IceAgentController>();
        
        // Set up serialized fields
        SerializedObject so = new SerializedObject(controller);
        
        // Ground detection - include both Ground and Default layers
        so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        so.FindProperty("wallCheck").objectReferenceValue = wallCheck.transform;
        so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        so.FindProperty("groundLayer").intValue = LayerMask.GetMask("Ground", "Default");
        so.FindProperty("playerLayer").intValue = LayerMask.GetMask("Player");
        
        // Movement behavior - like goombas, can fall off edges!
        so.FindProperty("canFallOffEdges").boolValue = true;
        so.FindProperty("startMovingRight").boolValue = false; // Run LEFT toward player!
        
        // Animation sprites
        SetSpriteArray(so, "runSprites", runSprites);
        SetSpriteArray(so, "jumpSprites", jumpSprites);
        SetSpriteArray(so, "shootSprites", shootSprites);
        SetSpriteArray(so, "smooshedSprites", smooshedSprites);
        
        so.ApplyModifiedProperties();
        
        // Position in scene
        iceAgent.transform.position = new Vector3(5f, 2f, 0f);
        
        // Create prefab folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Save prefab
        string prefabPath = "Assets/Prefabs/IceAgent.prefab";
        PrefabUtility.SaveAsPrefabAsset(iceAgent, prefabPath);
        
        // Create projectile prefab
        CreateProjectilePrefab();
        
        // Link projectile to IceAgent
        GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
        if (projPrefab != null)
        {
            so = new SerializedObject(controller);
            so.FindProperty("projectilePrefab").objectReferenceValue = projPrefab;
            so.ApplyModifiedProperties();
            
            // Update the prefab
            PrefabUtility.SaveAsPrefabAsset(iceAgent, prefabPath);
        }
        
        // Add PlayerHealth to player if not present
        AddPlayerHealth();
        
        // Select the enemy
        Selection.activeGameObject = iceAgent;
        
        EditorUtility.DisplayDialog("IceAgent Setup Complete!",
            "ICE Agent enemy has been created!\n\n" +
            "✓ IceAgent in scene with AI controller\n" +
            "✓ Prefab saved to Assets/Prefabs/IceAgent.prefab\n" +
            "✓ Projectile prefab created\n" +
            "✓ PlayerHealth added to player\n\n" +
            "The agent will patrol, shoot at the player,\n" +
            "and can be stomped like a Goomba!\n\n" +
            "You can duplicate the prefab to add more enemies.",
            "Nice!");
    }
    
    static Sprite[] LoadAndConfigureSprites(string folderPath, string prefix, int count)
    {
        Sprite[] sprites = new Sprite[count];
        
        for (int i = 0; i < count; i++)
        {
            string path = $"{folderPath}/{prefix}{i + 1}.png";
            
            // Configure importer
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.spritePixelsPerUnit = PIXELS_PER_UNIT;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        
        return sprites.Where(s => s != null).ToArray();
    }
    
    static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }
    }
    
    static void CreateProjectilePrefab()
    {
        // Create projectile
        GameObject projectile = new GameObject("Projectile");
        projectile.layer = LayerMask.NameToLayer("Default"); // Use default to hit ground
        
        // SpriteRenderer - the Projectile script will create the visual procedurally
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 5;
        
        // Scale it up to be visible
        projectile.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add CircleCollider2D as trigger
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;
        
        // Add Projectile script - it will create visual and trail automatically
        projectile.AddComponent<Projectile>();
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(projectile, "Assets/Prefabs/Projectile.prefab");
        Debug.Log("Created Projectile prefab with procedural bullet visual and trail effect!");
        
        // Clean up scene object
        Object.DestroyImmediate(projectile);
    }
    
    static void AddPlayerHealth()
    {
        // Find player and add health component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
            
            // Update player prefab if it exists
            string prefabPath = "Assets/Prefabs/Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            }
        }
    }
    
    [MenuItem("Tools/Add Enemy Tag")]
    public static void AddEnemyTag()
    {
        // This adds the Enemy tag if it doesn't exist
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool hasEnemy = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Enemy")
            {
                hasEnemy = true;
                break;
            }
        }
        
        if (!hasEnemy)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Enemy";
            tagManager.ApplyModifiedProperties();
            Debug.Log("Added 'Enemy' tag");
        }
    }
}

