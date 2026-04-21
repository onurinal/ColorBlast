using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "DiscoBlockData", menuName = "ColorBlast/Gameplay/Block/DiscoBlock")]
    public class DiscoBlockData : BlockData
    {
        [SerializeField] private float lineAnimateDuration = 0.1f;
        [SerializeField] private DiscoColorEntry[] colorEntries;
        [SerializeField] private Color[] cubeColorList;

        public override BlockType BlockType => BlockType.DiscoBall;
        public float LineAnimateDuration => lineAnimateDuration;
        public Color[] CubeColorList => cubeColorList;

        public Color GetColorForCube(BlockData cubeBlockData)
        {
            foreach (var entry in colorEntries)
            {
                if (entry.CubeData == cubeBlockData)
                {
                    return entry.Color;
                }
            }

            return Color.white;
        }
    }
}