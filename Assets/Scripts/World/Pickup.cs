using UnityEngine;

namespace World
{
    public class Pickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        public float frostReduction = 0.25f;    // How much frost to remove
    
        [Header("Effects")]
        public GameObject collectEffect;         // Particle prefab (optional)
        public AudioClip collectSound;           // Collect sound (assign later)
    
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }
    
        void Collect()
        {
            // TODO: use pooling service for effects and sounds
            
            // Notify FrostManager (Person C will implement this)
            // FrostManager.Instance.ReduceFrost(frostReduction);
        
            // Spawn particle effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
        
            
            // Return to pool or destroy
            gameObject.SetActive(false);
        }
    }
}