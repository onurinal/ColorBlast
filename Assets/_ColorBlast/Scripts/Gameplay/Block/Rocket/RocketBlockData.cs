using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "RocketBlockData", menuName = "ColorBlast/Gameplay/Block/Rocket")]
    public class RocketBlockData : BlockData
    {
        [SerializeField] private Sprite horizontalRocketSprite;
        [SerializeField] private Sprite verticalRocketSprite;

        [SerializeField] private Sprite leftRocketSprite;
        [SerializeField] private Sprite upRocketSprite;
        [SerializeField] private Sprite downRocketSprite;
        [SerializeField] private Sprite rightRocketSprite;

        public override BlockType BlockType => BlockType.Rocket;

        public Sprite HorizontalRocketSprite => horizontalRocketSprite;
        public Sprite VerticalRocketSprite => verticalRocketSprite;
        public Sprite LeftRocketSprite => leftRocketSprite;
        public Sprite UpRocketSprite => upRocketSprite;
        public Sprite DownRocketSprite => downRocketSprite;
        public Sprite RightRocketSprite => rightRocketSprite;

        // public Sprite GetSprite(RocketDirection direction)
        // {
        //     return direction switch
        //     {
        //         RocketDirection.Horizontal => horizontalRocketSprite,
        //         RocketDirection.Vertical => verticalRocketSprite,
        //         _ => null
        //     };
        // }
    }
}