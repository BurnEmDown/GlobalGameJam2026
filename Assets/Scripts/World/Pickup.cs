using UnityEngine;
using UnityEngine.Animations;

namespace World
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private AimConstraint aimConstraint;
        
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
            Debug.Log("Collecting");
            GameManager.instance.cleaner.CLearHalfOfAllFlakes();
        
            // Spawn particle effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            
            // Return to pool or destroy
            gameObject.SetActive(false);
        }

        public void SetLookAt(Transform lookAt)
        {
            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = lookAt,
                weight = 1f
            };
            aimConstraint.AddSource(source);
            aimConstraint.constraintActive = true;
        }
    }
}