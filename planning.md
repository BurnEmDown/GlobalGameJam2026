# Endless Frost Skier - Game Jam Planning Document

**Project Type:** 48-72 Hour Game Jam  
**Engine:** Unity 2022 LTS (Current Project)  
**Target Platform:** PC (Windows/Mac)  
**Core Hook:** Vision-obscured endless skiing with frost management mechanic

---

## 1. PROJECT OVERVIEW

### High Concept
First-person endless downhill skier where vision is heavily obstructed by a frosted ski mask. Player must collect hot pickups to clear frost and avoid obstacles while skiing at increasing speeds.

### Core Loop
1. Ski forward automatically (speed increases over time)
2. Steer left/right to dodge obstacles and collect pickups
3. Look around with mouse to spot obstacles through limited eye holes
4. Frost accumulates → vision degrades → harder to navigate
5. Collect pickups → frost clears → easier to see
6. Crash into obstacle → game over with distance score
7. Restart and try to beat previous distance

### Target Scope
**48-72 hours total development time**
- Day 1 (8-10h): Core movement, camera, basic spawning, collision
- Day 2 (8-10h): Frost shader/overlay, pickup system, difficulty scaling
- Day 3 (6-8h): Polish, audio, particles, UI, testing, build

---

## 2. TECHNICAL SETUP

### Unity Project Configuration

#### Required Packages (Install via Package Manager)
```
- Universal Render Pipeline (URP) — Already installed
- Input System (if not using legacy Input) — Already installed
- Post Processing (for bloom/glow effects)
- TextMeshPro — For UI text
```

#### Project Settings
- **Graphics:** URP Asset configured with:
  - Post Processing enabled
  - HDR enabled (for bloom/emissive materials)
  - Anti-aliasing: SMAA or TAA
- **Quality Settings:** Set to "High" for development, optimize later
- **Physics:** Default 3D physics, layer-based collision matrix
- **Input:** Configure Input System actions for movement and mouse look

#### Folder Structure
```
Assets/
├── Scenes/
│   ├── MainGame.unity          (Primary game scene)
│   └── MainMenu.unity           (Optional simple menu)
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── MouseLook.cs
│   │   └── PlayerCollision.cs
│   ├── World/
│   │   ├── ProceduralSpawner.cs
│   │   ├── ChunkManager.cs
│   │   └── ObstaclePool.cs
│   ├── Pickups/
│   │   ├── Pickup.cs
│   │   └── PickupCollector.cs
│   ├── Frost/
│   │   ├── FrostManager.cs
│   │   └── FrostOverlay.cs (UI overlay controller)
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── ScoreManager.cs
│   │   └── DifficultyManager.cs
│   └── UI/
│       ├── GameOverUI.cs
│       └── HUDController.cs
├── Materials/
│   ├── SnowGround.mat
│   ├── ObstacleSprites.mat      (Unlit/transparent)
│   ├── PickupGlow.mat           (Emissive)
│   └── FrostOverlay.mat         (UI material)
├── Textures/
│   ├── Snow/
│   │   └── snow_texture.png
│   ├── Obstacles/
│   │   ├── tree_sprite.png
│   │   ├── rock_sprite.png
│   │   └── branch_sprite.png
│   ├── Pickups/
│   │   ├── hot_drink.png
│   │   └── fire_icon.png
│   └── UI/
│       └── frost_overlay.png    (Full-screen frost texture with eye holes)
├── Prefabs/
│   ├── Player.prefab
│   ├── Obstacles/
│   │   ├── Tree.prefab
│   │   ├── Rock.prefab
│   │   └── Branch.prefab
│   ├── Pickups/
│   │   └── HotPickup.prefab
│   └── Chunks/
│       └── TerrainChunk.prefab
├── Audio/
│   ├── SFX/
│   │   ├── pickup.wav
│   │   ├── crash.wav
│   │   └── wind_loop.wav
│   └── Music/
│       └── background_loop.wav  (Optional)
├── Particles/
│   └── SnowParticles.prefab
└── Shaders/
    └── FrostOverlay.shader       (If using custom shader)
```

---

## 3. CORE SYSTEMS BREAKDOWN

### 3.1 Player Controller System

