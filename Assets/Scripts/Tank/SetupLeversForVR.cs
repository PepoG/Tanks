using UnityEngine;

public class SetupLeversForVR : MonoBehaviour
{
    public void SetupLevers()
    {
        // Find all levers
        GameObject leftLever = GameObject.Find("LeftTrackLever");
        GameObject rightLever = GameObject.Find("RightTrackLever");
        GameObject horizontalLever = GameObject.Find("HorizontalTurretLever");
        GameObject verticalLever = GameObject.Find("VerticalTurretLever");
        GameObject fireButton = GameObject.Find("FireButton");
        
        // Setup components for each lever
        if (leftLever) AddLeverComponents(leftLever);
        if (rightLever) AddLeverComponents(rightLever);
        if (horizontalLever) AddLeverComponents(horizontalLever);
        if (verticalLever) AddLeverComponents(verticalLever);
        if (fireButton) AddButtonComponents(fireButton);
        
        // Find doors
        GameObject entryDoor = GameObject.Find("TankEntryDoor");
        GameObject exitDoor = GameObject.Find("TankExitDoor");
        
        // Setup components for doors
        if (entryDoor) AddDoorComponents(entryDoor);
        if (exitDoor) AddDoorComponents(exitDoor);
        
        Debug.Log("VR components setup complete!");
    }
    
    private void AddLeverComponents(GameObject lever)
    {
        // Add a box collider if needed
        if (!lever.GetComponent<BoxCollider>())
        {
            BoxCollider collider = lever.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.2f, 0.05f);
            collider.center = Vector3.up * 0.1f;
        }
        
        // Add a rigidbody if needed
        if (!lever.GetComponent<Rigidbody>())
        {
            Rigidbody rb = lever.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Add a configurable joint if needed
        if (!lever.GetComponent<ConfigurableJoint>())
        {
            ConfigurableJoint joint = lever.AddComponent<ConfigurableJoint>();
            
            // Configure the joint
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            // Set up rotation limits
            joint.lowAngularXLimit = new SoftJointLimit { limit = -45f };
            joint.highAngularXLimit = new SoftJointLimit { limit = 45f };
            
            // Add spring for auto-centering
            JointDrive drive = new JointDrive
            {
                positionSpring = 50f,
                positionDamper = 5f,
                maximumForce = float.MaxValue
            };
            
            joint.angularXDrive = drive;
        }
    }
    
    private void AddButtonComponents(GameObject button)
    {
        // Add a box collider if needed
        if (!button.GetComponent<BoxCollider>())
        {
            BoxCollider collider = button.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.1f, 0.1f, 0.1f);
        }
        
        // Add a rigidbody if needed
        if (!button.GetComponent<Rigidbody>())
        {
            Rigidbody rb = button.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Add a configurable joint if needed
        if (!button.GetComponent<ConfigurableJoint>())
        {
            ConfigurableJoint joint = button.AddComponent<ConfigurableJoint>();
            
            // Configure the joint for button press
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            // Set up linear limits
            joint.linearLimit = new SoftJointLimit { limit = 0.02f };
            
            // Add spring for button return
            JointDrive drive = new JointDrive
            {
                positionSpring = 100f,
                positionDamper = 10f,
                maximumForce = float.MaxValue
            };
            
            joint.yDrive = drive;
        }
    }
    
    private void AddDoorComponents(GameObject door)
    {
        // Add a box collider if needed
        if (!door.GetComponent<BoxCollider>())
        {
            BoxCollider collider = door.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 1f, 0.2f);
        }
    }
}