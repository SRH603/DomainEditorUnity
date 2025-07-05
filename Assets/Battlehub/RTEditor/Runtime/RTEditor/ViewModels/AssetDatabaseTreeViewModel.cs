using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    [DefaultExecutionOrder(1)]
    public class AssetDatabaseTreeViewModel : AssetDatabaseBaseViewModel
    {
        private Dictionary<ID, AssetViewModel> m_items = new Dictionary<ID, AssetViewModel>();
        protected Dictionary<ID, AssetViewModel> Items
        {
            get { return m_items; }
        }

        public override AssetViewModel SelectedItem
        {
            get { return base.SelectedItem; }
            set
            {
                if (value == null)
                {
                    base.SelectedItem = null;
                }
                else
                {
                    AssetViewModel selectedItem = value;
                    selectedItem = m_items[selectedItem.ID];
                    if (selectedItem == null)
                    {
                        base.SelectedItem = null;
                    }
                    else
                    {
                        if (CanDisplay(selectedItem.ID))
                        {
                            var parent = GetParent(selectedItem);
                            if (parent == null)
                            {
                                Expand(selectedItem);
                            }
                            else
                            {
                                Expand(parent);
                            }

                            ScrollIntoView = true;
                            base.SelectedItem = selectedItem;
                            ScrollIntoView = false;
                        }
                    }
                }
            }
        }

        protected override void OnSelectedItemsChanged(IEnumerable<AssetViewModel> unselectedItems, IEnumerable<AssetViewModel> selectedItems)
        {
            var item = selectedItems != null ?
                selectedItems.FirstOrDefault() : 
                null;

            if (item != null)
            {
                Editor.CurrentFolderID = item.ID;
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (Editor != null && Editor.IsProjectLoaded)
            {
                RefreshTree();
            }
        }

        #region AssetDatabaseModel event handlers
        private bool m_reloadProject;

        protected override void OnBeforeReloadProject(object sender, EventArgs e)
        {
            m_reloadProject = true;
        }

        protected override void OnLoadProject(object sender, EventArgs e)
        {
            m_reloadProject = false;
            RefreshTree();
        }

        protected override void OnUnloadProject(object sender, EventArgs e)
        {
            if (!m_reloadProject)
            {
                SelectedItem = null;

                m_items.Clear();
                BindData();
            }
        }

        protected override void OnChangeCurrentFolder(object sender, EventArgs e)
        {
            if (Editor.IsProjectLoaded)
            {
                if (m_items.TryGetValue(Editor.CurrentFolderID, out var item))
                {
                    SelectedItem = item;
                }
            }
            else
            {
                SelectedItem = null;
            }
        }

        protected override void OnCreateAsset(object sender, CreateAssetEventArgs e)
        {
            if (!CanDisplay(e.AssetID))
            {
                return;
            }

            AddItem(e.AssetID, Editor.GetParent(e.AssetID), true, true);
        }

        protected override void OnCreateFolder(object sender, CreateFolderEventArgs e)
        {
            if (!CanDisplay(e.AssetID))
            {
                return;
            }          

            AddItem(e.AssetID, Editor.GetParent(e.AssetID), true, true);
        }

       
        protected override void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                Guid newAssetID = Editor.GetAssetID(e.NewPath[i]);

                var oldChildren = e.ChildrenID[i];
                var children = Editor.GetChildren(newAssetID, sortByName: false, recursive: true).ToArray();

                for (int j = 0; j < children.Length; ++j)
                {
                    var childID = children[j];
                    if (!CanDisplay(childID))
                    {
                        continue;
                    }

                    var oldChildID = oldChildren[j];
                    if (m_items.TryGetValue(oldChildID, out var childItem))
                    {
                        m_items.Remove(oldChildID);
                        childItem.ID = childID;
                        childItem.Name = Editor.GetName(childID);
                        m_items.Add(childID, childItem);
                    }
                }

                MoveItem(e.AssetID[i], e.ParentID[i], e.NewPath[i]);
            }
        }

        protected override void OnDuplicateAssets(object sender, DuplicateAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                if (!CanDisplay(e.AssetID[i]))
                {
                    continue;
                }

                var children = Editor.GetChildren(e.AssetID[i], sortByName: false, recursive: true);
                foreach (ID childID in children)
                {
                    if (!CanDisplay(childID))
                    {
                        continue;
                    }

                    CreateItem(childID);
                }

                AddItem(e.AssetID[i], Editor.GetParent(e.AssetID[i]), false, false);
            }
        }

        protected override void OnDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                RemoveItem(e.AssetID[i], e.ParentID[i]);
                foreach (var childID in e.ChildrenID[i])
                {
                    RemoveItem(childID);
                }
            }
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = base.GetFlags();
            
            flags &= ~HierarchicalDataFlags.CanUnselectAll;
            flags &= ~HierarchicalDataFlags.CanMultiSelect;

            return flags;
        }

        public override bool HasChildren(AssetViewModel parent)
        {
            return Editor.HasChildren(parent.ID) && Editor.GetChildren(parent.ID, sortByName:false).Where(child => CanDisplay(child)).Any();
        }

        public override IEnumerable<AssetViewModel> GetChildren(AssetViewModel parent)
        {
            if (parent == null)
            {
                if (m_items.Count == 0)
                {
                    return null;
                }

                return new[] { m_items[Editor.RootFolderID] };
            }

            var children = Editor.GetChildren(parent.ID, true);
            var items = new List<AssetViewModel>();
            foreach (var id in children) 
            {
                if (!m_items.TryGetValue(id, out var item))
                {
                    continue;
                }

                items.Add(item);
            }

            return items;
        }

        public override AssetViewModel GetParent(AssetViewModel item)
        {
            if(item.ID == ID.Empty || !m_items.TryGetValue(Editor.GetParent(item.ID), out var parent))
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


        public override void OnDelete()
        {
            BeginRemoveSelectedItems();
        }

        public override void OnItemsRemoved()
        {
            foreach (AssetViewModel item in SourceItems)
            {
                m_items.Remove(item.ID);
            }

            base.OnItemsRemoved();
        }

        #endregion

        #region Context Menu

        protected override ContextMenuAnchor GetContextMenuAnchor()
        {
            return new ContextMenuAnchor(
                TargetItem != null ? TargetItem.ID : default,
                SelectedItems?.Select(item => (object)item.ID).ToArray());
        }

        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            base.OnContextMenu(menuItems);

            MenuItemViewModel createFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_CreateFolder", "Create Folder") };
            createFolder.Validate = CreateFolderValidateContextMenuCmd;
            createFolder.Action = CreateFolderContextMenuCmd;
            menuItems.Add(createFolder);

            bool canDuplicate = SelectedItems.First() != null && GetParent(SelectedItems.First()) != null && SelectedItems.All(item => Editor.CanDuplicateAsset(item.ID));
            if (canDuplicate)
            {
                MenuItemViewModel duplicateFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Duplicate", "Duplicate") };
                duplicateFolder.Validate = DuplicateValidateContextMenuCmd;
                duplicateFolder.Action = DuplicateContextMenuCmd;
                menuItems.Add(duplicateFolder);
            }
            
            MenuItemViewModel deleteFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Delete", "Delete") };
            deleteFolder.Validate = DeleteFolderValidateContextMenuCmd;
            deleteFolder.Action = DeleteFolderContextMenuCmd;
            menuItems.Add(deleteFolder);

            MenuItemViewModel renameFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_AssetDatabase_Rename", "Rename") };
            renameFolder.Validate = RenameValidateContextMenuCmd;
            renameFolder.Action = RenameFolderContextMenuCmd;
            menuItems.Add(renameFolder);
        }

        protected virtual void CreateFolderValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
        }

        protected async virtual void CreateFolderContextMenuCmd(string arg)
        {
            var parentFolderId = SelectedItems.First().ID;
            var path = Editor.GetUniquePath(parentFolderId, Localization.GetString("ID_RTEditor_AssetDatabase_Folder", "Folder"));
            await Editor.CreateFolderAsync(path);
        }

        protected virtual void DuplicateValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            var item = SelectedItems.First();
            if (item == null || GetParent(item) == null || SelectedItems.Any(item => !Editor.CanDuplicateAsset(item.ID)))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DuplicateContextMenuCmd(string arg)
        {
            OnDuplicate();
        }

        protected virtual void DeleteFolderValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            var item = SelectedItems.First();
            if (item == null || GetParent(item) == null)
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteFolderContextMenuCmd(string arg)
        {
            OnDelete();
        }

        protected virtual void RenameValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            args.IsValid = (GetItemFlags(SelectedItems.First()) & HierarchicalDataItemFlags.CanEdit) != 0;
        }

        protected virtual void RenameFolderContextMenuCmd(string arg)
        {
            IsEditing = true;
        }

        #endregion

        #region Methods

        protected override bool CanDisplay(ID id)
        {
            return Editor.IsFolder(id) && base.CanDisplay(id);
        }

        private void PopulateItemsDictionary(ID id)
        {
            if (!CanDisplay(id))
            {
                return;
            }

            var item = new AssetViewModel(id, Editor.GetName(id), ThumbnailUtil.GetBuiltinThumbnail(id));
            m_items.Add(id, item);

            var children = Editor.GetChildren(id, sortByName: false);
            foreach (ID childID in children)
            {
                PopulateItemsDictionary(childID);
            }
        }

        private AssetViewModel CreateItem(ID id)
        {
            string name = Editor.GetName(id);
            var item = new AssetViewModel(id, name, ThumbnailUtil.GetBuiltinThumbnail(id));
            m_items.Add(id, item);
            return item;
        }

        protected virtual void AddItem(ID id, ID parentID, bool select, bool expand)
        {
            if (!m_items.TryGetValue(parentID, out var parentItem))
            {
                if (parentID == ID.Empty)
                {
                    return;
                }

                AddItem(parentID, Editor.GetParent(parentID), false, false);
                if (!m_items.TryGetValue(parentID, out parentItem))
                {
                    return;
                }
            }

            if (m_items.TryGetValue(id, out _))
            {
                Debug.LogWarning($"Item with same id {id} already added");
                return;
            }

            AssetViewModel item = CreateItem(id);

            RaiseItemAdded(parentItem, item);
            MoveItemToLocation(item, parentItem);

            if (expand)
            {
                Expand(parentItem);
            }

            if (select)
            {
                ScrollIntoView = true;
                SelectedItems = new[] { item };
                ScrollIntoView = false;
            }
        }

        protected virtual void MoveItem(ID id, ID parentID, string newPath)
        {
            if (!m_items.TryGetValue(parentID, out AssetViewModel oldParentItem))
            {
                return;
            }

            if (!m_items.TryGetValue(id, out AssetViewModel item))
            {
                return;
            }

            m_items.Remove(item.ID);
            item.ID = Editor.GetAssetID(newPath);
            item.Name = Editor.GetName(item.ID);
            m_items.Add(item.ID, item);

            RaiseParentChanged(oldParentItem, item);

            var newParentID = Editor.GetParent(item.ID);
            if (!m_items.TryGetValue(newParentID, out var newParentItem))
            {
                Debug.LogWarning($"item with id {newParentID} not found");
                return;
            }
            MoveItemToLocation(item, newParentItem);
            Expand(newParentItem);
        }

        private void MoveItemToLocation(AssetViewModel item, AssetViewModel parentItem)
        {
            var children = GetChildren(parentItem).ToArray();
            if (children.Length > 1)
            {
                int index = Array.IndexOf(children,  item);
                if (index == 0)
                {
                    var nextItem = children[index + 1];
                    RaisePrevSiblingChanged(item, nextItem);
                }
                else if (1 <= index && index < children.Length - 1)
                {
                    var prevItem = children[index - 1];
                    RaiseNextSiblingChanged(item, prevItem);
                }
            }
        }

        protected override void BeginRemoveSelectedItems()
        {
            if (SelectedItem != null)
            {
                var items = SelectedItems.ToArray();
                if (items.Any(item => item.ID == Editor.RootFolderID))
                {
                    WindowManager.MessageBox(
                        Localization.GetString("ID_RTEditor_AssetDatabase_UnableToRemove", "Unable to remove"),
                        Localization.GetString("ID_RTEditor_AssetDatabase_UnableToRemoveRootFolder", "Unable to remove root folder"));
                }
                else
                {
                    WindowManager.Confirmation(
                        Localization.GetString("ID_RTEditor_AssetDatabase_DeleteSelectedAssets", "Delete selected assets"),
                        Localization.GetString("ID_RTEditor_AssetDatabase_YouCanNotUndoThisAction", "You cannot undo this action"), (dialog, arg) =>
                        {
                            RaiseRemoveSelected();
                            if (m_items.TryGetValue(Editor.RootFolderID, out var rootItem))
                            {
                                SelectedItem = rootItem;
                            }
                        },
                    (dialog, arg) => { },
                        Localization.GetString("ID_RTEditor_AssetDatabase_BtnDelete", "Delete"),
                        Localization.GetString("ID_RTEditor_AssetDatabase_BtnCancel", "Cancel"));
                }
            }
        }

        protected virtual void RemoveItem(ID id, ID parentID)
        {
            if (!m_items.TryGetValue(id, out AssetViewModel item))
            {
                return;
            }

            if (!m_items.TryGetValue(parentID, out AssetViewModel parentItem))
            {
                return;
            }

            RaiseItemRemoved(parentItem, item);
            RemoveItem(id);
        }

        private void RemoveItem(ID id)
        {
            if (!m_items.TryGetValue(id, out AssetViewModel item))
            {
                return;
            }

            m_items.Remove(id);
            var items = new[] { item };
            if (m_selectedItems != null && m_selectedItems.Except(items).Count() != m_selectedItems.Count())
            {
                SelectedItems = m_selectedItems.Except(items);
                if (SelectedItem == null)
                {
                    SelectedItem = GetChildren(null).FirstOrDefault();
                }
            }
        }

        private void RefreshTree()
        {
            m_items.Clear();
            PopulateItemsDictionary(Editor.RootFolderID);
            BindData();

            if (m_items.TryGetValue(Editor.CurrentFolderID, out var item))
            {
                SelectedItem = item;
            }
        }

        #endregion
    }
}