**Components:**
- **PlayerController.cs** — Handles forward movement & lateral steering
- **MouseLook.cs** — FPS camera rotation (independent of movement)
- **PlayerCollision.cs** — Detects collisions with obstacles/pickups

**Key Features:**
- Constant forward movement (speed increases over time)
- Lateral movement input (A/D or arrows) moves X position within bounds (-3 to +3)
- Mouse look rotates camera only, NOT the character body
- Movement direction always world-forward (0, 0, 1)

**Variables to Expose:**
```csharp
[Header("Movement")]
public float baseSpeed = 10f;              // Starting forward speed
public float maxSpeed = 30f;               // Maximum forward speed
public float speedIncreaseRate = 0.5f;     // Speed increase per second
public float lateralSpeed = 5f;            // Left/right strafe speed
public float lateralBounds = 3f;           // Max X position (+/-)

[Header("Mouse Look")]
public float mouseSensitivity = 2f;
public float verticalLookLimit = 85f;      // Prevent full 360 vertical
```

**Implementation Notes:**
- Use `CharacterController` or `Rigidbody` for movement
- Store current speed as instance variable, increase in `Update()`
- Clamp X position to stay within bounds
- Camera rotation uses Euler angles, clamped on X-axis

---

### 3.2 Camera & Mouse Look System

**Setup:**
- Camera is child of Player GameObject
- Camera positioned at head height (e.g., Y = 0.6 on player)
- Mouse look script rotates camera transform only

**Key Behavior:**
- Mouse X → rotate camera around Y-axis (horizontal look)
- Mouse Y → rotate camera around X-axis (vertical look), clamped to prevent over-rotation
- Player capsule/body does NOT rotate — always faces world forward
- Lock cursor during gameplay: `Cursor.lockState = CursorLockMode.Locked`

**Testing Checklist:**
- [ ] Looking up/down doesn't affect movement direction
- [ ] Looking sideways doesn't affect movement direction
- [ ] Can look behind while still moving forward
- [ ] Camera rotation feels smooth and responsive

---

### 3.3 Procedural World Generation

**System Architecture:**
- **ChunkManager** spawns terrain chunks ahead of player
- **ProceduralSpawner** populates chunks with obstacles/pickups
- **ObstaclePool** reuses obstacle/pickup GameObjects for performance

**Chunk System:**
- Each chunk is 100 units long (Z-axis)
- Spawn chunks when player enters trigger at chunk boundary
- Keep 3-4 chunks active at once, despawn chunks behind player
- Chunks are simple planes with snow material

**Obstacle/Pickup Placement:**
- Define 3-5 lanes (X positions: -2, -1, 0, 1, 2)
- Per chunk, place 5-15 obstacles randomly in lanes
- Place 2-5 pickups per chunk (less frequent than obstacles)
- Ensure no overlapping spawns in same lane position
- Increase density over time/distance

**Lane System Example:**
```csharp
private float[] lanes = { -2f, -1f, 0f, 1f, 2f };

void SpawnObstaclesInChunk(float chunkStartZ) {
    int obstacleCount = Random.Range(5, 15);
    for (int i = 0; i < obstacleCount; i++) {
        float x = lanes[Random.Range(0, lanes.Length)];
        float z = chunkStartZ + Random.Range(10f, 90f);
        SpawnObstacle(new Vector3(x, 0, z));
    }
}
```

**Billboard Setup:**
- Obstacles/pickups use quad mesh with transparent sprite material
- Add script that rotates object to face camera each frame:
  ```csharp
  transform.LookAt(Camera.main.transform);
  transform.Rotate(0, 180f, 0); // Face camera
  ```
- Or use Unity's `BillboardRenderer` component (simpler)

---

### 3.4 Frost/Vision Obstruction System

**Core Mechanic:**
Player's vision is obstructed by a frosted ski mask overlay. Only two small eye holes show the clear world.

**Implementation Approach (UI Canvas):**

**Setup:**
1. Create full-screen UI Canvas (Screen Space - Overlay)
2. Add Image component covering entire screen
3. Assign frost overlay texture/material to Image
4. Texture has two transparent circular/elliptical regions (eye holes)
5. Rest of texture is opaque white/blue with frost/ice pattern

