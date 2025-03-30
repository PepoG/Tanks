using UnityEngine;


public class DoorInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isEntryDoor = true;
    [SerializeField] private TeleportManager teleportManager;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorOpenSpeed = 2f;
    [SerializeField] private float autoCloseDelay = 2f;
    
    [Header("References")]
    [SerializeField] private Transform doorPivot;
    
    // State
    private bool isDoorOpen = false;
    private Vector3 closedRotation;
    private Vector3 openRotation;
    private float doorTimer = 0f;
    
    private void Awake()
    {
        // Find teleport manager if not assigned
        if (teleportManager == null)
            teleportManager = FindObjectOfType<TeleportManager>();
            
        // Use this transform if no door pivot is specified
        if (doorPivot == null)
            doorPivot = transform;
            
        // Store door rotations
        closedRotation = doorPivot.localEulerAngles;
        openRotation = closedRotation + new Vector3(0, doorOpenAngle, 0);
        
        // Setup XR interactable
        SetupInteractable();
    }
    
    private void SetupInteractable()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            
        // Add interaction events
        interactable.selectEntered.AddListener(args => HandleDoorInteraction());
    }
    
    private void Update()
    {
        // Handle door animation
        if (isDoorOpen)
        {
            // Open door
            doorPivot.localEulerAngles = Vector3.Lerp(
                doorPivot.localEulerAngles,
                openRotation,
                Time.deltaTime * doorOpenSpeed
            );
            
            // Auto close after delay
            doorTimer += Time.deltaTime;
            if (doorTimer >= autoCloseDelay)
            {
                isDoorOpen = false;
                doorTimer = 0f;
            }
        }
        else
        {
            // Close door
            doorPivot.localEulerAngles = Vector3.Lerp(
                doorPivot.localEulerAngles,
                closedRotation,
                Time.deltaTime * doorOpenSpeed
            );
        }
    }
    
    private void HandleDoorInteraction()
    {
        // Open door
        OpenDoor();
        
        // Handle teleportation after a short delay
        Invoke(nameof(TeleportPlayer), 0.5f);
    }
    
    private void OpenDoor()
    {
        isDoorOpen = true;
        doorTimer = 0f;
    }
    
    private void TeleportPlayer()
    {
        if (teleportManager == null)
            return;
            
        if (isEntryDoor && !teleportManager.IsInside())
        {
            teleportManager.TeleportInside();
        }
        else if (!isEntryDoor && teleportManager.IsInside())
        {
            teleportManager.TeleportOutside();
        }
    }
}