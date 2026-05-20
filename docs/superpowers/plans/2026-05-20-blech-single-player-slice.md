# Blech Single-Player Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable single-player vertical slice of Blech — a tiny food character climbs from intestine through stomach and throat to escape via the mouth — using Unity 6 URP, ProBuilder greybox, and Quaternius food meshes.

**Architecture:** `CharacterController`-based player with custom climb state, spherecast wall detection, ScriptableObject-driven stats, single-purpose `MonoBehaviour`s for hazards and triggers, two scene singletons (`CheckpointManager`, `RunStatsTracker`). All environment geometry is ProBuilder; the *Blech look* comes from ShaderGraph + VFX Graph + URP lighting.

**Tech Stack:** Unity 6 LTS, URP, Input System (new), Cinemachine, ProBuilder, ProGrids, VFX Graph, Unity Test Framework (NUnit), TextMeshPro. Quaternius Ultimate Food Pack (CC0), Kenney Particle Pack (CC0), Kenney Audio (CC0).

**Spec:** `docs/superpowers/specs/2026-05-20-blech-single-player-slice-design.md`

---

## Prerequisites

Before starting Task 1 the developer needs:

1. **Unity Hub** installed (`https://unity.com/download`)
2. **Unity 6 LTS** installed via Unity Hub with the following modules:
   - Windows Build Support (IL2CPP)
   - Mac Build Support (IL2CPP) — optional, only if shipping for Mac
3. **Git** + this repo cloned locally (already done)
4. **A free Quaternius account or direct download** from `https://quaternius.com/packs/ultimatefood.html` — download the zip
5. **Kenney Particle Pack** zip from `https://kenney.nl/assets/particle-pack`
6. **Kenney UI Audio + Interface Sounds + Impact Sounds** zips from `https://kenney.nl/assets/category:audio`

Stage the three zip files anywhere convenient; Task 1 will copy them into the project.

---

## Editor-Driven Tasks vs. Script Tasks

This plan distinguishes two task styles:

- **Script tasks** follow strict TDD: write a failing test, watch it fail, write minimal implementation, watch it pass, commit. Tests use Unity Test Framework EditMode tests (pure C# in `Assets/Blech/Tests/Edit/`).
- **Editor tasks** (creating scenes, ProBuilder geometry, ScriptableObjects, materials, VFX Graphs, prefabs) cannot be TDD'd because they require Unity Editor manipulation. They follow explicit step-by-step instructions and end with a manual "verify in editor" step plus a commit.

Both styles end with a `git commit`.

---

## Task 1: Create Unity Project, Folders, and Third-Party Imports

**Style:** Editor task

**Files:**
- Create: entire Unity project structure under repo root
- Create: `Assets/Blech/_ThirdParty/Quaternius_UltimateFood/`
- Create: `Assets/Blech/_ThirdParty/Kenney_ParticlePack/`
- Create: `Assets/Blech/_ThirdParty/Kenney_Audio/`

- [ ] **Step 1: Create the Unity project at the repo root**

Open Unity Hub → New Project → "Universal 3D" (URP) template → Project name: `Blech` → Location: `/Users/aaronsimmons/Projects/` → click "Create project".

This will scaffold Unity files (`Assets/`, `Packages/`, `ProjectSettings/`, etc.) into `/Users/aaronsimmons/Projects/Blech/`. **Important:** the repo lives at `/Users/aaronsimmons/Projects/blech/` (lowercase). When Unity Hub finishes, manually merge the generated `Blech/` contents into `blech/` (move `Assets/`, `Packages/`, `ProjectSettings/` over).

- [ ] **Step 2: Open the project and install packages**

In Unity Editor: Window → Package Manager → "Packages: Unity Registry":

- Input System (latest) — install. When prompted to restart with new input system, click Yes.
- Cinemachine (latest)
- ProBuilder (latest)
- Visual Effect Graph (latest)
- TextMeshPro (already included with URP)
- Test Framework (already included)

- [ ] **Step 3: Create the folder structure**

In Unity Project window, create these folders under `Assets/Blech/`:

```
Assets/Blech/
  _ThirdParty/
  Art/
    Characters/
    Environment/
    Materials/
    Particles/
  Audio/
    SFX/
  Prefabs/
    Player/
    Hazards/
    Level/
    UI/
  Scenes/
  Scripts/
    Player/
    World/
    UI/
    Audio/
  ScriptableObjects/
    Characters/
  Tests/
    Edit/
    Play/
```

- [ ] **Step 4: Set up test assembly definitions**

Create `Assets/Blech/Scripts/Blech.Runtime.asmdef`:

```json
{
  "name": "Blech.Runtime",
  "rootNamespace": "Blech",
  "references": [
    "Unity.InputSystem",
    "Unity.Cinemachine",
    "Unity.TextMeshPro"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

Create `Assets/Blech/Tests/Edit/Blech.Tests.Edit.asmdef`:

```json
{
  "name": "Blech.Tests.Edit",
  "rootNamespace": "Blech.Tests",
  "references": [
    "Blech.Runtime",
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner"
  ],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": [
    "nunit.framework.dll"
  ],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "versionDefines": [],
  "noEngineReferences": false
}
```

- [ ] **Step 5: Import third-party assets**

Unzip the three downloaded packs:

- Quaternius Ultimate Food Pack → copy all contents to `Assets/Blech/_ThirdParty/Quaternius_UltimateFood/`
- Kenney Particle Pack → copy to `Assets/Blech/_ThirdParty/Kenney_ParticlePack/`
- Kenney Audio bundles → copy to `Assets/Blech/_ThirdParty/Kenney_Audio/`

Wait for Unity to finish importing. The Console should show no errors.

- [ ] **Step 6: Configure project settings**

Edit → Project Settings:

- **Player** → Company Name: your name; Product Name: `Blech`
- **Quality** → defaults are fine for URP
- **Time** → Fixed Timestep: `0.02` (default)
- **Input System Package** → confirm enabled

- [ ] **Step 7: Verify project opens cleanly and commit**

Close and reopen the project. Confirm no errors in Console.

```bash
cd /Users/aaronsimmons/Projects/blech
git add Assets Packages ProjectSettings
git commit -m "feat: Unity 6 URP project scaffold with packages, folders, third-party imports"
```

---

## Task 2: PlayerCharacterStats ScriptableObject + Bean Asset

**Style:** Script task (TDD)

**Files:**
- Create: `Assets/Blech/Scripts/Player/PlayerCharacterStats.cs`
- Create: `Assets/Blech/Tests/Edit/PlayerCharacterStatsTests.cs`
- Create (Editor): `Assets/Blech/ScriptableObjects/Characters/Bean.asset`

- [ ] **Step 1: Write the failing test**

Create `Assets/Blech/Tests/Edit/PlayerCharacterStatsTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class PlayerCharacterStatsTests
    {
        [Test]
        public void DefaultStats_HaveSensibleValues()
        {
            var stats = ScriptableObject.CreateInstance<PlayerCharacterStats>();
            Assert.AreEqual("Bean", stats.displayName);
            Assert.Greater(stats.moveSpeed, 0f);
            Assert.Greater(stats.jumpForce, 0f);
            Assert.Greater(stats.maxStamina, 0f);
            Assert.Greater(stats.staminaDrainPerSecond, 0f);
            Assert.Greater(stats.staminaRegenPerSecond, 0f);
            Assert.GreaterOrEqual(stats.gripStrength, 0f);
        }
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

In Unity: Window → General → Test Runner → EditMode tab → click "Run All".

Expected: FAIL — `PlayerCharacterStats` does not exist.

- [ ] **Step 3: Write minimal implementation**

Create `Assets/Blech/Scripts/Player/PlayerCharacterStats.cs`:

```csharp
using UnityEngine;

namespace Blech.Player
{
    [CreateAssetMenu(menuName = "Blech/Player Character Stats", fileName = "NewCharacterStats")]
    public class PlayerCharacterStats : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Bean";

        [Header("Movement")]
        public float moveSpeed = 4.5f;
        public float jumpForce = 7f;
        public float gravity = -20f;

        [Header("Stamina")]
        public float maxStamina = 100f;
        public float staminaDrainPerSecond = 12f;
        public float staminaRegenPerSecond = 30f;

        [Header("Climbing")]
        public float climbSpeed = 2.5f;
        public float gripStrength = 1f;
        public float slipResistance = 1f;
    }
}
```

- [ ] **Step 4: Run the test to verify it passes**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Create the Bean ScriptableObject asset**

In Unity Project: right-click `Assets/Blech/ScriptableObjects/Characters/` → Create → Blech → Player Character Stats → rename to `Bean`. Defaults are fine.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/Player/PlayerCharacterStats.cs \
        Assets/Blech/Scripts/Player/PlayerCharacterStats.cs.meta \
        Assets/Blech/Tests/Edit/PlayerCharacterStatsTests.cs \
        Assets/Blech/Tests/Edit/PlayerCharacterStatsTests.cs.meta \
        Assets/Blech/ScriptableObjects/Characters/Bean.asset \
        Assets/Blech/ScriptableObjects/Characters/Bean.asset.meta
git commit -m "feat: PlayerCharacterStats ScriptableObject + Bean asset"
```

---

## Task 3: PlayerInput Component

**Style:** Script task (TDD where possible; input wrapper)

**Files:**
- Create: `Assets/Blech/Scripts/Player/PlayerInput.cs`
- Create: `Assets/Blech/Scripts/Player/PlayerInputActions.inputactions` (asset)

- [ ] **Step 1: Generate input actions asset**

Right-click `Assets/Blech/Scripts/Player/` → Create → Input Actions → name it `PlayerInputActions`. Open it.

Create one Action Map: `Player`. Inside it, four actions:

- `Move` — Value, Vector2, with binding "Left Stick" and a 2D Vector composite (WASD/Arrows)
- `Jump` — Button, with bindings: Space, Gamepad South
- `Climb` — Button, with bindings: Left Shift, Gamepad East (hold)
- `Respawn` — Button, with bindings: R, Gamepad Start

Tick "Generate C# Class" on the asset and set the namespace to `Blech.Player.Generated`. Save.

- [ ] **Step 2: Write the wrapper component**

Create `Assets/Blech/Scripts/Player/PlayerInput.cs`:

```csharp
using System;
using UnityEngine;
using Blech.Player.Generated;

namespace Blech.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public event Action JumpPressed;
        public event Action RespawnPressed;

        public Vector2 Move { get; private set; }
        public bool ClimbHeld { get; private set; }

        private PlayerInputActions _actions;

        private void Awake()
        {
            _actions = new PlayerInputActions();
            _actions.Player.Jump.performed += _ => JumpPressed?.Invoke();
            _actions.Player.Respawn.performed += _ => RespawnPressed?.Invoke();
        }

        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        private void Update()
        {
            Move = _actions.Player.Move.ReadValue<Vector2>();
            ClimbHeld = _actions.Player.Climb.IsPressed();
        }
    }
}
```

- [ ] **Step 3: Verify compiles and commit**

Confirm Console shows no errors.

```bash
git add Assets/Blech/Scripts/Player/PlayerInput.cs \
        Assets/Blech/Scripts/Player/PlayerInput.cs.meta \
        Assets/Blech/Scripts/Player/PlayerInputActions.inputactions \
        Assets/Blech/Scripts/Player/PlayerInputActions.inputactions.meta
git commit -m "feat: PlayerInput component wrapping new Input System"
```

---

## Task 4: PlayerMovementController (Ground Movement + Jump)

**Style:** Script task with extracted pure logic for TDD

**Files:**
- Create: `Assets/Blech/Scripts/Player/MovementMath.cs`
- Create: `Assets/Blech/Scripts/Player/PlayerMovementController.cs`
- Create: `Assets/Blech/Tests/Edit/MovementMathTests.cs`

- [ ] **Step 1: Write failing test for pure movement math**

Create `Assets/Blech/Tests/Edit/MovementMathTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class MovementMathTests
    {
        [Test]
        public void HorizontalVelocity_FromInputAndCamera_AlignsToCameraForward()
        {
            Vector2 input = new Vector2(0, 1);
            Vector3 cameraForward = new Vector3(0, 0, 1);
            float speed = 5f;

            Vector3 result = MovementMath.HorizontalVelocity(input, cameraForward, speed);

            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(5f, result.z, 0.001f);
            Assert.AreEqual(0f, result.y, 0.001f);
        }

        [Test]
        public void HorizontalVelocity_NoInput_ReturnsZero()
        {
            Vector3 result = MovementMath.HorizontalVelocity(Vector2.zero, Vector3.forward, 5f);
            Assert.AreEqual(Vector3.zero, result);
        }

        [Test]
        public void ApplyGravity_AccumulatesNegativeY()
        {
            float v = MovementMath.ApplyGravity(0f, -20f, 0.5f);
            Assert.AreEqual(-10f, v, 0.001f);
        }

        [Test]
        public void ApplyGravity_ClampsWhenGrounded()
        {
            float v = MovementMath.ApplyGravity(-30f, -20f, 1f, isGrounded: true);
            Assert.AreEqual(-2f, v, 0.001f); // small downward stick force
        }
    }
}
```

- [ ] **Step 2: Run test to verify failure**

Test Runner → Run All. Expected: FAIL — `MovementMath` not found.

- [ ] **Step 3: Write MovementMath**

Create `Assets/Blech/Scripts/Player/MovementMath.cs`:

```csharp
using UnityEngine;

