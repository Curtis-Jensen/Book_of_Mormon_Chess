# Chess Journey: For Beginners

Notes for AI assistants (and future-us) working on the "Chess Journey" spinoff of Book of Mormon Chess. A secular/silly chess mode built for Curtis's son Dawson. Branch: `Chess_Journey_For_Beginners`.

---

## Origin

"Chess Journey" was originally a mode built directly into BOM Chess, then deleted (`Got rid of Journey chess`, `Deleted the journey scene because it was vestigial and never resulted in a build`) because it never went anywhere on its own. The piece art it motivated survived in `Assets/Prefabs/Pieces/Secular Pieces/` (Classic, Halloween, Christmas, Pixel Art, Birds) and stayed wired into the shared `PieceSets` ScriptableObject even after the mode itself was cut.

This time it's being revived with a real reason to exist: Dawson wants to play chess, and Book of Mormon theming isn't the point for him -- silly pieces are. So Chess Journey is being rebuilt as its own thing rather than a forgotten sub-menu.

---

## Architecture: what's shared, what's separate

The core design call (made explicit, not accidental): **menus diverge, gameplay doesn't.**

- **Separate**: menu scenes. `Assets/Scenes/CJ Main Menu.unity` is a duplicate of `Assets/Scenes/BOM Main Menu.unity`, kept as its own file. Menus are presentation -- they're *supposed* to look and feel different between a devotional chess game and a kid's silly chess game, so duplicating them costs nothing.
- **Shared**: the board/gameplay scene (`Duel Scene.unity`), `Piece`/`TurnProgresser`/`PieceSpawner` scripts, and the `PieceSets` ScriptableObject (`Assets/Scripts/Gameplay/PieceSets.cs`, asset at `Assets/Prefabs/Pieces/PieceSets.asset`). Any chess-logic improvement made for BOM Chess should reach Chess Journey for free, and vice versa, without manually re-applying a patch to two copies.

The skin/style system already existed before this project started and needed no new engineering:
- `PieceSets.spriteSets[]` holds named sprite sets. `PieceSets.colorSets[]` is a separate, unrelated array for board-piece colors (Red/Yellow/Green/Blue/Purple/White/Black). The `spriteSets[]` order is meant to be the ONE source of truth for both which index maps to which set (used by `PieceSpawner` at runtime) and what order players see them in a dropdown -- see the bugs section below for how that broke and got fixed.
- `PieceSpawner.SetSprite`/`SetColor` (`Assets/Scripts/SceneStart/PieceSpawner.cs`) read `PlayerPrefs.GetInt(player.name + "style")` / `+"color"` per player to pick which set/color to render. In `Duel Scene.unity`, the `Player[]` array on `PieceSpawner` is named literally `player1`/`player2`/`player3`/`player4`, so the actual keys are `player1style`, `player2style`, etc.
- `DropdownPopulator` (`Assets/Scripts/Ui/DropdownPopulator.cs`) populates a `TMP_Dropdown` from `pieceSets.spriteSets[].name`, in that array's order, so the dropdown always matches whatever order the ScriptableObject holds. It now runs in the Editor too (via `[ExecuteAlways]`, see bug 6), and has a custom Inspector (`Assets/Scripts/Editor/DropdownPopulatorEditor.cs`) with a **Populate Now** button for forcing a refresh after editing `PieceSets`.
- `DropdownSaver` (`Assets/Scripts/Ui/DropdownSaver.cs`) saves a dropdown's `value` to a `PlayerPrefs` key (`toSave`) on change, and `SettingsDefaultsSeeder` (`Assets/Scripts/Ui/SettingsDefaultsSeeder.cs`) walks the whole scene at `Awake()` (including inactive objects) seeding any missing key from each `DropdownSaver`'s dropdown. The dropdown's own `Value` field in the Inspector is the single place to set a default -- there's no separate `Default Value` field (removed on purpose, see bugs below).

---

## Bugs found and fixed while wiring this up

These were real latent bugs in shared UI scripts, not one-off mistakes -- worth knowing about since they affect BOM Chess's settings UI too, not just Chess Journey's:

