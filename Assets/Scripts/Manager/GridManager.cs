using System.Collections;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    /// <summary>
    /// Handles grid-related systems such as spawning, gravity, matching and shuffling
    /// </summary>
    public class GridManager : MonoBehaviour, IGridInteraction
    {
        [Header("Grid Configuration")]
        [SerializeField] private BlockProperties blockProperties;

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

        private List<Block> newSpawnBlocks;
        private List<Block> movedBlocks;
        private List<Block> clickedGroup;

        public bool IsBusy { get; private set; }

        public void Initialize(LevelProperties levelProperties, UIManager uiManager)
        {
            this.levelProperties = levelProperties;
            this.uiManager = uiManager;

            CacheValues();
            CreateGrid();

            InitializeGridSystems(levelProperties);
            InitializeCamera(levelProperties);
        }

        private void CacheValues()
        {
            blockSize = blockProperties.GetBlockSpriteBoundSize();
        }

        private void CreateGrid()
        {
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];

            var maxCapacity = levelProperties.RowCount * levelProperties.ColumnCount;
            newSpawnBlocks = new List<Block>(maxCapacity);
            movedBlocks = new List<Block>(maxCapacity);
            clickedGroup = new List<Block>(maxCapacity / 2);
        }

        private void InitializeGridSystems(LevelProperties levelProperties)
        {
            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties);
            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);
            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties);
            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this);
        }

        private void InitializeCamera(LevelProperties levelProperties)
        {
            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, this, blockProperties);
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(row * (blockSize.x + blockProperties.SpacingX), col * (blockSize.y + blockProperties.SpacingY));
        }

        public IEnumerator OnGameStart()
        {
            IsBusy = true;

            gridSpawner.CreateNewBlocksAtStart();
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        public void OnBlockClicked(Block block)
        {
            clickedGroup.Clear();
            gridChecker.GetGroup(block.GridX, block.GridY, clickedGroup);

            if (clickedGroup.Count < GameRule.MatchThreshold)
            {
                return;
            }

            StartCoroutine(ResolveGrid(clickedGroup));
            EventManager.TriggerOnMoveChanged();
        }

        private IEnumerator ResolveGrid(List<Block> blocks)
        {
            IsBusy = true;

            DestroyBlocks(blocks);
            yield return blockProperties.DestroyWait;

            movedBlocks.Clear();
            gridRefill.ApplyGravity(movedBlocks);
            yield return blockProperties.MoveWait;

            newSpawnBlocks.Clear();
            yield return gridSpawner.SpawnNewBlocks(newSpawnBlocks, blockProperties.SpawnWait);

            gridChecker.CheckAffectedBlocks(newSpawnBlocks, movedBlocks);

            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        private void DestroyBlocks(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                {
                    continue;
                }

                blocks[i].PlayDestroyAnimation();
                blockGrid[blocks[i].GridX, blocks[i].GridY] = null;
            }
        }
    }
}