namespace ColorBlast.ObjectPool
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}