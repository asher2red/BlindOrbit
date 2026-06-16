using System.IO;
using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class SaveManager : MonoBehaviour
    {
        const string RankingFileName = "blind_orbit_rankings.json";

        string RankingPath => Path.Combine(Application.persistentDataPath, RankingFileName);

        public RankingSaveData LoadRankings()
        {
            if (!File.Exists(RankingPath))
            {
                return new RankingSaveData();
            }

            try
            {
                var json = File.ReadAllText(RankingPath);
                var data = JsonUtility.FromJson<RankingSaveData>(json);
                return data ?? new RankingSaveData();
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Failed to load rankings: {exception.Message}");
                return new RankingSaveData();
            }
        }

        public void SaveRankings(RankingSaveData data)
        {
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(RankingPath, json);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Failed to save rankings: {exception.Message}");
            }
        }
    }
}