namespace Blech.Player
{
    public static class MovementMath
    {
        public static Vector3 HorizontalVelocity(Vector2 input, Vector3 cameraForward, float speed)
        {
            if (input.sqrMagnitude < 0.0001f) return Vector3.zero;
            Vector3 forward = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 dir = (forward * input.y + right * input.x).normalized;
            return dir * speed;
        }

        public static float ApplyGravity(float currentY, float gravity, float dt, bool isGrounded = false)
        {
            if (isGrounded) return -2f;
            return currentY + gravity * dt;
        }
    }
}
```

- [ ] **Step 4: Run test to verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Write the MonoBehaviour controller**

Create `Assets/Blech/Scripts/Player/PlayerMovementController.cs`:

```csharp
using System.Collections;
using UnityEngine;

namespace Blech.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterStats stats;
        [SerializeField] private Transform cameraTransform;

        private CharacterController _controller;
        private PlayerInput _input;
        private float _verticalVelocity;
        private Vector3 _externalVelocity;
        private float _externalVelocityUntil;

        public bool IsGrounded => _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
            _input.JumpPressed += OnJump;
        }

        private void OnDestroy()
        {
            if (_input != null) _input.JumpPressed -= OnJump;
        }

        private void Update()
        {
            if (cameraTransform == null) cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform == null) return;

            Vector3 horizontal = MovementMath.HorizontalVelocity(_input.Move, cameraTransform.forward, stats.moveSpeed);
            _verticalVelocity = MovementMath.ApplyGravity(_verticalVelocity, stats.gravity, Time.deltaTime, _controller.isGrounded);

            Vector3 external = Time.time < _externalVelocityUntil ? _externalVelocity : Vector3.zero;
            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity + external;

            _controller.Move(velocity * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, 12f * Time.deltaTime);
            }
        }

        private void OnJump()
        {
            if (_controller.isGrounded) _verticalVelocity = stats.jumpForce;
        }

        public void AddExternalVelocity(Vector3 velocity, float duration)
        {
            _externalVelocity = velocity;
            _externalVelocityUntil = Time.time + duration;
        }
    }
}
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/Player/MovementMath.cs \
        Assets/Blech/Scripts/Player/MovementMath.cs.meta \
        Assets/Blech/Scripts/Player/PlayerMovementController.cs \
        Assets/Blech/Scripts/Player/PlayerMovementController.cs.meta \
        Assets/Blech/Tests/Edit/MovementMathTests.cs \
        Assets/Blech/Tests/Edit/MovementMathTests.cs.meta
git commit -m "feat: PlayerMovementController with extracted MovementMath for TDD"
```

---

## Task 5: Prototype Climbing Toy Scene + Player Prefab v1 + Camera

**Style:** Editor task

**Files:**
- Create: `Assets/Blech/Scenes/Prototype_ClimbingToy.unity`
- Create: `Assets/Blech/Prefabs/Player/Bean.prefab`

- [ ] **Step 1: Create the scene**

File → New Scene → Standard (URP) template → Save as `Assets/Blech/Scenes/Prototype_ClimbingToy.unity`.

Delete the default cube. Keep the Directional Light. Set ambient to a slightly pink tint (Lighting Settings → Environment Lighting → Ambient Color → soft pink ~#FFE0E5).

- [ ] **Step 2: Create the test floor**

ProBuilder → New Shape → Plane, 20×20 units → name it `Floor`. Position (0,0,0).

Create a default Lit material `Assets/Blech/Art/Materials/M_Greybox.mat`, color light gray. Assign to `Floor`.

- [ ] **Step 3: Build the Bean player prefab (v1, no visuals yet)**

In scene Hierarchy: Create → 3D Object → Capsule. Rename to `Bean`. Remove the default `CapsuleCollider` (replaced by `CharacterController`).

Add components:

- `CharacterController` — Height 1.0, Radius 0.3, Step Offset 0.2, Center Y 0.5
- `PlayerInput` (Blech)
- `PlayerMovementController` (Blech) — drag `Bean.asset` into the `Stats` slot; leave `Camera Transform` empty (will auto-find via `Camera.main`).

- [ ] **Step 4: Set up Cinemachine FreeLook camera**

GameObject → Cinemachine → FreeLook Camera. Set `Follow` and `LookAt` to `Bean`. Default rig orbits are fine for now. Make sure the Camera with the `MainCamera` tag has a `CinemachineBrain` component (it should by default if you used the URP template's camera).

- [ ] **Step 5: Save the prefab**

Drag `Bean` from Hierarchy into `Assets/Blech/Prefabs/Player/` to create the prefab. Keep the instance in the scene.

- [ ] **Step 6: Test playmode**

Press Play. Confirm:

- Bean falls onto the floor and lands.
- WASD/Arrow keys move Bean relative to camera.
- Space jumps.
- Bean rotates toward movement direction.

Exit Play.

- [ ] **Step 7: Commit**

```bash
git add Assets/Blech/Scenes/Prototype_ClimbingToy.unity \
        Assets/Blech/Scenes/Prototype_ClimbingToy.unity.meta \
        Assets/Blech/Prefabs/Player/Bean.prefab \
        Assets/Blech/Prefabs/Player/Bean.prefab.meta \
        Assets/Blech/Art/Materials/M_Greybox.mat \
        Assets/Blech/Art/Materials/M_Greybox.mat.meta
git commit -m "feat: prototype climbing toy scene with Bean player prefab and Cinemachine camera"
```

---

## Task 6: ClimbableSurface Marker Component

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/ClimbableSurface.cs`
- Create: `Assets/Blech/Tests/Edit/ClimbableSurfaceTests.cs`

- [ ] **Step 1: Write the failing test**

Create `Assets/Blech/Tests/Edit/ClimbableSurfaceTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class ClimbableSurfaceTests
    {
        [Test]
        public void Grip_DefaultsToOne()
        {
            var go = new GameObject();
            var c = go.AddComponent<ClimbableSurface>();
            Assert.AreEqual(1f, c.grip);
            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run test, verify failure**

Test Runner → Run All. Expected: FAIL — `ClimbableSurface` missing.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/ClimbableSurface.cs`:

```csharp
using UnityEngine;

namespace Blech.World
{
    public class ClimbableSurface : MonoBehaviour
    {
        [Range(0f, 1f)] public float grip = 1f;
    }
}
```

- [ ] **Step 4: Run test, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/ClimbableSurface.cs \
        Assets/Blech/Scripts/World/ClimbableSurface.cs.meta \
        Assets/Blech/Tests/Edit/ClimbableSurfaceTests.cs \
        Assets/Blech/Tests/Edit/ClimbableSurfaceTests.cs.meta
