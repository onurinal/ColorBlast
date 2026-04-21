using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    /// <summary>
    /// Represents a single executable effect in the pipeline.
    /// </summary>
    public interface IBlockEffect
    {
        Block Source { get; }
        UniTask Execute(EffectExecutionContext context, IEffectScheduler effectScheduler);
    }
}