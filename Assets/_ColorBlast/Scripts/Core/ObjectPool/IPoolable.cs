namespace ColorBlast.Core
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}