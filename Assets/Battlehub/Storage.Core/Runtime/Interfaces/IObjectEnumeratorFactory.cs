using System;

namespace Battlehub.Storage
{
    public interface IObjectEnumeratorFactory
    {
        bool IsRegistered(Type type);

        void Register(Type type, Type enumeratorType);

        void Unregister(Type type);

        void Reset();

        IObjectEnumerator Create(object obj, Type type);
        
    }
}
