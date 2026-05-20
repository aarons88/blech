# Blech — MVP Product Requirements Document

## 1. One-Line Pitch

**Blech** is a cartoony co-op climbing survival game where tiny food characters escape a giant digestive tract by climbing from the intestines, through the stomach, up the throat, and out the mouth.

## 2. Product Goal

Build a small, playable Unity MVP that proves the core fantasy:

> “Tiny food friends cooperatively climb through a silly-gross body world while surviving slime, acid, wind, and their own bad teamwork.”

The MVP should be fun with simple assets, readable mechanics, and enough personality to feel like an intentional game rather than a prototype.

## 3. Tone and Creative Direction

### 3.1 Tone Statement

**Blech** should feel gross in the way a Nickelodeon slime gag is gross, not in a medical or horror way.

The world is squishy, bubbly, slimy, and weird, but never realistic, bloody, disturbing, or body-horror. The correct emotional reaction is:

> “Ew, haha.”

Not:

> “That is upsetting.”

### 3.2 Tone Pillars

1. **Silly grossness**
   Slime, burps, bubbles, acid goo, mucus slides, toothy hazards, squishy tunnels.

2. **Cute food heroes**
   The playable characters are actual food pieces with tiny limbs, expressive faces, and little climbing gear.

3. **Co-op chaos**
   Players should laugh when they slip, fall, yank each other around, waste items, or barely survive a hazard.

4. **Readable cartoon danger**
   Hazards should be clear, exaggerated, and funny. No realistic anatomy or gore.

5. **Low-asset intentionality**
   The game should embrace simple shapes, bold colors, and toy-like materials so that limited art production feels like a style choice.

### 3.3 Visual Style

Recommended style:

* Stylized 3D
* Soft, rounded geometry
* Glossy toy-like food characters
* Bright, exaggerated biome colors
* Simple faces: dot eyes, eyebrows, tiny mouths
* Squishy organic environments
* Minimal texture detail
* Heavy use of particles, bubbles, fog, lighting, and sound for atmosphere

### 3.4 Content Boundaries

Allowed:

* Slime
* Cartoon acid
* Burps
* Bubbles
* Mucus
* Saliva
* Squishy walls
* Teeth, tongue, uvula, throat wind
* Cartoon digestive hazards

Avoid:

* Blood
* Gore
* Realistic organs
* Feces-focused humor
* Medical imagery
* Horror lighting
* Detailed biological textures
* Anything that makes the body feel diseased, injured, or graphic

## 4. Target Player Experience

The player should feel:

* “This is immediately funny.”
* “I understand what I am trying to do.”
* “The climbing is tense but goofy.”
* “My friends make this better and worse.”
* “The food characters are charming.”
* “The body world is silly, not gross-gross.”

## 5. Target Platform for MVP

Primary MVP platform:

* PC
* Unity Editor / Windows build

Stretch:

* Steam-compatible PC build
* Mac build later, if low friction

Do not optimize for consoles, mobile, or Steam Deck during MVP unless it comes nearly free.

## 6. Engine and Technical Stack

### 6.1 Engine

* Unity 6
* 3D project
* URP recommended for stylized lighting and performance

### 6.2 Networking

Recommended MVP networking:

* PurrNet

Networking target:

* Host/join co-op
* 2 players for MVP
* 4 players as stretch

Do not build full matchmaking for MVP. Use a simple join flow, local network, direct connect, or whatever PurrNet path is fastest to get playable multiplayer.

### 6.3 Development Philosophy

This project should be built for AI-assisted development.

Code should be:

* Modular
* Small-file friendly
* Clearly named
* Low magic
* Easy to test in isolation
* Heavily driven by components

Avoid over-engineered frameworks, giant inheritance trees, and highly abstract architecture during MVP.

## 7. MVP Scope Summary

### 7.1 MVP Includes

* 2-player co-op climbing prototype
* 3 playable food characters
* 3 connected digestive biomes
* Basic movement
* Wall climbing
* Stamina
* Slipping/falling
* Checkpoints
* Respawn
* One traversal item
* Three environmental hazards
* Simple win condition
* Simple character select
* Minimal UI
* Placeholder/cartoon audio cues

