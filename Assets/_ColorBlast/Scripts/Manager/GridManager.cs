using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using ColorBlast.Gameplay;

namespace ColorBlast.Manager
{
    /// <summary>
    /// Handles grid-related systems such as spawning, gravity, matching and shuffling
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private CubeColorDatabase cubeColorDatabase;

        [Header("References")]
        [SerializeField] private CameraController cameraController;

        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private GridRefill gridRefill;
        private GridShuffler gridShuffler;
        private EffectPipeline effectPipeline;
        private BlockEffectFactory effectFactory;

        private LevelProperties levelProperties;
        private UIManager uiManager;

        private Block[,] blockGrid;

        public void Initialize(LevelProperties levelProperties, UIManager uiManager)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            InitializeSystems();
            InitializeCamera();
        }

        private void OnEnable()
        {
            EventManager.OnBlockInteract += HandleBlockInteract;
        }

        private void OnDisable()
        {
            EventManager.OnBlockInteract -= HandleBlockInteract;
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(row, col) * gameplayConfig.CellUnitSize;
        }

        public async UniTaskVoid OnGameStart()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            gridSpawner.SpawnNewCubeBlocks();
            gridChecker.CheckAllGrid();
            await CheckAndHandleDeadlock(cancellationToken);
        }

        private void HandleBlockInteract(Block block)
        {
            if (effectPipeline.IsProcessing)
            {
                return;
            }

            var effect = effectFactory.CreateFromPlayerTap(block);
            effectPipeline.EnqueueFromPlayer(effect);
        }

        private void InitializeSystems()
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, cubeColorDatabase, gameplayConfig);

            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties, gameplayConfig);

            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties, gameplayConfig);

            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this, gameplayConfig);

            var particleService = new BlockParticleService();
            var context = new EffectExecutionContext(blockGrid, levelProperties, gameplayConfig, gridSpawner,
                particleService);

            effectPipeline = new EffectPipeline();
            effectPipeline.Initialize(gridRefill, gridSpawner, gridChecker, context);

            var comboDetector = new ComboDetector();
            comboDetector.Initialize(blockGrid, levelProperties);

            effectFactory = new BlockEffectFactory(gridChecker, gameplayConfig, comboDetector);
        }

        private void InitializeCamera()
        {
            Vector2 gridWorldSize =
                new Vector2(levelProperties.RowCount, levelProperties.ColumnCount) * gameplayConfig.CellUnitSize;

            Vector2 bottomLeft = GetCellWorldPosition(0, 0);
            Vector2 topRight = GetCellWorldPosition(levelProperties.RowCount - 1, levelProperties.ColumnCount - 1);
            Vector2 gridCenterWorldPosition = (bottomLeft + topRight) / 2f;

            cameraController.Initialize(gridCenterWorldPosition, gridWorldSize);
        }

        // after adding some levels, you can make an event and when deadlock occurs then it should call this
        private async UniTask CheckAndHandleDeadlock(CancellationToken ct)
        {
            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(gameplayConfig.ShuffleDuration);
                await UniTask.Delay(TimeSpan.FromSeconds(gameplayConfig.ShuffleDuration), cancellationToken: ct);
                gridShuffler.Shuffle();
            }
        }
    }
}