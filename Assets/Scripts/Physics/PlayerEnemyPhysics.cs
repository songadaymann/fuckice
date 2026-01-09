using UnityEngine;

/// <summary>
/// Sets up physics so player can pass through enemies but still stomp them.
/// Attach to a manager object or the player.
/// </summary>
public class PlayerEnemyPhysics : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetupPhysics()
    {
        // Get layer indices
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        
        // If layers don't exist, they'll return -1
        if (playerLayer >= 0 && enemyLayer >= 0)
        {
            // Ignore physical collision between Player and Enemy layers
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
            Debug.Log($"PlayerEnemyPhysics: Ignoring collision between Player (layer {playerLayer}) and Enemy (layer {enemyLayer})");
        }
        else
        {
            Debug.LogWarning($"PlayerEnemyPhysics: Could not find Player or Enemy layer. Player={playerLayer}, Enemy={enemyLayer}");
        }
    }
}

