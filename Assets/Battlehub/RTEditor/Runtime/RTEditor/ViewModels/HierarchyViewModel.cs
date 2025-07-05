using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.UIControls.Binding;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class HierarchyViewModel : HierarchicalDataViewModel<ExposeToEditor>
    {
        #region ExposeToEditorViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ExposeToEditor without modifying the ExposeToEditor itself.
        /// </summary>
        [Binding]
        internal class ExposeToEditorViewModel
        {
            [Binding]
            public bool ActiveInHierarchy
            {
                get;
                set;
            }

            [Binding]
            public bool ActiveSelf
            {
                get;
                set;
            }

            [Binding]
            public string Name
            {
                get;
                set;
            }

            [Binding]
            public GameObject gameObject
            {
                get;
            }

            private ExposeToEditorViewModel() { Debug.Assert(false); }
        }
        #endregion

        [Binding]
        public string CurrentPrefabName
        {
            get { return m_currentPrefab != null ? m_currentPrefab.name : string.Empty; }
        }

        private GameObject m_currentPrefab;
        [Binding]
        public GameObject CurrentPrefab
        {
            get { return m_currentPrefab; }
            set
            {
                if (!ReferenceEquals(m_currentPrefab, value))
                {
                    m_currentPrefab = value;
                    RaisePropertyChanged(nameof(CurrentPrefabName));
                    RaisePropertyChanged(nameof(CurrentPrefab));
                }
            }
        }

        protected GameObject[] SelectedGameObjects
        {
            get
            {
                if (SelectedItems == null)
                {
                    return new GameObject[0];
                }

                return SelectedItems.Select(item => item.gameObject).ToArray();
            }
        }

        protected bool IsFilterTextEmpty
        {
            get { return string.IsNullOrWhiteSpace(m_filterText); }
        }

        private bool m_forceUseCache;
        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;
                    m_forceUseCache = true;
                    RaisePropertyChanged(nameof(FilterText));
                    BindData();
                }
            }
        }

        protected virtual bool Filter(ExposeToEditor go)
        {
            return go.name.ToLower().Contains(FilterText.ToLower());
        }

        private IRuntimeSelectionComponent m_selectionComponent;
        protected IRuntimeSelectionComponent SelectionComponent
        {
            get { return m_selectionComponent; }
        }

        private IPlacementModel m_placementModel;
        private IGroupingModel m_groupingModel;
        private ISettingsComponent m_settingsComponent;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_placementModel = IOC.Resolve<IPlacementModel>();
            m_groupingModel = IOC.Resolve<IGroupingModel>();
            m_settingsComponent = IOC.Resolve<ISettingsComponent>();

            if (Editor.CompatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                Editor.InitializeNewScene += OnInitializeNewScene;
                Editor.BeforeOpenScene += OnBeforeOpenScene;
                Editor.OpenScene += OnOpenScene;
                Editor.BeforeOpenPrefab += OnBeforeOpenPrefab;
                Editor.OpenPrefab += OnOpenPrefab;
                Editor.BeforeClosePrefab += OnBeforeClosePrefab;
                Editor.ClosePrefab += OnClosePrefab;
                Editor.LoadProject += OnLoadProject;
                Editor.UnloadProject += OnUnloadProject;
                Editor.BeforeReloadProject += OnBeforeReloadProject;
                Editor.ReloadProject += OnReloadProject;
                
                if (Editor.IsProjectLoaded)
                {
                    Enable();
                    CurrentPrefab = Editor.CurrentPrefab;
                }
            }
            else
            {     
                Enable();
                #pragma warning disable CS0612, CS0618
                Editor.SceneLoading += OnSceneLoading;
                Editor.SceneLoaded += OnSceneLoaded;
                #pragma warning restore CS0612, CS0618
            }
        }

        protected override void Start()
        {
            base.Start();
            EditorSelectionToSelectedObjects();
        }

        protected override void OnDisable()
        {
            #pragma warning disable CS0612, CS0618
            Editor.SceneLoading -= OnSceneLoading;
            Editor.SceneLoaded -= OnSceneLoaded;
            #pragma warning restore CS0612, CS0618

            Editor.InitializeNewScene -= OnInitializeNewScene;
            Editor.BeforeOpenScene -= OnBeforeOpenScene;
            Editor.OpenScene -= OnOpenScene;
            Editor.BeforeOpenPrefab -= OnBeforeOpenPrefab;
            Editor.OpenPrefab -= OnOpenPrefab;
            Editor.BeforeClosePrefab -= OnBeforeClosePrefab;
            Editor.ClosePrefab -= OnClosePrefab;
            Editor.LoadProject -= OnLoadProject;
            Editor.UnloadProject -= OnUnloadProject;
            Editor.BeforeReloadProject -= OnBeforeReloadProject;
            Editor.ReloadProject -= OnReloadProject;

            Disable();

            m_placementModel = null;
            m_groupingModel = null;
            m_settingsComponent = null;

            base.OnDisable();
        }

        protected virtual void LateUpdate()
        {
            m_rootGameObjects = null;
        }

        protected virtual void Enable()
        {
            if (m_reloadingProject)
            {
                return;
            }

            BindData();

            if (CurrentPrefab != null)
            {
                Expand(CurrentPrefab.GetComponent<ExposeToEditor>());
            }
            
            EditorSelectionToSelectedObjects();

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;

            Selection.SelectionChanged += OnEditorSelectionChanged;

            Editor.Object.Awaked += OnObjectAwaked;
            Editor.Object.Enabled += OnObjectEnabled;
            Editor.Object.Disabled += OnObjectDisabled;
            Editor.Object.Destroying += OnObjectDestroying;
            Editor.Object.MarkAsDestroyedChanging += OnObjectMarkAsDestroyedChanged;
            Editor.Object.ParentChanged += OnObjectParentChanged;
            Editor.Object.NameChanged += OnObjectNameChanged;
            Editor.Object.ComponentAdded += OnObjectComponentAdded;
            Editor.Object.ComponentDestroyed += OnObjectComponentDestroyed;
            
            Editor.SetDirty += OnSetDirty;
            Editor.Detach += OnDetach;
            Editor.ApplyChanges += OnApplyChanges;
            Editor.ApplyChangesToBase += OnApplyChangesToBase;
            Editor.RevertChangesToBase += OnRevertChangesToBase;

            if (m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged += OnThemeChanged;
            }
        }

        protected virtual void Disable()
        {
            if (m_reloadingProject)
            {
                return;
            }

            if (Selection != null)
            {
                Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;

            if (Editor.Object != null)
            {
                Editor.Object.Awaked -= OnObjectAwaked;
                Editor.Object.Enabled -= OnObjectEnabled;
                Editor.Object.Disabled -= OnObjectDisabled;
                Editor.Object.Destroying -= OnObjectDestroying;
                Editor.Object.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
                Editor.Object.ParentChanged -= OnObjectParentChanged;
                Editor.Object.NameChanged -= OnObjectNameChanged;
                Editor.Object.ComponentAdded -= OnObjectComponentAdded;
                Editor.Object.ComponentDestroyed -= OnObjectComponentDestroyed;
            }

            Editor.SetDirty -= OnSetDirty;
            Editor.Detach -= OnDetach;
            Editor.ApplyChanges -= OnApplyChanges;
            Editor.ApplyChangesToBase -= OnApplyChangesToBase;
            Editor.RevertChangesToBase -= OnRevertChangesToBase;
            
            if (m_settingsComponent != null)
            {
                m_settingsComponent.SelectedThemeChanged -= OnThemeChanged;
            }
        }

        public override void BindData()
        {
            if (m_reloadingProject)
            {
                return;
            }

            base.BindData();
        }

        #region Editor EventHandlers

        private void OnThemeChanged(object sender, ThemeAsset oldValue, ThemeAsset newValue)
        {
            BindData();
        }

        protected virtual void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            EditorSelectionToSelectedObjects();
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            BindData();
        }

        protected virtual void OnObjectAwaked(ExposeToEditor obj)
        {
            if(!obj.MarkAsDestroyed)
            {
                if (IsFilterTextEmpty)
                {
                    if (CanShowInHierarchy(obj))
                    {
                        var parent = obj.GetParent();
                        RaiseItemAdded(parent, obj);

                        if (Editor.DragDrop.InProgress && parent != null)
                        {
                            RefreshInstance(parent.gameObject);
                        }
                    }
                }
                else
                {
                    if(Filter(obj))
                    {
                        if (CanShowInHierarchy(obj))
                        {
                            RaiseItemAdded(null, obj);
                        }
                    }                    
                }
            }
        }

        protected virtual void OnObjectEnabled(ExposeToEditor obj)
        {
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveSelf));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveInHierarchy));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.gameObject));
        }

        protected virtual void OnObjectDisabled(ExposeToEditor obj)
        {
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveSelf));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveInHierarchy));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.gameObject));
        }

        protected virtual async void OnObjectDestroying(ExposeToEditor obj)
        {
            if (IsFilterTextEmpty)
            {
                ExposeToEditor parent = obj.GetParent();
                RaiseItemRemoved(parent, obj);

                if (Editor.DragDrop.InProgress)
                {
                    await Task.Yield();
                    if (parent != null)
                    {
                        RefreshInstance(parent.gameObject);
                    }
                }
            }
            else
            {
                RaiseItemRemoved(null, obj);
            }
        }

        protected virtual void OnObjectMarkAsDestroyedChanged(ExposeToEditor obj)
        {
            if (obj.MarkAsDestroyed)
            {
                ExposeToEditor parent = obj.GetParent();
                RaiseItemRemoved(parent, obj);
            }
            else
            {
                if (IsFilterTextEmpty)
                {
                    if (CanShowInHierarchy(obj))
                    {
                        ExposeToEditor parent = obj.GetParent();
                        RaiseItemAdded(parent, obj);
                        SetSiblingIndex(obj);
                    }
                }
                else
                {
                    if (Filter(obj))
                    {
                        if (CanShowInHierarchy(obj))
                        {
                            AddSortedByName(obj);
                        }
                    }
                }
            }
        }

        protected virtual void OnObjectParentChanged(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue)
        {
            if (Editor.IsPlaymodeStateChanging)
            {
                return;
            }

            if (!IsFilterTextEmpty)
            {
                return;
            }

            if (CanShowInHierarchy(obj))
            {
                RaiseParentChanged(oldValue, obj);
            }
            else
            {
                ExposeToEditor parent = obj.GetParent();
                RaiseItemRemoved(parent, obj);
            }
        }

        protected virtual void OnObjectNameChanged(ExposeToEditor obj)
        {
            if (IsPrefabRoot(obj))
            {
                RaisePropertyChanged(nameof(CurrentPrefabName));
            }

            RaiseReset(obj);

            if (IsFilterTextEmpty)
            {
                return;
            }

            if (Filter(obj))
            {
                AddSortedByName(obj);
                SelectedItems = Selection.gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).Where(exposed => exposed != null);
            }
            else
            {
                RaiseParentChanged(null, obj);
            }
        }

        private void OnObjectComponentAdded(ExposeToEditor obj, Component component)
        {
            RefreshInstanceAndParent(obj.gameObject);
        }

        private async void OnObjectComponentDestroyed(ExposeToEditor obj, Component component)
        {
            using var b = Editor.SetBusy();
            await Task.Yield();
            await Task.Yield();
            RefreshInstanceAndParent(obj.gameObject);
        }

        #endregion

        #region AssetDatabaseModel EventHandlers

        private bool m_reloadingProject = false;
        private void OnBeforeReloadProject(object sender, EventArgs e)
        {
            Disable();
            m_reloadingProject = true;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                m_reloadingProject = false;
                Enable();
            }
        }

        private void OnReloadProject(object sender, EventArgs e)
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            m_reloadingProject = false;
            Enable();
        }

        private void OnLoadProject(object sender, EventArgs e)
        {
            Enable();
        }

        private void OnUnloadProject(object sender, EventArgs e)
        {
            Disable();
            BindData();
        }

        private void OnSetDirty(object sender, InstanceEventArgs e)
        {
            RefreshInstanceAndParent(e.Instance);
        }

        private void OnDetach(object sender, InstancesEventArgs e)
        {
            if (Editor.CurrentPrefab != null)
            {
                RefreshInstance(Editor.CurrentPrefab, true);
            }
            else
            {
                foreach (GameObject instance in e.Instances)
                {
                    RefreshInstance(instance, true);
                }   
            }
        }

        private void OnApplyChanges(object sender, InstanceEventArgs e)
        {
            if (Editor.CurrentPrefab != null)
            {
                RefreshInstance(Editor.CurrentPrefab, true);
            }
            else
            {
                RefreshInstance(e.Instance, true);
            }    
        }

        private void OnApplyChangesToBase(object sender, InstanceEventArgs e)
        {
            if (Editor.CurrentPrefab != null)
            {
                RefreshInstance(Editor.CurrentPrefab, true);
            }
            else
            {
                RefreshInstance(e.Instance, true);
            }
        }

        private void OnRevertChangesToBase(object sender, InstanceEventArgs e)
        {
            if (Editor.CurrentPrefab != null)
            {
                RefreshInstance(Editor.CurrentPrefab, true);
            }
            else
            {
                RefreshInstance(e.Instance, true);
            }
        }

        private void OnInitializeNewScene(object sender, EventArgs e)
        {
            CurrentPrefab = Editor.CurrentPrefab;
            BindData();
        }

        private void OnBeforeOpenScene(object sender, AssetEventArgs e)
        {
            Disable();
        }

        private void OnOpenScene(object sender, AssetEventArgs e)
        {
            CurrentPrefab = Editor.CurrentPrefab;
            Enable();
        }

        private void OnBeforeOpenPrefab(object sender, AssetEventArgs e)
        {
            Disable();
        }

        private void OnOpenPrefab(object sender, AssetEventArgs e)
        {
            CurrentPrefab = Editor.CurrentPrefab;
            Enable();
        }

        private void OnBeforeClosePrefab(object sender, AssetEventArgs e)
        {
            Disable();
        }

        private void OnClosePrefab(object sender, AssetEventArgs e)
        {
            CurrentPrefab = Editor.CurrentPrefab;
            Enable();
        }

        #endregion

        #region IHierarchicalData
        public override ExposeToEditor GetParent(ExposeToEditor item)
        {
            return item?.GetParent();
        }

        public override bool HasChildren(ExposeToEditor parent)
        {
            if(!IsFilterTextEmpty)
            {
                return false;
            }

            if (parent == null)
            {
                return Editor.Object.Get(true).Any(obj => !obj.MarkAsDestroyed && CanShowInHierarchy(obj));
            }

            return parent.HasChildren();
        }

        public override IEnumerable<ExposeToEditor> GetChildren(ExposeToEditor parent)
        {
            if (Editor == null || Editor.CompatibilityMode != CompatibilityMode.LegacyRTSL && !Editor.IsProjectLoaded)
            {
                return new ExposeToEditor[0];
            }

            if (parent == null)
            {
                bool useCache = Editor.IsPlaying || m_forceUseCache;
                m_forceUseCache = false;

                IEnumerable<ExposeToEditor> objects = Editor.Object.Get(rootsOnly: IsFilterTextEmpty, useCache);
                if (IsFilterTextEmpty)
                {
                    return objects.Where(obj => CanShowInHierarchy(obj) && obj.IsDescendantOf(Editor.HierarchyRoot)).OrderBy(g => g.transform.GetSiblingIndex());
                }

                return objects.Where(obj => Filter(obj) && CanShowInHierarchy(obj) && obj.IsDescendantOf(Editor.HierarchyRoot)).OrderBy(g => g.name);
            }

            return parent.GetChildren();
        }

        public override int IndexOf(ExposeToEditor parent, ExposeToEditor item)
        {
            if (parent == null)
            {
                return Editor.Object.Get(rootsOnly: true).Where(obj => CanShowInHierarchy(obj) && !obj.MarkAsDestroyed && obj.IsDescendantOf(Editor.HierarchyRoot)).TakeWhile(x => x != item).Count();
            }

            return parent.GetChildren().IndexOf(item);
        }

        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;
            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ExposeToEditor item)
        {
            HierarchicalDataItemFlags flags = HierarchicalDataItemFlags.Default;

            if(!item.CanDelete)
            {
                flags &= ~HierarchicalDataItemFlags.CanRemove;
            }
            
            if(!item.CanRename || !IsFilterTextEmpty)
            {
                flags &= ~HierarchicalDataItemFlags.CanEdit;
            }

            if(!CanDrag(item))
            {
                flags &= ~HierarchicalDataItemFlags.CanDrag;
            }

            if (IsPrefabRoot(item))
            {
                flags &= ~HierarchicalDataItemFlags.CanBeSibling;
                flags &= ~HierarchicalDataItemFlags.CanRemove;
            }

            return flags;
        }
  
        #endregion

        #region Bound UnityEvent Handlers
        public override void OnSelectAll()
        {
            SelectedItems = GetExpandedItems();
        }
    
        public override void OnDelete()
        {
            Editor.Delete(Editor.Selection.gameObjects);
        }

        public override void OnDuplicate()
        {
            Editor.Duplicate(Editor.Selection.gameObjects);
        }

        public override async void OnExternalObjectDrop()
        {
            if (!AllowDropExternalObjects())
            {
                CanDropExternalObjects = false;
                return;
            }

            var assetIDs = ExternalDragObjects.OfType<IAsset>().Where(item => Editor.GetType(item.ID) == typeof(GameObject)).Select(item => item.ID).ToArray();
            if (assetIDs.Length > 0)
            {
                using var b = SetBusy();
                try
                {
                    Transform parent = null;

                    var targetItem = TargetItem;
                    if (targetItem != null)
                    {
                        if (LastDragDropAction == DragDropAction.SetLastChild)
                        {
                            parent = targetItem.transform;
                        }
                        else
                        {
                            var exposeToEditorParent = targetItem.GetParent();
                            if(exposeToEditorParent != null)
                            {
                                parent = exposeToEditorParent.transform;
                            }
                            else
                            {
                                parent = Editor.HierarchyRoot.transform;
                            }
                        }
                    }

                    var result = await Editor.InstantiateAssetsAsync(assetIDs, parent); 
                    if (result.IsCyclicNestingDetected)
                    {
                        ShowCyclicNestingDetected();
                    }
                    else
                    {
                        var createObjects = result.Instances.Select(instance => instance.GetComponent<ExposeToEditor>()).ToArray();
                        if (LastDragDropAction == DragDropAction.SetLastChild)
                        {
                            Expand(targetItem);
                        }
                        else if (LastDragDropAction == DragDropAction.SetNextSibling)
                        {
                            foreach (ExposeToEditor obj in createObjects)
                            {
                                obj.transform.SetSiblingIndex(TargetItem.transform.GetSiblingIndex() + 1);
                                RaiseNextSiblingChanged(obj, targetItem);
                            }
                        }
                        else if (LastDragDropAction == DragDropAction.SetPrevSilbling)
                        {
                            foreach (ExposeToEditor obj in createObjects)
                            {
                                obj.transform.SetSiblingIndex(TargetItem.transform.GetSiblingIndex());
                                RaisePrevSiblingChanged(obj, targetItem);
                            }
                        }
                       
                        Undo.BeginRecord();
                        Undo.RegisterCreatedObjects(createObjects);
                        var placement = IOC.Resolve<IPlacementModel>();
                        var selectionComponent = placement.GetSelectionComponent();
                        if (selectionComponent == null || selectionComponent.CanSelect)
                        {
                            Selection.objects = result.Instances;
                        }
                        Undo.EndRecord();
                    }   
                }
                catch (Exception e)
                {
                    ShowUnableToInstantiateAssets(e);
                    return;
                }
                finally
                {
                    CanDropExternalObjects = false;
                }
            }
            else
            {
                #pragma warning disable CS0612 // Type or member is obsolete
                LegacyProjectItemsDragDropHandler();
                #pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        public override void OnItemsBeginDrop()
        {
            if (!CanDrop(TargetItem, SourceItems))
            {
                TargetItem = null;
                return;
            }

            IEnumerable<ExposeToEditor> dragObjects = SourceItems;
            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                dragObjects = SourceItems.Where(item => item.GetParent() != TargetItem).ToArray();
            }

            foreach (ExposeToEditor item in dragObjects)
            {
                if (Editor.IsCyclicNesting(item.gameObject, TargetItem.transform))
                {
                    TargetItem = null;
                    ShowCyclicNestingDetected();
                    return;
                }
            }

            Undo.BeginRecord();
            Undo.CreateRecord(null, null, false,
                record => RefreshTree(record, true),
                record => RefreshTree(record, false));

            if (LastDragDropAction == DragDropAction.SetLastChild || dragObjects.Any(d => !ReferenceEquals(d.GetParent(), TargetItem?.GetParent())))
            {
                foreach (ExposeToEditor exposed in dragObjects.Reverse())
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }
            else
            {
                Transform dropT = TargetItem.transform;
                int dropTIndex = dropT.GetSiblingIndex();

                foreach (ExposeToEditor exposed in dragObjects
                    .Where(o => o.transform.GetSiblingIndex() > dropTIndex)
                    .OrderBy(o => o.transform.GetSiblingIndex())
                    .Union(dragObjects
                        .Where(o => o.transform.GetSiblingIndex() < dropTIndex)
                        .OrderByDescending(o => o.transform.GetSiblingIndex())))
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }

            Undo.EndRecord();
        }

        public override void OnItemsDrop()
        {
            base.OnItemsDrop();

            Transform dropT = TargetItem.transform;
            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                var dragObjects = SourceItems.Where(item => item.GetParent() != TargetItem).ToArray();

                Undo.BeginRecord();
                foreach(ExposeToEditor dragObject in dragObjects)
                {
                    Transform dragT = dragObject.transform;
                    dragT.SetParent(dropT, true);
                    dragT.SetAsLastSibling();

                    Undo.EndRecordTransform(dragT, dropT, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                   record => RefreshTree(record, true),
                   record => RefreshTree(record, false));
                Undo.EndRecord();
            }
            else if (LastDragDropAction == DragDropAction.SetNextSibling)
            {
                Undo.BeginRecord();

                var dragObjects = SourceItems.ToArray();
                for (int i = dragObjects.Length - 1; i >= 0; --i)
                {
                    ExposeToEditor dragObject = dragObjects[i];
                    Transform dragT = dragObject.transform;

                    int dropTIndex = dropT.GetSiblingIndex();
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                        dragT.SetSiblingIndex(dropTIndex + 1);
                    }
                    else
                    {
                        int dragTIndex = dragT.GetSiblingIndex();
                        if (dropTIndex < dragTIndex)
                        {
                            dragT.SetSiblingIndex(dropTIndex + 1);
                        }
                        else
                        {
                            dragT.SetSiblingIndex(dropTIndex);
                        }
                    }
                    Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Undo.EndRecord();

            }
            else if (LastDragDropAction == DragDropAction.SetPrevSilbling)
            {
                Undo.BeginRecord();
                foreach (ExposeToEditor dragObject in SourceItems)
                { 
                    Transform dragT = dragObject.transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int dropTIndex = dropT.GetSiblingIndex();
                    int dragTIndex = dragT.GetSiblingIndex();
                    if (dropTIndex > dragTIndex)
                    {
                        dragT.SetSiblingIndex(dropTIndex - 1);
                    }
                    else
                    {
                        dragT.SetSiblingIndex(dropTIndex);
                    }

                    Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Undo.EndRecord();
            }
        }

        public override void OnItemHold()
        {
            Selection.activeObject = TargetItem.gameObject;
            OpenContextMenu();
        }

        public override void OnItemBeginEdit()
        {
            base.OnItemBeginEdit();
            if (Selection.activeGameObject != null)
            {
                Undo.BeginRecordValue(Selection.activeGameObject.GetComponent<ExposeToEditor>(), Strong.MemberInfo((ExposeToEditor x) => x.Name));
            }
        }

        public override void OnItemEndEdit()
        {
            base.OnItemEndEdit();
            if (Selection.activeGameObject != null)
            {
                Undo.EndRecordValue(Selection.activeGameObject.GetComponent<ExposeToEditor>(), Strong.MemberInfo((ExposeToEditor x) => x.Name));
            }
        }

        [Binding]
        public async void OnCloseCurrentPrefab()
        {
            using var b = SetBusy();
            await Editor.ClosePrefabAsync();
        }

        #endregion

        #region Context Menu
        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            if (!Editor.IsProjectLoaded)
            {
                return;
            }

            if (HasSelectedItems && SelectedItems.Count() == 1)
            {
                var targetGameObject = SelectedItem.gameObject;
                if (Editor.IsInstance(targetGameObject))
                {
                    if (ShowSelectPrefabContextMenuCmd())
                    {
                        var selectPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabSelect", "Prefab/Select") };
                        selectPrefab.Action = SelectPrefabContextMenuCmd;
                        menuItems.Add(selectPrefab);
                    }

                    if (ShowOpenPrefabContextMenuCmd())
                    {
                        var openPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabOpen", "Prefab/Open") };
                        openPrefab.Action = OpenPrefabContextMenuCmd;                
                        menuItems.Add(openPrefab);
                    }

                    if (ShowCreateOriginalPrefabContextMenuCmd())
                    {
                        var createOriginalPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabCreate", "Prefab/Create") };
                        createOriginalPrefab.Action = CreatePrefabContextMenuCmd;
                        createOriginalPrefab.Validate = CreatePrefabValidateContextMenuCmd;
                        createOriginalPrefab.Command = "Original";
                        menuItems.Add(createOriginalPrefab);
                    }
                    
                    if (ShowCreatePrefabVariantContextMenuCmd())
                    {
                        var createPrefabVariant = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabCreateVariant", "Prefab/Create Variant") };
                        createPrefabVariant.Action = CreatePrefabContextMenuCmd;
                        createPrefabVariant.Validate = CreatePrefabValidateContextMenuCmd;
                        createPrefabVariant.Command = "Variant";
                        menuItems.Add(createPrefabVariant);
                    }

                    if (ShowUnpackPrefabContextMenuCmd())
                    {
                        var unpackPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabUnpack", "Prefab/Unpack") };
                        unpackPrefab.Action = UnpackPrefabContextMenuCmd;
                        menuItems.Add(unpackPrefab);
                    }

                    if (ShowUnpackPrefabCompletelyContextMenuCmd())
                    {
                        var unpackPrefabCompletely = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabUnpackCompletely", "Prefab/Unpack Completely") };
                        unpackPrefabCompletely.Action = UnpackPrefabContextMenuCmd;
                        unpackPrefabCompletely.Command = "Completely";
                        menuItems.Add(unpackPrefabCompletely);
                    }

                    if (ShowApplyChangesContextMenuCmd())
                    {
                        var applyChanges = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabApplyChanges", "Prefab/Apply Changes") };
                        applyChanges.Action = ApplyChangesContextMenuCmd;
                        menuItems.Add(applyChanges);
                    }

                    if (ShowApplyChangesToBaseContextMenuCmd())
                    {
                        var applyToBase = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabApplyToBase", "Prefab/Apply To Base") };
                        applyToBase.Action = ApplyChangesToBaseContextMenuCmd;
                        menuItems.Add(applyToBase);
                    }

                    if (ShowRevertChangesToBaseContextMenuCmd())
                    {
                        var revertToBase = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_PrefabRevertToBase", "Prefab/Revert To Base") };
                        revertToBase.Action = RevertChangesToBaseContextMenuCmd;
                        menuItems.Add(revertToBase);
                    }
                }
                else
                {
                    var createPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_CreatePrefab", "Create Prefab") };
                    createPrefab.Action = CreatePrefabContextMenuCmd;
                    createPrefab.Validate = CreatePrefabValidateContextMenuCmd;
                    menuItems.Add(createPrefab);
                }
            }

            var group = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_Group", "Grouping/Group") };
            group.Action = GroupContextMenuCmd;
            group.Validate = GroupValidateContextMenuCmd;
            menuItems.Add(group);

            var groupLocal = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_GroupLocal", "Grouping/Group (Local)"), Command = "Local" };
            groupLocal.Action = GroupContextMenuCmd;
            groupLocal.Validate = GroupValidateContextMenuCmd;
            menuItems.Add(groupLocal);

            var ungroup = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_Ungroup", "Grouping/Ungroup") };
            ungroup.Action = UngroupContextMenuCmd;
            ungroup.Validate = UngroupValidateContextMenuCmd;
            menuItems.Add(ungroup);

            var duplicate = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Duplicate", "Duplicate") };
            duplicate.Action = DuplicateContextMenuCmd;
            duplicate.Validate = DuplicateValidateContextMenuCmd;
            menuItems.Add(duplicate);

            var delete = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Delete", "Delete") };
            delete.Action = DeleteContextMenuCmd;
            delete.Validate = DeleteValidateContextMenuCmd;
            menuItems.Add(delete);

            var rename = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Rename", "Rename") };
            rename.Action = RenameContextMenuCmd;
            rename.Validate = RenameValidateContextMenuCmd;
            menuItems.Add(rename);
        }
        
        protected virtual bool ShowSelectPrefabContextMenuCmd()
        {
            return Editor.CanSelectPrefab(SelectedItem.gameObject);
        }

        protected virtual async void SelectPrefabContextMenuCmd(string arg)
        {
            await Editor.SelectPrefabAsync(SelectedItem.gameObject);
        }

        protected virtual bool ShowOpenPrefabContextMenuCmd()
        {
           return Editor.CanOpenPrefab(SelectedItem.gameObject);
        }

        protected virtual async void OpenPrefabContextMenuCmd(string arg)
        {
            await Editor.OpenPrefabAsync(SelectedItem.gameObject);
        }

        protected virtual bool ShowCreateOriginalPrefabContextMenuCmd()
        {
            return Editor.CanCreatePrefab(SelectedItem.gameObject);
        }

        protected virtual bool ShowCreatePrefabVariantContextMenuCmd()
        {
            return Editor.CanCreatePrefabVariant(SelectedItem.gameObject);
        }

        protected virtual void CreatePrefabValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems)
            {
                args.IsValid = false;
            }
        }

        protected virtual async void CreatePrefabContextMenuCmd(string arg)
        {
            bool? variant = null;
            if (arg == "Original")
            {
                variant = false;
            }
            else if (arg == "Variant")
            {
                variant = true;
            }

            await Editor.CreateAssetAsync(SelectedItem.gameObject, variant:variant);
        }

        protected virtual bool ShowUnpackPrefabContextMenuCmd()
        {
            return Editor.CanDetach(new[] { SelectedItem.gameObject });
        }

        protected virtual bool ShowUnpackPrefabCompletelyContextMenuCmd()
        {
            return Editor.CanDetach(new[] { SelectedItem.gameObject });
        }

        protected virtual async void UnpackPrefabContextMenuCmd(string arg)
        {
            bool completely = false;
            if (arg == "Completely")
            {
                completely = true;
            }
           
            await Editor.DetachAsync(new[] { SelectedItem.gameObject }, completely:completely);
        }

        protected virtual bool ShowApplyChangesContextMenuCmd()
        {
            return Editor.CanApplyChanges(SelectedItem.gameObject);
        }

        protected virtual async void ApplyChangesContextMenuCmd(string arg)
        {
            await Editor.ApplyChangesAsync(SelectedItem.gameObject);
        }

        protected virtual bool ShowApplyChangesToBaseContextMenuCmd()
        {
            return Editor.CanApplyToBase(SelectedItem.gameObject);
        }

        protected virtual async void ApplyChangesToBaseContextMenuCmd(string arg)
        {
            await Editor.ApplyToBaseAsync(SelectedItem.gameObject);
        }

        protected virtual bool ShowRevertChangesToBaseContextMenuCmd()
        {
            return Editor.CanRevertToBase(SelectedItem.gameObject);
        }

        protected virtual async void RevertChangesToBaseContextMenuCmd(string arg)
        {
            await Editor.RevertToBaseAsync(SelectedItem.gameObject);
        }

        protected virtual void DuplicateValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems || 
                !SelectedItems.Any(o => o.CanDuplicate && !IsPrefabRoot(o)) || 
                !Editor.CanDuplicate(SelectedItems.Select(o => o.gameObject).ToArray()))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DuplicateContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedItems.Select(o => o.gameObject).ToArray();
            Editor.Duplicate(gameObjects);
        }

        protected virtual void DeleteValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems || 
                !SelectedItems.Any(o => o.CanDelete && !IsPrefabRoot(o)) ||
                !Editor.CanRelease(SelectedItems.Select(o => o.gameObject).ToArray()))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedItems.Select(o => o.gameObject).ToArray();
            Editor.Delete(gameObjects);
        }

        protected virtual void RenameValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems || !SelectedItems.First().CanRename)
            {
                args.IsValid = false;
            }
        }

        protected virtual void RenameContextMenuCmd(string arg)
        {
            IsEditing = true;
        }

        protected virtual void GroupValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems || !m_groupingModel.CanGroup(SelectedGameObjects))
            {
                args.IsValid = false;
            }
        }

        protected virtual void GroupContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedGameObjects;
            WindowManager.Prompt(
                Localization.GetString("ID_RTEditor_HierarchyViewModel_EnterGroupName", "Enter Group Name"),
                Localization.GetString("ID_RTEditor_HierarchyViewModel_DefaultGroupName", "Group"),
                (sender, args) =>
                {
                    string groupName = args.Text;
                    m_groupingModel.GroupAndRecord(gameObjects, groupName, arg == "Local");
                },
                (sender, args) => { });;
        }

        protected virtual void UngroupValidateContextMenuCmd(ContextMenuItem.ValidationArgs args)
        {
            if (!HasSelectedItems || !m_groupingModel.CanUngroup(SelectedGameObjects))
            {
                args.IsValid = false;
            }
        }

        protected virtual void UngroupContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedGameObjects;
            m_groupingModel.UngroupAndRecord(gameObjects);
        }

        #endregion

        #region Methods

        private void ShowUnableToInstantiateAssets(Exception e)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.MessageBox(Localization.GetString("ID_RTEditor_SceneView_UnableToLoadAssetItems", "Unable to instantiate assets"), e.Message);
            Debug.LogException(e);
        }

        private void ShowCyclicNestingDetected()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.MessageBox(
                Localization.GetString("ID_RTEditor_SceneView_CyclicNestingDetected", "Cyclic nesting detected"), 
                Localization.GetString("ID_RTEditor_SceneView_CyclicNestingNotSupported", "Cyclic nesting of prefabs is not supported"));
        }

        public bool IsPrefabRoot(ExposeToEditor obj)
        {
            return Editor.CurrentPrefab != null && Editor.CurrentPrefab == obj.gameObject;
        }

        protected virtual bool CanShowInHierarchy(ExposeToEditor obj)
        {
            if (Editor.HierarchyRoot != null && obj.transform.parent == null)
            {
                return false;
            }

            return !obj.hideFlags.HasFlag(HideFlags.HideInHierarchy);
        }

        protected virtual bool CanDrag(ExposeToEditor obj)
        {
            return IsFilterTextEmpty;
        }

        protected virtual bool CanDrop(ExposeToEditor dropTarget, IEnumerable<object> dragItems)
        {
            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                if(!dragItems.OfType<ExposeToEditor>().Where(item => item.GetParent() != dropTarget).Any())
                {
                    return false;
                }
            }

            if (LastDragDropAction == DragDropAction.None)
            {
                return true;
            }

            return dropTarget != null && !IsPrefabRoot(dropTarget);
        }

        protected override bool AllowDropExternalObjects()
        {
            if (TargetItem == null)
            {
                return true;
            }

            if (!IsFilterTextEmpty)
            {
                return false;
            }

            #pragma warning disable CS0612 // Type or member is obsolete
            if (AllowDropLegacyProjectItems())
            {
                return true;
            }
            #pragma warning restore CS0612 // Type or member is obsolete

            var items = ExternalDragObjects.OfType<IAsset>();
            return items.Any(item => !Editor.IsFolder(item.ID) && Editor.GetType(item.ID) == typeof(GameObject));
        }

        protected GameObject[] GetGameObjects(IEnumerable<ExposeToEditor> exposedToEditor)
        {
            if (exposedToEditor == null)
            {
                return null;
            }

            return exposedToEditor.Select(e => e.gameObject).ToArray();
        }

        protected ExposeToEditor[] GetExposedToEditor(GameObject[] gameObjects)
        {
            if (gameObjects == null)
            {
                return null;
            }

            return gameObjects.Select(g => g.GetComponent<ExposeToEditor>()).Where(e => e != null).ToArray();
        }

        protected override void OnSelectedItemsChanged(IEnumerable<ExposeToEditor> unselectedObjects, IEnumerable<ExposeToEditor> selectedObjects)
        {
            Selection.objects = GetGameObjects(selectedObjects);
        }

        protected void EditorSelectionToSelectedObjects()
        {
            m_selectedItems = GetExposedToEditor(Selection.gameObjects);
            RaiseSelect(m_selectedItems);

            if (SelectedItems != null)
            {
                foreach (ExposeToEditor selectedObject in SelectedItems)
                {
                    if (!IsExpanded(GetParent(selectedObject)))
                    {
                        ExpandTo(selectedObject);
                    }
                }
            }
       }

        private List<GameObject> m_rootGameObjects;
        protected void SetSiblingIndex(ExposeToEditor obj)
        {
            if (obj.transform.parent == null && m_rootGameObjects == null)
            {
                m_rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects().OrderBy(g => g.transform.GetSiblingIndex()).ToList();
            }

            ExposeToEditor nextSibling = obj.NextSibling(m_rootGameObjects);
            if (nextSibling != null)
            {
                RaisePrevSiblingChanged(obj, nextSibling);
            }
        }

        protected virtual void AddSortedByName(ExposeToEditor obj)
        {
            IEnumerable<ExposeToEditor> items = DataSource.GetChildren(null);
            string[] names = items.Select(go => go.name).Union(new[] { obj.name }).OrderBy(k => k).ToArray();
            int index = Array.IndexOf(names, obj.name);
            ExposeToEditor sibling;
            if (index == 0)
            {
                sibling = items.FirstOrDefault();
                RaiseItemAdded(null, obj);
                if (sibling != null)
                {
                    RaisePrevSiblingChanged(obj, sibling);
                }
            }
            else
            {
                sibling = items.ElementAt(index - 1);
                RaiseItemAdded(null, obj);
                RaiseNextSiblingChanged(obj, sibling);
            }
        }

        protected override void ExpandTo(ExposeToEditor item)
        {
            if(!IsFilterTextEmpty)
            {
                return;
            }

            base.ExpandTo(item);
        }

        protected virtual bool RefreshTree(Record record, bool isRedo)
        {
            bool applyOnRedo = (bool)record.OldState;
            if (applyOnRedo != isRedo)
            {
                return false;
            }

            BindData();
            EditorSelectionToSelectedObjects();
            
            if (SelectedItems != null)
            {
                foreach (ExposeToEditor obj in SelectedItems.OfType<ExposeToEditor>().OrderBy(o => o.transform.GetSiblingIndex()))
                {
                    ExpandTo(obj);
                }
            }

            return false;
        }

        private void RefreshInstanceAndParent(GameObject instance)
        {
            RefreshInstance(instance);

            var parent = instance.transform.parent;
            if (parent != null && Editor.IsInstance(parent))
            {
                RefreshInstance(parent.gameObject);
            }
        }

        private static void RefreshInstance(GameObject instance, bool recursive = false)
        {
            var exposeToEditor = instance.GetComponent<ExposeToEditor>();
            exposeToEditor?.RaisePropertyChanged(nameof(ExposeToEditor.gameObject));

            if (recursive)
            {
                Transform transform = instance.transform;
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    RefreshInstance(child.gameObject, recursive);
                }
            }
        }

        #endregion

        #region Legacy

        [Obsolete]
        protected RTSL.Interface.IProjectAsync Project
        {
            get { return IOC.Resolve<RTSL.Interface.IProjectAsync>(); }
        }

        [Obsolete]
        protected virtual void OnSceneLoading()
        {
            Disable();
        }

        [Obsolete]
        protected virtual void OnSceneLoaded()
        {
            Enable();
        }

        [Obsolete]
        private bool AllowDropLegacyProjectItems()
        {
            var projectItems = ExternalDragObjects.OfType<RTSL.Interface.ProjectItem>();
            return projectItems.Any(projectItem => !projectItem.IsFolder && Project.Utils.ToType(projectItem) == typeof(GameObject));
        }

        [Obsolete]
        private async void LegacyProjectItemsDragDropHandler()
        {
            var projectItems = ExternalDragObjects.OfType<RTSL.Interface.ProjectItem>().Where(item => Project.Utils.ToType(item) == typeof(GameObject)).ToArray();
            if (projectItems.Length > 0)
            {
                Editor.IsBusy = true;
                UnityObject[] objects;
                try
                {
                    objects = await Project.Safe.LoadAsync(projectItems);
                }
                catch (Exception e)
                {
                    ShowUnableToInstantiateAssets(e);
                    CanDropExternalObjects = false;
                    return;
                }
                finally
                {
                    Editor.IsBusy = false;
                }

                try
                {
                    OnProjectItemsLoaded(objects, TargetItem);
                }
                finally
                {
                    CanDropExternalObjects = false;
                }
            }
        }

        [Obsolete]
        protected virtual void OnProjectItemsLoaded(UnityObject[] objects, ExposeToEditor dropTarget)
        {
            m_selectionComponent = m_placementModel.GetSelectionComponent();

            GameObject[] createdObjects = new GameObject[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                GameObject prefab = (GameObject)objects[i];
                bool wasPrefabEnabled = prefab.activeSelf;
                prefab.SetActive(false);

                GameObject prefabInstance = InstantiatePrefab(prefab);
                prefabInstance.hideFlags = HideFlags.None;
                prefabInstance.name = prefab.name;

                AddPrefabInstance(dropTarget, prefabInstance);
                prefab.SetActive(wasPrefabEnabled);

                createdObjects[i] = prefabInstance;
            }

            if (createdObjects.Length > 0)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                editor.RegisterCreatedObjects(createdObjects, m_selectionComponent != null ? m_selectionComponent.CanSelect : true);
            }
        }

        [Obsolete]
        private void AddPrefabInstance(ExposeToEditor parent, GameObject instance)
        {
            Editor.AddGameObjectToHierarchy(instance);

            ExposeToEditor exposeToEditor = ExposePrefabInstance(instance);

            if (parent == null)
            {
                exposeToEditor.transform.SetParent(Editor.HierarchyRoot != null ? Editor.HierarchyRoot.transform : null);
                RaiseItemAdded(null, exposeToEditor);
            }
            else
            {
                if (LastDragDropAction == DragDropAction.SetLastChild)
                {
                    exposeToEditor.transform.SetParent(parent.transform);
                    RaiseItemAdded(parent, exposeToEditor);

                    Expand(parent);
                }
                else if (LastDragDropAction == DragDropAction.SetNextSibling)
                {
                    ExposeToEditor dropTargetParent = parent.GetParent();

                    exposeToEditor.transform.SetParent(dropTargetParent != null ? dropTargetParent.transform : null, false);
                    exposeToEditor.transform.SetSiblingIndex(parent.transform.GetSiblingIndex() + 1);

                    RaiseItemAdded(parent.GetParent(), exposeToEditor);
                    RaiseNextSiblingChanged(exposeToEditor, parent);
                }
                else if (LastDragDropAction == DragDropAction.SetPrevSilbling)
                {
                    ExposeToEditor dropTargetParent = parent.GetParent();

                    exposeToEditor.transform.SetParent(dropTargetParent != null ? dropTargetParent.transform : null, false);
                    exposeToEditor.transform.SetSiblingIndex(parent.transform.GetSiblingIndex());

                    RaiseItemAdded(parent.GetParent(), exposeToEditor);
                    RaisePrevSiblingChanged(exposeToEditor, parent);
                }
            }

            OnActivatePrefabInstance(instance);
        }

        [Obsolete]
        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {
            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }
            return exposeToEditor;
        }

        [Obsolete]
        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            prefabInstance.SetActive(true);
        }

        [Obsolete]
        protected virtual GameObject InstantiatePrefab(GameObject prefab)
        {
            Vector3 pivot = Vector3.zero;
            if (m_selectionComponent != null)
            {
                pivot = m_selectionComponent.SecondaryPivot;
            }

            return Instantiate(prefab, pivot, Quaternion.identity);
        }

        #endregion
    }

}
