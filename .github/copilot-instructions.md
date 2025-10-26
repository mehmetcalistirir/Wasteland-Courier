# Copilot Instructions for Wasteland-Courier (Dawn Defenders)

## Project Overview
**Dawn Defenders** is a 2D top-down survival game built in Unity using Turkish comments/documentation. Players transport vital cargo through a post-apocalyptic world with day/night cycles, resource gathering, and mutant defense mechanics.

## Core Architecture

### Singleton Pattern Usage
Most managers use the standard Unity singleton pattern:
```csharp
public static ManagerName Instance { get; private set; }
void Awake() {
    if (Instance == null) Instance = this; 
    else Destroy(gameObject);
}
```
Key singletons: `GameManager`, `WeaponCraftingSystem`, `CaravanInventory`, `AudioManager`, `MusicManager`, `PauseMenu`

### Component Communication
- **Static Events**: `DayNightCycle.OnDayNightChanged` for game-wide day/night transitions
- **Sound System**: `SoundEmitter.EmitSound()` attracts enemies, `AnimalSoundEmitter.EmitSound()` scares animals
- **Game State**: `GameStateManager.IsGamePaused` consolidates pause states from menus/crafting

### Input System (Unity New Input System)
- **PlayerControls.inputactions**: WASD movement, mouse fire, left shift sprint
- **Input handling pattern**: Subscribe to `performed`/`canceled` events in `OnEnable`/`OnDisable`
- **Action maps**: Switch to "Gameplay" map in `GameSceneBootstrap` to prevent input conflicts

## Key Systems

### Day/Night Cycle (`DayNightCycle.cs`)
- **Core mechanic**: Timer-based system with configurable day/night durations
- **Integration points**: Spawns enemies at night, regenerates resources at dawn, triggers audio/lighting changes
- **Event system**: Use `OnDayNightChanged` static event for system-wide notifications
- **Enemy behavior**: Certain enemies burn during day (`burnsInDay = true`)

### Weapon System Architecture
- **WeaponSlotManager**: Central weapon switching (1-8 keys) with per-slot ammo tracking
- **CaravanInventory**: Instance-based weapon storage with unique IDs and ammo state
- **WeaponCraftingSystem**: Blueprint-based crafting using resource requirements
- **Pattern**: Each weapon slot maintains separate `ammoInClips[]` and `totalReserveAmmo[]` arrays

### Enemy AI & Audio
- **Type-based targeting**: Normal/Fast → Player, Armored/Exploder → Caravan
- **Sound attraction**: `Enemy.OnSoundHeard(Vector2)` temporarily redirects enemies to noise source
- **Wander system**: `EnemyWanderDriver` overrides target with waypoints for patrol behavior
- **Audio integration**: Enemies play random ambient sounds and react to player gunshots

### Inventory & Resources
- **Inventory.cs**: Event-driven system with `OnChanged` for UI updates
- **PlayerStats.cs**: Centralized resource storage (gold, health, move speed, ItemData references)
- **Pattern**: Use `TryAdd()` for safe inventory operations, `HasEnough()` for crafting checks

## Development Patterns

### Turkish Documentation
- Comments and debug logs are in Turkish
- Variable names use English but class documentation is Turkish
- Inspector tooltips often include Turkish explanations

### Error Handling
- **Null checks**: Always verify manager instances before use (`if (Manager.Instance != null)`)
- **Component safety**: Use `GetComponent<>()` with null checks, especially for cross-component communication
- **Scene transitions**: `GameSceneBootstrap` handles initialization and prevents duplicate EventSystems

### Animation System
- **8-directional movement**: `PlayerMovement.DiscretizeDirection()` snaps mouse aim to cardinal directions
- **Pattern**: Set `moveX`, `moveY` floats and `isMoving` bool on Animator
- **Facing direction**: Use `PlayerMovement.FacingDirection` static property for weapon orientation

### Performance Considerations
- **Object pooling**: Not implemented - instantiate/destroy pattern used for enemies/projectiles
- **Update optimization**: Many systems use coroutines instead of Update() for timed events
- **Audio**: Use `AudioSource.PlayClipAtPoint()` for one-shot sounds, avoid creating permanent AudioSources

## Dependencies & Tools
- **NavMeshPlus**: Third-party 2D pathfinding (h8man.2d.navmeshplus)
- **Universal Render Pipeline**: Used for 2D lighting effects
- **TextMeshPro**: For all UI text elements
- **Cinemachine**: For camera management
- **Unity Input System**: New input system for player controls

## Common Workflows

### Adding New Weapons
1. Create `WeaponBlueprint` ScriptableObject with requirements
2. Add to `WeaponCraftingSystem.availableBlueprints`
3. Set `weaponSlotIndexToUnlock` for slot assignment
4. Configure ammo data in `WeaponData` ScriptableObject

### Scene State Management
- Use `GameSceneBootstrap` for scene initialization
- Reset game state with `GameStateManager.ResetGameState()`
- Save progress with `SaveSystem.MarkLevelComplete(levelIndex)`

### Audio Integration
- Global volume control through `AudioManager` with mixer groups
- Music transitions handled by `MusicManager` singleton (survives scene changes)
- Enemy audio: Add clips to `ambientSounds`, `hurtSounds`, `deathSound` arrays

When modifying this codebase, maintain the established singleton patterns, respect the day/night event system, and ensure new components integrate properly with the sound-based enemy attraction mechanics.