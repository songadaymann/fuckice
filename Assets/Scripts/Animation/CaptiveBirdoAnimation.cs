using UnityEngine;

/// <summary>
/// Animates captive Birdo inside the cage - walks back and forth with sprite animation.
/// </summary>
public class CaptiveBirdoAnimation : MonoBehaviour
{
    [Header("Walking")]
    [SerializeField] private float walkSpeed = 0.5f;
    [SerializeField] private float walkDistance = 0.3f; // How far left/right to walk
    
    [Header("Animation")]
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private float animationFPS = 8f;
    
    // State
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float walkTimer;
    private float animTimer;
    private int currentFrame;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.localPosition;
    }
    
    private void Update()
    {
        // Walk back and forth
        walkTimer += Time.deltaTime * walkSpeed;
        float xOffset = Mathf.Sin(walkTimer * Mathf.PI) * walkDistance;
        transform.localPosition = new Vector3(
            startPosition.x + xOffset,
            startPosition.y,
            startPosition.z
        );
        
        // Flip sprite based on direction
        float previousX = Mathf.Sin((walkTimer - Time.deltaTime * walkSpeed) * Mathf.PI);
        float currentX = Mathf.Sin(walkTimer * Mathf.PI);
        
        if (currentX > previousX)
        {
            // Moving right
            spriteRenderer.flipX = false;
        }
        else if (currentX < previousX)
        {
            // Moving left
            spriteRenderer.flipX = true;
        }
        
        // Animate sprites
        if (walkSprites != null && walkSprites.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animationFPS)
            {
                animTimer = 0;
                currentFrame = (currentFrame + 1) % walkSprites.Length;
                spriteRenderer.sprite = walkSprites[currentFrame];
            }
        }
    }
}

