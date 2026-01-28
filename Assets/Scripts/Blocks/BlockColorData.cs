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
        [Header("Icon Sprites")]
        [Tooltip("Default icon - show up when group size < rocketIcon threshold")]
        [SerializeField] private Sprite defaultIcon;
        [Tooltip("First icon - show up when group size > defaultIcon threshold")]
        [SerializeField] private Sprite rocketIcon;
        [Tooltip("Second icon - show up when group size > rocketIcon threshold")]
        [SerializeField] private Sprite tntIcon;
        [Tooltip("Third icon - show up when group size > tntIcon threshold")]
        [SerializeField] private Sprite rainbowIcon;

        public Sprite GetSprite(BlockIconType type)
        {
            return type switch
            {
                BlockIconType.Default => defaultIcon,
                BlockIconType.RocketIcon => rocketIcon,
                BlockIconType.TntIcon => tntIcon,
                BlockIconType.RainbowIcon => rainbowIcon,
                _ => defaultIcon
            };
        }
    }
}