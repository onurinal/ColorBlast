using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlast.Gameplay
{
    public class DiscoBallEffect : IBlockEffect
    {
        public Block Tapped { get; }
        private readonly BlockEffectFactory effectFactory;

        public DiscoBallEffect(Block source, BlockEffectFactory effectFactory)
        {
            Tapped = source;
            this.effectFactory = effectFactory;
        }

        public async UniTask Execute(EffectExecutionContext context, IEffectSchedular effectSchedular)
        {
            var discoBall = (DiscoBlock)Tapped;

            if (discoBall.TargetCubeData == null)
            {
                return;
            }

            var affected = CollectTargetColor(context, discoBall.TargetCubeData);

            var lines = new List<GameObject>();
            foreach (var block in affected)
            {
                var line = CreateLine(discoBall.transform.position, block.transform.position);
                lines.Add(line);
                AnimateLine(line.GetComponent<LineRenderer>(), discoBall.transform.position, block.transform.position).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }

            await UniTask.Delay(TimeSpan.FromSeconds(context.Config.DiscoBallAnimationDuration));

            foreach (var line in lines)
            {
                Object.Destroy(line);
            }

            affected.Add(discoBall);

            foreach (var block in affected)
            {
                context.TryDestroyBlock(block);
            }
        }

        private static GameObject CreateLine(Vector3 from, Vector3 to)
        {
            var go = new GameObject("DiscoLine");
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.sortingLayerName = "Particle";
            lr.startWidth = 0.1f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.cyan;
            lr.endColor = Color.white;
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
            return go;
        }

        private async UniTaskVoid AnimateLine(LineRenderer lr, Vector3 from, Vector3 to)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            lr.SetPosition(0, from);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                lr.SetPosition(1, Vector3.Lerp(from, to, elapsed / duration));
                await UniTask.Yield();
            }

            lr.SetPosition(1, to);
        }

        private List<Block> CollectTargetColor(EffectExecutionContext context, BlockData targetData)
        {
            var result = new List<Block>();

            for (int row = 0; row < context.LevelProperties.RowCount; row++)
            {
                for (int col = 0; col < context.LevelProperties.ColumnCount; col++)
                {
                    if (context.BlockGrid[row, col] != null && context.BlockGrid[row, col].BlockData == targetData)
                    {
                        result.Add(context.BlockGrid[row, col]);
                    }
                }
            }

            return result;
        }
    }
}