**FrostManager.cs:**
```csharp
[Header("Frost Settings")]
public float frostLevel = 0.3f;           // 0 = clear, 1 = blind
public float frostIncreaseRate = 0.05f;   // Per second
public float frostDecreasePerPickup = 0.25f;
public float minEyeHoleScale = 0.3f;      // Smallest eye holes
public float maxEyeHoleScale = 1.0f;      // Largest eye holes

[Header("References")]
public Image frostOverlayImage;
public Transform leftEyeHole;             // Optional: scale eye holes
public Transform rightEyeHole;

void Update() {
    // Increase frost over time
    frostLevel += frostIncreaseRate * Time.deltaTime;
    frostLevel = Mathf.Clamp01(frostLevel);
    
    UpdateFrostVisuals();
}

public void ReduceFrost() {
    frostLevel -= frostDecreasePerPickup;
    frostLevel = Mathf.Clamp01(frostLevel);
}

void UpdateFrostVisuals() {
    // Option 1: Fade overlay opacity
    Color c = frostOverlayImage.color;
    c.a = Mathf.Lerp(0.6f, 0.95f, frostLevel);
    frostOverlayImage.color = c;
    
    // Option 2: Scale eye hole masks (if using separate mask objects)
    float scale = Mathf.Lerp(maxEyeHoleScale, minEyeHoleScale, frostLevel);
    // Apply scale to mask transforms
}
```

**Visual Design Tips:**
- Eye holes positioned symmetrically (mimic actual ski goggles)
- Frost texture should have organic ice crystals/patterns
- Can add slight animated noise/distortion for extra immersion
- Consider adding subtle vignette effect that intensifies with frost

**Alternate Implementation (Post-Process Shader):**
- Create custom post-processing shader that masks world render
- More complex but potentially better performance
- **Recommendation:** Start with UI approach, optimize later if needed

---

### 3.5 Pickup & Collision System

**Pickup Types:**
- **Hot Drink** — Reduces frost by 0.25
- **Fire Icon** — Reduces frost by 0.15 (optional variant)

**Pickup.cs:**
```csharp
public enum PickupType { HotDrink, Fire }

public PickupType type;
public float frostReduction = 0.25f;
public GameObject collectEffect;      // Particle prefab
public AudioClip collectSound;

void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
        Collect();
    }
}

void Collect() {
    // Notify FrostManager
    FrostManager.Instance.ReduceFrost(frostReduction);
    
    // Spawn particle effect
    if (collectEffect != null) {
        Instantiate(collectEffect, transform.position, Quaternion.identity);
    }
    
    // Play sound
    AudioSource.PlayClipAtPoint(collectSound, transform.position);
    
    // Return to pool or destroy
    gameObject.SetActive(false);
}
```

**Obstacle Collision:**
```csharp
// In PlayerCollision.cs
void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Obstacle")) {
        GameManager.Instance.GameOver();
    }
}
```

**Collider Setup:**
- Player: `CapsuleCollider` (trigger)
- Obstacles: `BoxCollider` or `SphereCollider` (trigger)
- Pickups: `SphereCollider` (trigger, larger radius for easier collection)

**Layer Setup:**
- Player layer: "Player"
- Obstacle layer: "Obstacle"
- Pickup layer: "Pickup"
- Configure Physics matrix so only Player interacts with Obstacles/Pickups

---

### 3.6 Difficulty Scaling System

**DifficultyManager.cs:**
```csharp
[Header("Speed Scaling")]
public float speedIncreaseRate = 0.5f;    // Units per second
public float maxSpeed = 30f;

[Header("Spawn Density")]
public AnimationCurve obstacleDensity;    // X = time/distance, Y = density
public AnimationCurve pickupDensity;

void Update() {
    float elapsedTime = Time.timeSinceLevelLoad;
    
    // Increase player speed over time
    PlayerController.Instance.speed = Mathf.Min(
        PlayerController.Instance.baseSpeed + (elapsedTime * speedIncreaseRate),
        maxSpeed
    );
    
    // Adjust spawn rates based on difficulty curve
    float densityMultiplier = obstacleDensity.Evaluate(elapsedTime);
    ProceduralSpawner.Instance.obstacleDensity = densityMultiplier;
}
```

**Difficulty Milestones:**
- **0-30s:** Tutorial phase — low obstacle density, frequent pickups
- **30-90s:** Ramp up — speed increases, obstacles more dense
- **90s+:** Hard mode — near-max speed, high obstacle density, sparse pickups

