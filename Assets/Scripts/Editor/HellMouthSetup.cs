using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets up Hell Mouth spawners from the character sprites.
/// Run from menu: Tools > Setup Hell Mouths
/// </summary>
public class HellMouthSetup : EditorWindow
{
    [MenuItem("Tools/Setup Hell Mouths")]
    public static void SetupHellMouths()
    {
        string basePath = "Assets/Characters/hellMouths/";
        string[] hellMouthFiles = { "trump", "miller", "kristi" };
        string[] displayNames = { "Trump", "Miller", "Kristi Noem" };
        
        // Find or create parent container
        GameObject hellMouthParent = GameObject.Find("HellMouths");
        if (hellMouthParent == null)
        {
            hellMouthParent = new GameObject("HellMouths");
        }
        
        // Find the IceAgent prefab to assign
        GameObject iceAgentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/IceAgent.prefab");
        if (iceAgentPrefab == null)
        {
            Debug.LogWarning("IceAgent prefab not found at Assets/Prefabs/IceAgent.prefab - you'll need to assign it manually");
        }
        
        int created = 0;
        float xOffset = 0;
        
        for (int i = 0; i < hellMouthFiles.Length; i++)
        {
            string spritePath = basePath + hellMouthFiles[i] + ".png";
            Sprite sprite = LoadAndConfigureSprite(spritePath);
            
            if (sprite == null)
            {
                Debug.LogWarning($"Could not load sprite: {spritePath}");
                continue;
            }
            
            // Create hell mouth GameObject
            string objName = $"HellMouth_{displayNames[i]}";
            GameObject hellMouth = GameObject.Find(objName);
            if (hellMouth == null)
            {
                hellMouth = new GameObject(objName);
            }
            
            hellMouth.transform.parent = hellMouthParent.transform;
            hellMouth.transform.localPosition = new Vector3(xOffset, 0, 0);
            hellMouth.tag = "HellMouth";
            
            // Add SpriteRenderer
            SpriteRenderer sr = hellMouth.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = hellMouth.AddComponent<SpriteRenderer>();
            }
            sr.sprite = sprite;
            sr.sortingLayerName = "Objects";
            sr.sortingOrder = 5;
            
            // Scale down - these are likely large images
            // Aim for roughly 3-4 units tall
            float spriteHeight = sprite.bounds.size.y;
            float targetHeight = 4f;
            float scale = targetHeight / spriteHeight;
            hellMouth.transform.localScale = new Vector3(scale, scale, 1);
            
            // Add Collider (for potential interaction)
            PolygonCollider2D col = hellMouth.GetComponent<PolygonCollider2D>();
            if (col == null)
            {
                col = hellMouth.AddComponent<PolygonCollider2D>();
            }
            col.isTrigger = true; // Trigger for detection, not physics blocking
            
            // Add HellMouth script
            HellMouth hm = hellMouth.GetComponent<HellMouth>();
            if (hm == null)
            {
                hm = hellMouth.AddComponent<HellMouth>();
            }
            
            // Load explosion sprites
            Sprite[] explosionSprites = LoadExplosionSprites();
            
            // Spawn intervals: Kristi = slowest, Miller = medium, Trump = fastest
            // Array order matches: [0]=Trump, [1]=Miller, [2]=Kristi
            float[] spawnIntervals = { 2.0f, 3.0f, 4.0f }; // Trump=fast, Miller=medium, Kristi=slow
            
            // Configure via SerializedObject
            SerializedObject so = new SerializedObject(hm);
            so.FindProperty("hellMouthName").stringValue = displayNames[i];
            so.FindProperty("spawnInterval").floatValue = spawnIntervals[i];
            so.FindProperty("maxActiveAgents").intValue = 3;
            so.FindProperty("maxHealth").intValue = 3; // 3 eggs to close!
            
            Debug.Log($"Setting up {displayNames[i]} with spawn interval {spawnIntervals[i]}s and {explosionSprites.Length} explosion sprites");
            
            if (iceAgentPrefab != null)
            {
                so.FindProperty("iceAgentPrefab").objectReferenceValue = iceAgentPrefab;
            }
            
            // Set explosion sprites
            if (explosionSprites != null && explosionSprites.Length > 0)
            {
                SerializedProperty explosionProp = so.FindProperty("explosionSprites");
                explosionProp.arraySize = explosionSprites.Length;
                for (int j = 0; j < explosionSprites.Length; j++)
                {
                    explosionProp.GetArrayElementAtIndex(j).objectReferenceValue = explosionSprites[j];
                }
            }
            
            so.ApplyModifiedProperties();
            
            // Create spawn point (from the mouth area - lower center)
            Transform spawnPoint = hellMouth.transform.Find("SpawnPoint");
            if (spawnPoint == null)
            {
                GameObject sp = new GameObject("SpawnPoint");
                sp.transform.parent = hellMouth.transform;
                sp.transform.localPosition = new Vector3(0, -0.3f, 0); // From mouth
                spawnPoint = sp.transform;
            }
            
            // Assign spawn point
            so = new SerializedObject(hm);
            so.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(hellMouth);
            
            xOffset += 8f; // Space them out for preview
            created++;
        }
        
        // Ensure "HellMouth" tag exists
        CreateTagIfNeeded("HellMouth");
        
        // Select parent
        Selection.activeGameObject = hellMouthParent;
        
        EditorUtility.DisplayDialog("Hell Mouths Setup Complete!",
            $"Created {created} Hell Mouth(s)!\n\n" +
            "✓ Trump, Miller, and Kristi Noem hell mouths created\n" +
            "✓ HellMouth spawner scripts attached\n" +
            "✓ Explosion animations configured\n" +
            "✓ 3 egg hits to close each one!\n\n" +
            "Next steps:\n" +
            "1. Position them in your level\n" +
            "2. Run 'Setup Birdo System' to create Birdo & Key\n" +
            "3. Birdo will shoot eggs at hell mouths automatically!",
            "Let's Go!");
    }
    
    static Sprite LoadAndConfigureSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100; // Keep high for detail
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    static Sprite[] LoadExplosionSprites()
    {
        string basePath = "Assets/Characters/explosion/explosion-";
        Sprite[] sprites = new Sprite[5];
        int loadedCount = 0;
        
        for (int i = 0; i < 5; i++)
        {
            string path = $"{basePath}{i}.png";
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] != null)
                {
                    loadedCount++;
                    Debug.Log($"Loaded explosion sprite: {path}");
                }
                else
                {
                    Debug.LogWarning($"Failed to load sprite at: {path}");
                }
            }
            else
            {
                Debug.LogWarning($"No texture importer found at: {path}");
            }
        }
        
        Debug.Log($"Loaded {loadedCount}/5 explosion sprites");
        return sprites;
    }
    
    static void CreateTagIfNeeded(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Created tag: {tagName}");
        }
    }
    
    [MenuItem("Tools/Test Damage Hell Mouth")]
    public static void TestDamageHellMouth()
    {
        // Find a hell mouth and damage it (for testing)
        HellMouth hm = Object.FindObjectOfType<HellMouth>();
        if (hm != null)
        {
            hm.TakeDamage(1);
            Debug.Log($"Damaged {hm.GetName()}! Health: {hm.GetCurrentHealth()}/{hm.GetMaxHealth()}");
        }
        else
        {
            EditorUtility.DisplayDialog("No Hell Mouth Found",
                "Run 'Setup Hell Mouths' first and enter Play mode to test.",
                "OK");
        }
    }
}

