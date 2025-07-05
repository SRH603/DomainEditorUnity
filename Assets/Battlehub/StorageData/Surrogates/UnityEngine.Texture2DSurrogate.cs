//#define Storage_Texture2D_Encode
#if UNITY_STANDALONE_WIN
//#define Storage_LoadTextureAsync_FreeImage
using Battlehub.Utils;
#endif

using ProtoBuf;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(Texture2D), propertyIndex: _PROPERTY_INDEX, typeIndex: _TYPE_INDEX, enableUpdates:false)]
    public class Texture2DSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 10;
        const int _TYPE_INDEX = 103;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        
        [ProtoMember(2)]
        public TID ID
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public string Name
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public byte[] Data
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public bool IsNormalMap
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public int AnisoLevel
        {
            get;
            set;
        }


#if USE_RAW_TEXTURE_DATA
        [ProtoMember(7)]
        public int MipCount
        {
            get;
            set;
        }

        [ProtoMember(8)]
        public int Width
        {
            get;
            set;
        }

        [ProtoMember(9)]
        public int Height
        {
            get;
            set;
        }
#endif
        [ProtoMember(10)]
        public TextureFormat Format
        {
            get;
            set;
        }

        [ProtoMember(11)]
        public FilterMode FilterMode
        {
            get;
            set;
        }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        
        public async ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var idmap = ctx.IDMap;
            Texture2D texture = (Texture2D)obj;
            IsNormalMap = await TextureUtils.IsNormalMap(texture);
            //Debug.Log(IsNormalMap + " " + texture.name);
            //Debug.Log("IsNormal Map Check took " + sw.ElapsedMilliseconds + " ms");

            ID = idmap.GetOrCreateID(texture);
            Name = texture.name;
            Format = texture.format;
            AnisoLevel = texture.anisoLevel;
            FilterMode = texture.filterMode;

#if !USE_RAW_TEXTURE_DATA

            // sw.Restart();
            Data = await TextureUtils.Encode(texture);
            //Debug.Log("TextureUtils.Encode took " + sw.ElapsedMilliseconds + " ms");
#else
            MipCount = texture.mipmapCount;
            Data = texture.GetRawTextureData();
            Width = texture.width;
            Height = texture.height; 
#endif
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        }
#if USE_RAW_TEXTURE_DATA
        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
#else
        public async ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
#endif
        {
            var idmap = ctx.IDMap;
            Texture2D texture = null;
#if !USE_RAW_TEXTURE_DATA
            //var sw = System.Diagnostics.Stopwatch.StartNew();

            bool canLoadImageAsync = true;
            #if !UNITY_STANDALONE_WIN
                canLoadImageAsync = false;
            #endif
            if (canLoadImageAsync && ctx.Options.LoadImagesAsync)
            {
                #if UNITY_STANDALONE_WIN
                #if Storage_LoadTextureAsync_FreeImage
                //https://github.com/Looooong/UnityAsyncImageLoader
                var settings = AsyncImageLoader.LoaderSettings.Default;
                settings.linear = IsNormalMap;
                texture = await AsyncImageLoader.CreateFromImageAsync(Data, settings);
                #else
                texture = idmap.GetObject<Texture2D>(ID);
                if (texture == null)
                {
                    texture = new Texture2D(1, 1, TextureFormat.RGBA32, true, IsNormalMap);
                    idmap.AddObject(texture, ID);
                }
                await texture.LoadImageAsync(Data);
                #endif
                #else
                Debug.LogError("Not Supported");
                await Task.Yield();
                #endif
            }
            else
            {
                texture = idmap.GetObject<Texture2D>(ID);
                if (texture == null)
                {
                    texture = new Texture2D(1, 1, TextureFormat.RGBA32, true, IsNormalMap);
                    idmap.AddObject(texture, ID);
                }
                texture.LoadImage(Data);
            }
            texture.anisoLevel = AnisoLevel;
            texture.filterMode = FilterMode;
            //Debug.Log("LoadImage took " + sw.ElapsedMilliseconds + " ms");
#else
            texture = idmap.GetObject<Texture2D>(ID);
            if (texture == null)
            {
                texture = new Texture2D(Width, Height, Format, MipCount, IsNormalMap);
                texture.anisoLevel = AnisoLevel;
                idmap.AddObject(texture, ID);
            }
            texture.LoadRawTextureData(Data);
            texture.Apply();
#endif
            texture.name = Name;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
            return new ValueTask<object>(texture);
        }
    }
}