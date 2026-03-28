using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class DiscoBlock : Block, IActivatable, IInteractable
    {
        public override BlockData BlockData { get; protected set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData, Sprite sprite = null)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;

            if (sprite != null)
            {
                blockView.UpdateVisual(sprite);
            }
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }
    }
}