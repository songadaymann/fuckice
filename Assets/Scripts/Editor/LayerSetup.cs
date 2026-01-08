using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets up the Enemy layer and configures physics for pass-through enemies
/// </summary>
public class LayerSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Enemy Layer Physics")]
    public static void SetupEnemyLayerPhysics()
    {
        // Check if Enemy layer exists
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        
        if (enemyLayer < 0)
        {
            Debug.LogError("Enemy layer not found! Please create a layer called 'Enemy' in Edit > Project Settings > Tags and Layers");
            EditorUtility.DisplayDialog("Layer Missing", 
                "Please create a layer called 'Enemy' in:\nEdit > Project Settings > Tags and Layers\n\nThen run this tool again.", 
                "OK");
            return;
        }
        
        if (playerLayer < 0)
        {
            Debug.LogError("Player layer not found!");
            return;
        }
        
        // Set up physics collision matrix
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        
        Debug.Log($"SUCCESS: Player (layer {playerLayer}) and Enemy (layer {enemyLayer}) will now pass through each other!");
        Debug.Log("Players can still stomp enemies thanks to the trigger collider detection.");
        
        // Save the physics settings
        EditorUtility.SetDirty(Physics2D.defaultPhysicsMaterial);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Layer Physics Setup Complete", 
            "Player and Enemy layers are now configured to pass through each other.\n\n" +
            "Make sure your IceAgent prefabs are on the 'Enemy' layer!", 
            "OK");
    }
    
    [MenuItem("Tools/Assign Enemy Layer to All ICE Agents")]
    public static void AssignEnemyLayerToAgents()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        
        if (enemyLayer < 0)
        {
            Debug.LogError("Enemy layer not found! Please create it first.");
            EditorUtility.DisplayDialog("Layer Missing", 
                "Please create a layer called 'Enemy' first.", 
                "OK");
            return;
        }
        
        // Find all IceAgentController objects in the scene
        IceAgentController[] agents = Object.FindObjectsOfType<IceAgentController>();
        
        foreach (var agent in agents)
        {
            agent.gameObject.layer = enemyLayer;
            EditorUtility.SetDirty(agent.gameObject);
        }
        
        Debug.Log($"Assigned Enemy layer to {agents.Length} ICE agents in the scene.");
        
        // Also try to update the prefab if there is one
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab IceAgent");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<IceAgentController>() != null)
            {
                prefab.layer = enemyLayer;
                EditorUtility.SetDirty(prefab);
                Debug.Log($"Updated prefab: {path}");
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Enemy Layer Assigned", 
            $"Assigned Enemy layer to {agents.Length} ICE agents in the scene.", 
            "OK");
    }
}

