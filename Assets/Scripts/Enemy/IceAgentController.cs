using UnityEngine;
using System.Collections;

/// <summary>
/// ICE Agent enemy controller - patrols, shoots, and can be stomped.
/// </summary>
public class IceAgentController : MonoBehaviour
{
    public enum State { Run, Shoot, Jump, Smooshed }
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f; // Faster when chasing player
    [SerializeField] private bool alwaysChasePlayer = true; // Always try to reach the player
    [SerializeField] private bool canFallOffEdges = true; // Like goombas!
    
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float shootPauseDuration = 0.5f;
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float shootingRange = 6f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Jumping")]
    [SerializeField] private bool canJumpObstacles = true;
    [SerializeField] private float jumpForce = 12f; // Base jump force
    [SerializeField] private float maxJumpableHeight = 2f; // Won't try to jump walls taller than this
    [SerializeField] private float jumpCooldown = 0.3f; // Time between jump attempts
    
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
    private int facingDirection = -1;
    private Vector3 startPosition;
    private float shootTimer;
    private bool isDead = false;
    private bool isGrounded = true;
    private float lastJumpTime = -10f;
    
    // Animation
    private Sprite[] currentAnimation;
    private int currentFrame;
    private float animTimer;
    
    // Player reference
    private Transform player;
    
    // Helper to check if a collider belongs to a HellMouth (can't use tags - not set!)
    private bool IsHellMouth(Collider2D col)
    {
        return col != null && col.GetComponent<HellMouth>() != null;
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        startPosition = transform.position;
        shootTimer = shootInterval;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // Start facing the player
            facingDirection = player.position.x > transform.position.x ? 1 : -1;
        }
        else
        {
            facingDirection = -1; // Default to left if no player
            Debug.LogWarning($"{name}: No player found!");
        }
        
