using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Simple button that reports press state to MobileControls.
/// Attach to each mobile control button.
/// </summary>
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Left, Right, Jump }
    
    [SerializeField] private ButtonType buttonType;
    
    private MobileControls controls;
    
    private void Start()
    {
        controls = GetComponentInParent<MobileControls>();
        if (controls == null)
        {
            controls = FindObjectOfType<MobileControls>();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (controls == null) return;
        
        switch (buttonType)
        {
            case ButtonType.Left: controls.OnLeftDown(); break;
            case ButtonType.Right: controls.OnRightDown(); break;
            case ButtonType.Jump: controls.OnJumpDown(); break;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (controls == null) return;
        
        switch (buttonType)
        {
            case ButtonType.Left: controls.OnLeftUp(); break;
            case ButtonType.Right: controls.OnRightUp(); break;
            case ButtonType.Jump: controls.OnJumpUp(); break;
        }
    }
}

