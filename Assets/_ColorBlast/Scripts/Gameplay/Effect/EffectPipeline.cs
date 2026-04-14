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

        private int suspendCounter = 0; // increase when any disco ball trigger
        private int activeEffectCount = 0; // increase any effect trigger
        private bool isApplyingGrid = false; // to understand gravity and refill is processing or not
        private bool isGridUpdatePending = false;
        public bool IsProcessing { get; private set; }

        public void Initialize(GridRefill gridRefill, GridSpawner gridSpawner, GridChecker gridChecker,
            EffectExecutionContext effectExecutionContext)
        {
            this.gridRefill = gridRefill;
            this.gridSpawner = gridSpawner;
            this.gridChecker = gridChecker;
            this.effectExecutionContext = effectExecutionContext;
        }

        public void TriggerEffectFromPlayer(IBlockEffect effect)
        {
            if (effect == null || IsProcessing)
            {
                return;
            }

            triggered.Clear();
            suspendCounter = 0;
            activeEffectCount = 0;
            isGridUpdatePending = false;
            isApplyingGrid = false;
            IsProcessing = true;

            TriggerEffect(effect);
        }

        public bool IsTriggered(Block block)
        {
            return block != null && triggered.Contains(block);
        }

        public void MarkTriggered(Block block)
        {
            if (block == null)
            {
                return;
            }

            triggered.Add(block);
        }

        public void TriggerEffect(IBlockEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            IsProcessing = true;
            ExecuteEffect(effect).Forget();
        }

        public async UniTask TriggerEffectAsync(IBlockEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            IsProcessing = true;
            await effect.Execute(effectExecutionContext, this);
        }

        public void BeginEffect()
        {
            activeEffectCount++;
        }

        public void EndEffect()
        {
            activeEffectCount = Mathf.Max(0, activeEffectCount - 1);
            isGridUpdatePending = true;
            TryRunPendingUpdateGrid();
        }

        public void SuspendGrid()
        {
            suspendCounter++;
        }

        public void ResumeGrid()
        {
            suspendCounter = Mathf.Max(0, suspendCounter - 1);
            TryRunPendingUpdateGrid();
        }

        // Forces gravity + refill immediately, regardless of activeEffectCount.
        // Waits if a grid cycle is already in progress.
        public async UniTask ForceGridUpdate()
        {
            if (isApplyingGrid)
            {
                await UniTask.WaitUntil(() => !isApplyingGrid);
                return;
            }

            isApplyingGrid = true;
            try
            {
                gridRefill.ApplyGravity();
                gridSpawner.SpawnNewCubeBlocks();
                gridChecker.CheckAllGrid();
                await UniTask.Yield();
            }
            finally
            {
                isApplyingGrid = false;
            }
        }

        // ── Execution loop ─
        private async UniTaskVoid ExecuteEffect(IBlockEffect effect)
        {
            try
            {
                BeginEffect();
                await effect.Execute(effectExecutionContext, this);
            }
            finally
            {
                EndEffect();
                TryCompleteRun();
            }
        }

        private void TryRunPendingUpdateGrid()
        {
            if (!isGridUpdatePending || isApplyingGrid || activeEffectCount > 0 || suspendCounter > 0)
            {
                return;
            }

            RunGravityAndRefill().Forget();
        }

        private async UniTaskVoid RunGravityAndRefill()
        {
            if (isApplyingGrid)
            {
                return;
            }

            isApplyingGrid = true;

            try
            {
                if (isGridUpdatePending && suspendCounter == 0)
                {
                    isGridUpdatePending = false;

                    gridRefill.ApplyGravity();
                    gridSpawner.SpawnNewCubeBlocks();
                    gridChecker.CheckAllGrid();

                    await UniTask.Yield();
                }
            }
            finally
            {
                isApplyingGrid = false;

                if (isGridUpdatePending)
                {
                    TryRunPendingUpdateGrid();
                }

                TryCompleteRun();
            }
        }

        private void TryCompleteRun()
        {
            if (activeEffectCount > 0 || suspendCounter > 0 || isApplyingGrid || isGridUpdatePending)
            {
                return;
            }

            IsProcessing = false;
        }
    }
}