using System;

namespace ColorBlast.Manager
{
    public static class EventManager
    {
        public static event Action OnMoveChanged;

        public static void TriggerOnMoveChanged()
        {
            OnMoveChanged?.Invoke();
        }
    }
}