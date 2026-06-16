using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Gameplay
{
    public static class StageLibrary
    {
        public static IReadOnlyList<StageRuntimeData> CreatePrototypeStages()
        {
            return new[]
            {
                new StageRuntimeData(
                    "01 - First Orbit",
                    new Vector2(40f, 84f),
                    new Vector2(0f, -37f),
                    Vector2.zero,
                    new Vector2(4.5f, 35.5f),
                    new Vector2(3.4f, 3.4f),
                    38f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(-6.5f, -23f), new Vector2(7.5f, 7.5f)),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(7.2f, -9f), new Vector2(8.6f, 8.6f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-4.8f, 8f), new Vector2(20f, 1.2f), -8f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(8.5f, 23f), new Vector2(5.2f, 13f), 14f),
                    }),
                new StageRuntimeData(
                    "02 - False Gap",
                    new Vector2(46f, 96f),
                    new Vector2(-3.5f, -42f),
                    Vector2.zero,
                    new Vector2(8.5f, 40.5f),
                    new Vector2(3.2f, 3.2f),
                    43f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(-9f, -28f), new Vector2(5.5f, 18f), 16f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(9.5f, -16f), new Vector2(5.0f, 21f), -18f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-3f, 3f), new Vector2(27f, 1.25f), 7f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(11f, 18f), new Vector2(8.5f, 8.5f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-9f, 29f), new Vector2(18f, 1.15f), -16f),
                    }),
                new StageRuntimeData(
                    "03 - Hollow Memory",
                    new Vector2(52f, 110f),
                    new Vector2(-5f, -48f),
                    Vector2.zero,
                    new Vector2(-9.5f, 46.5f),
                    new Vector2(3.0f, 3.0f),
                    48f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.HollowRing, new Vector2(0f, -27f), new Vector2(15f, 15f)),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(12f, -6f), new Vector2(5f, 22f), -10f),
                        new StageObstacle(ObstacleKind.MazeStructure, new Vector2(1f, 16f), new Vector2(25f, 17f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-11f, 34f), new Vector2(20f, 1.15f), 13f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(12f, 39f), new Vector2(4.8f, 14f), -8f),
                    }),
                new StageRuntimeData(
                    "04 - Edge Tax",
                    new Vector2(54f, 116f),
                    new Vector2(2f, -50f),
                    Vector2.zero,
                    new Vector2(6f, 49f),
                    new Vector2(3.0f, 3.0f),
                    50f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-18f, -34f), new Vector2(12f, 1.2f), 28f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(18f, -31f), new Vector2(13f, 1.2f), -26f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(-4f, -19f), new Vector2(10f, 10f)),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(10f, -2f), new Vector2(5.2f, 24f), 8f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-9f, 19f), new Vector2(24f, 1.2f), -12f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(16f, 35f), new Vector2(9f, 9f)),
                    }),
                new StageRuntimeData(
                    "05 - Needle Drift",
                    new Vector2(58f, 122f),
                    new Vector2(-4f, -53f),
                    Vector2.zero,
                    new Vector2(-7f, 52f),
                    new Vector2(2.8f, 2.8f),
                    52f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(-13f, -38f), new Vector2(4.8f, 20f), -18f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(13f, -26f), new Vector2(4.8f, 22f), 18f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(0f, -7f), new Vector2(30f, 1.1f), 0f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(-15f, 9f), new Vector2(8.5f, 8.5f)),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(11f, 24f), new Vector2(5.2f, 22f), -14f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-12f, 42f), new Vector2(20f, 1.1f), 17f),
                    }),
                new StageRuntimeData(
                    "06 - Broken Ring",
                    new Vector2(60f, 128f),
                    new Vector2(5f, -56f),
                    Vector2.zero,
                    new Vector2(8f, 55f),
                    new Vector2(2.8f, 2.8f),
                    54f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.HollowRing, new Vector2(0f, -31f), new Vector2(18f, 18f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-20f, -9f), new Vector2(14f, 1.15f), 34f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(20f, -5f), new Vector2(15f, 1.15f), -30f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(-7f, 16f), new Vector2(5f, 24f), 10f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(15f, 31f), new Vector2(10f, 10f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-8f, 46f), new Vector2(25f, 1.1f), -8f),
                    }),
                new StageRuntimeData(
                    "07 - Survey Marks",
                    new Vector2(64f, 134f),
                    new Vector2(-7f, -58f),
                    Vector2.zero,
                    new Vector2(-12f, 58f),
                    new Vector2(2.6f, 2.6f),
                    56f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.MazeStructure, new Vector2(0f, -34f), new Vector2(25f, 18f), 8f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(18f, -13f), new Vector2(9f, 9f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-18f, 4f), new Vector2(19f, 1.1f), 24f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(7f, 19f), new Vector2(5.2f, 27f), -10f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-1f, 39f), new Vector2(32f, 1.15f), 6f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(19f, 53f), new Vector2(9f, 9f)),
                    }),
                new StageRuntimeData(
                    "08 - Hollow Choice",
                    new Vector2(66f, 140f),
                    new Vector2(0f, -61f),
                    Vector2.zero,
                    new Vector2(13f, 61f),
                    new Vector2(2.5f, 2.5f),
                    58f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(-18f, -45f), new Vector2(5f, 24f), 20f),
                        new StageObstacle(ObstacleKind.HollowRing, new Vector2(9f, -23f), new Vector2(19f, 19f)),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-11f, -2f), new Vector2(28f, 1.1f), -15f),
                        new StageObstacle(ObstacleKind.MazeStructure, new Vector2(3f, 22f), new Vector2(28f, 18f), -6f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(-18f, 45f), new Vector2(4.8f, 21f), -16f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(18f, 51f), new Vector2(18f, 1.1f), 22f),
                    }),
                new StageRuntimeData(
                    "09 - Quiet Corridor",
                    new Vector2(70f, 148f),
                    new Vector2(-8f, -65f),
                    Vector2.zero,
                    new Vector2(-5f, 65f),
                    new Vector2(2.4f, 2.4f),
                    60f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-24f, -48f), new Vector2(17f, 1.1f), 32f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(24f, -44f), new Vector2(18f, 1.1f), -32f),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(0f, -25f), new Vector2(5.4f, 28f), 0f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(-18f, 0f), new Vector2(10f, 10f)),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(18f, 11f), new Vector2(10f, 10f)),
                        new StageObstacle(ObstacleKind.MazeStructure, new Vector2(-1f, 35f), new Vector2(30f, 20f), 4f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(20f, 61f), new Vector2(20f, 1.1f), -20f),
                    }),
                new StageRuntimeData(
                    "10 - Blind Orbit",
                    new Vector2(74f, 156f),
                    new Vector2(0f, -69f),
                    Vector2.zero,
                    new Vector2(0f, 69f),
                    new Vector2(2.4f, 2.4f),
                    62f,
                    new List<StageObstacle>
                    {
                        new StageObstacle(ObstacleKind.HollowRing, new Vector2(-11f, -48f), new Vector2(18f, 18f)),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(17f, -32f), new Vector2(5f, 28f), -18f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-18f, -11f), new Vector2(24f, 1.1f), 20f),
                        new StageObstacle(ObstacleKind.MazeStructure, new Vector2(6f, 10f), new Vector2(32f, 22f), -9f),
                        new StageObstacle(ObstacleKind.CircleAsteroid, new Vector2(-20f, 31f), new Vector2(11f, 11f)),
                        new StageObstacle(ObstacleKind.EllipseAsteroid, new Vector2(17f, 45f), new Vector2(5f, 26f), 16f),
                        new StageObstacle(ObstacleKind.LongWall, new Vector2(-9f, 62f), new Vector2(30f, 1.1f), -6f),
                    })
            };
        }
    }

    public sealed class StageRuntimeData
    {
        public readonly string StageName;
        public readonly Vector2 BoundsSize;
        public readonly Vector2 PlayerStart;
        public readonly Vector2 InitialVelocity;
        public readonly Vector2 GoalPosition;
        public readonly Vector2 GoalSize;
        public readonly float Fuel;
        public readonly IReadOnlyList<StageObstacle> Obstacles;

        public StageRuntimeData(string stageName, Vector2 boundsSize, Vector2 playerStart, Vector2 initialVelocity, Vector2 goalPosition, Vector2 goalSize, float fuel, IReadOnlyList<StageObstacle> obstacles)
        {
            StageName = stageName;
            BoundsSize = boundsSize;
            PlayerStart = playerStart;
            InitialVelocity = initialVelocity;
            GoalPosition = goalPosition;
            GoalSize = goalSize;
            Fuel = fuel;
            Obstacles = obstacles;
        }
    }
}
