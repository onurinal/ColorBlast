using UnityEngine;
using ColorBlast.Player;

namespace ColorBlast.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private ObjectPoolManager objectPoolManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LevelManager levelManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            levelManager.Initialize();
            objectPoolManager.InitializePool(levelManager.LevelProperties);
            gridManager.Initialize(levelManager.LevelProperties);
            playerController.Initialize(gridManager.GridChecker);
        }
    }
}