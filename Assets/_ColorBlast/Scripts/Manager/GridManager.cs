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
        [SerializeField] private MatchRulesConfig matchRulesConfig;
        [SerializeField] private BlockProperties blockProperties;
        [SerializeField] private BlockColorDatabase blockColorDatabase;

        [Header("References")]
        [SerializeField] private CameraController cameraController;

        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private GridRefill gridRefill;
        private GridShuffler gridShuffler;

        private LevelProperties levelProperties;
        private UIManager uiManager;

        private Block[,] blockGrid;
        private Vector2 blockSize;
        private bool isBusy = false;

        public void Initialize(LevelProperties levelProperties, UIManager uiManager)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;
            blockSize = blockProperties.BlockSpriteBoundSize;
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            InitializeSystems();
            InitializeCamera();
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(
                row * (blockSize.x + blockProperties.SpacingX),
                col * (blockSize.y + blockProperties.SpacingY));
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

            if (group.Count < matchRulesConfig.MatchThreshold)
            {
                return;
            }

            ResolveGrid(group).Forget();
            EventManager.TriggerOnMoveChanged();
        }

        private void InitializeSystems()
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, blockColorDatabase);

            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties, matchRulesConfig);

            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties);

            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this, blockColorDatabase, matchRulesConfig);
        }

        private void InitializeCamera()
        {
            var gridWidth = levelProperties.RowCount * blockProperties.BlockSpriteBoundSize.x;
            var gridHeight = levelProperties.ColumnCount * blockProperties.BlockSpriteBoundSize.y;
            Vector2 gridWorldSize = new Vector2(gridWidth, gridHeight);

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
                await ExecuteDestroyPhase(blocks, ct);
                await ExecuteGravityPhase(ct);
                await ExecuteSpawnPhase(ct);

                gridChecker.CheckAllGrid();
                await CheckAndHandleDeadlock(ct);
            }
            finally
            {
                isBusy = false;
            }
        }

        private async UniTask ExecuteDestroyPhase(List<Block> blocks, CancellationToken ct)
        {
            DestroyBlocks(blocks);
            await UniTask.Delay(blockProperties.DestroyDurationMs, cancellationToken: ct);
        }

        private async UniTask ExecuteGravityPhase(CancellationToken ct)
        {
            gridRefill.ApplyGravity();
            await UniTask.Delay(blockProperties.FallDurationMs, cancellationToken: ct);
        }

        private async UniTask ExecuteSpawnPhase(CancellationToken ct)
        {
            gridSpawner.SpawnNewBlocks();
            await UniTask.Delay(blockProperties.SpawnDurationMs, cancellationToken: ct);
        }

        private async UniTask CheckAndHandleDeadlock(CancellationToken ct)
        {
            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDurationSec);
                await UniTask.Delay(blockProperties.ShuffleDurationMs, cancellationToken: ct);
                gridShuffler.Shuffle();
            }
        }

        private void DestroyBlocks(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                {
                    continue;
                }

                blockGrid[blocks[i].GridX, blocks[i].GridY] = null;
                blocks[i].HandleDestroy();
            }
        }
    }
}