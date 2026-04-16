using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Handles chain reactions, deduplication and post-effect grid operations.
    /// </summary>
    public class EffectPipeline : IEffectSchedular
    {
        private readonly HashSet<Block> triggered = new();

        private GridRefill gridRefill;
        private GridSpawner gridSpawner;
        private GridChecker gridChecker;
        private EffectExecutionContext context;

        private int activeEffectCount; // increase any effect trigger
        private int suspendCount; // increase any disco ball trigger
        private int pendingGridUpdates;
        private bool isUpdatingGrid; // to understand gravity and refill is processing or not

        public bool IsProcessing => activeEffectCount > 0 || pendingGridUpdates > 0 || isUpdatingGrid;

        public void Initialize(GridRefill gridRefill, GridSpawner gridSpawner, GridChecker gridChecker, EffectExecutionContext context)
        {
            this.gridRefill = gridRefill;
            this.gridSpawner = gridSpawner;
            this.gridChecker = gridChecker;
            this.context = context;
        }

        // ─ Player entry point ─
        public void TriggerEffectFromPlayer(IBlockEffect effect)
        {
            if (effect == null || IsProcessing)
            {
                return;
            }

            triggered.Clear();
            activeEffectCount = 0;
            suspendCount = 0;
            pendingGridUpdates = 0;
            isUpdatingGrid = false;

            ExecuteEffect(effect).Forget();
        }

        // ── IEffectScheduler ───
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

        public void TriggerConcurrent(IBlockEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            ExecuteEffect(effect).Forget();
        }

        public void SuspendGridUpdates()
        {
            suspendCount++;
        }

        public void ResumeGridUpdates()
        {
            suspendCount = Mathf.Max(0, suspendCount - 1);
        }

        // ── Execution loop ─
        private async UniTask ExecuteEffect(IBlockEffect effect)
        {
            activeEffectCount++;

            try
            {
                await effect.Execute(context, this);
            }
            catch (Exception a)
            {
                Debug.LogError($"[EffectPipeline] {effect.GetType().Name} threw: {a}");
            }
            finally
            {
                activeEffectCount--;
                TryGridUpdate();
            }
        }

        private void TryGridUpdate()
        {
            pendingGridUpdates++;

            if (isUpdatingGrid)
            {
                return;
            }

            isUpdatingGrid = true;
            RunGridUpdate().Forget();
        }

        private async UniTaskVoid RunGridUpdate()
        {
            try
            {
                while (pendingGridUpdates > 0)
                {
                    while (suspendCount > 0)
                    {
                        await UniTask.Yield();
                    }

                    if (pendingGridUpdates <= 0)
                    {
                        break;
                    }

                    pendingGridUpdates = 0;
                    await UpdateGrid();
                }
            }
            finally
            {
                isUpdatingGrid = false;
            }
        }

        private async UniTask UpdateGrid()
        {
            gridRefill.ApplyGravity();
            gridSpawner.SpawnNewCubeBlocks();
            gridChecker.CheckAllGrid();

            await UniTask.Yield();
        }
    }
}