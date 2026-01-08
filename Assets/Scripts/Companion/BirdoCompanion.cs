using UnityEngine;
using System.Collections;

/// <summary>
/// Birdo companion AI - follows player, shoots eggs at ICE agents and hell mouths.
/// </summary>
public class BirdoCompanion : MonoBehaviour
{
    [Header("Following")]
    [SerializeField] private float followSpeed = 6f; // Faster base speed
    [SerializeField] private float catchUpSpeed = 12f; // Much faster when far away
    [SerializeField] private float followDistance = 1.2f; // Stay closer to player
    [SerializeField] private float jumpForce = 14f; // Jump higher to keep up
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Teleport When Lost")]
    [SerializeField] private float teleportDistance = 8f; // Teleport sooner if too far
    [SerializeField] private float stuckTime = 1.5f; // Teleport faster when stuck
    [SerializeField] private float teleportOffset = 1f; // Appear closer behind player
    
    [Header("Combat")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float eggSpeed = 8f;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float hellMouthShootRange = 5f;
    
    [Header("Animation Sprites")]
    [SerializeField] private Sprite[] normalSprites;
    [SerializeField] private Sprite[] shootSprites;
    [SerializeField] private Sprite[] hitSprites;
    [SerializeField] private float animationFPS = 10f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip shootSound;
    
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Transform groundCheck;
    
    // State
    private Transform player;
    private float shootTimer;
    private bool isGrounded;
    private int facingDirection = 1;
    private bool isShooting = false;
    
    // Teleport/stuck tracking
    private Vector3 lastPosition;
    private float stuckTimer;
    private float lastJumpTime;
    
    // Animation
    private Sprite[] currentAnimation;
    private int currentFrame;
    private float animTimer;
    
    // Tracking targets
    private Transform currentTarget;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create ground check
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.parent = transform;
        groundCheck.localPosition = new Vector3(0, -0.5f, 0);
    }
    
