using System;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Creates the correct IBlockEffect for any block.
    /// Handles both single blocks and combos.
    /// </summary>
    public class BlockEffectFactory
    {
        private readonly GridChecker gridChecker;
        private readonly GameplayConfig config;
        private readonly ComboDetector comboDetector;

        public BlockEffectFactory(GridChecker checker, GameplayConfig config, ComboDetector comboDetector)
        {
            gridChecker = checker;
            this.config = config;
            this.comboDetector = comboDetector;
        }

        /// <summary>
        /// Creates an effect for a player-tapped block.
        /// Checks for combos first; falls back to single effect.
        /// </summary>
        public IBlockEffect CreateFromPlayerTap(Block block)
        {
            if (block is IActivatable)
            {
                var combo = comboDetector.TryDetect(block);
                if (combo.HasValue)
                {
                    var (partner, adjacentSpecials, comboType) = combo.Value;
                    return new ComboEffect(block, partner, adjacentSpecials, comboType, this);
                }
            }

            return CreateEffect(block);
        }

        /// <summary>
        /// Creates a single-block effect (used for chains too).
        /// </summary>
        public IBlockEffect CreateEffect(Block block)
        {
            return block.BlockType switch
            {
                BlockType.Cube => new CubeEffect(block, gridChecker, config),
                BlockType.Bomb => new BombEffect(block, this),
                BlockType.Rocket => new RocketEffect(block, this),
                BlockType.DiscoBall => new DiscoBallEffect(block, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}