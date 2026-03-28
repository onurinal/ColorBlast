using ColorBlast.Manager;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class BombBlock : Block, IInteractable, IActivatable
    {
        public override BlockData BlockData { get; protected set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData, Sprite sprite = null)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }
    }
}