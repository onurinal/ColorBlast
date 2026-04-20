using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Represents a single executable effect in the pipeline.
    /// </summary>
    public interface IBlockEffect
    {
        Block Source { get; }
        UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular);
    }
}