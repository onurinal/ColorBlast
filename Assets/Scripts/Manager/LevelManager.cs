using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelProperties[] levels;
        private int currentLevelIndex;

        public LevelProperties CurrentLevel
        {
            get
            {
                if (levels == null || levels.Length == 0)
                {
                    return null;
                }

                return levels[currentLevelIndex];
            }
        }

        public void LoadLevel(int levelIndex)
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("No levels defined in LevelManager!");
                return;
            }

            if (levelIndex < 0 || levelIndex >= levels.Length)
            {
                Debug.LogError($"Level index {levelIndex} is out of range.");
                return;
            }

            currentLevelIndex = levelIndex;
        }
    }
}