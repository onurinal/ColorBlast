namespace ColorBlast.Gameplay
{
    public interface IBlockParticleService
    {
        void PlayDestroyEffect(Block block);
        void PlayBombEffect(Block block);
    }
}