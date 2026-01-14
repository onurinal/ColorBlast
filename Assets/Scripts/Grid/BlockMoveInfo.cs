using ColorBlast.Blocks;
using UnityEngine;

namespace ColorBlast.Grid
{
    /// <summary>
    /// Tracks a block's old position before it moved
    /// used to check neighbors at the old position
    /// </summary>
    public struct BlockMoveInfo
    {
        public Block Block;
        public Vector2Int OldPosition;
        public Vector2Int NewPosition;

        public BlockMoveInfo(Block block, Vector2Int oldPosition, Vector2Int newPosition)
        {
            Block = block;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }
}