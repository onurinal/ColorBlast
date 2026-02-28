using UnityEngine;
using ColorBlast.Core;
using ColorBlast.Manager;

namespace ColorBlast.Gameplay
{
    /// <summary>
    /// Represents a single block on the game board.
    /// Handles grid position data, visual updates, animations and pooling
    /// </summary>
    public class Block : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private BlockProperties blockProperties;
        [SerializeField] private BlockView blockView;

        public BlockColorData ColorData { get; private set; }
        public int CurrentGroupSize { get; private set; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public bool IsAnimating { get; private set; }

        private void Awake()
        {
            blockView.SetBlockScale(blockProperties.BlockSizeX, blockProperties.BlockSizeY);
        }

        public void Initialize(int gridX, int gridY, BlockColorData colorData)
        {
            SetGridPosition(gridX, gridY);
            ColorData = colorData;
            RefreshVisual();
        }

        public void SetGridPosition(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;

            blockView.UpdateSortingOrder(gridY);
        }

        public void UpdateGroupSize(int groupSize)
        {
            CurrentGroupSize = groupSize;
            RefreshVisual();
        }

        public void MoveToPosition(Vector2 targetPosition)
        {
            IsAnimating = true;
            blockView.PlayMoveAnim(targetPosition, blockProperties.MoveDurationSec, () => IsAnimating = false);
        }

        public void HandleDestroy()
        {
            IsAnimating = false;
            blockView.PlayDestroyAnim(blockProperties.DestroyDurationSec, (ReturnToPool));
        }

        public void OnSpawn()
        {
            //  when block has some animations after spawning
        }

        public void OnDespawn()
        {
            IsAnimating = false;
            CurrentGroupSize = 0;
            blockView.ResetView();
        }

        public bool CanInteract()
        {
            return !IsAnimating;
        }

        public void SetVisible(bool visible)
        {
            blockView.SetVisible(visible);
        }

        public void SetColor(BlockColorData newColorData)
        {
            ColorData = newColorData;
            RefreshVisual();
        }

        public bool IsVisible() => blockView.IsVisible();

        private void RefreshVisual()
        {
            if (ColorData == null)
            {
                return;
            }

            var sprite = ColorData.GetVisual(CurrentGroupSize);
            blockView.UpdateVisual(sprite);
        }

        private void ReturnToPool()
        {
            ObjectPoolManager.Instance.ReturnBlock(this);
        }
    }
}