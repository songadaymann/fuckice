using UnityEngine;
using System.Collections;

/// <summary>
/// Cage that holds Birdo. Opens when the key arrives.
/// </summary>
public class BirdoCage : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    
    [Header("Birdo")]
    [SerializeField] private GameObject birdoPrefab;
    [SerializeField] private Transform birdoSpawnPoint;
    [SerializeField] private GameObject captiveBirdoVisual; // Birdo shown inside cage
    
    [Header("Opening")]
    [SerializeField] private float keyDetectionRadius = 2f;
    [SerializeField] private float openAnimationDuration = 0.5f;
    
    [Header("Dialogue")]
    [SerializeField] private string preOpenDialogue = "Birdo: Help! They locked me up! Find the key!";
    [SerializeField] private string postOpenDialogue = "Birdo will follow you now! She'll shoot eggs at ICE agents and Hell Mouths!";
    
    // State
    private bool isOpen = false;
    private SpriteRenderer spriteRenderer;
    
    // Events
    public System.Action<GameObject> OnBirdoFreed;
    public System.Action<string> OnDialogue;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (closedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
        
        // Create spawn point if not assigned
        if (birdoSpawnPoint == null)
        {
            birdoSpawnPoint = transform;
        }
    }
    
    private void Update()
    {
        if (isOpen) return;
        
        // Check for nearby key
        FollowingKey key = FindNearbyKey();
        if (key != null && key.IsCollected)
        {
            OpenCage(key);
        }
    }
    
    private FollowingKey FindNearbyKey()
    {
        // Find all keys and check distance
        FollowingKey[] keys = FindObjectsOfType<FollowingKey>();
        foreach (var key in keys)
        {
            float dist = Vector2.Distance(transform.position, key.transform.position);
            if (dist < keyDetectionRadius)
            {
                return key;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Open the cage and free Birdo
    /// </summary>
    public void OpenCage(FollowingKey key = null)
    {
        if (isOpen) return;
        
        isOpen = true;
        StartCoroutine(OpenAnimation(key));
    }
    
    private IEnumerator OpenAnimation(FollowingKey key)
    {
        // Use the key
        if (key != null)
        {
            key.UseKey();
        }
        
        // Shake the cage
        Vector3 originalPos = transform.position;
        float elapsed = 0;
        while (elapsed < openAnimationDuration)
        {
            float shake = Mathf.Sin(elapsed * 50) * 0.1f;
            transform.position = originalPos + new Vector3(shake, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
        
        // Switch to open sprite
        if (openSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = openSprite;
        }
        
        // Hide captive Birdo visual
        if (captiveBirdoVisual != null)
        {
            captiveBirdoVisual.SetActive(false);
        }
        
        // Spawn the real Birdo companion
        GameObject birdo = null;
        if (birdoPrefab != null)
        {
            birdo = Instantiate(birdoPrefab, birdoSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            // Create a basic Birdo if no prefab
            birdo = CreateBasicBirdo();
        }
        
        // Show dialogue - use GameDialogue directly
        GameDialogue.Show(postOpenDialogue, 4f);
        OnDialogue?.Invoke(postOpenDialogue);
        
        // Notify listeners
        OnBirdoFreed?.Invoke(birdo);
        
        Debug.Log("Birdo is free!");
    }
    
    private GameObject CreateBasicBirdo()
    {
        GameObject birdo = new GameObject("Birdo");
        birdo.transform.position = birdoSpawnPoint.position;
        birdo.AddComponent<SpriteRenderer>();
        birdo.AddComponent<Rigidbody2D>();
        birdo.AddComponent<BoxCollider2D>();
        birdo.AddComponent<BirdoCompanion>();
        return birdo;
    }
    
    /// <summary>
    /// Show initial dialogue when player approaches
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            OnDialogue?.Invoke(preOpenDialogue);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Key detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, keyDetectionRadius);
        
        // Spawn point
        if (birdoSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(birdoSpawnPoint.position, 0.3f);
        }
    }
}