---

### 3.7 Game Manager & Flow

**GameManager.cs (Singleton):**
```csharp
public enum GameState { Menu, Playing, GameOver }

public GameState currentState;
public float distanceTraveled;
public float startTime;

void Start() {
    StartGame();
}

public void StartGame() {
    currentState = GameState.Playing;
    startTime = Time.time;
    distanceTraveled = 0;
    Time.timeScale = 1f;
    Cursor.lockState = CursorLockMode.Locked;
    // Reset frost, player position, etc.
}

public void GameOver() {
    currentState = GameState.GameOver;
    Time.timeScale = 0f;  // Optional: freeze time briefly
    Cursor.lockState = CursorLockMode.None;
    
    // Calculate final distance
    distanceTraveled = PlayerController.Instance.transform.position.z;
    
    // Show game over UI
    GameOverUI.Instance.Show(distanceTraveled);
    
    // Play crash sound
    AudioSource.PlayClipAtPoint(crashSound, Camera.main.transform.position);
}

public void Restart() {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
```

**ScoreManager.cs:**
```csharp
public float GetDistance() {
    return PlayerController.Instance.transform.position.z;
}

public string GetFormattedDistance() {
    return $"{Mathf.FloorToInt(GetDistance())}m";
}

// Optional: Save high score
public void SaveHighScore(float distance) {
    float currentHigh = PlayerPrefs.GetFloat("HighScore", 0);
    if (distance > currentHigh) {
        PlayerPrefs.SetFloat("HighScore", distance);
        PlayerPrefs.Save();
    }
}
```

---

### 3.8 UI System

**HUD (Always visible during gameplay):**
- Current distance (top center or corner)
- Optional: Current speed indicator
- Minimal, non-intrusive design

**Game Over Screen:**
- "Game Over" title
- Final distance traveled
- High score (if beaten)
- "Restart" button (Spacebar or click)
- "Quit" button (optional)

**Implementation (Canvas + TextMeshPro):**
```csharp
// GameOverUI.cs
public TextMeshProUGUI distanceText;
public TextMeshProUGUI highScoreText;
public GameObject gameOverPanel;

public void Show(float distance) {
    gameOverPanel.SetActive(true);
    distanceText.text = $"Distance: {Mathf.FloorToInt(distance)}m";
    
    float highScore = PlayerPrefs.GetFloat("HighScore", 0);
    if (distance > highScore) {
        highScoreText.text = "NEW HIGH SCORE!";
        ScoreManager.Instance.SaveHighScore(distance);
    } else {
        highScoreText.text = $"High Score: {Mathf.FloorToInt(highScore)}m";
    }
}

public void Hide() {
    gameOverPanel.SetActive(false);
}

public void OnRestartButton() {
    GameManager.Instance.Restart();
}
```

---

### 3.9 Audio System

**Required Audio:**
- **SFX:**
  - Pickup collect (warm, satisfying sound)
  - Crash impact (sharp, final sound)
  - Wind/whoosh loop (ambient, intensifies with speed)
- **Music (Optional):**
  - Chill atmospheric background track

**Implementation:**
- Attach `AudioSource` to Player for wind loop
- Use `AudioSource.PlayClipAtPoint()` for one-shots (pickup, crash)
- Adjust wind loop pitch/volume based on current speed:
  ```csharp
  windAudioSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed / maxSpeed);
  windAudioSource.volume = Mathf.Lerp(0.3f, 0.8f, speed / maxSpeed);
  ```

---

### 3.10 Visual Effects & Polish

**Snow Particle System:**
- Particle System component attached to camera or world
- Emit snow flakes falling downward and drifting past
- Increase emission rate with speed for sense of velocity

**Settings:**
```
- Shape: Box, wide and tall in front of camera
- Start Speed: Varies with player speed
- Start Lifetime: 2-4 seconds
- Emission Rate: 50-200 particles/sec (increases with speed)
- Size: Small (0.05 - 0.15)
- Color: White with slight transparency
```

**Pickup Glow:**
- Material with Emission enabled (bright orange/yellow)
- URP Bloom post-processing effect to make glows visible
- Optional: Small particle system with warm glow

**Crash Effect:**
- Brief camera shake on collision
- Screen flash (white or red tint for 0.2s)
- Particle burst at crash point

