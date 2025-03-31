using System.Collections;
using UnityEngine;

using Unity.XR.CoreUtils;

public class TankEnterExitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerSeatPosition;
    [SerializeField] private GameObject tankInterior;
    [SerializeField] private GameObject entryDoor;
    [SerializeField] private GameObject exitDoor;

    private bool isPlayerInside = false;
    private XROrigin playerOrigin;

    private void Awake()
    {
        // Find XR Origin
        playerOrigin = FindObjectOfType<XROrigin>();

        // Set up entry door
        if (entryDoor != null)
        {
            var entryInteractable = entryDoor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (entryInteractable == null)
                entryInteractable = entryDoor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

            entryInteractable.selectEntered.AddListener(args => EnterTank());
        }

        // Set up exit door
        if (exitDoor != null)
        {
            var exitInteractable = exitDoor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (exitInteractable == null)
                exitInteractable = exitDoor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

            exitInteractable.selectEntered.AddListener(args => ExitTank());
        }
    }

    public void EnterTank()
    {
        if (isPlayerInside || playerSeatPosition == null || playerOrigin == null) return;

        StartCoroutine(TeleportPlayer(playerSeatPosition.position, playerSeatPosition.rotation));
        isPlayerInside = true;

        if (tankInterior != null)
            tankInterior.SetActive(true);

        Debug.Log("Player entered tank at " + Time.time);
    }

    public void ExitTank()
    {
        if (!isPlayerInside || playerOrigin == null || exitDoor == null) return;

        Vector3 exitPosition = exitDoor.transform.position - exitDoor.transform.forward * 1.5f;
        Quaternion exitRotation = Quaternion.LookRotation(-exitDoor.transform.forward);

        StartCoroutine(TeleportPlayer(exitPosition, exitRotation));
        isPlayerInside = false;

        if (tankInterior != null)
            tankInterior.SetActive(false);

        Debug.Log("Player exited tank at " + Time.time);
    }

    private IEnumerator TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        yield return null;

        playerOrigin.transform.SetPositionAndRotation(position, rotation);

        yield return null;
    }
}
