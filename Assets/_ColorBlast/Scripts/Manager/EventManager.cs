using System;
using ColorBlast.Features;

namespace ColorBlast.Manager
{
    public static class EventManager
    {
        public static event Action OnMoveChanged;
        public static event Action<Block> OnBlockInteract;

        public static void TriggerMoveChanged()
        {
            OnMoveChanged?.Invoke();
        }

        public static void TriggerBlockInteracted(Block block)
        {
            OnBlockInteract?.Invoke(block);
        }
    }
}