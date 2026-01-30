using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;

namespace World
{
    public class ProceduralSpawner : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        
        [Header("Lane Settings")]
        public float[] lanes = { -8f, -4f, 0f, 4f, 8f };  // X positions for lanes
    
        [Header("Obstacle Settings")]
        public int minObstaclesPerChunk = 5;
        public int maxObstaclesPerChunk = 15;
        public float obstacleDensity = 1f;     // Multiplier (modified by DifficultyManager)
        public float obstacleHeight = 0f;       // Y position (ground level)
        public GameObject obstacleParent;      // Parent object for obstacles
    
        [Header("Pickup Settings")]
        public int minPickupsPerChunk = 2;
        public int maxPickupsPerChunk = 5;
        public float pickupHeight = 0.5f;       // Slightly above ground
        public GameObject pickupParent;        // Parent object for pickups
    
        [Header("Prefab References")]
        public Obstacle treePrefab;
        public GameObject rockPrefab;
        public Pickup hotPickupPrefab;
    
        [Header("Spacing")]
        public float minSpacing = 15f;          // Minimum distance between objects in same lane
    
        private IPoolManager poolingService;
    
        void Start()
        {
            InitializePoolingService();
        }
        
        private void InitializePoolingService()
        {
            if (poolingService != null) return; // Already initialized
    
            // Get the Pooling Service from UnityCoreKit
            poolingService = CoreServices.Get<IPoolManager>();

            // Register prefabs with the pooling service
            poolingService.InitPool<Obstacle>("Tree", 20);
            poolingService.InitPool<Pickup>("HotPickup", 20);
        }
    
        public void PopulateChunk(float chunkStartZ, float chunkLength)
        {
            // Ensure pooling service is initialized before use
            InitializePoolingService();
            
            SpawnObstacles(chunkStartZ, chunkLength);
            SpawnPickups(chunkStartZ, chunkLength);
        }
    
        void SpawnObstacles(float chunkStartZ, float chunkLength)
        {
            int obstacleCount = Mathf.RoundToInt(
                Random.Range(minObstaclesPerChunk, maxObstaclesPerChunk) * obstacleDensity
            );
        
            for (int i = 0; i < obstacleCount; i++)
            {
                // Random lane
                float x = lanes[Random.Range(0, lanes.Length)];
            
                // Random Z position within chunk (leave margins)
                float z = chunkStartZ + Random.Range(10f, chunkLength - 10f);
            
                Vector3 position = new Vector3(x, obstacleHeight, z);
            
                // Random obstacle type
                //string obstacleType = Random.value > 0.5f ? "Tree" : "Rock";
            
                // Spawn from pooling service
                // TODO: fix getting obstacle from pool
                poolingService.GetFromPool<Obstacle>("Tree", obstacleParent, (Obstacle obstacle) =>
                {
                    if (obstacle != null)
                    {
                        obstacle.transform.position = position;
                        obstacle.gameObject.SetActive(true);
                        WorldMover.Instance.RegisterObject(obstacle.transform);
                        obstacle.SetLookAt(mainCamera.transform);
                    }
                });
            }
        }
    
        void SpawnPickups(float chunkStartZ, float chunkLength)
        {
            int pickupCount = Random.Range(minPickupsPerChunk, maxPickupsPerChunk);
        
            for (int i = 0; i < pickupCount; i++)
            {
                // Random lane
                float x = lanes[Random.Range(0, lanes.Length)];
            
                // Random Z position within chunk
                float z = chunkStartZ + Random.Range(10f, chunkLength - 10f);
            
                Vector3 position = new Vector3(x, pickupHeight, z);
            
                // Spawn from pooling service
                poolingService.GetFromPool<Pickup>("HotPickup", pickupParent, (Pickup pickup) =>
                {
                    pickup.transform.position = position;
                    WorldMover.Instance.RegisterObject(pickup.transform);
                    pickup.SetLookAt(mainCamera.transform);
                });
            }
        }
    }
}