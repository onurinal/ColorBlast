using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBlock : Block, IActivatable, IInteractable
    {
        [SerializeField] private ParticleSystem particle;

        private DiscoBlockData DiscoBlockData => (DiscoBlockData)BlockData;
        public override BlockData BlockData { get; protected set; }

        public BlockData TargetCubeData { get; private set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
        }

        public void SetTargetCubeData(BlockData targetCubeData)
        {
            if (targetCubeData == null)
            {
                Debug.LogWarning($"DiscoBlock cannot set targetCubeData or sprite");
            }

            TargetCubeData = targetCubeData;

            if (targetCubeData is CubeBlockData cubeData)
            {
                blockView.SetColor(DiscoBlockData.GetColorForCube(cubeData));
            }
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }

        public void PlayParticle()
        {
            if (particle != null)
            {
                particle.Play();
            }
        }

        public override void OnDespawn()
        {
            if (particle != null)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            base.OnDespawn();
        }

        public void SetViewColor(Color color) => blockView.SetColor(color);
        public void ResetViewColor() => blockView.ResetColor();
    }
}