using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tools to create pre-configured dialogue triggers
/// </summary>
public class DialogueSetup : Editor
{
    // ============================================
    // DIALOGUE MESSAGES
    // ============================================
    
    public static readonly string ICE_AGENT_FIRST_ENCOUNTER = 
        "Look out! It's ICE! Hop on his head before he shoots you!";
    
    public static readonly string HELL_MOUTH_FIRST_ENCOUNTER = 
        "It's an ICE AGENT HELL MOUTH! I think only eggs from a transgender bird can close those!";
    
    public static readonly string BIRDO_CAGE_ENCOUNTER = 
        "Birdo is in a cage! I wonder if there's a key...";
    
    public static readonly string KEY_PICKUP = 
        "Got the key! Now to find that cage...";
    
    public static readonly string HELL_MOUTH_CLOSED_FIRST = 
        "You destroyed the ICE HELL MOUTH! Only two to go!";
    
    public static readonly string HELL_MOUTH_CLOSED_SECOND = 
        "Another one down! One more HELL MOUTH to close!";
    
    public static readonly string HELL_MOUTH_CLOSED_FINAL = 
        "You did it! All the HELL MOUTHS are closed! ICE is defeated!";
    
    // ============================================
    // MENU ITEMS
    // ============================================
    
    [MenuItem("Tools/Dialogue/Create ICE Agent Trigger")]
    public static void CreateIceAgentTrigger()
    {
        CreateDialogueTrigger("ICE Agent Dialogue", ICE_AGENT_FIRST_ENCOUNTER, 4f, new Vector2(5f, 3f));
    }
    
    [MenuItem("Tools/Dialogue/Create Hell Mouth Trigger")]
    public static void CreateHellMouthTrigger()
    {
        CreateDialogueTrigger("Hell Mouth Dialogue", HELL_MOUTH_FIRST_ENCOUNTER, 5f, new Vector2(6f, 4f));
    }
    
    [MenuItem("Tools/Dialogue/Create Birdo Cage Trigger")]
    public static void CreateBirdoCageTrigger()
    {
        CreateDialogueTrigger("Birdo Cage Dialogue", BIRDO_CAGE_ENCOUNTER, 4f, new Vector2(4f, 3f));
    }
    
    [MenuItem("Tools/Dialogue/Create Key Pickup Trigger")]
    public static void CreateKeyPickupTrigger()
    {
        CreateDialogueTrigger("Key Pickup Dialogue", KEY_PICKUP, 3f, new Vector2(2f, 2f));
    }
    
