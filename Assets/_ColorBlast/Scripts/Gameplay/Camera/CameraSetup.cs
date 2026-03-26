using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "ColorBlast/CameraSettings")]
    public class CameraSetup : ScriptableObject
    {
        [Header("Padding (0 = no padding, 0.1 = 10% of grid size)")]
        [SerializeField, Range(0f, 0.5f)] private float paddingX = 0.1f;
        [SerializeField] private float minOrthographicSize = 10f;

        public float PaddingX => paddingX;
        public float MinOrthographicSize => minOrthographicSize;
    }
}