using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorInteractor : MonoBehaviour
{
    public enum DoorType { Entry, Exit }
    
    [Header("Settings")]
    [SerializeField] private DoorType doorType = DoorType.Entry;
    [SerializeField] private TeleportManager teleportManager;
    
    [Header("Door Animation")]
    [SerializeField] private bool animateDoor = true;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorAnimationSpeed = 1.5f;
    
    [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.green;
    
    [Header("Audio")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    
    [Header("Events")]
    public UnityEvent OnDoorOpened;
    public UnityEvent OnDoorClosed;
    
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private bool isDoorOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
        
        // Find teleport manager if not assigned
        if (teleportManager == null)
        {
            teleportManager = FindObjectOfType<TeleportManager>();
            if (teleportManager == null)
            {
                Debug.LogWarning("No TeleportManager found in scene. Door teleportation will not function.");
            }
        }
        
        // Set up XR interaction
        SetupDoorInteractable();
        
        // Store original rotation
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, doorOpenAngle, 0));
    }
    
    private void SetupDoorInteractable()
    {
        // Add interactable component if not present
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
        
        // Add box collider if there's no collider
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false;
        }
        
        // Setup interaction events
        interactable.selectEntered.AddListener(OnDoorSelected);
        interactable.hoverEntered.AddListener(OnDoorHoverEnter);
        interactable.hoverExited.AddListener(OnDoorHoverExit);
    }
    
    private void OnDoorSelected(SelectEnterEventArgs args)
    {
        if (isDoorOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    private void OnDoorHoverEnter(HoverEnterEventArgs args)
    {
        if (meshRenderer != null && highlightMaterial != null)
        {
            meshRenderer.material = highlightMaterial;
            highlightMaterial.color = highlightColor;
        }
    }
    
    private void OnDoorHoverExit(HoverExitEventArgs args)
    {
        if (meshRenderer != null && highlightMaterial != null)
        {
            highlightMaterial.color = normalColor;
        }
    }
    
    public void OpenDoor()
    {
        isDoorOpen = true;
        
        // Play sound
        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Animate door
        if (animateDoor)
        {
            StartCoroutine(AnimateDoor(closedRotation, openRotation));
        }
        
        // Handle teleportation
        if (teleportManager != null)
        {
            if (doorType == DoorType.Entry)
            {
                teleportManager.TeleportIntoTank();
            }
            else
            {
                teleportManager.TeleportOutOfTank();
            }
        }
        
        // Trigger events
        OnDoorOpened?.Invoke();
    }
    
    public void CloseDoor()
    {
        isDoorOpen = false;
        
        // Play sound
        if (doorCloseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }
        
        // Animate door
        if (animateDoor)
        {
            StartCoroutine(AnimateDoor(openRotation, closedRotation));
        }
        
        // Trigger events
        OnDoorClosed?.Invoke();
    }
    
    private System.Collections.IEnumerator AnimateDoor(Quaternion from, Quaternion to)
    {
        float time = 0;
        while (time < 1)
        {
            transform.localRotation = Quaternion.Slerp(from, to, time);
            time += Time.deltaTime * doorAnimationSpeed;
            yield return null;
        }
        transform.localRotation = to;
    }
}