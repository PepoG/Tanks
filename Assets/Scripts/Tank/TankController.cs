using System;
using UnityEngine;

/// <summary>
/// Main controller for the tank. Manages movement, turret control, and firing.
/// </summary>
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float rotationSpeed = 50f;
    
    [Header("Turret")]
    [SerializeField] private Transform turretTransform;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private float turretRotationSpeed = 30f;
    [SerializeField] private float gunElevationSpeed = 20f;
    [SerializeField] private float maxGunElevation = 30f;
    [SerializeField] private float minGunElevation = -10f;
    
    [Header("Firing")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float reloadTime = 2f;
    
    // Control values from levers
    private float leftTrackValue = 0f;
    private float rightTrackValue = 0f;
    private float turretRotationValue = 0f;
    private float gunElevationValue = 0f;
    
    // Firing state
    private bool canFire = true;
    private float reloadTimer = 0f;
    
    private Rigidbody tankRigidbody;
    
    // Events
    public event Action OnFire;
    
    private void Awake()
    {
        tankRigidbody = GetComponent<Rigidbody>();
        
        // Add Rigidbody if it doesn't exist
        if (tankRigidbody == null)
        {
            tankRigidbody = gameObject.AddComponent<Rigidbody>();
            tankRigidbody.mass = 1000f;
            tankRigidbody.drag = 1f;
            tankRigidbody.angularDrag = 1f;
        }
    }
    
    private void Update()
    {
        // Update turret rotation based on control value
        if (turretTransform != null)
        {
            Vector3 turretRotation = turretTransform.localEulerAngles;
            turretRotation.y += turretRotationValue * turretRotationSpeed * Time.deltaTime;
            turretTransform.localEulerAngles = turretRotation;
        }
        
        // Update gun elevation based on control value
        if (gunTransform != null)
        {
            Vector3 gunRotation = gunTransform.localEulerAngles;
            
            // Convert to -180 to 180 range for easier clamping
            float currentXRotation = gunRotation.x;
            if (currentXRotation > 180f)
                currentXRotation -= 360f;
            
            // Apply new rotation with clamping
            float newXRotation = Mathf.Clamp(
                currentXRotation - gunElevationValue * gunElevationSpeed * Time.deltaTime,
                minGunElevation,
                maxGunElevation
            );
            
            gunRotation.x = newXRotation;
            gunTransform.localEulerAngles = gunRotation;
        }
        
        // Handle reload timer
        if (!canFire)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                canFire = true;
                reloadTimer = 0f;
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Calculate movement based on track values
        float forwardValue = (leftTrackValue + rightTrackValue) * 0.5f;
        float turnValue = (rightTrackValue - leftTrackValue) * 0.5f;
        
        // Apply movement
        Vector3 movement = transform.forward * forwardValue * maxSpeed;
        tankRigidbody.velocity = movement;
        
        // Apply rotation
        float rotation = turnValue * rotationSpeed;
        tankRigidbody.angularVelocity = new Vector3(0f, rotation * Mathf.Deg2Rad, 0f);
    }
    
    /// <summary>
    /// Sets the value for the left track control (-1 to 1)
    /// </summary>
    public void SetLeftTrackValue(float value)
    {
        leftTrackValue = Mathf.Clamp(value, -1f, 1f);
    }
    
    /// <summary>
    /// Sets the value for the right track control (-1 to 1)
    /// </summary>
    public void SetRightTrackValue(float value)
    {
        rightTrackValue = Mathf.Clamp(value, -1f, 1f);
    }
    
    /// <summary>
    /// Sets the value for the turret rotation control (-1 to 1)
    /// </summary>
    public void SetTurretRotationValue(float value)
    {
        turretRotationValue = Mathf.Clamp(value, -1f, 1f);
    }
    
    /// <summary>
    /// Sets the value for the gun elevation control (-1 to 1)
    /// </summary>
    public void SetGunElevationValue(float value)
    {
        gunElevationValue = Mathf.Clamp(value, -1f, 1f);
    }
    
    /// <summary>
    /// Fires a projectile if the tank is ready to fire
    /// </summary>
    public void Fire()
    {
        if (!canFire || projectileSpawnPoint == null || projectilePrefab == null)
            return;
        
        // Create projectile
        GameObject projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation
        );
        
        // Add force to projectile
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.velocity = projectileSpawnPoint.forward * projectileSpeed;
        }
        
        // Start reload timer
        canFire = false;
        reloadTimer = reloadTime;
        
        // Invoke fire event
        OnFire?.Invoke();
    }
}