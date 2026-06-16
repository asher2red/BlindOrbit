using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Gameplay
{
    [CreateAssetMenu(menuName = "Blind Orbit/Stage Definition")]
    public sealed class StageDefinition : ScriptableObject
    {
        [Header("Stage")]
        public string stageName = "Orbit Lesson";
        public Vector2 boundsSize = new Vector2(40f, 84f);
        public Vector2 playerStart = new Vector2(0f, -37f);
        public Vector2 initialVelocity = Vector2.zero;
        public Vector2 goalPosition = new Vector2(4.5f, 35.5f);
        public Vector2 goalSize = new Vector2(3.4f, 3.4f);
        public float fuel = 40f;

        [Header("Handcrafted Obstacles")]
        public List<StageObstacle> obstacles = new List<StageObstacle>();
    }
}
