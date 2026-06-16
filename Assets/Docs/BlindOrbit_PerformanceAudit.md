# Blind Orbit Performance Audit

## Profiler Status

Unity batchmode profiling could not be collected in this environment because the editor failed during licensing initialization:

- `Licensing initialization failed`
- `Failed to connect to LicenseClient-peter`

No CPU/GPU/memory numbers are reported here as measured profiler results. The changes below are based on code-level lifecycle inspection of retry, stage reload, UI, particles, audio, and update loops.

## Identified Issues

1. Retry rebuilt the entire stage every time.
   - Each retry created a new stage root, background, 90 stars, obstacles, player, Rigidbody2D, colliders, and three player particle systems.
   - Old objects were destroyed with Unity's delayed `Destroy`, so rapid retries could temporarily overlap old and new stage objects for a frame.

2. Failure reveal coroutine was not tracked.
   - A reveal coroutine could remain scheduled if stage restart was triggered before the reveal flow completed.

3. UI updated text and slider values every frame.
   - Speed text used string formatting every `Update`.
   - Fuel slider value was assigned every `Update`.

4. Thruster particle emission was written every `FixedUpdate`.
   - Emission state was set even when the input state had not changed.

5. Runtime audio clips had no explicit cleanup path.
   - Generated placeholder clips were intended to live for the manager lifetime, but explicit cleanup is safer for editor play-mode restarts and scene teardown.

## Root Cause Analysis

The most likely retry-related performance growth was object churn rather than a single permanent static leak. The previous retry flow destroyed and recreated nearly all gameplay objects. Repeated retries therefore caused:

- bursts of allocations
- repeated particle system creation
- repeated collider and renderer creation
- delayed destruction overlap
- avoidable garbage collection pressure

Gameplay did not require rebuilding static stage geometry on same-stage retry, so the stage can be reused safely.

## Code Changes Made

- `StageManager`
  - Reuses the current stage root on retry.
  - Reinitializes the existing player instead of recreating the whole stage.
  - Rebuilds stage geometry only when changing to another stage.
  - Tracks and stops the failure reveal coroutine on restart/cleanup.
  - Disables an old stage root before destroying it during stage changes.
  - Cleans up stage state in `OnDestroy`.

- `PlayerController`
  - Caches thruster VFX active states.
  - Updates particle emission only when effect state changes.
  - Force-stops thrusters during initialization and disable.

- `UIManager`
  - Caches failure reason text instead of using `Transform.Find` at failure time.
  - Updates speed text only when the displayed tenth changes.
  - Updates fuel slider only when the fuel value changes.
  - Keeps critical fuel blinking behavior below 20%.

- `AudioManager`
  - Guards `Initialize` against duplicate AudioSource/clip creation.
  - Destroys generated placeholder clips in `OnDestroy`.

- `GameManager`
  - Clears the static singleton reference in `OnDestroy`.

## Expected Optimization Impact

- Same-stage retry no longer recreates stars, obstacles, static renderers, colliders, or player particle systems.
- Repeated retries should have stable object counts.
- Gameplay UI should generate less garbage.
- Particle systems should do less control work while inputs are held.
- Stage transitions should avoid visible/physical overlap from old roots.

## Before vs After Profiler Results

Profiler data was not collected because Unity could not launch past licensing in batchmode on this machine.

Required manual verification in Unity Profiler:

- Initial launch
- After 10 retries
- After 50 retries

Expected result after these changes:

- Stage object count remains stable across same-stage retries.
- Particle system count remains stable across same-stage retries.
- AudioSource count remains stable.
- Managed allocations during normal gameplay are near zero, except occasional UI text changes and Unity internals.
- Memory should not climb after repeated retries.

## Remaining Optimization Opportunities

- Convert runtime-created stage geometry into pooled prefabs if stage switching becomes frequent.
- Pool optional explosion effects if a visible explosion effect is added later.
- Replace UGUI `Text` with TextMeshPro only if the project standardizes on TMP and profiling shows UGUI text cost matters.
- Build sprite atlases for authored art once placeholder sprites are replaced.
- Use the Unity Profiler on device to inspect GPU overdraw from transparent UI and stage reveal mode.
