using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBlock : Block, IActivatable, IInteractable
    {
        public override BlockData BlockData { get; protected set; }

        public BlockData TargetCubeData { get; private set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData, Sprite sprite = null,
            BlockData targetCubeData = null)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;

            if (sprite != null)
            {
                blockView.UpdateVisual(sprite);
            }

            TargetCubeData = targetCubeData;
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }
    }
}