### 7.2 MVP Does Not Include

* Full campaign
* Procedural full-body generation
* Advanced combat system
* Complex inventory
* Character progression
* Cosmetic unlocks
* Full matchmaking
* Voice chat
* Realistic animation
* Advanced IK
* Boss fights
* Deep enemy AI
* Full Steam integration
* Polished menus
* Save system beyond simple settings, if needed

## 8. Core Gameplay Loop

### 8.1 Moment-to-Moment Loop

1. Move through a digestive biome.
2. Climb surfaces and avoid falling.
3. Manage stamina.
4. Avoid silly-gross hazards.
5. Use a traversal item to solve or recover from a climb.
6. Reach the next checkpoint.
7. Help or hinder co-op partner through chaos.

### 8.2 Full Run Loop for MVP

1. Start as food characters in the intestine.
2. Learn basic movement, jumping, climbing, and stamina.
3. Traverse slippery mucus sections.
4. Enter stomach chamber.
5. Avoid acid hazards and unstable platforms.
6. Climb into throat shaft.
7. Survive upward wind gusts and swallowing pulses.
8. Exit through mouth.
9. See end screen with completion time, falls, and silly stats.

## 9. Playable Characters

### 9.1 MVP Characters

The MVP should include three characters.

#### 1. Bean

Role:

* Balanced starter character

Traits:

* Medium speed
* Medium stamina
* Medium grip
* Medium weight

Visual:

* Rounded bean body
* Tiny arms and legs
* Small backpack
* Neutral cute face

#### 2. Pea

Role:

* Small, agile character

Traits:

* Slightly faster
* Slightly less stamina
* Lower weight
* Smaller body

Visual:

* Small round green pea
* Big expressive eyes
* Tiny little climbing gear
* Slightly anxious personality

#### 3. Carrot Chunk

Role:

* Stable wedge-shaped climber

Traits:

* Slightly better grip
* Slightly slower movement
* Medium stamina
* Slightly heavier

Visual:

* Orange carrot wedge or chunk
* Flat cut face
* Little green stem tuft optional
* Confident expression

### 9.2 Character Trait Philosophy

Character differences should be subtle for MVP.

Do not create a complex class system yet.

Recommended tunable stats:

* Move speed
* Jump force
* Max stamina
* Grip strength
* Slip resistance
* Carry weight, if inventory exists

### 9.3 Shared Character Features

All characters should have:

* Tiny arms
* Tiny legs
* Simple eyes
* Simple mouth
* Small climbing gear
* Clear silhouette
* Same controller architecture

## 10. Biomes

The MVP should be a vertical slice with three main biome sections plus a short exit.

## 10.1 Biome 1: Intestine Intro

### Purpose

Tutorial and first traversal space.

### Visual Identity

* Soft, rounded tunnels
* Purple/pink/orange cartoon tissue
* Slime streaks
* Bubbles
* Pulsing walls
* Semi-transparent mucus patches

### Gameplay Purpose

Teach:

* Movement
* Jumping
* Climbing
* Stamina
* Slippery surfaces
* Checkpoints

### Hazards

* Slippery mucus patches
* Mild contraction pulses
* Short fall zones

### MVP Requirements

* One short corridor
* One vertical climb
* One slippery section
* One checkpoint
* Exit transition to stomach

## 10.2 Biome 2: Stomach Chamber

### Purpose

First major hazard/comedy set piece.

### Visual Identity

* Big round chamber
* Neon-green or yellow cartoon acid
* Floating food debris platforms
* Bubbles and burps
* Churning walls
* Suspended ledges

### Gameplay Purpose

Introduce:

* Dangerous liquid hazard
* Timed movement
* Item usage
* More vertical climbing

### Hazards

* Acid pool
* Acid splash geysers
* Churning platform movement

### MVP Requirements

* One central chamber
* Acid pool at bottom
* Floating or moving platforms
* One vertical exit climb
* First useful placement of traversal item

