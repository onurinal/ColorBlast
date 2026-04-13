using UnityEngine;
using ColorBlast.Player;
using ColorBlast.Core;

namespace ColorBlast.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private BlockPoolManager blockPoolManager;
        [SerializeField] private ParticlePoolManager particlePoolManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private UIManager uiManager;

        private void Start()
        {
            Application.targetFrameRate = 60;

            StartGame();
        }

        public void RestartLevel()
        {
            SceneLoader.LoadSameScene();
        }

        private void StartGame()
        {
            levelManager.LoadLevel(0);

            if (levelManager.CurrentLevel == null)
            {
                return;
            }

            uiManager.Initialize();
            blockPoolManager.InitializePool(levelManager.CurrentLevel);
            particlePoolManager.InitializePool(levelManager.CurrentLevel);
            gridManager.Initialize(levelManager.CurrentLevel, uiManager);
            playerController.Initialize();

            gridManager.OnGameStart().Forget();
        }
    }
}