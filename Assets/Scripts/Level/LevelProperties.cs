using UnityEngine;

namespace ColorBlast.Level
{
    [CreateAssetMenu(fileName = "LevelProperties", menuName = "ColorBlast/Level Properties")]
    public class LevelProperties : ScriptableObject
    {
        [SerializeField, Range(1, 6)] private int colorCount;
        [SerializeField, Range(2, 10)] private int rowCount;
        [SerializeField] [Range(2, 10)] private int columnCount;

        [SerializeField] private int firstIconThreshold;
        [SerializeField] private int secondIconThreshold;
        [SerializeField] private int thirdIconThreshold;

        public int ColorCount => colorCount;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;

        public int FirstIconThreshold => firstIconThreshold;
        public int SecondIconThreshold => secondIconThreshold;
        public int ThirdIconThreshold => thirdIconThreshold;

        private void OnValidate()
        {
            firstIconThreshold = Mathf.Max(1, firstIconThreshold);

            if (secondIconThreshold <= FirstIconThreshold)
            {
                secondIconThreshold = FirstIconThreshold + 1;
            }

            if (thirdIconThreshold <= SecondIconThreshold)
            {
                thirdIconThreshold = SecondIconThreshold + 1;
            }
        }
    }
}