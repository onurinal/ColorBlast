using ColorBlast.Manager;

namespace ColorBlast.Gameplay
{
    public class CubeBlock : Block, IInteractable, IMatchable, IRecolorable
    {
        public override BlockData BlockData { get; protected set; }
        private CubeBlockData CubeBlockData => (CubeBlockData)BlockData;

        public override void Initialize(int gridX, int gridY, BlockData data)
        {
            SetGridPosition(gridX, gridY);
            BlockData = data;
            RefreshVisual(0);
        }

        public void Interact()
        {
            // play particle and animations if block clicked
            EventManager.TriggerBlockInteracted(this);
        }

        public override void ClearBlock()
        {
            // play particle and animations if needed like death effect

            base.ClearBlock();
        }

        public bool CanMatchWith(IMatchable other)
        {
            if (other is CubeBlock cubeBlock && cubeBlock.BlockData == BlockData)
            {
                return true;
            }

            return false;
        }

        public override void UpdateIcon(int groupSize)
        {
            RefreshVisual(groupSize);
        }

        public void SetColor(BlockData newData)
        {
            BlockData = newData;
            RefreshVisual(0);
        }

        private void RefreshVisual(int groupSize)
        {
            if (BlockData == null)
            {
                return;
            }

            var sprite = CubeBlockData.GetVisual(groupSize);
            blockView.UpdateVisual(sprite);
        }
    }
}