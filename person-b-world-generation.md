# Person B: World Generation, Obstacles & Pickups - Implementation Plan

**Responsibility Area:** Procedural world generation, obstacle/pickup spawning, billboarding, and object pooling  
**Timeline:** Day 1-2 (12-16 hours)  
**Dependencies:** Needs Player position from Person A's PlayerController

---

## OVERVIEW

You're responsible for creating the infinite world that the player skis through. This includes:
1. **Terrain chunks** that spawn ahead and despawn behind
2. **Obstacle spawning** in lanes (trees, rocks, branches)
3. **Pickup spawning** (hot drinks, fire icons)
4. **Billboard system** so sprites always face the camera
5. **Object pooling** for performance optimization

---

## TASK BREAKDOWN

### Phase 1: Basic Terrain Chunks (2-3 hours)

#### 1.1 Create Terrain Chunk Prefab
**Assets/Prefabs/Chunks/TerrainChunk.prefab**

Steps:
1. In Unity: GameObject → 3D Object → Plane
2. Scale to (10, 1, 10) — this gives 100 units along Z-axis
3. Position at (0, 0, 0)
4. Create snow material:
   - Right-click in Materials folder → Create → Material → "SnowGround"
   - Shader: Universal Render Pipeline/Lit
   - Albedo: White or light gray/blue tint
   - Smoothness: 0.3-0.5 (slightly reflective)
   - Optional: Add snow texture if available
5. Apply material to plane
6. Save as prefab: Drag to Prefabs/Chunks folder

