using ColorBlast.Blocks;
using ColorBlast.Grid;
using ColorBlast.Level;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelProperties levelProperties;

        [SerializeField] private GridSpawner gridSpawner;
        [SerializeField] private GridChecker gridChecker;

        [SerializeField] private BlockProperties blockProperties;

        [SerializeField] private CameraController cameraController;

        public void Initialize()
        {
            gridSpawner.Initialize(blockProperties, levelProperties, gridChecker);
            cameraController.Initialize(levelProperties.RowCount, levelProperties.ColumnCount, gridSpawner, blockProperties);
        }
    }
}