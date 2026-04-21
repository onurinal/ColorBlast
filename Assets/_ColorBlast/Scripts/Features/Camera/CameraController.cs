using UnityEngine;

namespace ColorBlast.Features
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CameraSetup cameraSetup;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        public void Initialize(Vector2 gridCenterWorldPosition, Vector2 gridWorldSize)
        {
            UpdateCameraPosition(gridCenterWorldPosition);
            UpdateCameraOrthographicSize(gridWorldSize);
        }

        private void UpdateCameraPosition(Vector2 gridCenterWorldPosition)
        {
            transform.position =
                new Vector3(gridCenterWorldPosition.x, gridCenterWorldPosition.y, transform.position.z);
        }

        private void UpdateCameraOrthographicSize(Vector2 gridWorldSize)
        {
            var paddedWidth = gridWorldSize.x * (1f + cameraSetup.PaddingX * 2f);

            var sizeByWidth = paddedWidth / 2f / mainCamera.aspect;

            mainCamera.orthographicSize = Mathf.Max(sizeByWidth, cameraSetup.MinOrthographicSize);
        }
    }
}