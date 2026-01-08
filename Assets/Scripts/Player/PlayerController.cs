using UnityEngine;

/// <summary>
/// Polished 2D platformer controller with tight, responsive controls.
/// Based on proven game feel principles from Celeste, Hollow Knight, etc.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airAcceleration = 40f;
    [SerializeField] private float airDeceleration = 20f;
    [Tooltip("Gives snappier direction changes")]
    [SerializeField] private float turningBoost = 2f;
    
    [Header("Ice Physics")]
    [SerializeField] private bool alwaysOnIce = true;
    [SerializeField] private float iceAcceleration = 15f;
    [SerializeField] private float iceDeceleration = 5f;
    [SerializeField] private float iceTurningBoost = 0.5f;
    [Tooltip("Max speed when sliding on ice")]
    [SerializeField] private float iceMaxSpeed = 12f;
    [Tooltip("Animation speeds up when trying to move while sliding")]
    [SerializeField] private float slideAnimSpeedMultiplier = 3f;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 14f;
    [Tooltip("Time after leaving ground you can still jump")]
    [SerializeField] private float coyoteTime = 0.12f;
    [Tooltip("Buffer jump input before landing")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [Tooltip("How much gravity increases when falling")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [Tooltip("Gravity when releasing jump early (variable jump height)")]
    [SerializeField] private float lowJumpMultiplier = 4f;
    [Tooltip("Maximum fall speed")]
    [SerializeField] private float maxFallSpeed = 20f;
    [Tooltip("Brief hang time at jump apex")]
    [SerializeField] private float apexThreshold = 1.5f;
    [SerializeField] private float apexBonus = 1.5f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Wall Interaction (Optional)")]
    [SerializeField] private bool enableWallSlide = false;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;
    
    [Header("Slope Handling")]
    [SerializeField] private bool enableSlopeStick = true;
    [SerializeField] private float slopeCheckDistance = 0.5f;
    [SerializeField] private float maxSlopeAngle = 50f;
    [Tooltip("Downward force applied on slopes to prevent bouncing")]
    [SerializeField] private float slopeStickForce = 10f;
    [Tooltip("Speed multiplier when going up slopes")]
    [SerializeField] private float slopeUpSpeedMultiplier = 0.8f;
    [Tooltip("Speed multiplier when going down slopes")]
    [SerializeField] private float slopeDownSpeedMultiplier = 1.2f;
    
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AnimatedSprite animatedSprite;
    
    // Input state
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    
    // Physics state
    private bool isGrounded;
    private bool wasGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int facingDirection = 1;
    
    // Apex detection for floatier jump peak
    private bool isAtApex;
    
    // Slope detection
    private bool isOnSlope;
    private float slopeAngle;
    private Vector2 slopeNormal;
    private Vector2 slopeParallel;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animatedSprite = GetComponent<AnimatedSprite>();
    }
    
    private void Update()
    {
        GatherInput();
        CheckGrounded();
        CheckSlope();
        CheckWall();
        HandleTimers();
        HandleJump();
        HandleAnimation();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravityModifiers();
        HandleWallSlide();
        HandleSlopeStick();
    }
    
    private void GatherInput()
    {
        // Keyboard/Controller input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
            jumpBufferCounter = jumpBufferTime;
        }
        
        jumpHeld = Input.GetButton("Jump");
        
        // Mobile touch input (combines with keyboard)
        if (MobileControls.IsMobile)
        {
            // Add mobile horizontal input
            if (Mathf.Abs(MobileControls.HorizontalInput) > 0.1f)
            {
                horizontalInput = MobileControls.HorizontalInput;
            }
            
            // Mobile jump
            if (MobileControls.JumpPressed)
            {
                jumpPressed = true;
                jumpBufferCounter = jumpBufferTime;
            }
            
            if (MobileControls.JumpHeld)
            {
                jumpHeld = true;
            }
        }
    }
    
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // Fallback: raycast down
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        }
        
        // Just landed
        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }
    }
    
    private void CheckWall()
    {
        if (!enableWallSlide || wallCheck == null) return;
        
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, wallCheckDistance, groundLayer);
        isWallSliding = isTouchingWall && !isGrounded && rb.velocity.y < 0;
    }
    
    private void CheckSlope()
    {
        if (!enableSlopeStick || !isGrounded)
        {
            isOnSlope = false;
            slopeAngle = 0f;
            return;
        }
        
        // Cast ray downward to detect slope
        Vector2 origin = groundCheck != null ? groundCheck.position : transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, slopeCheckDistance, groundLayer);
        
        if (hit)
        {
            slopeNormal = hit.normal;
            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            
            // Calculate movement direction parallel to slope
            slopeParallel = Vector2.Perpendicular(slopeNormal).normalized;
            
            // Make sure parallel points in positive X direction
            if (slopeParallel.x < 0)
            {
                slopeParallel = -slopeParallel;
            }
            
            isOnSlope = slopeAngle > 0.5f && slopeAngle < maxSlopeAngle;
        }
        else
        {
            isOnSlope = false;
            slopeAngle = 0f;
        }
    }
    
    private void HandleSlopeStick()
    {
        if (!enableSlopeStick || !isGrounded) return;
        
        // Apply downward force to stick to slopes and prevent bouncing
        if (isOnSlope)
        {
            // Only stick when moving or standing still (not jumping)
            if (rb.velocity.y <= 0.1f)
            {
                // Apply force perpendicular to slope (into the ground)
                rb.AddForce(-slopeNormal * slopeStickForce, ForceMode2D.Force);
            }
        }
        
        // If standing still on slope with no input, kill velocity to prevent sliding
        if (isOnSlope && Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(rb.velocity.x) < 0.5f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    private void HandleTimers()
    {
        // Coyote time - grace period after leaving ground
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // Jump buffer countdown
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // Apex detection for floatier peak
        isAtApex = !isGrounded && Mathf.Abs(rb.velocity.y) < apexThreshold;
    }
    
    private void HandleJump()
    {
        // Can jump if: on ground OR within coyote time, AND have buffered jump input
        bool canJump = (isGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0;
        
        if (canJump)
        {
            ExecuteJump();
        }
        
        jumpPressed = false;
    }
    
    private void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
        isGrounded = false;
    }
    
    private void HandleMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        
        // Add apex bonus for more air control at jump peak
        if (isAtApex)
        {
            targetSpeed *= apexBonus;
        }
        
        // Slope speed adjustment
        if (isOnSlope && isGrounded)
        {
            // Going uphill (moving against gravity component)
            bool goingUphill = (horizontalInput > 0 && slopeParallel.y > 0) || 
                               (horizontalInput < 0 && slopeParallel.y < 0);
            
            if (goingUphill)
            {
                targetSpeed *= slopeUpSpeedMultiplier;
            }
            else if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Going downhill
                targetSpeed *= slopeDownSpeedMultiplier;
            }
        }
        
        float speedDiff = targetSpeed - rb.velocity.x;
        
        // Choose acceleration based on grounded state and ice
        float accelBase, decelBase, turnBoost, maxSpd;
        
        if (!isGrounded)
        {
            accelBase = airAcceleration;
            decelBase = airDeceleration;
            turnBoost = turningBoost;
            maxSpd = moveSpeed;
        }
        else if (alwaysOnIce)
        {
            // Ice physics - slippery!
            accelBase = iceAcceleration;
            decelBase = iceDeceleration;
            turnBoost = iceTurningBoost;
            maxSpd = iceMaxSpeed;
        }
        else
        {
            accelBase = acceleration;
            decelBase = deceleration;
            turnBoost = turningBoost;
            maxSpd = moveSpeed;
        }
        
        // Are we accelerating or decelerating?
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            // Moving - check if turning around
            bool isTurning = (rb.velocity.x > 0 && horizontalInput < 0) || (rb.velocity.x < 0 && horizontalInput > 0);
            accelRate = isTurning ? accelBase * turnBoost : accelBase;
        }
        else
        {
            // Stopping
            accelRate = decelBase;
        }
        
        // Calculate movement force
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        
        // Apply movement
        rb.velocity = new Vector2(rb.velocity.x + movement, rb.velocity.y);
        
        // Clamp to max speed
        if (Mathf.Abs(rb.velocity.x) > maxSpd)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpd, rb.velocity.y);
        }
        
        // Update facing direction
        if (horizontalInput != 0)
        {
            facingDirection = horizontalInput > 0 ? 1 : -1;
        }
    }
    
    private void ApplyGravityModifiers()
    {
        // Clamp fall speed
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
        
        // Skip if grounded
        if (isGrounded) return;
        
        // Falling - increase gravity for snappier descent
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Rising but jump released - cut jump short (variable jump height)
        else if (rb.velocity.y > 0 && !jumpHeld)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        // At apex - reduce gravity slightly for hang time
        else if (isAtApex)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * 0.5f * Time.fixedDeltaTime;
        }
    }
    
    private void HandleWallSlide()
    {
        if (!enableWallSlide) return;
        
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
    }
    
    private void HandleAnimation()
    {
        // Flip sprite based on facing direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0;
        }
        
        // Control AnimatedSprite based on movement
        if (animatedSprite != null)
        {
            bool hasInput = Mathf.Abs(horizontalInput) > 0.1f;
            bool isMoving = Mathf.Abs(rb.velocity.x) > 0.5f;
            
            // Animate when moving OR when trying to move (sliding)
            if (hasInput || isMoving)
            {
                animatedSprite.enabled = true;
                
                // Check if sliding (trying to move but velocity doesn't match intent)
                bool isTryingToTurn = hasInput && 
                    ((horizontalInput > 0 && rb.velocity.x < -0.5f) || 
                     (horizontalInput < 0 && rb.velocity.x > 0.5f));
                     
                bool isTryingToAccelerate = hasInput && 
                    Mathf.Abs(rb.velocity.x) < Mathf.Abs(horizontalInput * moveSpeed * 0.8f);
                
                // Frantic feet when sliding!
                if (isGrounded && alwaysOnIce && (isTryingToTurn || isTryingToAccelerate))
                {
                    animatedSprite.SetSpeedMultiplier(slideAnimSpeedMultiplier);
                }
                else
                {
                    animatedSprite.SetSpeedMultiplier(1f);
                }
            }
            else
            {
                animatedSprite.enabled = false;
                animatedSprite.SetSpeedMultiplier(1f);
            }
        }
        
        // Update Animator if using one
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
    }
    
    private void OnLand()
    {
        // Hook for landing effects (particles, sound, etc.)
    }
    
    private void OnDrawGizmosSelected()
    {
        // Ground check visualization
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Slope check ray
            if (enableSlopeStick)
            {
                Gizmos.color = isOnSlope ? Color.yellow : Color.gray;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * slopeCheckDistance);
                
                // Draw slope normal
                if (isOnSlope)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3)slopeNormal * 0.5f);
                }
            }
        }
        
        // Wall check visualization
        if (enableWallSlide && wallCheck != null)
        {
            Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * facingDirection * wallCheckDistance);
        }
    }
}
