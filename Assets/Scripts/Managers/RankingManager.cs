using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Managers
{
    [Serializable]
    public sealed class RankingEntry
    {
        public string name;
        public int score;
        public string date;
    }

    [Serializable]
    public sealed class RankingSaveData
    {
        public List<RankingEntry> entries = new List<RankingEntry>();
    }

    public sealed class RankingManager : MonoBehaviour
    {
        const int MaxEntries = 100;

        readonly RankingSaveData data = new RankingSaveData();
        SaveManager saveManager;

        public IReadOnlyList<RankingEntry> Entries => data.entries;

        public void Initialize(SaveManager save)
        {
            saveManager = save;
            var loaded = saveManager.LoadRankings();
            data.entries.Clear();
            if (loaded.entries != null)
            {
                data.entries.AddRange(loaded.entries);
            }

            SortAndTrim();
        }

        public int SubmitScore(string playerName, int score)
        {
            var entry = new RankingEntry
            {
                name = SanitizeName(playerName),
                score = Mathf.Max(0, score),
                date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            data.entries.Add(entry);
            SortAndTrim();
            saveManager.SaveRankings(data);
            return data.entries.IndexOf(entry) + 1;
        }

        static string SanitizeName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return "AAA";
            }

            var result = string.Empty;
            for (var i = 0; i < playerName.Length && result.Length < 3; i++)
            {
                var character = char.ToUpperInvariant(playerName[i]);
                if (char.IsLetterOrDigit(character))
                {
                    result += character;
                }
            }

            return string.IsNullOrEmpty(result) ? "AAA" : result;
        }

        void SortAndTrim()
        {
            data.entries.Sort((left, right) => right.score.CompareTo(left.score));
            while (data.entries.Count > MaxEntries)
            {
                data.entries.RemoveAt(data.entries.Count - 1);
            }
        }
    }
}
