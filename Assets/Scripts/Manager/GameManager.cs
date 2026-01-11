using UnityEngine;
using ColorBlast.Player;

namespace ColorBlast.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PlayerController playerController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            playerController.Initialize();
            levelManager.Initialize();
        }
    }
}