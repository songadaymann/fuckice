using UnityEngine;

/// <summary>
/// A checkpoint that saves player progress when touched.
/// Place these throughout the level at safe spots.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private bool showVisualFeedback = true;
    
    [Header("Settings")]
    [SerializeField] private bool activateOnce = true; // Only trigger once per life
    
    private SpriteRenderer spriteRenderer;
    private bool hasBeenActivated = false;
    private static Checkpoint lastActivatedCheckpoint = null;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && showVisualFeedback)
        {
            spriteRenderer.color = inactiveColor;
        }
        
        // Ensure we have a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (activateOnce && hasBeenActivated) return;
        
        // Get player health and set checkpoint
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetCheckpoint(transform.position);
            Activate();
        }
    }
    
    private void Activate()
    {
        hasBeenActivated = true;
        
        // Deactivate previous checkpoint visually
        if (lastActivatedCheckpoint != null && lastActivatedCheckpoint != this)
        {
            lastActivatedCheckpoint.Deactivate();
        }
        
        lastActivatedCheckpoint = this;
        
        // Visual feedback
        if (spriteRenderer != null && showVisualFeedback)
        {
            spriteRenderer.color = activeColor;
        }
        
        Debug.Log($"Checkpoint activated: {gameObject.name}");
        
        // Optional: Play sound, particle effect, etc.
    }
    
    private void Deactivate()
    {
        if (spriteRenderer != null && showVisualFeedback)
        {
            spriteRenderer.color = inactiveColor;
        }
    }
    
    // Reset when scene reloads (for respawning)
    private void OnDestroy()
    {
        if (lastActivatedCheckpoint == this)
        {
            lastActivatedCheckpoint = null;
        }
    }
}

