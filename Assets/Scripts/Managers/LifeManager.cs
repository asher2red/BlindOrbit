using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class LifeManager : MonoBehaviour
    {
        [SerializeField, Min(1)] int startingLives = 5;

        public event System.Action<int> LivesChanged;
        public int Lives { get; private set; }
        public int StartingLives => startingLives;

        public void ResetRun()
        {
            Lives = startingLives;
            LivesChanged?.Invoke(Lives);
        }

        public bool LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            LivesChanged?.Invoke(Lives);
            return Lives > 0;
        }
    }
}
