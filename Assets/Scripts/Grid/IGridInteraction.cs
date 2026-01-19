using ColorBlast.Blocks;

namespace ColorBlast.Grid
{
    public interface IGridInteraction
    {
        bool IsBusy { get; }
        void OnBlockClicked(Block block);
    }
}