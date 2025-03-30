using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TankEnterExitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerSeatPosition;
    [SerializeField] private GameObject tankInterior;
    [SerializeField] private GameObject entryDoor;
    [SerializeField] private GameObject exitDoor;
    
    private bool isPlayerInside = false;
    private XRRig playerRig;
    
    private void Awake()
    {
        // Find XR Rig
        playerRig = FindObjectOfType<XRRig>();
        
        // Set up entry door
        if (entryDoor != null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable entryInteractable = entryDoor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (entryInteractable == null)
            {
                entryInteractable = entryDoor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }
            entryInteractable.selectEntered.AddListener(args => EnterTank());
        }
        
        // Set up exit door
        if (exitDoor != null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable exitInteractable = exitDoor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (exitInteractable == null)
            {
                exitInteractable = exitDoor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }
            exitInteractable.selectEntered.AddListener(args => ExitTank());
        }
    }
    
    public void EnterTank()
    {
        if (isPlayerInside) return;
        
        if (playerSeatPosition != null && playerRig != null)
        {
            // Teleport player to seat position
            StartCoroutine(TeleportPlayer(playerSeatPosition.position, playerSeatPosition.rotation));
            isPlayerInside = true;
            
            // Show interior
            if (tankInterior != null)
            {
                tankInterior.SetActive(true);
            }
            
            Debug.Log("Player entered tank at " + Time.time);
        }
    }
    
    public void ExitTank()
    {
        if (!isPlayerInside) return;
        
        if (playerRig != null && exitDoor != null)
        {
            // Teleport player outside
            Vector3 exitPosition = exitDoor.transform.position - exitDoor.transform.forward * 1.5f;
            Quaternion exitRotation = Quaternion.LookRotation(-exitDoor.transform.forward);
            
            StartCoroutine(TeleportPlayer(exitPosition, exitRotation));
            isPlayerInside = false;
            
            // Hide interior
            if (tankInterior != null)
            {
                tankInterior.SetActive(false);
            }
            
            Debug.Log("Player exited tank at " + Time.time);
        }
    }
    
    private IEnumerator TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        // Fade out (in a real implementation)
        // yield return FadeScreen(0f, 1f);
        
        // Wait a frame
        yield return null;
        
        // Teleport
        playerRig.transform.position = position;
        playerRig.transform.rotation = rotation;
        
        // Wait a frame
        yield return null;
        
        // Fade in (in a real implementation)
        // yield return FadeScreen(1f, 0f);
    }
}