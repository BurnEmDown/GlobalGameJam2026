using System;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;

namespace World
{
    public class DespawnZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        public float distanceBehindPlayer = 50f;  // How far behind player to position the zone
        public Vector3 triggerSize = new Vector3(20f, 10f, 5f);  // Size of the trigger zone

        [SerializeField] private GameObject playerObject;
        [SerializeField] private ChunkManager ChunkManager;
        private Vector3 playerPosition;
        private IPoolManager poolingService;
        private BoxCollider triggerCollider;
    
        void Start()
        {
            // Get pooling service
            poolingService = CoreServices.Get<IPoolManager>();
            playerPosition = playerObject.transform.position;
        }
    
        void OnTriggerEnter(Collider other)
        {
            // Check if the object is an obstacle or pickup that should be despawned
            if (other.CompareTag("Obstacle"))
            {
                // Determine pool key based on object name or component
                string poolKey = DeterminePoolKey(other.gameObject);
                if (!string.IsNullOrEmpty(poolKey))
                {
                    poolingService.ReturnToPool<Obstacle>(poolKey, other.GetComponent<Obstacle>());
                }
            }
            else if (other.CompareTag("Pickup"))
            {
                // Return pickup to pool
                poolingService.ReturnToPool<Pickup>("HotPickup", other.GetComponent<Pickup>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Terrain"))
            {
                ChunkManager.DespawnOldChunk();
                ChunkManager.SpawnNewChunk();
            }
        }

        private string DeterminePoolKey(GameObject obj)
        {
            // Check object name or components to determine which pool it belongs to
            if (obj.name.Contains("Tree")) return "Tree";
            if (obj.name.Contains("Rock")) return "Rock";
            if (obj.name.Contains("TerrainChunk")) return "TerrainChunk";
        
            // Fallback: check for specific components
            // You can add custom logic here based on your prefab structure
            return null;
        }
    }
}