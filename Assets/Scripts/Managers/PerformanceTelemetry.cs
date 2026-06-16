using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using BlindOrbit.Gameplay;
using Unity.Profiling;
using UnityEngine.EventSystems;
#endif

namespace BlindOrbit.Managers
{
    public sealed class PerformanceTelemetry : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        const float SampleInterval = 5f;

        ProfilerRecorder gcAllocatedRecorder;
        ProfilerRecorder mainThreadRecorder;
        ProfilerRecorder renderThreadRecorder;
        float nextSampleTime;
        int sampleIndex;

        void OnEnable()
        {
            gcAllocatedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 64);
            renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 64);
            nextSampleTime = Time.unscaledTime + SampleInterval;
        }

        void OnDisable()
        {
            gcAllocatedRecorder.Dispose();
            mainThreadRecorder.Dispose();
            renderThreadRecorder.Dispose();
        }

        void Update()
        {
            if (Time.unscaledTime < nextSampleTime)
            {
                return;
            }

            nextSampleTime += SampleInterval;
            sampleIndex++;

            var mainMs = RecorderAverageMilliseconds(mainThreadRecorder);
            var renderMs = RecorderAverageMilliseconds(renderThreadRecorder);
            var gcBytes = gcAllocatedRecorder.Valid ? gcAllocatedRecorder.LastValue : 0;
            var managedMb = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            var gameManagers = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length;
            var stageManagers = Object.FindObjectsByType<StageManager>(FindObjectsSortMode.None).Length;
            var players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length;
            var particleSystems = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length;

            Debug.Log($"[BlindOrbitTelemetry] sample={sampleIndex} mainMs={mainMs:0.00} renderMs={renderMs:0.00} gcBytes={gcBytes} managedMb={managedMb:0.0} fpsTarget={Application.targetFrameRate} vSync={QualitySettings.vSyncCount} gameManagers={gameManagers} stageManagers={stageManagers} players={players} cameras={cameras} canvases={canvases} eventSystems={eventSystems} audioSources={audioSources} particleSystems={particleSystems}");
        }

        static double RecorderAverageMilliseconds(ProfilerRecorder recorder)
        {
            if (!recorder.Valid || recorder.Count == 0)
            {
                return 0d;
            }

            double total = 0d;
            for (var i = 0; i < recorder.Count; i++)
            {
                total += recorder.GetSample(i).Value;
            }

            return total / recorder.Count / 1000000d;
        }
#endif
    }
}