## 10.3 Biome 3: Throat Ascent

### Purpose

Main PEAK-like climbing challenge.

### Visual Identity

* Tall vertical shaft
* Blue/purple/red cartoon throat tunnel
* Wind streaks
* Flappy tissue platforms
* Uvula silhouette or mouth light above

### Gameplay Purpose

Test:

* Sustained climbing
* Stamina management
* Wind timing
* Recovery from slipping
* Co-op positioning

### Hazards

* Up/down wind gusts
* Swallowing pulses
* Mucus drips
* Narrow ledges

### MVP Requirements

* One tall climb
* At least two wind hazard zones
* At least one checkpoint midway
* Final exit into mouth

## 10.4 Exit: Mouth / PEAK

### Purpose

Short final celebration space.

### Visual Identity

* Giant cartoon tongue
* Teeth as cliff-like shapes
* Bright daylight through lips
* Saliva puddles
* Confetti/burpy particle effects optional

### Gameplay Purpose

* Give a satisfying end point
* Reinforce the premise
* Show score/completion stats

### MVP Requirements

* Small final scramble
* Finish trigger
* End screen

## 11. Core Mechanics

## 11.1 Movement

### Requirements

Player can:

* Move on ground
* Rotate toward movement direction
* Jump
* Fall with gravity
* Recover after landing
* Respawn after falling into kill zones

### Feel Target

Movement should be:

* Responsive
* Slightly goofy
* Not overly realistic
* Forgiving enough for casual co-op

## 11.2 Climbing

### Requirements

Player can:

* Detect climbable surfaces
* Enter climbing state by holding or pressing climb input
* Move up/down/sideways on climbable surfaces
* Drain stamina while climbing
* Detach intentionally
* Fall when stamina reaches zero
* Jump away from wall

### Implementation Guidance

Start simple:

* Use raycasts or spherecasts from character body/hands.
* Use surface normal to orient climbing movement.
* Project movement input onto the wall plane.
* Do not build complex handhold systems for MVP.

### Climbable Surface Types

* Normal climbable
* Slippery climbable
* Hazardous climbable, optional stretch
* Non-climbable

## 11.3 Stamina

### Requirements

* Stamina drains while climbing.
* Stamina regenerates when grounded or resting on safe ledges.
* Low stamina should be visually obvious.
* At zero stamina, player loses grip.

### UI

* Simple stamina bar above player or in HUD
* Low stamina warning pulse optional

## 11.4 Slipping

### Requirements

* Certain surfaces reduce grip.
* Slippery surfaces increase stamina drain or cause downward slide.
* Mucus should be visually readable.

### MVP Rule

Mucus = slippery.

## 11.5 Falling and Respawn

### Requirements

* Player can fall.
* Dangerous fall zones or acid zones trigger respawn.
* Player respawns at last checkpoint.
* Falling should be funny rather than punishing.

### Stretch

* Co-op rescue/revive before respawn
* Fall damage
* Character-specific fall resistance

## 11.6 Checkpoints

### Requirements

* Checkpoints update when reached.
* Respawn uses latest checkpoint.
* Both players can share checkpoint progression for MVP.

### Flavor

Possible checkpoint names:

* Blechmark
* Safe Spot
* Digestive Rest Stop

Recommended MVP name:

* **Blechmark**

## 12. Items

## 12.1 MVP Item: Toothpick Piton

### Purpose

Give players one cooperative traversal tool.

### Behavior

* Player picks up a toothpick piton.
* Player can place it on a climbable surface.
* Once placed, it becomes a temporary rest point or anchor.
* Standing/grabbing near it can pause stamina drain or restore stamina.

### Why This Item First

* Clear visual
* Easy to implement
* Useful for climbing
* Good co-op utility
* Fits the food/body theme

### MVP Requirements

* Pick up piton item
* Carry one item
* Place piton on valid climbable surface
* Sync placed piton over network
* Piton acts as stamina-rest anchor

## 12.2 Stretch Item: Spaghetti Rope