#### 1.2 Implement ChunkManager.cs
**Assets/Scripts/World/ChunkManager.cs**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject chunkPrefab;
    public int chunkLength = 100;          // Length of each chunk (Z-axis)
    public int activeChunkCount = 4;       // How many chunks to keep active
    public float spawnDistance = 200f;     // Distance ahead to spawn next chunk
    
    [Header("References")]
    public Transform player;               // Assign Player in inspector
    
    private Queue<GameObject> activeChunks = new Queue<GameObject>();
    private float nextSpawnZ = 0f;
    private int chunkIndex = 0;
    
    void Start()
    {
        // Spawn initial chunks
        for (int i = 0; i < activeChunkCount; i++)
        {
            SpawnChunk();
        }
    }
    
    void Update()
    {
        // Check if we need to spawn a new chunk
        if (player.position.z > nextSpawnZ - spawnDistance)
        {
            SpawnChunk();
            DespawnOldChunk();
        }
    }
    
    void SpawnChunk()
    {
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject chunk = Instantiate(chunkPrefab, spawnPosition, Quaternion.identity);
        chunk.name = $"Chunk_{chunkIndex}";
        chunk.transform.parent = transform;
        
        activeChunks.Enqueue(chunk);
        
        // Notify ProceduralSpawner to populate this chunk
        ProceduralSpawner spawner = GetComponent<ProceduralSpawner>();
        if (spawner != null)
        {
            spawner.PopulateChunk(nextSpawnZ, chunkLength);
        }
        
        nextSpawnZ += chunkLength;
        chunkIndex++;
    }
    
    void DespawnOldChunk()
    {
        if (activeChunks.Count > activeChunkCount)
        {
            GameObject oldChunk = activeChunks.Dequeue();
            Destroy(oldChunk);
        }
    }
}
```

**Setup in Unity:**
1. Create empty GameObject: "WorldManager"
2. Add ChunkManager component
3. Assign chunk prefab to field
4. Assign Player reference (will be set after Person A creates player)

**Test:** Run scene, check console for chunk spawning logs, verify chunks appear ahead

---

### Phase 2: Billboard System (1 hour)

#### 2.1 Create Billboard Script
**Assets/Scripts/World/Billboard.cs**

```csharp
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void LateUpdate()
    {
        if (mainCamera == null) return;
        
        // Make the sprite face the camera
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180f, 0); // Face toward camera (not away)
    }
}
```

**Alternative: Use Unity's Built-in Billboard Renderer**
- Select sprite GameObject
- Add Component → Effects → Billboard Renderer
- Assign material
- Set Billboard property to "Facing Camera Position"

**Recommendation:** Use the custom script for more control, or Billboard Renderer for simplicity.

---

### Phase 3: Create Obstacle Prefabs (2 hours)

#### 3.1 Create Placeholder Sprites (if needed)
If you don't have sprite assets yet:

**Option 1: Find Free Assets**
- Unity Asset Store: Search "tree sprite", "rock sprite"
- OpenGameArt.org
- Kenney.nl (has free game assets)

**Option 2: Create Quick Placeholders**
- Open any image editor (Paint, Photoshop, GIMP)
- Create 512x512 PNG with transparent background
- Draw simple tree silhouette (triangle + rectangle)
- Draw rock silhouette (irregular oval)
- Save as tree_sprite.png, rock_sprite.png
- Import to Assets/Textures/Obstacles/

**Import Settings:**
1. Select sprite in Unity
2. Texture Type: Sprite (2D and UI)
3. Alpha Is Transparency: ✓
4. Filter Mode: Bilinear
5. Click Apply

#### 3.2 Create Obstacle Material
**Assets/Materials/ObstacleSprites.mat**

1. Right-click in Materials folder → Create → Material
2. Name: "ObstacleSprites"
3. Shader: Universal Render Pipeline/Unlit
4. Surface Type: Transparent
5. Rendering Mode: Fade or Transparent
6. Assign sprite texture to Base Map
7. Color: White (full brightness)

#### 3.3 Build Obstacle Prefabs

**Tree Prefab (Assets/Prefabs/Obstacles/Tree.prefab):**
1. Create empty GameObject: "Tree"
2. Add child: GameObject → 3D Object → Quad
3. Scale quad to (2, 3, 1) — adjust to sprite dimensions
4. Apply ObstacleSprites material
5. Add Billboard script to parent GameObject
6. Add Box Collider to parent:
   - Is Trigger: ✓
   - Size: (1, 2, 0.5) — adjust based on sprite
   - Tag: "Obstacle"
7. Position quad at (0, 1.5, 0) so base is at ground level
8. Save as prefab

**Rock Prefab (similar process):**
1. Create "Rock" GameObject
2. Quad child, scale (1.5, 1.5, 1)
3. Same material, different texture/sprite
4. Box Collider: (1, 1, 0.5), Is Trigger ✓
5. Tag: "Obstacle"
6. Save as prefab

**Branch Prefab (optional, if time permits):**
- Smaller size, narrower collider
- Can be variation of tree with different sprite

#### 3.4 Set Up Tags & Layers
**Edit → Project Settings → Tags and Layers**

**Tags:**
- Add "Obstacle"
- Add "Pickup"

**Layers:**
- Layer 8: "Obstacle"
- Layer 9: "Pickup"
- Layer 10: "Player"

**Apply to Prefabs:**
- Tree prefab: Tag = "Obstacle", Layer = "Obstacle"
- Rock prefab: Tag = "Obstacle", Layer = "Obstacle"

---

### Phase 4: Create Pickup Prefabs (1.5 hours)

#### 4.1 Create Pickup Sprite
Similar to obstacles, create or find:
- hot_drink.png (coffee mug, steaming cup)
- fire_icon.png (flame symbol)

Make them bright/warm colors (orange, yellow, red)

#### 4.2 Create Emissive Pickup Material
**Assets/Materials/PickupGlow.mat**

1. Create new Material: "PickupGlow"
2. Shader: Universal Render Pipeline/Unlit
3. Surface Type: Transparent
4. Assign pickup texture
5. **Enable Emission:**
   - Scroll to Emission section
   - Enable Emission Map
   - Set Emission Color: Bright orange/yellow (HDR, intensity ~3-5)
   - Assign same texture to Emission Map
6. This will make pickups glow when Bloom is enabled

#### 4.3 Build Pickup Prefab
**Assets/Prefabs/Pickups/HotPickup.prefab**

1. Create GameObject: "HotPickup"
2. Add Quad child, scale (1, 1, 1)
3. Apply PickupGlow material
4. Add Billboard script to parent
5. Add Sphere Collider:
   - Is Trigger: ✓
   - Radius: 0.8 (larger than visual for easier collection)
   - Tag: "Pickup"
   - Layer: "Pickup"
6. Add Pickup.cs script (see below)
7. Optional: Add child Particle System for extra glow
   - Start Lifetime: 1
   - Start Speed: 0.5
   - Start Size: 0.2
   - Emission: 20 particles/sec
   - Color: Yellow/orange
   - Renderer: Material with additive blend
8. Save as prefab

#### 4.4 Create Pickup.cs Script
**Assets/Scripts/Pickups/Pickup.cs**

```csharp
using UnityEngine;

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
        // Notify FrostManager (Person C will implement this)
        // FrostManager.Instance.ReduceFrost(frostReduction);
        
        // Spawn particle effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Return to pool or destroy
        gameObject.SetActive(false);
    }
}
```

**Note:** FrostManager integration will be completed when Person C finishes their work. For now, add a Debug.Log to test collection.

---

### Phase 5: Object Pooling System (2 hours)

#### 5.1 Create ObjectPool.cs
**Assets/Scripts/World/ObjectPool.cs**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;              // Identifier (e.g., "Tree", "Rock")
        public GameObject prefab;
        public int size;                // Initial pool size
    }
    
    [Header("Pools")]
    public List<Pool> pools;
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.parent = transform;
                objectPool.Enqueue(obj);
            }
            
            poolDictionary.Add(pool.tag, objectPool);
        }
    }
    
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }
        
        GameObject objectToSpawn;
        
        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }
        else
        {
            // Pool exhausted, create new instance
            objectToSpawn = Instantiate(pools.Find(p => p.tag == tag).prefab);
            objectToSpawn.transform.parent = transform;
        }
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        
        return objectToSpawn;
    }
    
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
        
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
    }
}
```

