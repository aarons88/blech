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
3. Each biome has a visually distinct identity using only ProBuilder + ShaderLab + ParticleSystems.
4. The build is shippable as a Windows + Mac standalone for friends to try.
5. Total third-party dollar cost: $0.

## 2. Architectural Approach

**Greybox-first, mechanics-first, script-automated.** All environment geometry is authored with **Unity ProBuilder** via code (not the GUI). The *Blech look* comes from hand-written ShaderLab/HLSL shaders, ParticleSystem (Shuriken) effects, and URP lighting. The only imported 3D meshes are Quaternius food character meshes.

**Authoring philosophy:** every Unity asset (materials, prefabs, scenes, ScriptableObjects, UI canvases, particle systems) is created by an **editor automation toolkit** — C# editor scripts under `Assets/Blech/Editor/` — invokable from the menu bar (`Blech > Build > …`) or headlessly via `unity -batchmode -executeMethod`. Manual editor steps are minimized to: opening the project once, installing Unity itself, and playtesting. This keeps the project AI-agent-friendly and reproducible from source.

**Player physics:** `CharacterController` + custom climb logic. Predictable, no rigidbody drift during climbing, easy to project movement onto wall planes.

**Climbing detection:** spherecasts from the character's chest/hand origins. A surface is climbable if the cast hit has a `ClimbableSurface` component (single `TryGetComponent` per cast, no broad scans).

**Data-driven tuning:** `PlayerCharacterStats` is a `ScriptableObject` with movement, stamina, grip, slip-resistance fields. Bean / Pea / Carrot Chunk exist as three instances of that asset, not three code paths. Only Bean is wired up to the player prefab for this slice.

**Single scene for the route:** `MVP_VerticalSlice.unity` contains all four biomes as sibling root GameObjects. No async scene loading.

## 3. Project Foundation

**Engine:** Unity 6 LTS, 3D URP template.

**Packages** (via direct edits to `Packages/manifest.json`, no GUI):

- `com.unity.inputsystem` (default in Unity 6)
- `com.unity.cinemachine` — **Cinemachine 3.x** (major API rewrite; uses `CinemachineCamera` + `CinemachineOrbitalFollow`, namespace `Unity.Cinemachine`)
- `com.unity.probuilder` — for code-driven geometry via `ProBuilderMesh` API
- `com.unity.ugui` — TextMeshPro is now folded into this package; no separate `com.unity.textmeshpro` install
- `com.unity.test-framework` — NUnit-based EditMode/PlayMode tests

**Explicitly NOT used:**

- ~~`com.unity.progrids`~~ — deprecated; replaced by Unity's built-in Grid & Snap Settings
- ~~`com.unity.visualeffectgraph`~~ — no public C# authoring API, blocks our automation philosophy. Using `UnityEngine.ParticleSystem` (Shuriken) instead — fully script-authorable, not deprecated, on Unity's 2025 roadmap.
- ~~`com.unity.shadergraph`~~ — `.shadergraph` assets have no public authoring API. Using hand-written ShaderLab/HLSL `.shader` text files instead — fully supported in URP Unity 6, version-controllable, AI-editable.

**Folder layout:** follows PRD §22 with one addition:

