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

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            // it could be improved when get more levels
            levelManager.LoadLevel(0);

            if (levelManager.CurrentLevel != null)
            {
                objectPoolManager.InitializePool(levelManager.CurrentLevel);
                gridManager.Initialize(levelManager.CurrentLevel);
                playerController.Initialize(gridManager);

                StartCoroutine(gridManager.OnGameStart());
            }
        }
    }
}