#### 5.2 Set Up Object Pool in Scene
1. Select WorldManager GameObject
2. Add ObjectPool component
3. Set Pools list size to 3 (Tree, Rock, Pickup)
4. For each pool:
   - Tag: "Tree", "Rock", "HotPickup"
   - Prefab: Drag corresponding prefab
   - Size: 50 (for obstacles), 20 (for pickups)

---

### Phase 6: Procedural Spawner (3-4 hours)

#### 6.1 Implement ProceduralSpawner.cs
**Assets/Scripts/World/ProceduralSpawner.cs**

```csharp
using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] lanes = { -2f, -1f, 0f, 1f, 2f };  // X positions for lanes
    
    [Header("Obstacle Settings")]
    public int minObstaclesPerChunk = 5;
    public int maxObstaclesPerChunk = 15;
    public float obstacleDensity = 1f;     // Multiplier (modified by DifficultyManager)
    public float obstacleHeight = 0f;       // Y position (ground level)
    
    [Header("Pickup Settings")]
    public int minPickupsPerChunk = 2;
    public int maxPickupsPerChunk = 5;
    public float pickupHeight = 0.5f;       // Slightly above ground
    
    [Header("Spacing")]
    public float minSpacing = 15f;          // Minimum distance between objects in same lane
    
    private ObjectPool objectPool;
    
    void Start()
    {
        objectPool = GetComponent<ObjectPool>();
    }
    
    public void PopulateChunk(float chunkStartZ, float chunkLength)
    {
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
            string obstacleType = Random.value > 0.5f ? "Tree" : "Rock";
            
            // Spawn from pool
            GameObject obstacle = objectPool.SpawnFromPool(obstacleType, position, Quaternion.identity);
            
            // Optional: Add slight random rotation around Y-axis for variety
            if (obstacle != null)
            {
                obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
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
            
            // Spawn from pool
            objectPool.SpawnFromPool("HotPickup", position, Quaternion.identity);
        }
    }
}
```

#### 6.2 Connect to ChunkManager
1. Select WorldManager GameObject
2. Add ProceduralSpawner component
3. Configure settings in inspector:
   - Lanes: -2, -1, 0, 1, 2 (size: 5)
   - Min/Max Obstacles: 5/15
   - Min/Max Pickups: 2/5
4. Test: Run game, obstacles/pickups should spawn in chunks

---

### Phase 7: Cleanup & Recycling (1 hour)

#### 7.1 Add Auto-Despawn Script
**Assets/Scripts/World/AutoDespawn.cs**

```csharp
using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
    [Header("Despawn Settings")]
    public float despawnDistance = 50f;  // Distance behind player to despawn
    
    private Transform player;
    private string poolTag;
    
    void Start()
    {
        // Find player (Person A will create this)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Determine pool tag from object name
        if (gameObject.name.Contains("Tree")) poolTag = "Tree";
        else if (gameObject.name.Contains("Rock")) poolTag = "Rock";
        else if (gameObject.name.Contains("Pickup")) poolTag = "HotPickup";
    }
    
    void Update()
    {
        if (player == null) return;
        
        // If object is far behind player, return to pool
        if (transform.position.z < player.position.z - despawnDistance)
        {
            ObjectPool pool = FindObjectOfType<ObjectPool>();
            if (pool != null)
            {
                pool.ReturnToPool(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
```

#### 7.2 Add to Prefabs
Add AutoDespawn component to:
- Tree prefab
- Rock prefab
- HotPickup prefab

Set despawnDistance to 50 (or adjust based on testing)

---

## INTEGRATION POINTS

### With Person A (Player Controller):
- **You need:** Reference to Player transform for ChunkManager
- **You provide:** Obstacle and Pickup colliders for Person A's collision detection

