using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// In-game loading screen with animated Flurry running on a loading bar.
/// Use for scene transitions within the game.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBar;
    [SerializeField] private Text loadingText;
    [SerializeField] private RectTransform runnerIcon;
    
    [Header("Runner Animation")]
    [SerializeField] private Sprite[] runnerSprites; // Flurry sprites
    [SerializeField] private float runnerAnimSpeed = 8f;
    [SerializeField] private float barWidth = 300f;
    
    [Header("Settings")]
    [SerializeField] private float minLoadTime = 1f; // Minimum time to show loading
    
    private Image runnerImage;
    private int currentFrame;
    private float animTimer;
    private float runDirection = 1f;
    private static LoadingScreen instance;
    
    public static LoadingScreen Instance => instance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        if (runnerIcon != null)
        {
            runnerImage = runnerIcon.GetComponent<Image>();
        }
    }
    
    /// <summary>
    /// Load a scene with the loading screen
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.LoadSceneAsync(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// Load a scene by index with the loading screen
    /// </summary>
    public static void LoadScene(int sceneIndex)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.LoadSceneAsync(sceneIndex));
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return LoadSceneRoutine(SceneManager.LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        yield return LoadSceneRoutine(SceneManager.LoadSceneAsync(sceneIndex));
    }
    
    private IEnumerator LoadSceneRoutine(AsyncOperation operation)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        operation.allowSceneActivation = false;
        float startTime = Time.time;
        float progress = 0f;
        
        while (!operation.isDone)
        {
            // Unity loads to 0.9, then waits for allowSceneActivation
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            UpdateLoadingBar(progress);
            UpdateRunner();
            
            // Check if loading is done and minimum time has passed
            if (operation.progress >= 0.9f && Time.time - startTime >= minLoadTime)
            {
                // Finish the loading bar animation
                while (progress < 1f)
                {
                    progress += Time.deltaTime * 2f;
                    UpdateLoadingBar(progress);
                    UpdateRunner();
                    yield return null;
                }
                
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    private void UpdateLoadingBar(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.fillAmount = progress;
        }
        
        if (loadingText != null)
        {
            loadingText.text = $"LOADING... {Mathf.RoundToInt(progress * 100)}%";
        }
        
        // Move runner along the bar
        if (runnerIcon != null)
        {
            float xPos = progress * barWidth - (barWidth / 2f);
            runnerIcon.anchoredPosition = new Vector2(xPos, runnerIcon.anchoredPosition.y);
        }
    }
    
    private void UpdateRunner()
    {
        if (runnerSprites == null || runnerSprites.Length == 0 || runnerImage == null) return;
        
        animTimer += Time.deltaTime * runnerAnimSpeed;
        if (animTimer >= 1f)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % runnerSprites.Length;
            runnerImage.sprite = runnerSprites[currentFrame];
        }
    }
    
    /// <summary>
    /// Show a fake loading screen (for effect)
    /// </summary>
    public static IEnumerator ShowFakeLoading(float duration)
    {
        if (instance == null || instance.loadingPanel == null) yield break;
        
        instance.loadingPanel.SetActive(true);
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            instance.UpdateLoadingBar(progress);
            instance.UpdateRunner();
            yield return null;
        }
        
        instance.loadingPanel.SetActive(false);
    }
}

