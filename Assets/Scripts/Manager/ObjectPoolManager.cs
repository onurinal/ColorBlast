using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.ObjectPool;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance;

        [Header("Block Settings")]
        [SerializeField] private Block blockPrefab;
        [SerializeField] private Transform blockParent;

        private PoolableObject<Block> blockPool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void InitializePool(LevelProperties levelProperties)
        {
            var initialBlockSize = 2 * levelProperties.RowCount * levelProperties.ColumnCount;
            blockPool = new PoolableObject<Block>(blockPrefab, initialBlockSize, blockParent);
        }

        public Block GetBlock() => blockPool.Get();
        public void ReturnBlock(Block block) => blockPool.Return(block);
    }
}