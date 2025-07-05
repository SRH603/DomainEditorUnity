using System;
using UnityEngine;

namespace Battlehub.Storage
{
    public class ModuleDependencies : DefaultModuleDependencies
    {
        private ObjectEnumeratorFactoryBase m_enumeratorFactory;
        protected override IObjectEnumeratorFactory EnumeratorFactory
        {
            get
            {
                if (m_enumeratorFactory == null)
                {
                    m_enumeratorFactory = new ObjectEnumeratorFactory(TypeMap);
                }

                return m_enumeratorFactory;
            }
        }

        protected override ISurrogatesSerializer<Guid> CreateSurrogatesSerializer()
        {
            return new Serializer<Guid, string>(TypeMap);
        }

        protected override ISerializer CreateSerializer()
        {
            return new Serializer<Guid, string>(TypeMap);
        }
        
        public ModuleDependencies()
        {
        }

        public ModuleDependencies(GameObject hostGO) : base(hostGO)
        {
        }

    }
}
