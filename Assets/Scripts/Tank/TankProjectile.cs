using UnityEngine;

/// <summary>
/// Controls projectile behavior, collision and damage
/// </summary>
public class TankProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 50f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 1000f;
    
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    
    private bool hasExploded = false;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Add sphere collider if none exists
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
        }
    }
    
    private void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        // Align with velocity
        if (rb.velocity.sqrMagnitude > 1f)
        {
            transform.forward = rb.velocity.normalized;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        hasExploded = true;
        
        // Apply damage to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // Apply damage to destructible objects
            DestructibleTarget destructible = nearbyObject.GetComponent<DestructibleTarget>();
            if (destructible != null)
            {
                // Calculate damage based on distance
                float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
                float damagePercent = 1f - Mathf.Clamp01(distance / explosionRadius);
                float damageAmount = damage * damagePercent;
                
                destructible.TakeDamage(damageAmount);
            }
            
            // Add explosion force to rigidbodies
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        
        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        // Destroy projectile
        Destroy(gameObject);
    }
}