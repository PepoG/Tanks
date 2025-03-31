using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class TankDoorInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private GameObject doorMesh;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;
    private XROrigin xrOrigin;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isOpen = false;

    private void Awake()
    {
        xrOrigin = FindObjectOfType<XROrigin>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }

        interactable.selectEntered.AddListener(OnDoorInteracted);
    }

    private void OnDoorInteracted(SelectEnterEventArgs args)
    {
        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        if (doorMesh != null)
        {
            doorMesh.transform.Rotate(0, 90f, 0); // Simple open animation
        }

        audioSource?.PlayOneShot(openSound);

        if (xrOrigin != null && teleportTarget != null)
        {
            xrOrigin.transform.SetPositionAndRotation(
                teleportTarget.position,
                teleportTarget.rotation
            );
        }
    }

    private void CloseDoor()
    {
        isOpen = false;
        if (doorMesh != null)
        {
            doorMesh.transform.Rotate(0, -90f, 0); // Close animation
        }

        audioSource?.PlayOneShot(closeSound);
    }
}