        // Create groundCheck if missing (for spawned agents)
        if (groundCheck == null)
        {
            Debug.LogWarning($"{name}: No groundCheck assigned! Creating one...");
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.95f, 0);
            groundCheck = gc.transform;
        }
        
        // Create wallCheck if missing
        if (wallCheck == null)
        {
            Debug.LogWarning($"{name}: No wallCheck assigned! Creating one...");
            GameObject wc = new GameObject("WallCheck");
            wc.transform.parent = transform;
            wc.transform.localPosition = new Vector3(0.5f, 0, 0);
            wallCheck = wc.transform;
        }
        
        // Ensure groundLayer is set - if not, set it to Default + Ground layers
        if (groundLayer.value == 0)
        {
            // Layer 0 = Default, Layer 8 is often Ground - use both
            // LayerMask.GetMask returns a bitmask, so we OR them together
            int defaultLayer = 1 << 0; // Layer 0 = Default
            int groundLayerBit = LayerMask.NameToLayer("Ground");
            if (groundLayerBit >= 0)
            {
                groundLayer = defaultLayer | (1 << groundLayerBit);
            }
            else
            {
                groundLayer = defaultLayer; // Just use Default if Ground doesn't exist
            }
            Debug.Log($"{name}: groundLayer was not set! Auto-configured to {groundLayer.value}");
        }
        
        // Log setup info
        Debug.Log($"{name} started: groundCheck={groundCheck != null}, wallCheck={wallCheck != null}, groundLayer={groundLayer.value}, player={player != null}");
        
        // Set initial animation
        SetAnimation(runSprites);
        UpdateSpriteDirection();
    }
    
    private void Update()
    {
        if (isDead) return;
        
        // Ground check
        CheckGrounded();
        
        UpdateAnimation();
        
        switch (currentState)
        {
            case State.Run:
                HandlePatrol();
                CheckForPlayer();
                break;
            case State.Jump:
                // Check if landed
                if (isGrounded && rb.velocity.y <= 0)
                {
                    currentState = State.Run;
                    SetAnimation(runSprites);
                }
                break;
            case State.Shoot:
                // Handled by coroutine
                break;
            case State.Smooshed:
                // Dead, do nothing
                break;
        }
    }
    
    private void CheckGrounded()
    {
        // Use a wider check - if groundLayer isn't set, check everything except triggers
        LayerMask checkMask = groundLayer.value != 0 ? groundLayer : ~0; // ~0 = all layers
        
        Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.5f;
        
        // Use a box cast for more reliable ground detection
        Vector2 boxSize = new Vector2(0.4f, 0.1f); // Wide but thin box
        float distance = 0.3f; // Increased distance
        
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, distance, checkMask);
        
        // Also do a simple velocity check as backup - if we're not moving down much, we're probably grounded
        bool velocityGrounded = rb != null && Mathf.Abs(rb.velocity.y) < 0.1f && rb.velocity.y <= 0;
        
        // Only count as grounded if we hit something that's NOT a Hell Mouth
        if (hit.collider != null && !IsHellMouth(hit.collider))
        {
            isGrounded = true;
        }
        else if (velocityGrounded && currentState != State.Jump)
        {
            // Fallback: if velocity says we're grounded and we're not jumping, trust it
            // But verify with a longer raycast
            RaycastHit2D longHit = Physics2D.Raycast(origin, Vector2.down, 0.5f, checkMask);
            isGrounded = longHit.collider != null && !IsHellMouth(longHit.collider);
        }
        else
        {
            isGrounded = false;
        }
    }
    
    private void FixedUpdate()
    {
        if (isDead || currentState == State.Shoot || currentState == State.Smooshed) return;
        
        // Choose speed - faster when actively chasing
        float currentSpeed = moveSpeed;
        if (alwaysChasePlayer && player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            // Use chase speed when player is in detection range
            if (distToPlayer < detectionRange)
            {
                currentSpeed = chaseSpeed;
            }
        }
        
        // Check if blocked by wall (don't push against it)
        // Ignore Hell Mouths - they're not real walls!
        bool blockedByWall = false;
        if (wallCheck != null && currentState != State.Jump)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, checkDistance * 0.5f, groundLayer);
            if (wallHit.collider != null && !IsHellMouth(wallHit.collider))
            {
                blockedByWall = true;
                
                // KEY FIX: If we're blocked and can jump, DO IT!
                if (canJumpObstacles && isGrounded)
                {
                    Debug.Log($"{name}: BLOCKED by {wallHit.collider.name}, jumping!");
                    TryJump();
                }
            }
        }
        
        // Move - always move while jumping!
        if (currentState == State.Jump)
        {
            // While jumping, just keep horizontal momentum, don't reset velocity
            rb.velocity = new Vector2(facingDirection * currentSpeed, rb.velocity.y);
        }
        else if (!blockedByWall)
        {
            rb.velocity = new Vector2(facingDirection * currentSpeed, rb.velocity.y);
        }
        else
        {
            // Blocked by wall - stop horizontal movement but preserve vertical
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    private void HandlePatrol()
    {
        // Always try to face and chase the player
        if (alwaysChasePlayer && player != null)
        {
            // Determine which direction player is
            int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
            
            // Turn to face player if not already
            if (facingDirection != directionToPlayer)
            {
                facingDirection = directionToPlayer;
                UpdateSpriteDirection();
            }
            
            // Check if player is ABOVE us - need to jump to reach them!
            float heightDiff = player.position.y - transform.position.y;
            float horizontalDist = Mathf.Abs(player.position.x - transform.position.x);
            
            // If player is above us and we're close horizontally, try jumping
            if (heightDiff > 0.5f && horizontalDist < 3f)
            {
                // Debug.Log($"{name}: Player is above! heightDiff={heightDiff:F1}, dist={horizontalDist:F1}, grounded={isGrounded}");
                if (isGrounded) TryJump();
            }
            // Also jump if player is above and there's a wall in front (platform edge)
            else if (heightDiff > 0.5f && isGrounded)
            {
                // Check if there's a platform/wall we need to jump onto (ignore Hell Mouths)
                if (wallCheck != null)
                {
                    RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, checkDistance, groundLayer);
                    if (wallHit.collider != null && !IsHellMouth(wallHit.collider))
                    {
                        TryJump();
                    }
                }
            }
        }
        
        // Check for obstacles ahead - use multiple ray heights to catch all obstacle sizes
        if (canJumpObstacles && isGrounded)
        {
            bool obstacleDetected = false;
            string obstacleName = "";
            float rayDistance = checkDistance * 2f; // Detect obstacles earlier
            
            // Check at wall height (mid-body) for tall obstacles
            if (wallCheck != null)
            {
                RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, rayDistance, groundLayer);
                if (wallHit.collider != null && !IsHellMouth(wallHit.collider))
                {
                    obstacleDetected = true;
                    obstacleName = wallHit.collider.name + " (wall)";
                }
            }
            
            // Check at step height (between foot and mid-body) - catches single tile steps!
            if (!obstacleDetected && groundCheck != null)
            {
                Vector2 stepCheckPos = (Vector2)groundCheck.position + Vector2.up * 0.4f; // ~0.4 units up from feet
                RaycastHit2D stepHit = Physics2D.Raycast(stepCheckPos, Vector2.right * facingDirection, rayDistance, groundLayer);
                if (stepHit.collider != null && !IsHellMouth(stepHit.collider))
                {
                    obstacleDetected = true;
                    obstacleName = stepHit.collider.name + " (step)";
                }
            }
            
            // Check at foot level for very small bumps
            if (!obstacleDetected && groundCheck != null)
            {
                Vector2 footCheckPos = (Vector2)groundCheck.position + Vector2.up * 0.15f;
                RaycastHit2D footHit = Physics2D.Raycast(footCheckPos, Vector2.right * facingDirection, rayDistance, groundLayer);
                if (footHit.collider != null && !IsHellMouth(footHit.collider))
                {
                    obstacleDetected = true;
                    obstacleName = footHit.collider.name + " (foot)";
                }
            }
            
            if (obstacleDetected)
            {
                Debug.Log($"{name}: Obstacle detected ({obstacleName}), jumping!");
                TryJump();
            }
        }
        
        // DEBUG: Show raycast info every few seconds
        if (Time.frameCount % 120 == 0 && wallCheck != null && groundCheck != null)
        {
            float rayDistance = checkDistance * 2f;
            RaycastHit2D debugWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, rayDistance, groundLayer);
            RaycastHit2D debugStep = Physics2D.Raycast((Vector2)groundCheck.position + Vector2.up * 0.4f, Vector2.right * facingDirection, rayDistance, groundLayer);
            RaycastHit2D debugFoot = Physics2D.Raycast((Vector2)groundCheck.position + Vector2.up * 0.15f, Vector2.right * facingDirection, rayDistance, groundLayer);
            
            // Ground check debug
            RaycastHit2D debugGround = Physics2D.BoxCast(groundCheck.position, new Vector2(0.4f, 0.1f), 0f, Vector2.down, 0.3f, groundLayer);
            
            Debug.Log($"{name} DEBUG: grounded={isGrounded}, vel.y={rb.velocity.y:F2}, groundHit={debugGround.collider?.name ?? "NONE"}, groundCheckPos={groundCheck.position}, groundLayer={groundLayer.value}");
            Debug.Log($"{name} raycast: wallHit={debugWall.collider?.name ?? "none"}, stepHit={debugStep.collider?.name ?? "none"}, footHit={debugFoot.collider?.name ?? "none"}, facing={facingDirection}");
        }
        
        // Check for ledges - even chasers shouldn't run off cliffs (unless enabled)
        if (!canFallOffEdges && groundCheck != null && isGrounded)
        {
            bool hasGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);
            if (!hasGroundAhead)
            {
                // Stop at edge but don't turn - wait for player
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
        }
        
        // Update sprite direction
        UpdateSpriteDirection();
    }
    
    private bool CanJumpOverObstacle()
    {
        if (!isGrounded) return false;
        if (Time.time - lastJumpTime < jumpCooldown) return false;
        
        // Cast a ray from higher up to see if obstacle is short enough to jump
        Vector3 highCheckPos = wallCheck.position + Vector3.up * maxJumpableHeight;
        RaycastHit2D highHit = Physics2D.Raycast(highCheckPos, Vector2.right * facingDirection, checkDistance * 1.5f, groundLayer);
        
        // If high ray doesn't hit anything, obstacle is jumpable!
        return highHit.collider == null;
    }
    
    private void TryJump()
    {
        // Debug info - uncomment to diagnose jump issues
        if (!isGrounded)
        {
            Debug.Log($"{name}: Can't jump - not grounded (groundCheck={groundCheck}, groundLayer={groundLayer.value})");
            return;
        }
        if (currentState == State.Jump)
        {
            Debug.Log($"{name}: Can't jump - already jumping");
            return;
        }
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            // Debug.Log($"{name}: Can't jump - cooldown"); // This one is spammy
            return;
        }
        
        // Calculate jump force - jump higher if player is above us
        float actualJumpForce = jumpForce;
        if (player != null)
        {
            float heightDiff = player.position.y - transform.position.y;
            if (heightDiff > 1f)
            {
                // Need to jump higher! Scale jump force based on height difference
                actualJumpForce = jumpForce * Mathf.Clamp(1f + (heightDiff * 0.3f), 1f, 1.8f);
            }
        }
        
        Debug.Log($"{name}: JUMPING with force {actualJumpForce}!");
        
        // Jump!
        rb.velocity = new Vector2(rb.velocity.x, actualJumpForce);
        currentState = State.Jump;
        SetAnimation(jumpSprites);
        lastJumpTime = Time.time;
        isGrounded = false;
    }
    
    private void UpdateSpriteDirection()
    {
        // Flip sprite based on movement direction
        // ICE Agent sprite faces RIGHT by default, flip when going LEFT
        spriteRenderer.flipX = facingDirection < 0;
    }
    
    private void CheckForPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is in shooting range
        if (distanceToPlayer < shootingRange)
        {
            // Shoot timer
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                // Face the player only when actually shooting
                if (player.position.x > transform.position.x && facingDirection < 0)
                {
                    Flip();
                }
                else if (player.position.x < transform.position.x && facingDirection > 0)
                {
                    Flip();
                }
                
                StartCoroutine(ShootRoutine());
                shootTimer = shootInterval;
            }
        }
    }
    
    private IEnumerator ShootRoutine()
    {
        currentState = State.Shoot;
        rb.velocity = Vector2.zero;
        SetAnimation(shootSprites);
        
        yield return new WaitForSeconds(shootPauseDuration * 0.5f);
        
        // Fire projectile
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(facingDirection, projectileSpeed);
            }
            else
            {
                // Basic projectile movement
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
    
    private void Flip()
    {
        facingDirection *= -1;
        UpdateSpriteDirection();
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
        
        // Bounce the player
        if (playerRb != null)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, stompBounceForce);
        }
        
        // Play death animation
        SetAnimation(smooshedSprites);
        
        // Stop movement
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Disable collider
        col.enabled = false;
        
        // Destroy after animation
        Destroy(gameObject, smooshDuration);
    }
    
    /// <summary>
    /// Check if stomp came from above
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if player is above us (stomp)
            float playerY = collision.transform.position.y;
            float enemyTopY = transform.position.y + (col.bounds.size.y * 0.3f);
            
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (playerY > enemyTopY && playerRb != null && playerRb.velocity.y < 0)
            {
                // Stomped!
                GetStomped(playerRb);
            }
            else
            {
                // Player hit from side - hurt player
                PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1);
                }
            }
        }
    }
    
    /// <summary>
    /// Detect when agent is physically pressed against something - more reliable than raycasts for single steps
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead || currentState == State.Jump || currentState == State.Shoot) return;
        if (!canJumpObstacles || !isGrounded) return;
        
        // Don't try to jump over players or Hell Mouths
        if (collision.gameObject.CompareTag("Player")) return;
        if (IsHellMouth(collision.collider)) return;
        
        // Check each contact point to see if we're being blocked horizontally
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Get the contact normal - if it's mostly horizontal and opposing our movement, we're blocked
            float horizontalComponent = Vector2.Dot(contact.normal, Vector2.right * -facingDirection);
            
            if (horizontalComponent > 0.7f) // Contact is blocking our forward movement
            {
                // Check if the contact is at a jumpable height (not too high, not at head level)
                float contactHeight = contact.point.y - transform.position.y;
                
                // Only jump if the obstacle is at foot/leg level (roughly -0.5 to 0.5 of our center)
                if (contactHeight >= -0.8f && contactHeight <= 0.5f)
                {
                    Debug.Log($"{name}: Collision-based jump trigger! Contact={collision.gameObject.name}, height={contactHeight:F2}, normal={contact.normal}");
                    TryJump();
                    return; // Only need one jump trigger
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Always show obstacle detection rays (not just when selected)
        int dir = Application.isPlaying ? facingDirection : 1;
        float rayDistance = checkDistance * 2f;
        
        // Wall check ray (cyan) - for tall obstacles
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * dir * rayDistance);
        }
        
        // Step check ray (yellow) - for single-tile steps
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 stepPos = groundCheck.position + Vector3.up * 0.4f;
            Gizmos.DrawLine(stepPos, stepPos + Vector3.right * dir * rayDistance);
        }
        
        // Foot check ray (magenta) - for very small obstacles
        if (groundCheck != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 footPos = groundCheck.position + Vector3.up * 0.15f;
            Gizmos.DrawLine(footPos, footPos + Vector3.right * dir * rayDistance);
        }
        
        // Ground check ray (green)
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.2f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Shooting range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}

