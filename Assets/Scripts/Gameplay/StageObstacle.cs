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

        [Header("Device Tuning")]
        [Tooltip("Force, boost, or fuel drain rate depending on the obstacle kind.")]
        public float strength = 8f;
        [Tooltip("Warp destination or orbit center depending on the obstacle kind.")]
        public Vector2 targetPosition;
        [Tooltip("Degrees per second for rotating and orbiting obstacles.")]
        public float speed = 45f;
        [Tooltip("Seconds that both warp holes remain inactive after either one is used.")]
        public float cooldown = 1.5f;

        public StageObstacle(ObstacleKind kind, Vector2 position, Vector2 size, float rotation = 0f)
        {
            this.kind = kind;
            this.position = position;
            this.size = size;
            this.rotation = rotation;
        }


        public StageObstacle WithDevice(float effectStrength, Vector2 target, float motionSpeed = 45f)
        {
            strength = effectStrength;
            targetPosition = target;
            speed = motionSpeed;
            return this;
        }

        public StageObstacle WithCooldown(float seconds)
        {
            cooldown = Mathf.Max(0.1f, seconds);
            return this;
        }
    }
}
