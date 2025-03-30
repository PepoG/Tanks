using UnityEngine;
using UnityEngine.Events;


public class ButtonInteractor : MonoBehaviour
{
    public enum ButtonType { Fire, Reset, Custom }

    [Header("Button Settings")]
    [SerializeField] private ButtonType buttonType = ButtonType.Fire;
    [SerializeField] private Transform buttonTransform;
    [SerializeField] private Vector3 pressDirection = Vector3.down;
    [SerializeField] private float pressDistance = 0.02f;
    [SerializeField] private float pressResistance = 50f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private bool toggleButton = false;

    [Header("References")]
    [SerializeField] private TankController tankController;

    [Header("Audio")]
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private AudioClip releaseSound;

    [Header("Events")]
    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonReleased;

    private Rigidbody buttonRigidbody;
    private ConfigurableJoint buttonJoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable buttonInteractable;
    private AudioSource audioSource;
    private Vector3 initialPosition;
    private Vector3 pressedPosition;
    private float currentPressAmount = 0f;
    private bool isPressed = false;
    private bool wasPressed = false;
    private bool isToggledOn = false;

    private void Awake()
    {
        // Initialize transform reference if not set
        if (buttonTransform == null)
        {
            buttonTransform = transform;
        }

        // Find tank controller if not set
        if (tankController == null && buttonType != ButtonType.Custom)
        {
            tankController = FindObjectOfType<TankController>();
            if (tankController == null)
            {
                Debug.LogWarning("No TankController found in scene. Button will not control the tank.");
            }
        }

        // Store initial position
        initialPosition = buttonTransform.localPosition;
        
        // Calculate pressed position based on press direction and distance
        pressedPosition = initialPosition + pressDirection.normalized * pressDistance;

        // Setup components
        SetupButtonRigidbody();
        SetupButtonJoint();
        SetupButtonInteractable();
        SetupAudioSource();
    }

    private void Update()
    {
        // Calculate how far the button is pressed (0 to 1)
        Vector3 directionToInitial = initialPosition - buttonTransform.localPosition;
        float distanceToInitial = Vector3.Dot(directionToInitial, pressDirection.normalized);
        currentPressAmount = Mathf.Clamp01(distanceToInitial / pressDistance);

        // Check if button state changed
        wasPressed = isPressed;
        isPressed = currentPressAmount > 0.9f;

        // Handle button state changes
        if (isPressed && !wasPressed)
        {
            OnButtonPress();
        }
        else if (!isPressed && wasPressed)
        {
            OnButtonRelease();
        }
    }

    private void OnButtonPress()
    {
        // Play press sound
        if (pressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        // Toggle state if it's a toggle button
        if (toggleButton)
        {
            isToggledOn = !isToggledOn;
        }

        // Handle special button types
        if (tankController != null)
        {
            switch (buttonType)
            {
                case ButtonType.Fire:
                    tankController.Fire();
                    break;
                case ButtonType.Reset:
                    // Implement reset functionality if needed
                    break;
            }
        }

        // Invoke events
        OnButtonPressed?.Invoke();
    }

    private void OnButtonRelease()
    {
        // Play release sound
        if (releaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }

        // Invoke events
        OnButtonReleased?.Invoke();
    }

    private void SetupButtonRigidbody()
    {
        buttonRigidbody = GetComponent<Rigidbody>();
        if (buttonRigidbody == null)
        {
            buttonRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        buttonRigidbody.mass = 1f;
        buttonRigidbody.drag = 10f;
        buttonRigidbody.angularDrag = 10f;
        buttonRigidbody.useGravity = false;
        buttonRigidbody.isKinematic = false;
        buttonRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        buttonRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void SetupButtonJoint()
    {
        buttonJoint = GetComponent<ConfigurableJoint>();
        if (buttonJoint == null)
        {
            buttonJoint = gameObject.AddComponent<ConfigurableJoint>();
        }

        // Set motion limits
        buttonJoint.xMotion = pressDirection.x != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
        buttonJoint.yMotion = pressDirection.y != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
        buttonJoint.zMotion = pressDirection.z != 0 ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
        
        // Lock rotation
        buttonJoint.angularXMotion = ConfigurableJointMotion.Locked;
        buttonJoint.angularYMotion = ConfigurableJointMotion.Locked;
        buttonJoint.angularZMotion = ConfigurableJointMotion.Locked;
        
        // Set limits based on press direction
        if (pressDirection.x != 0)
        {
            SoftJointLimit limit = new SoftJointLimit { limit = pressDistance };
            buttonJoint.linearLimit = limit;
        }
        else if (pressDirection.y != 0)
        {
            SoftJointLimit limit = new SoftJointLimit { limit = pressDistance };
            buttonJoint.linearLimit = limit;
        }
        else if (pressDirection.z != 0)
        {
            SoftJointLimit limit = new SoftJointLimit { limit = pressDistance };
            buttonJoint.linearLimit = limit;
        }
        
        // Configure spring for button return
        JointDrive drive = new JointDrive
        {
            positionSpring = pressResistance,
            positionDamper = pressResistance * 0.1f,
            maximumForce = float.MaxValue
        };
        
        buttonJoint.xDrive = drive;
        buttonJoint.yDrive = drive;
        buttonJoint.zDrive = drive;
    }

    private void SetupButtonInteractable()
    {
        buttonInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (buttonInteractable == null)
        {
            buttonInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
        
        // Add collider if needed
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    public bool IsPressed()
    {
        return isPressed;
    }
    
    public bool IsToggledOn()
    {
        return isToggledOn;
    }
}