using System;
using UnityEngine;
using System.Threading;
using ColorBlast.Core;
using Cysharp.Threading.Tasks;
using ColorBlast.Features;

namespace ColorBlast.Manager
{
    /// <summary>
    /// Orchestrates grid systems: initialization, event handling, and game flow.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private GameConfig gameplayConfig;
        [SerializeField] private CubeColorDatabase cubeColorDatabase;

        [Header("References")]
        [SerializeField] private CameraController cameraController;

        private GridSystems gridSystems;
        private LevelProperties levelProperties;
        private UIManager uiManager;

        private Block[,] grid;

        public void Initialize(LevelProperties levelProperties, UIManager uiManager, IHapticService hapticService)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;
            grid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            gridSystems = new GridSystems();
            gridSystems.Build(grid, levelProperties, gameplayConfig, this, hapticService);
            gridSystems.Spawner.SetColorDatabase(cubeColorDatabase); //  // GridSpawner needs the color database
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

            gridSystems.Spawner.SpawnNewCubeBlocks();
            gridSystems.Checker.CheckAllGrid();
            await CheckAndHandleDeadlock(cancellationToken);
        }

        private void HandleBlockInteract(Block block)
        {
            if (gridSystems.EffectPipeline.IsProcessing)
            {
                return;
            }

            var effect = gridSystems.EffectFactory.CreateEffectFromPlayerTap(block);
            gridSystems.EffectPipeline.TriggerEffectFromPlayer(effect);
        }

        private async UniTask CheckAndHandleDeadlock(CancellationToken ct)
        {
            if (gridSystems.Checker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(gameplayConfig.ShuffleDuration);
                await UniTask.Delay(TimeSpan.FromSeconds(gameplayConfig.ShuffleDuration), cancellationToken: ct);
                gridSystems.Shuffler.Shuffle();
            }
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
    }
}