using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Makes an object destructible when hit by projectiles
/// </summary>
public class DestructibleTarget : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private bool breakIntoChunks = true;
    
    [SerializeField] private GameObject destroyedPrefab;
    [SerializeField] private GameObject destroyedEffect;
    [SerializeField] private AudioClip destroyedSound;
    [SerializeField] private Material damagedMaterial;
    
    private float currentHealth;
    private bool isDestroyed = false;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    
    public UnityEvent OnDamaged;
    public UnityEvent OnDestroyed;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
    }
    
    /// <summary>
    /// Apply damage to the target
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDestroyed)
            return;
            
        currentHealth -= amount;
        
        // Apply damage visuals
        ApplyDamageVisuals();
        
        // Invoke damage event
        OnDamaged?.Invoke();
        
        // Check if destroyed
        if (currentHealth <= 0)
        {
            DestroyTarget();
        }
    }
    
    private void ApplyDamageVisuals()
    {
        // Change material based on damage
        if (meshRenderer != null && damagedMaterial != null)
        {
            float damagePercent = 1f - (currentHealth / maxHealth);
            if (damagePercent > 0.5f && meshRenderer.material != damagedMaterial)
            {
                meshRenderer.material = damagedMaterial;
            }
        }
    }
    
    private void DestroyTarget()
    {
        isDestroyed = true;
        
        // Spawn destroyed effect
        if (destroyedEffect != null)
        {
            Instantiate(destroyedEffect, transform.position, transform.rotation);
        }
        
        // Play destroyed sound
        if (destroyedSound != null)
        {
            AudioSource.PlayClipAtPoint(destroyedSound, transform.position);
        }
        
        // Spawn destroyed prefab
        if (destroyedPrefab != null)
        {
            Instantiate(destroyedPrefab, transform.position, transform.rotation);
        }
        
        // Handle destruction based on settings
        if (breakIntoChunks)
        {
            // TODO: Break into chunks using mesh cutting
            // For prototype, just make physics-enabled chunks
            if (usePhysics)
            {
                CreatePhysicsChunks();
            }
        }
        
        // Invoke destroyed event
        OnDestroyed?.Invoke();
        
        // Destroy original object
        Destroy(gameObject);
    }
    
    private void CreatePhysicsChunks()
    {
        // Create simple physics chunks for prototype
        for (int i = 0; i < 5; i++)
        {
            GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            chunk.transform.rotation = Random.rotation;
            chunk.transform.localScale = transform.localScale * 0.3f;
            
            if (damagedMaterial != null)
            {
                chunk.GetComponent<MeshRenderer>().material = damagedMaterial;
            }
            
            Rigidbody rb = chunk.AddComponent<Rigidbody>();
            rb.AddExplosionForce(500f, transform.position, 2f);
            
            // Destroy chunk after a few seconds
            Destroy(chunk, 5f);
        }
    }
    
    /// <summary>
    /// Reset the target to its original state
    /// </summary>
    public void Reset()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
        
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }
}