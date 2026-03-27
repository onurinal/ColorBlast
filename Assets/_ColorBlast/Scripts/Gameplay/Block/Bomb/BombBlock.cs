using ColorBlast.Manager;

namespace ColorBlast.Gameplay
{
    public class BombBlock : Block, IInteractable, IActivatable
    {
        public override BlockData BlockData { get; protected set; }
        private BombBlockData BombBlockData => (BombBlockData)BlockData;

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