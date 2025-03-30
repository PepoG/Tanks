using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
        // Set up audio source if needed
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Find XR Origin if not assigned
        if (xrOrigin == null)
        {
            var originObject = GameObject.FindObjectsOfType<XROrigin>();
            if (originObject.Length > 0)
            {
                xrOrigin = originObject[0].transform;
            }
            else
            {
                Debug.LogWarning("No XR Origin found in scene. Teleportation may not work correctly.");
            }
        }
    }

    public void TeleportIntoTank()
    {
        if (insidePosition != null && xrOrigin != null)
        {
            StartCoroutine(TeleportRoutine(insidePosition));
        }
        else
        {
            Debug.LogError("Cannot teleport: Missing inside position or XR Origin");
        }
    }

    public void TeleportOutOfTank()
    {
        if (outsidePosition != null && xrOrigin != null)
        {
            StartCoroutine(TeleportRoutine(outsidePosition));
        }
        else
        {
            Debug.LogError("Cannot teleport: Missing outside position or XR Origin");
        }
    }

    private IEnumerator TeleportRoutine(Transform destination)
    {
        // Play sound
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
        
        // Fade out (you may need to reference your fade system)
        // TODO: Implement or reference scene fade system
        
        // Wait for fade
        yield return new WaitForSeconds(teleportDelay);
        
        // Teleport player
        Vector3 heightAdjust = new Vector3(0, xrOrigin.position.y - xrOrigin.GetComponentInChildren<Camera>().transform.position.y, 0);
        xrOrigin.position = destination.position + heightAdjust;
        xrOrigin.rotation = destination.rotation;
        
        // Fade in
        // TODO: Implement or reference scene fade system
        
        yield return new WaitForSeconds(teleportFadeTime);
    }
}