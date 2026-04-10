namespace ColorBlast.Gameplay
{
    public interface IBlockParticleService
    {
        void PlayDestroyEffect(Block block);
        void PlayRocketActivation(Block block);
        void PlayBombEffect(Block block);
    }
}