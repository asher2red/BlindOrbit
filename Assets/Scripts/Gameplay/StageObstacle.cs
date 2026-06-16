using UnityEngine;

namespace BlindOrbit.Gameplay
{
    [System.Serializable]
    public sealed class StageObstacle
    {
        public ObstacleKind kind;
        public Vector2 position;
        public Vector2 size = Vector2.one;
        public float rotation;
        public Color color = new Color(0.42f, 0.50f, 0.62f, 1f);

        public StageObstacle(ObstacleKind kind, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.kind = kind;
            this.position = position;
            this.size = size;
            this.rotation = rotation;
        }
    }
}
