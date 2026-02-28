using UnityEngine;
using System.Collections.Generic;
using ColorBlast.Gameplay;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Manager
{
    /// <summary>
    /// Handles grid-related systems such as spawning, gravity, matching and shuffling
    /// </summary>
    public class GridManager : MonoBehaviour, IGridInteraction
    {
        [Header("Grid Configuration")]
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

        private List<Block> selectedGroup;

        private bool isBusy = false;

        public void Initialize(LevelProperties levelProperties, UIManager uiManager)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;

            CacheValues();
            CreateGrid();

            InitializeGridSystems(levelProperties);
            InitializeCamera(levelProperties);
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(row * (blockSize.x + blockProperties.SpacingX),
                col * (blockSize.y + blockProperties.SpacingY));
        }

        public async UniTaskVoid OnGameStart()
        {
            gridSpawner.CreateNewBlocksAtStart();
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDurationMs);
                await UniTask.Delay(blockProperties.ShuffleDurationMs,
                    cancellationToken: this.GetCancellationTokenOnDestroy());
                gridShuffler.Shuffle();
            }
        }

        public void OnBlockClicked(Block block)
        {
            if (isBusy)
            {
                return;
            }

            selectedGroup.Clear();
            gridChecker.GetGroup(block.GridX, block.GridY, selectedGroup);

            if (selectedGroup.Count < GameConstRules.MatchThreshold)
            {
                return;
            }

            ResolveGrid(selectedGroup).Forget();
            EventManager.TriggerOnMoveChanged();
        }

        private void CacheValues()
        {
            blockSize = blockProperties.GetBlockSpriteBoundSize();
        }

        private void CreateGrid()
        {
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            var maxCapacity = levelProperties.RowCount * levelProperties.ColumnCount;
            selectedGroup = new List<Block>(maxCapacity / 2);
        }

        private void InitializeGridSystems(LevelProperties levelProperties)
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, blockColorDatabase);
            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);
            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties);
            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this, blockColorDatabase);
        }

        private void InitializeCamera(LevelProperties levelProperties)
        {
            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, this, blockProperties);
        }

        private async UniTask ResolveGrid(List<Block> blocks)
        {
            isBusy = true;

            try
            {
                await ExecuteDestroyPhase(blocks);
                await ExecuteGravityPhase();
                await ExecuteSpawnPhase();

                gridChecker.CheckAllGrid();

                if (gridChecker.IsDeadlocked())
                    await ExecuteShufflePhase();
            }
            finally
            {
                isBusy = false;
            }
        }

        private async UniTask ExecuteDestroyPhase(List<Block> blocks)
        {
            DestroyBlocks(blocks);
            await UniTask.Delay(blockProperties.DestroyDurationMs,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        private async UniTask ExecuteGravityPhase()
        {
            var movedBlocks = new List<Block>();
            gridRefill.ApplyGravity(movedBlocks);
            await UniTask.Delay(blockProperties.SpawnDurationMs,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        private async UniTask ExecuteSpawnPhase()
        {
            var newSpawnBlocks = new List<Block>();
            gridSpawner.SpawnNewBlocks(newSpawnBlocks);
            await UniTask.Delay(blockProperties.MoveDurationMs,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        private async UniTask ExecuteShufflePhase()
        {
            uiManager.ShowShuffleUI(blockProperties.ShuffleDurationMs);
            await UniTask.Delay(blockProperties.ShuffleDurationMs,
                cancellationToken: this.GetCancellationTokenOnDestroy());
            gridShuffler.Shuffle();
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