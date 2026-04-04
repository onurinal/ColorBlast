using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Represents a single executable effect in the pipeline.
    /// </summary>
    public interface IBlockEffect
    {
        Block Tapped { get; }
        UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular);
    }
}