using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles chain reactions, deduplication and post-effect grid operations.
    /// </summary>
    public class EffectPipeline : IChainSchedular
    {
        private readonly Stack<IBlockEffect> effectQueue = new();
        private readonly HashSet<Block> triggered = new();

        private GridRefill gridRefill;
        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private EffectExecutionContext effectExecutionContext;

        private int parallelEffectCount = 0;

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

            effectQueue.Push(effect);

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
            effectQueue.Push(effect);
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
                    var effect = effectQueue.Pop();

                    if (effect is IParallelEffect)
                    {
                        FireParallel(effect);
                    }
                    else
                    {
                        await effect.Execute(effectExecutionContext, this);
                        RunGravityAndRefill();
                    }
                }

                if (parallelEffectCount > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(effectExecutionContext.Config.DiscoBallAnimationDuration));
                    RunGravityAndRefill();
                }

                gridChecker.CheckAllGrid();
            }
            finally
            {
                IsProcessing = false;
                triggered.Clear();
                parallelEffectCount = 0;
            }
        }

        private void FireParallel(IBlockEffect effect)
        {
            parallelEffectCount++;
            RunParallelAsync(effect).Forget();
        }

        private async UniTask RunParallelAsync(IBlockEffect effect)
        {
            try
            {
                await effect.Execute(effectExecutionContext, this);
            }
            finally
            {
                parallelEffectCount--;
            }
        }

        private void RunGravityAndRefill()
        {
            if (parallelEffectCount == 0)
            {
                gridRefill.ApplyGravity();
                gridSpawner.SpawnNewCubeBlocks();
            }
        }
    }
}