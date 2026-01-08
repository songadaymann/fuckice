using UnityEngine;

/// <summary>
/// Base collectible script for coins, keys, etc.
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;
    
    [Header("Animation")]
    [SerializeField] private bool bobUpAndDown = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    
    private Vector3 startPosition;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        if (bobUpAndDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Collect();
        }
    }
    
    protected virtual void Collect()
    {
        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Spawn effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Add score (you can expand this with a GameManager)
        Debug.Log($"Collected! +{scoreValue} points");
        
        // Destroy the collectible
        Destroy(gameObject);
    }
}

