# 🎮 ChunkSpawner — Endless 3D Runner in Unity

![Game View](https://github.com/Chandan-Baskey/ChunkSpawner/blob/37b5233d1c9b328d3788fbdba86dd3abc08482c9/Game-View.jpg)

> An endless 3D runner built in Unity featuring a dynamic chunk-based world generation system, smooth player controls, checkpoint respawning, portal teleportation, and an adaptive camera.

---

## 📋 Table of Contents

- [About the Project](#about-the-project)
- [Core Systems](#core-systems)
- [Script Reference](#script-reference)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Design Decisions](#design-decisions)
- [Future Improvements](#future-improvements)

---

## About the Project

ChunkSpawner is a Unity 2D/3D hybrid endless runner where the world scrolls toward the player using a pool-based chunk recycling system. Instead of generating an infinite world, a fixed number of chunks are spawned ahead, moved toward the camera, destroyed when they pass behind the player, and immediately replaced at the front — creating the illusion of infinite forward motion with minimal memory overhead.

**Tech Stack:**
- **Engine:** Unity (2D Physics + 3D world movement)
- **Language:** C#
- **Physics:** Rigidbody2D, Collider2D, OverlapBox wall detection
- **Scene Management:** UnityEngine.SceneManagement

---

## Core Systems

### 1. Chunk Spawning & Recycling

The heart of the game. `ChunkSpawner.cs` manages an infinite scrolling world using a fixed-size list of chunk GameObjects. Rather than generating truly infinite geometry, it recycles chunks in a rolling-window pattern:

```
[Chunk 0] [Chunk 1] [Chunk 2] ... [Chunk N]
     ↑ moves backward → destroyed → new chunk added at front ↑
```

**Key Parameters:**

| Field | Type | Description |
|---|---|---|
| `chunkPrefab` | GameObject | The prefab used for each terrain chunk |
| `chunkParent` | GameObject | Parent object to keep hierarchy clean |
| `chunkAmount` | int | Number of chunks active at once (default: 10) |
| `chunkLength` | int | Length of each chunk in world units (default: 10) |
| `moveSpeed` | float | Speed at which chunks scroll toward the player (default: 30) |

---

### 2. Player Control

`PlayerControl.cs` handles all player movement with smooth acceleration/deceleration and automatic wall detection + flipping.

**Movement Model:**
- The player always moves forward (determined by `localScale.x` sign — left or right)
- Holding the screen/mouse button accelerates; releasing decelerates
- On hitting a wall, the player automatically flips direction using `Physics2D.OverlapBox`

**Key Parameters:**

| Field | Type | Description |
|---|---|---|
| `speed` | int | Base movement speed |
| `acceleration` | float (1–10) | How fast the player ramps up/down |
| `wallLayer` | LayerMask | Which layer counts as a wall |
| `wallCheckPoint` | Transform | Origin of the wall-detection box |
| `wallCheckSize` | Vector2 | Size of the wall-detection overlap box |

**Platform Riding:**  
When `isOnPlatform` is true, the player's velocity inherits the platform's `Rigidbody2D` velocity, enabling moving platform support without parenting.

---

### 3. Game Control (Death & Respawn)

`GameControl.cs` manages the player's lifecycle:

- **Obstacle collision** → triggers `Die()` → player shrinks to zero scale → waits 0.5 seconds → teleports to last checkpoint → restores scale
- **Finish trigger collision** → loads the next scene in build order
- **Checkpoint update** → stores a `Vector2` respawn position

```csharp
IEnumerator Respawn(float duration)
{
    transform.localScale = new Vector3(0, 0, 0); // Visual "death"
    yield return new WaitForSeconds(duration);
    transform.position = checkpointPos;           // Teleport to checkpoint
    transform.localScale = new Vector3(1f, 1f, 1f); // Restore
}
```

---

### 4. Checkpoint System

`Checkpoint.cs` provides one-time-use checkpoints scattered through the level:

- On player contact, calls `GameControl.UpdateCheckpoint()` with the assigned `respawnPoint` transform position
- Swaps the sprite from `passive` to `active` to give visual feedback
- Disables its own collider to prevent re-triggering

This means each checkpoint can only be activated once per run, encouraging forward progress.

---

### 5. Portal Teleportation

`Portal.cs` implements a bidirectional teleport system using a `HashSet` to prevent infinite teleport loops:

```
Player enters Portal A → teleported to Portal B's position
Portal B adds player to its "cooldown" set → player exits Portal B → set cleared
```

The `HashSet<GameObject> portalObjects` acts as a per-portal cooldown, meaning the teleport fires exactly once per entry, regardless of how slowly the player moves through it.

---

### 6. Camera Control

`CameraControl.cs` provides smooth camera following with configurable world boundaries:

- Uses `Vector3.SmoothDamp` for lag-free smooth tracking
- `positionOffset` allows the camera to lead ahead of the player
- `xLimits` and `yLimits` clamp the camera within level bounds, preventing it from showing out-of-bounds geometry
- Camera Z is always forced to `-10` (standard Unity 2D camera depth)

---

## Script Reference

| Script | Attached To | Purpose |
|---|---|---|
| `ChunkSpawner.cs` | Spawner GameObject | Spawns, moves, and recycles terrain chunks |
| `PlayerControl.cs` | Player | Handles movement, acceleration, wall flipping |
| `GameControl.cs` | Player | Death, respawn, scene transitions |
| `Checkpoint.cs` | Checkpoint objects | Saves respawn position, one-time activation |
| `Portal.cs` | Portal objects | Bidirectional teleportation with loop prevention |
| `CameraControl.cs` | Main Camera | Smooth follow with clamped world bounds |

---

## How It Works

### Chunk Lifecycle

```
Start()
  └─ spawnChunk()
       └─ Instantiate N chunks along Z axis, spaced by chunkLength

Update() every frame
  └─ MoveChunk()
       └─ For each chunk:
            ├─ Translate chunk backward (toward camera) at moveSpeed
            └─ If chunk.z <= spawner.z - chunkLength:
                 ├─ Destroy(chunk)
                 ├─ chunk.RemoveAt(i)
                 └─ NewSpawnChunk() → add new chunk at the far end
```

### Player Input Loop

```
Update()
  └─ btnPressed = Input.GetMouseButton(0)

FixedUpdate()
  └─ UpdateSpeedMultiplier()    ← smooth ramp up/down
  └─ Apply velocity to Rigidbody2D
  └─ If moving AND wall detected:
       └─ Flip()   ← reverse localScale.x
```

### Death & Respawn Flow

```
Player hits "Obstacle" tag
  └─ Die()
       └─ StartCoroutine(Respawn(0.5f))
            ├─ Scale → (0,0,0)   [hide player]
            ├─ Wait 0.5s
            ├─ Position → checkpointPos
            └─ Scale → (1,1,1)   [restore player]
```

---

## Getting Started

### Prerequisites

- Unity 2021.3 LTS or newer (2D template recommended)
- Basic knowledge of Unity's component system

### Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Chandan-Baskey/ChunkSpawner.git
   ```

2. **Open in Unity Hub** → Open Project → select the cloned folder.

3. **Tag setup** — make sure the following tags exist in your project:
   - `Player`
   - `Obstacle`
   - `Finish`

4. **Layer setup** — create a `Wall` layer and assign it to your wall colliders. Set `wallLayer` in `PlayerControl` to match.

5. **Scene setup:**
   - Attach `ChunkSpawner` to an empty GameObject.
   - Assign `chunkPrefab` (your terrain chunk prefab) and `chunkParent`.
   - Attach `PlayerControl` + `GameControl` to your Player GameObject.
   - Attach `CameraControl` to your Main Camera.
   - For checkpoints: attach `Checkpoint.cs`, set `respawnPoint` Transform, assign `passive`/`active` sprites.
   - For portals: attach `Portal.cs` to each portal object, set `destination` to the partner portal's Transform.

6. **Build Settings** — add your scenes in order if using scene transitions (Finish trigger loads `buildIndex + 1`).

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Checkpoint.cs       # Checkpoint save + visual feedback
│   ├── Portal.cs           # Bidirectional teleport
│   ├── GameControl.cs      # Death, respawn, scene loading
│   ├── PlayerControl.cs    # Movement, wall flip, platform support
│   └── CameraControl.cs    # Smooth follow + bounds clamping
├── ChunkSpawner.cs         # Infinite chunk recycling system
├── Prefabs/
│   └── Chunk.prefab        # Terrain chunk prefab
└── Scenes/
    └── Level_01.unity
```

---

## Design Decisions

**Why move chunks instead of the player?**  
Moving the world instead of the player keeps the player at a predictable world position, simplifying physics interactions, camera clamping, and respawn logic. The player is effectively stationary while the world flows past.

**Why a `HashSet` in Portal?**  
A simple boolean flag would only work for a single object. The `HashSet<GameObject>` allows multiple objects (e.g., enemies, projectiles) to teleport independently through the same portal without interfering with each other's cooldown state.

**Why `localScale.x` for direction instead of a boolean?**  
Using the sign of `localScale.x` means the flip state is entirely driven by the visible sprite transform. There's no separate "facing direction" variable to go out of sync — the ground truth is always what you see on screen.

**Why disable the checkpoint collider instead of using a flag?**  
Disabling the `Collider2D` is cheaper than running a comparison check every physics frame. Once a checkpoint is claimed, it literally cannot receive collision events anymore.

---

## Future Improvements

- [ ] Object pooling for chunks (avoid `Instantiate`/`Destroy` overhead)
- [ ] Procedural chunk selection (randomize chunk prefabs from a weighted list)
- [ ] Score system tied to distance traveled or chunks survived
- [ ] Difficulty scaling (increase `moveSpeed` over time)
- [ ] Sound effects for checkpoint activation, death, and portal entry
- [ ] Mobile touch input abstraction layer in `PlayerControl`
- [ ] Animated death/respawn sequence instead of scale snap

---

## License

This project is open source. Feel free to use, modify, and distribute.

---

*Built with Unity · C# · 2D Physics*
