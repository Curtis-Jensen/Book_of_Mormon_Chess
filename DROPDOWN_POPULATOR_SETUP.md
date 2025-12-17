# Dropdown Populator Setup Guide

## Overview
The `DropdownPopulator` script automatically populates dropdown menus with piece options based on `SceneConfig` settings. This centralizes piece configuration so you only need to define your piece sets in **one place** instead of three or more.

## How It Works

### Before (Manual Approach)
Previously, you had to manually:
1. Define piece prefabs in `SceneConfig` array in the Inspector
2. Keep the prefab list in sync across multiple scenes
3. Remember to update the dropdown options when adding new piece combinations

### After (Automatic Approach)
Now:
1. Define your piece combinations once in `SceneConfig`
2. `DropdownPopulator` automatically populates the dropdown with those options
3. Everything stays synchronized automatically

## Setup Instructions

### 1. Add DropdownPopulator to Your Main Menu Scene

In your main menu scene that has the game mode dropdown:

1. Select the GameObject that contains the `TMP_Dropdown` component
2. Add the `DropdownPopulator` script as a component
3. In the Inspector, assign:
   - **Game Mode Dropdown**: The TMP_Dropdown component
   - **Piece Sets**: Your PieceSets ScriptableObject (for reference, if needed)
   - **Scene Configs**: The array of SceneConfig objects

### 2. Update Your SceneLoader Button

Make sure your "Start Game" button calls `SceneLoader.SetupNewScene()` (it should already be doing this).

The updated `SceneLoader` will now:
- Check if `DropdownPopulator` is available and use it
- Fall back to the original logic if `DropdownPopulator` isn't set up
- Automatically save the selected config's prefabs to PlayerPrefs

## Usage Example

Suppose you have these piece configurations:

```csharp
// In your SceneConfig array in SceneLoader:
sceneConfigs[0]:
  - dropDownOptionName: "Standard"
  - sceneName: "GameScene"
  - backRowPrefabs: [King, Queen, Rook, Bishop, Knight, Pawn, Pawn, Pawn]

sceneConfigs[1]:
  - dropDownOptionName: "Classic"
  - sceneName: "GameScene"
  - backRowPrefabs: [King, Queen, Rook, Rook, Knight, Knight, Pawn, Pawn]

sceneConfigs[2]:
  - dropDownOptionName: "Hoard"
  - sceneName: "HoardGameScene"
  - backRowPrefabs: [King, Queen, Bishop]
```

When you press Play in the main menu:
1. The dropdown automatically shows: "Standard", "Classic", "Hoard"
2. Player selects an option (e.g., "Classic")
3. When they click "Start Game", `SetupNewScene()` is called
4. The selected config's prefabs are automatically saved
5. The game scene loads with the correct pieces

## Adding New Piece Combinations

To add a new game mode:

1. **Only modify `SceneConfig` array in SceneLoader** (in the Inspector)
2. Add a new entry with:
   - A descriptive name
   - The scene to load
   - Your piece prefabs
3. The dropdown will automatically include the new option

**No code changes needed!** Just update the Inspector.

## Optional: Customize Display Names

If you want to customize how options appear in the dropdown, modify the `GenerateDisplayName()` method in `DropdownPopulator.cs`:

```csharp
private string GenerateDisplayName(string configName, GameObject[] prefabs)
{
    // Current behavior: Shows just the config name
    // You could modify this to show piece details, difficulty, etc.
    return configName;
}
```

## Methods Available

### `PopulateDropdown()`
Automatically populates the dropdown with options from SceneConfigs.
Called automatically on Start().

### `GetSelectedConfigIndex()`
Returns the currently selected dropdown option index.

```csharp
int selectedIndex = dropdownPopulator.GetSelectedConfigIndex();
```

### `GetSelectedConfig()`
Returns the currently selected SceneConfig object.

```csharp
SceneConfig config = dropdownPopulator.GetSelectedConfig();
Debug.Log(config.sceneName);
```

### `SaveDropdownSelection()`
Saves the current dropdown selection and its prefabs to PlayerPrefs.
Called automatically by `SceneLoader.SetupNewScene()`.

## Technical Details

The `DropdownPopulator` works by:

1. **Reading SceneConfigs**: Gets the list of available game modes
2. **Extracting Piece Names**: Pulls the prefab names from each config
3. **Populating Dropdown**: Creates readable display names and adds them to the dropdown
4. **Saving Selection**: When a scene is loaded, saves the prefab names to PlayerPrefs
5. **BoardSetup Loading**: `BoardSetup` then loads these prefabs at game start

This keeps the piece configuration **centralized in one place** while maintaining backward compatibility with the existing system.

## Troubleshooting

**Dropdown shows no options:**
- Make sure `SceneConfig[]` array is populated in the Inspector
- Verify `backRowPrefabs` arrays in each config have items
- Check the Console for warning messages

**Pieces don't load correctly in game:**
- Verify prefab names in `SceneConfig` match actual prefab names
- Ensure prefabs are in the Resources/Prefabs/ folder
- Check BoardSetup.cs logs for prefab loading errors

**Changes to dropdown options don't appear:**
- Call `PopulateDropdown()` again after modifying SceneConfigs
- Or restart the scene (PopulateDropdown is called in Start())
