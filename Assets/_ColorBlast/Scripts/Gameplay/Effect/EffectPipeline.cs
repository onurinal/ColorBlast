using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles chain reactions, deduplication and post-effect grid operations.
    /// </summary>
    public class EffectPipeline : IChainSchedular
    {
        private readonly Queue<IBlockEffect> effectQueue = new();
        private readonly HashSet<Block> triggered = new();

        private GridRefill gridRefill;
        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private EffectExecutionContext effectExecutionContext;

        public bool IsProcessing { get; private set; }

        public void Initialize(GridRefill gridRefill, GridSpawner gridSpawner, GridChecker gridChecker,
            EffectExecutionContext effectExecutionContext)
        {
            this.gridRefill = gridRefill;
            this.gridSpawner = gridSpawner;
            this.gridChecker = gridChecker;
            this.effectExecutionContext = effectExecutionContext;
        }

        public void EnqueueFromPlayer(IBlockEffect effect)
        {
            effectQueue.Clear();
            triggered.Clear();

            // MarkTriggered(effect.Source);
            effectQueue.Enqueue(effect);

            if (!IsProcessing)
            {
                ProcessExecute().Forget();
            }
        }

        public void EnqueueChained(IBlockEffect effect)
        {
            if (effect.Tapped != null && IsTriggered(effect.Tapped))
            {
                return;
            }

            MarkTriggered(effect.Tapped);
            effectQueue.Enqueue(effect);
        }

        public bool IsTriggered(Block block)
        {
            return block != null && triggered.Contains(block);
        }

        public void MarkTriggered(Block block)
        {
            if (block != null)
            {
                triggered.Add(block);
            }
        }

        // ── Execution loop ─
        private async UniTaskVoid ProcessExecute()
        {
            IsProcessing = true;

            try
            {
                while (effectQueue.Count > 0)
                {
                    var effect = effectQueue.Dequeue();
                    await effect.Execute(effectExecutionContext, this);
                }

                RunGravityAndRefill();
                gridChecker.CheckAllGrid();
            }
            finally
            {
                IsProcessing = false;
                triggered.Clear();
            }
        }

        private void RunGravityAndRefill()
        {
            gridRefill.ApplyGravity();
            gridSpawner.SpawnNewCubeBlocks();
        }
    }
}