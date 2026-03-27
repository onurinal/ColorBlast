using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "ColorBlast/GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [SerializeField] private float cellUnitSize = 1f;

        [Header("Visual Settings")]
        [SerializeField] private float blockSizeX = 0.45f;
        [SerializeField] private float blockSizeY = 0.45f;

        [Header("Match Settings")]
        [SerializeField] private int matchThreshold = 2;

        [Header("Animation Settings")]
        [SerializeField] private int destroyDurationMs = 150;
        [SerializeField] private int fallDurationMs = 250;
        [SerializeField] private int spawnDurationMs = 150;
        [SerializeField] private int shuffleDurationMs = 2000;

        public float CellUnitSize => cellUnitSize;
        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public int MatchThreshold => matchThreshold;
        public int DestroyDurationMs => destroyDurationMs;
        public int FallDurationMs => fallDurationMs;
        public int SpawnDurationMs => spawnDurationMs;
        public int ShuffleDurationMs => shuffleDurationMs;
        public float ShuffleDurationSec => shuffleDurationMs / 1000f;
        public float FallDurationSec => fallDurationMs / 1000f;
        public float DestroyDurationSec => destroyDurationMs / 1000f;
    }
}