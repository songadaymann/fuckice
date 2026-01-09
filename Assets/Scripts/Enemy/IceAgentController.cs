using UnityEngine;
using System.Collections;

/// <summary>
/// ICE Agent enemy controller - chases player, shoots, jumps obstacles, can be stomped.
/// SIMPLIFIED VERSION - clear state machine with predictable behavior.
/// </summary>
public class IceAgentController : MonoBehaviour
{
    public enum State { Run, Shoot, Jump, Smooshed }
    
    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3f;
    
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float shootPauseDuration = 0.5f;
    [SerializeField] private float shootingRange = 6f;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = 1.5f;
    [SerializeField] private float stuckThreshold = 0.5f; // Must be stuck for this long before jumping
    
    [Header("Ground/Wall Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 0.2f;
    
    [Header("Stomp Settings")]
    [SerializeField] private float stompBounceForce = 10f;
    [SerializeField] private float smooshDuration = 0.5f;
    
    [Header("Animation Sprites")]
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite[] jumpSprites;
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] smooshedSprites;
    [SerializeField] private float animationFPS = 8f;
    
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    
    // State
    private State currentState = State.Run;
    private int facingDirection = 1;
    private float shootTimer;
    private float lastJumpTime = -10f;
    private bool isDead = false;
    private float stuckTime = 0f; // How long we've been stuck against a wall
    
    // Animation
    private Sprite[] currentAnimation;
    private int currentFrame;
    private float animTimer;
    
    // Player reference
    private Transform player;
    
    // Ground check points (created dynamically)
    private Vector2 GroundCheckPoint => (Vector2)transform.position + Vector2.down * 0.5f;
    // Wall check at chest height (0.3 units up) to avoid detecting ground tiles
    private Vector2 WallCheckPoint => (Vector2)transform.position + Vector2.up * 0.3f + Vector2.right * facingDirection * 0.5f;
    
    private bool IsGrounded()
    {
        // Simple ground check - raycast down
        RaycastHit2D hit = Physics2D.Raycast(GroundCheckPoint, Vector2.down, 0.2f, groundLayer);
        return hit.collider != null;
    }
    
    private bool IsBlockedByWall()
    {
        // Check if there's a wall in front of us
        RaycastHit2D hit = Physics2D.Raycast(WallCheckPoint, Vector2.right * facingDirection, wallCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            // Don't count Hell Mouths as walls
            if (hit.collider.GetComponent<HellMouth>() != null) return false;
            return true;
        }
        return false;
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        shootTimer = shootInterval;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Auto-configure ground layer if not set
        if (groundLayer.value == 0)
        {
            int defaultLayer = 1 << 0;
            int groundLayerBit = LayerMask.NameToLayer("Ground");
            groundLayer = groundLayerBit >= 0 ? (defaultLayer | (1 << groundLayerBit)) : defaultLayer;
        }
        
        // Create player detection trigger
        CreatePlayerDetectionTrigger();
        
        SetAnimation(runSprites);
    }
    
    private void CreatePlayerDetectionTrigger()
    {
        GameObject triggerObj = new GameObject("PlayerDetector");
        triggerObj.transform.parent = transform;
        triggerObj.transform.localPosition = Vector3.zero;
        triggerObj.transform.localRotation = Quaternion.identity;
        triggerObj.transform.localScale = Vector3.one;
        
        if (col is BoxCollider2D boxCol)
        {
            BoxCollider2D triggerBox = triggerObj.AddComponent<BoxCollider2D>();
            triggerBox.size = boxCol.size;
            triggerBox.offset = boxCol.offset;
            triggerBox.isTrigger = true;
        }
        else if (col is CapsuleCollider2D capsuleCol)
        {
            CapsuleCollider2D triggerCapsule = triggerObj.AddComponent<CapsuleCollider2D>();
            triggerCapsule.size = capsuleCol.size;
            triggerCapsule.offset = capsuleCol.offset;
            triggerCapsule.direction = capsuleCol.direction;
            triggerCapsule.isTrigger = true;
        }
        else
        {
            BoxCollider2D triggerBox = triggerObj.AddComponent<BoxCollider2D>();
            triggerBox.size = new Vector2(1f, 1f);
            triggerBox.isTrigger = true;
        }
        
        PlayerDetector detector = triggerObj.AddComponent<PlayerDetector>();
        detector.Initialize(this);
    }
    
    private void Update()
    {
        if (isDead) return;
        
        UpdateAnimation();
        
        switch (currentState)
        {
            case State.Run:
                UpdateRun();
                break;
            case State.Jump:
                UpdateJump();
                break;
            case State.Shoot:
                // Handled by coroutine
                break;
        }
    }
    
    private void UpdateRun()
    {
        if (player == null) return;
        
        // Always face the player
        facingDirection = player.position.x > transform.position.x ? 1 : -1;
        UpdateSpriteDirection();
        
        // Check for shooting
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer < shootingRange)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                StartCoroutine(ShootRoutine());
                shootTimer = shootInterval;
            }
        }
        
        // Check if we're stuck (not moving horizontally when we should be)
        bool isStuck = Mathf.Abs(rb.velocity.x) < 0.5f && IsGrounded();
        
        if (isStuck && IsBlockedByWall())
        {
            stuckTime += Time.deltaTime;
            
            // Only jump if we've been stuck for a while
            if (stuckTime >= stuckThreshold && CanJump())
            {
                Debug.Log($"{name}: Been stuck for {stuckTime:F1}s, jumping!");
                Jump();
                stuckTime = 0f;
            }
        }
        else
        {
            stuckTime = 0f; // Reset if we're moving
        }
    }
    
    private void UpdateJump()
    {
        // Check if we've landed
        if (IsGrounded() && rb.velocity.y <= 0.1f)
        {
            currentState = State.Run;
            SetAnimation(runSprites);
        }
    }
    
    private void FixedUpdate()
    {
        if (isDead) return;
        
        // Always move toward player (except when shooting or dead)
        if (currentState == State.Shoot || currentState == State.Smooshed)
        {
            return;
        }
        
        // Move horizontally - always chase!
        rb.velocity = new Vector2(facingDirection * chaseSpeed, rb.velocity.y);
    }
    
    private bool CanJump()
    {
        return Time.time - lastJumpTime >= jumpCooldown;
    }
    
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        currentState = State.Jump;
        SetAnimation(jumpSprites);
        lastJumpTime = Time.time;
    }
    
    private IEnumerator ShootRoutine()
    {
        currentState = State.Shoot;
        rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
        SetAnimation(shootSprites);
        
        yield return new WaitForSeconds(shootPauseDuration * 0.5f);
        
        // Fire projectile
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.right * facingDirection * 0.5f;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(facingDirection, projectileSpeed);
            }
            else
            {
                Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
                if (projRb != null)
                {
                    projRb.velocity = new Vector2(facingDirection * projectileSpeed, 0);
                }
            }
        }
        
        yield return new WaitForSeconds(shootPauseDuration * 0.5f);
        
        currentState = State.Run;
        SetAnimation(runSprites);
    }
    
    private void UpdateSpriteDirection()
    {
        // ICE Agent sprite faces RIGHT by default, flip when going LEFT
        spriteRenderer.flipX = facingDirection < 0;
    }
    
    private void SetAnimation(Sprite[] sprites)
    {
        if (sprites != null && sprites.Length > 0)
        {
            currentAnimation = sprites;
            currentFrame = 0;
            animTimer = 0;
            spriteRenderer.sprite = sprites[0];
        }
    }
    
    private void UpdateAnimation()
    {
        if (currentAnimation == null || currentAnimation.Length == 0) return;
        
        animTimer += Time.deltaTime;
        if (animTimer >= 1f / animationFPS)
        {
            animTimer = 0;
            currentFrame = (currentFrame + 1) % currentAnimation.Length;
            spriteRenderer.sprite = currentAnimation[currentFrame];
        }
    }
    
    /// <summary>
    /// Called when player stomps on this enemy
    /// </summary>
    public void GetStomped(Rigidbody2D playerRb)
    {
        if (isDead) return;
        
        isDead = true;
        currentState = State.Smooshed;
        
        if (playerRb != null)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, stompBounceForce);
        }
        
        SetAnimation(smooshedSprites);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        col.enabled = false;
        
        Destroy(gameObject, smooshDuration);
    }
    
    /// <summary>
    /// Handle player contact (called by PlayerDetector trigger)
    /// </summary>
    public void HandlePlayerContact(Collider2D playerCollider)
    {
        if (isDead) return;
        
        float playerY = playerCollider.transform.position.y;
        float enemyTopY = transform.position.y + (col.bounds.size.y * 0.3f);
        
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
        
        if (playerY > enemyTopY && playerRb != null && playerRb.velocity.y < 0)
        {
            GetStomped(playerRb);
        }
        else
        {
            PlayerHealth health = playerCollider.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
            }
        }
    }
    
    /// <summary>
    /// Handle continuous player contact
    /// </summary>
    public void HandlePlayerContactContinuous(Collider2D playerCollider)
    {
        // Not used currently
    }
    
    private void OnDrawGizmos()
    {
        int dir = Application.isPlaying ? facingDirection : 1;
        
        // Ground check (green)
        Gizmos.color = Color.green;
        Vector3 groundPos = transform.position + Vector3.down * 0.5f;
        Gizmos.DrawLine(groundPos, groundPos + Vector3.down * 0.2f);
        
        // Wall check at chest height (cyan)
        Gizmos.color = Color.cyan;
        Vector3 wallPos = transform.position + Vector3.up * 0.3f + Vector3.right * dir * 0.5f;
        Gizmos.DrawLine(wallPos, wallPos + Vector3.right * dir * wallCheckDistance);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Shooting range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