**Setup:**
1. After Person A creates Player GameObject:
   - Tag it as "Player"
   - Assign to ChunkManager's player field
2. Ensure Player has a collider (capsule or box) with Is Trigger enabled

### With Person C (Frost Manager):
- **You need:** FrostManager.Instance reference in Pickup.cs
- **Temporary workaround:** Use `Debug.Log("Pickup collected!")` until Person C is ready

**Integration step:**
```csharp
// In Pickup.cs Collect() method, replace debug log with:
FrostManager.Instance?.ReduceFrost(frostReduction);
```

### With Person D (Game Manager):
- **You provide:** Obstacles tagged properly for game over detection
- **You need:** Notification when game restarts to clear active obstacles

---

## TESTING CHECKLIST

### Phase 1: Chunks
- [ ] 4 chunks visible at start
- [ ] New chunk spawns as player moves forward
- [ ] Old chunks despawn behind player
- [ ] No gaps between chunks
- [ ] No overlapping chunks

### Phase 2: Billboards
- [ ] Sprites always face camera
- [ ] No flickering or rotation jitter
- [ ] Works when looking from different angles

### Phase 3: Obstacles
- [ ] Tree prefab renders correctly
- [ ] Rock prefab renders correctly
- [ ] Colliders are proper size
- [ ] Tagged as "Obstacle"
- [ ] Billboard script works on prefabs

### Phase 4: Pickups
- [ ] Pickup sprite visible and glowing
- [ ] Collider larger than visual for easy collection
- [ ] Pickup.cs script attached
- [ ] Tagged as "Pickup"
- [ ] Collection triggers (debug log appears)

### Phase 5: Object Pooling
- [ ] Objects spawn from pool
- [ ] Pool doesn't create extra instances unnecessarily
- [ ] Objects return to pool when far behind
- [ ] No memory leaks (check Profiler)
- [ ] Performance is smooth

### Phase 6: Spawning
- [ ] Obstacles spawn in lanes
- [ ] Pickups spawn in lanes
- [ ] Spacing looks reasonable
- [ ] Density feels balanced
- [ ] Objects don't overlap

### Phase 7: Cleanup
- [ ] Objects despawn when behind player
- [ ] No infinite accumulation of objects
- [ ] Scene stays clean in Hierarchy during play

---

## TIMELINE & MILESTONES

### Day 1 (6-8 hours)
**Morning (3-4 hours):**
- ✅ Phase 1: Terrain chunks working
- ✅ Phase 2: Billboard system
- ✅ Phase 3: Obstacle prefabs created

**Afternoon (3-4 hours):**
- ✅ Phase 4: Pickup prefabs created
- ✅ Phase 5: Object pooling system
- ✅ Basic testing

**End of Day 1:** Can spawn chunks with obstacles and pickups (even if not fully functional)

### Day 2 (4-6 hours)
**Morning (2-3 hours):**
- ✅ Phase 6: Procedural spawning refined
- ✅ Phase 7: Cleanup & despawning
- ✅ Integration with Person A's player

**Afternoon (2-3 hours):**
- ✅ Polish spawn patterns
- ✅ Balance obstacle/pickup density
- ✅ Test with increasing speed
- ✅ Bug fixes

**End of Day 2:** Fully functional world generation system ready for difficulty scaling

---

## ASSET REQUIREMENTS

### Textures Needed:
- [ ] snow_texture.png (for ground, 1024x1024, tileable)
- [ ] tree_sprite.png (with alpha, 512x512)
- [ ] rock_sprite.png (with alpha, 512x512)
- [ ] branch_sprite.png (optional, with alpha, 512x512)
- [ ] hot_drink.png (with alpha, 256x256)
- [ ] fire_icon.png (optional, with alpha, 256x256)

### Asset Sources:
1. **Unity Asset Store** (free section)
2. **Kenney.nl** (free game assets, great for prototyping)
3. **OpenGameArt.org** (community assets)
4. **Quick DIY:** Use image editor to create simple silhouettes

### Material Setup:
- [ ] SnowGround.mat (Lit shader)
- [ ] ObstacleSprites.mat (Unlit, transparent)
- [ ] PickupGlow.mat (Unlit, emissive)

---

## OPTIMIZATION TIPS

### Performance Goals:
- 60 FPS with 50+ obstacles visible
- Object pooling prevents garbage collection spikes
- Billboards are cheaper than full 3D models

