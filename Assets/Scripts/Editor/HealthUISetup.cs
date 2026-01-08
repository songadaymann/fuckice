using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor tool to set up the Health UI with heart sprites.
/// </summary>
public class HealthUISetup : Editor
{
    [MenuItem("Tools/Setup Health UI")]
    public static void SetupHealthUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Set canvas scaler for pixel perfect look
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            Debug.Log("Created Canvas");
        }
        
        // Check if HealthUI already exists
        HealthUI existingUI = FindObjectOfType<HealthUI>();
        if (existingUI != null)
        {
            Debug.Log("HealthUI already exists! Updating sprites...");
            UpdateHealthUISprites(existingUI);
            return;
        }
        
        // Create HealthUI container
        GameObject healthUIObj = new GameObject("HealthUI");
        healthUIObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = healthUIObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); // Top-left
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(200, 50);
        
        HealthUI healthUI = healthUIObj.AddComponent<HealthUI>();
        
        // Load heart sprites
        UpdateHealthUISprites(healthUI);
        
        // Select it
        Selection.activeGameObject = healthUIObj;
        
        Debug.Log("Health UI created! Make sure to save the scene.");
    }
    
    private static void UpdateHealthUISprites(HealthUI healthUI)
    {
        // Try to find heart sprites
        string[] heartFullGuids = AssetDatabase.FindAssets("heart-full t:Sprite");
        string[] heartHalfGuids = AssetDatabase.FindAssets("heart-half t:Sprite");
        string[] heartEmptyGuids = AssetDatabase.FindAssets("heart-empty t:Sprite");
        
        SerializedObject so = new SerializedObject(healthUI);
        
        if (heartFullGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(heartFullGuids[0]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            so.FindProperty("heartFull").objectReferenceValue = sprite;
            Debug.Log($"Found heart-full: {path}");
        }
        else
        {
            Debug.LogWarning("Could not find heart-full sprite! Please add it to Assets/Sprites/UI/Hearts/heart-full.png");
        }
        
        if (heartHalfGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(heartHalfGuids[0]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            so.FindProperty("heartHalf").objectReferenceValue = sprite;
            Debug.Log($"Found heart-half: {path}");
        }
        else
        {
            Debug.LogWarning("Could not find heart-half sprite! Please add it to Assets/Sprites/UI/Hearts/heart-half.png");
        }
        
        if (heartEmptyGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(heartEmptyGuids[0]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            so.FindProperty("heartEmpty").objectReferenceValue = sprite;
            Debug.Log($"Found heart-empty: {path}");
        }
        else
        {
            Debug.LogWarning("Could not find heart-empty sprite! Please add it to Assets/Sprites/UI/Hearts/heart-empty.png");
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(healthUI);
    }
}

