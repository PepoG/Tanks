using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class TeleportManager : MonoBehaviour
{
    [Header("Teleport Points")]
    [SerializeField] private Transform insidePosition;
    [SerializeField] private Transform outsidePosition;

    [Header("Player References")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private float teleportFadeTime = 0.25f;
    [SerializeField] private float teleportDelay = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip teleportSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (xrOrigin == null)
        {
            var originObject = FindObjectOfType<XROrigin>();
            if (originObject != null)
            {
                xrOrigin = originObject.transform;
            }
            else
            {
                Debug.LogWarning("No XR Origin found in scene. Teleportation may not work.");
            }
        }
    }

    public void TeleportIntoTank()
    {
        if (insidePosition != null && xrOrigin != null)
            StartCoroutine(TeleportRoutine(insidePosition));
        else
            Debug.LogError("TeleportIntoTank failed: Missing references.");
    }

    public void TeleportOutOfTank()
    {
        if (outsidePosition != null && xrOrigin != null)
            StartCoroutine(TeleportRoutine(outsidePosition));
        else
            Debug.LogError("TeleportOutOfTank failed: Missing references.");
    }

    private IEnumerator TeleportRoutine(Transform destination)
    {
        if (teleportSound != null && audioSource != null)
            audioSource.PlayOneShot(teleportSound);

        yield return new WaitForSeconds(teleportDelay);

        Vector3 headOffset = xrOrigin.position - xrOrigin.GetComponentInChildren<Camera>().transform.position;
        xrOrigin.position = destination.position + headOffset;
        xrOrigin.rotation = destination.rotation;

        yield return new WaitForSeconds(teleportFadeTime);
    }
}
