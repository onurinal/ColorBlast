using System;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    [Serializable]
    public class DiscoColorEntry
    {
        [SerializeField] private CubeBlockData cubeData;
        [SerializeField] private Color color;

        public CubeBlockData CubeData => cubeData;
        public Color Color => color;
    }
}