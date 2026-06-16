using System.Collections.Generic;
using BlindOrbit.Gameplay;
using UnityEngine;

namespace BlindOrbit.Managers
{
    public sealed class LevelLoader : MonoBehaviour
    {
        IReadOnlyList<StageRuntimeData> stages;

        public int CurrentIndex { get; private set; }
        public int StageCount => stages?.Count ?? 0;

        public void Initialize()
        {
            stages = StageLibrary.CreatePrototypeStages();
            CurrentIndex = 0;
        }

        public StageRuntimeData CurrentStage => stages[Mathf.Clamp(CurrentIndex, 0, StageCount - 1)];

        public bool HasNextStage => CurrentIndex + 1 < StageCount;

        public StageRuntimeData LoadCurrent()
        {
            return CurrentStage;
        }

        public StageRuntimeData LoadNext()
        {
            if (HasNextStage)
            {
                CurrentIndex++;
            }

            return CurrentStage;
        }
    }
}
