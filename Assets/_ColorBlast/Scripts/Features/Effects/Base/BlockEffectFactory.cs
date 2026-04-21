using System;

namespace ColorBlast.Features
{
    /// <summary>
    /// Creates the correct IBlockEffect for any block.
    /// Handles both single blocks and combos.
    /// </summary>
    public class BlockEffectFactory
    {
        private readonly GridChecker gridChecker;
        private readonly GameConfig config;
        private readonly ComboDetector comboDetector;

        public BlockEffectFactory(GridChecker checker, GameConfig config, ComboDetector comboDetector)
        {
            gridChecker = checker;
            this.config = config;
            this.comboDetector = comboDetector;
        }

        /// <summary>
        /// Creates an effect for a player-tapped block.
        /// Checks for combos first; falls back to single effect.
        /// </summary>
        public IBlockEffect CreateEffectFromPlayerTap(Block block)
        {
            if (block is IActivatable && comboDetector.TryDetect(block, out var combo))
            {
                return CreateComboEffect(combo);
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
                BlockType.DiscoBall => new DiscoBallEffect(block),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IBlockEffect CreateComboEffect(ComboResult comboResult)
        {
            IBlockEffect comboEffect = comboResult.ComboType switch
            {
                ComboType.DiscoBallDiscoBall => new DiscoDiscoEffect(comboResult),
                ComboType.DiscoBallBomb => new DiscoBombEffect(comboResult, this),
                ComboType.DiscoBallRocket => new DiscoRocketEffect(comboResult, this),
                ComboType.BombBomb => new BombBombEffect(comboResult, this),
                ComboType.BombRocket => new BombRocketEffect(comboResult, this),
                ComboType.RocketRocket => new RocketRocketEffect(comboResult, this),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new ComboEffectWrapper(comboEffect, comboResult, config.MergeDuration);
        }
    }
}