using UnityEngine;

/// <summary>
/// Parallax scrolling effect for background layers.
/// Attach to each background layer sprite.
/// Creates seamless horizontal tiling automatically.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = moves with camera (foreground), 1 = stationary (far background)")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectX = 0.5f;
    
    [Tooltip("0 = moves with camera, 1 = stationary (recommended for backgrounds)")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectY = 1f;
    
    [Header("Infinite Scrolling")]
    [SerializeField] private bool infiniteHorizontal = true;
    [SerializeField] private bool infiniteVertical = false;
    [Tooltip("How many copies on each side (more = wider view support)")]
    [SerializeField] private int tilesPerSide = 2;
    
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float spriteWidth;
    private float spriteHeight;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    
    // For seamless tiling, we create child copies
    private GameObject[,] tileCopies;
    
    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
        startPosition = transform.position;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // Calculate sprite dimensions in world units
            Sprite sprite = spriteRenderer.sprite;
            spriteWidth = sprite.bounds.size.x * transform.localScale.x;
            spriteHeight = sprite.bounds.size.y * transform.localScale.y;
            
            // Create tile copies for seamless scrolling
            if (infiniteHorizontal || infiniteVertical)
            {
                CreateTileCopies();
            }
        }
    }
    
    private void CreateTileCopies()
    {
        int horizontalTiles = infiniteHorizontal ? (tilesPerSide * 2 + 1) : 1;
        int verticalTiles = infiniteVertical ? (tilesPerSide * 2 + 1) : 1;
        
        tileCopies = new GameObject[horizontalTiles, verticalTiles];
        
        int startX = infiniteHorizontal ? -tilesPerSide : 0;
        int endX = infiniteHorizontal ? tilesPerSide : 0;
        int startY = infiniteVertical ? -tilesPerSide : 0;
        int endY = infiniteVertical ? tilesPerSide : 0;
        
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                // Skip center (0,0) - that's the original sprite
                if (x == 0 && y == 0) continue;
                
                GameObject copy = CreateTileCopy(x, y);
                int indexX = x - startX;
                int indexY = y - startY;
                tileCopies[indexX, indexY] = copy;
            }
        }
    }
    
    private GameObject CreateTileCopy(int offsetX, int offsetY)
    {
        GameObject copy = new GameObject($"{gameObject.name}_Tile_{offsetX}_{offsetY}");
        copy.transform.parent = transform;
        copy.transform.localPosition = new Vector3(spriteWidth * offsetX, spriteHeight * offsetY, 0);
        copy.transform.localRotation = Quaternion.identity;
        copy.transform.localScale = Vector3.one;
        
        SpriteRenderer copySR = copy.AddComponent<SpriteRenderer>();
        copySR.sprite = spriteRenderer.sprite;
        copySR.sortingLayerName = spriteRenderer.sortingLayerName;
        copySR.sortingOrder = spriteRenderer.sortingOrder;
        copySR.color = spriteRenderer.color;
        
        return copy;
    }
    
    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // Calculate parallax offset
        // parallaxEffectX: 0 = moves with camera, 1 = stationary
        float parallaxX = deltaMovement.x * (1f - parallaxEffectX);
        
        // parallaxEffectY: 0 = moves with camera, 1 = stationary  
        float parallaxY = deltaMovement.y * (1f - parallaxEffectY);
        
        // Apply movement
        transform.position += new Vector3(parallaxX, parallaxY, 0);
        
        lastCameraPosition = cameraTransform.position;
        
        // Handle infinite scrolling by repositioning when too far
        Vector3 newPos = transform.position;
        
        if (infiniteHorizontal && spriteWidth > 0)
        {
            float relativeX = cameraTransform.position.x - transform.position.x;
            if (Mathf.Abs(relativeX) >= spriteWidth)
            {
                float snapOffset = Mathf.Round(relativeX / spriteWidth) * spriteWidth;
                newPos.x += snapOffset;
            }
        }
        
        if (infiniteVertical && spriteHeight > 0)
        {
            float relativeY = cameraTransform.position.y - transform.position.y;
            if (Mathf.Abs(relativeY) >= spriteHeight)
            {
                float snapOffset = Mathf.Round(relativeY / spriteHeight) * spriteHeight;
                newPos.y += snapOffset;
            }
        }
        
        transform.position = newPos;
    }
    
    private void OnDestroy()
    {
        // Clean up tile copies
        if (tileCopies != null)
        {
            for (int x = 0; x < tileCopies.GetLength(0); x++)
            {
                for (int y = 0; y < tileCopies.GetLength(1); y++)
                {
                    if (tileCopies[x, y] != null)
                    {
                        Destroy(tileCopies[x, y]);
                    }
                }
            }
        }
    }
}
