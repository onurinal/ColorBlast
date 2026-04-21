using System.Collections.Generic;

namespace ColorBlast.Features
{
    public readonly struct ComboResult
    {
        public Block Tapped { get; }
        public Block Best { get; }
        public Block Partner { get; }
        public HashSet<Block> AffectedSpecials { get; }
        public ComboType ComboType { get; }

        public ComboResult(Block tapped, Block best, Block partner, HashSet<Block> affectedSpecials,
            ComboType comboType)
        {
            Tapped = tapped;
            Best = best;
            Partner = partner;
            AffectedSpecials = affectedSpecials;
            ComboType = comboType;
        }
    }
}