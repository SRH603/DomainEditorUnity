using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;
using UnityEngine;

using UnityObject = UnityEngine.Object;
using System.Threading.Tasks;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetDatabaseSaveViewModel :  HierarchicalDataViewModel<AssetViewModel>, ISaveAssetDialog
    {
        public event Action<ISaveAssetDialog, UnityObject> SaveCompleted;

        private Sprite m_assetIcon;
        [Binding]
        public Sprite AssetIcon
        {
            get { return m_assetIcon; }
            set
            {
                if (m_assetIcon != value)
                {
                    m_assetIcon = value;
                    RaisePropertyChanged(nameof(AssetIcon));
                }
            }
        }


        private UnityObject m_asset;
        public UnityObject Asset
        {
            get { return m_asset != null ? m_asset : Editor.CurrentScene; }
            set { m_asset = value; }
        }

        private bool m_selectSavedAssets = true;
        public bool SelectSavedAssets 
        {
            get { return m_selectSavedAssets; }
            set { m_selectSavedAssets = value; }
        }

        private string m_assetName;
        [Binding]
        public string AssetName
        {
            get { return m_assetName; }
            set
            {
                if (m_assetName != value)
                {
                    m_assetName = value;
                    RaisePropertyChanged(nameof(AssetName));

                    if (m_parentDialog != null)
                    {
                        m_parentDialog.IsOkInteractable = IsAssetNameValid();
                    }
                }
            }
        }

        private bool m_activateInputField;
        [Binding]
        public bool ActivateInputField
        {
            get { return m_activateInputField; }
            private set
            {
                if (m_activateInputField != value)
                {
                    m_activateInputField = value;
                    RaisePropertyChanged(nameof(ActivateInputField));
                    m_activateInputField = false;
                }
            }
        }

        private DialogViewModel m_parentDialog;
        [Binding]
        public DialogViewModel ParentDialog
        {
            get
            {
                if (m_parentDialog == null)
                {
                    m_parentDialog = new DialogViewModel();
                }
                return m_parentDialog;
            }
        }

        private Dictionary<ID, AssetViewModel> m_items = new Dictionary<ID, AssetViewModel>();
        protected Dictionary<ID, AssetViewModel> Items
        {
            get { return m_items; }
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ISaveAssetDialog>(this);
        }

        protected override void Start()
        {
            base.Start();
            
            if (Editor.IsProjectLoaded)
            {
                m_items.Clear();
                PopulateItemsDictionary(Editor.RootFolderID);
                BindData();
                RefreshThumbnails();
                if (m_items.TryGetValue(Editor.RootFolderID, out var rootFolder))
                {
                    Expand(rootFolder);
                    SelectedItem = rootFolder;
                }

                if (Asset != null)
                {
                    string desiredName = Asset == Editor.CurrentScene ?
                        Localization.GetString("ID_RTEditor_AssetDatabaseSave_Scene", "Scene") :
                        Asset.name;
                    string ext = Editor.GetExt(Asset);
                    string path = Editor.GetUniquePath(rootFolder.ID, desiredName + ext);
                    AssetName = Editor.GetDisplayName(path);
                }

                ActivateInputField = true;
            }

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetDatabaseSave_Save", "Save"),
                CancelText = Localization.GetString("ID_RTEditor_AssetDatabaseSave_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.IsOkInteractable = IsAssetNameValid();
            m_parentDialog.Ok += OnOk;

            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            IOC.UnregisterFallback<ISaveAssetDialog>(this);
        }

        protected override void OnSelectedItemsChanged(IEnumerable<AssetViewModel> unselectedObjects, IEnumerable<AssetViewModel> selectedObjects)
        {
            if (SelectedItem != null && !Editor.IsFolder(SelectedItem.ID))
            {
                AssetName = SelectedItem.DisplayName;
            }   
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            args.Cancel = true;
            Save();
        }
        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanDrag;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanUnselectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;
            flags &= ~HierarchicalDataFlags.CanEdit;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(AssetViewModel item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override bool HasChildren(AssetViewModel parent)
        {
            return Editor.HasChildren(parent.ID) && Editor.GetChildren(parent.ID, sortByName: false).Where(child => CanDisplay(child)).Any();
        }

        public override IEnumerable<AssetViewModel> GetChildren(AssetViewModel parent)
        {
            if (parent == null)
            {
                if (m_items.Count == 0)
                {
                    return new AssetViewModel[0];
                }

                return new[] { m_items[Editor.RootFolderID] };
            }

            var children = Editor.GetChildren(parent.ID, true);
            var items = new List<AssetViewModel>();
            foreach (var id in children)
            {
                if (!m_items.TryGetValue(id, out var item) || !CanDisplay(id))
                {
                    continue;
                }

                items.Add(item);
            }

            return items;
        }

        public override AssetViewModel GetParent(AssetViewModel item)
        {
            if (!m_items.TryGetValue(Editor.GetParent(item.ID), out var parent))
            {
                return null;
            }

            return parent;
        }

        #endregion

        #region Bound Unity Event Handlers

        public override void OnItemDoubleClick()
        {
            if (IsExpanded(TargetItem))
            {
                Collapse(TargetItem);
            }
            else
            {
                Expand(TargetItem);
            }
        }

        #endregion

        #region Methods

        protected virtual bool CanDisplay(ID id)
        {
            var name = Editor.GetName(id);
            if(name.StartsWith("."))
            {
                return false;
            }

            if (Editor.IsFolder(id))
            {
                return true;
            }

            if (Asset == null || Asset == Editor.CurrentScene)
            {
                return Editor.IsScene(id);
            }

            var type = Editor.GetType(id);
            return type == Asset.GetType();
        }

        private void PopulateItemsDictionary(ID id)
        {
            if (!CanDisplay(id))
            {
                return;
            }

            var item = new AssetViewModel(id, Editor.GetName(id), Editor.ThumbnailUtil.GetBuiltinThumbnail(id, large:false));
            m_items.Add(id, item);

            var children = Editor.GetChildren(id, sortByName: false);
            foreach (ID childID in children)
            {
                if (!CanDisplay(childID))
                {
                    continue;
                }

                PopulateItemsDictionary(childID);
            }
        }

        private async void RefreshThumbnails()
        {
            foreach (var item in m_items.Values.ToArray())
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

                item.Thumbnail = await Editor.ThumbnailUtil.LoadThumbnailAsync(item.ID, large:false);
            }
        }

        private bool IsAssetNameValid()
        {
            return !string.IsNullOrEmpty(AssetName);
        }

        protected virtual async void Save()
        {
            if (!HasSelectedItems)
            {
                return;
            }

            if (Editor.IsPlaying)
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_UnableToSaveAsset", "Unable to save asset"),
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_UnableToSaveAssetInPlayMode", "Unable to save asset in play mode"));
                return;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                return;
            }

            if (AssetName != null && AssetName.Length > 0 && (!char.IsLetter(AssetName[0]) || AssetName[0] == '-'))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_AssetNameIsInvalid", "Asset name is invalid"),
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_AssetNameShouldStartWith", "Asset name should start with letter"));
                return;
            }

            if (!Editor.IsValidName(AssetName))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_AssetNameShouldStartWith", "Asset name is invalid"),
                    Localization.GetString("ID_RTEditor_AssetDatabaseSave_AssetNameInvalidCharacters", "Asset name contains invalid characters"));
                return;
            }

            var selectedItem = SelectedItems.First();

            var asset = Asset;
            string path;
            if (Editor.IsFolder(selectedItem.ID))
            {
                path = Editor.GetUniquePath(selectedItem.ID, asset, AssetName);
            }
            else
            {
                var parentID = Editor.GetParent(selectedItem.ID);
                path = Editor.GetPath(parentID, asset, AssetName);
            }
            
            if (Editor.Exists(path))
            {
                var lc = IOC.Resolve<ILocalization>();
                var wm = IOC.Resolve<IWindowManager>();

                string header = lc.GetString("ID_RTEditor_AssetDatabaseSave_Warning", "Warning");
                string assetName = Editor.IsScene(path) ?
                    lc.GetString("ID_RTEditor_AssetDatabaseSave_Scene", "Scene") :
                    lc.GetString("ID_RTEditor_AssetDatabaseSave_Asset", "Asset");
                string body = string.Format(lc.GetString("ID_RTEditor_AssetDatabaseSave_AlreadyExistsOverwriteFormat", "{0} with with the same name {1} already exists. Overwrite it?"),
                     assetName,
                     Editor.GetDisplayName(path));

                wm.Confirmation(header, body,
                    async (sender, _) =>
                    {
                        await SaveAsync(asset, path);
                    },
                    (sender, _) => { },
                    lc.GetString("ID_RTEditor_AssetDatabaseSave_Overwrite", "Overwrite"),
                    lc.GetString("ID_RTEditor_AssetDatabaseSave_Cancel", "Cancel"));
            }
            else
            {
                await SaveAsync(asset, path);
            }
        }

        private async Task SaveAsync(UnityObject asset, string path)
        {
            try
            {
                var editor = Editor;
                m_parentDialog.Close(null);
                using var b = SetBusy();
                await editor.CreateAssetAsync(asset, path, forceOverwrite: true, extractSubAssets:null, variant:null, select: SelectSavedAssets);
                RaiseSaveCompleted(asset);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                RaiseSaveCompleted(null); // compatibility with previous version
            } 
        }

        private void RaiseSaveCompleted(UnityObject result)
        {
            SaveCompleted?.Invoke(this, result);
        }

        #endregion
    }

}
