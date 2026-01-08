using UnityEngine;
using System.Collections;

/// <summary>
/// Player health and damage handling with regeneration and checkpoint system.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1.5f;
    
    [Header("Regeneration")]
    [SerializeField] private bool enableRegen = true;
    [SerializeField] private float regenDelay = 5f; // Time after damage before regen starts
    [SerializeField] private float regenInterval = 2f; // Time between each heart regen
    
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float flashInterval = 0.1f;
    
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1f;
    
    private int currentHealth;
    private bool isInvincible = false;
    private bool isKnockedBack = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerController controller;
    
    // Regeneration
    private float lastDamageTime;
    private Coroutine regenCoroutine;
    
    // Checkpoint system - static so it persists across scene reloads
    private static Vector3? lastCheckpoint = null;
    private static int hellMouthsClosedAtCheckpoint = 0;
    private static bool hasCheckpoint = false;
    private static bool hadBirdoAtCheckpoint = false;
    private static bool hadKeyAtCheckpoint = false;
    
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDeath;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        controller = GetComponent<PlayerController>();
        currentHealth = maxHealth;
        lastDamageTime = -regenDelay; // Allow immediate regen at start if needed
    }
    
    private void Start()
    {
        // If we have a saved checkpoint, teleport there
        if (hasCheckpoint && lastCheckpoint.HasValue)
        {
            transform.position = lastCheckpoint.Value;
            Debug.Log($"Respawned at checkpoint: {lastCheckpoint.Value}");
            
            // Restore Birdo if we had her at the checkpoint
            if (hadBirdoAtCheckpoint)
            {
                RestoreBirdo();
            }
            
            // Restore Key if we had it (and Birdo wasn't freed yet)
            if (hadKeyAtCheckpoint && !hadBirdoAtCheckpoint)
            {
                RestoreKey();
            }
        }
        
        // Set initial checkpoint at spawn position
        if (!hasCheckpoint)
        {
            SetCheckpoint(transform.position);
        }
    }
    
    private void RestoreBirdo()
    {
        // Check if Birdo already exists (shouldn't, but just in case)
        if (FindObjectOfType<BirdoCompanion>() != null)
        {
            Debug.Log("Birdo already exists, skipping restore");
            return;
        }
        
        // Try to find Birdo prefab
        GameObject birdoPrefab = Resources.Load<GameObject>("Birdo");
        
        // If not in Resources, try to find it via the cage
        if (birdoPrefab == null)
        {
            BirdoCage cage = FindObjectOfType<BirdoCage>();
            if (cage != null)
            {
                // Get prefab from cage using reflection or serialized field
                var field = typeof(BirdoCage).GetField("birdoPrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    birdoPrefab = field.GetValue(cage) as GameObject;
                }
            }
        }
        
        // Spawn Birdo near player
        Vector3 spawnPos = transform.position + Vector3.left * 1.5f;
        
        if (birdoPrefab != null)
        {
            Instantiate(birdoPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Birdo restored from checkpoint!");
        }
        else
        {
            // Fallback: Create basic Birdo
            GameObject birdo = new GameObject("Birdo_Restored");
            birdo.transform.position = spawnPos;
            birdo.AddComponent<SpriteRenderer>();
            Rigidbody2D rb = birdo.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            birdo.AddComponent<BoxCollider2D>();
            birdo.AddComponent<BirdoCompanion>();
            Debug.Log("Birdo restored (basic fallback) from checkpoint!");
        }
        
        // Also destroy the cage and captive Birdo if they exist
        BirdoCage existingCage = FindObjectOfType<BirdoCage>();
        if (existingCage != null)
        {
            // Open it so it looks correct
            existingCage.OpenCage();
        }
    }
    
    private void RestoreKey()
    {
        // Find the key and make it follow the player again
        FollowingKey key = FindObjectOfType<FollowingKey>();
        if (key != null)
        {
            // Teleport key to player
            key.transform.position = transform.position + Vector3.up * 1f;
            // The key should auto-collect when near player
        }
    }
    
    private void Update()
    {
        // Handle regeneration
        if (enableRegen && currentHealth < maxHealth && currentHealth > 0)
        {
            if (Time.time - lastDamageTime >= regenDelay && regenCoroutine == null)
            {
                regenCoroutine = StartCoroutine(RegenRoutine());
            }
        }
    }
    
    private IEnumerator RegenRoutine()
    {
        while (currentHealth < maxHealth && currentHealth > 0)
        {
            yield return new WaitForSeconds(regenInterval);
            
            // Check if we took damage during the wait
            if (Time.time - lastDamageTime < regenDelay)
            {
                break;
            }
            
            if (currentHealth < maxHealth && currentHealth > 0)
            {
                Heal(1);
                Debug.Log($"Health regenerated: {currentHealth}/{maxHealth}");
            }
        }
        
        regenCoroutine = null;
    }
    
    /// <summary>
    /// Set a checkpoint for respawning. Call this when player reaches safe areas.
    /// </summary>
    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpoint = position;
        hasCheckpoint = true;
        
        // Save current hell mouth progress
        hellMouthsClosedAtCheckpoint = HellMouth.ClosedCount;
        
        // Save Birdo state - check if Birdo companion exists in scene
        hadBirdoAtCheckpoint = FindObjectOfType<BirdoCompanion>() != null;
        
        // Save Key state - check if player has the key following them
        FollowingKey key = FindObjectOfType<FollowingKey>();
        hadKeyAtCheckpoint = key != null && key.IsCollected;
        
        Debug.Log($"Checkpoint set at {position}, HellMouths: {hellMouthsClosedAtCheckpoint}, HasBirdo: {hadBirdoAtCheckpoint}, HasKey: {hadKeyAtCheckpoint}");
    }
    
    /// <summary>
    /// Clear all checkpoints (for starting a new game)
    /// </summary>
    public static void ClearCheckpoints()
    {
        lastCheckpoint = null;
        hasCheckpoint = false;
        hellMouthsClosedAtCheckpoint = 0;
        hadBirdoAtCheckpoint = false;
        hadKeyAtCheckpoint = false;
    }
    
    public void TakeDamage(int damage, Vector2? knockbackDirection = null)
    {
        if (isInvincible || currentHealth <= 0) return;
        
        currentHealth -= damage;
        lastDamageTime = Time.time; // Reset regen timer
        
        // Stop any ongoing regen
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
            
            // Apply knockback
            if (knockbackDirection.HasValue)
            {
                StartCoroutine(KnockbackRoutine(knockbackDirection.Value));
            }
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        // Flash effect
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    
    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockedBack = true;
        
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        rb.velocity = direction.normalized * knockbackForce;
        
        yield return new WaitForSeconds(knockbackDuration);
        
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        isKnockedBack = false;
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        OnDeath?.Invoke();
        
        // Simple respawn - reload scene
        // You can customize this
        StartCoroutine(DeathRoutine());
    }
    
    private IEnumerator DeathRoutine()
    {
        // Disable controls
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Simple death effect - shrink
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn at checkpoint (scene reload will use the static checkpoint data)
        // The checkpoint and hell mouth progress are preserved in static variables
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

