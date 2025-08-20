using System.Collections.Generic;

namespace Blackout.Pool
{
    public class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> _pool = new ObjectPool<List<T>>(() => new List<T>(), actionOnRelease: l => l.Clear());

        public static List<T> Get() => _pool.Get();

        public static PooledObject<List<T>> Get(out List<T> value)
        {
            return _pool.Get(out value);
        }

        public static void Release(List<T> toRelease)
        {
            _pool.Release(toRelease);
        }
    }
}