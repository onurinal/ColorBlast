using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Represents a single executable effect in the pipeline.
    /// </summary>
    public interface IBlockEffect
    {
        UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular);
    }
}