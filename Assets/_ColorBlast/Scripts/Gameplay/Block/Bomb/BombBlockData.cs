using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "BombBlockData", menuName = "ColorBlast/Gameplay/Block/Bomb")]
    public class BombBlockData : BlockData
    {
        [SerializeField] private int radius = 3;
        public override BlockType BlockType => BlockType.Bomb;

        public int Radius => radius;
    }
}