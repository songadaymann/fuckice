using UnityEngine;
using System.Collections;

/// <summary>
/// Hell Mouth spawner - spawns ICE agents until closed by the player.
/// Attach to a hell mouth sprite (Trump, Miller, Kristi).
/// </summary>
public class HellMouth : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string hellMouthName = "Hell Mouth";
    
    [Header("Spawning")]
    [SerializeField] private GameObject iceAgentPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private int maxActiveAgentsPerMouth = 10; // Per hell mouth limit
    [SerializeField] private float spawnAnimationDuration = 0.5f;
    
    [Header("Global Agent Limit")]
    [SerializeField] private static int globalMaxAgents = 50; // Total agents across all hell mouths
    private static System.Collections.Generic.List<GameObject> allActiveAgents = new System.Collections.Generic.List<GameObject>();
    
    [Header("Health / Closing")]
    [SerializeField] private int maxHealth = 3; // 3 eggs to close!
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float damageFlashDuration = 0.1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    [Header("Explosion Animation")]
    [SerializeField] private Sprite[] explosionSprites;
    [SerializeField] private float explosionFPS = 12f;
    [SerializeField] private float explosionScale = 2f;
    [SerializeField] private float closeDuration = 1f;
    [SerializeField] private bool destroyOnClose = true;
    
    [Header("Dialogue")]
    [SerializeField] private string onFirstCloseMessage = "You destroyed the ICE HELL MOUTH! Only two to go!";
    [SerializeField] private string onSecondCloseMessage = "Another one down! One more to go!";
    [SerializeField] private string onFinalCloseMessage = "You win! Abolish ICE! Go find a local organization working to protect immigrants and start organizing!";
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip explosionSound;
    
    // Static tracking for all hell mouths
    private static int totalHellMouths = 0;
    private static int closedHellMouths = 0;
    
    // State
    private int currentHealth;
    private bool isClosed = false;
    private bool isInvincible = false;
    private float spawnTimer;
    private int activeAgentCount = 0;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Color originalColor;
    
    // Events (for UI, game manager, etc.)
    public System.Action<HellMouth> OnClosed;
    public System.Action<HellMouth, int> OnDamaged;
    public System.Action<HellMouth, GameObject> OnAgentSpawned;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        originalPosition = transform.position;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    private void Start()
    {
        currentHealth = maxHealth;
        spawnTimer = initialDelay;
        
        // Track total hell mouths for progress messages
        totalHellMouths++;
        
        // Create spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("SpawnPoint");
            sp.transform.parent = transform;
            sp.transform.localPosition = new Vector3(0, -0.5f, 0); // Spawn from mouth
            spawnPoint = sp.transform;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up static counter
        if (!isClosed)
        {
            totalHellMouths--;
        }
    }
    
    private void Update()
    {
        if (isClosed) return;
        
        // Clean up null references from destroyed agents
        allActiveAgents.RemoveAll(a => a == null);
        
        // Spawning logic - always spawn if under per-mouth limit
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && activeAgentCount < maxActiveAgentsPerMouth)
        {
            StartCoroutine(SpawnAgent());
            spawnTimer = spawnInterval;
        }
    }
    
    private IEnumerator SpawnAgent()
    {
        if (iceAgentPrefab == null) yield break;
        
        // Pre-spawn animation (mouth opens wider, shakes, etc.)
        yield return StartCoroutine(SpawnAnimation());
        
        // If at global max, destroy the oldest agent to make room
        if (allActiveAgents.Count >= globalMaxAgents)
        {
            // Find and destroy the oldest living agent
            for (int i = 0; i < allActiveAgents.Count; i++)
            {
                if (allActiveAgents[i] != null)
                {
                    GameObject oldestAgent = allActiveAgents[i];
                    allActiveAgents.RemoveAt(i);
                    Destroy(oldestAgent);
                    break;
                }
            }
        }
        
        // Spawn the agent
        GameObject agent = Instantiate(iceAgentPrefab, spawnPoint.position, Quaternion.identity);
        activeAgentCount++;
        
        // Add to global tracking list
        allActiveAgents.Add(agent);
        
        // Make agent ignore collision with this Hell Mouth!
        Collider2D agentCollider = agent.GetComponent<Collider2D>();
        Collider2D hellMouthCollider = GetComponent<Collider2D>();
        if (agentCollider != null && hellMouthCollider != null)
        {
            Physics2D.IgnoreCollision(agentCollider, hellMouthCollider);
        }
        
        // Track when agent dies
        IceAgentController controller = agent.GetComponent<IceAgentController>();
        if (controller != null)
        {
            // We'll need to track agent death - add a callback or use a wrapper
            StartCoroutine(TrackAgent(agent));
        }
        
        // Play spawn sound
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        OnAgentSpawned?.Invoke(this, agent);
    }
    
    private IEnumerator SpawnAnimation()
    {
        // Shake effect
        float elapsed = 0;
        while (elapsed < spawnAnimationDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeIntensity, shakeIntensity);
            float y = originalPosition.y + Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
    
    private IEnumerator TrackAgent(GameObject agent)
    {
        while (agent != null)
        {
            yield return null;
        }
        activeAgentCount--;
    }
    
    /// <summary>
    /// Deal damage to the hell mouth. Call this from whatever closing mechanic you implement.
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        if (isClosed || isInvincible) return;
        
        currentHealth -= damage;
        OnDamaged?.Invoke(this, currentHealth);
        
        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        StartCoroutine(DamageShake());
        StartCoroutine(InvincibilityFrames());
        
        // Check if closed
        if (currentHealth <= 0)
        {
            Close();
        }
    }
    
    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }
    
    private IEnumerator DamageShake()
    {
        float elapsed = 0;
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeIntensity * 2, shakeIntensity * 2);
            float y = originalPosition.y + Random.Range(-shakeIntensity * 2, shakeIntensity * 2);
            transform.position = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
    
    /// <summary>
    /// Close the hell mouth (victory!)
    /// </summary>
    public void Close()
    {
        if (isClosed) return;
        
        Debug.Log($"🔥 {hellMouthName} HELL MOUTH CLOSING! 🔥");
        isClosed = true;
        StartCoroutine(CloseAnimation());
    }
    
    private IEnumerator CloseAnimation()
    {
        // Play explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        else if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        
        // Play explosion animation if we have sprites
        if (explosionSprites != null && explosionSprites.Length > 0)
        {
            // Scale up for explosion
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * explosionScale;
            
            // Play through explosion frames
            float frameTime = 1f / explosionFPS;
            foreach (Sprite frame in explosionSprites)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(frameTime);
            }
        }
        else
        {
            // Fallback: Shrink and fade animation
            float elapsed = 0;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < closeDuration)
            {
                float t = elapsed / closeDuration;
                
                // Shrink
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                // Fade
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f - t;
                    spriteRenderer.color = c;
                }
                
                // Spin
                transform.Rotate(0, 0, 360 * Time.deltaTime);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Track closed hell mouths and show appropriate dialogue
        closedHellMouths++;
        int remaining = totalHellMouths - closedHellMouths;
        
        string message = "";
        if (remaining == 2)
        {
            message = onFirstCloseMessage;
        }
        else if (remaining == 1)
        {
            message = onSecondCloseMessage;
        }
        else if (remaining == 0)
        {
            message = onFinalCloseMessage;
        }
        
        // Show dialogue - longer duration for the final victory message
        if (!string.IsNullOrEmpty(message))
        {
            float duration = (remaining == 0) ? 8f : 4f; // Final message shows longer
            GameDialogue.Show(message, duration);
        }
        
        OnClosed?.Invoke(this);
        
        if (destroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Reset static counters (call when restarting level)
    /// </summary>
    public static void ResetCounters()
    {
        totalHellMouths = 0;
        closedHellMouths = 0;
        allActiveAgents.Clear();
    }
    
    /// <summary>
    /// Get current number of active agents globally
    /// </summary>
    public static int ActiveAgentCount => allActiveAgents.Count;
    
    /// <summary>
    /// Set the global max agents limit
    /// </summary>
    public static void SetGlobalMaxAgents(int max)
    {
        globalMaxAgents = max;
    }
    
    /// <summary>
    /// Get remaining hell mouths
    /// </summary>
    public static int GetRemainingCount()
    {
        return totalHellMouths - closedHellMouths;
    }
    
    /// <summary>
    /// Get number of closed hell mouths
    /// </summary>
    public static int ClosedCount => closedHellMouths;
    
    /// <summary>
    /// Get total number of hell mouths
    /// </summary>
    public static int TotalCount => totalHellMouths;
    
    /// <summary>
    /// Get current health percentage (for UI)
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Get current health
    /// </summary>
    public int GetCurrentHealth() => currentHealth;
    
    /// <summary>
    /// Get max health
    /// </summary>
    public int GetMaxHealth() => maxHealth;
    
    /// <summary>
    /// Check if this hell mouth is closed
    /// </summary>
    public bool IsClosed() => isClosed;
    
    /// <summary>
    /// Get the name of this hell mouth
    /// </summary>
    public string GetName() => hellMouthName;
    
    /// <summary>
    /// Handle eggs entering our trigger collider
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isClosed) return;
        
        // Check if it's an egg
        BirdoEgg egg = other.GetComponent<BirdoEgg>();
        if (egg != null)
        {
            Debug.Log($"{hellMouthName} hit by egg! Health: {currentHealth - 1}");
            TakeDamage(1);
        }
    }
    
    /// <summary>
    /// Handle eggs colliding with us (if not using trigger)
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isClosed) return;
        
        // Check if it's an egg
        BirdoEgg egg = collision.gameObject.GetComponent<BirdoEgg>();
        if (egg != null)
        {
            Debug.Log($"{hellMouthName} hit by egg! Health: {currentHealth - 1}");
            TakeDamage(1);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Spawn point
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
        else
        {
            // Default spawn position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.5f, 0.3f);
        }
    }
}

