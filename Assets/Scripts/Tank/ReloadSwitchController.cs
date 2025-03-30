using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReloadSwitchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankController tankController;
    [SerializeField] private Transform switchTransform;

    [Header("Switch Settings")]
    [SerializeField] private float rotationLimit = 90f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float switchResistance = 5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip switchOnSound;
    [SerializeField] private AudioClip switchOffSound;
    
    // Components
    private Rigidbody switchRigidbody;
    private ConfigurableJoint switchJoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    
    // State tracking
    private bool isGrabbed = false;
    private bool isActivated = false;
    private Quaternion initialRotation;
    
    public event Action OnSwitchActivated;
    public event Action OnSwitchDeactivated;
    
    private void Awake()
    {
        // Get or create references
        if (switchTransform == null)
            switchTransform = transform;
            
        if (tankController == null)
            tankController = FindObjectOfType<TankController>();
            
        // Setup components
        SetupSwitchRigidbody();
        SetupSwitchJoint();
        SetupGrabInteractable();
        SetupAudioSource();
        
        initialRotation = switchTransform.localRotation;
    }
    
    private void SetupSwitchRigidbody()
    {
        switchRigidbody = GetComponent<Rigidbody>();
        if (switchRigidbody == null)
            switchRigidbody = gameObject.AddComponent<Rigidbody>();
            
        switchRigidbody.mass = 1f;
        switchRigidbody.drag = switchResistance;
        switchRigidbody.angularDrag = switchResistance;
        switchRigidbody.useGravity = false;
        switchRigidbody.isKinematic = false;
        switchRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        switchRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        switchRigidbody.constraints = RigidbodyConstraints.FreezePosition | 
                                     RigidbodyConstraints.FreezeRotationX | 
                                     RigidbodyConstraints.FreezeRotationY;
    }
    
    private void SetupSwitchJoint()
    {
        switchJoint = GetComponent<ConfigurableJoint>();
        if (switchJoint == null)
            switchJoint = gameObject.AddComponent<ConfigurableJoint>();
            
        // Lock position and unwanted rotation axes
        switchJoint.xMotion = ConfigurableJointMotion.Locked;
        switchJoint.yMotion = ConfigurableJointMotion.Locked;
        switchJoint.zMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularXMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularYMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularZMotion = ConfigurableJointMotion.Limited;
        
        // Set rotation limits
        SoftJointLimit lowLimit = new SoftJointLimit();
        lowLimit.limit = -rotationLimit;
        switchJoint.lowAngularZLimit = lowLimit;
        
        SoftJointLimit highLimit = new SoftJointLimit();
        highLimit.limit = rotationLimit;
        switchJoint.highAngularZLimit = highLimit;
        
        // Spring settings to return to center
        JointDrive drive = new JointDrive();
        drive.positionSpring = 1f;
        drive.positionDamper = 0.2f;
        drive.maximumForce = 1f;
        switchJoint.angularZDrive = drive;
        
        // Set the joint's reference rotation to current rotation
        switchJoint.targetRotation = Quaternion.identity;
    }
    
    private void SetupGrabInteractable()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
        grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.throwOnDetach = false;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.smoothPosition = true;
        grabInteractable.smoothRotation = true;
        
        // Set up events
        grabInteractable.selectEntered.AddListener(OnSwitchGrabbed);
        grabInteractable.selectExited.AddListener(OnSwitchReleased);
    }
    
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = 0.7f;
    }
    
    private void OnSwitchGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }
    
    private void OnSwitchReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }
    
    private void Update()
    {
        CheckSwitchRotation();
    }
    
    private void CheckSwitchRotation()
    {
        // Get the current rotation around Z axis
        float currentRotationZ = transform.localEulerAngles.z;
        if (currentRotationZ > 180f)
            currentRotationZ -= 360f;
            
        // Check if the switch has been activated (rotated past halfway)
        bool wasActivated = isActivated;
        isActivated = currentRotationZ > (rotationLimit / 2);
        
        // If state has changed
        if (wasActivated != isActivated)
        {
            if (isActivated)
            {
                // Switch turned on
                OnSwitchActivated?.Invoke();
                if (tankController != null)
                    tankController.ReloadAmmo();
                    
                if (switchOnSound != null && audioSource != null)
                    audioSource.PlayOneShot(switchOnSound);
            }
            else
            {
                // Switch turned off
                OnSwitchDeactivated?.Invoke();
                
                if (switchOffSound != null && audioSource != null)
                    audioSource.PlayOneShot(switchOffSound);
            }
        }
    }
}