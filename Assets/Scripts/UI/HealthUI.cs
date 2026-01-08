using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Displays player health as Zelda-style hearts.
/// Supports full, half, and empty heart states.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;
    
    [Header("Layout")]
    [SerializeField] private float heartSize = 32f;
    [SerializeField] private float heartSpacing = 4f;
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f); // From top-left
    
    [Header("Animation")]
    [SerializeField] private float damageShakeAmount = 5f;
    [SerializeField] private float damageShakeDuration = 0.3f;
    [SerializeField] private float healPulseScale = 1.3f;
    [SerializeField] private float healPulseDuration = 0.2f;
    
    private List<Image> heartImages = new List<Image>();
    private PlayerHealth playerHealth;
    private RectTransform containerRect;
    private int lastDisplayedHealth = -1;
    
    private void Start()
    {
        // Find player health
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnHealthChanged;
                CreateHearts(playerHealth.MaxHealth);
                UpdateHearts(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogWarning("HealthUI: No PlayerHealth found! Make sure player has 'Player' tag.");
        }
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }
    
    private void CreateHearts(int maxHealth)
    {
        // Clear existing hearts
        foreach (var img in heartImages)
        {
            if (img != null) Destroy(img.gameObject);
        }
        heartImages.Clear();
        
        // Create container if needed
        if (containerRect == null)
        {
            containerRect = GetComponent<RectTransform>();
            if (containerRect == null)
            {
                containerRect = gameObject.AddComponent<RectTransform>();
            }
        }
        
        // Calculate how many heart icons we need (each heart = 2 HP for half-heart support)
        // Or if you prefer 1 heart = 1 HP, we just show that many hearts
        int heartCount = maxHealth; // 1 heart per HP
        
        for (int i = 0; i < heartCount; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}");
            heartObj.transform.SetParent(transform, false);
            
            Image heartImg = heartObj.AddComponent<Image>();
            heartImg.sprite = heartFull;
            heartImg.preserveAspect = true;
            
            RectTransform rect = heartObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(heartSize, heartSize);
            rect.anchorMin = new Vector2(0, 1); // Top-left
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(
                offset.x + i * (heartSize + heartSpacing),
                offset.y
            );
            
            heartImages.Add(heartImg);
        }
    }
    
    private void OnHealthChanged(int current, int max)
    {
        // Check if we need to recreate hearts (max changed)
        if (heartImages.Count != max)
        {
            CreateHearts(max);
        }
        
        // Animate based on whether we took damage or healed
        if (lastDisplayedHealth > 0)
        {
            if (current < lastDisplayedHealth)
            {
                // Took damage - shake!
                StartCoroutine(DamageShake());
            }
            else if (current > lastDisplayedHealth)
            {
                // Healed - pulse the healed heart
                int healedIndex = current - 1;
                if (healedIndex >= 0 && healedIndex < heartImages.Count)
                {
                    StartCoroutine(HealPulse(heartImages[healedIndex]));
                }
            }
        }
        
        UpdateHearts(current, max);
        lastDisplayedHealth = current;
    }
    
    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < current)
            {
                heartImages[i].sprite = heartFull;
            }
            else
            {
                heartImages[i].sprite = heartEmpty;
            }
            
            // If we wanted half hearts, we'd do:
            // if (i < current / 2) full
            // else if (i == current / 2 && current % 2 == 1) half
            // else empty
        }
    }
    
    private System.Collections.IEnumerator DamageShake()
    {
        float elapsed = 0f;
        List<Vector2> originalPositions = new List<Vector2>();
        
        foreach (var img in heartImages)
        {
            originalPositions.Add(img.rectTransform.anchoredPosition);
        }
        
        while (elapsed < damageShakeDuration)
        {
            elapsed += Time.deltaTime;
            float shakeX = Random.Range(-damageShakeAmount, damageShakeAmount);
            float shakeY = Random.Range(-damageShakeAmount, damageShakeAmount);
            
            for (int i = 0; i < heartImages.Count; i++)
            {
                heartImages[i].rectTransform.anchoredPosition = originalPositions[i] + new Vector2(shakeX, shakeY);
            }
            
            yield return null;
        }
        
        // Reset positions
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].rectTransform.anchoredPosition = originalPositions[i];
        }
    }
    
    private System.Collections.IEnumerator HealPulse(Image heart)
    {
        Vector3 originalScale = heart.rectTransform.localScale;
        Vector3 targetScale = originalScale * healPulseScale;
        float elapsed = 0f;
        float halfDuration = healPulseDuration / 2f;
        
        // Scale up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            heart.rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            heart.rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            yield return null;
        }
        
        heart.rectTransform.localScale = originalScale;
    }
}

