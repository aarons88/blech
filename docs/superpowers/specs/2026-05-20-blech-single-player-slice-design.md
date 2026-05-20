# Blech — Single-Player Vertical Slice Design

**Date:** 2026-05-20
**Scope:** PRD Milestones 1+2+3 — climbing mechanics, "Blech look", and the full intestine→stomach→throat→mouth route, single-player only.
**Out of scope:** Toothpick piton (M4), PurrNet co-op (M5), polish pass (M6), character select.
**Parent PRD:** `docs/PRD.md`

## 1. Goal

Produce a playable 5–10 minute single-player route that proves the core fantasy: a tiny food character climbs from intestine to mouth through silly-gross cartoon body biomes. The build should already read as *Blech*, not a generic climbing prototype.

Success criteria:

1. A single player can complete the full route (intestine → stomach → throat → mouth) in 5–10 minutes.
2. Climbing feels responsive, stamina drains meaningfully, slipping is readable.
3. Each biome has a visually distinct identity using only ProBuilder + ShaderGraph + VFX.
4. The build is shippable as a Windows + Mac standalone for friends to try.
5. Total third-party dollar cost: $0.

## 2. Architectural Approach

**Greybox-first, mechanics-first.** All environment geometry is authored with **Unity ProBuilder**; the *Blech look* comes from ShaderGraph materials, VFX Graph particles, and lighting. The only imported 3D meshes are the Quaternius food characters.

**Player physics:** `CharacterController` + custom climb logic. Predictable, no rigidbody drift during climbing, easy to project movement onto wall planes.

**Climbing detection:** spherecasts from the character's chest/hand origins. A surface is climbable if the cast hit has a `ClimbableSurface` component (single `TryGetComponent` per cast, no broad scans).

**Data-driven tuning:** `PlayerCharacterStats` is a `ScriptableObject` with movement, stamina, grip, slip-resistance fields. Bean / Pea / Carrot Chunk exist as three instances of that asset, not three code paths. Only Bean is wired up to the player prefab for this slice.

**Single scene for the route:** `MVP_VerticalSlice.unity` contains all four biomes as sibling root GameObjects. No async scene loading.

## 3. Project Foundation

**Engine:** Unity 6 LTS, 3D URP template.

**Packages:**

- Input System (new)
- Cinemachine
- ProBuilder
- ProGrids
- VFX Graph
- TextMeshPro (default with URP)

**Folder layout:** follows PRD §22 with one addition:

```text
Assets/Blech/
  _ThirdParty/        # Quaternius, Kenney imports — never edited
  Art/
    Characters/       # food character prefabs, eye/limb primitives
    Environment/      # nothing imported; ProBuilder authoring here
    Materials/        # 6 ShaderGraph materials
    Particles/        # 4 VFX Graphs
  Audio/
    SFX/              # Kenney + freesound placeholders
    Music/            # empty for now
  Prefabs/
    Player/
    Hazards/
    Level/
    UI/
  Scenes/
    MainMenu.unity
    Prototype_ClimbingToy.unity
    MVP_VerticalSlice.unity
  Scripts/            # matches PRD §22
  ScriptableObjects/
    Characters/       # Bean.asset, Pea.asset, CarrotChunk.asset
```

## 4. Player Systems

Five small components on the player prefab. Each has one job.

### 4.1 `PlayerInput`

Wraps Unity Input System. Exposes:

- `Vector2 Move`
- `event Action JumpPressed`
- `bool ClimbHeld`
- `event Action RespawnPressed`

Single source of truth so other components never poll input directly.

### 4.2 `PlayerMovementController`

`CharacterController`-based ground movement.

- Reads `PlayerInput.Move`, applies move speed from `PlayerCharacterStats`.
- Handles jump (instantaneous Y velocity from `jumpForce`) and gravity (constant).
- Rotates body toward movement direction with a slight lag for "goofy" feel.
- Disabled (`enabled = false`) while climbing.
- Exposes `AddExternalVelocity(Vector3 v, float duration)` for wind hazards to push the player.

### 4.3 `PlayerClimbingController`

Owns climb state. State machine: `Off → Climbing → Off`.

On `PlayerInput.ClimbHeld` + wall detected by spherecast (origin ~chest height, radius 0.3m, distance 0.5m forward):

1. Enter `Climbing`. Disable `PlayerMovementController`.
2. Each frame: spherecast forward to keep wall reference. If lost, exit climb.
3. Move via `CharacterController.Move`, with input projected onto the wall plane using the hit normal.
4. Detect `SlipperySurface` on the hit collider; multiply stamina drain by `slipMultiplier` and add a constant downward drift.
5. Drain stamina each frame.
6. Exit climb on: `ClimbHeld` released, jump pressed (apply away-from-wall + up impulse via movement controller), `PlayerStamina.OnZero`, or lost wall contact.

