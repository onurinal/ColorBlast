namespace ColorBlast.Gameplay
{
    public interface IChainSchedular
    {
        void EnqueueChained(IBlockEffect effect);
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);
        void RunGravityAndRefill();
    }
}