    private void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            
            // Ignore collision between Birdo and Player
            Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
            Collider2D myCollider = GetComponent<Collider2D>();
            if (playerCollider != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, playerCollider);
            }
        }
        
        // Set initial animation
        SetAnimation(normalSprites);
        
        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.parent = transform;
            fp.transform.localPosition = new Vector3(0.5f, 0.2f, 0);
            firePoint = fp.transform;
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        // Ground check
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        
        // Update timers
        shootTimer -= Time.deltaTime;
        
        // Check if we need to teleport
        CheckTeleport();
        
        // Look for targets
        FindAndShootTargets();
        
        // Update animation
        UpdateAnimation();
    }
    
    private void CheckTeleport()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Teleport if too far away
        if (distToPlayer > teleportDistance)
        {
            TeleportToPlayer();
            return;
        }
        
        // Check if stuck (not moving much)
        float movedDistance = Vector2.Distance(transform.position, lastPosition);
        if (movedDistance < 0.1f && distToPlayer > followDistance * 2)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTime)
            {
                TeleportToPlayer();
                return;
            }
        }
        else
        {
            stuckTimer = 0;
        }
        
        lastPosition = transform.position;
    }
    
    private void TeleportToPlayer()
    {
        // Teleport behind the player
        float behindOffset = player.localScale.x > 0 ? -teleportOffset : teleportOffset;
        Vector3 teleportPos = player.position + new Vector3(behindOffset, 0.5f, 0);
        
        // Make sure we're not inside ground
        RaycastHit2D groundHit = Physics2D.Raycast(teleportPos, Vector2.down, 5f, groundLayer);
        if (groundHit.collider != null)
        {
            teleportPos.y = groundHit.point.y + 0.5f;
        }
        
        transform.position = teleportPos;
        rb.velocity = Vector2.zero;
        stuckTimer = 0;
        
        Debug.Log("Birdo teleported to catch up!");
    }
    
    private void FixedUpdate()
    {
        if (player == null) return;
        
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        float distToPlayerX = player.position.x - transform.position.x;
        float absDistToPlayerX = Mathf.Abs(distToPlayerX);
        float distToPlayerY = player.position.y - transform.position.y;
        float totalDist = Vector2.Distance(transform.position, player.position);
        
        // Face the direction of movement or target
        if (currentTarget != null)
        {
            facingDirection = currentTarget.position.x > transform.position.x ? 1 : -1;
        }
        else if (Mathf.Abs(distToPlayerX) > 0.5f)
        {
            facingDirection = distToPlayerX > 0 ? 1 : -1;
        }
        
        // Birdo sprite faces RIGHT by default, flip when going LEFT
        spriteRenderer.flipX = facingDirection < 0;
        
        // Choose speed based on distance (catch up if far)
        float currentSpeed = totalDist > followDistance * 3 ? catchUpSpeed : followSpeed;
        
        // Move toward player if too far
        if (absDistToPlayerX > followDistance)
        {
            float moveDir = Mathf.Sign(distToPlayerX);
            rb.velocity = new Vector2(moveDir * currentSpeed, rb.velocity.y);
        }
        else
        {
            // Stop when close enough horizontally
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // Jumping logic - be more aggressive about following upward
        bool canJump = isGrounded && Time.time - lastJumpTime > 0.3f;
        
        if (canJump)
        {
            bool shouldJump = false;
            
            // Jump if player is above us
            if (distToPlayerY > 1f)
            {
                shouldJump = true;
            }
            
            // Jump if blocked by obstacle
            if (IsBlockedAhead() && absDistToPlayerX > 0.5f)
            {
                shouldJump = true;
            }
            
            // Jump if player is on a platform above and we're below
            if (distToPlayerY > 0.5f && absDistToPlayerX < followDistance * 2)
            {
                shouldJump = true;
            }
            
            if (shouldJump)
            {
                // Calculate jump force based on height difference
                float neededForce = jumpForce;
                if (distToPlayerY > 3f)
                {
                    neededForce = jumpForce * 1.3f; // Jump higher for bigger gaps
                }
                
                rb.velocity = new Vector2(rb.velocity.x, neededForce);
                lastJumpTime = Time.time;
            }
        }
        
        // Air control - steer toward player while in air
        if (!isGrounded && absDistToPlayerX > 1f)
        {
            float airControl = Mathf.Sign(distToPlayerX) * currentSpeed * 0.5f;
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, airControl, Time.deltaTime * 3f), rb.velocity.y);
        }
    }
    
    private bool IsBlockedAhead()
    {
        Vector2 rayStart = transform.position + new Vector3(0, 0.5f, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right * facingDirection, 0.8f, groundLayer);
        return hit.collider != null;
    }
    
    private void FindAndShootTargets()
    {
        if (shootTimer > 0 || isShooting) return;
        
        // Priority 1: Hell Mouths in range
        HellMouth nearestHellMouth = FindNearestHellMouth();
        if (nearestHellMouth != null)
        {
            float dist = Vector2.Distance(transform.position, nearestHellMouth.transform.position);
            if (dist < hellMouthShootRange)
            {
                currentTarget = nearestHellMouth.transform;
                StartCoroutine(ShootRoutine(nearestHellMouth.transform.position));
                return;
            }
        }
        
        // Priority 2: ICE Agents
        IceAgentController nearestAgent = FindNearestIceAgent();
        if (nearestAgent != null)
        {
            float dist = Vector2.Distance(transform.position, nearestAgent.transform.position);
            if (dist < detectionRange)
            {
                currentTarget = nearestAgent.transform;
                StartCoroutine(ShootRoutine(nearestAgent.transform.position));
                return;
            }
        }
        
        currentTarget = null;
    }
    
    private HellMouth FindNearestHellMouth()
    {
        HellMouth[] hellMouths = FindObjectsOfType<HellMouth>();
        HellMouth nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (var hm in hellMouths)
        {
            if (hm.IsClosed()) continue;
            
            float dist = Vector2.Distance(transform.position, hm.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hm;
            }
        }
        
        return nearest;
    }
    
    private IceAgentController FindNearestIceAgent()
    {
        IceAgentController[] agents = FindObjectsOfType<IceAgentController>();
        IceAgentController nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (var agent in agents)
        {
            float dist = Vector2.Distance(transform.position, agent.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = agent;
            }
        }
        
        return nearest;
    }
    
    private IEnumerator ShootRoutine(Vector3 targetPos)
    {
        isShooting = true;
        
        // Face target
        facingDirection = targetPos.x > transform.position.x ? 1 : -1;
        // Birdo sprite faces RIGHT by default, flip when facing LEFT
        spriteRenderer.flipX = facingDirection < 0;
        
        // Shoot animation
        SetAnimation(shootSprites);
        
        yield return new WaitForSeconds(0.2f);
        
        // Fire egg
        ShootEgg(targetPos);
        
        yield return new WaitForSeconds(0.3f);
        
        // Return to normal
        SetAnimation(normalSprites);
        shootTimer = shootCooldown;
        isShooting = false;
    }
    
    private void ShootEgg(Vector3 targetPos)
    {
        if (eggPrefab == null)
        {
            Debug.LogWarning("No egg prefab assigned to Birdo!");
            return;
        }
        
        // Calculate fire point position based on facing direction
        Vector3 spawnPos = transform.position + new Vector3(facingDirection * 0.5f, 0.2f, 0);
        
        GameObject egg = Instantiate(eggPrefab, spawnPos, Quaternion.identity);
        
        BirdoEgg eggScript = egg.GetComponent<BirdoEgg>();
        if (eggScript != null)
        {
            Vector2 direction = (targetPos - spawnPos).normalized;
            eggScript.Initialize(direction, eggSpeed);
        }
        else
        {
            // Basic movement if no script
            Rigidbody2D eggRb = egg.GetComponent<Rigidbody2D>();
            if (eggRb != null)
            {
                Vector2 direction = (targetPos - spawnPos).normalized;
                eggRb.velocity = direction * eggSpeed;
            }
        }
        
        // Play sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
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
    
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Hell mouth shoot range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hellMouthShootRange);
        
        // Ground check
        Gizmos.color = Color.green;
        Vector3 groundCheckPos = Application.isPlaying && groundCheck != null 
            ? groundCheck.position 
            : transform.position + Vector3.down * 0.5f;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector3.down * groundCheckDistance);
    }
}