### 4.4 `PlayerStamina`

Float `0..1`.

- Drains while climbing at `drainRate * slipMultiplier` per second.
- Regens while grounded at `regenRate` per second.
- Fires `OnStaminaChanged(float)` and `OnStaminaZero()`.
- All rates from `PlayerCharacterStats`.

### 4.5 `PlayerRespawn`

Listens for kill events.

- `Kill(KillCause cause)` method — increments fall counter in `RunStatsTracker`, teleports player to `CheckpointManager.CurrentSpawn`.
- Subscribes to `PlayerInput.RespawnPressed` (manual reset).
- Records max fall height by tracking peak descending Y velocity each frame; resets on grounded or respawn.

### 4.6 `PlayerCharacterVisual`

Holds the food mesh and procedural animation.

- Body: Quaternius food mesh (bean for MVP).
- Eyes: two primitive spheres parented at face level. Material with black iris quad mapped on.
- Limbs: four primitive capsules parented to body (two arms, two legs). No rigging.
- Animations driven from code:
  - **Idle:** Y-bob via `sin(time)`.
  - **Walk/run:** bob + squash-stretch synced to horizontal speed.
  - **Jump:** vertical stretch on launch, squash on landing.
  - **Climb:** body tilted into wall normal; limbs cycled out of phase.
  - **Fall:** body spin + wide-eye material swap.
- Total materials per character: 2 (body, eyes).

### 4.7 Camera

Cinemachine FreeLook on the player. Single rig. Default Unity FreeLook tuning to start; revisit if it feels bad during climb.

## 5. World & Hazard Systems

All world behaviors are single-purpose `MonoBehaviour`s placed on geometry. Two scene singletons only: `CheckpointManager` and `RunStatsTracker`.

### 5.1 `ClimbableSurface`

Marker component. Optional `float grip = 1.0`. Found via `TryGetComponent` on spherecast hits.

### 5.2 `SlipperySurface`

Marker component. `float slipMultiplier = 2.0` (stamina drain multiplier), `float slideRate = 1.0` (downward drift m/s while climbing). Coexists with `ClimbableSurface` — mucus patches are climbable and slippery.

### 5.3 `KillVolume`

Trigger collider. On `OnTriggerEnter` with player: calls `PlayerRespawn.Kill(cause)`. `KillCause` enum: `Acid`, `Pit`, `OutOfBounds`.

### 5.4 `AcidHazard`

Composite: a `KillVolume` (cause = `Acid`) + an `AcidHazardVisual` companion that spawns the bubble VFX Graph and applies the `Acid_Surface` material.

### 5.5 `AcidGeyser`

Lerps a child trigger collider upward on a configurable interval (`warningTime`, `eruptTime`, `cooldownTime`). Same kill effect via embedded `KillVolume`. Telegraphs with a warning particle burst.

### 5.6 `WindHazard`

Trigger volume cycling `Idle → Warning → Gust → Cooldown` on configurable timing.

- Warning phase: plays audio cue + tissue-flap VFX.
- Gust phase: calls `PlayerMovementController.AddExternalVelocity(direction * strength, gustDuration)` on any player inside.
- Up-gusts can pop a climbing player off the wall: climbing controller checks impulse magnitude against `PlayerCharacterStats.gripStrength`; if impulse > grip, climb state exits.

### 5.7 `Checkpoint`

Trigger collider. On first entry by player:

1. Registers self as `CheckpointManager.CurrentSpawn`.
2. Fires `OnCheckpointReached(this)` — `CheckpointToastUI` and audio listen.
3. Plays a small particle burst.
4. Re-entering does nothing (idempotent).

### 5.8 `CheckpointManager`

Scene singleton. Holds `Transform CurrentSpawn`. That's it. No persistence.

### 5.9 `FinishTrigger`

Trigger collider in the mouth biome. On entry: freezes input via `PlayerInput.enabled = false`, fires `OnRouteComplete`. End screen UI listens.

### 5.10 `RunStatsTracker`

Scene singleton. Accumulates:

- `float elapsedSeconds`
- `int fallCount`
- `float maxFallHeight`
- `Dictionary<KillCause, int> causeCounts`

Subscribes to `PlayerRespawn.OnKill`, route start/end. End screen reads from this.

### 5.11 Materials & VFX

Six ShaderGraph materials authored in-project:

1. `Intestine_Organic` — pink, fake subsurface (gradient + emission), slow pulse via shader time.
2. `Stomach_Wall` — sickly green, glossy.
3. `Throat_Tissue` — deep red, animated vertex displacement noise.
4. `Mucus_Slip` — translucent, animated UV scroll for shimmer.
5. `Acid_Surface` — neon green, strong emission, distortion.
6. `Tongue` — pink, high gloss for saliva sheen.