**Environmental Details (Low Priority):**
- Skybox with overcast/snowy sky
- Fog (exponential fog) for depth/atmosphere
- Distant mountain silhouettes (static mesh or skybox)

---

## 4. IMPLEMENTATION SCHEDULE (Game Jam Timeline)

### Day 1 — Core Mechanics (8-10 hours)

#### Phase 1A: Project Setup (1 hour)
- [ ] Verify URP is configured correctly
- [ ] Install necessary packages (Post Processing, TMP)
- [ ] Create folder structure
- [ ] Create main scene with basic lighting

#### Phase 1B: Player Movement (2 hours)
- [ ] Create Player GameObject with capsule collider
- [ ] Implement `PlayerController.cs` — forward + lateral movement
- [ ] Add Camera as child, position at head height
- [ ] Implement `MouseLook.cs` — FPS camera rotation
- [ ] Test: Can move left/right, look around independently

#### Phase 1C: World & Terrain (1.5 hours)
- [ ] Create simple ground plane (or Unity Terrain)
- [ ] Apply snow material/texture
- [ ] Create basic `ChunkManager.cs` — spawn/despawn chunks
- [ ] Test: Chunks spawn ahead as player moves forward

#### Phase 1D: Obstacles (2 hours)
- [ ] Create obstacle sprites (tree, rock) or find placeholder art
- [ ] Set up billboard script or use Billboard Renderer
- [ ] Create obstacle prefabs with colliders
- [ ] Implement `ProceduralSpawner.cs` — place obstacles in lanes
- [ ] Test: Obstacles spawn in chunks, face camera

#### Phase 1E: Basic Collision (1 hour)
- [ ] Implement `PlayerCollision.cs` — detect obstacle hits
- [ ] Implement basic `GameManager.cs` — game over state
- [ ] Test: Hitting obstacle triggers game over

#### Phase 1F: Pickups (1.5 hours)
- [ ] Create pickup sprite and prefab
- [ ] Add emissive material to pickup
- [ ] Implement `Pickup.cs` — collision detection
- [ ] Test: Can collect pickups (no frost effect yet)

**End of Day 1 Goal:** Playable skiing with obstacles to avoid and pickups to collect. No frost system yet.

---

### Day 2 — Frost System & Difficulty (8-10 hours)

#### Phase 2A: Frost Overlay (3 hours)
- [ ] Create frost overlay texture with eye holes
- [ ] Set up full-screen Canvas with Image component
- [ ] Implement `FrostManager.cs` — frost accumulation
- [ ] Connect pickup collection to frost reduction
- [ ] Implement dynamic frost visuals (opacity/scale)
- [ ] Test: Frost increases over time, decreases on pickup

#### Phase 2B: Difficulty Scaling (2 hours)
- [ ] Implement `DifficultyManager.cs`
- [ ] Speed increase over time
- [ ] Obstacle density curve
- [ ] Test: Game gets harder as you progress

#### Phase 2C: Score & UI (2 hours)
- [ ] Implement `ScoreManager.cs` — distance tracking
- [ ] Create HUD with distance display
- [ ] Create Game Over UI with restart button
- [ ] Implement high score save/load
- [ ] Test: UI displays correctly, restart works

#### Phase 2D: Object Pooling (1 hour)
- [ ] Implement simple object pooling for obstacles/pickups
- [ ] Reuse objects instead of instantiate/destroy
- [ ] Test: Performance improved, no memory leaks

#### Phase 2E: Audio Integration (1.5 hours)
- [ ] Find/create placeholder audio files
- [ ] Implement wind loop with dynamic pitch/volume
- [ ] Add pickup and crash SFX
- [ ] Test: Audio feels responsive and immersive

**End of Day 2 Goal:** Complete core game loop with frost mechanic, difficulty scaling, and audio.

---

### Day 3 — Polish & Finalization (6-8 hours)

#### Phase 3A: Visual Effects (2 hours)
- [ ] Add snow particle system
- [ ] Configure URP Post-Processing (bloom for pickups)
- [ ] Add crash effects (camera shake, screen flash)
- [ ] Add fog for atmosphere
- [ ] Test: Game looks cohesive and atmospheric

