using System;
using ColorBlast.Manager;
using Random = UnityEngine.Random;

namespace ColorBlast.Features
{
    public class RocketBlock : Block, IActivatable, IInteractable
    {
        public override BlockData BlockData { get; protected set; }
        public RocketBlockData RocketBlockData => (RocketBlockData)BlockData;

        public RocketDirection Direction { get; private set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }

        public void SetupDirection()
        {
            Array values = Enum.GetValues(typeof(RocketDirection));
            var randomIndex = Random.Range(0, values.Length);
            Direction = (RocketDirection)values.GetValue(randomIndex);
            SetupRocketVisual();
        }

        private void SetupRocketVisual()
        {
            if (Direction == RocketDirection.Horizontal)
            {
                blockView.UpdateVisual(RocketBlockData.HorizontalRocketSprite);
            }
            else
            {
                blockView.UpdateVisual(RocketBlockData.VerticalRocketSprite);
            }
        }
    }
}