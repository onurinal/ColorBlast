using UnityEngine;

namespace ColorBlast.Level
{
    [CreateAssetMenu(fileName = "LevelProperties", menuName = "ColorBlast/Level Properties")]
    public class LevelProperties : ScriptableObject
    {
        [SerializeField, Range(1, 6)] private int colorCount;
        [SerializeField, Range(2, 10)] private int rowCount;
        [SerializeField] [Range(2, 10)] private int columnCount;

        public int ColorCount => colorCount;
        public int RowCount => rowCount;
        public int ColumnCount => columnCount;
    }
}