using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets up ice physics material on all ground tilemaps.
/// </summary>
public class IcePhysicsSetup : EditorWindow
{
    [MenuItem("Tools/Setup Ice Physics")]
    public static void SetupIcePhysics()
    {
        // Load or create ice material
        string materialPath = "Assets/Physics/IceMaterial.physicsMaterial2D";
        PhysicsMaterial2D iceMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        
        if (iceMaterial == null)
        {
            // Create folder if needed
            if (!AssetDatabase.IsValidFolder("Assets/Physics"))
            {
                AssetDatabase.CreateFolder("Assets", "Physics");
            }
            
            // Create the material
            iceMaterial = new PhysicsMaterial2D("IceMaterial");
            iceMaterial.friction = 0f;
            iceMaterial.bounciness = 0f;
            AssetDatabase.CreateAsset(iceMaterial, materialPath);
            AssetDatabase.SaveAssets();
        }
        
        // Apply to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCol = player.GetComponent<Collider2D>();
            if (playerCol != null)
            {
                playerCol.sharedMaterial = iceMaterial;
                EditorUtility.SetDirty(player);
                Debug.Log("Applied ice material to Player");
            }
            
            // Also set Rigidbody2D settings for extra slidiness
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.drag = 0f; // No air resistance
                rb.angularDrag = 0f;
                EditorUtility.SetDirty(rb);
            }
        }
        
        // Apply to all tilemaps
        var tilemaps = Object.FindObjectsOfType<UnityEngine.Tilemaps.Tilemap>();
        int count = 0;
        
        foreach (var tilemap in tilemaps)
        {
            var collider = tilemap.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            if (collider != null)
            {
                // For tilemap colliders, we need to use a CompositeCollider2D
                var composite = tilemap.GetComponent<CompositeCollider2D>();
                if (composite == null)
                {
                    // Add Rigidbody2D (required for composite)
                    var rb = tilemap.GetComponent<Rigidbody2D>();
                    if (rb == null)
                    {
                        rb = tilemap.gameObject.AddComponent<Rigidbody2D>();
                    }
                    rb.bodyType = RigidbodyType2D.Static;
                    
                    // Add CompositeCollider2D
                    composite = tilemap.gameObject.AddComponent<CompositeCollider2D>();
                    
                    // Set tilemap collider to use composite
                    collider.usedByComposite = true;
                }
                
                composite.sharedMaterial = iceMaterial;
                EditorUtility.SetDirty(tilemap.gameObject);
                count++;
            }
        }
        
        // Update player prefab
        string prefabPath = "Assets/Prefabs/Player.prefab";
        if (player != null && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        }
        
        EditorUtility.DisplayDialog("Ice Physics Applied!",
            $"Applied zero-friction ice material to:\n\n" +
            $"✓ Player collider\n" +
            $"✓ {count} tilemap(s)\n\n" +
            "Everything should be super slippery now!\n\n" +
            "Tip: Adjust 'Ice Acceleration' and 'Ice Deceleration'\n" +
            "on PlayerController to fine-tune the feel.",
            "Slippery!");
    }
}

