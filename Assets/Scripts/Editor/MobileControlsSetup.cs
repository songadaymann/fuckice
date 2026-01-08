using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Editor tool to create mobile touch controls UI.
/// </summary>
public class MobileControlsSetup : Editor
{
    [MenuItem("Tools/Setup Mobile Controls")]
    public static void SetupMobileControls()
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
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // Check for EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
        
        // Check if mobile controls already exist
        MobileControls existing = FindObjectOfType<MobileControls>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("Mobile Controls already exist!");
            return;
        }
        
        // Create mobile controls container
        GameObject mobileControls = new GameObject("MobileControls");
        mobileControls.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = mobileControls.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        CanvasGroup canvasGroup = mobileControls.AddComponent<CanvasGroup>();
        MobileControls mobileScript = mobileControls.AddComponent<MobileControls>();
        
        // Create Left Button
        GameObject leftBtn = CreateButton("LeftButton", mobileControls.transform, 
            new Vector2(0, 0), new Vector2(0, 0), // Bottom-left anchor
            new Vector2(120, 120), new Vector2(80, 80), // Size and position
            "◀", 60);
        MobileButton leftMB = leftBtn.AddComponent<MobileButton>();
        SerializedObject leftSO = new SerializedObject(leftMB);
        leftSO.FindProperty("buttonType").enumValueIndex = 0; // Left
        leftSO.ApplyModifiedProperties();
        
        // Create Right Button
        GameObject rightBtn = CreateButton("RightButton", mobileControls.transform,
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(120, 120), new Vector2(220, 80),
            "▶", 60);
        MobileButton rightMB = rightBtn.AddComponent<MobileButton>();
        SerializedObject rightSO = new SerializedObject(rightMB);
        rightSO.FindProperty("buttonType").enumValueIndex = 1; // Right
        rightSO.ApplyModifiedProperties();
        
        // Create Jump Button
        GameObject jumpBtn = CreateButton("JumpButton", mobileControls.transform,
            new Vector2(1, 0), new Vector2(1, 0), // Bottom-right anchor
            new Vector2(150, 150), new Vector2(-100, 100),
            "JUMP", 32);
        MobileButton jumpMB = jumpBtn.AddComponent<MobileButton>();
        SerializedObject jumpSO = new SerializedObject(jumpMB);
        jumpSO.FindProperty("buttonType").enumValueIndex = 2; // Jump
        jumpSO.ApplyModifiedProperties();
        
        // Link references
        SerializedObject so = new SerializedObject(mobileScript);
        so.FindProperty("leftButton").objectReferenceValue = leftBtn.GetComponent<RectTransform>();
        so.FindProperty("rightButton").objectReferenceValue = rightBtn.GetComponent<RectTransform>();
        so.FindProperty("jumpButton").objectReferenceValue = jumpBtn.GetComponent<RectTransform>();
        so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        so.FindProperty("alwaysShow").boolValue = false;
        so.ApplyModifiedProperties();
        
        Selection.activeGameObject = mobileControls;
        
        EditorUtility.DisplayDialog("Mobile Controls Created!",
            "Touch controls added!\n\n" +
            "✓ Left/Right buttons (bottom-left)\n" +
            "✓ Jump button (bottom-right)\n" +
            "✓ Auto-hides on desktop\n" +
            "✓ Shows on mobile/touch devices\n\n" +
            "Set 'Always Show' to true in the Inspector to test on desktop.",
            "Got it!");
    }
    
    private static GameObject CreateButton(string name, Transform parent, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, 
        string label, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.4f);
        
        // Create circular button look
        // (Using default white sprite, could be improved with actual sprites)
        
        // Add text label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Add Button component for visual feedback
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.4f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.8f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
        btn.colors = colors;
        
        return btnObj;
    }
    
}