#### Phase 3B: Balancing & Tuning (2 hours)
- [ ] Playtest and tune movement speeds
- [ ] Adjust frost accumulation/reduction rates
- [ ] Balance obstacle density and spacing
- [ ] Tune difficulty curve
- [ ] Test: Game feels fair and challenging

#### Phase 3C: Bug Fixes & Edge Cases (1.5 hours)
- [ ] Test all collision scenarios
- [ ] Fix any camera clipping issues
- [ ] Ensure UI works at different resolutions
- [ ] Handle pause/unpause (if implemented)
- [ ] Test restart functionality thoroughly

#### Phase 3D: Build & Publish (1 hour)
- [ ] Create builds for target platforms (Windows, Mac)
- [ ] Test builds outside Unity
- [ ] Package for submission (itch.io, game jam site)
- [ ] Write game description and controls

#### Phase 3E: Optional Polish (If time permits)
- [ ] Main menu scene with start button
- [ ] Settings menu (volume control)
- [ ] Additional obstacle/pickup variants
- [ ] Particle trail behind player (snow spray)
- [ ] Better sky/background visuals

**End of Day 3 Goal:** Complete, polished, playable game ready for submission.

---

## 5. ASSET REQUIREMENTS

### 3D Models/Meshes
- **Player:** Simple capsule (Unity primitive)
- **Ground:** Plane or basic terrain
- **Billboards:** Quad mesh (Unity primitive)

### 2D Sprites/Textures
- **Obstacles:**
  - Tree sprite (PNG with alpha, ~512x512)
  - Rock sprite (PNG with alpha)
  - Branch sprite (optional)
- **Pickups:**
  - Hot drink icon (bright, emissive)
  - Fire icon (optional variant)
- **UI/Overlay:**
  - Frost overlay texture (full-screen, with transparent eye holes)
  - Game over background (optional)
- **Terrain:**
  - Snow ground texture (tileable, ~1024x1024)
  - Normal map for snow (optional)

### Audio
- **SFX:**
  - Pickup collect (warm, positive)
  - Crash impact (sharp, negative)
  - Wind loop (ambient whoosh)
- **Music:** Optional atmospheric track

### Materials
- Snow ground material (standard/URP Lit)
- Billboard material (URP Unlit, transparent)
- Pickup glow material (URP Unlit, emissive)
- Frost overlay material (UI/Default)

**Asset Sources:**
- **Free Assets:** Unity Asset Store, OpenGameArt.org, Kenney.nl
- **Quick Creation:** Photoshop/GIMP for sprites, Audacity for audio
- **Procedural:** Use Unity's particle systems and procedural generation

---

## 6. CODE ARCHITECTURE

### Core Patterns
- **Singleton:** GameManager, FrostManager, ScoreManager (for easy global access)
- **Object Pooling:** Obstacles, pickups (for performance)
- **Component-based:** Separate concerns (movement, collision, visuals)

### Class Responsibilities

```
PlayerController
├── Movement (forward + lateral)
├── Speed management
└── Input handling (steering)

MouseLook
├── Camera rotation
└── Mouse input handling

PlayerCollision
├── Collision detection
└── Trigger events (obstacle/pickup)

FrostManager (Singleton)
├── Frost level tracking
├── Frost accumulation over time
├── Frost reduction (pickups)
└── Visual update (overlay)

ProceduralSpawner
├── Spawn obstacles in lanes
├── Spawn pickups
├── Random placement logic
└── Difficulty-based density

ChunkManager
├── Spawn terrain chunks ahead
├── Despawn chunks behind player
└── Chunk pooling

GameManager (Singleton)
├── Game state (playing/game over)
├── Game over trigger
├── Restart logic
└── Scene management

ScoreManager (Singleton)
├── Distance calculation
├── High score tracking
└── UI data provider

DifficultyManager
├── Speed scaling
├── Density curves
└── Time-based difficulty
```

### Communication Patterns
- **Events:** Use UnityEvents or C# events for loose coupling (e.g., OnPickupCollected)
- **Direct References:** Singletons for global managers (GameManager, etc.)
- **Dependency Injection:** Pass references via inspector where possible

---

## 7. TESTING CHECKLIST

### Movement & Camera
- [ ] Player moves forward constantly
- [ ] Lateral movement stays within bounds
- [ ] Mouse look rotates camera, not player body
- [ ] Can look behind while moving forward
- [ ] Camera doesn't clip through objects

