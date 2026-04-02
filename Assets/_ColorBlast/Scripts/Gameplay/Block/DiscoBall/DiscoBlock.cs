using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBlock : Block, IActivatable, IInteractable
    {
        public override BlockData BlockData { get; protected set; }

        public BlockData TargetCubeData { get; private set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
        }

        public void SetTargetCubeData(BlockData targetCubeData, Sprite sprite)
        {
            if (targetCubeData == null || sprite == null)
            {
                Debug.LogWarning($"DiscoBlock cannot set targetCubeData or sprite");
            }

            TargetCubeData = targetCubeData;
            blockView.UpdateVisual(sprite);
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }
    }
}