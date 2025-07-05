using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    [Flags]
    public enum SelectObjectSource
    {
        Scene = 1,
        AssetDatabase = 2,
    }

    public interface ISelectObjectDialog
    {
        Type ObjectType
        {
            get;
            set;
        }

        SelectObjectSource ObjectSource
        {
            get;
            set;
        }

        bool IsNoneSelected
        {
            get;
        }

        UnityObject SelectedObject
        {
            get;
        }
    }
}

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetDatabaseSelectViewModel : HierarchicalDataViewModel<AssetViewModel>, ISelectObjectDialog
    {
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

        #region ISelectObjectDialog
        public Type ObjectType
        {
            get;
            set;
        }

        private SelectObjectSource m_objectSource = SelectObjectSource.Scene | SelectObjectSource.AssetDatabase;
        public SelectObjectSource ObjectSource
        {
            get { return m_objectSource; }
            set
            {
                if (m_objectSource != value)
                {
                    m_objectSource = value;

                    if (m_objectSource == SelectObjectSource.Scene)
                    {
                        m_isAssetsTabSelected = false;
                    }
                    else if (m_objectSource == SelectObjectSource.AssetDatabase)
                    {
                        m_isAssetsTabSelected = true;
                    }

                    RaisePropertyChanged(nameof(IsHeaderVisible));
                }

            }
        }

        public bool IsNoneSelected
        {
            get;
            protected set;
        }

        protected UnityObject m_selectedUnityObject;
        UnityObject ISelectObjectDialog.SelectedObject
        {
            get { return m_selectedUnityObject; }
        }
        #endregion


        [Binding]
        public bool IsHeaderVisible
        {
            get
            {
                return m_objectSource.HasFlag(SelectObjectSource.Scene) &&
                       m_objectSource.HasFlag(SelectObjectSource.AssetDatabase);
            }
           
        }

        private bool m_isAssetsTabSelected = true;
        
        [Binding]
        public bool IsAssetsTabSelected
        {
            get { return m_isAssetsTabSelected; }
            set
            {
                if(m_isAssetsTabSelected != value)
                {
                    m_isAssetsTabSelected = value;
                    OnAssetsTabSelectionChanged();
                }
            }
        }

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
                    RaisePropertyChanged(nameof(FilterText));
                    BindData();
                }
            }
        }

        private AssetViewModel[] m_items;
        private AssetViewModel[] m_assetsCache;
        private AssetViewModel[] m_sceneCache;
        private Dictionary<ID, UnityObject> m_sceneObjects;
        private AssetViewModel m_noneAsset;
 
        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ISelectObjectDialog>(this);
        }

        protected override async void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetDatabaseSelect_Select", "Select"),
                CancelText = Localization.GetString("ID_RTEditor_AssetDatabaseSelect_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            await LoadDataAsync();
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            IOC.UnregisterFallback<ISelectObjectDialog>(this);

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs e)
        {
            e.Cancel = !CanClose();
        }
        #endregion

        #region Bound UnityEvent Handlers
        protected virtual void OnAssetsTabSelectionChanged()
        {
            BindData();
        }

        protected override async void OnSelectedItemsChanged(IEnumerable<AssetViewModel> unselectedObjects, IEnumerable<AssetViewModel> selectedObjects)
        {
            AssetViewModel item = selectedObjects.FirstOrDefault();
            await HandleSelectionChangedAsync(item);
        }

        public override async void OnItemDoubleClick()
        {
            AssetViewModel item = TargetItem;
            if (item != null && item == m_noneAsset)
            {
                IsNoneSelected = true;
                m_selectedUnityObject = null;
                m_parentDialog?.Close(true);
            }
            else
            {
                IsNoneSelected = false;
                if (item != null)
                {
                    if (m_sceneObjects.ContainsKey(item.ID))
                    {
                        m_selectedUnityObject = m_sceneObjects[item.ID];
                        m_parentDialog?.Close(true);
                    }
                    else
                    {
                        m_selectedUnityObject = null;
                        using var b = Editor.SetBusy();
                        m_selectedUnityObject = await Editor.LoadAssetAsync(item.ID) as UnityObject;
                        m_parentDialog?.Close(true);
                    }
                }
                else
                {
                    m_selectedUnityObject = null;
                    m_parentDialog?.Close(true);
                }
            }
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

        public override IEnumerable<AssetViewModel> GetChildren(AssetViewModel parent)
        {
            ApplyFilter();
            return m_items;
        }
        #endregion

        #region Methods
    
        protected virtual bool CanDisplay(ID id)
        {  
            if (Editor.IsFolder(id))
            {
                return false;
            }

            var type = Editor.GetType(id);
            if (type == null)
            {
                return false;
            }

            var name = Editor.GetName(id);
            return CanDisplay(name, type);
        }

        protected virtual bool CanDisplay(string name, Type type)
        {
            if (!CanDisplay(name))
            {
                return false;
            }

            return type == ObjectType || type.IsSubclassOf(ObjectType);
        }

        protected virtual bool CanDisplay(string name)
        {
            return !name.StartsWith(".");
        }

        protected virtual async Task LoadDataAsync()
        {
            ParentDialog.IsOkInteractable = false;

            var iteToItem = new Dictionary<ID, AssetViewModel>();
            GetItems(Editor.RootFolderID, iteToItem);
           
            m_noneAsset = new AssetViewModel(ID.NewID(), Localization.GetString("ID_RTEditor_AssetDatabaseSelect_None", "None"));
           
            var items = new[] { m_noneAsset }.Union(iteToItem.Values).ToArray();
            m_assetsCache = items;

         
            ParentDialog.IsOkInteractable = SelectedItem != null;
            Editor.IsBusy = false;
            
            m_sceneObjects = new Dictionary<ID, UnityObject>();
            
            var sceneCache = new List<AssetViewModel> { m_noneAsset };
            var sceneObjects = Editor.Object.Get(false, true).ToArray();
            var hs = new HashSet<UnityObject>();
            for (int i = 0; i < sceneObjects.Length; ++i)
            {
                ExposeToEditor exposeToEditor = sceneObjects[i];
                if(exposeToEditor == null)
                {
                    continue;
                }

                if (ObjectType == typeof(GameObject))
                {
                    AddSceneObject(sceneCache, exposeToEditor.gameObject);
                }
                else if (ObjectType.IsSubclassOf(typeof(Component)))
                {
                    Component obj = exposeToEditor.GetComponent(ObjectType);
                    if (obj != null)
                    {
                        AddSceneObject(sceneCache, obj);
                    }
                }
                else
                {
                    var subAssets = await Editor.ExtractSubAssetsAsync(exposeToEditor.gameObject);
                    foreach (var subAsset in subAssets)
                    {
                        var obj = subAsset as UnityObject;
                        if (obj == null)
                        {
                            continue;
                        }

                        var type = subAsset.GetType();
                        var name = GetName(subAsset, type);
                        if (hs.Add(obj) && CanDisplay(name, type))
                        {
                            AddSceneObject(sceneCache, obj);
                        }
                    }
                }
            }

            m_sceneCache = sceneCache.ToArray();

            BindData();
            RefreshThumbnails();

            await HandleSelectionChangedAsync(SelectedItem);
        }

        private async void GetItems(ID id, Dictionary<ID, AssetViewModel> items)
        {
            if (CanDisplay(id))
            {
                if (!items.ContainsKey(id))
                {
                    var item = new AssetViewModel(id, Editor.GetName(id), Editor.ThumbnailUtil.GetBuiltinThumbnail(id, large: true));
                    items.Add(item.ID, item);
                }
            }
           
            if (Editor.IsLoaded(id))
            {
                await GetSubAssets(Editor.GetAsset(id), items);
            }

            var children = Editor.GetChildren(id, sortByName: false);
            foreach (ID childID in children)
            {
                if(CanDisplay(Editor.GetName(childID)))
                {
                    GetItems(childID, items);
                }
            }
        }

        private async Task GetSubAssets(object obj, Dictionary<ID, AssetViewModel> items)
        {
            var subAssets = await Editor.ExtractSubAssetsAsync(obj, new ExtractSubAssetOptions { IncludeExisting = true, IncludeExternal = true });
            foreach (var subAsset in subAssets)
            {
                string name;
                var type = subAsset.GetType();
                var id = Editor.GetAssetID(subAsset);
                var subAssetID = Editor.GetSubAssetID(subAsset);
                if (subAssetID == ID.Empty)
                {
                    subAssetID = ID.NewID();
                }

                if (id == subAssetID)
                {
                    name = Editor.GetName(subAssetID);
                }
                else
                {
                    name = GetName(subAsset, type);
                }

                if (!items.ContainsKey(subAssetID) && CanDisplay(name, type))
                {
                    var thumbnail = Editor.ThumbnailUtil.GetBuiltinThumbnail(type, large: true);
                    var item = new AssetViewModel(subAssetID, name, thumbnail);

                    items.Add(subAssetID, item);
                }
            }
        }

        private static string GetName(object subAsset, Type type)
        {
            string name;
            UnityObject uo = subAsset as UnityObject;
            if (uo != null)
            {
                name = uo.name;
            }
            else
            {
                name = type.Name;
            }

            return name;
        }

        private void AddSceneObject(List<AssetViewModel> sceneCache, UnityObject obj)
        {
            if (Editor.IsAsset(obj))
            {
                return;
            }

            var id = ID.NewID();
            
            var item = new AssetViewModel(id, obj.name, Editor.ThumbnailUtil.GetBuiltinThumbnail(obj.GetType()));
            sceneCache.Add(item);

            m_sceneObjects.Add(id, obj);
        }

        private async void RefreshThumbnails()
        {
            var settings = IOC.Resolve<ISettingsComponent>();
            var noneSprite = settings.SelectedTheme.GetIcon("None");
            if (noneSprite != null)
            {
                m_noneAsset.Thumbnail = noneSprite.texture;
            }

            if (m_items == null)
            {
                return;
            }

            foreach (var item in m_items.ToArray())
            {
                if (!isActiveAndEnabled)
                {
                    return;
                }

                if (item == m_noneAsset)
                {
                    continue;
                }

                item.Thumbnail = await Editor.ThumbnailUtil.LoadThumbnailAsync(item.ID, large: true);
            }
        }

        protected virtual async Task HandleSelectionChangedAsync(AssetViewModel item)
        {
            if (item != null && item == m_noneAsset)
            {
                IsNoneSelected = true;
                m_selectedUnityObject = null;
            }
            else
            {
                IsNoneSelected = false;

                if (item != null)
                {
                    if (m_sceneObjects.ContainsKey(item.ID))
                    {
                        m_selectedUnityObject = m_sceneObjects[item.ID];
                    }
                    else
                    {
                        using var b = Editor.SetBusy();
                        m_selectedUnityObject = await Editor.LoadAssetAsync(item.ID) as UnityObject;
                    }
                }
                else
                {
                    m_selectedUnityObject = null;
                }
            }

            ParentDialog.IsOkInteractable = SelectedItem != null;
        }

        protected virtual bool CanClose()
        {
            return SelectedItem != null || IsNoneSelected;
        }

        protected virtual void ApplyFilter()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                m_items = IsAssetsTabSelected ? m_assetsCache : m_sceneCache;
                if(m_items != null)
                {
                    m_items = m_items.ToArray();
                }
            }
            else
            {
                AssetViewModel[] cache = IsAssetsTabSelected ? m_assetsCache : m_sceneCache;
                m_items = cache != null ? cache.Where(Filter).ToArray() : null;
            }
        }

        protected bool Filter(AssetViewModel item)
        {
            return item.DisplayName.ToLower().Contains(FilterText.ToLower());
        }
        #endregion
    }
}