### Collision & Game Over
- [ ] Hitting tree triggers game over
- [ ] Hitting rock triggers game over
- [ ] Game over screen displays correct distance
- [ ] Restart button works correctly

### Frost System
- [ ] Frost increases steadily over time
- [ ] Collecting pickup reduces frost
- [ ] Eye holes shrink/fog increases with frost
- [ ] Visual feedback is clear and noticeable

### Pickups
- [ ] Pickups spawn in chunks
- [ ] Pickups are collectible
- [ ] Collection plays sound/particle effect
- [ ] Pickup effect (frost reduction) works

### Difficulty
- [ ] Speed increases over time
- [ ] Obstacles become more dense over time
- [ ] Game feels progressively harder
- [ ] Difficulty curve feels fair

### UI & Scoring
- [ ] Distance counter updates in real-time
- [ ] Game over UI shows final score
- [ ] High score saves correctly
- [ ] UI is readable through frost overlay

### Audio
- [ ] Wind loop plays continuously
- [ ] Wind pitch/volume scales with speed
- [ ] Pickup sound plays on collection
- [ ] Crash sound plays on game over

### Performance
- [ ] Game runs at 60+ FPS
- [ ] No memory leaks (object pooling works)
- [ ] Chunks spawn/despawn correctly
- [ ] No stuttering during gameplay

---

## 8. SCOPE MANAGEMENT (What to Cut if Behind)

### Priority 1 (Must-Have — Core Game)
✅ Player forward movement + lateral steering  
✅ Mouse look (independent of movement)  
✅ Obstacle collision → game over  
✅ Basic frost overlay (even static eye holes)  
✅ Pickups that reduce frost  
✅ Distance score  
✅ Restart button  

### Priority 2 (Should-Have — Full Experience)
✅ Frost accumulation over time  
✅ Dynamic frost visuals (eye holes scale)  
✅ Difficulty scaling (speed + density)  
✅ Object pooling  
✅ Wind audio loop  

### Priority 3 (Nice-to-Have — Polish)
⚠️ Snow particle system  
⚠️ Pickup glow effects (bloom)  
⚠️ Crash visual effects (shake, flash)  
⚠️ High score save/load  
⚠️ Multiple obstacle/pickup variants  

### Priority 4 (Cut if Needed — Extras)
❌ Main menu scene  
❌ Settings menu  
❌ Background music  
❌ Advanced post-processing effects  
❌ Tutorial or instructions screen  

**If running out of time:** Focus on Priority 1 items first. A simple but working game is better than a complex but broken one.

---

## 9. RISK MITIGATION

### Common Pitfalls & Solutions

**Problem:** Frost overlay blocks UI/buttons  
**Solution:** Place frost overlay on separate UI layer, ensure game over UI renders on top

**Problem:** Player can see through frost by looking at certain angles  
**Solution:** Ensure frost overlay is Screen Space - Overlay mode, always covers camera

**Problem:** Billboard sprites flicker or don't face camera  
**Solution:** Use Unity's Billboard Renderer or update LookAt in LateUpdate()

**Problem:** Collision detection misses fast-moving obstacles  
**Solution:** Use continuous collision detection on player Rigidbody, or larger collider triggers

**Problem:** Performance drops with many obstacles  
**Solution:** Implement object pooling early, limit active objects in scene

**Problem:** Difficulty curve too easy or too hard  
**Solution:** Playtest with multiple people, use AnimationCurve for easy tuning in inspector

**Problem:** Mouse look feels laggy or jittery  
**Solution:** Implement mouse look in LateUpdate(), use smooth damping if needed

---

## 10. POST-JAM IMPROVEMENTS (Future Ideas)

If you want to expand the game after the jam:

### Gameplay Enhancements
- Power-up variety (speed boost, invincibility, frost immunity)
- Multiple biomes (forest, glacier, avalanche zone)
- Day/night cycle affecting visibility
- Trick system (jumps off ramps for bonus points)

### Progression Systems
- Unlockable character skins
- Achievement system
- Daily challenges
- Leaderboards (online integration)

### Visual Upgrades
- Better 3D models for environment
- Animated sprites for obstacles
- Weather effects (blizzard, clear sky)
- Customizable ski equipment

