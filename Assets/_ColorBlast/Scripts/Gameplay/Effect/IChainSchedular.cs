using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public interface IChainSchedular
    {
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);
        void TriggerEffect(IBlockEffect effect);
        UniTask TriggerEffectAsync(IBlockEffect effect); // sequential, awaitable
        UniTask ForceGridUpdate(); // force gravity + refill
        void SuspendGrid();
        void ResumeGrid();
    }
}