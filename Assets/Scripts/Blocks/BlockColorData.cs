using UnityEngine;

namespace ColorBlast.Blocks
{
    /// <summary>
    /// Stores sprite data for a specific block color type
    /// Contains default icon and tier-based icons based on group size threshold
    /// </summary>
    [CreateAssetMenu(fileName = "BlockColorData", menuName = "ColorBlast/Block Color Data")]
    public class BlockColorData : ScriptableObject
    {
        [SerializeField] private BlockColorType colorType;

        [Header("Icon Sprites")]
        [Tooltip("Default icon - show up when group size < firstIcon threshold")]
        [SerializeField] private Sprite defaultIcon;
        [Tooltip("First icon - show up when group size > defaultIcon threshold")]
        [SerializeField] private Sprite firstIcon; // A tier
        [Tooltip("Second icon - show up when group size > firstIcon threshold")]
        [SerializeField] private Sprite secondIcon; // B tier
        [Tooltip("Third icon - show up when group size > secondIcon threshold")]
        [SerializeField] private Sprite thirdIcon; // C tier

        public BlockColorType ColorType => colorType;

        public Sprite GetSprite(BlockIconType type)
        {
            return type switch
            {
                BlockIconType.Default => defaultIcon,
                BlockIconType.FirstIcon => firstIcon,
                BlockIconType.SecondIcon => secondIcon,
                BlockIconType.ThirdIcon => thirdIcon,
                _ => defaultIcon
            };
        }
    }
}