using System.Collections;
using BlindOrbit.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BlindOrbit.Managers
{
    public sealed class SceneFlowManager : MonoBehaviour
    {
        public const string LoadingSceneName = "Loading Scene";
        public const string TitleSceneName = "Title Scene";
        public const string GameSceneName = "Game Scene";

        GameManager gameManager;
        RankingManager rankingManager;
        GameObject screenRoot;
        bool titleInputArmed;
        bool howToPlayOpen;
        bool runEnding;

        public void Initialize(GameManager owner, RankingManager rankings)
        {
            gameManager = owner;
            rankingManager = rankings;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        void Update()
        {
            if (!titleInputArmed)
            {
                if (howToPlayOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CloseHowToPlay();
                }

                return;
            }

            if (AnyStartInputPressed())
            {
                titleInputArmed = false;
                LoadGameScene();
            }
        }

        public void CompleteRun(int finalScore)
        {
            if (runEnding)
            {
                return;
            }

            runEnding = true;
            Time.timeScale = 0f;
            DestroyScreen();
            var entryObject = new GameObject("Name Entry Flow", typeof(NameEntryUI));
            entryObject.transform.SetParent(transform, false);
            screenRoot = entryObject;
            entryObject.GetComponent<NameEntryUI>().Show(finalScore, playerName =>
            {
                var rank = rankingManager.SubmitScore(playerName, finalScore);
                ShowRankingScreen(rank, finalScore);
            });
        }

        public void LoadTitleScene()
        {
            Time.timeScale = 1f;
            runEnding = false;
            titleInputArmed = false;
            howToPlayOpen = false;
            gameManager.CleanupGameplaySystems();
            DestroyScreen();
            SceneManager.LoadScene(TitleSceneName);
        }

        void LoadGameScene()
        {
            DestroyScreen();
            SceneManager.LoadScene(GameSceneName);
        }

        void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DestroyScreen();
            titleInputArmed = false;
            howToPlayOpen = false;

            if (scene.name == GameSceneName)
            {
                Time.timeScale = 1f;
                runEnding = false;
                gameManager.StartNewRun();
                return;
            }

            gameManager.CleanupGameplaySystems();
            if (scene.name == TitleSceneName)
            {
                BuildTitleScreen();
                titleInputArmed = true;
                return;
            }

            BuildLoadingScreen();
            StartCoroutine(LoadTitleAfterInitialization());
        }

        IEnumerator LoadTitleAfterInitialization()
        {
            yield return new WaitForSecondsRealtime(0.45f);
            SceneManager.LoadScene(TitleSceneName);
        }

        void BuildLoadingScreen()
        {
            screenRoot = new GameObject("Loading Screen");
            screenRoot.transform.SetParent(transform, false);
            var canvas = ArcadeUIFactory.CreateCanvas("Loading Canvas", screenRoot.transform);
            var root = canvas.GetComponent<RectTransform>();

            var background = ArcadeUIFactory.CreatePanel("Background", root, new Color(0.01f, 0.015f, 0.03f, 1f));
            ArcadeUIFactory.Anchor(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = ArcadeUIFactory.CreateText("Logo", background, "BLIND ORBIT", 70, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.58f), Vector2.zero, Vector2.zero);

            var loading = ArcadeUIFactory.CreateText("Loading", background, "Loading...", 30, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(loading.rectTransform, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.48f), Vector2.zero, Vector2.zero);
        }

        void BuildTitleScreen()
        {
            screenRoot = new GameObject("Title Screen");
            screenRoot.transform.SetParent(transform, false);
            var canvas = ArcadeUIFactory.CreateCanvas("Title Canvas", screenRoot.transform);
            var root = canvas.GetComponent<RectTransform>();

            var background = ArcadeUIFactory.CreatePanel("Background", root, new Color(0.008f, 0.012f, 0.03f, 1f));
            ArcadeUIFactory.Anchor(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            for (var i = 0; i < 42; i++)
            {
                var star = ArcadeUIFactory.CreatePanel("Star", background, new Color(0.45f, 0.72f, 1f, Random.Range(0.35f, 0.9f)));
                var x = Random.Range(0.04f, 0.96f);
                var y = Random.Range(0.08f, 0.94f);
                ArcadeUIFactory.Anchor(star, new Vector2(x, y), new Vector2(x, y), new Vector2(-2f, -2f), new Vector2(2f, 2f));
            }

            var title = ArcadeUIFactory.CreateText("Logo", background, "BLIND\nORBIT", 84, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.76f), Vector2.zero, Vector2.zero);

            var subtitle = ArcadeUIFactory.CreateText("Subtitle", background, "INERTIA PUZZLE RUN", 28, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(subtitle.rectTransform, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.55f), Vector2.zero, Vector2.zero);

            var start = ArcadeUIFactory.CreateButton("Start Button", background, "START", StartFromTitle);
            ArcadeUIFactory.Anchor(start.GetComponent<RectTransform>(), new Vector2(0.2f, 0.29f), new Vector2(0.8f, 0.36f), Vector2.zero, Vector2.zero);

            var howToPlay = ArcadeUIFactory.CreateButton("How To Play Button", background, "HOW TO PLAY", OpenHowToPlay);
            ArcadeUIFactory.Anchor(howToPlay.GetComponent<RectTransform>(), new Vector2(0.2f, 0.20f), new Vector2(0.8f, 0.27f), Vector2.zero, Vector2.zero);

            var prompt = ArcadeUIFactory.CreateText("Prompt", background, "Press Enter / Space", 24, TextAnchor.MiddleCenter);
            prompt.color = new Color(0.52f, 0.7f, 0.78f, 1f);
            ArcadeUIFactory.Anchor(prompt.rectTransform, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.19f), Vector2.zero, Vector2.zero);
        }

        void StartFromTitle()
        {
            if (!titleInputArmed || howToPlayOpen)
            {
                return;
            }

            titleInputArmed = false;
            LoadGameScene();
        }

        void OpenHowToPlay()
        {
            if (howToPlayOpen || screenRoot == null)
            {
                return;
            }

            howToPlayOpen = true;
            titleInputArmed = false;
            var helpObject = new GameObject("How To Play", typeof(HowToPlayUI));
            helpObject.transform.SetParent(screenRoot.transform, false);
            helpObject.GetComponent<HowToPlayUI>().Show(CloseHowToPlay);
        }

        void CloseHowToPlay()
        {
            howToPlayOpen = false;
            titleInputArmed = true;
        }

        void ShowRankingScreen(int rank, int score)
        {
            DestroyScreen();
            var rankingObject = new GameObject("Ranking Flow", typeof(RankingUI));
            rankingObject.transform.SetParent(transform, false);
            screenRoot = rankingObject;
            rankingObject.GetComponent<RankingUI>().Show(rankingManager.Entries, rank, score, LoadTitleScene);
        }

        void DestroyScreen()
        {
            if (screenRoot != null)
            {
                Destroy(screenRoot);
                screenRoot = null;
            }
        }

        static bool AnyStartInputPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }

            return false;
        }
    }
}
