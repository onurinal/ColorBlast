using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public interface IBlockParticleService
    {
        void PlayDestroyEffect(Block block);
        UniTask PlayBombEffect(Block block);
    }
}