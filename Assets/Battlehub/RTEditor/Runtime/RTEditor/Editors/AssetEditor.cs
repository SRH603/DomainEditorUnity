using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    [Binding]
    public class AssetEditor : ViewModelBase
    {
        [SerializeField]
        private Texture m_thumbnail;

        [Binding]
        public Texture Thumbnail
        {
            get { return m_thumbnail; }
            set 
            {
                if (m_thumbnail != value) 
                {
                    m_thumbnail = value;
                    RaisePropertyChanged(nameof(Thumbnail));
                }
            }
        }

        private string m_name;

        [Binding]
        public string Name
        {
            get { return m_name; }
            set 
            {
                if (m_name != value)
                {
                    m_name = value;
                    RaisePropertyChanged(nameof(Name)); 
                }
            }
        }

        private string m_noteText;
        [Binding]
        public string NoteText
        {
            get { return m_noteText; }
            set
            {
                if (m_noteText != value)
                {
                    m_noteText = value;
                    RaisePropertyChanged(nameof(NoteText));
                }
            }
        }


        private string m_loadText;
        [Binding]
        public string LoadText
        { 
            get { return m_loadText; }
            set
            {
                if (m_loadText != value)
                {
                    m_loadText = value;
                    RaisePropertyChanged(nameof(LoadText));
                }
            }
        }

        private bool m_isLoaded;

        [Binding]
        public bool IsLoaded
        {
            get { return m_isLoaded; }
            set
            {
                if (m_isLoaded != value)
                {
                    m_isLoaded = value;
                    RaisePropertyChanged(nameof(IsLoaded));
                }
            }
        }

        private bool m_canSelectBasePrefab;
        [Binding]
        public bool CanSelectBasePrefab
        {
            get { return m_canSelectBasePrefab; }
            set
            {
                if (m_canSelectBasePrefab != value)
                {
                    m_canSelectBasePrefab = value;
                    RaisePropertyChanged(nameof(CanSelectBasePrefab));
                }
            }
        }

        private string m_selectBasePrefabText;
        [Binding]
        public string SelectBasePrefabText
        {
            get { return m_selectBasePrefabText; }
            set
            {
                if (m_selectBasePrefabText != value)
                {
                    m_selectBasePrefabText = value;
                    RaisePropertyChanged(nameof(SelectBasePrefabText));
                }
            }
        }


        private string m_openText;
        [Binding]
        public string OpenText
        {
            get { return m_openText; }
            set
            {
                if (m_openText != value)
                {
                    m_openText = value;
                    RaisePropertyChanged(nameof(OpenText));
                }
            }
        }

        private bool m_canOpen;
        [Binding]
        public bool CanOpen
        {
            get { return m_canOpen; }
            set
            {
                if (m_canOpen != value)
                {
                    m_canOpen = value;
                    RaisePropertyChanged(nameof(CanOpen));
                }
            }
        }

        private bool m_canCreatePrefabVariant;

        [Binding]
        public bool CanCreatePrefabVariant
        {
            get { return m_canCreatePrefabVariant; }
            set
            {
                if (m_canCreatePrefabVariant != value)
                {
                    m_canCreatePrefabVariant = value;
                    RaisePropertyChanged(nameof(CanCreatePrefabVariant));
                }
            }
        }

        private string m_createPrefabVariantText;
        [Binding]
        public string CreatePrefabVariantText
        {
            get { return m_createPrefabVariantText; }
            set
            {
                if (m_createPrefabVariantText != value)
                {
                    m_createPrefabVariantText = value;
                    RaisePropertyChanged(nameof(CreatePrefabVariantText));
                }
            }
        }

        private IRuntimeEditor m_editor;
        private ILocalization m_localization;
        
        private void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.MoveAssets += OnMoveAssets;
            m_localization = IOC.Resolve<ILocalization>();

            RefreshName();
            RefreshThumbnailTexture();

            bool isLoaded = true;
            var selectedAssets = m_editor.SelectedAssets;

            CanSelectBasePrefab = false;
            CanCreatePrefabVariant = false;

            if (selectedAssets.Any(assetID => !m_editor.IsFolder(assetID) && !m_editor.IsLoaded(assetID) && !m_editor.IsScene(assetID)))
            {
                LoadText = selectedAssets.Length > 1 ?
                    m_localization.GetString("ID_RTEditor_AE_LoadAssets", "Load Assets") :
                    m_localization.GetString("ID_RTEditor_AE_LoadAsset", "Load Asset");
                    
                isLoaded = false;
            }
            IsLoaded = isLoaded;
            if (isLoaded)
            {
                OnAssetsLoaded();
            }
        }

        private void OnDestroy()
        {
            m_localization = null;
            if (m_editor != null)
            {
                m_editor.MoveAssets -= OnMoveAssets;
                m_editor = null;
            }
        }

        [Binding]
        public async void OnLoadAsset()
        {
            using var b = m_editor.SetBusy();

            LoadText = m_localization.GetString("ID_RTEditor_AE_Loading", "Loading...");

            foreach (var assetID in m_editor.SelectedAssets.ToArray())
            {
                await m_editor.LoadAssetAsync(assetID);
            }

            IsLoaded = true;
            OnAssetsLoaded();
        }

        [Binding]
        public void OnSelectBasePrefab()
        {
            var selectedAsset = m_editor.SelectedAssets[0];
            object prefabVariant = m_editor.GetAsset(selectedAsset);
            m_editor.SelectedAssets = new[] { m_editor.GetAssetIDByInstance(prefabVariant) };
        }

        [Binding]
        public async void OnOpenAsset()
        {
            using var b = m_editor.SetBusy();
            var selectedAsset = m_editor.SelectedAssets[0];
            var assetDatabase = m_editor;
            await assetDatabase.OpenAssetAsync(selectedAsset);
        }


        [Binding]
        public async void OnCreatePrefabVariant()
        {
            using var b = m_editor.SetBusy();

            var assetID = m_editor.SelectedAssets[0];
            var folderID = m_editor.CurrentFolderID;
            var asset = await m_editor.LoadAssetAsync(assetID);
            var name = m_editor.GetDisplayName(assetID);
            var path = m_editor.GetUniquePath(folderID, asset, name);

            assetID = await m_editor.CreateAssetAsync(asset, path, variant: true);
            m_editor.SelectedAssets = new[] { assetID };
        }


        private void OnAssetsLoaded()
        {
            var selectedAssets = m_editor.SelectedAssets;
            if (selectedAssets.All(id => m_editor.CanEditAsset(id)))
            {
                var assets = selectedAssets.Select(id => m_editor.GetAsset(id)).OfType<UnityObject>().ToArray();
                var typesHs = new HashSet<Type>(assets.Select(asset => asset.GetType()));
                if (typesHs.Count == 1)
                {
                    var type = typesHs.First();
                    var asset = assets.First();
                    
                    var editorsMap = IOC.Resolve<IEditorsMap>();
                    var editorForTypeExists = editorsMap == null ||
                        editorsMap.GetObjectEditor(type) != null ||
                        asset is Material material && material.shader != null && editorsMap.GetMaterialEditor(material.shader) != null;

                    if (editorForTypeExists)
                    {
                        m_editor.Selection.Select(assets.FirstOrDefault(), assets);
                    }
                }
            }
            else
            {
                if (selectedAssets.Length == 1)
                {
                    var assetID = selectedAssets[0];
                    
                    // if (m_assetDatabase.IsExternalAsset(assetID))
                    // {
                    //    NoteText = m_localization.GetString("ID_RTEditor_AE_ExternalAssetsAreReadOnly", "External assets are read-only.");
                    // }
                    
                    CanOpen = m_editor.CanOpenAsset(assetID);

                    if (m_editor.IsScene(assetID))
                    {
                        OpenText = m_localization.GetString("ID_RTEditor_AE_OpenScene", "Open Scene");
                    }
                    else
                    {

                        SelectBasePrefabText = m_localization.GetString("ID_RTEditor_AE_SelectBasePrefab", "Select Base Prefab");
                        CanSelectBasePrefab = m_editor.IsPrefabVariant(assetID);

                        CreatePrefabVariantText = m_localization.GetString("ID_RTEditor_AE_CreatePrefabVariant", "Create Prefab Variant");
                        CanCreatePrefabVariant = m_editor.CanCreatePrefabVariant(assetID);

                        OpenText = m_localization.GetString("ID_RTEditor_AE_OpenPrefab", "Open Prefab"); 
                    }
                }   
            }
        }

        private void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            RefreshName();
        }

        private void RefreshName()
        {
            Name = GetName();
        }

        private async void RefreshThumbnailTexture()
        {
            Thumbnail = await LoadThumbnailAsync();
        }

        private async Task<Texture2D> LoadThumbnailAsync()
        {
            var selectedAssets = m_editor.SelectedAssets.ToArray();
            if (selectedAssets.Length > 0)
            {
                var thumbnailUtil = m_editor.ThumbnailUtil;
                if (selectedAssets.Length == 1)
                {
                    return await thumbnailUtil.LoadThumbnailAsync(selectedAssets[0]);
                }

                var genericAssetThumbnail = thumbnailUtil.GetBuiltinThumbnail(ID.Empty);
                string typeName = GetTypeName(selectedAssets[0]);
                for (int i = 1; i < selectedAssets.Length; ++i)
                {
                    if (typeName != GetTypeName(selectedAssets[i]))
                    {
                        return genericAssetThumbnail;
                    }
                }

                var thumbnail = thumbnailUtil.GetBuiltinThumbnail(selectedAssets[0]);
                if (thumbnail == null)
                {
                    thumbnail = genericAssetThumbnail;
                }
                return thumbnail;
            }

            return null;
        }

        private string GetName()
        {
            var selectedAssets = m_editor.SelectedAssets;
            if (selectedAssets.Length > 0)
            {
                string name = m_editor.GetDisplayName(selectedAssets[0]);
                string typeName = GetTypeName(selectedAssets[0]);

                if (selectedAssets.Length == 1)
                {
                    if (m_editor.IsExternalAsset(selectedAssets[0]))
                    {
                        string externalFormat = m_localization.GetString("ID_RTEditor_AE_ExternalFormat", "External {0}");
                        return $"{name} ({string.Format(externalFormat, typeName)})";
                    }
                    else
                    {
                        return $"{name} ({typeName})";
                    }
                }

                for (int i = 1; i < selectedAssets.Length; ++i)
                {
                    if (typeName != GetTypeName(selectedAssets[i]))
                    {
                        typeName = m_localization.GetString("ID_RTEditor_AE_Asset", "Asset");
                        break;
                    }
                }

                return $"{selectedAssets.Length} {typeName}s";
            }

            return m_localization.GetString("ID_RTEditor_AE_0Assets", "0 Assets");
        }

        private string GetTypeName(ID id)
        {
            if (m_editor.IsScene(id))
            {
                return m_localization.GetString("ID_RTEditor_AE_Scene", "Scene");
            }
            return !m_editor.IsFolder(id) ?
                m_editor.GetType(id).Name :
                m_localization.GetString("ID_RTEditor_AE_Folder", "Folder");
        }
    }
}
