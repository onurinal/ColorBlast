namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Effects implementing this run in parallel with other block effects
    /// Gravity/spawn pausing is handled separately via IChainSchedular.
    /// </summary>
    public interface IParallelEffect { }
}