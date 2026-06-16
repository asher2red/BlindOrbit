using BlindOrbit.UI;
using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class GameManager : MonoBehaviour
    {
        static GameManager instance;

        LevelLoader levelLoader;
        StageManager stageManager;
        CameraController cameraController;
        UIManager uiManager;
        AudioManager audioManager;
        SaveManager saveManager;
        RankingManager rankingManager;
        SceneFlowManager sceneFlowManager;
        LifeManager lifeManager;
        ScoreManager scoreManager;
        GameObject gameplayRoot;

        public static GameManager Instance => instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindFirstObjectByType<GameManager>() != null)
            {
                return;
            }

            var gameManager = new GameObject("GameManager", typeof(GameManager));
            DontDestroyOnLoad(gameManager);
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            BuildPersistentManagers();
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void StartNewRun()
        {
            CleanupGameplaySystems();
            gameplayRoot = new GameObject("Gameplay Systems");
            gameplayRoot.transform.SetParent(transform, false);

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetParent(transform, false);
            camera.transform.SetParent(gameplayRoot.transform, false);

            levelLoader = gameplayRoot.AddComponent<LevelLoader>();
            stageManager = gameplayRoot.AddComponent<StageManager>();
            cameraController = camera.gameObject.AddComponent<CameraController>();
            uiManager = gameplayRoot.AddComponent<UIManager>();
            audioManager = gameplayRoot.AddComponent<AudioManager>();
            lifeManager = gameplayRoot.AddComponent<LifeManager>();
            scoreManager = gameplayRoot.AddComponent<ScoreManager>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            gameplayRoot.AddComponent<PerformanceTelemetry>();
#endif

            levelLoader.Initialize();
            cameraController.Initialize(camera);
            audioManager.Initialize();
            scoreManager.ResetRun();
            stageManager.Initialize(levelLoader, cameraController, uiManager, audioManager, lifeManager, scoreManager, sceneFlowManager);
            uiManager.Initialize(stageManager, lifeManager, scoreManager);
            lifeManager.ResetRun();
            stageManager.StartCurrentStage();
        }

        public void CleanupGameplaySystems()
        {
            if (gameplayRoot != null)
            {
                Destroy(gameplayRoot);
                gameplayRoot = null;
            }

            levelLoader = null;
            stageManager = null;
            cameraController = null;
            uiManager = null;
            audioManager = null;
            lifeManager = null;
            scoreManager = null;
        }

        void BuildPersistentManagers()
        {
            saveManager = gameObject.AddComponent<SaveManager>();
            rankingManager = gameObject.AddComponent<RankingManager>();
            sceneFlowManager = gameObject.AddComponent<SceneFlowManager>();
            rankingManager.Initialize(saveManager);
            sceneFlowManager.Initialize(this, rankingManager);
        }
    }
}
