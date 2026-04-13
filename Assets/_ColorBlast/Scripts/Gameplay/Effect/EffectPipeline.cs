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
        private readonly HashSet<Block> triggered = new();

        private GridRefill gridRefill;
        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private EffectExecutionContext effectExecutionContext;

        private int suspendCounter = 0;
        private int activeEffectCount = 0;
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
            triggered.Clear();

            if (!IsProcessing)
            {
                ProcessExecute(effect).Forget();
            }
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
        private async UniTaskVoid ProcessExecute(IBlockEffect effect)
        {
            IsProcessing = true;

            try
            {
                await effect.Execute(effectExecutionContext, this);
            }
            finally
            {
                IsProcessing = false;
                triggered.Clear();
            }
        }

        public void BeginEffect()
        {
            activeEffectCount++;
        }

        public void EndEffect()
        {
            activeEffectCount--;

            if (activeEffectCount == 0)
            {
                RunGravityAndRefill();
            }
        }

        public void SuspendGrid()
        {
            suspendCounter++;
        }

        public void ResumeGrid()
        {
            suspendCounter--;
        }

        public void RunGravityAndRefill()
        {
            if (suspendCounter > 0)
            {
                return;
            }

            gridRefill.ApplyGravity();
            gridSpawner.SpawnNewCubeBlocks();
            gridChecker.CheckAllGrid();
        }
    }
}