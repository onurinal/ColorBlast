using System.Collections.Generic;

namespace ColorBlast.Features
{
    /// <summary>
    /// Shared operations used across multiple effect classes.
    /// Keeps individual Execute() methods readable and focused on intent.
    /// </summary>
    public static class EffectUtility
    {
        /// <summary>
        /// Removes all blocks from the combo set except the preserved one.
        /// Used by every combo effect to clean up adjacent specials before the main explosion.
        /// </summary>
        public static void RemoveComboSpecials(EffectExecutionContext context, HashSet<Block> affectedSpecials, params Block[] preserve)
        {
            var preserveSet = new HashSet<Block>(preserve);

            foreach (var block in affectedSpecials)
            {
                if (!preserveSet.Contains(block))
                {
                    context.TryRemoveBlock(block);
                }
            }
        }

        /// <summary>
        /// Triggers a special block as a concurrent chain reaction or destroys a normal block.
        /// </summary>
        public static void TriggerOrDestroy(Block block, EffectExecutionContext context, IEffectScheduler scheduler, BlockEffectFactory factory)
        {
            if (block == null)
            {
                return;
            }

            if (block is IActivatable && !scheduler.IsTriggered(block))
            {
                scheduler.MarkTriggered(block);
                scheduler.TriggerConcurrent(factory.CreateEffect(block));
            }
            else if (block is not IActivatable)
            {
                context.TryDestroyBlock(block);
            }
        }

        /// <summary>
        /// Collects all non-null blocks within a square radius around a center cell.
        /// </summary>
        public static HashSet<Block> CollectBlocksInRadius(EffectExecutionContext context, int centerRow, int centerCol, int radius)
        {
            var result = new HashSet<Block>();

            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                for (int col = centerCol - radius; col <= centerCol + radius; col++)
                {
                    if (context.IsInBounds(row, col) && context.Grid[row, col] != null)
                    {
                        result.Add(context.Grid[row, col]);
                    }
                }
            }

            return result;
        }
    }
}