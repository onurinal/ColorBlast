using UnityEngine;

namespace ColorBlast.Features
{
    [CreateAssetMenu(fileName = "BombBlockData", menuName = "ColorBlast/Gameplay/Block/Bomb")]
    public class BombBlockData : BlockData
    {
        [SerializeField] private int radius = 1;
        [SerializeField] private int doubleBombMultiplier = 3;

        public int Radius => radius;
        public int DoubleBombMultiplier => doubleBombMultiplier;
        public override BlockType BlockType => BlockType.Bomb;
    }
}