    [MenuItem("Tools/Dialogue/Create All Story Triggers")]
    public static void CreateAllStoryTriggers()
    {
        Debug.Log("=== Creating Story Dialogue Triggers ===");
        
        // Parent for organization
        GameObject parent = new GameObject("StoryDialogueTriggers");
        int created = 0;
        
        // 1. Find first ICE Agent and place trigger BEFORE it (to the left)
        IceAgentController[] agents = Object.FindObjectsOfType<IceAgentController>();
        if (agents.Length > 0)
        {
            // Find the leftmost agent (first one player encounters)
            IceAgentController firstAgent = agents[0];
            foreach (var agent in agents)
            {
                if (agent.transform.position.x < firstAgent.transform.position.x)
                    firstAgent = agent;
            }
            
            GameObject ice = CreateDialogueTrigger("ICE_Agent_Dialogue", ICE_AGENT_FIRST_ENCOUNTER, 4f, new Vector2(4f, 4f));
            // Place trigger 5 units to the LEFT of the agent (player walks into it first)
            ice.transform.position = firstAgent.transform.position + Vector3.left * 5f;
            ice.transform.SetParent(parent.transform);
            created++;
            Debug.Log($"Placed ICE trigger near agent at {firstAgent.transform.position}");
        }
        else
        {
            Debug.LogWarning("No ICE Agents found in scene - skipping ICE trigger");
        }
        
        // 2. Find Birdo cage and place trigger nearby
        BirdoCage cage = Object.FindObjectOfType<BirdoCage>();
        if (cage != null)
        {
            GameObject cageTrigger = CreateDialogueTrigger("Birdo_Cage_Dialogue", BIRDO_CAGE_ENCOUNTER, 4f, new Vector2(4f, 4f));
            // Place trigger to the left of the cage
            cageTrigger.transform.position = cage.transform.position + Vector3.left * 4f;
            cageTrigger.transform.SetParent(parent.transform);
            created++;
            Debug.Log($"Placed cage trigger near Birdo cage at {cage.transform.position}");
        }
        else
        {
            Debug.LogWarning("No BirdoCage found in scene - skipping cage trigger");
        }
        
        // 3. Find first Hell Mouth and place trigger nearby
        HellMouth[] hellMouths = Object.FindObjectsOfType<HellMouth>();
        if (hellMouths.Length > 0)
        {
            // Find the leftmost hell mouth
            HellMouth firstHM = hellMouths[0];
            foreach (var hm in hellMouths)
            {
                if (hm.transform.position.x < firstHM.transform.position.x)
                    firstHM = hm;
            }
            
            GameObject hmTrigger = CreateDialogueTrigger("Hell_Mouth_Dialogue", HELL_MOUTH_FIRST_ENCOUNTER, 5f, new Vector2(5f, 5f));
            // Place trigger to the left of the hell mouth
            hmTrigger.transform.position = firstHM.transform.position + Vector3.left * 6f;
            hmTrigger.transform.SetParent(parent.transform);
            created++;
            Debug.Log($"Placed Hell Mouth trigger near {firstHM.name} at {firstHM.transform.position}");
        }
        else
        {
            Debug.LogWarning("No Hell Mouths found in scene - skipping Hell Mouth trigger");
        }
        
        Selection.activeGameObject = parent;
        Undo.RegisterCreatedObjectUndo(parent, "Create Story Triggers");
        
        if (created > 0)
        {
            EditorUtility.DisplayDialog("Story Triggers Created!",
                $"Created {created} dialogue triggers near your game objects:\n\n" +
                "• ICE Agent trigger - placed before first ICE agent\n" +
                "• Birdo Cage trigger - placed before the cage\n" +
                "• Hell Mouth trigger - placed before first Hell Mouth\n\n" +
                "You can adjust positions in the Scene view if needed!",
                "Awesome!");
        }
        else
        {
            EditorUtility.DisplayDialog("No Objects Found",
                "Couldn't find ICE Agents, Birdo Cage, or Hell Mouths in the scene.\n\n" +
                "Make sure you've set up these objects first:\n" +
                "• Tools → Setup IceAgent Enemy\n" +
                "• Tools → Setup Birdo System\n" +
                "• Tools → Setup Hell Mouths",
                "OK");
        }
    }
    
    // ============================================
    // HELPER METHODS
    // ============================================
    
    static GameObject CreateDialogueTrigger(string name, string message, float duration, Vector2 triggerSize)
    {
        GameObject triggerObj = new GameObject(name);
        
        // Add trigger collider
        BoxCollider2D collider = triggerObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = triggerSize;
        
        // Add dialogue trigger component
        DialogueTrigger trigger = triggerObj.AddComponent<DialogueTrigger>();
        trigger.message = message;
        trigger.displayDuration = duration;
        trigger.triggerOnce = true;
        trigger.destroyAfterTrigger = true; // Clean up after showing
        
        // Position at scene view center
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            triggerObj.transform.position = sceneView.pivot;
        }
        
        Selection.activeGameObject = triggerObj;
        Undo.RegisterCreatedObjectUndo(triggerObj, "Create Dialogue Trigger");
        
        Debug.Log($"Created dialogue trigger: {name}");
        return triggerObj;
    }
}

