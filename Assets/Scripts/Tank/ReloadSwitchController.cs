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

    private Rigidbody switchRigidbody;
    private ConfigurableJoint switchJoint;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private AudioSource audioSource;

    private bool isGrabbed = false;
    private bool isActivated = false;

    public event Action OnSwitchActivated;
    public event Action OnSwitchDeactivated;

    private void Awake()
    {
        if (switchTransform == null)
            switchTransform = transform;

        if (tankController == null)
            tankController = FindObjectOfType<TankController>();

        SetupSwitchRigidbody();
        SetupSwitchJoint();
        SetupGrabInteractable();
        SetupAudioSource();
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

        switchJoint.xMotion = ConfigurableJointMotion.Locked;
        switchJoint.yMotion = ConfigurableJointMotion.Locked;
        switchJoint.zMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularXMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularYMotion = ConfigurableJointMotion.Locked;
        switchJoint.angularZMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit { limit = rotationLimit };
        switchJoint.angularZLimit = limit;

        JointDrive yzDrive = new JointDrive
        {
            positionSpring = 100f,
            positionDamper = 5f,
            maximumForce = 100f
        };
        switchJoint.angularYZDrive = yzDrive;

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
        float currentZ = switchTransform.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        bool wasActivated = isActivated;
        isActivated = currentZ > (rotationLimit / 2f);

        if (wasActivated != isActivated)
        {
            if (isActivated)
            {
                OnSwitchActivated?.Invoke();
                tankController?.SendMessage("ReloadAmmo", SendMessageOptions.DontRequireReceiver);
                audioSource?.PlayOneShot(switchOnSound);
            }
            else
            {
                OnSwitchDeactivated?.Invoke();
                audioSource?.PlayOneShot(switchOffSound);
            }
        }
    }
}
