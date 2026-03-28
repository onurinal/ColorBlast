using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "RocketBlockData", menuName = "ColorBlast/Gameplay/Block/Rocket")]
    public class RocketBlockData : BlockData
    {
        [SerializeField] private Sprite horizontalRocketSprite;
        [SerializeField] private Sprite verticalRocketSprite;
        public override BlockType BlockType => BlockType.Rocket;

        public Sprite GetSprite(RocketDirection direction)
        {
            return direction switch
            {
                RocketDirection.Horizontal => horizontalRocketSprite,
                RocketDirection.Vertical => verticalRocketSprite,
                _ => null
            };
        }
    }
}