using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.Storage.Surrogates.UnityEngine.SceneManagement
{
    [ProtoContract]
    [Surrogate(typeof(Scene), propertyIndex: _PROPERTY_INDEX, typeIndex: _TYPE_INDEX, enableUpdates:false)]

    public struct SceneSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 3;
        const int _TYPE_INDEX = 102;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        
        [ProtoMember(2)]
        public TID ID
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public List<TID> GameObjectIDs
        {
            get;
            set;
        }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            ID = idmap.GetOrCreateID(obj);

            Scene scene = (Scene)obj;
            var gameObjects = new List<GameObject>();
            scene.GetRootGameObjects(gameObjects);

            GameObjectIDs = new List<TID>();

            int layer = LayerMask.NameToLayer(StorageLayers.IgnoreLayer);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                if (go.layer == layer)
                {
                    continue;
                }

                GameObjectIDs.Add(idmap.GetOrCreateID(go));
            }

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            Scene scene = SceneManager.GetActiveScene();
            int siblingIndex = scene.rootCount;
            for (int i = 0; i < GameObjectIDs.Count; ++i)
            {
                TID id = GameObjectIDs[i];

                GameObject go = idmap.GetObject<GameObject>(id);
                if(go != null)
                {
                    go.transform.SetSiblingIndex(siblingIndex);
                    siblingIndex++;
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            object result = scene;
            idmap.AddObject(result, ID);
            return new ValueTask<object>(result);
        }
    }
}
