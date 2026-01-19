using ColorBlast.Blocks;
using ColorBlast.Level;
using ColorBlast.ObjectPool;
using UnityEngine;

namespace ColorBlast.Manager
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [Header("Block Pool Settings")]
        [SerializeField] private Block blockPrefab;
        [SerializeField] private Transform blockParent;
        [SerializeField] private int poolMultiplier = 2;

        private PoolableObject<Block> blockPool;
        private bool isInitialized;

        private void Awake()
        {
            InitializeSingleton();
        }

        private void InitializeSingleton()
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
            if (isInitialized)
            {
                return;
            }

            var initialBlockSize = poolMultiplier * levelProperties.RowCount * levelProperties.ColumnCount;
            blockPool = new PoolableObject<Block>(blockPrefab, initialBlockSize, blockParent);

            isInitialized = true;
        }

        public Block GetBlock()
        {
            if (!isInitialized)
            {
                return null;
            }

            return blockPool.Get();
        }

        public void ReturnBlock(Block block)
        {
            if (!isInitialized)
            {
                return;
            }

            blockPool.Return(block);
        }
    }
}