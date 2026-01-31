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

        [Header("Goal Settings")]
        public int minGoalsPerChunk = 1;
        public int maxGoalsPerChunk = 2;
        
        [Header("Prefab References")]
        public Obstacle treePrefab;
        public GameObject rockPrefab;
        public Pickup hotPickupPrefab;
    
        [Header("Spacing")]
        public float minSpacing = 15f;          // Minimum distance between objects in same lane
    
        private IPoolManager poolingService;
        private GameManager gameManager;
        void Start()
        {
            InitializePoolingService();
            gameManager = GameManager.instance;
        }
        
        private void InitializePoolingService()
        {
            if (poolingService != null) return; // Already initialized
    
            // Get the Pooling Service from UnityCoreKit
            poolingService = CoreServices.Get<IPoolManager>();

            // Register prefabs with the pooling service
            poolingService.InitPool<Obstacle>("Tree", 20);
            poolingService.InitPool<Pickup>("HotPickup", 20);
            poolingService.InitPool<Goal>("Goal1", 10);
        }
    
        public void PopulateChunk(TerrainChunk chunk)
        {
            if(chunk.IsPopulated) return;
            
            // Ensure pooling service is initialized before use
            InitializePoolingService();
            
            SpawnObstacles(chunk);
            SpawnPickups(chunk);
            SpawnGoals(chunk);
            
            chunk.SetPopulated();
        }
    
        // Helper method to calculate Y position based on chunk rotation and terrain height
        private float CalculateYPosition(float baseHeight, float localZ, float chunkXRotation, float chunkHeight)
        {
            // Start with the chunk's base height
            float terrainHeight = chunkHeight;
            
            // Convert rotation to radians
            float rotationRad = chunkXRotation * Mathf.Deg2Rad;
            
            // Calculate the vertical offset based on the distance along Z and the rotation
            // As we move along Z on a tilted surface, the height changes by: distance * sin(angle)
            float yOffset = localZ * Mathf.Sin(rotationRad);
            
            return terrainHeight + baseHeight + -yOffset;
        }
    
        void SpawnObstacles(TerrainChunk chunk)
        {
            float chunkStartZ = chunk.planeStart.transform.position.z;
            float chunkEndZ = chunk.planeEnd.transform.position.z;
            
            int obstacleCount = Mathf.RoundToInt(
                Random.Range(minObstaclesPerChunk, maxObstaclesPerChunk) * obstacleDensity
            );
        
            for (int i = 0; i < obstacleCount; i++)
            {
                var position = CalculatePosition(chunk, chunkStartZ, chunkEndZ);
            
                // Random obstacle type
                //string obstacleType = Random.value > 0.5f ? "Tree" : "Rock";
            
                // Spawn from pooling service
                // TODO: fix getting obstacle from pool
                poolingService.GetFromPool<Obstacle>("Tree", obstacleParent, (Obstacle obstacle) =>
                {
                    if (obstacle != null)
                    {
                        obstacle.transform.position = position;
                        chunk.AddObjectAsChild(obstacle.gameObject);
                        obstacle.gameObject.SetActive(true);
                    }
                });
            }
        }
    
        void SpawnPickups(TerrainChunk chunk)
        {
            float chunkStartZ = chunk.planeStart.transform.position.z;
            float chunkEndZ = chunk.planeEnd.transform.position.z;
            
            int pickupCount = Random.Range(minPickupsPerChunk, maxPickupsPerChunk);
        
            for (int i = 0; i < pickupCount; i++)
            {
                var position = CalculatePosition(chunk, chunkStartZ, chunkEndZ);

                // Spawn from pooling service
                poolingService.GetFromPool<Pickup>("HotPickup", pickupParent, (Pickup pickup) =>
                {
                    pickup.transform.position = position;
                    chunk.AddObjectAsChild(pickup.gameObject);
                    pickup.gameObject.SetActive(true);
                });
            }
        }
        
        void SpawnGoals(TerrainChunk chunk)
        {
            float chunkStartZ = chunk.planeStart.transform.position.z;
            float chunkEndZ = chunk.planeEnd.transform.position.z;
            
            int pickupCount = Random.Range(minGoalsPerChunk, maxGoalsPerChunk);
        
            for (int i = 0; i < pickupCount; i++)
            {
                var position = CalculatePosition(chunk, chunkStartZ, chunkEndZ);

                // Spawn from pooling service
                poolingService.GetFromPool<Goal>("Goal1", pickupParent, (Goal goal) =>
                {
                    goal.transform.position = position;
                    chunk.AddObjectAsChild(goal.gameObject);
                    goal.gameObject.SetActive(true);
                });
            }
        }

        private Vector3 CalculatePosition(TerrainChunk chunk, float chunkStartZ, float chunkEndZ)
        {
            // Random lane
            float x = lanes[Random.Range(0, lanes.Length)];

            // Random Z position within chunk
            float z = Random.Range(chunkStartZ, chunkEndZ);

            // Calculate local Z position within the chunk
            float localZ = z - chunkStartZ;

            // Calculate correct Y position based on chunk rotation and terrain height
            float y = CalculateYPosition(obstacleHeight, localZ, chunk.transform.rotation.x, chunk.transform.position.y);

            Vector3 position = new Vector3(x, y, z);
            return position;
        }
    }
}