using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Mobile touch controls - virtual joystick/d-pad and jump button.
/// Automatically shows on touch devices, hides on desktop.
/// </summary>
public class MobileControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform leftButton;
    [SerializeField] private RectTransform rightButton;
    [SerializeField] private RectTransform jumpButton;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    [SerializeField] private bool alwaysShow = false; // For testing on desktop
    [SerializeField] private float buttonOpacity = 0.6f;
    
    // Input state - read by PlayerController
    public static float HorizontalInput { get; private set; }
    public static bool JumpPressed { get; private set; }
    public static bool JumpHeld { get; private set; }
    public static bool IsMobile { get; private set; }
    
    // Button states
    private bool leftHeld = false;
    private bool rightHeld = false;
    private bool jumpHeldInternal = false;
    private bool jumpPressedThisFrame = false;
    
    private static MobileControls instance;
    
    private void Awake()
    {
        instance = this;
        
        // Detect if we're on a touch device
        IsMobile = Application.isMobilePlatform || 
                   Input.touchSupported ||
                   alwaysShow;
        
        // For WebGL, also check screen size as a hint
        #if UNITY_WEBGL
        IsMobile = IsMobile || Screen.width < 1024;
        #endif
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = IsMobile ? buttonOpacity : 0f;
            canvasGroup.interactable = IsMobile;
            canvasGroup.blocksRaycasts = IsMobile;
        }
        
        Debug.Log($"MobileControls: IsMobile={IsMobile}, touchSupported={Input.touchSupported}, screenWidth={Screen.width}, alwaysShow={alwaysShow}");
    }
    
    private void Start()
    {
        // Double-check visibility - if controls are visible, enable mobile input
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            IsMobile = true;
        }
    }
    
    private void Update()
    {
        // Calculate horizontal input
        HorizontalInput = 0f;
        if (leftHeld) HorizontalInput -= 1f;
        if (rightHeld) HorizontalInput += 1f;
        
        // Jump input
        JumpPressed = jumpPressedThisFrame;
        JumpHeld = jumpHeldInternal;
        
        // Reset the "pressed this frame" flag at end of frame
        jumpPressedThisFrame = false;
        
        // Also check for touch anywhere on right side of screen for jump (optional)
        CheckTouchInput();
    }
    
    private void CheckTouchInput()
    {
        // Optional: Allow tapping right side of screen to jump
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            // If touch is on right half of screen and not on a button
            if (touch.position.x > Screen.width * 0.7f && 
                touch.position.y > Screen.height * 0.3f)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    jumpPressedThisFrame = true;
                    jumpHeldInternal = true;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    jumpHeldInternal = false;
                }
            }
        }
    }
    
    // Called by UI buttons via EventTrigger
    public void OnLeftDown() { leftHeld = true; }
    public void OnLeftUp() { leftHeld = false; }
    
    public void OnRightDown() { rightHeld = true; }
    public void OnRightUp() { rightHeld = false; }
    
    public void OnJumpDown() 
    { 
        jumpHeldInternal = true; 
        jumpPressedThisFrame = true;
    }
    public void OnJumpUp() { jumpHeldInternal = false; }
    
    /// <summary>
    /// Force show/hide mobile controls (for testing)
    /// </summary>
    public static void SetVisible(bool visible)
    {
        if (instance != null && instance.canvasGroup != null)
        {
            IsMobile = visible;
            instance.canvasGroup.alpha = visible ? instance.buttonOpacity : 0f;
            instance.canvasGroup.interactable = visible;
            instance.canvasGroup.blocksRaycasts = visible;
        }
    }
}

