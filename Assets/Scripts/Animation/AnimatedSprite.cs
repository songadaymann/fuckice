using UnityEngine;

/// <summary>
/// Simple sprite animation for objects without Animator.
/// Use for animated tiles like coins, keys, flags.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
{
    [Header("Animation Frames")]
    [SerializeField] private Sprite[] frames;
    
    [Header("Settings")]
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private bool loop = true;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    private float speedMultiplier = 1f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        
        timer += Time.deltaTime;
        
        float effectiveFPS = framesPerSecond * speedMultiplier;
        
        if (timer >= 1f / effectiveFPS)
        {
            timer = 0f;
            currentFrame++;
            
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                }
            }
            
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
    
    /// <summary>
    /// Set animation speed multiplier (1 = normal, 2 = double speed, etc.)
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.1f, multiplier);
    }
    
    public void SetFrames(Sprite[] newFrames)
    {
        frames = newFrames;
        currentFrame = 0;
        timer = 0f;
    }
    
    public void Play()
    {
        enabled = true;
        currentFrame = 0;
        timer = 0f;
    }
    
    public void Stop()
    {
        enabled = false;
    }
}

