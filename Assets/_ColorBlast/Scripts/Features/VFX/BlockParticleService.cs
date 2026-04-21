using System;
using ColorBlast.Manager;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Features
{
    public class BlockParticleService : IBlockParticleService
    {
        public void PlayDestroyEffect(Block block)
        {
            switch (block)
            {
                case CubeBlock cubeBlock: PlayCubeEffect(cubeBlock); break;
            }
        }

        public async UniTask PlayBombEffect(Block block)
        {
            var vfx = ParticlePoolManager.Instance.GetParticle(block.BlockData);

            if (vfx is PoolableParticle particle)
            {
                var particleDuration = particle.GetParticleDuration();
                particle.transform.position = block.transform.position;
                await UniTask.Delay(TimeSpan.FromSeconds(particleDuration / 8f));

                ReturnToPool(BlockType.Bomb, particle, particleDuration).Forget();
            }
        }

        private void PlayCubeEffect(Block block)
        {
            var vfx = ParticlePoolManager.Instance.GetParticle(block.BlockData);

            if (vfx is PoolableParticle particle)
            {
                var particleDuration = particle.GetParticleDuration();
                particle.transform.position = block.transform.position;

                var cubeBlockData = (CubeBlockData)block.BlockData;
                particle.SetColor(cubeBlockData.ParticleColor);

                ReturnToPool(BlockType.Cube, particle, particleDuration).Forget();
            }
        }

        private async UniTask ReturnToPool(BlockType blockType, PoolableParticle particle, float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            ParticlePoolManager.Instance.ReturnParticle(blockType, particle);
        }
    }
}