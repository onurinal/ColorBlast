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
        [SerializeField] private float destroyDuration = 0.15f;
        [SerializeField] private float fallDuration = 0.25f;
        [SerializeField] private float spawnDuration = 0.15f;
        [SerializeField] private float shuffleDuration = 2f;
        [SerializeField] private float spawnDurationBetweenSpecials = 0.05f;
        [SerializeField] private float discoBallAnimationDuration = 3f;

        public float CellUnitSize => cellUnitSize;
        public float BlockSizeX => blockSizeX;
        public float BlockSizeY => blockSizeY;
        public int MatchThreshold => matchThreshold;
        public float DestroyDuration => destroyDuration;
        public float FallDuration => fallDuration;
        public float SpawnDuration => spawnDuration;
        public float ShuffleDuration => shuffleDuration;
        public float SpawnDurationBetweenSpecials => spawnDurationBetweenSpecials;
        public float DiscoBallAnimationDuration => discoBallAnimationDuration;
    }
}