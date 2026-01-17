using System;

namespace ColorBlast.Manager
{
    public static class EventManager
    {
        public static event Action OnMove;

        public static void OnMoveChanged()
        {
            OnMove?.Invoke();
        }
    }
}