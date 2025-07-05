using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetDatabaseBaseViewModel : HierarchicalDataViewModel<AssetViewModel>
    {
        protected IAssetThumbnailUtil ThumbnailUtil
        {
            get { return Editor.ThumbnailUtil; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            var editor = Editor;
            if (editor != null)
            {
                editor.BeforeReloadProject += OnBeforeReloadProject;
                editor.LoadProject += OnLoadProject;
                editor.UnloadProject += OnUnloadProject;
                editor.ChangeCurrentFolder += OnChangeCurrentFolder;
                editor.ChangeAssetSelection += OnChangeAssetSelection;
                editor.CreateAsset += OnCreateAsset;
                editor.CreateFolder += OnCreateFolder;
                editor.BeforeMoveAssets += OnBeforeMoveAssets;
                editor.MoveAssets += OnMoveAssets;
                editor.DuplicateAssets += OnDuplicateAssets;
                editor.BeforeDeleteAssets += OnBeforeDeleteAssets;
                editor.DeleteAssets += OnDeleteAssets;
                editor.SaveAsset += OnSaveAsset;
                editor.UpdateAssetThumbnail += OnUpdateAssetThumbnail;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnCleanup();
        }

        protected virtual void OnCleanup()
        {
            var editor = Editor;
            if (editor != null)
            {
                editor.BeforeReloadProject -= OnBeforeReloadProject;
                editor.LoadProject -= OnLoadProject;
                editor.UnloadProject -= OnUnloadProject;
                editor.ChangeCurrentFolder -= OnChangeCurrentFolder;
                editor.ChangeAssetSelection -= OnChangeAssetSelection;
                editor.CreateAsset -= OnCreateAsset;
                editor.CreateFolder -= OnCreateFolder;
                editor.BeforeMoveAssets -= OnBeforeMoveAssets;
                editor.MoveAssets -= OnMoveAssets;
                editor.DuplicateAssets -= OnDuplicateAssets;
                editor.BeforeDeleteAssets -= OnBeforeDeleteAssets;
                editor.DeleteAssets -= OnDeleteAssets;
                editor.SaveAsset -= OnSaveAsset;
                editor.UpdateAssetThumbnail -= OnUpdateAssetThumbnail;
                editor = null;
            }
        }

        #region

        protected virtual void OnBeforeReloadProject(object sender, EventArgs e)
        {
            
        }

        protected virtual void OnLoadProject(object sender, EventArgs e)
        {
        }

        protected virtual void OnUnloadProject(object sender, EventArgs e)
        {
        }

        protected virtual void OnChangeCurrentFolder(object sender, EventArgs e)
        {
        }

        protected virtual void OnChangeAssetSelection(object sender, AssetSelectionEventArgs e)
        {
        }

        protected virtual void OnCreateAsset(object sender, CreateAssetEventArgs e)
        {
        }

        protected virtual void OnCreateFolder(object sender, CreateFolderEventArgs e)
        {
        }

        protected virtual void OnBeforeMoveAssets(object sender, MoveAssetsEventArgs e)
        {
        }

        protected virtual void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
        }

        protected virtual void OnDuplicateAssets(object sender, DuplicateAssetsEventArgs e)
        {
        }

        protected virtual void OnBeforeDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
        }

        protected virtual void OnDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
        }

        protected virtual void OnSaveAsset(object sender, SaveAssetEventArgs e)
        {   
        }

        protected virtual void OnUpdateAssetThumbnail(object sender, SaveAssetEventArgs e)
        {
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanReorder;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;


            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(AssetViewModel item)
        {
            HierarchicalDataItemFlags flags = HierarchicalDataItemFlags.Default;
            if (item.ID == Editor.RootFolderID)
            {
                flags &= ~HierarchicalDataItemFlags.CanRemove;
                flags &= ~HierarchicalDataItemFlags.CanEdit;
                flags &= ~HierarchicalDataItemFlags.CanDrag;
            }

            if (!CanDrop(item, SourceItems))
            {
                flags &= ~HierarchicalDataItemFlags.CanBeParent;
            }

            return flags;
        }

        #endregion

        #region Bound UnityEvent Handlers

        public override async void OnExternalObjectDrop()
        {
            if (!AllowDropExternalObjects())
            {
                CanDropExternalObjects = false;
                return;
            }

            if (CanDrop(TargetItem, ExternalDragObjects))
            {
                using var b = SetBusy();
                var assetIDs = ExternalDragObjects.OfType<AssetViewModel>().Select(item => item.ID).ToArray();
                await Editor.MoveAssetsAsync(assetIDs, TargetItem.ID);
            }
            else if (CanCreatePrefab(TargetItem, ExternalDragObjects))
            {
                var dropTarget = GetDropTarget();
                foreach (ExposeToEditor obj in ExternalDragObjects.OfType<ExposeToEditor>())
                {
                    await Editor.CreateAssetAsync(obj.gameObject, Editor.GetPath(dropTarget.ID));
                    obj.RaisePropertyChanged(nameof(gameObject));
                }    
            }

            CanDropExternalObjects = false;
        }

        public override void OnItemDragEnter()
        {
            if (!CanDrop(TargetItem, SourceItems))
            {
                TargetItem = null;
            }
        }

        public override async void OnItemsDrop()
        {
            CanDropItems = false;

            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                using var b = SetBusy();

                var assetIDs = SourceItems.OfType<AssetViewModel>().Select(item => item.ID).ToArray();
                await Editor.MoveAssetsAsync(assetIDs, TargetItem.ID);
            }
        }

        private string m_oldName;
        public override void OnItemBeginEdit()
        {
            m_oldName = TargetItem.Name;
        }

        public override async void OnItemEndEdit()
        {
            string newName = TargetItem.Name;
            if (CanRename(TargetItem.ID, newName))
            {
                var parentItemID = Editor.GetParent(TargetItem.ID);
                string targetPath = Editor.GetUniquePath(parentItemID, newName);
                await Editor.MoveAssetsAsync(new[] { TargetItem.ID }, new[] { targetPath });
            }
            else
            {
                TargetItem.Name = m_oldName;
            }
        }

        public override void OnDuplicate()
        {
            if (SelectedItem != null && SelectedItems.All(item => Editor.CanDuplicateAsset(item.ID)))
            {
                var items = SelectedItems.ToArray();
                DuplicateItems(items);
            }
        }

        public override void OnDelete()
        {
            BeginRemoveSelectedItems();
        }

        public override async void OnItemsRemoved()
        {
            var ids = new List<ID>();
            foreach (AssetViewModel item in SourceItems)
            {
                ids.Add(item.ID);
            }

            await Editor.DeleteAssetsAsync(ids);
        }

        #endregion

        #region Methods

        protected virtual AssetViewModel GetDropTarget()
        {
            return TargetItem;
        }

        protected override bool AllowDropExternalObjects()
        {
            var targetItem = GetDropTarget();
            if (targetItem == null || !Editor.IsFolder(targetItem.ID))
            {
                return false;
            }

            var items = ExternalDragObjects.OfType<AssetViewModel>();
            if (items.Any() && !CanDrop(targetItem, items))
            {
                return false;
            }

            if (!ExternalDragObjects.OfType<ExposeToEditor>().Any())
            {
                return true;
            }

            return CanCreatePrefab(targetItem, ExternalDragObjects);
        }

        protected virtual bool CanCreatePrefab(AssetViewModel targetItem, IEnumerable<object> dragObjects)
        {
            ExposeToEditor[] objects = dragObjects.OfType<ExposeToEditor>().ToArray();
            if (objects.Length == 0)
            {
                return false;
            }

            return objects.All(o => o.CanCreatePrefab && Editor.CanCreatePrefab(o.gameObject));
        }

        protected virtual bool CanRename(ID id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }

            if (!Editor.IsValidName(newName))
            {
                return false;
            }

            ID parentID = Editor.GetParent(id);

            string newPath = Editor.GetUniquePath(parentID, newName);

            return Editor.GetName(newPath) == newName;
        }

        protected virtual bool CanDisplay(ID id)
        {
            var name = Editor.GetName(id);
            return !name.StartsWith("."); // - hidden
        }

        protected virtual bool CanDrop(AssetViewModel dropTarget, IEnumerable<object> dragItems)
        {
            if (dropTarget == null || !Editor.IsFolder(dropTarget.ID))
            {
                return false;
            }

            if (dragItems == null)
            {
                return true;
            }

            AssetViewModel[] dragProjectItems = dragItems.OfType<AssetViewModel>().ToArray();
            if (dragProjectItems.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < dragProjectItems.Length; ++i)
            {
                AssetViewModel dragItem = dragProjectItems[i];
                if (dragItem.ID == dropTarget.ID)
                {
                    return false;
                }

                if (IsDescendantOf(dragItem, dropTarget))
                {
                    return false;
                }

                string path = Editor.GetUniquePath(dropTarget.ID, dragItem.Name);
                if (Editor.GetName(path) != dragItem.Name)
                {
                    return false;
                }
            }
            return true;
        }

        protected bool IsDescendantOf(AssetViewModel ancestor, AssetViewModel item)
        {
            var id = item != null ? item.ID : ID.Empty;
            while (id != ID.Empty)
            {
                if (id == ancestor.ID)
                {
                    return true;
                }

                id = Editor.GetParent(id);
            }
            return false;
        }

        protected virtual async void DuplicateItems(AssetViewModel[] items)
        {
            var ids = items.Where(item => item.ID != Editor.RootFolderID).Select(item => item.ID);

            await Editor.DuplicateAssetsAsync(ids);
        }

        protected virtual void BeginRemoveSelectedItems()
        {
            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_AssetDatabase_DeleteSelectedAssets", "Delete Selected Assets"),
                Localization.GetString("ID_RTEditor_AssetDatabase_YouCanNotUndoThisAction", "You cannot undo this action"),
                (sender, arg) =>
                {
                    bool wasEnabled = Editor.Undo.Enabled;
                    Undo.Enabled = false;
                    if (Selection.activeObject != null)
                    {
                        var selectedIDs = SelectedItems != null ?
                            new HashSet<ID>(SelectedItems.Select(item => item.ID)) :
                            new HashSet<ID>();

                        var selection = Selection.objects.ToList();
                        for (int i = selection.Count - 1; i >=0; i--)
                        {
                            ID assetId = Editor.GetAssetIDByInstance(selection[i]);
                            if (selectedIDs.Contains(assetId))
                            {
                                selection.RemoveAt(i);
                            }
                        }

                        Selection.objects = 
                            selection.Count > 0 ? 
                            selection.ToArray() : null;
                    }
                    Undo.Enabled = wasEnabled;

                    RaiseRemoveSelected();

                },
            (sender, arg) => { },
            Localization.GetString("ID_RTEditor_AssetDatabase_BtnDelete", "Delete"),
            Localization.GetString("ID_RTEditor_AssetDatabase_BtnCancel", "Cancel"));
        }
        #endregion
    }
}