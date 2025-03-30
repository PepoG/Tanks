using UnityEngine;


public class InteractableLever : MonoBehaviour
{
    public enum LeverFunction
    {
        LeftTrack,
        RightTrack,
        TurretHorizontal,
        TurretVertical
    }

    [Header("Settings")]
    [SerializeField] private LeverFunction leverFunction = LeverFunction.LeftTrack;
    [SerializeField] private Transform leverTransform;
    [SerializeField] private Vector3 leverAxis = Vector3.right; // Axis of rotation
    [SerializeField] private float maxRotation = 45f; // Maximum angle in degrees
    [SerializeField] private float returnSpeed = 5f; // Speed to return to center when released
    [SerializeField] private bool returnToCenter = true; // Return to center when released?
    
    [Header("References")]
    [SerializeField] private TankController tankController;
    
    private bool isGrabbed = false;
    private Vector3 startRotation;
    private float currentValue = 0f;
    
    private void Awake()
    {
        // If leverTransform not assigned, use this transform
        if (leverTransform == null)
            leverTransform = transform;
            
        // Store starting rotation
        startRotation = leverTransform.localEulerAngles;
        
        // Find tank controller if not assigned
        if (tankController == null)
            tankController = FindObjectOfType<TankController>();
            
        // Setup XR interactable
        SetupInteractable();
    }
    
    private void SetupInteractable()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
        // Configure grab settings
        interactable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
        interactable.trackPosition = false;
        interactable.trackRotation = true;
        interactable.throwOnDetach = false;
        
        // Add grab events
        interactable.selectEntered.AddListener(args => isGrabbed = true);
        interactable.selectExited.AddListener(args => isGrabbed = false);
    }
    
    private void Update()
    {
        if (isGrabbed)
        {
            // Calculate lever value based on current rotation
            UpdateLeverValue();
        }
        else if (returnToCenter && Mathf.Abs(currentValue) > 0.01f)
        {
            // Return to center
            currentValue = Mathf.Lerp(currentValue, 0f, Time.deltaTime * returnSpeed);
            ApplyLeverRotation();
        }
        
        // Update tank controller based on lever function
        if (tankController != null)
        {
            switch (leverFunction)
            {
                case LeverFunction.LeftTrack:
                    tankController.SetLeftTrackValue(currentValue);
                    break;
                case LeverFunction.RightTrack:
                    tankController.SetRightTrackValue(currentValue);
                    break;
                case LeverFunction.TurretHorizontal:
                    tankController.SetTurretRotationValue(currentValue);
                    break;
                case LeverFunction.TurretVertical:
                    tankController.SetGunElevationValue(currentValue);
                    break;
            }
        }
    }
    
    private void UpdateLeverValue()
    {
        // Calculate rotation around the main axis
        float angle = 0f;
        Vector3 currentRot = leverTransform.localEulerAngles;
        
        // Get appropriate angle based on axis
        if (leverAxis == Vector3.right || leverAxis == -Vector3.right)
        {
            angle = NormalizeAngle(currentRot.x - startRotation.x);
        }
        else if (leverAxis == Vector3.up || leverAxis == -Vector3.up)
        {
            angle = NormalizeAngle(currentRot.y - startRotation.y);
        }
        else if (leverAxis == Vector3.forward || leverAxis == -Vector3.forward)
        {
            angle = NormalizeAngle(currentRot.z - startRotation.z);
        }
        
        // Convert to -1 to 1 range
        currentValue = Mathf.Clamp(angle / maxRotation, -1f, 1f);
    }
    
    private void ApplyLeverRotation()
    {
        // Apply rotation based on current value
        Vector3 targetRot = startRotation;
        
        // Apply rotation around appropriate axis
        if (leverAxis == Vector3.right || leverAxis == -Vector3.right)
        {
            targetRot.x = startRotation.x + currentValue * maxRotation;
        }
        else if (leverAxis == Vector3.up || leverAxis == -Vector3.up)
        {
            targetRot.y = startRotation.y + currentValue * maxRotation;
        }
        else if (leverAxis == Vector3.forward || leverAxis == -Vector3.forward)
        {
            targetRot.z = startRotation.z + currentValue * maxRotation;
        }
        
        leverTransform.localEulerAngles = targetRot;
    }
    
    private float NormalizeAngle(float angle)
    {
        // Convert angle to -180 to 180 range
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
            
        return angle;
    }
}