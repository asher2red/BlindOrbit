# Blind Orbit Prototype Plan

## Scene Hierarchy Proposal

- `GameManager`
  - `LevelLoader`
  - `StageManager`
  - `UIManager`
  - `AudioManager`
- `Main Camera`
  - `CameraController`
- Runtime stage root
  - `Space Background`
  - `Stage Boundary`
  - `Goal Area`
  - handcrafted obstacle objects
  - `Player Ship`
- `Blind Orbit UI`
  - gameplay HUD
  - failure panel
  - clear panel

The prototype currently bootstraps this hierarchy automatically when any scene starts.

## Folder Structure

- `Assets/Art`
- `Assets/Audio`
- `Assets/Materials`
- `Assets/Prefabs`
- `Assets/Scenes`
- `Assets/Scripts/Core`
- `Assets/Scripts/Gameplay`
- `Assets/Scripts/UI`
- `Assets/Scripts/Managers`
- `Assets/Scripts/Utility`

## Required Prefabs List

- `Player Ship`
- `Circle Asteroid`
- `Ellipse Asteroid`
- `Long Wall`
- `Hollow Ring`
- `Maze Structure`
- `Goal Area`
- `Stage Boundary`

For the first playable prototype these are generated at runtime from simple sprites and colliders. They can be converted to prefabs after the feel is validated.

## Required Scripts List

- `GameManager`
- `StageManager`
- `PlayerController`
- `CameraController`
- `FuelSystem`
- `UIManager`
- `LevelLoader`
- `AudioManager`
- `StageDefinition`
- `StageLibrary`
- `StageObstacle`
- `GoalArea`
- `ObstacleMarker`
- `PlaceholderSpriteFactory`

## Core Class Responsibilities

- `GameManager`: bootstraps the prototype and connects managers.
- `LevelLoader`: stores current stage index and loads handcrafted stage data.
- `StageManager`: builds stages, handles success/failure/retry/next-stage flow.
- `PlayerController`: handles touch/mouse thrust, inertia movement, velocity-facing visual rotation, collision callbacks.
- `FuelSystem`: tracks max fuel, current fuel, and fuel consumption.
- `CameraController`: follows the player with a small view and zooms out on failure/clear.
- `UIManager`: builds HUD, fuel gauge, speed readout, goal indicator, pause, failure, and clear UI.
- `AudioManager`: provides placeholder thruster, collision, explosion, and goal sounds.

## Implementation Order

1. Runtime bootstrap and manager wiring.
2. Inertia-based player movement with mobile-style left/right input.
3. Fuel consumption and HUD feedback.
4. Static handcrafted stage builder.
5. Obstacle collision, boundaries, and goal trigger.
6. Failure reveal, retry, clear, and next-stage flow.
7. Replace runtime placeholders with authored prefabs and visual polish.
8. Playtest and tune fuel, initial velocity, camera size, and obstacle spacing.

## MVP Scope

- Portrait 2D top-down play.
- Small follow camera during gameplay.
- Local crash-site reveal after failure.
- Ten handcrafted prototype stages.
- Placeholder geometric obstacles: circle, ellipse, wall, hollow ring, maze.
- Fuel gauge, speed indicator, goal direction, pause, retry, next stage.
- Marker placement, marker count HUD, and per-stage marker cleanup.
- Placeholder generated audio.

Primary validation question: is navigating hidden obstacles with inertia-based movement fun?

## Current Balance Direction

- Stages are scaled far beyond the gameplay camera view.
- The ship starts from zero velocity on every attempt.
- The ship uses separate rotation and forward thrust controls.
- Turning is inexpensive and damped, so the ship does not spin forever.
- Forward thrust is fuel-expensive and creates momentum.
- Translational velocity uses configurable `linearDamping` on `PlayerController`; default `0.55` makes the ship slow gradually over several seconds.
- Efficient routes should leave only a small fuel reserve.

## Added UX And Content Notes

- Failure camera now stays near the crash site and zooms out locally instead of revealing the full level.
- Markers are red world-space X notes placed at the player's current position.
- Marker limit defaults to `5` in `StageManager`.
- Markers persist through retries on the same stage and are cleared on stage clear or stage change.
- Stages 1-3 introduce controls, obstacle reading, and hollow structures.
- Stages 4-6 add larger route choices and punish boundary hugging with angled edge obstacles.
- Stages 7-10 expect route memory, marker use, and fuel-efficient central navigation.
- Boundary exploits are discouraged with visible obstacles near outer routes rather than invisible walls.