git commit -m "feat: ClimbableSurface marker component"
```

---

## Task 7: PlayerStamina Component

**Style:** Script task (TDD)

**Files:**
- Create: `Assets/Blech/Scripts/Player/PlayerStamina.cs`
- Create: `Assets/Blech/Tests/Edit/PlayerStaminaTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/PlayerStaminaTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class PlayerStaminaTests
    {
        private PlayerCharacterStats MakeStats()
        {
            var s = ScriptableObject.CreateInstance<PlayerCharacterStats>();
            s.maxStamina = 100f;
            s.staminaDrainPerSecond = 10f;
            s.staminaRegenPerSecond = 20f;
            return s;
        }

        [Test]
        public void Initial_StaminaIsMax()
        {
            var go = new GameObject();
            var stamina = go.AddComponent<PlayerStamina>();
            stamina.Configure(MakeStats());
            Assert.AreEqual(100f, stamina.Current);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_Drains_WhenSpendingFlagOn()
        {
            var go = new GameObject();
            var stamina = go.AddComponent<PlayerStamina>();
            stamina.Configure(MakeStats());
            stamina.Tick(dt: 1f, spending: true, slipMultiplier: 1f);
            Assert.AreEqual(90f, stamina.Current, 0.001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_DrainScaledBySlipMultiplier()
        {
            var go = new GameObject();
            var stamina = go.AddComponent<PlayerStamina>();
            stamina.Configure(MakeStats());
            stamina.Tick(dt: 1f, spending: true, slipMultiplier: 2f);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_Regens_WhenNotSpending()
        {
            var go = new GameObject();
            var stamina = go.AddComponent<PlayerStamina>();
            stamina.Configure(MakeStats());
            stamina.Tick(1f, spending: true, slipMultiplier: 1f);
            stamina.Tick(1f, spending: false, slipMultiplier: 1f);
            Assert.AreEqual(100f, stamina.Current, 0.001f); // capped at max
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Tick_FiresOnZero_WhenDepleted()
        {
            var go = new GameObject();
            var stamina = go.AddComponent<PlayerStamina>();
            stamina.Configure(MakeStats());
            bool zeroFired = false;
            stamina.OnStaminaZero += () => zeroFired = true;
            stamina.Tick(100f, spending: true, slipMultiplier: 1f); // forces drain past zero
            Assert.IsTrue(zeroFired);
            Assert.AreEqual(0f, stamina.Current);
            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run tests, verify failure**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/Player/PlayerStamina.cs`:

```csharp
using System;
using UnityEngine;

namespace Blech.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        private PlayerCharacterStats _stats;
        private float _current;
        private bool _wasZero;

        public float Current => _current;
        public float Normalized => _stats == null ? 0f : _current / _stats.maxStamina;

        public event Action<float> OnStaminaChanged;
        public event Action OnStaminaZero;

        public void Configure(PlayerCharacterStats stats)
        {
            _stats = stats;
            _current = stats.maxStamina;
            _wasZero = false;
            OnStaminaChanged?.Invoke(_current);
        }

        public void Tick(float dt, bool spending, float slipMultiplier)
        {
            if (_stats == null) return;

            if (spending)
                _current -= _stats.staminaDrainPerSecond * slipMultiplier * dt;
            else
                _current += _stats.staminaRegenPerSecond * dt;

            _current = Mathf.Clamp(_current, 0f, _stats.maxStamina);
            OnStaminaChanged?.Invoke(_current);

            if (_current <= 0f && !_wasZero)
            {
                _wasZero = true;
                OnStaminaZero?.Invoke();
            }
            else if (_current > 0f)
            {
                _wasZero = false;
            }
        }
    }
}
```

- [ ] **Step 4: Run tests, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/Player/PlayerStamina.cs \
        Assets/Blech/Scripts/Player/PlayerStamina.cs.meta \
        Assets/Blech/Tests/Edit/PlayerStaminaTests.cs \
        Assets/Blech/Tests/Edit/PlayerStaminaTests.cs.meta
git commit -m "feat: PlayerStamina with drain/regen and zero event"
```

---

## Task 8: SlipperySurface Marker Component

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/SlipperySurface.cs`
- Create: `Assets/Blech/Tests/Edit/SlipperySurfaceTests.cs`

- [ ] **Step 1: Write failing test**

Create `Assets/Blech/Tests/Edit/SlipperySurfaceTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class SlipperySurfaceTests
    {
        [Test]
        public void Defaults_AreReasonable()
        {
            var go = new GameObject();
            var s = go.AddComponent<SlipperySurface>();
            Assert.AreEqual(2f, s.slipMultiplier);
            Assert.AreEqual(1f, s.slideRate);
            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/SlipperySurface.cs`:

```csharp
using UnityEngine;

namespace Blech.World
{
    public class SlipperySurface : MonoBehaviour
    {
        public float slipMultiplier = 2f;
        public float slideRate = 1f;
    }
}
```

- [ ] **Step 4: Run, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/SlipperySurface.cs \
        Assets/Blech/Scripts/World/SlipperySurface.cs.meta \
        Assets/Blech/Tests/Edit/SlipperySurfaceTests.cs \
        Assets/Blech/Tests/Edit/SlipperySurfaceTests.cs.meta
git commit -m "feat: SlipperySurface marker component"
```

---

## Task 9: PlayerClimbingController

**Style:** Hybrid — extract climb math for TDD, MonoBehaviour ties it together

**Files:**
- Create: `Assets/Blech/Scripts/Player/ClimbMath.cs`
- Create: `Assets/Blech/Scripts/Player/PlayerClimbingController.cs`
- Create: `Assets/Blech/Tests/Edit/ClimbMathTests.cs`
- Modify: `Assets/Blech/Scripts/Player/PlayerMovementController.cs` (expose enable/disable hook)

- [ ] **Step 1: Write failing tests for climb math**

Create `Assets/Blech/Tests/Edit/ClimbMathTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class ClimbMathTests
    {
        [Test]
        public void ProjectInputOnWall_VerticalInputOnVerticalWall_GivesUpward()
        {
            Vector2 input = new Vector2(0, 1);
            Vector3 wallNormal = Vector3.back; // wall facing -Z, climbing surface on +Z
            float speed = 2f;

            Vector3 v = ClimbMath.ProjectInputOnWall(input, wallNormal, speed);

            Assert.AreEqual(0f, v.x, 0.001f);
            Assert.AreEqual(2f, v.y, 0.001f);
            Assert.AreEqual(0f, v.z, 0.001f);
        }

        [Test]
        public void ProjectInputOnWall_HorizontalInputOnVerticalWall_GivesSideways()
        {
            Vector2 input = new Vector2(1, 0);
            Vector3 wallNormal = Vector3.back;
            Vector3 v = ClimbMath.ProjectInputOnWall(input, wallNormal, 2f);
            Assert.AreEqual(-2f, v.x, 0.001f); // right-of-wall when wall faces -Z is -X
            Assert.AreEqual(0f, v.y, 0.001f);
        }

        [Test]
        public void JumpOffWall_AddsAwayAndUp()
        {
            Vector3 wallNormal = Vector3.back;
            Vector3 v = ClimbMath.JumpOffWallVelocity(wallNormal, awayForce: 5f, upForce: 3f);
            Assert.AreEqual(0f, v.x, 0.001f);
            Assert.AreEqual(3f, v.y, 0.001f);
            Assert.AreEqual(-5f, v.z, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement ClimbMath**

Create `Assets/Blech/Scripts/Player/ClimbMath.cs`:

```csharp
using UnityEngine;

namespace Blech.Player
{
    public static class ClimbMath
    {
        public static Vector3 ProjectInputOnWall(Vector2 input, Vector3 wallNormal, float speed)
        {
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, wallNormal).normalized;
            Vector3 wallUp = Vector3.Cross(wallNormal, right).normalized;
            Vector3 v = right * input.x + wallUp * input.y;
            return v.normalized * (v.magnitude > 0.0001f ? speed : 0f);
        }

        public static Vector3 JumpOffWallVelocity(Vector3 wallNormal, float awayForce, float upForce)
        {
            return wallNormal * awayForce + Vector3.up * upForce;
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Modify PlayerMovementController to expose toggle**

In `PlayerMovementController.cs`, add a public method to set vertical velocity from outside (used by the climb controller to inject jump-off impulse):

```csharp
public void SetVerticalVelocity(float y)
{
    _verticalVelocity = y;
}
```

Place this near `AddExternalVelocity`.

- [ ] **Step 6: Implement PlayerClimbingController**

Create `Assets/Blech/Scripts/Player/PlayerClimbingController.cs`:

```csharp
using UnityEngine;
using Blech.World;

namespace Blech.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerStamina))]
    public class PlayerClimbingController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterStats stats;
        [SerializeField] private float castOriginHeight = 0.5f;
        [SerializeField] private float castRadius = 0.3f;
        [SerializeField] private float castDistance = 0.5f;
        [SerializeField] private float jumpAwayForce = 4f;
        [SerializeField] private float jumpUpForce = 5f;
        [SerializeField] private LayerMask wallMask = ~0;

        private CharacterController _controller;
        private PlayerInput _input;
        private PlayerMovementController _movement;
        private PlayerStamina _stamina;
        private bool _climbing;
        private Vector3 _wallNormal;
        private float _currentSlipMultiplier = 1f;
        private float _currentSlideRate;

        public bool IsClimbing => _climbing;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInput>();
            _movement = GetComponent<PlayerMovementController>();
            _stamina = GetComponent<PlayerStamina>();
            _stamina.Configure(stats);
            _stamina.OnStaminaZero += ExitClimb;
            _input.JumpPressed += TryJumpOff;
        }

        private void OnDestroy()
        {
            if (_stamina != null) _stamina.OnStaminaZero -= ExitClimb;
            if (_input != null) _input.JumpPressed -= TryJumpOff;
        }

        private void Update()
        {
            bool wallHit = TryDetectWall(out RaycastHit hit);

            if (!_climbing)
            {
                _stamina.Tick(Time.deltaTime, spending: false, slipMultiplier: 1f);
                if (_input.ClimbHeld && wallHit && hit.collider.TryGetComponent(out ClimbableSurface _))
                {
                    EnterClimb(hit);
                }
                return;
            }

            if (!_input.ClimbHeld || !wallHit || !hit.collider.TryGetComponent(out ClimbableSurface _))
            {
                ExitClimb();
                return;
            }

            _wallNormal = hit.normal;
            UpdateSlipFromHit(hit);

            Vector3 climbVel = ClimbMath.ProjectInputOnWall(_input.Move, _wallNormal, stats.climbSpeed);
            climbVel += Vector3.down * _currentSlideRate;
            _controller.Move(climbVel * Time.deltaTime);

            transform.rotation = Quaternion.LookRotation(-_wallNormal);

            _stamina.Tick(Time.deltaTime, spending: true, slipMultiplier: _currentSlipMultiplier);
        }

        private bool TryDetectWall(out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * castOriginHeight;
            return Physics.SphereCast(origin, castRadius, transform.forward, out hit, castDistance, wallMask, QueryTriggerInteraction.Ignore);
        }

        private void EnterClimb(RaycastHit hit)
        {
            _climbing = true;
            _wallNormal = hit.normal;
            UpdateSlipFromHit(hit);
            _movement.enabled = false;
        }

        private void ExitClimb()
        {
            if (!_climbing) return;
            _climbing = false;
            _movement.enabled = true;
            _currentSlipMultiplier = 1f;
            _currentSlideRate = 0f;
        }

        private void TryJumpOff()
        {
            if (!_climbing) return;
            Vector3 v = ClimbMath.JumpOffWallVelocity(_wallNormal, jumpAwayForce, jumpUpForce);
            ExitClimb();
            _movement.AddExternalVelocity(new Vector3(v.x, 0, v.z), 0.3f);
            _movement.SetVerticalVelocity(v.y);
        }

        private void UpdateSlipFromHit(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent(out SlipperySurface slip))
            {
                _currentSlipMultiplier = slip.slipMultiplier;
                _currentSlideRate = slip.slideRate;
            }
            else
            {
                _currentSlipMultiplier = 1f;
                _currentSlideRate = 0f;
            }
        }

        public void ApplyExternalImpulse(Vector3 impulse)
        {
            if (!_climbing) return;
            if (impulse.magnitude > stats.gripStrength)
                ExitClimb();
        }
    }
}
```

- [ ] **Step 7: Add component to Bean prefab**

Open `Bean.prefab`. Add `PlayerStamina` component. Add `PlayerClimbingController` component. Drag `Bean.asset` into the `Stats` slot of `PlayerClimbingController`.

- [ ] **Step 8: Add a climbable test wall to the prototype scene**

Open `Prototype_ClimbingToy.unity`. ProBuilder → New Shape → Cube, scale to (3, 8, 0.5). Position (5, 4, 5). Name `TestClimbWall`. Add `ClimbableSurface` component. Assign `M_Greybox`.

Add a small mucus patch: ProBuilder → New Shape → Cube, scale (3, 1, 0.2). Position (5, 4, 4.85) (just in front of the wall surface). Add both `ClimbableSurface` AND `SlipperySurface`. Use the same greybox material but tint slightly green for visual readability (duplicate material as `M_Mucus_Test`).

- [ ] **Step 9: Playtest**

Press Play. Confirm:

- Hold Shift near the wall and Bean enters climb (rotates to face wall).
- WASD moves Bean along the wall plane.
- Stamina drains during climb (no UI yet, observe in Inspector during play).
- Releasing Shift detaches.
- On the mucus patch, stamina drains faster and Bean slides down slightly.
- Jumping while climbing pushes Bean away from the wall and upward.

Exit Play.

- [ ] **Step 10: Commit**

```bash
git add Assets/Blech/Scripts/Player/ClimbMath.cs \
        Assets/Blech/Scripts/Player/ClimbMath.cs.meta \
        Assets/Blech/Scripts/Player/PlayerClimbingController.cs \
        Assets/Blech/Scripts/Player/PlayerClimbingController.cs.meta \
        Assets/Blech/Scripts/Player/PlayerMovementController.cs \
        Assets/Blech/Tests/Edit/ClimbMathTests.cs \
        Assets/Blech/Tests/Edit/ClimbMathTests.cs.meta \
        Assets/Blech/Prefabs/Player/Bean.prefab \
        Assets/Blech/Scenes/Prototype_ClimbingToy.unity \
        Assets/Blech/Art/Materials/M_Mucus_Test.mat \
        Assets/Blech/Art/Materials/M_Mucus_Test.mat.meta
git commit -m "feat: PlayerClimbingController with wall detection, slip handling, jump-off"
```

---

## Task 10: KillCause Enum + KillVolume Component

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/KillCause.cs`
- Create: `Assets/Blech/Scripts/World/KillVolume.cs`
- Create: `Assets/Blech/Tests/Edit/KillVolumeTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/KillVolumeTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class KillVolumeTests
    {
        [Test]
        public void KillVolume_DefaultCauseIsPit()
        {
            var go = new GameObject();
            var kv = go.AddComponent<KillVolume>();
            Assert.AreEqual(KillCause.Pit, kv.cause);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void KillCause_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "Acid"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "Pit"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(KillCause), "OutOfBounds"));
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/KillCause.cs`:

```csharp
namespace Blech.World
{
    public enum KillCause
    {
        Pit,
        Acid,
        OutOfBounds
    }
}
```

Create `Assets/Blech/Scripts/World/KillVolume.cs`:

```csharp
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class KillVolume : MonoBehaviour
    {
        public KillCause cause = KillCause.Pit;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerRespawn respawn))
                respawn.Kill(cause);
        }
    }
}
```

> Note: `PlayerRespawn` doesn't exist yet — this will not compile until Task 13. Leave the test failing temporarily; we'll come back. Skip the test verification step and re-run after Task 13.

Actually, to keep the plan running, **forward-declare** by writing a minimal stub:

Create `Assets/Blech/Scripts/Player/PlayerRespawn.cs` (stub — will be fleshed out in Task 13):

```csharp
using UnityEngine;
using Blech.World;

namespace Blech.Player
{
    public class PlayerRespawn : MonoBehaviour
    {
        public virtual void Kill(KillCause cause) { /* implemented in Task 13 */ }
    }
}
```

- [ ] **Step 4: Run tests, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/KillCause.cs \
        Assets/Blech/Scripts/World/KillCause.cs.meta \
        Assets/Blech/Scripts/World/KillVolume.cs \
        Assets/Blech/Scripts/World/KillVolume.cs.meta \
        Assets/Blech/Scripts/Player/PlayerRespawn.cs \
        Assets/Blech/Scripts/Player/PlayerRespawn.cs.meta \
        Assets/Blech/Tests/Edit/KillVolumeTests.cs \
        Assets/Blech/Tests/Edit/KillVolumeTests.cs.meta
git commit -m "feat: KillVolume + KillCause; PlayerRespawn stub"
```

---

## Task 11: CheckpointManager Singleton

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/CheckpointManager.cs`
- Create: `Assets/Blech/Tests/Edit/CheckpointManagerTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/CheckpointManagerTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class CheckpointManagerTests
    {
        [SetUp]
        public void ResetSingleton() => CheckpointManager.ResetForTests();

        [Test]
        public void DefaultSpawn_IsNull_UntilSet()
        {
            Assert.IsNull(CheckpointManager.CurrentSpawn);
        }

        [Test]
        public void SetSpawn_UpdatesCurrent()
        {
            var t = new GameObject("spawn").transform;
            CheckpointManager.SetSpawn(t);
            Assert.AreEqual(t, CheckpointManager.CurrentSpawn);
            Object.DestroyImmediate(t.gameObject);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/CheckpointManager.cs`:

```csharp
using UnityEngine;

namespace Blech.World
{
    public static class CheckpointManager
    {
        public static Transform CurrentSpawn { get; private set; }

        public static void SetSpawn(Transform t) => CurrentSpawn = t;

        public static void ResetForTests() => CurrentSpawn = null;
    }
}
```

- [ ] **Step 4: Run, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/CheckpointManager.cs \
        Assets/Blech/Scripts/World/CheckpointManager.cs.meta \
        Assets/Blech/Tests/Edit/CheckpointManagerTests.cs \
        Assets/Blech/Tests/Edit/CheckpointManagerTests.cs.meta
git commit -m "feat: CheckpointManager static singleton"
```

---

## Task 12: Checkpoint Trigger Component

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/Checkpoint.cs`
- Create: `Assets/Blech/Tests/Edit/CheckpointTests.cs`

- [ ] **Step 1: Write failing test**

Create `Assets/Blech/Tests/Edit/CheckpointTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class CheckpointTests
    {
        [SetUp]
        public void Reset() => CheckpointManager.ResetForTests();

        [Test]
        public void FirstTrigger_RegistersAsCurrentSpawn()
        {
            var go = new GameObject("cp");
            go.AddComponent<BoxCollider>().isTrigger = true;
            var cp = go.AddComponent<Checkpoint>();
            cp.RegisterFromTest();
            Assert.AreEqual(go.transform, CheckpointManager.CurrentSpawn);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SecondTrigger_DoesNotRefire()
        {
            var go = new GameObject("cp");
            go.AddComponent<BoxCollider>().isTrigger = true;
            var cp = go.AddComponent<Checkpoint>();
            int count = 0;
            cp.OnRegistered += _ => count++;
            cp.RegisterFromTest();
            cp.RegisterFromTest();
            Assert.AreEqual(1, count);
            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/Checkpoint.cs`:

```csharp
using System;
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public string displayName = "Blechmark";
        public event Action<Checkpoint> OnRegistered;

        private bool _registered;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRespawn _)) return;
            RegisterFromTest();
        }

        public void RegisterFromTest()
        {
            if (_registered) return;
            _registered = true;
            CheckpointManager.SetSpawn(transform);
            OnRegistered?.Invoke(this);
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/Checkpoint.cs \
        Assets/Blech/Scripts/World/Checkpoint.cs.meta \
        Assets/Blech/Tests/Edit/CheckpointTests.cs \
        Assets/Blech/Tests/Edit/CheckpointTests.cs.meta
git commit -m "feat: Checkpoint trigger registers spawn idempotently"
```

---

## Task 13: PlayerRespawn (Replaces Stub)

**Style:** Script task

**Files:**
- Modify: `Assets/Blech/Scripts/Player/PlayerRespawn.cs`
- Create: `Assets/Blech/Tests/Edit/PlayerRespawnTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/PlayerRespawnTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;
using Blech.World;

namespace Blech.Tests
{
    public class PlayerRespawnTests
    {
        [SetUp]
        public void Reset() => CheckpointManager.ResetForTests();

        [Test]
        public void Kill_RaisesEvent()
        {
            var go = new GameObject();
            var pr = go.AddComponent<PlayerRespawn>();
            KillCause? captured = null;
            pr.OnKill += c => captured = c;
            pr.Kill(KillCause.Acid);
            Assert.AreEqual(KillCause.Acid, captured);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Kill_TeleportsToCheckpoint()
        {
            var spawn = new GameObject("spawn").transform;
            spawn.position = new Vector3(10, 5, 7);
            CheckpointManager.SetSpawn(spawn);

            var go = new GameObject();
            go.AddComponent<CharacterController>();
            var pr = go.AddComponent<PlayerRespawn>();
            pr.Kill(KillCause.Pit);

            Assert.AreEqual(new Vector3(10, 5, 7), go.transform.position);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(spawn.gameObject);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Test Runner → Run All. Expected: FAIL.

- [ ] **Step 3: Replace the stub**

Overwrite `Assets/Blech/Scripts/Player/PlayerRespawn.cs`:

```csharp
using System;
using UnityEngine;
using Blech.World;

namespace Blech.Player
{
    public class PlayerRespawn : MonoBehaviour
    {
        public event Action<KillCause> OnKill;

        private CharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public virtual void Kill(KillCause cause)
        {
            OnKill?.Invoke(cause);

            var spawn = CheckpointManager.CurrentSpawn;
            if (spawn == null) return;

            if (_controller != null) _controller.enabled = false;
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
            if (_controller != null) _controller.enabled = true;
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Test Runner → Run All. Expected: PASS.

- [ ] **Step 5: Wire up Bean prefab**

Open `Bean.prefab`. Add `PlayerRespawn` component if not already there. Also subscribe `PlayerRespawn.RespawnPressed`: in `PlayerRespawn.Awake`, optionally add:

```csharp
var input = GetComponent<PlayerInput>();
if (input != null) input.RespawnPressed += () => Kill(KillCause.OutOfBounds);
```

(Add this to the script above before committing if you want manual R-key respawn.)

- [ ] **Step 6: Add a kill-floor and a checkpoint to the prototype scene**

Open `Prototype_ClimbingToy.unity`.

- Create a wide thin box ProBuilder cube under the floor at Y=-5, scale (50, 0.5, 50). Add `KillVolume` (cause = `Pit`). Make sure the collider is trigger.
- Create a small empty GameObject above the floor at (0, 0.5, 0) named `SpawnPoint_Start`. Add a BoxCollider trigger (scale 1×1×1). Add `Checkpoint` component.
- Position Bean prefab instance over `SpawnPoint_Start`.

- [ ] **Step 7: Playtest**

Play. Walk into the checkpoint trigger first (it auto-registers on enter). Walk off the floor to fall into the KillVolume. Confirm Bean respawns at the checkpoint. Confirm R key also triggers a respawn.

- [ ] **Step 8: Commit**

```bash
git add Assets/Blech/Scripts/Player/PlayerRespawn.cs \
        Assets/Blech/Tests/Edit/PlayerRespawnTests.cs \
        Assets/Blech/Tests/Edit/PlayerRespawnTests.cs.meta \
        Assets/Blech/Prefabs/Player/Bean.prefab \
        Assets/Blech/Scenes/Prototype_ClimbingToy.unity
git commit -m "feat: PlayerRespawn teleports to checkpoint with kill event"
```

---

## Task 14: PlayerCharacterVisual (Procedural Animation)

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/Player/PlayerCharacterVisual.cs`
- Create: `Assets/Blech/Tests/Edit/CharacterVisualMathTests.cs`
- Create: `Assets/Blech/Scripts/Player/VisualMath.cs`

- [ ] **Step 1: Failing test for visual math**

Create `Assets/Blech/Tests/Edit/CharacterVisualMathTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.Player;

namespace Blech.Tests
{
    public class CharacterVisualMathTests
    {
        [Test]
        public void Bob_AtTimeZero_IsZero()
        {
            Assert.AreEqual(0f, VisualMath.IdleBob(time: 0f, amplitude: 0.1f, frequency: 2f), 0.0001f);
        }

        [Test]
        public void Bob_AtQuarterPeriod_IsAmplitude()
        {
            float v = VisualMath.IdleBob(time: 0.25f / 2f, amplitude: 0.1f, frequency: 2f);
            Assert.AreEqual(0.1f, v, 0.001f);
        }

        [Test]
        public void Squash_VelocityHigh_ReturnsScale()
        {
            Vector3 scale = VisualMath.WalkSquash(velocityMagnitude: 4f, walkSpeed: 4f, amount: 0.1f);
            Assert.Greater(scale.x, 1f);
            Assert.Less(scale.y, 1f);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Expected: FAIL.

- [ ] **Step 3: Implement VisualMath**

Create `Assets/Blech/Scripts/Player/VisualMath.cs`:

```csharp
using UnityEngine;

namespace Blech.Player
{
    public static class VisualMath
    {
        public static float IdleBob(float time, float amplitude, float frequency)
        {
            return Mathf.Sin(time * frequency * Mathf.PI * 2f) * amplitude;
        }

        public static Vector3 WalkSquash(float velocityMagnitude, float walkSpeed, float amount)
        {
            float t = Mathf.Clamp01(velocityMagnitude / Mathf.Max(walkSpeed, 0.0001f));
            return new Vector3(1f + amount * t, 1f - amount * t, 1f + amount * t);
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Expected: PASS.

- [ ] **Step 5: Implement PlayerCharacterVisual**

Create `Assets/Blech/Scripts/Player/PlayerCharacterVisual.cs`:

```csharp
using UnityEngine;

namespace Blech.Player
{
    [RequireComponent(typeof(PlayerMovementController))]
    public class PlayerCharacterVisual : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform[] limbs;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobFrequency = 1.5f;
        [SerializeField] private float walkSquashAmount = 0.1f;
        [SerializeField] private float limbSwingAmplitude = 25f;
        [SerializeField] private float limbSwingFrequency = 4f;
        [SerializeField] private float walkSpeedReference = 4.5f;

        private CharacterController _controller;
        private Vector3 _basePos;
        private Vector3 _baseScale;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (body != null)
            {
                _basePos = body.localPosition;
                _baseScale = body.localScale;
            }
        }

        private void LateUpdate()
        {
            if (body == null) return;

            float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            float bob = VisualMath.IdleBob(Time.time, bobAmplitude, bobFrequency);
            body.localPosition = _basePos + Vector3.up * bob;
            body.localScale = Vector3.Scale(_baseScale, VisualMath.WalkSquash(speed, walkSpeedReference, walkSquashAmount));

            float swing = Mathf.Sin(Time.time * limbSwingFrequency * Mathf.PI * 2f) * limbSwingAmplitude * Mathf.Clamp01(speed / walkSpeedReference);
            for (int i = 0; i < limbs.Length; i++)
            {
                if (limbs[i] == null) continue;
                float sign = (i % 2 == 0) ? 1f : -1f;
                limbs[i].localRotation = Quaternion.Euler(swing * sign, 0, 0);
            }
        }
    }
}
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/Player/PlayerCharacterVisual.cs \
        Assets/Blech/Scripts/Player/PlayerCharacterVisual.cs.meta \
        Assets/Blech/Scripts/Player/VisualMath.cs \
        Assets/Blech/Scripts/Player/VisualMath.cs.meta \
        Assets/Blech/Tests/Edit/CharacterVisualMathTests.cs \
        Assets/Blech/Tests/Edit/CharacterVisualMathTests.cs.meta
git commit -m "feat: PlayerCharacterVisual with procedural bob/squash/limb animation"
```

---

## Task 15: Assemble Bean Prefab with Quaternius Mesh + Limbs + Eyes

**Style:** Editor task

**Files:**
- Modify: `Assets/Blech/Prefabs/Player/Bean.prefab`
- Create: `Assets/Blech/Art/Materials/M_Bean.mat`
- Create: `Assets/Blech/Art/Materials/M_Eyes.mat`

- [ ] **Step 1: Open the Bean prefab in isolation mode**

Project window → `Assets/Blech/Prefabs/Player/Bean.prefab` → double-click to open.

- [ ] **Step 2: Remove the visible capsule mesh**

Delete the MeshFilter and MeshRenderer on the root (or set the MeshFilter mesh to None). Keep the `CharacterController` and all scripts.

- [ ] **Step 3: Add the Quaternius bean mesh**

Drag a bean mesh from `_ThirdParty/Quaternius_UltimateFood/` (look for `Bean.fbx` or similar — Pinto Bean recommended) as a child of the root. Name the child `Body`. Position (0, 0.3, 0), scale ~ (1, 1, 1) but visually-match to the CharacterController capsule (Bean's body should be visible from -0.5 to 0.5 along Y when controller height = 1.0).

Create `Assets/Blech/Art/Materials/M_Bean.mat` — URP Lit, base color warm beige (~ #D4A574), smoothness 0.6. Assign to the Body mesh.

- [ ] **Step 4: Add primitive limbs**

Create four child Capsules under root, each with default `MeshFilter` capsule, scaled to (0.1, 0.15, 0.1):

- `Arm_L` — position (-0.25, 0.4, 0.1), rotation (0, 0, 90)
- `Arm_R` — position (0.25, 0.4, 0.1), rotation (0, 0, -90)
- `Leg_L` — position (-0.15, 0.1, 0)
- `Leg_R` — position (0.15, 0.1, 0)

Remove the auto-added `CapsuleCollider` on each. Assign `M_Bean` material.

- [ ] **Step 5: Add primitive eyes**

Create two Sphere children under `Body`:

- `Eye_L` — position (-0.1, 0.55, 0.25), scale (0.08, 0.08, 0.08)
- `Eye_R` — position (0.1, 0.55, 0.25), scale (0.08, 0.08, 0.08)

Remove Sphere colliders. Create `M_Eyes.mat` — URP Lit, base color white. (We'll add black iris detail later via a decal or shader trick; for MVP, white spheres + black pupil-sphere children are enough.)

Add a tiny black pupil sphere as a child of each eye, position (0, 0, 0.04), scale (0.5, 0.5, 0.5).

- [ ] **Step 6: Add PlayerCharacterVisual component**

On the Bean root, add `PlayerCharacterVisual`. Set:

- `Body` → drag the `Body` child
- `Limbs` → array of 4 elements: Arm_L, Arm_R, Leg_L, Leg_R

- [ ] **Step 7: Test in prototype scene**

Save prefab, return to `Prototype_ClimbingToy.unity`. Press Play. Confirm:

- Bean's body bobs gently while idle.
- Limbs swing while walking.
- Body squashes/stretches when moving.

- [ ] **Step 8: Commit**

```bash
git add Assets/Blech/Prefabs/Player/Bean.prefab \
        Assets/Blech/Art/Materials/M_Bean.mat \
        Assets/Blech/Art/Materials/M_Bean.mat.meta \
        Assets/Blech/Art/Materials/M_Eyes.mat \
        Assets/Blech/Art/Materials/M_Eyes.mat.meta
git commit -m "feat: Bean prefab with Quaternius mesh, primitive limbs/eyes, procedural anim wired"
```

---

## Task 16: Six Biome ShaderGraph Materials

**Style:** Editor task

**Files:**
- Create six ShaderGraph + material pairs in `Assets/Blech/Art/Materials/`

- [ ] **Step 1: Intestine Organic shader**

Right-click `Assets/Blech/Art/Materials/` → Create → Shader Graph → URP → Lit Shader Graph → name `SG_Intestine_Organic`. Open it.

In the graph:

- Base Color: a `Color` property `BaseColor` (default warm pink #E67E8C)
- Smoothness: 0.4
- Emission: `BaseColor * sin(Time * 0.5) * 0.1 + BaseColor * 0.2` (gentle pulse)

Save. Create a material from it: right-click the SG asset → Create → Material → name `M_Intestine_Organic`.

- [ ] **Step 2: Stomach Wall shader**

Create `SG_Stomach_Wall`, similar structure but:

- Base Color default: sickly green #8FA856
- Smoothness: 0.6
- No emission pulse

Material: `M_Stomach_Wall`.

- [ ] **Step 3: Throat Tissue shader**

Create `SG_Throat_Tissue`:

- Base Color: deep red-purple #6B2E45
- Smoothness: 0.5
- Vertex Position: add a small `noise * sin(Time)` perturbation to the local position node (~ 0.02 amplitude)

Material: `M_Throat_Tissue`.

- [ ] **Step 4: Mucus Slip shader**

Create `SG_Mucus_Slip`:

- Surface Type: Transparent
- Base Color: pale yellow-green #C5DCA0 with alpha 0.5
- Smoothness: 0.95
- UV scroll: `UV + Vector2(Time * 0.05, Time * 0.1)` feeding a simple noise sample to add to emission for shimmer

Material: `M_Mucus_Slip`.

- [ ] **Step 5: Acid Surface shader**

Create `SG_Acid_Surface`:

- Surface Type: Opaque (it's the top of the pool; bubbles handled via VFX)
- Base Color: neon green #B6FF3B
- Emission: strong (BaseColor * 2)
- Distortion via animated UV noise into BaseColor

Material: `M_Acid_Surface`.

- [ ] **Step 6: Tongue shader**

Create `SG_Tongue`:

- Base Color: bright pink #FF9AAB
- Smoothness: 0.85 (very glossy/saliva)
- Emission: subtle (BaseColor * 0.1)

Material: `M_Tongue`.

- [ ] **Step 7: Commit**

```bash
git add Assets/Blech/Art/Materials/
git commit -m "feat: six biome ShaderGraph materials (intestine, stomach, throat, mucus, acid, tongue)"
```

---

## Task 17: Four VFX Graphs (Bubbles, Acid Splash, Wind Streaks, Confetti)

**Style:** Editor task

**Files:**
- Create: `Assets/Blech/Art/Particles/VFX_Bubbles.vfx`
- Create: `Assets/Blech/Art/Particles/VFX_AcidSplash.vfx`
- Create: `Assets/Blech/Art/Particles/VFX_WindStreaks.vfx`
- Create: `Assets/Blech/Art/Particles/VFX_Confetti.vfx`

- [ ] **Step 1: VFX_Bubbles (ambient)**

Right-click `Assets/Blech/Art/Particles/` → Create → Visual Effects → Visual Effect Graph → name `VFX_Bubbles`. Open.

- Spawn: Constant rate, ~20/sec
- Init: Position in a sphere or cylinder volume, lifetime 2–4s, size 0.05–0.2
- Update: Velocity upward (0–1 m/s), drag for floaty feel
- Output: Particle Quad with a Kenney bubble billboard texture from `_ThirdParty/Kenney_ParticlePack/`. Alpha multiplied by lifetime (fade in/out)

- [ ] **Step 2: VFX_AcidSplash (burst)**

Create `VFX_AcidSplash`.

- Spawn: Burst, 30 particles at start
- Init: Position at origin, velocity in a cone upward, lifetime 0.5s, size 0.05–0.15
- Update: Gravity, drag
- Output: green-tinted quad with bubble texture, additive blend

Mark "Initial Spawn" off to default; the component will play on enable. Add a Visual Effect component user property to fire from script.

- [ ] **Step 3: VFX_WindStreaks**

Create `VFX_WindStreaks`.

- Spawn: Constant 60/sec while playing
- Init: Position in a thin box (the gust volume), lifetime 0.5s, velocity in gust direction (~5 m/s)
- Output: stretched billboard quad with line texture, white with low alpha

- [ ] **Step 4: VFX_Confetti**

Create `VFX_Confetti`.

- Spawn: Burst 100 particles
- Init: Velocity in random cone upward, lifetime 2s, color randomized across pink/yellow/green/blue
- Update: Gravity, drag
- Output: small quad

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Art/Particles/
git commit -m "feat: four VFX Graphs for bubbles, acid splash, wind streaks, confetti"
```

---

## Task 18: AcidHazard Composite

**Style:** Editor task with small script

**Files:**
- Create: `Assets/Blech/Scripts/World/AcidHazardVisual.cs`
- Create: `Assets/Blech/Prefabs/Hazards/AcidPool.prefab`

- [ ] **Step 1: Write AcidHazardVisual**

Create `Assets/Blech/Scripts/World/AcidHazardVisual.cs`:

```csharp
using UnityEngine;
using UnityEngine.VFX;

namespace Blech.World
{
    public class AcidHazardVisual : MonoBehaviour
    {
        [SerializeField] private VisualEffect ambientBubbles;

        private void OnEnable()
        {
            if (ambientBubbles != null) ambientBubbles.Play();
        }

        private void OnDisable()
        {
            if (ambientBubbles != null) ambientBubbles.Stop();
        }
    }
}
```

- [ ] **Step 2: Build the prefab**

In an empty test scene (or use Prototype scene): GameObject → Create Empty → name `AcidPool`. Children:

- `Surface` — ProBuilder cube scaled (10, 0.2, 10), `M_Acid_Surface` material.
- `KillVolume` — primitive cube collider set to Trigger, scaled (10, 1, 10) positioned at the surface Y. Add `KillVolume` component with `cause = Acid`.
- `Bubbles` — empty GameObject with a `Visual Effect` component referencing `VFX_Bubbles`.

On the `AcidPool` root, add `AcidHazardVisual` and drag the `Bubbles` Visual Effect into the slot.

Drag `AcidPool` into `Assets/Blech/Prefabs/Hazards/` to create the prefab.

- [ ] **Step 3: Commit**

```bash
git add Assets/Blech/Scripts/World/AcidHazardVisual.cs \
        Assets/Blech/Scripts/World/AcidHazardVisual.cs.meta \
        Assets/Blech/Prefabs/Hazards/AcidPool.prefab \
        Assets/Blech/Prefabs/Hazards/AcidPool.prefab.meta
git commit -m "feat: AcidPool prefab with KillVolume + AcidHazardVisual"
```

---

## Task 19: AcidGeyser

**Style:** Script task with prefab assembly

**Files:**
- Create: `Assets/Blech/Scripts/World/AcidGeyser.cs`
- Create: `Assets/Blech/Tests/Edit/AcidGeyserTests.cs`
- Create: `Assets/Blech/Prefabs/Hazards/AcidGeyser.prefab`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/AcidGeyserTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class AcidGeyserTests
    {
        [Test]
        public void Phase_StartsAtIdle()
        {
            var go = new GameObject();
            var g = go.AddComponent<AcidGeyser>();
            Assert.AreEqual(AcidGeyser.Phase.Idle, g.CurrentPhase);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void TickPhase_AdvancesThroughCycle()
        {
            var go = new GameObject();
            var g = go.AddComponent<AcidGeyser>();
            g.idleDuration = 1f;
            g.warningDuration = 1f;
            g.eruptDuration = 1f;
            g.cooldownDuration = 1f;

            g.TickForTest(0.5f);
            Assert.AreEqual(AcidGeyser.Phase.Idle, g.CurrentPhase);
            g.TickForTest(1f);
            Assert.AreEqual(AcidGeyser.Phase.Warning, g.CurrentPhase);
            g.TickForTest(1f);
            Assert.AreEqual(AcidGeyser.Phase.Erupt, g.CurrentPhase);
            g.TickForTest(1f);
            Assert.AreEqual(AcidGeyser.Phase.Cooldown, g.CurrentPhase);
            g.TickForTest(1f);
            Assert.AreEqual(AcidGeyser.Phase.Idle, g.CurrentPhase);

            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/AcidGeyser.cs`:

```csharp
using UnityEngine;
using UnityEngine.VFX;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class AcidGeyser : MonoBehaviour
    {
        public enum Phase { Idle, Warning, Erupt, Cooldown }

        public float idleDuration = 3f;
        public float warningDuration = 1f;
        public float eruptDuration = 1f;
        public float cooldownDuration = 2f;

        [SerializeField] private VisualEffect splashVfx;
        [SerializeField] private GameObject killVolumeRoot;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        private float _phaseTimer;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            TickForTest(Time.deltaTime);
        }

        public void TickForTest(float dt)
        {
            _phaseTimer += dt;
            switch (CurrentPhase)
            {
                case Phase.Idle:
                    if (_phaseTimer >= idleDuration) EnterPhase(Phase.Warning);
                    break;
                case Phase.Warning:
                    if (_phaseTimer >= warningDuration) EnterPhase(Phase.Erupt);
                    break;
                case Phase.Erupt:
                    if (_phaseTimer >= eruptDuration) EnterPhase(Phase.Cooldown);
                    break;
                case Phase.Cooldown:
                    if (_phaseTimer >= cooldownDuration) EnterPhase(Phase.Idle);
                    break;
            }
        }

        private void EnterPhase(Phase next)
        {
            CurrentPhase = next;
            _phaseTimer = 0f;
            if (next == Phase.Erupt)
            {
                if (splashVfx != null) splashVfx.Play();
                if (killVolumeRoot != null) killVolumeRoot.SetActive(true);
            }
            else
            {
                if (killVolumeRoot != null) killVolumeRoot.SetActive(false);
            }
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Expected: PASS.

- [ ] **Step 5: Build the prefab**

GameObject → Create Empty → `AcidGeyser`. Children:

- `KillVolume` — a thin cube trigger collider with `KillVolume(cause=Acid)`, starts disabled (will be enabled during Erupt).
- `Splash` — empty with `Visual Effect` referencing `VFX_AcidSplash`.

On root, add `AcidGeyser` script. Drag `Splash` and `KillVolume` into slots.

Save as prefab in `Assets/Blech/Prefabs/Hazards/`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/World/AcidGeyser.cs \
        Assets/Blech/Scripts/World/AcidGeyser.cs.meta \
        Assets/Blech/Tests/Edit/AcidGeyserTests.cs \
        Assets/Blech/Tests/Edit/AcidGeyserTests.cs.meta \
        Assets/Blech/Prefabs/Hazards/AcidGeyser.prefab \
        Assets/Blech/Prefabs/Hazards/AcidGeyser.prefab.meta
git commit -m "feat: AcidGeyser with timed phase cycle + splash VFX"
```

---

## Task 20: WindHazard

**Style:** Script task with prefab

**Files:**
- Create: `Assets/Blech/Scripts/World/WindHazard.cs`
- Create: `Assets/Blech/Tests/Edit/WindHazardTests.cs`
- Create: `Assets/Blech/Prefabs/Hazards/WindHazard.prefab`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/WindHazardTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class WindHazardTests
    {
        [Test]
        public void Phase_StartsAtIdle()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider>().isTrigger = true;
            var w = go.AddComponent<WindHazard>();
            Assert.AreEqual(WindHazard.Phase.Idle, w.CurrentPhase);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Phase_AdvancesThroughCycle()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider>().isTrigger = true;
            var w = go.AddComponent<WindHazard>();
            w.idleDuration = 1f;
            w.warningDuration = 1f;
            w.gustDuration = 1f;
            w.cooldownDuration = 1f;

            w.TickForTest(1f);
            Assert.AreEqual(WindHazard.Phase.Warning, w.CurrentPhase);
            w.TickForTest(1f);
            Assert.AreEqual(WindHazard.Phase.Gust, w.CurrentPhase);
            w.TickForTest(1f);
            Assert.AreEqual(WindHazard.Phase.Cooldown, w.CurrentPhase);
            w.TickForTest(1f);
            Assert.AreEqual(WindHazard.Phase.Idle, w.CurrentPhase);

            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/WindHazard.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class WindHazard : MonoBehaviour
    {
        public enum Phase { Idle, Warning, Gust, Cooldown }

        public float idleDuration = 4f;
        public float warningDuration = 1.5f;
        public float gustDuration = 1.5f;
        public float cooldownDuration = 1f;
        public Vector3 gustDirection = Vector3.up;
        public float gustStrength = 6f;

        [SerializeField] private VisualEffect streaksVfx;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        private float _phaseTimer;
        private readonly HashSet<PlayerMovementController> _playersInside = new HashSet<PlayerMovementController>();
        private readonly HashSet<PlayerClimbingController> _climbersInside = new HashSet<PlayerClimbingController>();

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void Update() => TickForTest(Time.deltaTime);

        public void TickForTest(float dt)
        {
            _phaseTimer += dt;
            switch (CurrentPhase)
            {
                case Phase.Idle:
                    if (_phaseTimer >= idleDuration) EnterPhase(Phase.Warning);
                    break;
                case Phase.Warning:
                    if (_phaseTimer >= warningDuration) EnterPhase(Phase.Gust);
                    break;
                case Phase.Gust:
                    ApplyGustToPlayers();
                    if (_phaseTimer >= gustDuration) EnterPhase(Phase.Cooldown);
                    break;
                case Phase.Cooldown:
                    if (_phaseTimer >= cooldownDuration) EnterPhase(Phase.Idle);
                    break;
            }
        }

        private void EnterPhase(Phase next)
        {
            CurrentPhase = next;
            _phaseTimer = 0f;
            if (next == Phase.Gust)
            {
                if (streaksVfx != null) streaksVfx.Play();
            }
            else if (next == Phase.Idle)
            {
                if (streaksVfx != null) streaksVfx.Stop();
            }
        }

        private void ApplyGustToPlayers()
        {
            Vector3 v = gustDirection.normalized * gustStrength;
            foreach (var p in _playersInside)
                if (p != null) p.AddExternalVelocity(v, Time.deltaTime * 2f);
            foreach (var c in _climbersInside)
                if (c != null) c.ApplyExternalImpulse(v);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovementController m)) _playersInside.Add(m);
            if (other.TryGetComponent(out PlayerClimbingController c)) _climbersInside.Add(c);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovementController m)) _playersInside.Remove(m);
            if (other.TryGetComponent(out PlayerClimbingController c)) _climbersInside.Remove(c);
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Expected: PASS.

- [ ] **Step 5: Build the prefab**

GameObject → Create Empty → `WindHazard`. Add a BoxCollider (Trigger), scale to ~ (5, 5, 5). Child: `Streaks` empty with Visual Effect referencing `VFX_WindStreaks`.

Add `WindHazard` script to root, drag `Streaks` VFX into slot.

Save as `Assets/Blech/Prefabs/Hazards/WindHazard.prefab`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/World/WindHazard.cs \
        Assets/Blech/Scripts/World/WindHazard.cs.meta \
        Assets/Blech/Tests/Edit/WindHazardTests.cs \
        Assets/Blech/Tests/Edit/WindHazardTests.cs.meta \
        Assets/Blech/Prefabs/Hazards/WindHazard.prefab \
        Assets/Blech/Prefabs/Hazards/WindHazard.prefab.meta
git commit -m "feat: WindHazard with phase cycle, gust force, and climber impulse"
```

---

## Task 21: RunStatsTracker

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/RunStatsTracker.cs`
- Create: `Assets/Blech/Tests/Edit/RunStatsTrackerTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Blech/Tests/Edit/RunStatsTrackerTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class RunStatsTrackerTests
    {
        [SetUp]
        public void Reset() => RunStatsTracker.ResetForTests();

        [Test]
        public void RecordKill_IncrementsFalls()
        {
            RunStatsTracker.RecordKill(KillCause.Pit);
            Assert.AreEqual(1, RunStatsTracker.FallCount);
        }

        [Test]
        public void RecordKill_TracksPerCauseCounts()
        {
            RunStatsTracker.RecordKill(KillCause.Acid);
            RunStatsTracker.RecordKill(KillCause.Acid);
            RunStatsTracker.RecordKill(KillCause.Pit);
            Assert.AreEqual(2, RunStatsTracker.CountForCause(KillCause.Acid));
            Assert.AreEqual(1, RunStatsTracker.CountForCause(KillCause.Pit));
        }

        [Test]
        public void RecordFallHeight_KeepsMax()
        {
            RunStatsTracker.RecordFallHeight(3f);
            RunStatsTracker.RecordFallHeight(7f);
            RunStatsTracker.RecordFallHeight(5f);
            Assert.AreEqual(7f, RunStatsTracker.MaxFallHeight);
        }

        [Test]
        public void TickTime_AccumulatesElapsed()
        {
            RunStatsTracker.TickTime(2f);
            RunStatsTracker.TickTime(1.5f);
            Assert.AreEqual(3.5f, RunStatsTracker.ElapsedSeconds, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/RunStatsTracker.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Blech.World
{
    public static class RunStatsTracker
    {
        public static int FallCount { get; private set; }
        public static float MaxFallHeight { get; private set; }
        public static float ElapsedSeconds { get; private set; }
        private static readonly Dictionary<KillCause, int> _causes = new Dictionary<KillCause, int>();

        public static int CountForCause(KillCause c) => _causes.TryGetValue(c, out int n) ? n : 0;

        public static void RecordKill(KillCause cause)
        {
            FallCount++;
            _causes[cause] = CountForCause(cause) + 1;
        }

        public static void RecordFallHeight(float height)
        {
            if (height > MaxFallHeight) MaxFallHeight = height;
        }

        public static void TickTime(float dt) => ElapsedSeconds += dt;

        public static void ResetForTests()
        {
            FallCount = 0;
            MaxFallHeight = 0;
            ElapsedSeconds = 0;
            _causes.Clear();
        }
    }
}
```

- [ ] **Step 4: Wire RunStatsTracker.RecordKill from PlayerRespawn**

Modify `PlayerRespawn.Kill`:

```csharp
public virtual void Kill(KillCause cause)
{
    OnKill?.Invoke(cause);
    Blech.World.RunStatsTracker.RecordKill(cause);

    var spawn = Blech.World.CheckpointManager.CurrentSpawn;
    if (spawn == null) return;

    if (_controller != null) _controller.enabled = false;
    transform.position = spawn.position;
    transform.rotation = spawn.rotation;
    if (_controller != null) _controller.enabled = true;
}
```

- [ ] **Step 5: Run, verify pass**

Expected: PASS.

- [ ] **Step 6: Add a RunClock MonoBehaviour to tick elapsed time in the scene**

Create `Assets/Blech/Scripts/World/RunClock.cs`:

```csharp
using UnityEngine;

namespace Blech.World
{
    public class RunClock : MonoBehaviour
    {
        public bool ticking = true;

        private void Update()
        {
            if (ticking) RunStatsTracker.TickTime(Time.deltaTime);
        }
    }
}
```

- [ ] **Step 7: Commit**

```bash
git add Assets/Blech/Scripts/World/RunStatsTracker.cs \
        Assets/Blech/Scripts/World/RunStatsTracker.cs.meta \
        Assets/Blech/Scripts/World/RunClock.cs \
        Assets/Blech/Scripts/World/RunClock.cs.meta \
        Assets/Blech/Scripts/Player/PlayerRespawn.cs \
        Assets/Blech/Tests/Edit/RunStatsTrackerTests.cs \
        Assets/Blech/Tests/Edit/RunStatsTrackerTests.cs.meta
git commit -m "feat: RunStatsTracker static singleton + RunClock + PlayerRespawn integration"
```

---

## Task 22: FinishTrigger

**Style:** Script task

**Files:**
- Create: `Assets/Blech/Scripts/World/FinishTrigger.cs`
- Create: `Assets/Blech/Tests/Edit/FinishTriggerTests.cs`

- [ ] **Step 1: Write failing test**

Create `Assets/Blech/Tests/Edit/FinishTriggerTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;
using Blech.World;

namespace Blech.Tests
{
    public class FinishTriggerTests
    {
        [Test]
        public void RaiseFromTest_FiresEventOnce()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider>().isTrigger = true;
            var ft = go.AddComponent<FinishTrigger>();
            int count = 0;
            ft.OnRouteComplete += () => count++;
            ft.RaiseFromTest();
            ft.RaiseFromTest();
            Assert.AreEqual(1, count);
            Object.DestroyImmediate(go);
        }
    }
}
```

- [ ] **Step 2: Run, verify fail**

Expected: FAIL.

- [ ] **Step 3: Implement**

Create `Assets/Blech/Scripts/World/FinishTrigger.cs`:

```csharp
using System;
using UnityEngine;
using Blech.Player;

namespace Blech.World
{
    [RequireComponent(typeof(Collider))]
    public class FinishTrigger : MonoBehaviour
    {
        public event Action OnRouteComplete;
        private bool _fired;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRespawn _)) return;
            RaiseFromTest();
        }

        public void RaiseFromTest()
        {
            if (_fired) return;
            _fired = true;
            OnRouteComplete?.Invoke();
        }
    }
}
```

- [ ] **Step 4: Run, verify pass**

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Blech/Scripts/World/FinishTrigger.cs \
        Assets/Blech/Scripts/World/FinishTrigger.cs.meta \
        Assets/Blech/Tests/Edit/FinishTriggerTests.cs \
        Assets/Blech/Tests/Edit/FinishTriggerTests.cs.meta
git commit -m "feat: FinishTrigger with one-shot OnRouteComplete event"
```

---

## Task 23: Build MVP Vertical Slice Scene + Biome 1 (Intestine)

**Style:** Editor task

**Files:**
- Create: `Assets/Blech/Scenes/MVP_VerticalSlice.unity`

- [ ] **Step 1: Create the scene**

File → New Scene → URP template → Save as `Assets/Blech/Scenes/MVP_VerticalSlice.unity`.

Delete default cube. Keep Directional Light, set color warm orange-pink and intensity ~1.2. Set ambient to a warm pink (#FFE0E5).

Add:
- A `RunClock` GameObject (empty) with the `RunClock` component.

- [ ] **Step 2: Build the intestine root**

Empty GameObject `Biome_1_Intestine` at origin. All intestine geometry goes inside.

- [ ] **Step 3: Author intestine geometry with ProBuilder**

Inside `Biome_1_Intestine`:

- **Floor corridor**: ProBuilder cube scaled (6, 0.5, 10), position (0, 0, 0). Apply `M_Intestine_Organic`. Add `MeshCollider` if needed (ProBuilder usually handles).
- **Left wall**: cube (0.5, 4, 10), position (-3, 2, 0). Same material.
- **Right wall**: cube (0.5, 4, 10), position (3, 2, 0). Same material.
- **Climb wall** (at end of corridor, ~6m tall): cube (6, 6, 0.5), position (0, 3, 8). Apply `M_Intestine_Organic`. Add `ClimbableSurface` component.
- **Mucus patch** (halfway up the climb): cube (4, 1, 0.2), position (0, 4, 7.85). Apply `M_Mucus_Slip` (Transparent). Add both `ClimbableSurface` and `SlipperySurface`.
- **Top ledge**: cube (4, 0.5, 4), position (0, 6.5, 9). Apply `M_Intestine_Organic`.
- **Checkpoint** at start: empty `Checkpoint_Start` at (0, 0.6, 1) with BoxCollider trigger (1,1,1) and `Checkpoint` component.
- **Checkpoint** at top ledge: `Checkpoint_Intestine_Top` at (0, 7, 9) similar.
- **Bubbles ambient**: a Visual Effect referencing `VFX_Bubbles` placed near (0, 1, 5).

Spawn point for the player: place the Bean prefab instance at `Checkpoint_Start` position.

- [ ] **Step 4: Add a doorway transition to stomach**

At the end of the top ledge, an empty `Transition_To_Stomach` marker. It's just a label for now; biome 2 will be positioned past it.

- [ ] **Step 5: Playtest**

Press Play. Walk through the corridor, climb the wall, slip on mucus, reach the top ledge. The top ledge checkpoint should auto-register on enter. Fall off the side to confirm respawn at the top checkpoint.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scenes/MVP_VerticalSlice.unity \
        Assets/Blech/Scenes/MVP_VerticalSlice.unity.meta
git commit -m "feat: MVP vertical slice scene + Biome 1 intestine corridor and climb"
```

---

## Task 24: Biome 2 (Stomach)

**Style:** Editor task

**Files:**
- Modify: `Assets/Blech/Scenes/MVP_VerticalSlice.unity`

- [ ] **Step 1: Position Biome_2_Stomach**

Empty `Biome_2_Stomach` parented at world (0, 0, 25) — adjacent to the intestine's exit.

- [ ] **Step 2: Build the chamber**

Approximate a hollow sphere using ProBuilder primitives. Easiest: use a large stretched cube as the chamber walls with the inside walls textured. Or: ProBuilder New Shape → Icosphere/Sphere if available, scale to ~15 units. Apply `M_Stomach_Wall`. Reverse normals (ProBuilder menu → Geometry → Flip Normals) so the inside is visible.

Add a `MeshCollider` (set Convex OFF — the inside walls need accurate collision).

- [ ] **Step 3: Place an AcidPool prefab**

Drag `Assets/Blech/Prefabs/Hazards/AcidPool.prefab` into the chamber at (0, 1, 25). Scale the Surface and KillVolume to fit the bottom of the chamber (~10m wide).

- [ ] **Step 4: Add 4 floating platforms**

Four small ProBuilder cubes scaled (2, 0.3, 2), positioned at staggered heights above the acid:

- Platform A: (-4, 3, 22)
- Platform B: (3, 5, 25)
- Platform C: (-3, 7, 28)
- Platform D: (4, 9, 26)

Add `M_Stomach_Wall` material.

- [ ] **Step 5: Add climbable patches on the chamber wall**

Two ProBuilder cube patches scaled (3, 5, 0.3) embedded on the chamber's back wall. Apply `M_Stomach_Wall` (or a slightly varied material). Add `ClimbableSurface` to each.

- [ ] **Step 6: Place one AcidGeyser**

Drag `AcidGeyser.prefab` between Platform B and Platform C, position (0, 1.5, 25). Adjust the KillVolume child scale so it reaches up to platform height when erupting.

- [ ] **Step 7: Stomach checkpoint**

Empty `Checkpoint_Stomach_Top` at (0, 9, 28) on top of Platform D. Add Box trigger + `Checkpoint`.

- [ ] **Step 8: Exit climb to throat**

A tall climbable cube (3, 12, 0.5) at (0, 10, 30), going from the stomach top up to ~22m. Apply `M_Stomach_Wall`, add `ClimbableSurface`.

- [ ] **Step 9: Lighting**

Add a sickly green Point Light inside the chamber for atmosphere.

- [ ] **Step 10: Playtest**

From the intestine ledge, jump down (or walk down) into the stomach. Climb platforms to dodge the acid and geyser. Reach the exit climb at the back. Reaching the stomach checkpoint should re-anchor respawn.

- [ ] **Step 11: Commit**

```bash
git add Assets/Blech/Scenes/MVP_VerticalSlice.unity
git commit -m "feat: Biome 2 stomach chamber with acid pool, geyser, platforms, exit climb"
```

---

## Task 25: Biome 3 (Throat)

**Style:** Editor task

**Files:**
- Modify: `Assets/Blech/Scenes/MVP_VerticalSlice.unity`

- [ ] **Step 1: Build the throat shaft**

Empty `Biome_3_Throat` at (0, 22, 30). Inside it: four ProBuilder cubes forming the inside of a square shaft (walls only, no floor/ceiling). Each wall scaled (4, 30, 0.5). Apply `M_Throat_Tissue`. Add `ClimbableSurface` to all four walls.

- [ ] **Step 2: Rest ledges**

Four small ProBuilder cubes scaled (1.5, 0.3, 1.5) jutting from alternating walls at heights:

- Ledge 1: y=8 from north wall
- Ledge 2: y=14 from east wall
- Ledge 3: y=20 from south wall
- Ledge 4: y=26 from west wall

Apply `M_Throat_Tissue`.

- [ ] **Step 3: Mucus drips on two walls**

Two slim ProBuilder cubes (0.5, 5, 0.2) on the north and south walls at random heights. Apply `M_Mucus_Slip`. Add `ClimbableSurface` + `SlipperySurface`.

- [ ] **Step 4: Two WindHazard prefabs**

Drag `WindHazard.prefab` into the shaft:

- Mid-shaft: position (0, 37, 30) (relative to world, since Biome_3 root is at (0,22,30) — so local y=15), gust direction (1, 0, 0), strength 4.
- Upper: position (0, 47, 30), gust direction (0, 1, 0), strength 8. (This one knocks climbers off.)

Scale BoxCollider on each to fill the shaft cross-section.

- [ ] **Step 5: Mid-shaft checkpoint**

Place `Checkpoint_Throat_Mid` on Ledge 2 at (1.5, 36, 30). Box trigger + `Checkpoint`.

- [ ] **Step 6: Uvula prop**

A simple ProBuilder cylinder hanging from the top of the shaft, scaled (0.5, 1.5, 0.5), position (0, 52, 30). Material `M_Throat_Tissue`. No collision.

- [ ] **Step 7: Lighting**

Add a bright point light at the top of the shaft (the "mouth above") to create dramatic upward lighting.

- [ ] **Step 8: Playtest**

From the stomach exit climb, enter the throat shaft. Climb up, rest on ledges, time the wind hazards. The upper gust should yank you off the wall if stamina is low.

- [ ] **Step 9: Commit**

```bash
git add Assets/Blech/Scenes/MVP_VerticalSlice.unity
git commit -m "feat: Biome 3 throat shaft with wind hazards, ledges, mucus drips, mid checkpoint"
```

---

## Task 26: Biome 4 (Mouth) + FinishTrigger

**Style:** Editor task

**Files:**
- Modify: `Assets/Blech/Scenes/MVP_VerticalSlice.unity`

- [ ] **Step 1: Build the tongue platform**

Empty `Biome_4_Mouth` at (0, 52, 30). Inside:

- **Tongue**: ProBuilder cube scaled (8, 0.5, 12), position (0, 0, 6). Apply `M_Tongue`.
- **Left teeth cliff**: scaled (1, 4, 12) at position (-4.5, 2, 6). Use a white material (create `M_Teeth` — URP Lit white).
- **Right teeth cliff**: mirrored.
- **Daylight background**: a large flat quad scaled to (30, 20, 1) at z=14, with a bright cyan unlit material (`M_Sky`).

- [ ] **Step 2: Place FinishTrigger at the lip edge**

Empty `FinishLine` at (0, 1, 12). Box trigger collider (10, 4, 1). Add `FinishTrigger` component.

- [ ] **Step 3: Confetti VFX**

Place a Visual Effect with `VFX_Confetti` at the FinishLine position. Disable it by default; the end-screen UI will enable it.

- [ ] **Step 4: Connect throat exit to mouth**

The Biome_3 throat top opens into the mouth. Make sure the player can transition cleanly from the throat top (y=52) onto the tongue at y=52. Add a small ramp or step if needed.

- [ ] **Step 5: Playtest the full route**

From start, complete intestine → stomach → throat → mouth. Stepping into the FinishLine should fire `FinishTrigger.OnRouteComplete` (no UI yet, observe in Inspector or via a debug log).

Add a temporary `Debug.Log("Route complete!")` listener to verify:

```csharp
// Temporarily in any MonoBehaviour you choose:
void Start() {
    FindObjectOfType<FinishTrigger>().OnRouteComplete += () => Debug.Log("Route complete!");
}
```

Remove the temporary listener after confirming, since the UI hookup is in Task 29.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scenes/MVP_VerticalSlice.unity \
        Assets/Blech/Art/Materials/M_Teeth.mat \
        Assets/Blech/Art/Materials/M_Teeth.mat.meta \
        Assets/Blech/Art/Materials/M_Sky.mat \
        Assets/Blech/Art/Materials/M_Sky.mat.meta
git commit -m "feat: Biome 4 mouth with tongue, teeth, daylight, FinishTrigger, confetti"
```

---

## Task 27: Main Menu Scene

**Style:** Editor task with small script

**Files:**
- Create: `Assets/Blech/Scenes/MainMenu.unity`
- Create: `Assets/Blech/Scripts/UI/MainMenu.cs`

- [ ] **Step 1: Write MainMenu controller**

Create `Assets/Blech/Scripts/UI/MainMenu.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blech.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string verticalSliceSceneName = "MVP_VerticalSlice";

        public void Play() => SceneManager.LoadScene(verticalSliceSceneName);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
```

- [ ] **Step 2: Create the scene**

File → New Scene → URP template → Save as `Assets/Blech/Scenes/MainMenu.unity`.

Delete default cube, keep camera and directional light. Make the camera background a solid color (pink).

- [ ] **Step 3: Build the UI**

GameObject → UI → Canvas. Set to Screen Space Overlay.

Children of Canvas:

- TextMeshPro - Text "Blech" — large bold rounded font (Fredoka or Bagel Fat One), centered top half.
- Button "Play" (TMP) — centered, large, calls `MainMenu.Play` on click.
- Button "Quit" (TMP) — below Play, calls `MainMenu.Quit`.

Create an empty GameObject `MainMenu` with the script. Wire the button onClick callbacks to its methods.

- [ ] **Step 4: Add both scenes to Build Settings**

File → Build Settings → Add Open Scenes. Make sure `MainMenu` is index 0 and `MVP_VerticalSlice` is index 1.

- [ ] **Step 5: Playtest**

Open MainMenu scene, press Play. Click "Play" → loads vertical slice. From the slice, return to MainMenu requires Esc → main menu wire-up; this happens in Task 28.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scenes/MainMenu.unity \
        Assets/Blech/Scenes/MainMenu.unity.meta \
        Assets/Blech/Scripts/UI/MainMenu.cs \
        Assets/Blech/Scripts/UI/MainMenu.cs.meta \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "feat: MainMenu scene with Play/Quit buttons"
```

---

## Task 28: HUD — Stamina Bar, Run Timer, Checkpoint Toast, Pause-To-Menu

**Style:** Editor task with scripts

**Files:**
- Create: `Assets/Blech/Scripts/UI/StaminaUI.cs`
- Create: `Assets/Blech/Scripts/UI/RunTimerUI.cs`
- Create: `Assets/Blech/Scripts/UI/CheckpointToastUI.cs`
- Create: `Assets/Blech/Scripts/UI/EscapeToMainMenu.cs`
- Modify: `Assets/Blech/Scenes/MVP_VerticalSlice.unity` (add HUD canvas)

- [ ] **Step 1: Write the four UI scripts**

Create `Assets/Blech/Scripts/UI/StaminaUI.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using Blech.Player;

namespace Blech.UI
{
    public class StaminaUI : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Color highColor = Color.green;
        [SerializeField] private Color lowColor = Color.red;
        [SerializeField] private float lowThreshold = 0.2f;
        [SerializeField] private float pulseSpeed = 4f;

        private PlayerStamina _stamina;

        private void Start()
        {
            _stamina = FindObjectOfType<PlayerStamina>();
            if (_stamina != null) _stamina.OnStaminaChanged += OnChanged;
            if (_stamina != null) OnChanged(_stamina.Current);
        }

        private void OnDestroy()
        {
            if (_stamina != null) _stamina.OnStaminaChanged -= OnChanged;
        }

        private void OnChanged(float value)
        {
            float n = _stamina != null ? _stamina.Normalized : 0f;
            fill.fillAmount = n;
            Color c = Color.Lerp(lowColor, highColor, Mathf.Clamp01(n / lowThreshold));
            if (n < lowThreshold)
                c *= (0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)));
            fill.color = c;
        }
    }
}
```

Create `Assets/Blech/Scripts/UI/RunTimerUI.cs`:

```csharp
using TMPro;
using UnityEngine;
using Blech.World;

namespace Blech.UI
{
    public class RunTimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void Update()
        {
            float t = RunStatsTracker.ElapsedSeconds;
            int m = Mathf.FloorToInt(t / 60f);
            float s = t - m * 60;
            label.text = $"{m:00}:{s:00.0}";
        }
    }
}
```

Create `Assets/Blech/Scripts/UI/CheckpointToastUI.cs`:

```csharp
using System.Collections;
using TMPro;
using UnityEngine;
using Blech.World;

namespace Blech.UI
{
    public class CheckpointToastUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float showSeconds = 2f;
        [SerializeField] private float fadeSeconds = 0.4f;

        private void Awake()
        {
            if (group != null) group.alpha = 0f;
        }

        private void Start()
        {
            foreach (var cp in FindObjectsOfType<Checkpoint>())
                cp.OnRegistered += OnRegistered;

            foreach (var f in FindObjectsOfType<FinishTrigger>())
                f.OnRouteComplete += () => Show("PEAK reached!");
        }

        private void OnRegistered(Checkpoint cp) => Show($"{cp.displayName}!");

        public void Show(string text)
        {
            if (label == null || group == null) return;
            label.text = text;
            StopAllCoroutines();
            StartCoroutine(Routine());
        }

        private IEnumerator Routine()
        {
            float t = 0f;
            while (t < fadeSeconds) { t += Time.deltaTime; group.alpha = t / fadeSeconds; yield return null; }
            group.alpha = 1f;
            yield return new WaitForSeconds(showSeconds);
            t = 0f;
            while (t < fadeSeconds) { t += Time.deltaTime; group.alpha = 1f - (t / fadeSeconds); yield return null; }
            group.alpha = 0f;
        }
    }
}
```

Create `Assets/Blech/Scripts/UI/EscapeToMainMenu.cs`:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Blech.UI
{
    public class EscapeToMainMenu : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
```

- [ ] **Step 2: Build the HUD canvas in MVP_VerticalSlice scene**

Open `MVP_VerticalSlice.unity`. Add a Screen Space Overlay Canvas named `HUD`.

Children:

- `StaminaPanel` (anchored bottom-center, 200×20 px) with an `Image` child `Fill` (set Image Type = Filled, Fill Method = Horizontal). Add `StaminaUI` script to `StaminaPanel`, drag the `Fill` Image into the `fill` slot.
- `TimerLabel` (TMP, anchored top-right, mono font). Add `RunTimerUI` script, drag the TMP text into `label`.
- `CheckpointToast` (centered top, TMP text on a CanvasGroup). Add `CheckpointToastUI` script, drag the TMP and CanvasGroup into slots.
- Add an empty `_EscapeWatcher` GameObject in the scene with `EscapeToMainMenu` component.

- [ ] **Step 3: Playtest**

Play `MVP_VerticalSlice`. Confirm:

- Stamina bar fills/empties with climbing.
- Timer counts up.
- Walking into a checkpoint shows "Blechmark!" toast.
- Esc returns to MainMenu.

- [ ] **Step 4: Commit**

```bash
git add Assets/Blech/Scripts/UI/ \
        Assets/Blech/Scenes/MVP_VerticalSlice.unity
git commit -m "feat: HUD with stamina bar, run timer, checkpoint toast, escape-to-menu"
```

---

## Task 29: End Screen

**Style:** Editor + script

**Files:**
- Create: `Assets/Blech/Scripts/UI/EndScreenUI.cs`
- Modify: `Assets/Blech/Scenes/MVP_VerticalSlice.unity`

- [ ] **Step 1: Write EndScreenUI**

Create `Assets/Blech/Scripts/UI/EndScreenUI.cs`:

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;
using Blech.World;

namespace Blech.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private TMP_Text fallsLabel;
        [SerializeField] private TMP_Text maxFallLabel;
        [SerializeField] private Button runAgainButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private VisualEffect confetti;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private RunClock runClock;

        private void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }

        private void Start()
        {
            foreach (var f in FindObjectsOfType<FinishTrigger>())
                f.OnRouteComplete += Show;

            if (runAgainButton != null) runAgainButton.onClick.AddListener(RunAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ToMainMenu);
        }

        public void Show()
        {
            if (runClock != null) runClock.ticking = false;
            if (timeLabel != null)
            {
                float t = RunStatsTracker.ElapsedSeconds;
                int m = Mathf.FloorToInt(t / 60f);
                float s = t - m * 60;
                timeLabel.text = $"Time: {m:00}:{s:00.0}";
            }
            if (fallsLabel != null) fallsLabel.text = $"Falls: {RunStatsTracker.FallCount}";
            if (maxFallLabel != null) maxFallLabel.text = $"Most dramatic fall: {RunStatsTracker.MaxFallHeight:0.0}m";
            if (confetti != null) confetti.Play();
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
                group.interactable = true;
            }
        }

        private void RunAgain()
        {
            RunStatsTracker.ResetForTests();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ToMainMenu()
        {
            RunStatsTracker.ResetForTests();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
```

- [ ] **Step 2: Build the end screen UI**

In `MVP_VerticalSlice.unity` HUD canvas: add a child `EndScreen` GameObject with a CanvasGroup and dim background image.

Children of EndScreen:

- TMP "You escaped!" header
- TMP `TimeLabel`
- TMP `FallsLabel`
- TMP `MaxFallLabel`
- Button `RunAgain`
- Button `MainMenu`

Add `EndScreenUI` to EndScreen, drag all references including the `Confetti` VFX from Biome_4 mouth and the `RunClock` scene object.

- [ ] **Step 3: Playtest the full route end-to-end**

Play. Run the full course. Stepping into the FinishTrigger should pop the End Screen with stats. "Run Again" reloads. "Main Menu" returns to title.

- [ ] **Step 4: Commit**

```bash
git add Assets/Blech/Scripts/UI/EndScreenUI.cs \
        Assets/Blech/Scripts/UI/EndScreenUI.cs.meta \
        Assets/Blech/Scenes/MVP_VerticalSlice.unity
git commit -m "feat: End screen with run stats, confetti, run-again, main-menu"
```

---

## Task 30: SfxPlayer and Audio Wiring

**Style:** Script + editor task

**Files:**
- Create: `Assets/Blech/Scripts/Audio/SfxId.cs`
- Create: `Assets/Blech/Scripts/Audio/SfxPlayer.cs`
- Modify: various scripts to call `SfxPlayer.Play(...)` at events
- Create: `Assets/Blech/Prefabs/SfxPlayer.prefab`

- [ ] **Step 1: Define SfxId enum**

Create `Assets/Blech/Scripts/Audio/SfxId.cs`:

```csharp
namespace Blech.Audio
{
    public enum SfxId
    {
        None,
        Footstep,
        Jump,
        WallGrab,
        Slip,
        FallYell,
        MucusSquelch,
        AcidBubble,
        AcidSplash,
        WindWarning,
        Checkpoint,
        FinishFanfare
    }
}
```

- [ ] **Step 2: Write the SfxPlayer**

Create `Assets/Blech/Scripts/Audio/SfxPlayer.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Blech.Audio
{
    [System.Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }

        [SerializeField] private List<SfxEntry> entries = new List<SfxEntry>();
        [SerializeField] private AudioSource source;

        private Dictionary<SfxId, SfxEntry> _byId;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _byId = new Dictionary<SfxId, SfxEntry>();
            foreach (var e in entries)
                if (!_byId.ContainsKey(e.id)) _byId.Add(e.id, e);
        }

        public static void Play(SfxId id)
        {
            if (Instance == null) return;
            if (id == SfxId.None) return;
            if (!Instance._byId.TryGetValue(id, out SfxEntry e) || e.clip == null) return;
            Instance.source.PlayOneShot(e.clip, e.volume);
        }
    }
}
```

- [ ] **Step 3: Build the SfxPlayer prefab**

Create empty GameObject `SfxPlayer` with an `AudioSource` (2D, Spatial Blend 0). Add `SfxPlayer` component, drag the AudioSource into slot.

Populate the `entries` list — one entry per `SfxId` value. Drag a Kenney clip from `_ThirdParty/Kenney_Audio/` into each entry's clip slot. For "Fall Yell" / "Mucus Squelch" — pick the closest cartoony Kenney clip; we'll polish later.

Save as `Assets/Blech/Prefabs/SfxPlayer.prefab`. Place an instance in `MainMenu.unity` (so it persists into the slice via `DontDestroyOnLoad`).

- [ ] **Step 4: Wire SFX into game events**

Add these calls at appropriate spots:

In `PlayerMovementController.OnJump`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.Jump);
```

In `PlayerClimbingController.EnterClimb`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.WallGrab);
```

In `PlayerClimbingController.UpdateSlipFromHit`, if newly slippery:

```csharp
if (_currentSlipMultiplier > 1f) Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.MucusSquelch);
```

In `PlayerRespawn.Kill`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.FallYell);
```

In `Checkpoint.RegisterFromTest` (after the OnRegistered fire):

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.Checkpoint);
```

In `WindHazard.EnterPhase` when entering `Warning`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.WindWarning);
```

In `AcidGeyser.EnterPhase` when entering `Erupt`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.AcidSplash);
```

In `FinishTrigger.RaiseFromTest`:

```csharp
Blech.Audio.SfxPlayer.Play(Blech.Audio.SfxId.FinishFanfare);
```

Each modification is a one-liner; add the `using Blech.Audio;` at the top of each file if not present.

- [ ] **Step 5: Playtest**

Run through the level. Confirm every triggered event plays a sound.

- [ ] **Step 6: Commit**

```bash
git add Assets/Blech/Scripts/Audio/ \
        Assets/Blech/Prefabs/SfxPlayer.prefab \
        Assets/Blech/Prefabs/SfxPlayer.prefab.meta \
        Assets/Blech/Scenes/MainMenu.unity \
        Assets/Blech/Scripts/Player/PlayerMovementController.cs \
        Assets/Blech/Scripts/Player/PlayerClimbingController.cs \
        Assets/Blech/Scripts/Player/PlayerRespawn.cs \
        Assets/Blech/Scripts/World/Checkpoint.cs \
        Assets/Blech/Scripts/World/WindHazard.cs \
        Assets/Blech/Scripts/World/AcidGeyser.cs \
        Assets/Blech/Scripts/World/FinishTrigger.cs
git commit -m "feat: SfxPlayer singleton with placeholder audio wired into all game events"
```

---

## Task 31: Tuning Pass + Standalone Build

**Style:** Editor + Playtest

**Files:**
- Modify: `Assets/Blech/ScriptableObjects/Characters/Bean.asset`
- Modify: various hazard prefab values
- Create: `Builds/` directory and standalone build

- [ ] **Step 1: Tune Bean stats**

Open `Bean.asset`. Playtest each pass and adjust:

- `moveSpeed`: too slow or too fast → 4.0 to 5.5 range
- `jumpForce`: should reach a 1.5m ledge → 6 to 8 range
- `maxStamina`: short enough that you need rests in throat → 70 to 100
- `staminaDrainPerSecond`: 10–15 range; tune so a single climb without slip takes ~6–10 sec
- `staminaRegenPerSecond`: should regen full in 3–4 sec on ground
- `climbSpeed`: 1.5–3 range
- `gripStrength`: 5 (used by wind hazard threshold)

Commit each tuning pass with a message like `tune: Bean stats v1` so the history shows iteration.

- [ ] **Step 2: Tune hazards**

In `MVP_VerticalSlice` scene:

- `AcidGeyser`: idle 3s, warning 1s, erupt 1s, cooldown 2s → playtest, adjust if it feels too punishing.
- `WindHazard` mid: gust strength 4, every ~6s.
- `WindHazard` upper: gust strength 8, every ~5s. Should knock low-stamina climbers off.

- [ ] **Step 3: Cut anything that isn't fun**

Per spec §6.4: saliva slip patches in the mouth are optional. If they bog the finale, delete them.

Per spec §4.3: if the throat upper-gust feels unfair more often than funny, lower strength to 6.

- [ ] **Step 4: Configure standalone build**

File → Build Settings:

- Platform: Mac (or Windows, depending on dev OS). Click "Switch Platform" if needed.
- Player Settings → set Company/Product/Icon.
- Build → output to `/Users/aaronsimmons/Projects/blech/Builds/Blech_VS_<date>/`.

Add `Builds/` to `.gitignore` so build output isn't committed:

```
Builds/
```

- [ ] **Step 5: Run the standalone build**

Open the built executable. Play through the slice. Confirm everything works outside the Editor.

- [ ] **Step 6: Final commit**

```bash
git add Assets/Blech/ProjectSettings/ \
        .gitignore \
        Assets/Blech/ScriptableObjects/Characters/Bean.asset
git commit -m "tune: balance Bean stats and hazard timings, configure standalone build"
```

- [ ] **Step 7: Push to GitHub**

```bash
git push
```

---

## Plan Self-Review

**Spec coverage** (each spec section → task):

- §1 Goal — covered by overall plan
- §2 Architecture — Tasks 4, 9 (CharacterController, spherecasts, ScriptableObject stats)
- §3 Project Foundation — Task 1
- §4.1 PlayerInput — Task 3
- §4.2 PlayerMovementController — Task 4
- §4.3 PlayerClimbingController — Task 9
- §4.4 PlayerStamina — Task 7
- §4.5 PlayerRespawn — Task 13
- §4.6 PlayerCharacterVisual — Task 14
- §4.7 Camera — Task 5
- §5.1 ClimbableSurface — Task 6
- §5.2 SlipperySurface — Task 8
- §5.3 KillVolume — Task 10
- §5.4 AcidHazard — Task 18
- §5.5 AcidGeyser — Task 19
- §5.6 WindHazard — Task 20
- §5.7 Checkpoint — Task 12
- §5.8 CheckpointManager — Task 11
- §5.9 FinishTrigger — Task 22
- §5.10 RunStatsTracker — Task 21
- §5.11 Materials & VFX — Tasks 16, 17
- §6.1 Biome 1 — Task 23
- §6.2 Biome 2 — Task 24
- §6.3 Biome 3 — Task 25
- §6.4 Biome 4 — Task 26
- §7.1 Main Menu — Task 27
- §7.2 HUD — Task 28
- §7.3 End Screen — Task 29
- §8 Audio — Task 30
- §9 Assets & Acquisition — Task 1 (third-party imports)

**Tuning + build:** Task 31.

All spec sections have a corresponding task.

**Type consistency check:**

- `KillCause` enum values: `Pit`, `Acid`, `OutOfBounds` — used consistently in Tasks 10, 13, 18, 21.
- `PlayerCharacterStats` field names — used the same across Tasks 2, 4, 7, 9.
- `PlayerStamina.Tick(dt, spending, slipMultiplier)` signature — used in Tasks 7, 9.
- `CheckpointManager.SetSpawn / CurrentSpawn / ResetForTests` — used in Tasks 11, 12, 13.
- `RunStatsTracker.RecordKill / RecordFallHeight / TickTime / ResetForTests` — used in Tasks 21, 29.
- `FinishTrigger.OnRouteComplete` event — used in Tasks 22, 28, 29.

No mismatches.

**Placeholder scan:** No "TBD" / "implement later" / "add appropriate error handling" / "similar to Task N" references. Every step shows the actual code or commands.

**Known imperfections (acceptable):**

- Task 26 step 5 includes a temporary debug listener that's intentionally removed; flagged in-task.
- Tasks 23–26 (biome construction) are unavoidably visual/editor work — they list exact positions and components but the final aesthetic depends on a human's ProBuilder judgment. This is honest, not a placeholder.
- Several scripts (`PlayerInput`, `PlayerMovementController`, hazard prefabs) can't be EditMode-tested for engine-integrated behavior; extracted pure math has tests instead. Documented as "Hybrid" or "Editor task" up front.
