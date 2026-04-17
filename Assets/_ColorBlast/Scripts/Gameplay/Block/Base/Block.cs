using UnityEngine;
using ColorBlast.Core;
using ColorBlast.Manager;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Abstract base for all block types.
    /// Owns grid position, view, shared animations and pooling lifecycle.
    /// Block type–specific logic lives in derived classes.
    /// </summary>
    public abstract class Block : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] protected BlockView blockView;

        private bool isDestroying;

        public abstract BlockData BlockData { get; protected set; }
        public BlockType BlockType => BlockData.BlockType;
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        public bool IsBusy => isDestroying || blockView.IsAnimating;

        public abstract void Initialize(int gridX, int gridY, BlockData blockData);

        public virtual void OnSpawn() { }

        public virtual void OnDespawn()
        {
            isDestroying = false;
            blockView.ResetView();
        }

        public virtual void DestroyBlock()
        {
            if (isDestroying)
            {
                return;
            }

            isDestroying = true;
            blockView.HandleDestroy(gameplayConfig.DestroyDuration, ReturnToPool);
        }

        public virtual void RemoveBlock()
        {
            BlockPoolManager.Instance.ReturnBlock(this);
        }

        public virtual void UpdateIcon(int groupSize) { }

        public virtual void SetupVisual()
        {
            blockView.SetModelScale(gameplayConfig.BlockSizeX, gameplayConfig.BlockSizeY);
        }

        public void SetGridPosition(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;

            blockView.UpdateSortingOrder(gridY);
        }

        public void MoveToPosition(Vector2 targetPosition)
        {
            blockView.MoveToPosition(targetPosition, gameplayConfig.FallDuration);
        }

        private void ReturnToPool()
        {
            BlockPoolManager.Instance.ReturnBlock(this);
        }
    }
}