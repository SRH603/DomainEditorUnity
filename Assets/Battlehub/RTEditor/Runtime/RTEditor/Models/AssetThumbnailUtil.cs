#if UNITY_STANDALONE_WIN
#define LOAD_IMAGE_ASYNC
#endif

#if LOAD_IMAGE_ASYNC
using Battlehub.Utils;
#endif

using Battlehub.RTEditor.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Battlehub.Storage;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public interface IAssetThumbnailUtil 
    {
        IAssetDatabaseModel AssetDatabaseModel
        {
            set;
        }

        Texture2D NoneThumbnail
        {
            get;
        }

        Task<Texture2D> LoadThumbnailAsync(ID id, bool large = true);
        Texture2D GetBuiltinThumbnail(ID id, bool large = true);
        Texture2D GetBuiltinThumbnail(Type type, bool large = true);
        void DestroyThumbnail(Texture2D texture);

        Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate = true);
        Task<byte[]> EncodeToPngAsync(Texture2D texture);
    }


    public class AssetThumbnailUtil : MonoBehaviour, IAssetThumbnailUtil
    {
        private IAssetDatabaseModel m_assetDatabaseModel;
        public IAssetDatabaseModel AssetDatabaseModel
        {
            get { return m_assetDatabaseModel; }
            set { m_assetDatabaseModel = value; }
        }

        private IThumbnailUtil m_thumbnailUtil;
        protected IThumbnailUtil ThumbnailUtil
        {
            get { return m_thumbnailUtil; }
        }

        private HashSet<Texture> m_builtInIcons;
        private Texture2D m_folderLargeIcon;
        private Texture2D m_folderSmallIcon;
        private Texture2D m_sceneLargeIcon;
        private Texture2D m_sceneSmallIcon;
        private Texture2D m_genericAssetIcon;
        private Texture2D m_noneThumbnail;
        public virtual Texture2D NoneThumbnail
        {
            get { return m_noneThumbnail; }
        }

        private ThemeAsset m_selectedTheme;

        private ISettingsComponent Settings
        {
            get { return IOC.Resolve<ISettingsComponent>(); }
        }

        protected virtual void Awake()
        {
            m_thumbnailUtil = GetComponentInChildren<IThumbnailUtil>();
            m_thumbnailUtil.Layer = 20;

            Init();
        }

        protected virtual void OnDestroy()
        {
            
        }

        private void Init()
        {
            var theme = Settings?.SelectedTheme;
            if (theme == m_selectedTheme)
            {
                return;
            }
            m_selectedTheme = theme;
            if (m_selectedTheme == null)
            {
                return;
            }

            m_thumbnailUtil.Layer = IOC.Resolve<IRTE>().CameraLayerSettings.ResourcePreviewLayer;
            m_folderLargeIcon = m_selectedTheme.GetIcon("RTEAsset_FolderLarge")?.texture;
            m_folderSmallIcon = m_selectedTheme.GetIcon("RTEAsset_FolderSmall")?.texture;
            m_sceneLargeIcon = m_selectedTheme.GetIcon("RTEAsset_SceneLarge")?.texture;
            m_sceneSmallIcon = m_selectedTheme.GetIcon("RTEAsset_SceneSmall")?.texture;
            m_genericAssetIcon = m_selectedTheme.GetIcon("RTEAsset_Object")?.texture;
            m_noneThumbnail = m_selectedTheme.GetIcon("None")?.texture;
            m_builtInIcons = new HashSet<Texture>();
        }

        private bool UsesBuiltinThumbnail(Type type)
        {
            return type != typeof(GameObject) &&
                type != typeof(Material) &&
                type != typeof(Texture2D);
        }

        private bool IsBuiltinThumbnail(Texture texture)
        {
            Init();
            return
                texture == m_folderLargeIcon ||
                texture == m_sceneLargeIcon ||
                texture == m_genericAssetIcon ||
                m_builtInIcons.Contains(texture);
        }

        public virtual Texture2D GetBuiltinThumbnail(ID id, bool large)
        {
            Init();
            Texture2D thumbnail;
            if (m_assetDatabaseModel.IsFolder(id))
            {
                thumbnail = large ? m_folderLargeIcon : m_folderSmallIcon;
            }
            else
            {
                var type = m_assetDatabaseModel.GetType(id);
                if (type == null)
                {
                    object asset = m_assetDatabaseModel.GetAsset(id);
                    if (asset != null)
                    {
                        type = asset.GetType();
                    }
                }
               
                if (type == typeof(GameObject) || type == typeof(Scene))
                {
                    if (m_assetDatabaseModel.IsScene(id))
                    {
                        thumbnail = large ? m_sceneLargeIcon : m_sceneSmallIcon;
                    }
                    else
                    {
                        thumbnail = m_genericAssetIcon;
                    }
                }
                else
                {
                    if (type == null || m_assetDatabaseModel.IsRawData(id))
                    {
                        string ext = m_assetDatabaseModel.GetExtByID(id);
                        if (!string.IsNullOrEmpty(ext))
                        {
                            thumbnail = GetBuiltinThumbnail(ext, large);
                        }
                        else
                        {
                            thumbnail = m_genericAssetIcon;
                        }
                    }
                    else
                    {
                        thumbnail = GetBuiltinThumbnail(type, large);
                    }
                    
                }
            }

            return thumbnail;
        }

        public virtual Texture2D GetBuiltinThumbnail(Type type, bool large)
        {
            Init();
            if (type == null)
            {
                return large ? m_folderLargeIcon : m_folderSmallIcon;
            }

            var icon = Settings?.SelectedTheme.GetIcon($"RTEAsset_{type.FullName}");
            if (icon != null)
            {
                m_builtInIcons.Add(icon.texture);
                return icon.texture;
            }

            return m_genericAssetIcon;
        }

        private Texture2D GetBuiltinThumbnail(string ext, bool large)
        {
            if (ext == null)
            {
                return large ? m_folderLargeIcon : m_folderSmallIcon;
            }

            var icon = Settings?.SelectedTheme.GetIcon($"RTEAsset{ext}");
            if (icon != null)
            {
                m_builtInIcons.Add(icon.texture);
                return icon.texture;
            }

            return m_genericAssetIcon;
        }

        public virtual async Task<Texture2D> LoadThumbnailAsync(ID id, bool large)
        {
            Init();

            Texture2D thumbnail = null;
            if (m_assetDatabaseModel.IsFolder(id))
            {
                thumbnail = m_folderLargeIcon;
            }
            else
            {
                var type = m_assetDatabaseModel.GetType(id);
                if (!UsesBuiltinThumbnail(type) && !m_assetDatabaseModel.IsScene(id))
                {
                    var thumbnailBytes = await m_assetDatabaseModel.LoadThumbnailDataAsync(id);
                    if (thumbnailBytes != null && thumbnailBytes.Length > 0)
                    {
                        thumbnail = new Texture2D(1, 1);
#if LOAD_IMAGE_ASYNC
                        await thumbnail.LoadImageAsync(thumbnailBytes);
#else
                        thumbnail.LoadImage(thumbnailBytes);
#endif
                    }
                    else
                    {
                        thumbnail = GetBuiltinThumbnail(id, large);
                    }
                }
                else
                {
                    thumbnail = GetBuiltinThumbnail(id, large);
                }
            }

            return thumbnail;
        }

        public virtual void DestroyThumbnail(Texture2D texture)
        {
            if (texture == null || IsBuiltinThumbnail(texture) || texture == m_noneThumbnail)
            {
                return;
            }

            UnityEngine.Object.Destroy(texture);
        }

        public virtual Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate = true)
        {
            return m_thumbnailUtil.CreateThumbnailAsync(obj, instantiate);
        }

        public virtual Task<byte[]> EncodeToPngAsync(Texture2D texture)
        {
            return m_thumbnailUtil.EncodeToPngAsync(texture);
        }
    }
}
