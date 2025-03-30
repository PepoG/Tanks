using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TankDoorInteraction : MonoBehaviour
{
    [SerializeField] private Transform playerSeatPosition;
    [SerializeField] private GameObject tankInterior;
    [SerializeField] private GameObject tankExterior;
    
    private bool isPlayerInside = false;
    
    // Setup the interactable component
    private void Awake()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
        
        // Add activation event
        interactable.selectEntered.AddListener(args => OnDoorActivated());
    }
    
    // Handle door activation (entry/exit)
    private void OnDoorActivated()
    {
        if (isPlayerInside)
        {
            ExitTank();
        }
        else
        {
            EnterTank();
        }
        
        isPlayerInside = !isPlayerInside;
    }
    
    // Teleport player inside tank
    private void EnterTank()
    {
        if (playerSeatPosition != null)
        {
            // Position the VR rig at the seat position
            RepositionPlayer(playerSeatPosition.position, playerSeatPosition.rotation);
            
            // Show interior, hide exterior if needed
            if (tankInterior != null) tankInterior.SetActive(true);
            if (tankExterior != null) tankExterior.SetActive(false);
            
            Debug.Log("Player entered tank");
        }
        else
        {
            Debug.LogError("Player seat position not assigned!");
        }
    }
    
    // Teleport player outside tank
    private void ExitTank()
    {
        // Get exit position (1 meter behind the door)
        Vector3 exitPosition = transform.position - transform.forward * 1.5f;
        Quaternion exitRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);
        
        // Position the VR rig outside
        RepositionPlayer(exitPosition, exitRotation);
        
        // Show exterior, hide interior if needed
        if (tankInterior != null) tankInterior.SetActive(false);
        if (tankExterior != null) tankExterior.SetActive(true);
        
        Debug.Log("Player exited tank");
    }
    
    // Helper method to move the player/camera rig
    private void RepositionPlayer(Vector3 position, Quaternion rotation)
    {
        // Find XR Rig or main camera
        var xrRig = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
        if (xrRig != null)
        {
            xrRig.transform.position = position;
            xrRig.transform.rotation = rotation;
        }
        else
        {
            // Fallback to camera
            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != null)
            {
                mainCamera.transform.parent.position = position;
                mainCamera.transform.parent.rotation = rotation;
            }
        }
    }
}