Four VFX Graphs:

1. **Bubbles** — generic ambient bubbles (used in intestine + stomach).
2. **Acid splash** — eruption burst for geysers, splash on player respawn.
3. **Wind streaks** — stretched billboards along gust direction.
4. **Confetti** — burst on finish.

## 6. Biome Layouts

One scene, four sibling biome GameObjects. Total playable space ~70m tall × ~20m wide footprint.

### 6.1 Biome 1 — Intestine Intro (~60s)

- L-shaped tunnel: short horizontal corridor → first vertical climb (~6m) → ledge with checkpoint.
- One mucus patch (`SlipperySurface` + `ClimbableSurface`) halfway up the climb.
- Geometry: 6–8 ProBuilder shapes (extruded curves).
- Palette: warm pink/coral, soft pulsing emission via `Intestine_Organic`.
- Exit: doorway iris into stomach.
- Teaches: move, jump, climb, stamina, slip, checkpoint.

### 6.2 Biome 2 — Stomach Chamber (~90s)

- Roughly spherical chamber, ~15m wide.
- Acid pool fills bottom 3m (`AcidHazard`).
- 3–4 static floating platforms over the acid (no churning for MVP).
- Perimeter walls have climbable patches; gaps require jumps to platforms.
- One `AcidGeyser` between two key platforms (telegraph + splash).
- Checkpoint on a high ledge before the exit climb.
- Exit: climb up the back wall into the throat shaft.
- Palette: neon yellow-green acid, sickly green walls.

### 6.3 Biome 3 — Throat Ascent (~3 min, main challenge)

- Vertical shaft, ~25–30m tall, climbable on most surfaces.
- Two `WindHazard` zones: mid-shaft (sideways push), upper (up-gust strong enough to dislodge low-stamina climbers).
- 3–4 flappy tissue rest ledges (small horizontal `ClimbableSurface` platforms).
- Mid-shaft checkpoint.
- Mucus drips on certain walls force route choices.
- Uvula silhouette hanging from above as a visual goal.
- Palette: deep red-purple, dramatic upward lighting from the mouth.
- Exit: lip-line opens into the mouth.

### 6.4 Biome 4 — Mouth Exit (~30s)

- Wide tongue platform, teeth-cliffs on either side.
- Final ~10m scramble across the tongue. Optional saliva slip patches if it adds tension; skip if it bogs the finale.
- `FinishTrigger` at the lip edge, bright daylight beyond.
- End screen pops on trigger.
- Palette: bright pink tongue, white teeth, blue-sky beyond.

## 7. UI

UGUI canvases; UI Toolkit deferred. Three UI scenes/canvases.

### 7.1 Main Menu (`MainMenu.unity`)

- Title text "Blech" with a wobble shader.
- Buttons: **Play**, **Quit**.
- No character select (Bean only this slice).
- One background image (Kenney UI pack or a single generated image).

### 7.2 In-Game HUD

Screen Space Overlay canvas in the vertical slice scene.

- **Stamina bar**: bottom-center, 200×20px, green→red fill. Pulses red below 20%. Bound to `PlayerStamina.OnStaminaChanged` via `StaminaUI`.
- **Checkpoint toast**: top-center, fades in "Blechmark!" text for 2s on `Checkpoint.OnCheckpointReached`. Reused for "PEAK reached!" on finish.
- **Run timer**: top-right, mono font, shows current run time. Hidden during finish.

No pause menu — Esc returns to main menu (loads `MainMenu.unity`).

### 7.3 End Screen

Overlay on the gameplay scene, triggered by `FinishTrigger.OnRouteComplete`.

- Shows: completion time, fall count, max fall height ("Most dramatic fall: Xm").
- Buttons: **Run again** (reload current scene), **Main menu**.
- Background dim + confetti VFX behind text.

## 8. Audio

Placeholder-only for this slice. Eleven SFX hooks; tracks deferred.

| Event | Source | Volume |
|---|---|---|
| Footstep | Kenney UI Audio (light tick) | one-shot per step (timed by movement controller) |
| Jump | Kenney Interface Sounds | one-shot |
| Grab wall | Kenney Impact Sounds | one-shot |
| Slip | Kenney Impact Sounds | one-shot |
| Fall yell | freesound.org cartoon yell | one-shot |
| Mucus squelch | freesound.org "squelch" | one-shot |
| Acid bubble | Kenney Particle Pack audio | loop in acid range |
| Acid splash | Kenney Impact | one-shot on geyser |
| Wind warning | Kenney Interface (whoosh ramp) | one-shot pre-gust |
| Checkpoint "blub" | Kenney UI Audio | one-shot |
| Finish fanfare | Kenney UI Audio (jingle) | one-shot |

