using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using ColorBlast.Gameplay;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Manager
{
    /// <summary>
    /// Handles grid-related systems such as spawning, gravity, matching and shuffling
    /// </summary>
    public class GridManager : MonoBehaviour, IGridInteraction
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

        private LevelProperties levelProperties;
        private UIManager uiManager;

        private Block[,] blockGrid;
        private bool isBusy = false;

        public void Initialize(LevelProperties levelProperties, UIManager uiManager)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            InitializeSystems();
            InitializeCamera();
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(
                row * GameplayConfig.CellUnitSize,
                col * GameplayConfig.CellUnitSize);
        }

        public async UniTaskVoid OnGameStart()
        {
            isBusy = true;
            var ct = this.GetCancellationTokenOnDestroy();
            try
            {
                gridSpawner.SpawnNewBlocks();
                gridChecker.CheckAllGrid();
                await CheckAndHandleDeadlock(ct);
            }
            finally
            {
                isBusy = false;
            }
        }

        public void OnBlockClicked(Block block)
        {
            if (isBusy)
            {
                return;
            }

            List<Block> group = gridChecker.GetGroupAt(block.GridX, block.GridY);

            if (group.Count < gameplayConfig.MatchThreshold)
            {
                return;
            }

            ResolveGrid(group).Forget();
            EventManager.TriggerOnMoveChanged();
        }

        private void InitializeSystems()
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, cubeColorDatabase);

            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties, gameplayConfig);

            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties);

            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this, gameplayConfig);
        }

        private void InitializeCamera()
        {
            Vector2 gridWorldSize = new Vector2(levelProperties.RowCount, levelProperties.ColumnCount);

            Vector2 bottomLeft = GetCellWorldPosition(0, 0);
            Vector2 topRight = GetCellWorldPosition(levelProperties.RowCount - 1, levelProperties.ColumnCount - 1);
            Vector2 gridCenterWorldPosition = (bottomLeft + topRight) / 2f;

            cameraController.Initialize(gridCenterWorldPosition, gridWorldSize);
        }

        private async UniTask ResolveGrid(List<Block> blocks)
        {
            isBusy = true;
            var ct = this.GetCancellationTokenOnDestroy();

            try
            {
                await ExecuteClearPhase(blocks, ct);
                await ExecuteGravityPhase(ct);
                await ExecuteRefillPhase(ct);

                gridChecker.CheckAllGrid();
                await CheckAndHandleDeadlock(ct);
            }
            finally
            {
                isBusy = false;
            }
        }

        private async UniTask ExecuteClearPhase(List<Block> blocks, CancellationToken ct)
        {
            ClearBlocks(blocks);
            await UniTask.Delay(gameplayConfig.DestroyDurationMs, cancellationToken: ct);
        }

        private async UniTask ExecuteGravityPhase(CancellationToken ct)
        {
            gridRefill.ApplyGravity();
            await UniTask.Delay(gameplayConfig.FallDurationMs, cancellationToken: ct);
        }

        private async UniTask ExecuteRefillPhase(CancellationToken ct)
        {
            gridSpawner.SpawnNewBlocks();
            await UniTask.Delay(gameplayConfig.SpawnDurationMs, cancellationToken: ct);
        }

        private async UniTask CheckAndHandleDeadlock(CancellationToken ct)
        {
            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(gameplayConfig.ShuffleDurationSec);
                await UniTask.Delay(gameplayConfig.ShuffleDurationMs, cancellationToken: ct);
                gridShuffler.Shuffle();
            }
        }

        private void ClearBlocks(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                {
                    continue;
                }

                blockGrid[blocks[i].GridX, blocks[i].GridY] = null;
                blocks[i].ClearBlock();
            }
        }
    }
}