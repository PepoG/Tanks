using System.Collections;
using UnityEngine;


public class LeverFixedVersion : MonoBehaviour
{
    public enum LeverType { LeftTrack, RightTrack, TurretRotation, GunElevation }

    [Header("Lever Settings")]
    [SerializeField] private LeverType leverType = LeverType.LeftTrack;
    [SerializeField] private Transform leverTransform;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float maxRotation = 45f;
    [SerializeField] private bool returnToCenter = true;
    [SerializeField] private float returnSpeed = 2f;

    [Header("References")]
    [SerializeField] private TankController tankController;

    private Rigidbody leverRigidbody;
    private ConfigurableJoint leverJoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private float currentValue = 0f;

    private void Awake()
    {
        // Initialize transform reference if not set
        if (leverTransform == null)
        {
            leverTransform = transform;
        }

        // Find tank controller if not set
        if (tankController == null)
        {
            tankController = FindObjectOfType<TankController>();
            if (tankController == null)
            {
                Debug.LogWarning("No TankController found in scene. Lever will not control the tank.");
            }
        }

        // Setup components
        SetupComponents();
    }

    private void SetupComponents()
    {
        // Add rigidbody if needed
        leverRigidbody = GetComponent<Rigidbody>();
        if (leverRigidbody == null)
        {
            leverRigidbody = gameObject.AddComponent<Rigidbody>();
            leverRigidbody.mass = 2f;
            leverRigidbody.useGravity = false;
            leverRigidbody.isKinematic = false;
        }

        // Add configurable joint if needed
        leverJoint = GetComponent<ConfigurableJoint>();
        if (leverJoint == null)
        {
            leverJoint = gameObject.AddComponent<ConfigurableJoint>();
            leverJoint.connectedBody = null;
            leverJoint.anchor = Vector3.zero;
            leverJoint.axis = rotationAxis;

            // Lock all motion except rotation around the axis
            leverJoint.xMotion = ConfigurableJointMotion.Locked;
            leverJoint.yMotion = ConfigurableJointMotion.Locked;
            leverJoint.zMotion = ConfigurableJointMotion.Locked;
            leverJoint.angularXMotion = rotationAxis.x != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
            leverJoint.angularYMotion = rotationAxis.y != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
            leverJoint.angularZMotion = rotationAxis.z != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
        }

        // Add XR grab interactable if needed
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
            grabInteractable.throwOnDetach = false;
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = true;
        }

        // Add collider if needed
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.2f, 0.05f);
            collider.center = Vector3.up * 0.1f;
        }

        // Setup events
        grabInteractable.selectEntered.AddListener(args => isGrabbed = true);
        grabInteractable.selectExited.AddListener(args => isGrabbed = false);
    }

    private void Update()
    {
        // Calculate current lever value based on rotation
        CalculateLeverValue();

        // Auto-return to center if not grabbed and returnToCenter is enabled
        if (returnToCenter && !isGrabbed && Mathf.Abs(currentValue) > 0.01f)
        {
            currentValue = Mathf.Lerp(currentValue, 0f, Time.deltaTime * returnSpeed);
            UpdateLeverRotation();
        }

        // Update tank controller based on lever value
        if (tankController != null)
        {
            switch (leverType)
            {
                case LeverType.LeftTrack:
                    tankController.SetLeftTrackValue(currentValue);
                    break;
                case LeverType.RightTrack:
                    tankController.SetRightTrackValue(currentValue);
                    break;
                case LeverType.TurretRotation:
                    tankController.SetTurretRotationValue(currentValue);
                    break;
                case LeverType.GunElevation:
                    tankController.SetGunElevationValue(currentValue);
                    break;
            }
        }
    }

    private void CalculateLeverValue()
    {
        // Get the current rotation
        Vector3 currentRotation = leverTransform.localEulerAngles;
        float angle = 0f;
        
        // Extract the angle around the appropriate axis
        if (rotationAxis.x != 0) angle = NormalizeAngle(currentRotation.x);
        else if (rotationAxis.y != 0) angle = NormalizeAngle(currentRotation.y);
        else if (rotationAxis.z != 0) angle = NormalizeAngle(currentRotation.z);
        
        // Normalize to -1 to 1 range
        currentValue = Mathf.Clamp(angle / maxRotation, -1f, 1f);
    }

    private float NormalizeAngle(float angle)
    {
        // Convert angle from 0-360 to -180 to 180
        if (angle > 180) angle -= 360;
        return angle;
    }

    private void UpdateLeverRotation()
    {
        // Apply the current value to lever rotation
        Quaternion targetRotation = Quaternion.identity;
        
        if (rotationAxis.x != 0)
            targetRotation = Quaternion.Euler(currentValue * maxRotation, 0, 0);
        else if (rotationAxis.y != 0)
            targetRotation = Quaternion.Euler(0, currentValue * maxRotation, 0);
        else if (rotationAxis.z != 0)
            targetRotation = Quaternion.Euler(0, 0, currentValue * maxRotation);
            
        leverTransform.localRotation = Quaternion.Slerp(
            leverTransform.localRotation, 
            targetRotation, 
            Time.deltaTime * returnSpeed * 2
        );
    }
}