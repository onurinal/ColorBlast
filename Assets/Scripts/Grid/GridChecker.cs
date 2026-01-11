using System.Collections.Generic;
using ColorBlast.Blocks;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Grid
{
    public class GridChecker : MonoBehaviour
    {
        private LevelProperties levelProperties;
        private Block[,] blockGrid;

        private Dictionary<Vector2Int, int> matchedBlockToGroupID;
        private Dictionary<int, HashSet<Vector2Int>> matchedGroupToBlocks;
        private int nextMatchGroupID = 0;

        private enum Direction
        {
            Horizontal,
            Vertical
        }

        public void Initialize(Block[,] blockGrid, LevelProperties levelProperties)
        {
            this.blockGrid = blockGrid;
            this.levelProperties = levelProperties;
            InitializeLists();

            CheckAllGrid();
            CheckMatchedList();
        }

        private void InitializeLists()
        {
            var maxBlocks = levelProperties.RowCount * levelProperties.ColumnCount;
            matchedBlockToGroupID = new Dictionary<Vector2Int, int>(maxBlocks);
            matchedGroupToBlocks = new Dictionary<int, HashSet<Vector2Int>>(maxBlocks / LevelRule.MatchThreshold);
        }

        private void CheckAllGrid()
        {
            for (int row = 0; row < levelProperties.RowCount; row++)
            {
                for (int col = 0; col < levelProperties.ColumnCount; col++)
                {
                    CheckWithDirection(row, col, Direction.Vertical);
                    CheckWithDirection(row, col, Direction.Horizontal);
                }
            }
        }

        private void CheckWithDirection(int row, int col, Direction direction)
        {
            var block1 = new Vector2Int(row, col);
            var block2 = direction == Direction.Vertical ? new Vector2Int(row, col + 1) : new Vector2Int(row + 1, col);

            if (block2.x >= levelProperties.RowCount || block2.y >= levelProperties.ColumnCount) return;

            TryMatch(block1, block2);
        }

        private void TryMatch(Vector2Int block1, Vector2Int block2)
        {
            if (blockGrid[block1.x, block1.y].ColorType != blockGrid[block2.x, block2.y].ColorType) return;

            bool isBlock1Matched = IsBlockMatched(block1);
            bool isBlock2Matched = IsBlockMatched(block2);

            if (!isBlock1Matched && !isBlock2Matched)
            {
                var newGroupID = nextMatchGroupID++;
                AddMatchGroup(block1, newGroupID);
                AddMatchGroup(block2, newGroupID);
            }
            else if (isBlock1Matched && !isBlock2Matched)
            {
                var groupID = matchedBlockToGroupID[block1];
                AddMatchGroup(block2, groupID);
            }
            else if (!isBlock1Matched)
            {
                var groupID = matchedBlockToGroupID[block2];
                AddMatchGroup(block1, groupID);
            }
        }

        private bool IsBlockMatched(Vector2Int block)
        {
            return matchedBlockToGroupID.ContainsKey(block);
        }

        private void AddMatchGroup(Vector2Int block, int groupID)
        {
            matchedBlockToGroupID[block] = groupID;

            if (!matchedGroupToBlocks.ContainsKey(groupID))
            {
                matchedGroupToBlocks.Add(groupID, new HashSet<Vector2Int>());
            }

            matchedGroupToBlocks[groupID].Add(block);
        }

        private void CheckMatchedList()
        {
            Debug.Log($"Toplam {matchedBlockToGroupID.Count} blok eşleşti");
            foreach (var item in matchedBlockToGroupID)
            {
                var block = item.Key;
                var id = item.Value;
                Debug.Log($"Block: {block}, Group: {id}");
            }
        }
    }
}