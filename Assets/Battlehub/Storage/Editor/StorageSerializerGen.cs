using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Battlehub.Storage
{
    public class StorageSerializerGen
    {
        private const string k_registerSurrogateTemplate = @"
            RegisterSurrogate<{0}<TID>>();";

        private const string k_registerValueSurrogateTemplate = @"
            Register<{0}>();";

        private const string k_registerTemplate = @"
            Register<{0}>();";

        private const string k_registerSerializableArrayTemplate = @"
            Register<SerializableArray<{0}>>();";

        private const string k_registerSerializableListTemplate = @"
            Register<SerializableList<{0}>>();";

        private const string k_template =
@"using System;
namespace Battlehub.Storage
{{
    public class Serializer<TID, TFID> : SerializerBase<TID, TFID> where TID : IEquatable<TID> where TFID : IEquatable<TFID>
    {{
        public Serializer(ITypeMap typeMap) : base(typeMap) {{ }}

        protected override void Initialize()
        {{
            {0}
        }}
    }}
}}
";
        private static bool SurrogateTypeFilter(Type type)
        {
            return type != null && type.GetCustomAttribute<SurrogateAttribute>() != null &&
                type.GetInterfaces().Any(ReflectionHelpers.IsSurrogateOrValueTypeSurrogate);
        }

        private static bool IsNotRTSLType(Type type)
        {
            return type.Namespace == null ||
                !type.FullName.Contains("Battlehub.RTSL") &&
                !type.FullName.Contains(".SL2.");
        }

        private static bool ProtoContractTypeFilter(Type type)
        {
            return type != null && !type.IsGenericType &&
                type.GetCustomAttribute<ProtoContractAttribute>() != null &&
                type.GetCustomAttribute<SurrogateAttribute>() == null &&
                IsNotRTSLType(type);
        }

        public static void Generate()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}/Protobuf";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/Serializer.cs";
            StringBuilder sb = new StringBuilder();

            TypeFinder surrogateTypesFinder = new TypeFinder(BaseTypeFinder.DefaultAssemblyFilter, SurrogateTypeFilter);
            surrogateTypesFinder.Find();

            var surrogatesGen = new SurrogatesGen();
            var primitivesHs = new HashSet<Type>();
            var valueTypeSurrogates = new HashSet<Type>();
            foreach (Type type in surrogateTypesFinder.Types)
            {
                string typeName = surrogatesGen.GetTypeName(type);

                if (type.GetInterfaces().Any(ReflectionHelpers.IsValueTypeSurrogate))
                {
                    valueTypeSurrogates.Add(type);
                    continue;
                }
                else
                {
                    sb.AppendFormat(k_registerSurrogateTemplate, typeName.Remove(typeName.IndexOf('`')));
                }


                SurrogateAttribute surrogateAttribute = type.GetCustomAttribute<SurrogateAttribute>();
                if (surrogateAttribute == null)
                {
                    continue;
                }

                foreach (var property in surrogatesGen.GetSerializableMembers(type, includeProperties:true))
                {
                    var propertyType = surrogatesGen.GetPropertyType(property);
                    foreach (var elementType in surrogatesGen.GetElementTypes(propertyType))
                    {
                        if (surrogatesGen.IsPrimitive(elementType))
                        {
                            primitivesHs.Add(elementType);
                        }
                    }
                }
            }

            foreach (Type type in valueTypeSurrogates)
            {
                string typeName = surrogatesGen.GetTypeName(type);

                string valueTypeSurrogateTypeName = $"{typeName.Remove(typeName.IndexOf('`'))}<TID>";
                sb.AppendFormat(k_registerValueSurrogateTemplate, valueTypeSurrogateTypeName);
                sb.AppendFormat(k_registerSerializableArrayTemplate, valueTypeSurrogateTypeName);
                sb.AppendFormat(k_registerSerializableListTemplate, valueTypeSurrogateTypeName);

                SurrogateAttribute surrogateAttribute = type.GetCustomAttribute<SurrogateAttribute>();
                if (surrogateAttribute == null)
                {
                    continue;
                }

                foreach (var property in surrogatesGen.GetSerializableMembers(type, includeProperties:true))
                {
                    var propertyType = surrogatesGen.GetPropertyType(property);
                    foreach (var elementType in surrogatesGen.GetElementTypes(propertyType))
                    {
                        if (surrogatesGen.IsPrimitive(elementType))
                        {
                            primitivesHs.Add(elementType);
                        }
                    }
                }
            }

            TypeFinder protoContractTypeFinder = new TypeFinder(BaseTypeFinder.DefaultAssemblyFilter, ProtoContractTypeFilter);
            protoContractTypeFinder.Find();

            foreach (Type type in protoContractTypeFinder.Types)
            {
                string typeName = surrogatesGen.GetTypeName(type);
                sb.AppendFormat(k_registerTemplate, typeName);
                sb.AppendFormat(k_registerSerializableArrayTemplate, typeName);
                sb.AppendFormat(k_registerSerializableListTemplate, typeName);
            }

            foreach (Type type in TypeFinder.PrimitiveTypes)
            {
                primitivesHs.Add(type);
            }

            foreach (Type type in primitivesHs)
            {
                string typeName = surrogatesGen.GetTypeName(type);
                sb.AppendFormat(k_registerSerializableArrayTemplate, typeName);
                sb.AppendFormat(k_registerSerializableListTemplate, typeName);
            }

            File.Delete(path);
            File.WriteAllText(path, string.Format(k_template, sb.ToString()));
        }

        public static void GenerateEmpty()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}/Protobuf";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/Serializer.cs";
            File.Delete(path);
            File.WriteAllText(path, string.Format(k_template, string.Empty));
        }
    }
}