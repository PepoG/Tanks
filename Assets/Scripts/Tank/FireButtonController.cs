using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controls the fire button for the tank
/// </summary>
public class FireButtonController : MonoBehaviour
{
    [SerializeField] private TankController tankController;
    [SerializeField] private float pressDistance = 0.05f;
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private AudioClip buttonReleaseSound;
    
    private Vector3 initialPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Get tank controller if not set
        if (tankController == null)
            tankController = FindObjectOfType<TankController>();
            
        // Set up initial and pressed positions
        initialPosition = transform.localPosition;
        pressedPosition = initialPosition - new Vector3(0, pressDistance, 0);
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (buttonPressSound != null || buttonReleaseSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    /// <summary>
    /// Called when button is pressed
    /// </summary>
    public void OnPress()
    {
        if (isPressed)
            return;
            
        isPressed = true;
        transform.localPosition = pressedPosition;
        
        // Play sound
        if (audioSource != null && buttonPressSound != null)
            audioSource.PlayOneShot(buttonPressSound);
        
        // Fire tank
        if (tankController != null)
            tankController.Fire();
    }
    
    /// <summary>
    /// Called when button is released
    /// </summary>
    public void OnRelease()
    {
        if (!isPressed)
            return;
            
        isPressed = false;
        transform.localPosition = initialPosition;
        
        // Play sound
        if (audioSource != null && buttonReleaseSound != null)
            audioSource.PlayOneShot(buttonReleaseSound);
    }
}