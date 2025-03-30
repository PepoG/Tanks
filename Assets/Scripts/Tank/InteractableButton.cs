using UnityEngine;

using UnityEngine.Events;

public class InteractableButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pressDistance = 0.02f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private Transform buttonTransform;
    [SerializeField] private Vector3 buttonAxis = Vector3.up;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;
    
    // State
    private Vector3 startPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private bool isBeingPressed = false;
    
    private void Awake()
    {
        // Use this transform if no button transform specified
        if (buttonTransform == null) 
            buttonTransform = transform;
            
        // Store starting position
        startPosition = buttonTransform.localPosition;
        
        // Calculate pressed position
        pressedPosition = startPosition - buttonAxis.normalized * pressDistance;
        
        // Setup XR interactable
        SetupInteractable();
    }
    
    private void SetupInteractable()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            
        // Add interaction events
        interactable.selectEntered.AddListener(args => PressButton());
        interactable.selectExited.AddListener(args => ReleaseButton());
    }
    
    private void Update()
    {
        // Handle button movement
        if (isBeingPressed)
        {
            buttonTransform.localPosition = Vector3.Lerp(
                buttonTransform.localPosition, 
                pressedPosition, 
                Time.deltaTime * returnSpeed);
                
            // If button wasn't pressed before and now is close enough to pressed position
            if (!isPressed && Vector3.Distance(buttonTransform.localPosition, pressedPosition) < 0.002f)
            {
                isPressed = true;
                onPressed?.Invoke();
            }
        }
        else
        {
            buttonTransform.localPosition = Vector3.Lerp(
                buttonTransform.localPosition, 
                startPosition, 
                Time.deltaTime * returnSpeed);
                
            // Reset pressed state when button moves back
            if (isPressed && Vector3.Distance(buttonTransform.localPosition, startPosition) < 0.002f)
            {
                isPressed = false;
                onReleased?.Invoke();
            }
        }
    }
    
    private void PressButton()
    {
        isBeingPressed = true;
    }
    
    private void ReleaseButton()
    {
        isBeingPressed = false;
    }
    
    // Call this method to press the button programmatically
    public void Press()
    {
        PressButton();
        Invoke(nameof(ReleaseButton), 0.2f);
    }
}