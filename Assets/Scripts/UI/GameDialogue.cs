using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Displays story/tutorial dialogue on screen.
/// Singleton pattern for easy access from any script.
/// </summary>
public class GameDialogue : MonoBehaviour
{
    public static GameDialogue Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText; // Use TextMeshProUGUI if you have TMP
    [SerializeField] private Image speakerImage;
    
    [Header("Timing")]
    [SerializeField] private float defaultDisplayDuration = 4f;
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private bool useTypewriter = true;
    
    [Header("Styling")]
    [SerializeField] private Color defaultTextColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip showSound;
    
    // State
    private Coroutine currentDialogue;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Hide dialogue initially
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show a dialogue message
    /// </summary>
    public void ShowDialogue(string message, float duration = -1, Sprite speaker = null)
    {
        if (currentDialogue != null)
        {
            StopCoroutine(currentDialogue);
        }
        
        float dur = duration > 0 ? duration : defaultDisplayDuration;
        currentDialogue = StartCoroutine(ShowDialogueRoutine(message, dur, speaker));
    }
    
    /// <summary>
    /// Static helper for easy access
    /// </summary>
    public static void Show(string message, float duration = -1)
    {
        if (Instance != null)
        {
            Instance.ShowDialogue(message, duration);
        }
        else
        {
            Debug.Log($"[Dialogue]: {message}");
        }
    }
    
    private IEnumerator ShowDialogueRoutine(string message, float duration, Sprite speaker)
    {
        // Show panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // Play show sound
        if (showSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(showSound);
        }
        
        // Set speaker image
        if (speakerImage != null)
        {
            if (speaker != null)
            {
                speakerImage.sprite = speaker;
                speakerImage.gameObject.SetActive(true);
            }
            else
            {
                speakerImage.gameObject.SetActive(false);
            }
        }
        
        // Display text
        if (dialogueText != null)
        {
            if (useTypewriter)
            {
                yield return StartCoroutine(TypewriterEffect(message));
            }
            else
            {
                dialogueText.text = message;
            }
        }
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Hide
        HideDialogue();
    }
    
    private IEnumerator TypewriterEffect(string message)
    {
        dialogueText.text = "";
        
        foreach (char c in message)
        {
            dialogueText.text += c;
            
            // Play type sound
            if (typeSound != null && audioSource != null && c != ' ')
            {
                audioSource.PlayOneShot(typeSound);
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    /// <summary>
    /// Hide the dialogue immediately
    /// </summary>
    public void HideDialogue()
    {
        if (currentDialogue != null)
        {
            StopCoroutine(currentDialogue);
            currentDialogue = null;
        }
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Create a retro SMB2-style UI with black box and light blue border
    /// </summary>
    [ContextMenu("Create Basic UI")]
    public void CreateBasicUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create outer border (light blue)
        if (dialoguePanel == null)
        {
            // Border container
            GameObject borderObj = new GameObject("DialogueBorder");
            borderObj.transform.SetParent(canvas.transform, false);
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0.05f, 0.7f);
            borderRect.anchorMax = new Vector2(0.95f, 0.92f);
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = new Color(0.4f, 0.7f, 1f, 1f); // Light blue border
            
            // Inner panel (solid black)
            dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(borderObj.transform, false);
            
            RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(4, 4); // 4px border
            panelRect.offsetMax = new Vector2(-4, -4);
            
            Image panelImage = dialoguePanel.AddComponent<Image>();
            panelImage.color = Color.black; // Solid black background
            
            // Store border reference for hiding
            dialoguePanel = borderObj;
        }
        
        // Create text with SMB2 font
        if (dialogueText == null)
        {
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(dialoguePanel.transform.GetChild(0), false); // Parent to inner panel
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.03f, 0.1f);
            textRect.anchorMax = new Vector2(0.97f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            dialogueText = textObj.AddComponent<Text>();
            dialogueText.fontSize = 28;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAnchor.MiddleCenter;
            
            // Note: Font must be loaded separately via editor script
            Debug.Log("Text created - run 'Setup Birdo System' to assign SMB2 font!");
        }
        
        dialoguePanel.SetActive(false);
        Debug.Log("Retro dialogue UI created with black box and light blue border!");
    }
}

