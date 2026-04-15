using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    public interface IBlockParticleService
    {
        void PlayDestroyEffect(Block block);
        UniTask PlayBombEffect(Block block);
    }
}