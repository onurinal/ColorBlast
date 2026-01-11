using UnityEngine;

namespace ColorBlast.Level
{
    [CreateAssetMenu(fileName = "LevelProperties", menuName = "ColorBlast/Level Properties")]
    public class LevelProperties : ScriptableObject
    {
        [SerializeField, Range(1, 6)] private int colorCount;
        [SerializeField] private int rowCount;
        [SerializeField] private int columnCount;

        [SerializeField] private int firstIconThreshold;
        [SerializeField] private int secondIconThreshold;
        [SerializeField] private int thirdIconThreshold;

        public int ColorCount => colorCount;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;

        public int FirstIconThreshold => firstIconThreshold;
        public int SecondIconThreshold => secondIconThreshold;
        public int ThirdIconThreshold => thirdIconThreshold;
    }
}