### Audio Improvements
- Dynamic music that intensifies with speed
- Character breathing/exertion sounds
- Environmental audio (wind gusts, snow crunch)

### Technical Optimizations
- LOD system for distant objects
- Occlusion culling
- GPU instancing for obstacles
- Mobile port (touch controls)

---

## 11. CONTROLS & INPUT MAPPING

### Keyboard + Mouse
- **W / Up Arrow:** (Optional) Manual speed boost
- **A / Left Arrow:** Steer left
- **S / Down Arrow:** (Optional) Slow down
- **D / Right Arrow:** Steer right
- **Mouse Movement:** Look around (FPS style)
- **ESC:** Pause/Menu (if implemented)
- **Space / R:** Restart (on game over screen)

### Gamepad (Optional)
- **Left Stick:** Lateral steering
- **Right Stick:** Look around
- **A Button:** Restart

**Note:** Keep controls simple for game jam scope. Mouse + keyboard is sufficient.

---

## 12. BUILD & SUBMISSION

### Build Settings
- **Platform:** PC, Mac & Linux Standalone
- **Target Resolution:** 1920x1080 (windowed or fullscreen)
- **Graphics API:** Default (DirectX 11 on Windows, Metal on Mac)

### Build Checklist
- [ ] Test build outside Unity editor
- [ ] Check all scenes are included in build
- [ ] Verify audio/assets load correctly
- [ ] Test on target platforms (Windows/Mac)
- [ ] Package with README (controls, credits)

### Submission Format (e.g., itch.io)
- ZIP file with executable + data folder
- Game name: "Endless Frost Skier" (or your chosen title)
- Cover image/screenshot
- Brief description with controls
- Tags: endless-runner, skiing, first-person, game-jam

---

## 13. FINAL NOTES

### Development Philosophy for Game Jams
1. **Start simple, iterate:** Get core loop working first, add features incrementally
2. **Playtest early and often:** Test every few hours to catch issues early
3. **Scope ruthlessly:** Cut features that don't directly support the core experience
4. **Use placeholders:** Ugly sprites/sounds are better than nothing, replace later
5. **Rest and take breaks:** Burnout kills productivity, pace yourself

### What Makes This Game Compelling
- **Unique mechanic:** Vision obstruction creates tension and forces risk/reward decisions
- **Simple but deep:** Easy to understand, hard to master
- **Escalating challenge:** Difficulty curve keeps player engaged
- **One-more-try factor:** Quick restarts encourage repeated attempts
- **Atmospheric immersion:** Frost + wind + speed create intense experience

### Success Metrics
- **Core goal:** Complete, playable game with no critical bugs
- **Stretch goal:** Polished experience with audio, particles, and juice
- **Dream goal:** Balanced, replayable game that feels like a unique experience

---

## QUICK START STEPS

### To begin implementation:

1. **Open Unity project** (already initialized)
2. **Verify URP setup** — Check Project Settings > Graphics
3. **Install packages** — Window > Package Manager:
   - Post Processing (if not installed)
   - TextMeshPro (if not installed)
4. **Create MainGame scene** — File > New Scene > Save as `MainGame.unity`
5. **Create folder structure** — Follow structure in Section 2
6. **Start with Day 1, Phase 1B** — Implement PlayerController first
7. **Build incrementally** — Test after each phase
8. **Reference this document** throughout development for guidance

---

**Good luck with your game jam! Remember: done is better than perfect. Focus on the core mechanic (frost-obscured skiing) and make it feel tense and satisfying. Everything else is secondary.**

---

## APPENDIX: Quick Reference Scripts

### Singleton Template
```csharp
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
```

### Billboard Facing Script
```csharp
public class Billboard : MonoBehaviour {
    private Camera mainCamera;
    
    void Start() {
        mainCamera = Camera.main;
    }
    
    void LateUpdate() {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180f, 0); // Face camera
    }
}
```

### Simple Object Pool
```csharp
public class ObjectPool : MonoBehaviour {
    public GameObject prefab;
    public int poolSize = 20;
    private Queue<GameObject> pool;
    
    void Start() {
        pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++) {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public GameObject GetObject() {
        if (pool.Count > 0) {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }
    
    public void ReturnObject(GameObject obj) {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

**END OF PLANNING DOCUMENT**
