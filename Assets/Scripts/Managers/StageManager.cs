using System.Collections;
using System.Collections.Generic;
using BlindOrbit.Core;
using BlindOrbit.Gameplay;
using BlindOrbit.UI;
using BlindOrbit.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlindOrbit.Managers
{
    public sealed class StageManager : MonoBehaviour
    {
        LevelLoader levelLoader;
        CameraController cameraController;
        UIManager uiManager;
        AudioManager audioManager;
        LifeManager lifeManager;
        ScoreManager scoreManager;
        SceneFlowManager sceneFlowManager;
        GameObject stageRoot;
        Transform markerRoot;
        PlayerController player;
        readonly List<GameObject> markers = new List<GameObject>();
        const int DefaultMaxMarkerCount = 5;
        [SerializeField, Min(0)] int maxMarkerCount = DefaultMaxMarkerCount;
        int loadedStageIndex = -1;
        Coroutine failureRevealCoroutine;
        float stageStartTime;
        float emptyFuelTimer;

        public event System.Action<int, int> MarkerCountChanged;
        public GameState State { get; private set; } = GameState.Boot;
        public Vector2 GoalPosition { get; private set; }
        public Vector2 CurrentBounds { get; private set; }
        public int ActiveMarkerCount => markers.Count;
        public int MaxMarkerCount => maxMarkerCount;

        public void Initialize(LevelLoader loader, CameraController camera, UIManager ui, AudioManager audio, LifeManager lives, ScoreManager score, SceneFlowManager flow)
        {
            levelLoader = loader;
            cameraController = camera;
            uiManager = ui;
            audioManager = audio;
            lifeManager = lives;
            scoreManager = score;
            sceneFlowManager = flow;
        }

        public void StartCurrentStage()
        {
            Time.timeScale = 1f;
            StopFailureReveal();

            var stage = levelLoader.LoadCurrent();
            CurrentBounds = stage.BoundsSize;
            GoalPosition = stage.GoalPosition;
            emptyFuelTimer = 0f;
            stageStartTime = Time.time;
            State = GameState.Playing;

            if (stageRoot == null || loadedStageIndex != levelLoader.CurrentIndex || player == null)
            {
                CleanupStage();
                BuildStage(stage);
            }

            player.Initialize(this, stage.PlayerStart, stage.InitialVelocity, stage.Fuel, audioManager);
            cameraController.Follow(player, stage.BoundsSize);
            uiManager.BindPlayer(player, player.Fuel, stage.StageName);
            NotifyMarkerCountChanged();
        }

        public void RetryStage()
        {
            StartCurrentStage();
        }

        public void NextStage()
        {
            ClearMarkers();
            levelLoader.LoadNext();
            StartCurrentStage();
        }

        public void TogglePause()
        {
            if (State == GameState.Paused)
            {
                State = GameState.Playing;
                Time.timeScale = 1f;
                uiManager.SetPauseVisual(false);
                return;
            }

            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.Paused;
            Time.timeScale = 0f;
            uiManager.SetPauseVisual(true);
        }

        void Update()
        {
            if (State != GameState.Playing || player == null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                PlaceMarker();
            }

            var half = CurrentBounds * 0.5f;
            var position = (Vector2)player.transform.position;
            if (Mathf.Abs(position.x) > half.x || Mathf.Abs(position.y) > half.y)
            {
                FailStage("Left Stage Boundary");
                return;
            }

            if (player.Fuel.IsEmpty)
            {
                emptyFuelTimer += Time.deltaTime;
                if (emptyFuelTimer > 1.4f && player.Velocity.magnitude < 0.35f)
                {
                    FailStage("Fuel Depleted");
                }
            }
            else
            {
                emptyFuelTimer = 0f;
            }
        }

        public void FailStage(string reason)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.RevealingFailure;
            player.StopInput();
            audioManager.PlayCollision();
            lifeManager.LoseLife();
            cameraController.RevealDeathArea(player.transform.position, CurrentBounds);
            failureRevealCoroutine = StartCoroutine(ShowFailureAfterReveal(reason));
        }

        public void ClearStage()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            State = GameState.Cleared;
            player.StopInput();
            var isFinalStage = !levelLoader.HasNextStage;
            var scoreBreakdown = scoreManager.RecordStageClear(levelLoader.CurrentIndex, player.Fuel.CurrentFuel, lifeManager.Lives, isFinalStage);
            ClearMarkers();
            audioManager.PlayGoal();
            cameraController.RevealFullStage(CurrentBounds);
            if (isFinalStage)
            {
                uiManager.ShowClear(scoreBreakdown, false, () => sceneFlowManager.CompleteRun(scoreManager.CalculateFinalScore()));
                return;
            }

            uiManager.ShowClear(scoreBreakdown, true, null);
        }

        public void PlaceMarker()
        {
            if (State != GameState.Playing || player == null)
            {
                return;
            }

            if (markers.Count >= maxMarkerCount)
            {
                uiManager.ShowMarkerLimitFeedback();
                return;
            }

            var marker = CreateMarker(player.transform.position);
            markers.Add(marker);
            NotifyMarkerCountChanged();
        }

        void OnDestroy()
        {
            CleanupStage();
        }

        void OnValidate()
        {
            maxMarkerCount = Mathf.Max(0, maxMarkerCount);
            if (Application.isPlaying)
            {
                NotifyMarkerCountChanged();
            }
        }

        IEnumerator ShowFailureAfterReveal(string reason)
        {
            yield return new WaitForSecondsRealtime(2.2f);
            failureRevealCoroutine = null;
            if (lifeManager.Lives > 0)
            {
                StartCurrentStage();
                yield break;
            }

            State = GameState.Failed;
            sceneFlowManager.CompleteRun(scoreManager.CalculateFinalScore());
        }

        void BuildStage(StageRuntimeData stage)
        {
            loadedStageIndex = levelLoader.CurrentIndex;
            stageRoot = new GameObject($"Stage - {stage.StageName}");
            markerRoot = new GameObject("Player Markers").transform;
            markerRoot.SetParent(stageRoot.transform, false);
            CreateBackground(stage.BoundsSize);
            CreateBoundary(stage.BoundsSize);
            CreateGoal(stage.GoalPosition, stage.GoalSize);

            foreach (var obstacle in stage.Obstacles)
            {
                CreateObstacle(obstacle);
            }

            player = CreatePlayer(stage.PlayerStart, stage.InitialVelocity, stage.Fuel);
        }

        PlayerController CreatePlayer(Vector2 start, Vector2 initialVelocity, float fuel)
        {
            var playerObject = new GameObject("Player Ship", typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(FuelSystem), typeof(PlayerController));
            playerObject.transform.SetParent(stageRoot.transform, false);
            var controller = playerObject.GetComponent<PlayerController>();
            controller.Initialize(this, start, initialVelocity, fuel, audioManager);
            return controller;
        }

        void CreateBackground(Vector2 bounds)
        {
            var background = new GameObject("Space Background");
            background.transform.SetParent(stageRoot.transform, false);

            CreateSpriteObject("Dark Space", background.transform, Vector2.zero, bounds + new Vector2(4f, 4f), 0f, new Color(0.012f, 0.015f, 0.032f, 1f), -20, PlaceholderSpriteFactory.Square());

            for (var i = 0; i < 90; i++)
            {
                var star = CreateSpriteObject("Star", background.transform, new Vector2(Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f), Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f)), Vector2.one * Random.Range(0.025f, 0.075f), 0f, new Color(0.55f, 0.78f, 1f, Random.Range(0.45f, 0.95f)), -18, PlaceholderSpriteFactory.Circle());
                star.isStatic = true;
            }
        }

        void CreateBoundary(Vector2 bounds)
        {
            var boundary = new GameObject("Stage Boundary");
            boundary.transform.SetParent(stageRoot.transform, false);
            var color = new Color(0.9f, 0.18f, 0.34f, 0.48f);
            var half = bounds * 0.5f;
            CreateSpriteObject("Top", boundary.transform, new Vector2(0f, half.y), new Vector2(bounds.x, 0.08f), 0f, color, 2, PlaceholderSpriteFactory.Square());
            CreateSpriteObject("Bottom", boundary.transform, new Vector2(0f, -half.y), new Vector2(bounds.x, 0.08f), 0f, color, 2, PlaceholderSpriteFactory.Square());
            CreateSpriteObject("Left", boundary.transform, new Vector2(-half.x, 0f), new Vector2(0.08f, bounds.y), 0f, color, 2, PlaceholderSpriteFactory.Square());
            CreateSpriteObject("Right", boundary.transform, new Vector2(half.x, 0f), new Vector2(0.08f, bounds.y), 0f, color, 2, PlaceholderSpriteFactory.Square());
        }

        void CreateGoal(Vector2 position, Vector2 size)
        {
            var goal = CreateSpriteObject("Goal Area", stageRoot.transform, position, size, 0f, new Color(0.3f, 1f, 0.42f, 0.45f), 3, PlaceholderSpriteFactory.Circle());
            goal.AddComponent<GoalArea>();
            var collider = goal.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }

        void CreateObstacle(StageObstacle obstacle)
        {
            switch (obstacle.kind)
            {
                case ObstacleKind.CircleAsteroid:
                    CreateCircleObstacle(obstacle);
                    break;
                case ObstacleKind.EllipseAsteroid:
                    CreateEllipseObstacle(obstacle);
                    break;
                case ObstacleKind.LongWall:
                    CreateWallObstacle(obstacle.position, obstacle.size, obstacle.rotation, "Long Wall");
                    break;
                case ObstacleKind.HollowRing:
                    CreateHollowRing(obstacle);
                    break;
                case ObstacleKind.MazeStructure:
                    CreateMaze(obstacle);
                    break;
            }
        }

        void CreateCircleObstacle(StageObstacle obstacle)
        {
            var asteroid = CreateSpriteObject("Circle Asteroid", stageRoot.transform, obstacle.position, obstacle.size, obstacle.rotation, obstacle.color, 5, PlaceholderSpriteFactory.Circle());
            asteroid.AddComponent<ObstacleMarker>();
            var collider = asteroid.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
        }

        void CreateEllipseObstacle(StageObstacle obstacle)
        {
            var asteroid = CreateSpriteObject("Ellipse Asteroid", stageRoot.transform, obstacle.position, obstacle.size, obstacle.rotation, new Color(0.5f, 0.55f, 0.68f, 1f), 5, PlaceholderSpriteFactory.Circle());
            asteroid.AddComponent<ObstacleMarker>();
            var collider = asteroid.AddComponent<PolygonCollider2D>();
            var points = new Vector2[28];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = i / (float)points.Length * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f);
            }

            collider.points = points;
        }

        void CreateWallObstacle(Vector2 position, Vector2 size, float rotation, string name)
        {
            var wall = CreateSpriteObject(name, stageRoot.transform, position, size, rotation, new Color(0.28f, 0.35f, 0.45f, 1f), 5, PlaceholderSpriteFactory.Square());
            wall.AddComponent<ObstacleMarker>();
            wall.AddComponent<BoxCollider2D>();
        }

        void CreateHollowRing(StageObstacle obstacle)
        {
            var root = new GameObject("Hollow Ring");
            root.transform.SetParent(stageRoot.transform, false);
            root.transform.position = obstacle.position;
            root.transform.rotation = Quaternion.Euler(0f, 0f, obstacle.rotation);

            var radius = obstacle.size.x * 0.5f;
            var thickness = 0.42f;
            var segments = 18;
            for (var i = 0; i < segments; i++)
            {
                var angle = i / (float)segments * Mathf.PI * 2f;
                var degrees = angle * Mathf.Rad2Deg;
                var localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var segment = CreateSpriteObject("Ring Segment", root.transform, localPosition, new Vector2(radius * 0.55f, thickness), degrees + 90f, new Color(0.36f, 0.42f, 0.55f, 1f), 5, PlaceholderSpriteFactory.Square());
                segment.AddComponent<ObstacleMarker>();
                segment.AddComponent<BoxCollider2D>();
            }
        }

        void CreateMaze(StageObstacle obstacle)
        {
            var root = new GameObject("Maze Structure");
            root.transform.SetParent(stageRoot.transform, false);
            root.transform.position = obstacle.position;
            root.transform.rotation = Quaternion.Euler(0f, 0f, obstacle.rotation);

            var width = obstacle.size.x;
            var height = obstacle.size.y;
            CreateWallObstacle(root.transform, new Vector2(-width * 0.22f, -height * 0.32f), new Vector2(width * 0.62f, 0.42f), 0f, "Maze Wall A");
            CreateWallObstacle(root.transform, new Vector2(width * 0.18f, 0f), new Vector2(0.42f, height * 0.72f), 0f, "Maze Wall B");
            CreateWallObstacle(root.transform, new Vector2(-width * 0.18f, height * 0.3f), new Vector2(width * 0.56f, 0.42f), 0f, "Maze Wall C");
            CreateWallObstacle(root.transform, new Vector2(width * 0.42f, height * 0.24f), new Vector2(0.42f, height * 0.48f), 0f, "Maze Wall D");
        }

        void CreateWallObstacle(Transform parent, Vector2 localPosition, Vector2 size, float rotation, string name)
        {
            var wall = CreateSpriteObject(name, parent, localPosition, size, rotation, new Color(0.28f, 0.35f, 0.45f, 1f), 5, PlaceholderSpriteFactory.Square());
            wall.AddComponent<ObstacleMarker>();
            wall.AddComponent<BoxCollider2D>();
        }

        GameObject CreateSpriteObject(string name, Transform parent, Vector2 position, Vector2 size, float rotation, Color color, int sortingOrder, Sprite sprite)
        {
            var gameObject = new GameObject(name, typeof(SpriteRenderer));
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        GameObject CreateMarker(Vector2 position)
        {
            var marker = new GameObject($"Marker {markers.Count + 1}");
            marker.transform.SetParent(markerRoot, false);
            marker.transform.position = position;

            var color = new Color(1f, 0.1f, 0.12f, 0.95f);
            CreateSpriteObject("Stroke A", marker.transform, Vector2.zero, new Vector2(1.25f, 0.12f), 45f, color, 12, PlaceholderSpriteFactory.Square());
            CreateSpriteObject("Stroke B", marker.transform, Vector2.zero, new Vector2(1.25f, 0.12f), -45f, color, 12, PlaceholderSpriteFactory.Square());
            return marker;
        }

        void ClearMarkers()
        {
            for (var i = 0; i < markers.Count; i++)
            {
                if (markers[i] != null)
                {
                    Destroy(markers[i]);
                }
            }

            markers.Clear();
            NotifyMarkerCountChanged();
        }

        void CleanupStage()
        {
            StopFailureReveal();
            player = null;
            loadedStageIndex = -1;
            markers.Clear();
            markerRoot = null;
            if (stageRoot != null)
            {
                stageRoot.SetActive(false);
                Destroy(stageRoot);
                stageRoot = null;
            }
        }

        void StopFailureReveal()
        {
            if (failureRevealCoroutine != null)
            {
                StopCoroutine(failureRevealCoroutine);
                failureRevealCoroutine = null;
            }
        }

        void NotifyMarkerCountChanged()
        {
            MarkerCountChanged?.Invoke(markers.Count, maxMarkerCount);
        }
    }
}