### Purpose

Create co-op traversal and recovery moments.

### Behavior

* Place or throw a spaghetti rope.
* Other players can climb it.
* May swing slightly.

### MVP Status

Stretch only. Do not implement until piton works.

## 13. Hazards

## 13.1 Slippery Mucus

### Biome

Intestine and throat

### Behavior

* Reduces grip
* Causes sliding
* Increases stamina drain

### Visual

* Shiny translucent patches
* Drips
* Squelch sound

## 13.2 Acid Pool / Acid Splash

### Biome

Stomach

### Behavior

* Falling into acid respawns player
* Acid splash temporarily damages or knocks player loose

### Visual

* Bright neon goo
* Bubbles
* Splash particles

## 13.3 Throat Wind

### Biome

Throat

### Behavior

* Periodic gusts push players up, down, or sideways
* Gusts can knock players from surfaces if stamina/grip is low

### Visual

* Wind streaks
* Tissue flapping
* Audio cue before gust

## 14. Co-op / Multiplayer Requirements

### 14.1 MVP Player Count

* Required: 2 players
* Stretch: 4 players

### 14.2 Networked Features

Required:

* Player spawning
* Player movement sync
* Climbing state sync
* Stamina state sync enough for UI/readability
* Checkpoint progression
* Piton item pickup and placement
* Hazard effects
* Finish state

### 14.3 Session Flow

MVP flow:

1. Host creates session.
2. Client joins.
3. Both choose character, or host starts with default characters.
4. Players spawn at start.
5. Players complete route.
6. End screen appears.

### 14.4 Networking Philosophy

Keep the MVP simple.

* Prefer host-authoritative gameplay where practical.
* Do not solve cheating.
* Do not build competitive-grade prediction unless necessary.
* Prioritize playable co-op feel over perfect simulation.

## 15. UI Requirements

## 15.1 Main Menu

MVP:

* Start Host
* Join Game
* Quit

Optional:

* Character select

## 15.2 In-Game HUD

Required:

* Stamina display
* Current item indicator
* Checkpoint notification
* Simple teammate indicator

## 15.3 End Screen

Show silly run stats:

* Completion time
* Number of falls
* Pitons placed
* Most soggy player
* Most dramatic fall, stretch

## 16. Audio Direction

Audio should heavily support the silly-gross tone.

### Needed MVP Sounds

* Footsteps / tiny steps
* Jump
* Grab wall
* Slip
* Fall yell/squeak
* Mucus squelch
* Acid bubble
* Acid splash
* Wind gust warning
* Checkpoint reached
* Finish fanfare

Use placeholder sounds at first. Sound is important for charm, but custom polish can come later.

## 17. Animation Direction

MVP animation should be simple.

### Required

* Idle wiggle
* Walk/run
* Jump/fall
* Climb pose
* Slip/fall reaction

### Recommended Approach

Use simple procedural or minimal animations:

* Body bob
* Tiny limb cycling
* Squash/stretch
* Face changes
* Simple arm position while climbing

Do not require realistic humanoid animation.

## 18. Worldbuilding

### 18.1 Premise

A group of tiny food characters have been eaten and must escape before digestion finishes. Their only hope is to climb upward through a bizarre cartoon digestive world and reach the mouth, known among food-kind as **PEAK**.

### 18.2 Tone of Lore

Light, silly, and optional.

Example phrases:

* “Reach PEAK before you become soup.”
* “The throat winds are picking up.”
* “You found a Blechmark.”
* “Don’t get soggy.”

## 19. Success Criteria

The MVP is successful if:

1. Two players can complete a 5–10 minute route together.
2. Climbing feels understandable and funny.
3. Food characters feel charming even with simple assets.
4. Each biome has a clear gameplay identity.
5. Players laugh at least once from a fall, slip, gust, or co-op mistake.
6. The game reads as **Blech**, not as a generic climbing prototype.

## 20. MVP Milestones

## Milestone 1: Single-Player Climbing Toy

Goal:

* Prove movement and climbing feel.

