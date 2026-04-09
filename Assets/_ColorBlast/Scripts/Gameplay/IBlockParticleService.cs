using ColorBlast.Gameplay;

namespace ColorBlast._ColorBlast.Scripts.Gameplay
{
    public interface IBlockParticleService
    {
        void PlayDestroyEffect(Block block);
        void PlayRocketActivation(Block block);
        void PlayBombEffect(Block block);
    }
}