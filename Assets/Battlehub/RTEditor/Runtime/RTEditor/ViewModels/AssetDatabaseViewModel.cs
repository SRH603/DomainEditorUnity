using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    [DefaultExecutionOrder(0)]
    public class AssetDatabaseViewModel : AssetDatabaseBaseViewModel
    {
        private bool m_tryToChangeSelectedFolder;
        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_tryToChangeSelectedFolder = !string.IsNullOrWhiteSpace(m_filterText) && string.IsNullOrWhiteSpace(value);
                    m_filterText = value;
                    RaisePropertyChanged(nameof(FilterText));
                    ApplyFilter();
                }
            }
        }

        private AssetViewModel m_currentFolderItem;
        private Dictionary<ID, AssetViewModel> m_idToItem = new Dictionary<ID, AssetViewModel>();
        private List<AssetViewModel> m_items = new List<AssetViewModel>();
        protected List<AssetViewModel> Items
        {
            get { return m_items; }
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();

            if (Editor != null && Editor.IsProjectLoaded)
            {
                await RefreshCurrentFolder();
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
        }

        #region AssetDatabaseModel event handlers

        private bool m_reloadProject;
        protected override void OnBeforeReloadProject(object sender, EventArgs e)
        {
            m_reloadProject = true;
        }

        protected override void OnUnloadProject(object sender, EventArgs e)
        {
            if (!m_reloadProject)
            {
                SelectedItem = null;
                m_currentFolderItem = null;
                m_idToItem.Clear();
                m_items.Clear();
                BindData();
            }
        }

        protected override void OnLoadProject(object sender, EventArgs e)
        {
            m_reloadProject = false;
        }

        private bool m_selectAndScrollIntoView = false;
        protected override async void OnChangeCurrentFolder(object sender, EventArgs e)
        {
            m_tryToChangeSelectedFolder = false;
            m_filterText = null;
            RaisePropertyChanged(nameof(FilterText));
            await RefreshCurrentFolder();

            if (m_selectAndScrollIntoView)
            {
                try
                {
                    await Task.Yield();
                    RefreshSelectionAndScrollIntoView(Editor.SelectedAssets);
                }
                finally
                {
                    m_selectAndScrollIntoView = false;
                }
            }
        }

        private bool m_handleChangeAssetSelection = true;
        protected override void OnChangeAssetSelection(object sender, AssetSelectionEventArgs e)
        {
            if (!m_handleChangeAssetSelection)
            {
                return;
            }

            if (e.SelectedAssets.Length > 0)
            {
                var folderID = Editor.GetParent(e.SelectedAssets[0]);
                if (folderID != ID.Empty && Editor.CurrentFolderID != folderID)
                {
                    m_selectAndScrollIntoView = true;
                    Editor.CurrentFolderID = folderID;
                    return;
                }
            }

            RefreshSelectionAndScrollIntoView(e.SelectedAssets);
        }

        private void RefreshSelectionAndScrollIntoView(IEnumerable<ID> selectedAssets)
        {
            ScrollIntoView = true;
            SelectedItems = selectedAssets
                .Select(id => GetItem(id))
                .Where(item => item != null)
                .ToArray();
            ScrollIntoView = false;
        }

        protected override void OnCreateAsset(object sender, CreateAssetEventArgs e)
        {
            var parentID = Editor.GetParent(e.AssetID);

            var item = GetItem(e.OverwrittenAssetID);
            if (item != null)
            {
                m_idToItem.Remove(e.OverwrittenAssetID);
                item.ID = e.AssetID;
                m_idToItem.Add(item.ID, item);
            }

            if (parentID == Editor.CurrentFolderID)
            {
                CreateMissingItems(false);
            }
            else
            {
                TryCreateMissingItems(parentID);
            }
        }

        protected override void OnCreateFolder(object sender, CreateFolderEventArgs e)
        {
            var parentID = Editor.GetParent(e.AssetID);
            if (parentID == Editor.CurrentFolderID)
            {
                CreateMissingItems(false);
            }
            else
            {
                TryCreateMissingItems(parentID);
            }
        }

        protected override void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            bool createMissingItems = false;
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                createMissingItems |= RefreshItem(e.AssetID[i], e.NewPath[i]);

                var newAssetID = Editor.GetAssetID(e.NewPath[i]);
                var childrenIDs = e.ChildrenID[i];
                var newChildrenIDs = Editor.GetChildren(newAssetID, sortByName: false, recursive: true).ToArray();
                for (int j = 0; j < newChildrenIDs.Length; ++j)
                {
                    var childID = childrenIDs[j];
                    var newPath = Editor.GetPath(newChildrenIDs[j]);
                    createMissingItems |= RefreshItem(childID, newPath);
                }
            }

            m_items = m_items.OrderBy(item => item.Name).ToList();

            if (createMissingItems)
            {
                CreateMissingItems(false);
            }
        }

        protected override void OnDuplicateAssets(object sender, DuplicateAssetsEventArgs e)
        {
            if (!e.AssetID.Any(id => CanDisplay(id) && Editor.GetParent(id) == Editor.CurrentFolderID))
            {
                return;
            }

            CreateMissingItems(false);
        }

        protected override void OnDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                AssetViewModel item = GetItem(e.AssetID[i]);
                if (item != null)
                {
                    m_items.Remove(item);
                    m_idToItem.Remove(item.ID);
                    RaiseItemRemoved(null, item);
                }

                foreach (var childID in e.ChildrenID[i])
                {
                    item = GetItem(childID);
                    if (item != null)
                    {
                        m_items.Remove(item);
                        m_idToItem.Remove(item.ID);
                    }
                }
            }
        }

        protected override void OnSaveAsset(object sender, SaveAssetEventArgs e)
        {
            var item = GetItem(e.AssetID);
            if (item != null)
            {
                if (e.Thumbnail == null)
                {
                    RefreshThumbnail(item, ThumbnailUtil.GetBuiltinThumbnail(e.AssetID));
                }
                else
                {
                    RefreshThumbnail(item, e.Thumbnail);
                }
            }
        }

        protected override void OnUpdateAssetThumbnail(object sender, SaveAssetEventArgs e)
        {
            var item = GetItem(e.AssetID);
            if (item != null)
            {
                if (e.Thumbnail == null)
                {
                    RefreshThumbnail(item, ThumbnailUtil.GetBuiltinThumbnail(e.AssetID));
                }
                else
                {
                    RefreshThumbnail(item, e.Thumbnail);
                }
            }
        }

        #endregion

        #region IHierarchicalData

        public override IEnumerable<AssetViewModel> GetChildren(AssetViewModel parent)
        {
            return m_items != null ? m_items.ToList() : null;
        }

        #endregion

        #region Bound UnityEvent Handlers

        public override async void OnItemDoubleClick()
        {
            await OpenAsync(TargetItem.ID);
        }

        public override void OnItemsRemoved()
        {
            foreach (AssetViewModel item in SourceItems)
            {
                m_items.Remove(item);
                m_idToItem.Remove(item.ID);
            }

            base.OnItemsRemoved();
        }

        private bool m_handleSelectedItemsChanged = true;
        protected override void OnSelectedItemsChanged(IEnumerable<AssetViewModel> unselectedItems, IEnumerable<AssetViewModel> selectedItems)
        {
            if (!m_handleSelectedItemsChanged)
            {
                return;
            }

            m_handleChangeAssetSelection = false;
            try
            {
                Editor.SelectedAssets = selectedItems != null ? selectedItems.Select(asset => asset.ID).ToArray() : new ID[0];
            }
            finally
            {
                m_handleChangeAssetSelection = true;
            }
        }

        #endregion

        #region Context Menu

        protected override ContextMenuAnchor GetContextMenuAnchor()
        {
            var target = TargetItem != null ? TargetItem.ID : Editor?.CurrentFolderID;
            var selection = SelectedItems?.Select(item => (object)item.ID).ToArray();
            return new ContextMenuAnchor(target, selection);
        }

        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            if (!Editor.IsProjectLoaded)
            {
                return;
            }

            if (TargetItem == null)
            {
                var createFolder = new MenuItemViewModel
                {
                    Path = string.Format("{0}/{1}",
                        Localization.GetString("ID_RTEditor_AssetDatabase_Create", "Create"),
                        Localization.GetString("ID_RTEditor_AssetDatabase_Folder", "Folder"))
                };
                createFolder.Command = "CurrentFolder";
                createFolder.Action = CreateFolderContextMenuCmd;
                menuItems.Add(createFolder);

                string materialStr = Localization.GetString("ID_RTEditor_AssetDatabase_Material", "Material");
                string animationClipStr = Localization.GetString("ID_RTEditor_AssetDatabase_AnimationClip", "Animation Clip");
                CreateMenuItem(materialStr, materialStr, typeof(Material), menuItems);
                CreateMenuItem(animationClipStr, animationClipStr.Replace(" ", ""), typeof(RuntimeAnimationClip), menuItems);

                /*
                MenuItemViewModel showInExplorerCmd = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_ShowInExplorer", "Show In Explorer") };
                showInExplorerCmd.Action = ShowInExplorerContextMenuCmd;
                showInExplorerCmd.Validate = ShowInExplorerValidateContextMenuCmd;
                menuItems.Add(showInExplorerCmd);
                */
            }
            else
            {
                var createFolder = new MenuItemViewModel
                {
                    Path = string.Format("{0}/{1}",
                        Localization.GetString("ID_RTEditor_AssetDatabase_Create", "Create"),
                        Localization.GetString("ID_RTEditor_AssetDatabase_Folder", "Folder"))
                };

                createFolder.Action = CreateFolderContextMenuCmd;
                createFolder.Validate = CreateValidateContextMenuCmd;
                menuItems.Add(createFolder);

                if (Editor.CanCreatePrefabVariant(TargetItem.ID))
                {
                    MenuItemViewModel createVariant = new MenuItemViewModel
                    {
                        Path = string.Format("{0}/{1}",
                            Localization.GetString("ID_RTEditor_AssetDatabase_Create", "Create"),
                            Localization.GetString("ID_RTEditor_AssetDatabase_PrefabVariant", "Prefab Variant"))
                    };

                    createVariant.Action = CreateVariantContextMenuCmd;
                    createVariant.Validate = CreateVariantValidateContextMenuCmd;
                    menuItems.Add(createVariant);
                }

                string materialStr = Localization.GetString("ID_RTEditor_AssetDatabase_Material", "Material");
                string animationClipStr = Localization.GetString("ID_RTEditor_AssetDatabase_AnimationClip", "Animation Clip");
                CreateMenuItem(materialStr, materialStr, typeof(Material), menuItems);
                CreateMenuItem(animationClipStr, animationClipStr.Replace(" ", ""), typeof(RuntimeAnimationClip), menuItems);

                MenuItemViewModel open = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Open", "Open") };
                open.Action = OpenContextMenuCmd;
                open.Validate = OpenValidateContextMenuCmd;
                menuItems.Add(open);

                bool canDuplicate = SelectedItems.First() != null && SelectedItems.All(item => Editor.CanDuplicateAsset(item.ID));
                if (canDuplicate)
                {
                    MenuItemViewModel duplicate = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Duplicate", "Duplicate") };
                    duplicate.Action = DuplicateContextMenuCmd;
                    duplicate.Validate = DuplicateValidateContextMenuCmd;
                    menuItems.Add(duplicate);
                }

                MenuItemViewModel deleteFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Delete", "Delete") };
                deleteFolder.Action = DeleteContextMenuCmd;
                deleteFolder.Validate = DeleteValidateContextMenuCmd;
                menuItems.Add(deleteFolder);

                MenuItemViewModel renameFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Rename", "Rename") };
                renameFolder.Action = RenameContextMenuCmd;
                renameFolder.Validate = RenameValidateContextMenuCmd;
                menuItems.Add(renameFolder);

                /*
                MenuItemViewModel showInExplorerCmd = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_ShowInExplorer", "Show In Explorer") };
                showInExplorerCmd.Action = ShowInExplorerContextMenuCmd;
                showInExplorerCmd.Validate = ShowInExplorerValidateContextMenuCmd;
                menuItems.Add(showInExplorerCmd);
                */
            }
        }

        private void CreateMenuItem(string text, string defaultName, Type type, List<MenuItemViewModel> menuItems)
        {
            if (Editor != null && Editor.GetTypeID(type) != ID.Empty)
            {
                MenuItemViewModel createAsset = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Create", "Create") + "/" + text };
                createAsset.Command = "CurrentFolder";
                createAsset.Action = arg => CreateAsset(arg, type, defaultName);
                createAsset.Validate = CreateValidateContextMenuCmd;
                menuItems.Add(createAsset);
            }
        }

        protected virtual void CreateValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = true;
        }

        protected virtual void CreateFolderContextMenuCmd(string arg)
        {
            bool currentFolder = !string.IsNullOrEmpty(arg);
            if (Editor != null)
            {
                _ = CreateFolderAsync(currentFolder ? Editor.CurrentFolderID : SelectedItem.ID);
            }
        }

        protected virtual void CreateVariantValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = SelectedItem != null;
        }

        protected virtual async void CreateVariantContextMenuCmd(string arg)
        {
            using var b = SetBusy();

            var folderID = Editor.CurrentFolderID;
            var asset = await Editor.LoadAssetAsync(SelectedItem.ID);
            var name = Editor.GetDisplayName(SelectedItem.ID);
            var path = Editor.GetUniquePath(folderID, asset, name);

            await Editor.CreateAssetAsync(asset, path, variant: true);
        }

        protected virtual void OpenValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = SelectedItem != null;
            if (args.IsValid)
            {
                args.IsValid = Editor.IsFolder(SelectedItem.ID) || Editor.CanOpenAsset(SelectedItem.ID);
            }
        }

        protected virtual void OpenContextMenuCmd(string arg)
        {
            _ = OpenAsync(SelectedItem.ID);
        }

        protected virtual void DuplicateValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            var item = SelectedItem;
            if (item == null || SelectedItems.Any(item => !Editor.CanDuplicateAsset(item.ID)))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DuplicateContextMenuCmd(string arg)
        {
            OnDuplicate();
        }

        protected virtual void DeleteValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            var item = SelectedItem;
            if (item == null)
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteContextMenuCmd(string arg)
        {
            OnDelete();
        }

        protected virtual void RenameValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = (GetItemFlags(SelectedItems.First()) & HierarchicalDataItemFlags.CanEdit) != 0;
        }

        protected virtual void RenameContextMenuCmd(string arg)
        {
            IsEditing = true;
        }

        /*
        protected virtual void ShowInExplorerValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = true;
        }

        protected virtual void ShowInExplorerContextMenuCmd(string arg)
        {
            string path = AssetDatabase.GetPath(AssetDatabase.CurrentFolder);
            Application.OpenURL(new Uri(path).AbsoluteUri);
        }
        */

        #endregion

        #region Methods

        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                if (m_tryToChangeSelectedFolder)
                {
                    var currentFolder = Editor.CurrentFolderID;
                    var selectedItems = SelectedItems;
                    if (selectedItems != null && selectedItems.Any())
                    {
                        if (!selectedItems.Any(item => Editor.GetParent(item.ID) == currentFolder))
                        {
                            Editor.CurrentFolderID = Editor.GetParent(selectedItems.First().ID);
                            return;
                        }
                    }
                }
            }

            _ = RefreshCurrentFolder();

        }

        protected override AssetViewModel GetDropTarget()
        {
            return TargetItem != null ? TargetItem : m_currentFolderItem;
        }

        public override void BindData()
        {
            base.BindData();

            m_handleSelectedItemsChanged = false;
            try
            {
                if (Editor.SelectedAssets == null)
                {
                    SelectedItems = null;
                }
                else
                {
                    var selectedIDs = new HashSet<ID>(Editor.SelectedAssets);
                    SelectedItems = m_items.Where(item => selectedIDs.Contains(item.ID)).ToArray();
                }
            }
            finally
            {
                m_handleSelectedItemsChanged = true;
            }
        }

        private async void CreateAsset(string arg, Type type, string defaultName)
        {
            var obj = Editor.CreateObjectOfType(type) as UnityObject;
            if (obj == null)
            {
                return;
            }

            obj.name = defaultName;
            await Editor.CreateAssetAsync(obj);
        }

        protected virtual async Task CreateFolderAsync(ID parentFolderId)
        {
            var path = Editor.GetUniquePath(parentFolderId, "Folder");
            await Editor.CreateFolderAsync(path);
        }

        private void TryCreateMissingItems(ID parentID)
        {
            while (true)
            {
                ID parentOfParent = Editor.GetParent(parentID);
                if (parentOfParent == ID.Empty)
                {
                    break;
                }
                else if (parentOfParent == Editor.CurrentFolderID)
                {
                    if (!m_idToItem.ContainsKey(parentID))
                    {
                        CreateMissingItems(false);
                        break;
                    }
                }
                parentID = parentOfParent;
            }
        }

        private bool RefreshItem(ID assetID, string assetPath)
        {
            bool createMissingItems = false;
            var item = GetItem(assetID);
            if (item != null)
            {
                m_idToItem.Remove(assetID);

                item.ID = Editor.GetAssetID(assetPath);
                item.Name = Editor.GetName(item.ID);

                if (Editor.GetParent(item.ID) != Editor.CurrentFolderID)
                {
                    RaiseItemRemoved(null, item);
                    m_items.Remove(item);
                }
                else
                {
                    m_idToItem.Add(item.ID, item);
                }
            }
            else
            {
                createMissingItems = true;
            }

            return createMissingItems;
        }

        private async void CreateMissingItems(bool selectAndScrollIntoView)
        {
            var ids = Editor.GetChildren(Editor.CurrentFolderID, sortByName: true).Where(CanDisplay).ToArray();
            var newItems = new List<AssetViewModel>();
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var itemId = i < m_items.Count ? m_items[i].ID : ID.Empty;

                if (id != itemId)
                {
                    var newItem = new AssetViewModel(id, Editor.GetName(id), Editor.ThumbnailUtil.NoneThumbnail);
                    newItems.Add(newItem);
                    m_items.Insert(i, newItem);
                    m_idToItem.Add(newItem.ID, newItem);
                    RaiseItemInserted(i, newItem);
                }
                else
                {
                    RaiseReset(m_items[i]);
                }
            }

            if (newItems.Count > 0 && selectAndScrollIntoView)
            {
                var item = newItems.Last();
                SelectAndScrollIntoView(item);
            }

            for (int i = 0; i < newItems.Count; ++i)
            {
                AssetViewModel newItem = newItems[i];
                newItem.Thumbnail = await ThumbnailUtil.LoadThumbnailAsync(newItem.ID);
            }
        }

        private void SelectAndScrollIntoView(AssetViewModel item)
        {
            ScrollIntoView = true;
            SelectedItem = item;
            ScrollIntoView = false;
        }

        private async Task OpenAsync(ID id)
        {
            if (Editor.IsFolder(id))
            {
                Editor.CurrentFolderID = id;
            }
            else
            {
                if (Editor.CanOpenAsset(id))
                {
                    using var b = SetBusy();
                    await Editor.OpenAssetAsync(id);
                }
            }
        }

        private async Task RefreshCurrentFolder()
        {
            if (Editor == null)
            {
                return;
            }

            if (Editor.IsProjectLoaded)
            {
                using var b = SetBusy();
                var (currentFolderItem, items) = await CreateItems();
                if (currentFolderItem.ID == Editor.CurrentFolderID)
                {
                    DestroyItems();

                    m_currentFolderItem = currentFolderItem;
                    for (int i = 0; i < items.Count; ++i)
                    {
                        var item = items[i];
                        m_items.Add(item);
                        m_idToItem.Add(item.ID, item);
                    }
                }
            }
            else
            {
                DestroyItems();
            }

            BindData();
        }

        private void RefreshThumbnail(AssetViewModel item, Texture2D thumbnail)
        {
            if (thumbnail == null)
            {
                return;
            }

            ThumbnailUtil.DestroyThumbnail(item.Thumbnail as Texture2D);
            item.Thumbnail = thumbnail;
        }

        private AssetViewModel GetItem(ID assetID)
        {
            return m_idToItem.TryGetValue(assetID, out var item) ? item : null;
        }

        private async Task<(AssetViewModel, List<AssetViewModel>)> CreateItems()
        {
            var items = new List<AssetViewModel>();

            var currentFolderItem = new AssetViewModel(Editor.CurrentFolderID, Editor.GetName(Editor.CurrentFolderID));

            foreach (ID id in Editor.GetChildren(Editor.CurrentFolderID, sortByName: true, recursive: !string.IsNullOrEmpty(FilterText), FilterText))
            {
                if (!CanDisplay(id))
                {
                    continue;
                }

                var item = new AssetViewModel(id, Editor.GetName(id));
                item.Thumbnail = await ThumbnailUtil.LoadThumbnailAsync(id);
                items.Add(item);
            }

            return (currentFolderItem, items);
        }

        private void DestroyItems()
        {
            foreach (var item in Items)
            {
                ThumbnailUtil.DestroyThumbnail(item.Thumbnail as Texture2D);
            }

            if (m_currentFolderItem != null)
            {
                m_currentFolderItem = null;
            }

            m_items.Clear();
            m_idToItem.Clear();
        }

        #endregion

    }
}
