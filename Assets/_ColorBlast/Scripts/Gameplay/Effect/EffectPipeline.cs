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
        private readonly List<UniTask> discoBallTasks = new List<UniTask>();

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

                    if (effect is DiscoBallEffect)
                    {
                        FireDiscoBallEffect(effect);
                    }
                    else
                    {
                        await effect.Execute(effectExecutionContext, this);
                        RunGravityAndRefill();
                    }
                }

                // Await all parallel effects in this wave concurrently
                if (discoBallTasks.Count > 0)
                {
                    await UniTask.WhenAll(discoBallTasks);
                    discoBallTasks.Clear();
                    RunGravityAndRefill();
                }

                gridChecker.CheckAllGrid();
            }
            finally
            {
                IsProcessing = false;
                triggered.Clear();
            }
        }

        private void FireDiscoBallEffect(IBlockEffect effect)
        {
            var task = RunDiscoBallAsync(effect);
            discoBallTasks.Add(task);
        }

        private async UniTask RunDiscoBallAsync(IBlockEffect effect)
        {
            await effect.Execute(effectExecutionContext, this);
        }

        public void RunGravityAndRefill()
        {
            if (discoBallTasks.Count == 0)
            {
                gridRefill.ApplyGravity();
                gridSpawner.SpawnNewCubeBlocks();
            }
        }
    }
}