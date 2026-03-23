using ColorBlast.Core;

namespace ColorBlast.Gameplay.TNT
{
    public class TntBlock : Block, IInteractable, IActivatable
    {
        public override BlockData BlockData { get; protected set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
        }

        public void Interact() { }
    }
}