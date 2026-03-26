using UnityEngine;

namespace ColorBlast.Gameplay.TNT
{
    [CreateAssetMenu(fileName = "TntData", menuName = "ColorBlast/Gameplay/Block/TNT")]
    public class TntBlockData : BlockData
    {
        [SerializeField] private int radius = 3;
        public override BlockType BlockType => BlockType.Tnt;
    }
}