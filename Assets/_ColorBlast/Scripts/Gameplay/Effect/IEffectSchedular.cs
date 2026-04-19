using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public interface IEffectSchedular
    {
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);
        void TriggerConcurrent(IBlockEffect effect);
        UniTask TriggerAsync(IBlockEffect effect);
        void SuspendGridUpdates();
        void ResumeGridUpdates();
    }
}