Tasks:

* Create simple bean player prefab.
* Add ground movement.
* Add jump.
* Add climbable surface detection.
* Add climbing state.
* Add stamina drain/regeneration.
* Add fall and respawn.
* Create greybox climb wall.

Done when:

* A single player can climb, run out of stamina, fall, and respawn.

## Milestone 2: Blech Visual Prototype

Goal:

* Make the prototype feel like Blech, not a greybox.

Tasks:

* Add bean, pea, and carrot placeholder models.
* Add simple eyes/faces.
* Add intestine materials.
* Add mucus material.
* Add bubbles/slime particles.
* Add basic UI stamina bar.

Done when:

* A screenshot clearly communicates “cute food climber in silly digestive world.”

## Milestone 3: Vertical Slice Level

Goal:

* Build the MVP route structure.

Tasks:

* Build intestine intro.
* Build stomach chamber.
* Build throat ascent.
* Build mouth exit.
* Add checkpoints.
* Add acid pool.
* Add throat wind.
* Add finish trigger.

Done when:

* A single player can complete the whole route in 5–10 minutes.

## Milestone 4: Toothpick Piton

Goal:

* Add one meaningful traversal item.

Tasks:

* Create piton pickup.
* Add item carry state.
* Add placement targeting.
* Validate placement on climbable surface.
* Make piton restore or pause stamina.
* Add simple visual and sound feedback.

Done when:

* A player can place a piton to survive a difficult climb.

## Milestone 5: PurrNet Co-op

Goal:

* Make the route playable by two players.

Tasks:

* Add host/join flow.
* Add networked player spawn.
* Sync player movement.
* Sync climbing state.
* Sync checkpoint state.
* Sync piton pickup and placement.
* Sync hazard effects.
* Test full route with two players.

Done when:

* Two players can start together, climb together, place pitons, hit checkpoints, and finish.

## Milestone 6: MVP Polish Pass

Goal:

* Make the MVP presentable.

Tasks:

* Add title screen.
* Add end screen stats.
* Add basic sound effects.
* Add better particles.
* Add simple character select.
* Add route timing.
* Fix major bugs.
* Improve readability of hazards.

Done when:

* The game can be shown to another person without explaining every system first.

## 21. AI-Assisted Development Guidelines

When using AI coding agents, keep prompts small and concrete.

Good prompt shape:

> “Implement a ClimbableSurface component and a PlayerClimbingController that uses spherecasts to detect climbable walls, enters climb state when the climb input is held, projects movement along the wall plane, drains stamina, and detaches when stamina reaches zero. Keep the code simple and component-based.”

Avoid:

> “Make the whole game.”

### 21.1 Recommended Ticket Size

Each AI ticket should produce:

* 1–3 scripts
* One test scene change
* One clearly testable behavior

### 21.2 Coding Style

* Prefer readable code over clever code.
* Use explicit names.
* Avoid premature abstraction.
* Keep systems independent where possible.
* Use ScriptableObjects for tunable character stats and item data.
* Use components for world behaviors like hazards and checkpoints.

## 22. Suggested Initial Folder Structure

```text
Assets/
  Blech/
    Art/
      Characters/
      Environment/
      Materials/
      Particles/
    Audio/
      SFX/
      Music/
    Prefabs/
      Player/
      Items/
      Hazards/
      Level/
      UI/
    Scenes/
      Prototype_ClimbingToy.unity
      MVP_VerticalSlice.unity
    Scripts/
      Player/
        PlayerMovementController.cs
        PlayerClimbingController.cs
        PlayerStamina.cs
        PlayerRespawn.cs
        PlayerCharacterStats.cs
      Networking/
        NetworkPlayerSpawner.cs
        NetworkPlayerState.cs
        NetworkItemSync.cs
      Items/
        ItemPickup.cs
        PlayerInventory.cs
        ToothpickPiton.cs
      World/
        ClimbableSurface.cs
        SlipperySurface.cs
        AcidHazard.cs
        WindHazard.cs
        Checkpoint.cs
        FinishTrigger.cs
      UI/
        StaminaUI.cs
        EndScreenUI.cs
    ScriptableObjects/
      Characters/
      Items/
      Hazards/
```

