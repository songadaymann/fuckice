using UnityEngine;

/// <summary>
/// Triggers dialogue when player enters a zone. Only triggers once.
/// Attach to an empty GameObject with a trigger collider.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 4)]
    public string message = "Hello!";
    public float displayDuration = 4f;
    
    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    public bool destroyAfterTrigger = false;
    
    [Header("Optional Delay")]
    public float delayBeforeShow = 0f;
    
    private bool hasTriggered = false;
    private GameDialogue gameDialogue;
    
    private void Start()
    {
        gameDialogue = FindObjectOfType<GameDialogue>();
        
        // Ensure we have a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            hasTriggered = true;
            
            if (delayBeforeShow > 0)
            {
                StartCoroutine(ShowDialogueDelayed());
            }
            else
            {
                ShowDialogue();
            }
        }
    }
    
    private System.Collections.IEnumerator ShowDialogueDelayed()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        ShowDialogue();
    }
    
    private void ShowDialogue()
    {
        if (gameDialogue != null)
        {
            gameDialogue.ShowDialogue(message, displayDuration);
        }
        else
        {
            Debug.LogWarning("No GameDialogue found in scene!");
        }
        
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Reset trigger so it can fire again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    /// <summary>
    /// Manually trigger the dialogue
    /// </summary>
    public void TriggerDialogue()
    {
        if (!triggerOnce || !hasTriggered)
        {
            hasTriggered = true;
            ShowDialogue();
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw a speech bubble icon in editor
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw trigger area
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.25f);
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}

