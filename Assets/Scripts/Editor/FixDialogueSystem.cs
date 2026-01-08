using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Fixes the broken dialogue system by rebuilding it completely.
/// Run from menu: Tools > Fix Dialogue System
/// </summary>
public class FixDialogueSystem : Editor
{
    [MenuItem("Tools/Fix Dialogue System")]
    public static void FixDialogue()
    {
        Debug.Log("=== Fixing Dialogue System ===");
        
        // Step 1: Find and delete broken dialogue triggers
        DeleteBrokenTriggers();
        
        // Step 2: Fix or recreate GameDialogue
        FixGameDialogue();
        
        EditorUtility.DisplayDialog("Dialogue System Fixed!",
            "✓ Removed broken dialogue triggers\n" +
            "✓ Created dialogue UI (Canvas + Panel + Text)\n" +
            "✓ Connected GameDialogue to UI\n\n" +
            "Next: Run 'Tools > Dialogue > Create All Story Triggers' to create new trigger zones!",
            "Great!");
    }
    
    static void DeleteBrokenTriggers()
    {
        // Find all GameObjects with "DialogueTrigger" in the name that have missing scripts
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int deleted = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("DialogueTrigger") || obj.name.Contains("Dialogue"))
            {
                // Check for missing scripts
                Component[] components = obj.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null)
                    {
                        Debug.Log($"Deleting broken object: {obj.name}");
                        Undo.DestroyObjectImmediate(obj);
                        deleted++;
                        break;
                    }
                }
            }
        }
        
        Debug.Log($"Deleted {deleted} broken dialogue trigger objects");
    }
    
    static void FixGameDialogue()
    {
        // Find existing GameDialogue or create new one
        GameDialogue existingDialogue = Object.FindObjectOfType<GameDialogue>();
        
        if (existingDialogue != null)
        {
            // Check if it has valid UI references
            SerializedObject so = new SerializedObject(existingDialogue);
            SerializedProperty panelProp = so.FindProperty("dialoguePanel");
            SerializedProperty textProp = so.FindProperty("dialogueText");
            
            bool needsUI = panelProp.objectReferenceValue == null || textProp.objectReferenceValue == null;
            
            if (needsUI)
            {
                Debug.Log("GameDialogue exists but has no UI - creating UI...");
                CreateDialogueUI(existingDialogue);
            }
            else
            {
                Debug.Log("GameDialogue already has valid UI");
            }
        }
        else
        {
            Debug.Log("No GameDialogue found - creating from scratch...");
            CreateCompleteDialogueSystem();
        }
    }
    
    static void CreateCompleteDialogueSystem()
    {
        // Create the GameDialogue object
        GameObject dialogueObj = new GameObject("GameDialogue");
        GameDialogue dialogue = dialogueObj.AddComponent<GameDialogue>();
        
        CreateDialogueUI(dialogue);
        
        Undo.RegisterCreatedObjectUndo(dialogueObj, "Create Dialogue System");
    }
    
    static void CreateDialogueUI(GameDialogue dialogue)
    {
        // Find or create Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // On top of everything
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Debug.Log("Created Canvas");
        }
        
        // Create outer border (light blue) - this is the main "panel" that gets shown/hidden
        // More compact size - centered, narrower, good for reading
        GameObject borderObj = new GameObject("DialogueBorder");
        borderObj.transform.SetParent(canvas.transform, false);
        
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        // Centered horizontally, near top - narrower for better readability
        borderRect.anchorMin = new Vector2(0.2f, 0.78f);
        borderRect.anchorMax = new Vector2(0.8f, 0.95f);
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = new Color(0.4f, 0.75f, 1f, 1f); // Light blue border
        
        // Create inner panel (solid black)
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(borderObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = new Vector2(6, 6); // 6px border
        panelRect.offsetMax = new Vector2(-6, -6);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = Color.black;
        
        // Create text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.03f, 0.1f);
        textRect.anchorMax = new Vector2(0.97f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text dialogueText = textObj.AddComponent<Text>();
        dialogueText.fontSize = 32;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.MiddleCenter;
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Try to load SMB2 font
        Font smb2Font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Super Mario Bros. 2.ttf");
        if (smb2Font != null)
        {
            dialogueText.font = smb2Font;
            Debug.Log("Applied SMB2 font to dialogue");
        }
        else
        {
            dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Debug.LogWarning("SMB2 font not found, using default font");
        }
        
        // Connect to GameDialogue
        SerializedObject so = new SerializedObject(dialogue);
        so.FindProperty("dialoguePanel").objectReferenceValue = borderObj; // Show/hide the border
        so.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        so.FindProperty("useTypewriter").boolValue = false; // Disable for now for instant text
        so.ApplyModifiedProperties();
        
        // Start hidden
        borderObj.SetActive(false);
        
        EditorUtility.SetDirty(dialogue);
        
        Debug.Log("Created dialogue UI with black panel and blue border!");
    }
    
    [MenuItem("Tools/Test Dialogue")]
    public static void TestDialogue()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Play Mode Required",
                "Enter Play Mode first, then use this to test the dialogue.", "OK");
            return;
        }
        
        GameDialogue.Show("This is a test message! If you can see this, the dialogue system is working!", 3f);
    }
}