```text
Assets/Blech/
  _ThirdParty/        # Quaternius, Kenney imports — never edited
  Art/
    Characters/       # food character prefabs, eye/limb primitives
    Environment/      # nothing imported; ProBuilder authoring here
    Shaders/          # 6 ShaderLab .shader text files
    Materials/        # 6 generated .mat assets
    Particles/        # 4 ParticleSystem prefabs
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

**Cinemachine 3** `CinemachineCamera` GameObject with a `CinemachineOrbitalFollow` component (the FreeLook-equivalent 3-rig orbit camera in Cinemachine 3). `Follow` and `LookAt` bound to the Bean transform. Defaults to start; revisit during tuning.

## 5. World & Hazard Systems

All world behaviors are single-purpose `MonoBehaviour`s placed on geometry. Two scene singletons only: `CheckpointManager` and `RunStatsTracker`.

### 5.1 `ClimbableSurface`

Marker component. Optional `float grip = 1.0`. Found via `TryGetComponent` on spherecast hits.

### 5.2 `SlipperySurface`

Marker component. `float slipMultiplier = 2.0` (stamina drain multiplier), `float slideRate = 1.0` (downward drift m/s while climbing). Coexists with `ClimbableSurface` — mucus patches are climbable and slippery.

### 5.3 `KillVolume`

Trigger collider. On `OnTriggerEnter` with player: calls `PlayerRespawn.Kill(cause)`. `KillCause` enum: `Acid`, `Pit`, `OutOfBounds`.

### 5.4 `AcidHazard`

Composite: a `KillVolume` (cause = `Acid`) + an `AcidHazardVisual` companion that drives the bubble `ParticleSystem` and applies the `Acid_Surface` material.

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

**Six hand-written ShaderLab/HLSL shaders** under `Assets/Blech/Art/Shaders/`. All inherit from a common URP Lit template (`Universal Render Pipeline/Lit` features: SRP Batcher compatible, shadow caster pass, GI). Time-driven effects use the URP `_Time` shader variable. Materials are generated from C# via an editor toolkit menu (`Blech > Build > Materials`).

1. `Blech/Intestine_Organic` — pink Lit, slow emission pulse via `_Time`.
2. `Blech/Stomach_Wall` — sickly green Lit, high smoothness, no pulse.
3. `Blech/Throat_Tissue` — deep red-purple Lit, optional subtle vertex displacement (sine-wave `_Time` perturbation).
4. `Blech/Mucus_Slip` — translucent, animated UV scroll noise for shimmer.
5. `Blech/Acid_Surface` — neon green emission, animated UV distortion.
6. `Blech/Tongue` — bright pink, very high smoothness for saliva sheen.

**Four ParticleSystems** (Shuriken), authored as prefabs by editor scripts using the full `ParticleSystem` C# API (emission, shape, velocity-over-lifetime, color-over-lifetime, size-over-lifetime modules):

1. **Bubbles** — ambient particle effect (used in intestine + stomach acid).
2. **Acid splash** — burst on geyser erupt.
3. **Wind streaks** — stretched billboards in gust direction.
4. **Confetti** — burst on finish trigger.

Particle textures pull from Kenney Particle Pack (CC0 billboards/sprites).

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

| Event | Kenney source pack | Volume |
|---|---|---|
| Footstep | RPG Audio | one-shot per step (timed by movement controller) |
| Jump | Voiceover Pack (short grunt) | one-shot |
| Grab wall | Impact Sounds | one-shot |
| Slip | Impact Sounds | one-shot |
| Fall yell | Voiceover Pack | one-shot |
| Mucus squelch | Impact Sounds | one-shot on slip onset |
| Acid bubble | Sci-fi Sounds (looped bubbly synth) | low-volume loop near acid |
| Acid splash | Impact Sounds | one-shot on geyser erupt |
| Wind warning | Sci-fi Sounds (whoosh ramp) | one-shot pre-gust |
| Checkpoint "blub" | Digital Audio | one-shot |
| Finish fanfare | Music Jingles | one-shot |

Audio implementation: a single `SfxPlayer` scene singleton with `Play(SfxId id)` and an inspector-assigned clip table.

## 9. Assets & Acquisition

Nine zip imports total (two Quaternius packs + seven Kenney packs). Everything else is authored in-project by the editor toolkit.

| Asset | Source | License | Purpose |
|---|---|---|---|
| Quaternius Ultimate Food Pack | `quaternius.com/packs/ultimatefood.html` | CC0 | Food character meshes (prepared foods) |
| Quaternius Ultimate Crops Pack | `quaternius.com/packs/ultimatecrops.html` | CC0 | Bean/pea/carrot vegetable meshes (paired with Food Pack to cover all three characters) |
| Kenney Particle Pack | `kenney.nl/assets/particle-pack` | CC0 | ParticleSystem billboard textures |
| Kenney RPG Audio | `kenney.nl/assets/rpg-audio` | CC0 | Footsteps |
| Kenney Voiceover Pack | `kenney.nl/assets/voiceover-pack` | CC0 | Jumps, falls, yells |
| Kenney Impact Sounds | `kenney.nl/assets/impact-sounds` | CC0 | Wall grabs, slips, squelch, splashes |
| Kenney Sci-fi Sounds | `kenney.nl/assets/sci-fi-sounds` | CC0 | Wind warnings, acid bubbling |
| Kenney Digital Audio | `kenney.nl/assets/digital-audio` | CC0 | Checkpoint pickup |
| Kenney Music Jingles | `kenney.nl/assets/music-jingles` | CC0 | Finish fanfare |
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
| ProBuilder geometry looks too flat | Lean harder on ShaderLab vertex displacement (in `Throat_Tissue.shader`) and ParticleSystem particles. Add fog/volumetric to mask geometry weakness. |
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
