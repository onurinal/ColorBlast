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

        private Block[,] blockGrid;
        private Vector2 blockSize;

        public bool IsProcessing { get; private set; } // prevent multiple clicks during refill,animations,falls

        // for affected blocks
        private List<Block> newSpawnBlocks;
        private List<Block> movedBlocks;

        public void Initialize(LevelProperties levelProperties)
        {
            this.levelProperties = levelProperties;

            CacheBlockSize();
            CreateGrid();

            newSpawnBlocks = new List<Block>();
            movedBlocks = new List<Block>();

            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, blockProperties);
            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);
            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties, blockProperties);
            gridShuffler = new GridShuffler();
            gridShuffler.Initialize(blockGrid, levelProperties, this);

            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, this, blockProperties);

            // Start LEVEL
            StartCoroutine(ResolveGridAtStart());
        }

        private void CacheBlockSize()
        {
            blockSize = blockProperties.GetBlockSpriteBoundSize();
        }

        private void CreateGrid()
        {
            blockGrid = new Block[levelProperties.RowCount, levelProperties.ColumnCount];
        }

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(row * (blockSize.x + blockProperties.SpacingX), col * (blockSize.y + blockProperties.SpacingY));
        }

        private IEnumerator ResolveGridAtStart()
        {
            yield return gridSpawner.CreateBlocksAtStart();
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                Debug.Log("DEADLOCK! NO MATCH FOUND");
                Debug.Log("SHUFFLE IN 2 SECONDS...");
                yield return gridShuffler.Shuffle();
            }
        }

        /// <summary>
        /// Destroy blocks, refill existing block to empty slots, spawn new blocks.
        /// After that it checks all grid
        /// </summary>
        private IEnumerator ResolveGrid(List<Block> blocks)
        {
            IsProcessing = true;

            // Destroy blocks with animation
            yield return DestroyBlocks(blocks);

            // existing blocks fall down
            yield return gridRefill.StartRefillToEmptySlots(movedBlocks);

            // Spawn new blocks to fill empty slots
            yield return gridSpawner.SpawnNewBlocks(newSpawnBlocks);

            gridChecker.CheckAffectedBlocks(blocks, newSpawnBlocks, movedBlocks);

            if (gridChecker.IsDeadlocked())
            {
                Debug.Log("DEADLOCK! NO MATCH FOUND");
                Debug.Log("SHUFFLE IN 2 SECONDS...");
                yield return gridShuffler.Shuffle();
            }

            IsProcessing = false;
        }

        public void OnBlockClicked(Block block)
        {
            var groups = gridChecker.GetGroup(block.GridX, block.GridY);

            if (groups.Count >= 2)
            {
                StartCoroutine(ResolveGrid(groups));
            }
        }

        private IEnumerator DestroyBlocks(List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                if (block == null)
                {
                    continue;
                }

                block.Destroy();
                blockGrid[block.GridX, block.GridY] = null;
            }

            yield return new WaitForSeconds(blockProperties.DestroyDuration);
        }
    }
}