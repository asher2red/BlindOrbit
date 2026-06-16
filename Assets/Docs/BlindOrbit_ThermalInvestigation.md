# Blind Orbit Thermal Investigation

## Profiling Status

Unity Profiler, Frame Debugger, Memory Profiler, and Profile Analyzer require an active editor/player profiling session. In this automation environment, direct collection was blocked:

1. Sandboxed batchmode failed to connect to the Unity licensing client.
2. Unsandboxed batchmode then reached Unity, but Unity aborted because the project was already open in another Unity instance.

Because of that, this document does not include CPU/GPU profiler screenshots or before/after measured frame-time captures. Any numbers below are project setting values or runtime telemetry fields added for the next profiling run, not claimed profiler measurements.

## Confirmed Project Settings

- `Application.targetFrameRate` is set to `60` in `GameManager`.
- `QualitySettings.vSyncCount` is now explicitly forced to `0` in `GameManager`.
- Current quality level is `Very Low`.
- Current quality `vSyncCount` is `0`.
- Fixed timestep is `0.02`, meaning 50 physics ticks per second.
- URP render scale is `1`.
- URP depth texture and opaque texture are disabled.
- URP HDR support was enabled.
- URP main light shadows were enabled.
- URP additional lights were enabled.

## Identified Thermal Issues

1. URP had unused lighting/shadow/HDR support enabled.
   - The prototype uses simple 2D placeholder sprites and does not depend on HDR, main-light shadows, or additional 3D lights.
   - These features are unnecessary rendering capability for the current visual target.

2. Runtime had no built-in profiling telemetry for long retry sessions.
   - Without an attached Profiler, there was no way to compare object counts and frame-time trends at 5 retries, 20 retries, and long play sessions from logs.

## Root Cause Analysis

Confirmed root cause from profiler data is still pending because the requested profiling tools could not be attached from this session.

Confirmed configuration overhead:

- URP lighting/shadow/HDR paths were enabled despite the prototype not using those visual features.
- This is a credible GPU/battery-risk configuration for mobile, but final thermal attribution still requires Profiler/Frame Debugger data on device.

Already-fixed retry churn from the previous audit remains relevant:

- Same-stage retry now reuses the stage root and player instead of recreating stars, obstacles, colliders, and thruster particle systems.

## Changes Implemented

- `GameManager`
  - Explicitly sets `QualitySettings.vSyncCount = 0`.
  - Keeps `Application.targetFrameRate = 60`.
  - Adds `PerformanceTelemetry` only in `UNITY_EDITOR` or `DEVELOPMENT_BUILD`.

- `UniversalRP.asset`
  - Disabled HDR support.
  - Disabled main light shadows.
  - Disabled additional lights.
  - Disabled global shadow support.

- `PerformanceTelemetry`
  - Development/editor-only logging every 5 seconds.
  - Records main thread time, render thread time, GC allocated in frame, managed memory, frame cap, VSync, and counts for key object types.
  - Tracks counts for managers, players, cameras, canvases, event systems, audio sources, and particle systems to catch retry duplication.

## How To Collect Required Measurements

Run a Development Build or play in Editor, then collect logs containing `[BlindOrbitTelemetry]`.

Capture checkpoints:

- Initial launch
- After 5 retries
- After 20 retries
- After 10 minutes

Required Profiler modules:

- CPU Usage
- GPU Usage
- Memory
- Rendering
- Physics 2D
- UI
- Audio
- Garbage Collector

Use Frame Debugger to confirm:

- No unexpected lighting/shadow passes.
- No post-processing pass.
- Transparent overdraw is limited to visible sprites, particles, and UI.

Use Memory Profiler to confirm:

- Player count remains `1`.
- GameManager count remains `1`.
- StageManager count remains `1`.
- Camera count remains stable.
- Canvas count remains stable.
- AudioSource count remains stable.
- ParticleSystem count remains stable.

## Before vs. After Comparison

Profiler measurements are not available from this session.

Expected after-change comparison targets:

- Frame cap remains 60 FPS maximum.
- Render thread time should not include unused HDR/shadow/additional-light overhead.
- Object counts should remain stable across retries.
- Managed memory should plateau after startup.
- GC allocated in frame should remain near zero during normal play.

## Remaining Risks

- Device temperature cannot be validated without on-device profiling.
- If heating remains high after the URP cleanup, the likely next areas are transparent overdraw, screen resolution/render scale, and sustained 60 FPS on the target device.
- A 30 FPS battery-saver mode should be considered only after comparing 60 FPS thermal data against design requirements.
