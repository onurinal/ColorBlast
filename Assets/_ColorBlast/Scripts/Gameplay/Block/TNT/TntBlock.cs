using ColorBlast.Manager;

namespace ColorBlast.Gameplay.TNT
{
    public class TntBlock : Block, IInteractable, IActivatable
    {
        public override BlockData BlockData { get; protected set; }
        private TntBlockData TntBlockData => (TntBlockData)BlockData;

        public override void Initialize(int gridX, int gridY, BlockData blockData)
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