Audio implementation: a single `SfxPlayer` scene singleton with `Play(SfxId id)` and an inspector-assigned clip table.

## 9. Assets & Acquisition

Three zip imports total. Everything else authored in-project.

| Asset | Source | License | Purpose |
|---|---|---|---|
| Quaternius Ultimate Food Pack | `quaternius.com/packs/ultimatefood.html` | CC0 | Bean, Pea, Carrot meshes |
| Kenney Particle Pack | `kenney.nl/assets/particle-pack` | CC0 | VFX Graph billboard textures |
| Kenney Audio Bundles | `kenney.nl/assets/category:audio` | CC0 | SFX placeholders |
| Fredoka / Bagel Fat One font | Google Fonts | OFL | Title + UI |

All imports land in `Assets/Blech/_ThirdParty/<PackName>/` and are never edited in place.

**Escape hatches if a free option fails:**

1. AI-generate a specific texture via the `nano-banana-sprites` skill already in this repo.
2. Buy a single targeted Unity Asset Store pack (~$15–30) for a recurring specific need.

**Total third-party cost: $0.**

## 10. Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Climbing feels bad on `CharacterController` | Validate in `Prototype_ClimbingToy.unity` first before building biomes. If unworkable, switch to Kinematic Rigidbody; movement controller is small enough to swap. |
| ProBuilder geometry looks too flat | Lean harder on ShaderGraph vertex displacement and VFX particles. Add fog/volumetric to mask geometry weakness. |
| Quaternius meshes don't match "tiny limbed food character" | Add primitive limbs/eyes as children (already in plan). If body shape is wrong, swap with another Quaternius mesh or generate a custom one via `nano-banana-sprites`. |
| Scope creep into M4 (piton) before slice is fun | Hard rule: no piton work until the route is completable end-to-end without one. |
| Wind hazard breaks climbing in unfun ways | Tune `WindHazard.strength` and `gripStrength` in playtest. Worst case, gusts only knock players in `Idle`, not in `Climbing`. |

## 11. Implementation Order

The plan that follows this spec should ship work in this order — each phase produces something runnable:

1. **Project setup** — Unity 6 URP project, packages, folders, third-party imports.
2. **Player movement (no climb yet)** — `PlayerInput`, `PlayerMovementController`, Cinemachine, on a flat ProBuilder plane.
3. **Climbing toy** — `ClimbableSurface`, `PlayerClimbingController`, `PlayerStamina`, `SlipperySurface` on `Prototype_ClimbingToy.unity`. Iterate until climbing feels good.
4. **Respawn & checkpoints** — `KillVolume`, `Checkpoint`, `CheckpointManager`, `PlayerRespawn`.
5. **Character visuals** — Quaternius Bean mesh, primitive limbs/eyes, procedural animation in `PlayerCharacterVisual`.
6. **Biome 1 — Intestine** — ProBuilder geometry, `Intestine_Organic` + `Mucus_Slip` shaders, ambient bubbles VFX.
7. **Biome 2 — Stomach** — ProBuilder chamber, `AcidHazard`, `AcidGeyser`, `Stomach_Wall` + `Acid_Surface` shaders, acid VFX.
8. **Biome 3 — Throat** — vertical shaft, `WindHazard`, `Throat_Tissue` shader, wind streaks VFX.
9. **Biome 4 — Mouth + Finish** — tongue platform, `FinishTrigger`, `Tongue` shader, confetti VFX.
10. **UI** — main menu, stamina bar, checkpoint toast, run timer, end screen.
11. **Audio pass** — wire all SFX placeholders to events.
12. **Tuning + cut scene** — playtest, tune stamina/grip/wind, cut anything that's not fun.

## 12. What Comes After This Spec

The next spec(s), in order:

1. **Toothpick Piton** (PRD M4) — pickup, placement, stamina anchor.
2. **PurrNet Co-op** (PRD M5) — host/join, networked spawn, state sync.
3. **Polish Pass** (PRD M6) — character select, end-screen stats expansion, sound pass, bug fixes.

## 13. Open Questions Resolved For This Spec

From PRD §26:

- **Solo as first-class?** Yes — this entire spec is the solo experience. Co-op layers on top later.
- **Falling damage?** No. Falls respawn at checkpoint; "dramatic fall height" is a stat, not a damage value.
- **Character abilities?** Stat-only for MVP. Bean is the only playable character this slice.
- **Whose body?** Abstract / unstated. The biomes are stylized enough not to require a host.
- **Title?** Working title remains "Blech" (no subtitle).

Remaining open questions deferred to later specs.
