using UnityEngine;

/// <summary>
/// Smooth camera follow script for 2D platformers.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10);
    [Tooltip("Lower = smoother/slower camera, Higher = snappier")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private bool useSmoothDamp = true;
    
    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;
    
    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 3f;
    
    private float currentLookAhead;
    private Vector3 velocity = Vector3.zero;
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        
        // Look ahead in direction of movement
        if (useLookAhead)
        {
            float targetLookAhead = 0;
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null && Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                targetLookAhead = Mathf.Sign(rb.velocity.x) * lookAheadDistance;
            }
            currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            desiredPosition.x += currentLookAhead;
        }
        
        // Clamp to bounds
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smooth follow - choose method
        Vector3 smoothedPosition;
        if (useSmoothDamp)
        {
            // SmoothDamp is gentler and handles velocity changes better
            smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
        else
        {
            // Lerp-based (original behavior)
            smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);
        }
        
        transform.position = smoothedPosition;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
}

