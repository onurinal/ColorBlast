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

        public Vector2 GetCellWorldPosition(int row, int col)
        {
            return new Vector2(row * (blockSize.x + blockProperties.SpacingX), col * (blockSize.y + blockProperties.SpacingY));
        }

        public IEnumerator OnGameStart()
        {
            gridSpawner.CreateNewBlocksAtStart();
            gridChecker.CheckAllGrid();

            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
                gridShuffler.Shuffle();
            }
        }

        public void OnBlockClicked(Block block)
        {
            selectedGroup.Clear();
            gridChecker.GetGroup(block.GridX, block.GridY, selectedGroup);

            if (selectedGroup.Count < GameConstRules.MatchThreshold)
            {
                return;
            }

            StartCoroutine(ResolveGrid(selectedGroup));
            EventManager.TriggerOnMoveChanged();
        }

        private IEnumerator ResolveGrid(List<Block> blocks)
        {
            var movedBlocks = new List<Block>();
            var newSpawnBlocks = new List<Block>();

            DestroyBlocks(blocks);
            gridRefill.ApplyGravity(movedBlocks);
            gridSpawner.SpawnNewBlocks(newSpawnBlocks);
            gridChecker.CheckAffectedBlocks(newSpawnBlocks, movedBlocks);

            PlayDestroyAnimation(blocks);
            yield return blockProperties.DestroyWait;
            gridRefill.PlayRefillAnimation(movedBlocks);
            yield return blockProperties.SpawnWait;
            gridSpawner.PlayNewSpawnBlocksAnimation(newSpawnBlocks);

            // StartCoroutine(PlayAnimations(blocks, movedBlocks, newSpawnBlocks));

            if (gridChecker.IsDeadlocked())
            {
                uiManager.ShowShuffleUI(blockProperties.ShuffleDuration);
                yield return blockProperties.ShuffleWait;
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
            }
        }

        private static void PlayDestroyAnimation(List<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != null)
                {
                    blocks[i].HandleDestroy();
                }
            }
        }

        // private IEnumerator PlayAnimations(List<Block> destroyBlocks, List<Block> movedBlocks, List<Block> newSpawnBlocks)
        // {
        //     PlayDestroyAnimation(destroyBlocks);
        //     yield return blockProperties.DestroyWait;
        //     gridRefill.PlayRefillAnimation(movedBlocks);
        //     yield return blockProperties.SpawnWait;
        //     gridSpawner.PlayNewSpawnBlocksAnimation(newSpawnBlocks);
        // }
    }
}