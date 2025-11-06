# AI Assistant Instructions for Book of Mormon Chess

This is a Unity-based chess game implementation with Book of Mormon theming and unique gameplay mechanics. Here are the key patterns and conventions to understand:

## Core Architecture

- `TurnManager`: Central game controller managing turns, moves, and board state
  - Singleton pattern via `TurnManager.Instance`
  - Handles piece selection, movement validation, and turn transitions 
  - Example: `TurnManager.tiles[x,y]` accesses board positions

- `Piece` base class: Abstract class all chess pieces inherit from
  - Core movement/validation in `GetMoves()` method
  - Uses `Vector2Int` for positions and movement patterns
  - Example: See `StriplingWarrior.cs` for unique piece implementation

- Board setup and initialization:
  - `BoardSetup` handles board creation and piece placement
  - Board size configurable via PlayerPrefs ("boardSize")
  - Pieces randomized on back row with `RandomizePieces()`

## Unique Game Mechanics

1. Faction system instead of traditional colors:
   ```csharp
   public enum Faction { Nephite, Lamanite, Inanimate }
   ```

2. Special piece: StriplingWarrior
   - Becomes wounded instead of dying
   - Tracks wounded state with turn counter
   - Returns to active state after healing period

3. AI player support:
   - `AiManager` handles computer moves
   - Basic prioritization: Check escapes > Kills > Random moves
   - Configurable via PlayerPrefs ("1isAI"/"2isAI")

## Common Development Tasks

1. Adding new piece types:
   - Inherit from `Piece` base class
   - Override `GetMoves()` for movement pattern
   - Add prefab to `backPiecePrefabs` array
   - See: `Queen.cs`, `Rook.cs` for examples

2. Movement patterns:
   - Use `Vector2Int[]` arrays for directions
   - Validate with `IsTileEmpty()` and `IsEnemyPiece()`
   - Handle edge cases with board size checks

3. Turn handling:
   - Subscribe to `TurnManager.OnMoveEnd` for post-move actions
   - Use `MoveEnd()` override for piece-specific behaviors
   - See: Pawn promotion in `Pawn.cs`

## Project Structure

- `Assets/Scripts/Gameplay/`: Core game logic
- `Assets/Scripts/Gameplay/Piece Scripts/`: Chess piece implementations
- `Assets/Scripts/SceneStart/`: Initialization and setup code
- `Assets/Prefabs/`: Piece and tile prefabs

## Common Pitfalls

1. Board coordinates:
   - Use `Vector2Int` for grid positions
   - Board origin (0,0) is bottom-left
   - Always check against `boardSize` for bounds

2. Turn management:
   - Use `TurnManager.Instance` for global access
   - Verify piece ownership before moves
   - Handle AI turns via `AiTurn()` method

3. Piece states:
   - Track special states (wounded, promoted, etc.)
   - Clean up event subscriptions in piece destruction
   - Update visual indicators for state changes