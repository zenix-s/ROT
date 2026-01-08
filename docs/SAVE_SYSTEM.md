# Save System Documentation

This document explains the save system architecture in Rot of Time.

## Architecture Overview

The save system uses a clean separation of concerns:

```
┌─────────────────┐     ToGameData()      ┌─────────────┐
│   GameManager   │ ───────────────────►  │   GameData  │
│  (Live State)   │                       │    (DTO)    │
│                 │ ◄───────────────────  │             │
└─────────────────┘   LoadFromGameData()  └─────────────┘
                                                │
                                                │ Save/Load
                                                ▼
                                         ┌─────────────┐
                                         │ SaveManager │
                                         │ (File I/O)  │
                                         └─────────────┘
                                                │
                                                ▼
                                          user://saves/
```

## Components

### SaveManager (Persistence Layer)
**File:** `Autoload/SaveManager.cs`

Handles all file I/O operations:
- `Save(slotId, GameData)` - Write data to disk
- `Load(slotId)` - Read data from disk
- `GetAllSlotInfo()` - List all save slots
- `SaveExists(slotId)` - Check if slot has data
- `DeleteSave(slotId)` - Remove save file

Signals: `SaveCompleted`, `LoadCompleted`, `SaveDeleted`

### GameManager (Runtime State)
**File:** `Autoload/GameManager.cs`

Holds live game state during gameplay:
- `ActiveSlot` - Current save slot
- `PlayTimeSeconds` - Session play time
- `CurrentLevel` - Current tower level
- `PlayerPosition` - Player position

Key methods:
- `ToGameData()` - Create DTO snapshot for saving
- `LoadFromGameData(data)` - Populate from loaded DTO
- `NewGame(slotId)` - Initialize fresh state
- `Save()` - Save via SaveManager
- `Load(slotId)` - Load via SaveManager
- `ClearSession()` - Reset state

### GameData (DTO)
**File:** `Core/GameData/GameData.cs`

Pure data class for serialization:
```csharp
public class GameData
{
    public int Version { get; set; }
    public int SlotId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSavedAt { get; set; }
    public double PlayTimeSeconds { get; set; }
    public TowerLevel CurrentLevel { get; set; }
    public PlayerData Player { get; set; }
}
```

## File Locations

| Component | Path |
|-----------|------|
| SaveManager | `Autoload/SaveManager.cs` |
| GameManager | `Autoload/GameManager.cs` |
| GameData | `Core/GameData/GameData.cs` |
| PlayerData | `Core/GameData/PlayerData.cs` |

### Save File Location

Save files are stored in Godot's user data directory:
- **Linux:** `~/.local/share/godot/app_userdata/RotOfTime/saves/`
- **Windows:** `%APPDATA%/Godot/app_userdata/RotOfTime/saves/`
- **macOS:** `~/Library/Application Support/Godot/app_userdata/RotOfTime/saves/`

Files: `save_1.json`, `save_2.json`, `save_3.json`

## Usage Examples

### Starting a New Game
```csharp
GameManager.Instance.NewGame(1);  // Creates slot 1, saves initial state
```

### Loading a Game
```csharp
if (GameManager.Instance.Load(1))
{
    // Access live state
    var level = GameManager.Instance.CurrentLevel;
    var position = GameManager.Instance.PlayerPosition;
}
```

### Manual Save
```csharp
GameManager.Instance.Save();  // Saves to active slot
```

### Auto-Save
Auto-save triggers automatically on scene transitions. No code needed.

### Listing Save Slots
```csharp
var slots = SaveManager.Instance.GetAllSlotInfo();
foreach (var slot in slots)
{
    if (slot != null)
        GD.Print($"Slot {slot.SlotId}: {slot.GetFormattedPlayTime()}");
}
```

### Deleting a Save
```csharp
SaveManager.Instance.DeleteSave(1);
```

### Updating Game State
```csharp
GameManager.Instance.UpdatePlayerPosition(player.GlobalPosition);
GameManager.Instance.UpdateCurrentLevel(TowerLevel.Level2);
```

## Data Flow

### Saving
```
GameManager.Save()
    → GameData dto = ToGameData()     // Create snapshot
    → SaveManager.Save(slot, dto)     // Write to disk
    → JSON file created
```

### Loading
```
GameManager.Load(slotId)
    → GameData dto = SaveManager.Load(slotId)  // Read from disk
    → LoadFromGameData(dto)                     // Populate state
    → GameManager ready to use
```

### New Game
```
GameManager.NewGame(slotId)
    → Initialize fresh state
    → ActiveSlot = slotId
    → Save()  // Create initial save file
```

## Adding New Saveable Data

### Step 1: Add to GameData (DTO)
```csharp
// Core/GameData/GameData.cs
public int PlayerHealth { get; set; } = 100;
```

### Step 2: Add to GameManager (Runtime)
```csharp
// Autoload/GameManager.cs
public int PlayerHealth { get; set; } = 100;
```

### Step 3: Update ToGameData()
```csharp
public GameData ToGameData()
{
    return new GameData
    {
        // existing...
        PlayerHealth = PlayerHealth
    };
}
```

### Step 4: Update LoadFromGameData()
```csharp
public void LoadFromGameData(GameData data)
{
    // existing...
    PlayerHealth = data.PlayerHealth;
}
```

## Save File Format (JSON)

```json
{
  "Version": 1,
  "SlotId": 1,
  "CreatedAt": "2026-01-08T12:00:00",
  "LastSavedAt": "2026-01-08T14:30:00",
  "PlayTimeSeconds": 3600,
  "CurrentLevel": 0,
  "Player": {
    "PositionX": 100,
    "PositionY": 200
  }
}
```

## Signals

```csharp
// SaveManager signals
SaveManager.Instance.SaveCompleted += (slotId) => { };
SaveManager.Instance.LoadCompleted += (slotId) => { };
SaveManager.Instance.SaveDeleted += (slotId) => { };
```

## Best Practices

1. **Use GameManager for runtime state** - Don't access GameData directly during gameplay
2. **SaveManager is stateless** - It only handles file operations
3. **Call Save() at checkpoints** - Don't rely solely on auto-save
4. **Update positions regularly** - Call `UpdatePlayerPosition()` in player's `_Process()`
5. **Clear session on menu return** - Call `GameManager.Instance.ClearSession()`
