using UnityEngine;


public class SimpleVRSetup : MonoBehaviour
{
    [SerializeField] private Transform leftTrackLever;
    [SerializeField] private Transform rightTrackLever;
    [SerializeField] private Transform horizontalTurretLever;
    [SerializeField] private Transform verticalTurretLever;
    [SerializeField] private Transform fireButton;
    [SerializeField] private Transform tankEntryDoor;
    [SerializeField] private Transform tankExitDoor;
    [SerializeField] private TankController tankController;

    private void Awake()
    {
        FindObjectsIfNeeded();
        SetupScene();
    }

    private void FindObjectsIfNeeded()
    {
        if (leftTrackLever == null)
            leftTrackLever = GameObject.Find("LeftTrackLever")?.transform;

        if (rightTrackLever == null)
            rightTrackLever = GameObject.Find("RightTrackLever")?.transform;

        if (horizontalTurretLever == null)
            horizontalTurretLever = GameObject.Find("HorizontalTurretLever")?.transform;

        if (verticalTurretLever == null)
            verticalTurretLever = GameObject.Find("VerticalTurretLever")?.transform;

        if (fireButton == null)
            fireButton = GameObject.Find("FireButton")?.transform;

        if (tankEntryDoor == null)
            tankEntryDoor = GameObject.Find("TankEntryDoor")?.transform;

        if (tankExitDoor == null)
            tankExitDoor = GameObject.Find("TankExitDoor")?.transform;

        if (tankController == null)
            tankController = FindObjectOfType<TankController>();
    }

    private void SetupScene()
    {
        // Setup levers
        if (leftTrackLever != null)
            SetupLever(leftTrackLever.gameObject, true);

        if (rightTrackLever != null)
            SetupLever(rightTrackLever.gameObject, true);

        if (horizontalTurretLever != null)
            SetupLever(horizontalTurretLever.gameObject, true);

        if (verticalTurretLever != null)
            SetupLever(verticalTurretLever.gameObject, true);

        // Setup fire button
        if (fireButton != null)
            SetupButton(fireButton.gameObject);

        // Setup doors
        if (tankEntryDoor != null)
            SetupDoor(tankEntryDoor.gameObject);

        if (tankExitDoor != null)
            SetupDoor(tankExitDoor.gameObject);
    }

    private void SetupLever(GameObject leverObject, bool addJoint = true)
    {
        // Add BoxCollider if needed
        if (leverObject.GetComponent<BoxCollider>() == null)
        {
            BoxCollider collider = leverObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.2f, 0.05f); // Adjust based on your model
            collider.center = Vector3.up * 0.1f;
        }

        // Add Rigidbody if needed
        if (leverObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = leverObject.AddComponent<Rigidbody>();
            rb.mass = 1.0f;
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Add XRGrabInteractable if needed
        if (leverObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = leverObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            interactable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
            interactable.trackPosition = false;
            interactable.trackRotation = true;
            interactable.throwOnDetach = false;
        }

        // Add joint if needed and requested
        if (addJoint && leverObject.GetComponent<ConfigurableJoint>() == null)
        {
            ConfigurableJoint joint = leverObject.AddComponent<ConfigurableJoint>();
            
            // Lock all degrees of freedom except rotation around x-axis
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            // Set up rotation limits (±45 degrees)
            joint.lowAngularXLimit = new SoftJointLimit { limit = -45f };
            joint.highAngularXLimit = new SoftJointLimit { limit = 45f };
            
            // Add some resistance and return force
            JointDrive drive = new JointDrive
            {
                positionSpring = 50f,
                positionDamper = 5f,
                maximumForce = 1000f
            };
            
            joint.angularXDrive = drive;
        }
    }

    private void SetupButton(GameObject buttonObject)
    {
        // Add BoxCollider if needed
        if (buttonObject.GetComponent<BoxCollider>() == null)
        {
            BoxCollider collider = buttonObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.05f, 0.05f); // Adjust based on your model
        }

        // Add Rigidbody if needed
        if (buttonObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = buttonObject.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Add XRSimpleInteractable if needed
        if (buttonObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>() == null)
        {
            buttonObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }

        // Add joint for button press motion
        if (buttonObject.GetComponent<ConfigurableJoint>() == null)
        {
            ConfigurableJoint joint = buttonObject.AddComponent<ConfigurableJoint>();
            
            // Allow only y-axis motion for pressing
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            // Set up linear limits (2cm press distance)
            joint.linearLimit = new SoftJointLimit { limit = 0.02f };
            
            // Add resistance and return force
            JointDrive drive = new JointDrive
            {
                positionSpring = 100f,
                positionDamper = 10f,
                maximumForce = 1000f
            };
            
            joint.yDrive = drive;
        }
    }

    private void SetupDoor(GameObject doorObject)
    {
        // Add BoxCollider if needed
        if (doorObject.GetComponent<BoxCollider>() == null)
        {
            BoxCollider collider = doorObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.8f, 1.2f, 0.05f); // Adjust based on your model
        }

        // Add XRSimpleInteractable for door interaction
        if (doorObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>() == null)
        {
            doorObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
    }
}