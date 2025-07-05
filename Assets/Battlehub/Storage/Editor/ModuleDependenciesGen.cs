using System.IO;

namespace Battlehub.Storage
{
    public class ModuleDependenciesGen
    {
        private const string m_template =
@"using System;
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
";

        private const string m_emptyTemplate =
@"using System;
using UnityEngine;

namespace Battlehub.Storage
{
    public class ModuleDependencies : DefaultModuleDependencies
    {
        protected override IObjectEnumeratorFactory EnumeratorFactory
        {
            get { throw new NotImplementedException(); }
        }
        
        protected override ISurrogatesSerializer<Guid> CreateSurrogatesSerializer()
        {
            throw new NotImplementedException();
        }

        protected override ISerializer CreateSerializer()
        {
            throw new NotImplementedException();
        }

        public ModuleDependencies()
        {
        }

        public ModuleDependencies(GameObject hostGO) : base(hostGO)
        {
        }
    }
}
";
        public static void Generate()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}/Protobuf";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/ModuleDependencies.cs";

            File.WriteAllText(path, m_template);
        }
        public static void GenerateEmpty()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}/Protobuf";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/ModuleDependencies.cs";

            File.WriteAllText(path, m_emptyTemplate);
        }
    }
}