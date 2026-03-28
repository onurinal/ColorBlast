using System;
using ColorBlast.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ColorBlast.Gameplay
{
    public class RocketBlock : Block, IActivatable, IInteractable
    {
        public override BlockData BlockData { get; protected set; }
        public RocketBlockData RocketBlockData => (RocketBlockData)BlockData;

        public RocketDirection Direction { get; private set; }

        public override void Initialize(int gridX, int gridY, BlockData blockData, Sprite sprite = null,
            BlockData targetCubeData = null)
        {
            SetGridPosition(gridX, gridY);
            BlockData = blockData;
            Direction = SetRandomRocketDirection();
            blockView.UpdateVisual(RocketBlockData.GetSprite(Direction));
        }

        public void Interact()
        {
            EventManager.TriggerBlockInteracted(this);
        }

        private RocketDirection SetRandomRocketDirection()
        {
            Array values = Enum.GetValues(typeof(RocketDirection));
            var randomIndex = Random.Range(0, values.Length);
            var randomDirection = (RocketDirection)values.GetValue(randomIndex);
            return randomDirection;
        }
    }
}