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

        public async UniTask Execute(EffectExecutionContext context, IChainSchedular chainSchedular)
        {
            try
            {
                chainSchedular.SuspendGrid();

                var discoBall = (DiscoBlock)Tapped;

                if (discoBall.TargetCubeData == null)
                {
                    return;
                }

                var affected = CollectTargetColor(context, discoBall.TargetCubeData);

                var lines = new List<GameObject>();
                foreach (var block in affected)
                {
                    var newLine = new GameObject("DiscoLine");

                    var lineRenderer = newLine.AddComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.sortingLayerName = "Particle";
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.05f;
                    lineRenderer.SetPosition(0, discoBall.transform.position);
                    lineRenderer.SetPosition(1, block.transform.position);
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.startColor = Color.cyan;
                    lineRenderer.endColor = Color.white;
                    lines.Add(newLine);

                    AnimateLine(lineRenderer, discoBall.transform.position, block.transform.position).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(0.1f)); // 
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
            finally
            {
                chainSchedular.ResumeGrid();
            }
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