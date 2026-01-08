using UnityEngine;

/// <summary>
/// Mario 2 style key that follows the player when collected.
/// Bounces along behind the player until used on a cage.
/// </summary>
public class FollowingKey : MonoBehaviour
{
    [Header("Following Behavior")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceSpeed = 8f;
    
    [Header("Collection")]
    [SerializeField] private float collectRadius = 1f;
    [SerializeField] private bool autoCollect = true;
    
    [Header("Animation")]
    [SerializeField] private Sprite keySprite;
    [SerializeField] private float rotateSpeed = 90f; // Degrees per second when idle
    
    // State
    private bool isCollected = false;
    private Transform player;
    private Vector3 targetPosition;
    private float bounceTimer;
    private SpriteRenderer spriteRenderer;
    
    // For cage interaction
    public bool IsCollected => isCollected;
    
    // Events
    public System.Action OnCollected;
    public System.Action OnUsed;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            // Idle animation - gentle rotation/bob
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            
            // Check for player proximity to collect
            if (autoCollect && player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist < collectRadius)
                {
                    Collect();
                }
            }
        }
        else
        {
            // Following behavior
            FollowPlayer();
        }
    }
    
    private void FollowPlayer()
    {
        if (player == null) return;
        
        // Target position is behind the player
        float behindOffset = player.localScale.x > 0 ? -followDistance : followDistance;
        targetPosition = player.position + new Vector3(behindOffset, 0.5f, 0);
        
        // Move toward target with bounce
        bounceTimer += Time.deltaTime * bounceSpeed;
        float bounceY = Mathf.Abs(Mathf.Sin(bounceTimer)) * bounceHeight;
        
        Vector3 desiredPos = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        desiredPos.y = targetPosition.y + bounceY;
        
        transform.position = desiredPos;
        
        // Reset rotation when following
        transform.rotation = Quaternion.identity;
    }
    
    /// <summary>
    /// Collect the key (starts following player)
    /// </summary>
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        OnCollected?.Invoke();
        
        // Disable collider so it doesn't interfere
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Debug.Log("Key collected! It's following you now.");
    }
    
    /// <summary>
    /// Use the key on something (like a cage)
    /// </summary>
    public void UseKey()
    {
        OnUsed?.Invoke();
        
        // Animate key into lock, then destroy
        // For now, just destroy
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Manual collection via trigger
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Collection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}

