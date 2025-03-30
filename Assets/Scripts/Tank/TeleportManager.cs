using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform avatarHead;
    
    [Header("Teleport Locations")]
    [SerializeField] private Transform outsidePosition;
    [SerializeField] private Transform insidePosition;
    
    private bool isInside = false;
    
    private void Awake()
    {
        // Find references if not assigned
        if (xrOrigin == null)
        {
            XROrigin origin = FindObjectOfType<XROrigin>();
            if (origin != null)
                xrOrigin = origin.transform;
        }
        
        if (playerCamera == null && xrOrigin != null)
        {
            Camera mainCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (mainCamera != null)
                playerCamera = mainCamera.transform;
        }
    }
    
    public void TeleportInside()
    {
        if (isInside || insidePosition == null || xrOrigin == null)
            return;
            
        TeleportTo(insidePosition);
        isInside = true;
    }
    
    public void TeleportOutside()
    {
        if (!isInside || outsidePosition == null || xrOrigin == null)
            return;
            
        TeleportTo(outsidePosition);
        isInside = false;
    }
    
    public void ToggleTeleport()
    {
        if (isInside)
            TeleportOutside();
        else
            TeleportInside();
    }
    
    private void TeleportTo(Transform targetPosition)
    {
        if (xrOrigin == null || targetPosition == null)
            return;
        
        // Calculate the offset between camera and XR origin
        Vector3 cameraOffset = Vector3.zero;
        if (playerCamera != null)
        {
            cameraOffset = new Vector3(
                playerCamera.localPosition.x,
                0, // Don't consider Y offset to keep player on ground
                playerCamera.localPosition.z
            );
        }
        
        // Calculate the final position
        Vector3 finalPosition = targetPosition.position - cameraOffset;
        
        // Set the XR origin position
        xrOrigin.position = new Vector3(
            finalPosition.x,
            targetPosition.position.y, // Use target Y position directly
            finalPosition.z
        );
        
        // Set the XR origin rotation (only Y rotation)
        Vector3 targetRotation = targetPosition.eulerAngles;
        xrOrigin.rotation = Quaternion.Euler(0, targetRotation.y, 0);
    }
    
    // Move avatar to match player head position
    public void UpdateAvatarPosition()
    {
        if (avatarHead == null || playerCamera == null)
            return;
            
        // Set avatar head position and rotation to match player camera
        avatarHead.position = playerCamera.position;
        avatarHead.rotation = playerCamera.rotation;
    }
    
    public bool IsInside()
    {
        return isInside;
    }
}