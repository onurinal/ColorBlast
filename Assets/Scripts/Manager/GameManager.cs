using UnityEngine;
using ColorBlast.Player;

namespace ColorBlast.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private ObjectPoolManager objectPoolManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private UIManager uiManager;

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            levelManager.LoadLevel(0);

            if (levelManager.CurrentLevel == null)
            {
                return;
            }

            uiManager.Initialize();
            objectPoolManager.InitializePool(levelManager.CurrentLevel);
            gridManager.Initialize(levelManager.CurrentLevel, uiManager);
            playerController.Initialize(gridManager);

            StartCoroutine(gridManager.OnGameStart());
        }

        public void RestartLevel()
        {
            SceneLoader.LoadSameScene();
        }
    }
}