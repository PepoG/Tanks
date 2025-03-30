using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controls an interactive lever that can be manipulated in VR
/// </summary>
public class LeverController : MonoBehaviour
{
    public enum LeverType
    {
        LeftTrack,
        RightTrack,
        TurretRotation,
        GunElevation
    }
    
    [SerializeField] private LeverType leverType = LeverType.LeftTrack;
    [SerializeField] private float maxRotation = 45f;
    [SerializeField] private bool returnToCenter = true;
    [SerializeField] private TankController tankController;
    
    private float normalizedValue = 0f;
    private bool isGrabbed = false;
    private Vector3 initialRotation;
    
    private void Awake()
    {
        initialRotation = transform.localEulerAngles;
        
        if (tankController == null)
            tankController = FindObjectOfType<TankController>();
    }
    
    private void Update()
    {
        // Return to center if not grabbed
        if (!isGrabbed && returnToCenter)
        {
            normalizedValue = Mathf.Lerp(normalizedValue, 0f, Time.deltaTime * 5f);
            UpdateRotation();
        }
        
        // Update tank controller
        if (tankController != null)
        {
            switch (leverType)
            {
                case LeverType.LeftTrack:
                    tankController.SetLeftTrackValue(normalizedValue);
                    break;
                case LeverType.RightTrack:
                    tankController.SetRightTrackValue(normalizedValue);
                    break;
                case LeverType.TurretRotation:
                    tankController.SetTurretRotationValue(normalizedValue);
                    break;
                case LeverType.GunElevation:
                    tankController.SetGunElevationValue(normalizedValue);
                    break;
            }
        }
    }
    
    public void OnGrab()
    {
        isGrabbed = true;
    }
    
    public void OnRelease()
    {
        isGrabbed = false;
    }
    
    public void SetValue(float value)
    {
        normalizedValue = Mathf.Clamp(value, -1f, 1f);
        UpdateRotation();
    }
    
    private void UpdateRotation()
    {
        // Simple rotation for prototype
        Vector3 newRotation = initialRotation;
        
        // Apply rotation based on lever type
        if (leverType == LeverType.LeftTrack || leverType == LeverType.RightTrack)
        {
            // Forward/backward rotation
            newRotation.x = initialRotation.x + normalizedValue * maxRotation;
        }
        else if (leverType == LeverType.TurretRotation)
        {
            // Left/right rotation
            newRotation.y = initialRotation.y + normalizedValue * maxRotation;
        }
        else if (leverType == LeverType.GunElevation)
        {
            // Up/down rotation
            newRotation.z = initialRotation.z + normalizedValue * maxRotation;
        }
        
        transform.localEulerAngles = newRotation;
    }
}