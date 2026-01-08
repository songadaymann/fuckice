using UnityEngine;

/// <summary>
/// Child trigger collider for detecting player contact without physical collision.
/// Allows player to pass through enemies while still being able to stomp/take damage.
/// </summary>
public class PlayerDetector : MonoBehaviour
{
    private IceAgentController parentAgent;
    
    public void Initialize(IceAgentController agent)
    {
        parentAgent = agent;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (parentAgent == null) return;
        
        if (other.CompareTag("Player"))
        {
            parentAgent.HandlePlayerContact(other);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (parentAgent == null) return;
        
        // Continuous contact for damage over time if needed
        if (other.CompareTag("Player"))
        {
            parentAgent.HandlePlayerContactContinuous(other);
        }
    }
}