### If Performance Issues:
1. **Reduce active chunks:** Lower `activeChunkCount` to 3
2. **Lower obstacle density:** Reduce `maxObstaclesPerChunk`
3. **Simplify billboards:** Remove particle effects on pickups
4. **Cull distant objects:** Add LOD or distance-based culling
5. **Use Billboard Renderer:** Unity's built-in is optimized

### Memory Management:
- Pool size should handle peak density (50+ obstacles)
- Don't destroy/instantiate during gameplay
- Clear inactive objects on restart

---

## DEBUGGING TIPS

### Common Issues:

**Problem:** Chunks have gaps or overlap  
**Solution:** Check `chunkLength` matches actual chunk size (100 units)

**Problem:** Billboards flicker or spin wildly  
**Solution:** Use `LateUpdate()` for billboard rotation, add null check for camera

**Problem:** Obstacles don't spawn  
**Solution:** Check ObjectPool is initialized, verify pool tags match spawn calls

**Problem:** Pickups not collected  
**Solution:** Verify Player has collider, check Tag/Layer settings, ensure Is Trigger enabled

**Problem:** Performance drops over time  
**Solution:** Check AutoDespawn is working, verify objects return to pool

**Problem:** Objects spawn in wrong positions  
**Solution:** Debug.Log spawn positions, check lane array values, verify chunk Z calculation

### Debug Helpers:
```csharp
// Add to ProceduralSpawner for visual debugging
void OnDrawGizmos()
{
    Gizmos.color = Color.green;
    foreach (float lane in lanes)
    {
        Gizmos.DrawLine(new Vector3(lane, 0, 0), new Vector3(lane, 0, 500));
    }
}
```

---

## QUICK REFERENCE

### Key Variables to Expose:
```csharp
// ChunkManager
- chunkLength = 100
- activeChunkCount = 4
- spawnDistance = 200

// ProceduralSpawner
- lanes[] = {-2, -1, 0, 1, 2}
- minObstaclesPerChunk = 5
- maxObstaclesPerChunk = 15
- minPickupsPerChunk = 2
- maxPickupsPerChunk = 5

// ObjectPool
- Pool sizes: 50 (obstacles), 20 (pickups)
```

### Prefab Hierarchy:
```
Tree
├── Quad (with material)
├── BoxCollider (trigger)
└── Billboard script

HotPickup
├── Quad (emissive material)
├── SphereCollider (trigger)
├── Pickup.cs
└── ParticleSystem (optional)
```

---

## COMMUNICATION CHECKPOINTS

### With Team:
- **After Phase 3:** Share obstacle prefabs so Person A can test collision
- **After Phase 4:** Share pickup prefabs for Person C to integrate frost reduction
- **After Phase 6:** Coordinate with Person D on difficulty scaling variables

### Questions to Ask:
- **Person A:** "What's the Player GameObject name/tag?"
- **Person A:** "What collider type are you using on Player?"
- **Person C:** "When will FrostManager singleton be ready?"
- **Person D:** "What difficulty curve do you want for obstacle density?"

---

## NICE-TO-HAVE (If Extra Time)

### Visual Polish:
- [ ] Add slight bobbing animation to pickups
- [ ] Vary obstacle sizes randomly (scale 0.8-1.2)
- [ ] Add shadow sprites beneath objects
- [ ] Particle effects on obstacle hit (Person D may handle this)

### Gameplay Variety:
- [ ] Multiple obstacle types (tall vs wide, different collision shapes)
- [ ] Rare "super pickup" that clears all frost
- [ ] Obstacle clusters (trees in groups)
- [ ] Safe zones (areas with no obstacles)

### Technical Improvements:
- [ ] Spatial hashing for spawn collision detection
- [ ] Weighted random for obstacle types (more trees than rocks)
- [ ] Difficulty-based pickup frequency (fewer as game progresses)
- [ ] Sound on obstacle despawn (satisfying "passed it" audio cue)

---

## FINAL CHECKLIST

Before marking complete:
- [ ] All scripts compile without errors
- [ ] All prefabs saved in correct folders
- [ ] Object pool configured with all prefab types
- [ ] ChunkManager spawns chunks smoothly
- [ ] Obstacles spawn in lanes and face camera
- [ ] Pickups spawn and are collectible
- [ ] Objects despawn behind player
- [ ] No performance issues (60 FPS)
- [ ] Tags and layers configured
- [ ] Player reference assigned to ChunkManager
- [ ] Code commented and readable
- [ ] Tested with Player movement (with Person A)

---

**Good luck! Focus on getting Phase 1-6 working solidly. Phase 7 and polish can come after core functionality is proven. Test frequently as you build each phase.**
