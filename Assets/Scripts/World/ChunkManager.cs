using System.Collections.Generic;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;

namespace World
{
    public class ChunkManager : MonoBehaviour
    {
        [Header("Chunk Settings")]
        public TerrainChunk chunkPrefab;
        public GameObject terrainChunkParent;
        public int chunkLength = 100;          // Length of each chunk (Z-axis)
        public int activeChunkCount = 8;       // How many chunks to keep active
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
        
            // Spawn initial chunks
            for (int i = 0; i < activeChunkCount; i++)
            {
                SpawnChunk();
            }
        }
    
        void SpawnChunk()
        {
            poolingService.GetFromPool<TerrainChunk>("TerrainChunk", terrainChunkParent, (TerrainChunk chunk) =>
            {
                if (chunk != null)
                {
                    chunk.name = $"Chunk_{chunkIndex}";
                    OnCreateChunk(chunk);
                    Debug.Log("Spawned Chunk at Z: " + nextSpawnZ);
                    if(activeChunks.Count < activeChunkCount)
                    {
                        nextSpawnZ += chunkLength;
                        chunkIndex++;
                    }
                }
            });
        }
    
        public void DespawnOldChunk()
        {
            TerrainChunk oldChunk = activeChunks.Dequeue();
            poolingService.ReturnToPool("TerrainChunk", oldChunk);
        }

        public void SpawnNewChunk()
        {
            poolingService.GetFromPool("TerrainChunk", terrainChunkParent, (TerrainChunk chunk) =>
            {
                if (chunk != null)
                {
                    OnCreateChunk(chunk);
                }
            });
        }

        private void OnCreateChunk(TerrainChunk chunk)
        {
            chunk.transform.parent = transform;
            chunkXRotation = chunk.transform.rotation.eulerAngles.x;

            // Calculate the offset for proper alignment
            float yOffset = Mathf.Sin(chunkXRotation * Mathf.Deg2Rad) * chunkLength * -1;

            Vector3 chunkPosition = new Vector3(
                0f,
                yOffset * chunkIndex,
                nextSpawnZ
            );

            chunk.transform.position = chunkPosition;

            activeChunks.Enqueue(chunk);

            if (proceduralSpawner != null)
            {
                proceduralSpawner.PopulateChunk(nextSpawnZ, chunkLength, chunkXRotation, chunk.transform.position.y);
            }

            WorldMover.Instance.RegisterObject(chunk.transform);
        }
    }
}