## 23. Initial Implementation Tickets

### Ticket 1: Project Setup

Create Unity 6 URP project named Blech. Add folders, initial scenes, and placeholder materials.

### Ticket 2: Bean Player Movement

Create a simple controllable bean character with movement, jump, gravity, and camera follow.

### Ticket 3: Climbable Surfaces

Add climbable surface detection and basic climbing state.

### Ticket 4: Stamina

Add stamina drain while climbing and regeneration while grounded/resting.

### Ticket 5: Slip and Mucus

Add slippery surfaces that reduce grip and cause sliding.

### Ticket 6: Checkpoints and Respawn

Add checkpoint triggers and respawn behavior.

### Ticket 7: Intestine Test Scene

Create a small intestine-themed traversal room with a climb, mucus, and checkpoint.

### Ticket 8: Stomach Acid Hazard

Add acid pool and splash hazard behaviors.

### Ticket 9: Throat Wind Hazard

Add periodic wind gust volumes with visual/audio warning.

### Ticket 10: Toothpick Piton

Add pickup, placement, and stamina-rest behavior.

### Ticket 11: MVP Route Assembly

Connect intestine, stomach, throat, and mouth exit into a playable route.

### Ticket 12: PurrNet Prototype

Add basic host/join, networked player spawning, and transform sync.

### Ticket 13: Network Climbing State

Sync climbing state and stamina-relevant state for co-op readability.

### Ticket 14: Network Items and Hazards

Sync piton placement and hazard outcomes.

### Ticket 15: Character Select

Add Bean, Pea, and Carrot Chunk selection with simple stat differences.

### Ticket 16: MVP Polish

Add UI, audio, particles, end screen stats, and bug fixes.

## 24. Risks and Mitigations

### Risk: Climbing feels bad

Mitigation:

* Prototype single-player climbing before networking.
* Keep the controller simple.
* Tune stamina and grip early.

### Risk: Networking makes movement janky

Mitigation:

* Start host-authoritative.
* Avoid complex physics interactions at first.
* Keep player collision simple.
* Add prediction only if necessary.

### Risk: Theme becomes too gross

Mitigation:

* Use bright colors and cartoon shapes.
* Avoid realism.
* Use slime/bubbles instead of biological detail.
* Keep characters cute and expressive.

### Risk: Scope explodes

Mitigation:

* Only 3 characters.
* Only 3 biomes.
* Only 1 item.
* No real combat in MVP.
* No advanced animation in MVP.

### Risk: AI-generated code becomes messy

Mitigation:

* Use small tickets.
* Review every script.
* Keep architecture simple.
* Refactor after each milestone.

## 25. Stretch Features After MVP

Only consider after MVP is playable.

* 4-player co-op
* Proximity voice
* Spaghetti rope
* More food characters
* More digestive biomes
* Daily seed route
* Procedural chunk assembly
* Emotes
* Cosmetic gear
* More status effects
* Mild enemy hazards
* Boss-like stomach churn event
* Steam lobby integration
* Achievements
* Leaderboards
* Photo mode

## 26. Open Questions

1. Should the game be purely co-op, or can players lightly sabotage each other?
2. Should falling be mostly harmless, or should there be meaningful run pressure?
3. Should characters have unique abilities, or only minor stat differences?
4. Should the body belong to a giant human, monster, animal, or remain abstract?
5. Should the game support solo play as a first-class mode?
6. Should the title be exactly **Blech**, **Blech!**, or **Blech: [Subtitle]**?

## 27. Current MVP Recommendation

Build the MVP as:

> **A 2-player co-op Unity climbing prototype called Blech, featuring Bean, Pea, and Carrot Chunk escaping through a silly digestive vertical slice: intestine intro, stomach acid chamber, throat wind climb, and mouth exit.**

Do not add combat, complex procedural generation, or advanced animation until this version is fun.