1. **`TMP_Dropdown.ClearOptions()` resets `value` to 0 as a side effect.** `DropdownPopulator.PopulateDropdown()` calls `ClearOptions()` then `AddOptions()` every `Start()`, which was silently wiping out whatever `DropdownSaver` had just loaded from `PlayerPrefs` in `OnEnable()` (which always runs before `Start()`). Fixed by capturing `dropdown.value` before clearing and restoring it after.
2. **`DropdownSaver.OnValidate()` was calling `Save()`.** `OnValidate()` fires on *any* Inspector edit to the component -- including editing an unrelated field -- and `Save()` wrote the dropdown's current runtime `value` to `PlayerPrefs`, silently re-clobbering a value you thought you'd just changed. Removed the `Save()` call from `OnValidate()`.
3. **Removed the separate `defaultValue` field on `DropdownSaver` entirely**, per Curtis's call that having both a dropdown's own `Value` field *and* a duplicate `Default Value` field on the saver violated DRY. Now the dropdown's own `Value` field is the single source of truth; `SeedIfMissing()` reads from `dropdown.value` directly.
4. **Duplicate dropdowns writing to the same `PlayerPrefs` key, caused by nested prefab structure.** `CJ Main Menu.unity` ended up with two separate style-dropdown UI elements per player both wired to `toSave: player1style` (one under a `Style (DO NOT REMOVE)` node inside the shared `Nephites`/`Lamanites` prefab, one a separately-added `Piece Style` object) -- so whichever got seeded first by `SettingsDefaultsSeeder` silently won. Root cause was parent-prefab structure blocking normal deletion (Unity's "Cannot restructure Prefab instance" error can come from a *parent* several levels up, not the object you're clicking on -- check the Inspector header of parents for Prefab Select/Open/Overrides buttons). Curtis found and fixed the actual prefab structure himself; the redundant duplicate dropdown was then removed, leaving one dropdown per player.
5. **The real, sneaky bug: the canonical `Style (DO NOT REMOVE)` dropdown had a hand-typed, alphabetically-ordered `Options` list that didn't match `PieceSets.spriteSets[]`'s real order, and had no `DropdownPopulator` attached to regenerate it.** So picking an option by its displayed label saved a `PlayerPrefs` index based on the alphabetical position, but `PieceSpawner` read that index against the *real* array order -- e.g. picking "Pixel Art" (alphabetical position 6) actually saved index 6, which in the real array is "Birds", so the board rendered bird pieces instead. This was the "3+ places order is set" problem: the real array, the runtime-populated dropdown, and a stale hand-authored list all claiming to represent the same ordering. Fixed by attaching `DropdownPopulator` (pointed at `PieceSets.asset`) to the `Style (DO NOT REMOVE)` dropdown so its list is always regenerated from the one real array, and deleting the redundant `Piece Style` object once this was confirmed working.
6. **`DropdownPopulator` only populated in `Start()`, which doesn't run in the Editor outside Play mode**, so adding the component or reordering `PieceSets` didn't visibly update the dropdown until you pressed Play. Fixed by adding `[ExecuteAlways]` to the class (so `Start()` also runs in Editor) plus a `PopulateNow()` method exposed as a **Populate Now** button in a custom Inspector (`DropdownPopulatorEditor.cs`), for forcing a refresh after editing `PieceSets` without a scene reload.

---

## Things to watch so the split stays clean

