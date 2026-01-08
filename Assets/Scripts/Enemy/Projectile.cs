using UnityEngine;

/// <summary>
/// Projectile that damages the player on contact.
/// Features spinning animation, trail effect, and muzzle flash.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool destroyOnHit = true;
    
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 720f; // Degrees per second
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private Color bulletColor = new Color(1f, 0.8f, 0.2f); // Yellow/gold
    [SerializeField] private Color trailColor = new Color(1f, 0.5f, 0.1f, 0.5f); // Orange trail
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseAmount = 0.2f;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private TrailRenderer trail;
    private float direction;
    private float speed;
    private float baseScale;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Create visual if no sprite assigned
        if (sr != null && sr.sprite == null)
        {
            CreateBulletVisual();
        }
        
        // Add trail effect
        SetupTrail();
        
        baseScale = transform.localScale.x;
    }
    
    private void CreateBulletVisual()
    {
        // Create a simple bullet texture procedurally
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        
        Color transparent = new Color(0, 0, 0, 0);
        Color core = bulletColor;
        Color edge = new Color(bulletColor.r * 0.6f, bulletColor.g * 0.6f, bulletColor.b * 0.6f);
        
        // Clear to transparent
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, transparent);
        
        // Draw a bullet shape (elongated diamond/oval)
        int centerX = size / 2;
        int centerY = size / 2;
        
        // Core pixels (bright center)
        for (int x = 4; x < 12; x++)
        {
            for (int y = 6; y < 10; y++)
            {
                float distX = Mathf.Abs(x - centerX) / 4f;
                float distY = Mathf.Abs(y - centerY) / 2f;
                if (distX + distY < 1.2f)
                {
                    tex.SetPixel(x, y, distX + distY < 0.6f ? core : edge);
                }
            }
        }
        
        // Add bright highlight
        tex.SetPixel(centerX - 1, centerY, Color.white);
        tex.SetPixel(centerX, centerY, Color.white);
        
        tex.Apply();
        
        // Create sprite
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        sr.color = Color.white; // Use texture colors
    }
    
    private void SetupTrail()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
        
        trail.time = 0.15f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.02f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(trailColor, 0f), 
                new GradientColorKey(trailColor * 0.5f, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        trail.colorGradient = gradient;
        trail.sortingLayerName = "Player";
        trail.sortingOrder = -1;
    }
    
    private void Start()
    {
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
        
        // Muzzle flash effect
        StartCoroutine(MuzzleFlash());
    }
    
    private System.Collections.IEnumerator MuzzleFlash()
    {
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
        }
    }
    
    private void Update()
    {
        // Rotation animation
        if (enableRotation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * direction);
        }
        
        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * baseScale * pulse;
    }
    
    public void Initialize(float dir, float spd)
    {
        direction = dir;
        speed = spd;
        
        if (rb != null)
        {
            rb.velocity = new Vector2(direction * speed, 0);
        }
        
        // Flip sprite if going left
        if (sr != null && direction < 0)
        {
            sr.flipX = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore other projectiles and enemies
        if (other.GetComponent<Projectile>() != null) return;
        if (other.GetComponent<IceAgentController>() != null) return;
        if (other.GetComponent<HellMouth>() != null) return;
        
        // Hit player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            
            if (destroyOnHit)
            {
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
        // Hit wall/ground (check by layer or by having a collider that's not a trigger)
        else if (!other.isTrigger)
        {
            if (destroyOnHit)
            {
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }
    }
    
    private void SpawnHitEffect()
    {
        // Create a simple hit particle burst
        GameObject hitEffect = new GameObject("HitEffect");
        hitEffect.transform.position = transform.position;
        
        // Create a few particles
        for (int i = 0; i < 4; i++)
        {
            GameObject particle = new GameObject($"Particle_{i}");
            particle.transform.position = transform.position;
            
            SpriteRenderer psr = particle.AddComponent<SpriteRenderer>();
            psr.color = bulletColor;
            psr.sortingLayerName = "Player";
            
            // Create tiny square sprite
            Texture2D tex = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            psr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
            
            // Add simple movement
            Rigidbody2D prb = particle.AddComponent<Rigidbody2D>();
            prb.gravityScale = 2f;
            float angle = i * 90f + Random.Range(-30f, 30f);
            prb.velocity = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 3f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 3f + 2f
            );
            
            Destroy(particle, 0.5f);
        }
        
        Destroy(hitEffect, 1f);
    }
}

