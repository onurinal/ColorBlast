using UnityEngine.SceneManagement;

namespace ColorBlast.Core
{
    public static class SceneLoader
    {
        public static void LoadSameScene()
        {
            var currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex);
        }
    }
}