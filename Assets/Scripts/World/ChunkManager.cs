using System.Collections.Generic;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.Core.UpdateManagers;
using UnityCoreKit.Runtime.Core.UpdateManagers.Interfaces;
using UnityEngine;

namespace World
{
    public class ChunkManager : MonoBehaviour, IUpdateObserver
    {
        [Header("Chunk Settings")]
        public TerrainChunk chunkPrefab;
        public GameObject terrainChunkParent;
        public int chunkLength = 100;          // Length of each chunk (Z-axis)
        public int activeChunkCount = 4;       // How many chunks to keep active
        public float spawnDistance = 200f;     // Distance ahead to spawn next chunk
        public float chunkXRotation = 0f;
    
        [Header("References")]
        public Transform player;               // Assign Player in inspector
        public ProceduralSpawner proceduralSpawner; // Reference to ProceduralSpawner
    
        private Queue<TerrainChunk> activeChunks = new Queue<TerrainChunk>();
        private float nextSpawnZ = 0f;
        private int chunkIndex = 0;
        private IPoolManager poolingService;
    
        void Start()
        {
            // Get the Pooling Service from UnityCoreKit
            poolingService = CoreServices.Get<IPoolManager>();
        
            // Register chunk prefab with the pooling service
            poolingService.InitPool<TerrainChunk>("TerrainChunk", activeChunkCount + 2);
        
            // Register with UpdateManager
            UpdateManager.RegisterObserver(this);
        
            // Spawn initial chunks
            for (int i = 0; i < activeChunkCount; i++)
            {
                SpawnChunk();
            }
        }
    
        void OnDestroy()
        {
            // Unregister from UpdateManager
            UpdateManager.UnregisterObserver(this);
        }
    
        public void ObservedUpdate()
        {
            // Check if we need to spawn a new chunk
            if (player != null && player.position.z > nextSpawnZ - spawnDistance)
            {
                SpawnChunk();
                DespawnOldChunk();
            }
        }
    
        void SpawnChunk()
        {
            poolingService.GetFromPool<TerrainChunk>("TerrainChunk", terrainChunkParent, (TerrainChunk chunk) =>
            {
                if (chunk != null)
                {
                    chunk.name = $"Chunk_{chunkIndex}";
                    chunk.transform.parent = transform;
                    chunkXRotation = chunk.transform.rotation.eulerAngles.x;
                    
                    // Calculate the offset for proper alignment
                    // Each chunk needs to be offset by the "rise" over its length
                    float yOffset = Mathf.Sin(chunkXRotation * Mathf.Deg2Rad) * chunkLength * -1;
            
                    // Position accumulates both Z and Y offsets
                    Vector3 chunkPosition = new Vector3(
                        0f, 
                        yOffset * chunkIndex,  // Accumulate Y offset for each chunk
                        nextSpawnZ
                    );
                    
                    chunk.transform.position = chunkPosition;
            
                    activeChunks.Enqueue(chunk);
            
                    // Notify ProceduralSpawner to populate this chunk
                    if (proceduralSpawner != null)
                    {
                        proceduralSpawner.PopulateChunk(nextSpawnZ, chunkLength);
                    }
            
                    Debug.Log("Spawned Chunk at Z: " + nextSpawnZ);
                    nextSpawnZ += chunkLength;
                    chunkIndex++;
                }
            });
        }
    
        void DespawnOldChunk()
        {
            if (activeChunks.Count > activeChunkCount)
            {
                TerrainChunk oldChunk = activeChunks.Dequeue();
                poolingService.ReturnToPool("TerrainChunk", oldChunk);
            }
        }
    }
}
