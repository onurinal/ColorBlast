using System.Collections;
using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class GridManager : MonoBehaviour
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

        public bool IsBusy { get; private set; }

        private List<Block> newSpawnBlocks;
        private List<Block> movedBlocks;
        private List<Block> clickedGroup;

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

            yield return gridSpawner.CreateNewBlocksAtStart();
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                uiManager.PopUpShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        public void OnBlockClicked(Block block)
        {
            clickedGroup.Clear();
            gridChecker.GetGroup(block.GridX, block.GridY, clickedGroup);

            if (clickedGroup.Count >= LevelProperties.MatchThreshold)
            {
                StartCoroutine(ResolveGrid(clickedGroup));
                EventManager.OnMoveChanged();
            }
        }

        private IEnumerator ResolveGrid(List<Block> blocks)
        {
            IsBusy = true;

            // Destroy blocks with animation
            DestroyBlocks(blocks);
            yield return blockProperties.DestroyWait;

            // existing blocks fall down
            movedBlocks.Clear();
            gridRefill.ApplyGravity(movedBlocks);
            yield return blockProperties.MoveWait;

            // Spawn new blocks to fill empty slots
            newSpawnBlocks.Clear();
            gridSpawner.SpawnNewBlocks(newSpawnBlocks);
            yield return blockProperties.SpawnWait;

            gridChecker.CheckAffectedBlocks(newSpawnBlocks, movedBlocks);

            if (gridChecker.IsDeadlocked())
            {
                uiManager.PopUpShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        private void DestroyBlocks(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != null)
                {
                    blocks[i].PlayDestroyAnimation();
                    blockGrid[blocks[i].GridX, blocks[i].GridY] = null;
                }
            }
        }
    }
}