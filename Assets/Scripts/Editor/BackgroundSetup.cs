using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets up parallax background layers from the background sprites.
/// Run from menu: Tools > Setup Parallax Background
/// </summary>
public class BackgroundSetup : EditorWindow
{
    [MenuItem("Tools/Setup Parallax Background")]
    public static void SetupBackground()
    {
        string basePath = "Assets/Sprites/Background/";
        
        // Create parent container
        GameObject backgroundParent = GameObject.Find("ParallaxBackground");
        if (backgroundParent == null)
        {
            backgroundParent = new GameObject("ParallaxBackground");
        }
        backgroundParent.transform.position = Vector3.zero;
        
        // Layer configuration: layer number, parallax effect (higher = more distant/slower)
        // Layer 7 is farthest back, Layer 1 is closest to camera
        float[] parallaxValues = new float[] {
            0.95f,  // Layer 7 - farthest (almost stationary)
            0.85f,  // Layer 6
            0.75f,  // Layer 5
            0.65f,  // Layer 4
            0.50f,  // Layer 3
            0.35f,  // Layer 2
            0.20f   // Layer 1 - closest (moves more with camera)
        };
        
        int layersCreated = 0;
        
        // Create layers 7 down to 1 (back to front)
        for (int i = 7; i >= 1; i--)
        {
            string spritePath = basePath + i + ".png";
            Sprite sprite = LoadAndConfigureSprite(spritePath);
            
            if (sprite == null)
            {
                Debug.LogWarning($"Could not find background layer: {spritePath}");
                continue;
            }
            
            // Create layer GameObject
            string layerName = $"Parallax_Layer_{i}";
            
            // Look for existing child first
            Transform existingChild = backgroundParent.transform.Find(layerName);
            GameObject layer = existingChild != null ? existingChild.gameObject : null;
            
            if (layer == null)
            {
                layer = new GameObject(layerName);
            }
            layer.transform.parent = backgroundParent.transform;
            layer.transform.localPosition = Vector3.zero;
            
            // Add/configure SpriteRenderer
            SpriteRenderer sr = layer.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = layer.AddComponent<SpriteRenderer>();
            }
            sr.sprite = sprite;
            sr.sortingLayerName = "Background";
            sr.sortingOrder = i - 8; // Layer 7 = -1, Layer 1 = -7 (back to front)
            
            // Add/configure ParallaxLayer script
            ParallaxLayer parallax = layer.GetComponent<ParallaxLayer>();
            if (parallax == null)
            {
                parallax = layer.AddComponent<ParallaxLayer>();
            }
            
            // Set parallax values using SerializedObject
            SerializedObject so = new SerializedObject(parallax);
            so.FindProperty("parallaxEffectX").floatValue = parallaxValues[7 - i];
            so.FindProperty("infiniteHorizontal").boolValue = true;
            
            // Layers 1 & 2 (sky/clouds) tile vertically and follow camera up/down
            bool isSkyLayer = (i == 1 || i == 2);
            if (isSkyLayer)
            {
                // These layers follow camera vertically (with slight parallax) and tile
                so.FindProperty("parallaxEffectY").floatValue = 0.1f; // Moves with camera, slight lag
                so.FindProperty("infiniteVertical").boolValue = true;
            }
            else
            {
                // Other layers stay fixed vertically (grounded elements)
                so.FindProperty("parallaxEffectY").floatValue = 1f;
                so.FindProperty("infiniteVertical").boolValue = false;
            }
            
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(layer);
            layersCreated++;
        }
        
        // Optionally add the main Background.png as the base
        string mainBgPath = basePath + "Background.png";
        Sprite mainBgSprite = LoadAndConfigureSprite(mainBgPath);
        if (mainBgSprite != null)
        {
            Transform existingBase = backgroundParent.transform.Find("Parallax_Base");
            GameObject mainBg = existingBase != null ? existingBase.gameObject : null;
            
            if (mainBg == null)
            {
                mainBg = new GameObject("Parallax_Base");
            }
            mainBg.transform.parent = backgroundParent.transform;
            mainBg.transform.localPosition = new Vector3(0, 0, 10); // Push back in Z
            
            SpriteRenderer sr = mainBg.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = mainBg.AddComponent<SpriteRenderer>();
            }
            sr.sprite = mainBgSprite;
            sr.sortingLayerName = "Background";
            sr.sortingOrder = -10; // Very back
            
            EditorUtility.SetDirty(mainBg);
            layersCreated++;
        }
        
        // Position background at camera position
        Camera cam = Camera.main;
        if (cam != null)
        {
            backgroundParent.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y,
                0
            );
        }
        
        // Select the background parent
        Selection.activeGameObject = backgroundParent;
        
        EditorUtility.DisplayDialog("Parallax Background Setup Complete!",
            $"Created {layersCreated} background layer(s)!\n\n" +
            "✓ All layers parented under 'ParallaxBackground'\n" +
            "✓ Parallax scripts added with varying depths\n" +
            "✓ Sorting layers configured (back to front)\n" +
            "✓ Infinite horizontal scrolling enabled\n" +
            "✓ Layers 1 & 2 (sky/clouds) tile vertically too!\n\n" +
            "Layer 7 = farthest (sky), Layer 1 = closest\n\n" +
            "Press Play to see the parallax effect!",
            "Beautiful!");
    }
    
    static Sprite LoadAndConfigureSprite(string path)
    {
        // Configure importer for pixel art
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Repeat; // For infinite scrolling
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    [MenuItem("Tools/Adjust Parallax Position")]
    public static void AdjustBackgroundPosition()
    {
        GameObject background = GameObject.Find("ParallaxBackground");
        Camera cam = Camera.main;
        
        if (background != null && cam != null)
        {
            // Center background on camera
            background.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y,
                0
            );
            
            Debug.Log("ParallaxBackground repositioned to camera location");
        }
        else
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find ParallaxBackground object or Main Camera",
                "OK");
        }
    }
}