- **Don't let BOM-specific content leak into the shared board scene.** Nephites Last Stand references, Stripling Warrior lore, and the `StriplingWarrior` field baked into `SpriteSet` are Book-of-Mormon-specific. If the shared board scene ever assumes that content exists, Chess Journey stops being a clean mode and starts being "BOM Chess with reskinned pieces and dead references."
- **Chess Journey's "Play" flow needs to land in `Duel Scene.unity`, not `Nephites' Last Stand.unity`.** They look similar in a screenshot (both have a chess-like board) but Nephites' Last Stand is the wave-defense mode with its own hardcoded unit prefabs -- it does not go through `PieceSpawner`/`PieceSets` the same way, so secular styles picked in the menu won't apply there. Confirm which scene a given "Play"/mode button actually loads before assuming a style bug.
- **PlayerPrefs keys collide across modes.** `"1style"` / `"1color"` etc. are shared by player name regardless of mode, so if someone plays BOM Chess then Chess Journey (or vice versa) in the same session, the wrong style can carry over. Each mode's menu should set/seed the desired style every time it loads, not just once.
- **"Chess Journey" as a name isn't locked in yet.** Once it spreads into more scene names, PlayerPrefs key prefixes, and build settings entries, renaming gets more mechanical work. Worth nailing the final name down early rather than after it's everywhere.
- **Nephites/Lamanites piece sets being included as options is fine, not blasphemous** -- they're just skins with no scripture text or mode context attached at that point. Keep them lower in the dropdown list below the silly options, and don't make them the default, and it reads as harmless cross-promotion rather than irreverence.
- **A direct file write outside the repo (e.g. to an app-managed session folder) is not durable.** It can get silently overwritten by a background sync. Anything meant to persist belongs in this repo (or committed/pushed) rather than trusted to survive elsewhere unverified.

---

## Project Status

Active, early. Branch `Chess_Journey_For_Beginners` (3 commits pushed to `origin` as of last sync):
- Branch created off `Mac_Editing`.
- `CJ Main Menu.unity` duplicated from `BOM Main Menu.unity` and separated out as its own scene, with a secular default piece style wired through the existing dropdown/PlayerPrefs system (no hardcoded style in script, per Curtis's DRY call).
- Fixed all the dropdown/ordering bugs above (items 1-6), which affect BOM Chess's settings UI too, not just Chess Journey.
- Style dropdowns for both players confirmed correctly showing/saving the real `PieceSets` order (verified Pixel Art and Birds both render correctly on the board).
- Not yet done: any visual/menu-art work distinguishing `CJ Main Menu` from `BOM Main Menu` beyond the background swap already made (`Assets/Prefabs/CJ Background.png`); deciding whether other BOM modes (Nephites' Last Stand, etc.) should be reachable from Chess Journey at all.

---

## Individual Piece Styles (mix-and-match)

Branch `Individual_Piece_Styles`, off `Chess_Journey_For_Beginners`. Dawson wants to mix a King from one set with a Queen from another instead of picking one bundled `SpriteSet` for everything.

**Data model**: `PieceSets` gained a `PieceStyleOption { name, transformScale, sprite }` and one array per piece type (`kingOptions`, `queenOptions`, `rookOptions`, `bishopOptions`, `knightOptions`, `pawnOptions`, `striplingWarriorOptions`), replacing the bundled `SpriteSet[] spriteSets` as the thing gameplay actually reads. `PieceSets.GetOptions(pieceTypeName)` switches on the piece prefab's name to return the right array. The old `spriteSets[]` field and `SpriteSet` class are kept only until the migration is fully trusted, then should be deleted.

**Migration**: `Assets/Scripts/Editor/PieceSetsMigrator.cs` (`Tools > BOM Chess > Migrate Piece Sets To Per-Piece Styles`) copies the old bundled data into the new per-type arrays so nothing had to be manually re-dragged in the Inspector.

**Scale**: the old bundled sets had wildly inconsistent `transformScale` (0.17 to 5) because each ChatGPT-generated sprite came in at a different resolution and got hand-tuned until it looked right. `Assets/Scripts/Editor/PieceScaleWindow.cs` (`Tools > BOM Chess > Auto Scale Piece Sprites`) computes `transformScale` per option from the sprite's real pixel height ÷ its PPU against a target world-unit height, so no more manual slider-tuning -- works automatically for future ChatGPT art too.

**Ghost bug found along the way**: `Piece.InstantiateDeathEffects()` spawned the death "ghost" from a prefab with a hardcoded `transform.localScale` (0.17), so a piece from a differently-scaled set (e.g. Pixel Art at scale 5) died into a wrong-sized ghost. Fixed by copying `transform.localScale` from the dying piece onto the ghost instance instead of trusting the prefab default.

**PlayerPrefs keys**: changed from one key per player (`player.name + "style"`) to one key per player *and* piece type (`player.name + "style" + pieceTypeName`, e.g. `1styleKing`). `PieceSpawner.SetSprite` reads the new scheme.

**StriplingWarrior / "Invincible"**: this was never really a per-set thing -- every old `SpriteSet` pointed at the exact same `StriplingWarrior_Active` sprite GUID, it just got dragged along because the bundled data model forced every set to have one. Decision: keep it as its own dropdown (Dawson may ask for a second one later) but trimmed to a single real option instead of 7 duplicate-sprite entries. Renaming "Stripling Warrior" to "Invincible" throughout is being considered but not decided.

**Also noticed, not yet fixed**: in the old data, Birds' Rook and Knight sprites are the exact same GUIDs as Pixel Art's Rook and Knight -- Birds was already a partial/borrowed set before this refactor touched anything. Low priority, cosmetic, revisit whenever someone's looking at the art anyway.

**UI**: replaced the single per-player `Style (DO NOT REMOVE)` dropdown with a `Customize Pieces` button (same slot, same gold-banner button style as the rest of the menu) that opens a shared `Piece Customization Panel` -- one panel reused for every player rather than duplicated per player, following the same "menus diverge, gameplay doesn't" instinct as the CJ/BOM menu split, just parameterized instead of duplicated. The panel holds one dropdown per piece type (`King Style`, `Queen Style`, `Invincible Style`, `Rook Style`, `Bishop Style`, `Knight Style`, `Pawn Style`), each a `Piece Style Dropdown` prefab instance with `DropdownPopulator.pieceTypeName` set to match and `DropdownSaver.toSave` set to that player's per-type key. `DropdownPopulator` also gained sprite-icon support (`TMP_Dropdown.OptionData(name, sprite)`) for the "show the actual piece, not just its name" ask -- still needs Caption Image/Item Image wired on the dropdown Template to actually render.

**Not yet done**: the `Customize Pieces` button/panel is currently only wired for one player (keys hardcoded to the `1style...` prefix) -- needs the panel parameterized by which player's button opened it before repeating this for player 2+. Confirmed working end-to-end for mixed styles in Play mode (pixel-art knight next to a cardinal queen next to a peacock, etc.) before that generalization.

---

## Open Questions

- Does Chess Journey need the Stripling Warrior mechanic at all, or is it meant to be plain chess for a beginner? If plain chess, the `StriplingWarrior` sprite field on secular `SpriteSet`s may just go unused -- confirm that's fine rather than a hole.
- Is the mode meant to start Dawson with a fixed silly piece set, or should he get to pick (Halloween vs. Birds vs. Pixel Art) every time he plays?
- What's the actual final name -- does "Chess Journey: For Beginners" stay, or does it simplify once it's a real thing Dawson names or reacts to?
- Does this stay a small side mode inside the same app/build as BOM Chess, or does it eventually want to be its own build/release entirely?
