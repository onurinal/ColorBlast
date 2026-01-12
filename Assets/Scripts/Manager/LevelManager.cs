using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelProperties levelProperties;

        public LevelProperties LevelProperties => levelProperties;

        public void Initialize()
        {
            if (levelProperties == null)
            {
                Debug.LogError($"LevelProperties is null");
            }
        }
    }
}