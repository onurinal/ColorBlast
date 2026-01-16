using UnityEngine;
using ColorBlast.Player;

namespace ColorBlast.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private ObjectPoolManager objectPoolManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LevelManager levelManager;

        private bool isGameStarted = false;

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            levelManager.Initialize();
            objectPoolManager.InitializePool(levelManager.LevelProperties);
            gridManager.Initialize(levelManager.LevelProperties);
            playerController.Initialize(gridManager);

            StartGame();
        }

        private void StartGame()
        {
            if (isGameStarted)
            {
                return;
            }

            isGameStarted = true;
            StartCoroutine(gridManager.OnGameStart());
        }
    }
}