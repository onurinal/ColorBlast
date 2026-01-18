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

        private WaitForSeconds destroyDelay;
        private WaitForSeconds moveDelay;
        private WaitForSeconds spawnDelayBetweenBlocks;
        private WaitForSeconds spawnDelay;
        private WaitForSeconds shuffleDelay;

        public bool IsBusy { get; private set; }

        private readonly List<Block> newSpawnBlocks = new List<Block>();
        private readonly List<Block> movedBlocks = new List<Block>();

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

            destroyDelay = new WaitForSeconds(blockProperties.DestroyDuration);
            moveDelay = new WaitForSeconds(blockProperties.MoveDuration);
            spawnDelayBetweenBlocks = new WaitForSeconds(blockProperties.SpawnDelayBetweenBlocks);
            spawnDelay = new WaitForSeconds(blockProperties.SpawnDuration);
            shuffleDelay = new WaitForSeconds(blockProperties.ShuffleDuration);
        }

        private void CreateGrid()
        {
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];
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
                yield return shuffleDelay;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        public void OnBlockClicked(Block block)
        {
            var groups = gridChecker.GetGroup(block.GridX, block.GridY);

            if (groups.Count >= LevelRule.MatchThreshold)
            {
                StartCoroutine(ResolveGrid(groups));
                EventManager.OnMoveChanged();
            }
        }

        /// <summary>
        /// Destroy blocks, refill existing block to empty slots, spawn new blocks.
        /// After that it checks all grid
        /// </summary>
        private IEnumerator ResolveGrid(List<Block> blocks)
        {
            IsBusy = true;

            // Destroy blocks with animation
            yield return DestroyBlocks(blocks);

            // existing blocks fall down
            movedBlocks.Clear();
            yield return gridRefill.ApplyGravity(movedBlocks, moveDelay);

            // Spawn new blocks to fill empty slots
            newSpawnBlocks.Clear();
            yield return gridSpawner.SpawnNewBlocks(newSpawnBlocks, spawnDelayBetweenBlocks, spawnDelay);

            gridChecker.CheckAffectedBlocks(blocks, newSpawnBlocks, movedBlocks);

            if (gridChecker.IsDeadlocked())
            {
                uiManager.PopUpShuffleUI(blockProperties.ShuffleDuration);
                yield return shuffleDelay;
                gridShuffler.Shuffle();
            }

            IsBusy = false;
        }

        private IEnumerator DestroyBlocks(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != null)
                {
                    blocks[i].Destroy();
                    blockGrid[blocks[i].GridX, blocks[i].GridY] = null;
                }
            }

            yield return destroyDelay;
        }
    }
}