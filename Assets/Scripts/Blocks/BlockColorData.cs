using UnityEngine;

namespace ColorBlast.Blocks
{
    [CreateAssetMenu(fileName = "BlockColorData", menuName = "ColorBlast/Block Color Data")]
    public class BlockColorData : ScriptableObject
    {
        [SerializeField] private BlockColorType colorType;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private Sprite firstIcon; // A tier
        [SerializeField] private Sprite secondIcon; // B tier
        [SerializeField] private Sprite thirdIcon; // C tier

        public BlockColorType ColorType => colorType;

        public Sprite GetSprite(BlockIconType type)
        {
            switch (type)
            {
                case BlockIconType.Default: return defaultIcon;
                case BlockIconType.FirstIcon: return firstIcon;
                case BlockIconType.SecondIcon: return secondIcon;
                case BlockIconType.ThirdIcon: return thirdIcon;
                default: return defaultIcon;
            }
        }
    }
}