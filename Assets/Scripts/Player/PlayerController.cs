using UnityEngine;

namespace ColorBlast.Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerInputHandler playerInputHandler;

        public void Initialize()
        {
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerInputHandler.Initialize(this);
        }


        public void HandleTap()
        {
        }
    }
}