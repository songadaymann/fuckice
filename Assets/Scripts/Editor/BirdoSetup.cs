using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// Sets up Birdo, Key, Cage, and Egg prefabs.
/// Run from menu: Tools > Setup Birdo System
/// </summary>
public class BirdoSetup : EditorWindow
{
    [MenuItem("Tools/Setup Birdo System")]
    public static void SetupBirdoSystem()
    {
        // Create prefabs folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Setup all components
        CreateEggPrefab();
        CreateBirdoPrefab();
        CreateKeyPrefab();
        CreateCagePrefab();
        CreateDialogueSystem();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Birdo System Setup Complete!",
            "Created prefabs and scene objects:\n\n" +
            "✓ Egg prefab (Birdo's projectile)\n" +
            "✓ Birdo prefab (companion)\n" +
            "✓ Key prefab (following key)\n" +
            "✓ Cage prefab (holds Birdo)\n" +
            "✓ GameDialogue UI system\n\n" +
            "Next steps:\n" +
            "1. Drag Cage into scene near level start\n" +
            "2. Drag Key somewhere to explore\n" +
            "3. Position Hell Mouths in level\n" +
            "4. Play and rescue Birdo!\n\n" +
            "The key follows you like Mario 2!",
            "Awesome!");
    }
    
    static void CreateEggPrefab()
    {
        string prefabPath = "Assets/Prefabs/BirdoEgg.prefab";
        
        // Load egg sprites
        Sprite egg1 = LoadSprite("Assets/Characters/Items/egg/egg1.png");
        Sprite egg2 = LoadSprite("Assets/Characters/Items/egg/egg2.png");
        Sprite eggExplode = LoadSprite("Assets/Characters/Items/egg/egg-explode.png");
        
        // Create egg object
        GameObject egg = new GameObject("BirdoEgg");
        
        // SpriteRenderer
        SpriteRenderer sr = egg.AddComponent<SpriteRenderer>();
        if (egg1 != null) sr.sprite = egg1;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 10;
        
        // Rigidbody2D
        Rigidbody2D rb = egg.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        
        // CircleCollider2D - NOT a trigger so it can hit trigger colliders (hell mouths)
        CircleCollider2D col = egg.AddComponent<CircleCollider2D>();
        col.radius = 0.2f;
        col.isTrigger = false; // Must be solid to trigger OnTriggerEnter2D on hell mouths!
        
        // BirdoEgg script
        BirdoEgg eggScript = egg.AddComponent<BirdoEgg>();
        
        // Configure via SerializedObject
        SerializedObject so = new SerializedObject(eggScript);
        
        // Set sprites array
        if (egg1 != null && egg2 != null)
        {
            SerializedProperty flyingProp = so.FindProperty("flyingSprites");
            flyingProp.arraySize = 2;
            flyingProp.GetArrayElementAtIndex(0).objectReferenceValue = egg1;
            flyingProp.GetArrayElementAtIndex(1).objectReferenceValue = egg2;
        }
        
        if (eggExplode != null)
        {
            so.FindProperty("explodeSprite").objectReferenceValue = eggExplode;
        }
        
        so.ApplyModifiedProperties();
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(egg, prefabPath);
        Object.DestroyImmediate(egg);
        
        Debug.Log("Created BirdoEgg prefab");
    }
    
    static void CreateBirdoPrefab()
    {
        string prefabPath = "Assets/Prefabs/Birdo.prefab";
        
        // Load Birdo sprites
        Sprite[] normalSprites = LoadSpriteSequence("Assets/Characters/Birdo/birdo-normal", 8);
        Sprite[] shootSprites = LoadSpriteSequence("Assets/Characters/Birdo/birdo-shoot", 4);
        Sprite[] hitSprites = LoadSpriteSequence("Assets/Characters/Birdo/birdo-hit", 4);
        
        // Create Birdo object
        GameObject birdo = new GameObject("Birdo");
        birdo.tag = "Companion";
        
        // SpriteRenderer
        SpriteRenderer sr = birdo.AddComponent<SpriteRenderer>();
        if (normalSprites.Length > 0 && normalSprites[0] != null)
        {
            sr.sprite = normalSprites[0];
        }
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 0;
        
        // Rigidbody2D
        Rigidbody2D rb = birdo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // BoxCollider2D
        BoxCollider2D col = birdo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);
        
        // BirdoCompanion script
        BirdoCompanion companion = birdo.AddComponent<BirdoCompanion>();
        
        // Load egg prefab
        GameObject eggPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BirdoEgg.prefab");
        
        // Configure via SerializedObject
        SerializedObject so = new SerializedObject(companion);
        
        SetSpriteArray(so, "normalSprites", normalSprites);
        SetSpriteArray(so, "shootSprites", shootSprites);
        SetSpriteArray(so, "hitSprites", hitSprites);
        
        if (eggPrefab != null)
        {
            so.FindProperty("eggPrefab").objectReferenceValue = eggPrefab;
        }
        
        so.FindProperty("groundLayer").intValue = LayerMask.GetMask("Ground", "Default");
        so.FindProperty("catchUpSpeed").floatValue = 8f;
        so.FindProperty("teleportDistance").floatValue = 15f;
        so.FindProperty("stuckTime").floatValue = 3f;
        so.FindProperty("jumpForce").floatValue = 12f;
        
        so.ApplyModifiedProperties();
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(birdo, prefabPath);
        Object.DestroyImmediate(birdo);
        
        // Create "Companion" tag if needed
        CreateTagIfNeeded("Companion");
        
        Debug.Log("Created Birdo prefab");
    }
    
    static void CreateKeyPrefab()
    {
        string prefabPath = "Assets/Prefabs/FollowingKey.prefab";
        
        // Load key sprite
        Sprite keySprite = LoadSprite("Assets/Characters/Items/key.png");
        
        // Create key object
        GameObject key = new GameObject("FollowingKey");
        
        // SpriteRenderer
        SpriteRenderer sr = key.AddComponent<SpriteRenderer>();
        if (keySprite != null) sr.sprite = keySprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 5;
        
        // CircleCollider2D for collection
        CircleCollider2D col = key.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;
        
        // FollowingKey script
        key.AddComponent<FollowingKey>();
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(key, prefabPath);
        Object.DestroyImmediate(key);
        
        Debug.Log("Created FollowingKey prefab");
    }
    
    static void CreateCagePrefab()
    {
        string prefabPath = "Assets/Prefabs/BirdoCage.prefab";
        
        // Load cage sprites
        Sprite closedSprite = LoadSprite("Assets/Characters/Items/cage-closed.png");
        Sprite openSprite = LoadSprite("Assets/Characters/Items/cage-open.png");
        
        // Load Birdo prefab
        GameObject birdoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Birdo.prefab");
        
        // Create cage object
        GameObject cage = new GameObject("BirdoCage");
        
        // SpriteRenderer
        SpriteRenderer sr = cage.AddComponent<SpriteRenderer>();
        if (closedSprite != null) sr.sprite = closedSprite;
        sr.sortingLayerName = "Objects";
        sr.sortingOrder = 0;
        
        // Scale down if needed (cage might be large)
        if (closedSprite != null)
        {
            float spriteHeight = closedSprite.bounds.size.y;
            float targetHeight = 3f;
            float scale = targetHeight / spriteHeight;
            cage.transform.localScale = new Vector3(scale, scale, 1);
        }
        
        // BoxCollider2D for trigger
        BoxCollider2D col = cage.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 2f);
        
        // BirdoCage script
        BirdoCage cageScript = cage.AddComponent<BirdoCage>();
        
        // Configure via SerializedObject
        SerializedObject so = new SerializedObject(cageScript);
        
        if (closedSprite != null)
            so.FindProperty("closedSprite").objectReferenceValue = closedSprite;
        if (openSprite != null)
            so.FindProperty("openSprite").objectReferenceValue = openSprite;
        if (birdoPrefab != null)
            so.FindProperty("birdoPrefab").objectReferenceValue = birdoPrefab;
        
        so.ApplyModifiedProperties();
        
        // Create captive Birdo visual (Birdo shown inside cage, animated)
        Sprite[] birdoWalkSprites = LoadSpriteSequence("Assets/Characters/Birdo/birdo-normal", 8);
        if (birdoWalkSprites.Length > 0 && birdoWalkSprites[0] != null)
        {
            GameObject captiveBirdo = new GameObject("CaptiveBirdoVisual");
            captiveBirdo.transform.parent = cage.transform;
            captiveBirdo.transform.localPosition = new Vector3(0, -0.1f, 0);
            captiveBirdo.transform.localScale = Vector3.one * 4f; // Bigger! Visible inside cage
            
            SpriteRenderer captiveSR = captiveBirdo.AddComponent<SpriteRenderer>();
            captiveSR.sprite = birdoWalkSprites[0];
            captiveSR.sortingLayerName = "Objects";
            captiveSR.sortingOrder = -1; // Behind cage bars
            
            // Add walking animation
            CaptiveBirdoAnimation captiveAnim = captiveBirdo.AddComponent<CaptiveBirdoAnimation>();
            
            // Configure animation via SerializedObject
            SerializedObject captiveSO = new SerializedObject(captiveAnim);
            captiveSO.FindProperty("walkSpeed").floatValue = 0.5f;
            captiveSO.FindProperty("walkDistance").floatValue = 0.15f; // Smaller walk in cage
            
            SerializedProperty walkSpritesProp = captiveSO.FindProperty("walkSprites");
            walkSpritesProp.arraySize = birdoWalkSprites.Length;
            for (int j = 0; j < birdoWalkSprites.Length; j++)
            {
                walkSpritesProp.GetArrayElementAtIndex(j).objectReferenceValue = birdoWalkSprites[j];
            }
            captiveSO.ApplyModifiedProperties();
            
            // Assign to cage script
            so = new SerializedObject(cageScript);
            so.FindProperty("captiveBirdoVisual").objectReferenceValue = captiveBirdo;
            so.ApplyModifiedProperties();
        }
        
        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(cage, prefabPath);
        Object.DestroyImmediate(cage);
        
        Debug.Log("Created BirdoCage prefab");
    }
    
    static void CreateDialogueSystem()
    {
        // Check if already exists
        GameDialogue existingDialogue = Object.FindObjectOfType<GameDialogue>();
        if (existingDialogue != null)
        {
            // Just update the font
            AssignSMB2Font(existingDialogue);
            Debug.Log("GameDialogue already exists - updated font");
            return;
        }
        
        // Create dialogue system
        GameObject dialogueObj = new GameObject("GameDialogue");
        GameDialogue dialogue = dialogueObj.AddComponent<GameDialogue>();
        
        // Create retro UI with black box and blue border
        dialogue.CreateBasicUI();
        
        // Assign the SMB2 font
        AssignSMB2Font(dialogue);
        
        Debug.Log("Created GameDialogue system with SMB2 font!");
    }
    
    static void AssignSMB2Font(GameDialogue dialogue)
    {
        // Load the SMB2 font
        Font smb2Font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Super Mario Bros. 2.ttf");
        
        if (smb2Font == null)
        {
            Debug.LogWarning("SMB2 font not found at Assets/Fonts/Super Mario Bros. 2.ttf");
            return;
        }
        
        // Find the dialogue text component in the UI
        Text[] texts = dialogue.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            text.font = smb2Font;
            text.fontSize = 24;
            EditorUtility.SetDirty(text);
            Debug.Log($"Assigned SMB2 font to {text.gameObject.name}");
        }
    }
    
    static Sprite LoadSprite(string path)
    {
        // Configure importer
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    static Sprite[] LoadSpriteSequence(string basePath, int count)
    {
        Sprite[] sprites = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            string path = $"{basePath}{i + 1}.png";
            sprites[i] = LoadSprite(path);
        }
        return sprites;
    }
    
    static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }
    }
    
    static void CreateTagIfNeeded(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
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
        }
    }
    
    [MenuItem("Tools/Create Hell Mouth Trigger Zone")]
    public static void CreateHellMouthTrigger()
    {
        // Creates a trigger zone that shows dialogue when player enters
        GameObject trigger = new GameObject("HellMouthDialogueTrigger");
        
        BoxCollider2D col = trigger.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(5, 3);
        
        DialogueTrigger dt = trigger.AddComponent<DialogueTrigger>();
        
        Selection.activeGameObject = trigger;
        
        EditorUtility.DisplayDialog("Trigger Created",
            "Position this trigger near a Hell Mouth.\n\n" +
            "When the player enters, it will show:\n" +
            "\"It's an ICE AGENT HELL MOUTH! I think only eggs from a transgender bird can close those!\"",
            "Got it!");
    }
}


