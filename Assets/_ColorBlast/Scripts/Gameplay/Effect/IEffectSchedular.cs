using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public interface IEffectSchedular
    {
        bool IsTriggered(Block block);
        void MarkTriggered(Block block);

        /// <summary>
        /// Fire-and-forget concurrent chain.
        /// Use for: rockets/bombs triggered inside a blast radius.
        /// </summary>
        void TriggerConcurrent(IBlockEffect effect);

        /// <summary>
        /// Awaits this effect's impact AND the subsequent grid settle before returning.
        /// Use for: DiscoBomb / DiscoRocket sequential chains.
        /// </summary>
        UniTask TriggerSequential(IBlockEffect effect); // sequential, awaitable
    }
}