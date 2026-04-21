using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public interface IEffectScheduler
    {
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);
        void TriggerConcurrent(IBlockEffect effect);
        UniTask TriggerAsync(IBlockEffect effect);
        void SuspendGridUpdates();
        void ResumeGridUpdates();
    }
}