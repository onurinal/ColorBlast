using UnityEngine;

namespace ColorBlast.Gameplay
{
    [CreateAssetMenu(fileName = "MatchRulesConfig", menuName = "ColorBlast/MatchRulesConfig")]
    public sealed class MatchRulesConfig : ScriptableObject
    {
        [SerializeField] private int matchThreshold = 2;

        [SerializeField] private int rocketThreshold = 5;
        [SerializeField] private int tntThreshold = 6;
        [SerializeField] private int rainbowThreshold = 7;

        public int MatchThreshold => matchThreshold;
        public int RocketThreshold => rocketThreshold;
        public int TntThreshold => tntThreshold;
        public int RainbowThreshold => rainbowThreshold;

        private void OnValidate()
        {
            if (rocketThreshold < matchThreshold)
            {
                rocketThreshold = matchThreshold + 1;
            }

            if (tntThreshold < rocketThreshold)
            {
                tntThreshold = rocketThreshold + 1;
            }

            if (rainbowThreshold < tntThreshold)
            {
                rainbowThreshold = tntThreshold + 1;
            }
        }
    }
}