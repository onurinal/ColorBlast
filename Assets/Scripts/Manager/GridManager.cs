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
        private LevelProperties levelProperties;

        private Block[,] blockGrid;
        private Vector2 blockSize;

        public bool IsProcessing { get; private set; } // prevent multiple clicks during refill,animations,falls

        // for affected blocks
        private List<Block> newSpawnBlocks;
        private List<Block> movedBlocks;

        // for shuffle
        private List<Block> allBlocks;
        private List<Vector2Int> allBlockPositions;

        public void Initialize(LevelProperties levelProperties)
        {
            this.levelProperties = levelProperties;

            CacheBlockSize();
            CreateGrid();

            newSpawnBlocks = new List<Block>();
            movedBlocks = new List<Block>();
            allBlocks = new List<Block>();
            allBlockPositions = new List<Vector2Int>();

            gridSpawner = new GridSpawner();
            gridSpawner.Initialize(blockGrid, this, levelProperties, blockProperties);
            gridChecker = new GridChecker();
            gridChecker.Initialize(blockGrid, levelProperties);
            gridRefill = new GridRefill();
            gridRefill.Initialize(blockGrid, this, levelProperties, blockProperties);

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
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                Debug.Log("DEADLOCK! NO MATCH FOUND");
                Debug.Log("SHUFFLE IN 3 SECONDS...");
                yield return Shuffle();
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
                Debug.Log("SHUFFLE IN 3 SECONDS...");
                yield return Shuffle();
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

        #region Shuffle System

        private IEnumerator Shuffle()
        {
            allBlocks.Clear();
            allBlockPositions.Clear();

            yield return new WaitForSeconds(3f);

            // get all block and positions first
            AddAllBlockAndPositionToShuffle();

            // shuffle with Fisher-Yates
            ProcessShuffle();

            // assign new positions
            for (int i = 0; i < allBlockPositions.Count; i++)
            {
                var pos = allBlockPositions[i];
                allBlocks[i].SetGridPosition(pos.x, pos.y); // set new indices
                blockGrid[pos.x, pos.y] = allBlocks[i]; // update block position in block grid
                blockGrid[pos.x, pos.y].MoveTo(GetCellWorldPosition(pos.x, pos.y));
            }
        }

        private void AddAllBlockAndPositionToShuffle()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    if (blockGrid[row, col] != null)
                    {
                        allBlocks.Add(blockGrid[row, col]);
                        allBlockPositions.Add(new Vector2Int(row, col));
                    }
                }
            }
        }

        private void ProcessShuffle()
        {
            for (int i = allBlocks.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                var tempBlock = allBlocks[i];
                allBlocks[i] = allBlocks[j];
                allBlocks[j] = tempBlock;
            }
        }

        #endregion
    }
}