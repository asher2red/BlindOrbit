using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Managers
{
    public readonly struct StageScoreBreakdown
    {
        public readonly int stageClearScore;
        public readonly int fuelBonus;
        public readonly int livesBonus;
        public readonly int stageTotal;
        public readonly int currentScore;
        public readonly bool isFinalStage;

        public StageScoreBreakdown(int stageClearScore, int fuelBonus, int livesBonus, int currentScore, bool isFinalStage)
        {
            this.stageClearScore = stageClearScore;
            this.fuelBonus = fuelBonus;
            this.livesBonus = livesBonus;
            stageTotal = stageClearScore + fuelBonus + livesBonus;
            this.currentScore = currentScore;
            this.isFinalStage = isFinalStage;
        }
    }

    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField, Min(0)] int baseStageScore = 1000;
        [SerializeField, Min(0)] int fuelScoreMultiplier = 100;
        [SerializeField, Min(0)] int lifeScoreMultiplier = 5000;

        readonly HashSet<int> scoredStages = new HashSet<int>();

        public event System.Action<int> ScoreChanged;
        public event System.Action<StageScoreBreakdown> StageScoreCalculated;
        public event System.Action<int> FinalScoreCalculated;
        public int CurrentScore { get; private set; }
        public int ClearedStages { get; private set; }

        public void ResetRun()
        {
            CurrentScore = 0;
            ClearedStages = 0;
            scoredStages.Clear();
            ScoreChanged?.Invoke(CurrentScore);
        }

        public StageScoreBreakdown RecordStageClear(int stageIndex, float remainingFuel, int remainingLives, bool isFinalStage)
        {
            if (!scoredStages.Add(stageIndex))
            {
                var duplicateBreakdown = new StageScoreBreakdown(0, 0, 0, CurrentScore, isFinalStage);
                StageScoreCalculated?.Invoke(duplicateBreakdown);
                return duplicateBreakdown;
            }

            ClearedStages++;
            var stageClearScore = (stageIndex + 1) * baseStageScore;
            var fuelBonus = Mathf.RoundToInt(Mathf.Max(0f, remainingFuel) * fuelScoreMultiplier);
            var livesBonus = isFinalStage ? Mathf.Max(0, remainingLives) * lifeScoreMultiplier : 0;
            CurrentScore += stageClearScore + fuelBonus + livesBonus;
            var breakdown = new StageScoreBreakdown(stageClearScore, fuelBonus, livesBonus, CurrentScore, isFinalStage);
            ScoreChanged?.Invoke(CurrentScore);
            StageScoreCalculated?.Invoke(breakdown);
            if (isFinalStage)
            {
                FinalScoreCalculated?.Invoke(CurrentScore);
            }

            return breakdown;
        }

        public int CalculateFinalScore()
        {
            return Mathf.Max(0, CurrentScore);
        }
    }
}
