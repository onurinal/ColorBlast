using UnityEngine;

namespace ColorBlast.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float padding;
        [SerializeField] private float minOrthographicSize = 10f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Initialize(Vector2 gridCenterWorldPosition, Vector2 gridWorldSize)
        {
            UpdateCameraPosition(gridCenterWorldPosition);
            // UpdateCameraOrthographicSize(gridWorldSize);
        }

        private void UpdateCameraPosition(Vector2 gridCenterWorldPosition)
        {
            transform.position =
                new Vector3(gridCenterWorldPosition.x, gridCenterWorldPosition.y, transform.position.z);
        }

        // private void UpdateCameraOrthographicSize(Vector2 gridWorldSize)
        // {
        //     var minWidthSize = (gridWorldSize.x + (padding * 2f)) / 2f / mainCamera.aspect;
        //     var minHeightSize = (gridWorldSize.y + (padding * 2f)) / 2f;
        //
        //     var targetSize = Mathf.Max(minWidthSize, minHeightSize);
        //     mainCamera.orthographicSize = Mathf.Max(targetSize, minOrthographicSize);
        // }
    }
}