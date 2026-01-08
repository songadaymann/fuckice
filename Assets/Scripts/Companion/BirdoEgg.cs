using UnityEngine;

/// <summary>
/// Birdo's egg projectile - damages ICE agents and hell mouths.
/// </summary>
public class BirdoEgg : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Sprite[] flyingSprites;
    [SerializeField] private Sprite explodeSprite;
    [SerializeField] private float animationFPS = 10f;
    
    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float explodeDuration = 0.2f;
    
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // State
    private Vector2 direction;
    private float speed;
    private bool hasHit = false;
    private float animTimer;
    private int currentFrame;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure rigidbody for projectile
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }
    
    private void Start()
    {
        // Self-destruct after lifetime
        Destroy(gameObject, maxLifetime);
    }
    
    /// <summary>
    /// Initialize the egg with direction and speed
    /// </summary>
    public void Initialize(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void Update()
    {
        if (hasHit) return;
        
        // Animate flying sprites
        if (flyingSprites != null && flyingSprites.Length > 1)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animationFPS)
            {
                animTimer = 0;
                currentFrame = (currentFrame + 1) % flyingSprites.Length;
                spriteRenderer.sprite = flyingSprites[currentFrame];
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check for Hell Mouth - it handles its own damage via OnTriggerEnter2D
        HellMouth hellMouth = other.GetComponent<HellMouth>();
        if (hellMouth != null)
        {
            Debug.Log("Egg hit hell mouth (trigger)!");
            Explode();
            return;
        }
        
        // Check for ICE Agent
        IceAgentController agent = other.GetComponent<IceAgentController>();
        if (agent != null)
        {
            agent.GetStomped(null);
            Explode();
            return;
        }
        
        // Ignore player and Birdo
        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            return;
        }
        
        // Hit ground/walls
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
            other.CompareTag("Ground"))
        {
            Explode();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        
        // Check for Hell Mouth - it handles its own damage via OnCollisionEnter2D
        HellMouth hellMouth = collision.gameObject.GetComponent<HellMouth>();
        if (hellMouth != null)
        {
            Debug.Log("Egg hit hell mouth (collision)!");
            Explode();
            return;
        }
        
        // Check for ICE Agent
        IceAgentController agent = collision.gameObject.GetComponent<IceAgentController>();
        if (agent != null)
        {
            agent.GetStomped(null);
            Explode();
            return;
        }
        
        // Ignore player and Birdo
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Companion"))
        {
            return;
        }
        
        // Hit anything else - explode
        Explode();
    }
    
    private void Explode()
    {
        hasHit = true;
        
        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // Show explode sprite
        if (explodeSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = explodeSprite;
        }
        
        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Destroy after brief delay
        Destroy(gameObject, explodeDuration);
    }
}

