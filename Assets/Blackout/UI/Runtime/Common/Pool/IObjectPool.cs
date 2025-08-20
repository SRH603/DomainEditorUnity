namespace Blackout.Pool
{
    public interface IObjectPool<T> where T : class
    {
        int CountInactive { get; }

        T Get();

        PooledObject<T> Get(out T v);

        void Release(T element);

        void Clear();
    }
}