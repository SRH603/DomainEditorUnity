using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    public interface IAssetDatabaseImportDialog
    {
        public class CloseEventArgs : EventArgs
        {
            public bool? Result
            {
                get;
            }

            public CloseEventArgs(bool? result)
            {
                Result = result;
            }
        }


        event EventHandler<CloseEventArgs> Closed;

        int ImportSourceIndex
        {
            set;
        }

        string GroupKey
        {
            set;
        }
    }

    [Binding]
    public class AssetDatabaseImportViewModel : HierarchicalDataViewModel<IImportAsset>, IAssetDatabaseImportDialog
    {
        [Binding]
        internal class ImportAssetViewModel
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            [Binding]
            public Texture Thumbnail
            {
                get;
                set;
            }

            [Binding]
            public ImportAsset Self
            {
                get;
                set;
            }

            public ImportAssetViewModel(string name)
            {
                Name = name;
            }
        }

        public event EventHandler<IAssetDatabaseImportDialog.CloseEventArgs> Closed;

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


        private bool m_noItemsToImport;
        [Binding]
        public bool NoItemsToImport
        {
            get { return m_noItemsToImport; }
            protected set
            {
                if (m_noItemsToImport != value)
                {
                    m_noItemsToImport = value;
                    RaisePropertyChanged(nameof(NoItemsToImport));
                }
            }
        }

        protected ImportAsset[] SelectedAssets
        {
            get { return SelectedItems != null ? SelectedItems.OfType<ImportAsset>().Where(p => p.Children == null).ToArray() : null; }
        }

        public int ImportSourceIndex
        {
            get;
            set;
        }

        public string GroupKey
        {
            get;
            set;
        }

        private IImportAsset[] m_rootAssets;
        
        protected override void Awake()
        {
            base.Awake();

            IOC.RegisterFallback<IAssetDatabaseImportDialog>(this);
        }

        protected override void Start()
        {
            base.Start();
            
            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetDatabaseImportDialog_Btn_Import", "Import"),
                CancelText = Localization.GetString("ID_RTEditor_AssetDatabaseImportDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;
            m_parentDialog.Cancel += OnCancel;
            m_parentDialog.Closed += OnClosed;

            LoadAndBindData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog.Cancel -= OnCancel;
                m_parentDialog.Closed -= OnClosed;
                m_parentDialog = null;
            }

            IOC.UnregisterFallback<IAssetDatabaseImportDialog>(this);
        }

        #region Dialog Event Handlers

        private async void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            var editor = Editor;
            if (editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }
            if (SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            using var b = editor.SetBusy();

            var importSource = Editor.ImportSources.Where(src => src.IsEnabled).OrderBy(src => src.SortIndex).ElementAt(ImportSourceIndex);
            foreach (var selectedAsset in SelectedAssets)
            {
                object key = selectedAsset.Key;
                string path = Path.GetDirectoryName(key.ToString());

                if (!editor.Exists(path) && !string.IsNullOrWhiteSpace(path))
                {
                    await editor.CreateFolderAsync(path);
                }

                string desiredName = Path.GetFileNameWithoutExtension(selectedAsset.Name);
                try
                {
                    var folderID = editor.GetAssetID(path);
                    if (folderID == ID.Empty)
                    {
                        folderID = editor.CurrentFolderID;
                    }
                    
                    await editor.ImportExternalAssetAsync(folderID, key, importSource.LoaderID, desiredName);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnCancel(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (Editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }
        }

        private void OnClosed(object sender, DialogViewModel.CloseEventArgs e)
        {
            Closed?.Invoke(this, new IAssetDatabaseImportDialog.CloseEventArgs(e.Result));
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(IImportAsset item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override bool HasChildren(IImportAsset parent)
        {
            return parent.Children != null && parent.Children.Where(ProjectItemPassesFilter).Count() > 0;
        }

        public override IEnumerable<IImportAsset> GetChildren(IImportAsset parent)
        {
            if (parent == null)
            {
                return m_rootAssets;
            }

            if (parent.Children == null)
            {
                return new IImportAsset[0];
            }

            return parent.Children.Where(ProjectItemPassesFilter);
        }

        protected virtual bool ProjectItemPassesFilter(IImportAsset p)
        {
            return true;
        }

        #endregion

        #region Methods
        protected virtual async void LoadAndBindData()
        {
            using var b = Editor.SetBusy();

            var importSource = Editor.ImportSources.Where(src => src.IsEnabled).OrderBy(src => src.SortIndex).ElementAt(ImportSourceIndex);
            m_rootAssets = await importSource.GetAssetsAsync(GroupKey);

            if (m_rootAssets.Where(ProjectItemPassesFilter).Count() > 0)
            {
                var assets = m_rootAssets.SelectMany(asset => asset.Flatten(excludeFolders: false)).ToArray();
                for (int i = 0; i < assets.Length; ++i)
                {
                    if (assets[i].Thumbnail != null)
                    {
                        continue;
                    }

                    if (assets[i].IsFolder)
                    {
                        assets[i].Thumbnail = Editor.ThumbnailUtil.GetBuiltinThumbnail(null, large: false);
                    }
                    else
                    {
                        assets[i].Thumbnail = Editor.ThumbnailUtil.GetBuiltinThumbnail(typeof(GameObject), large:false);
                    }
                }

                BindData();
                SelectedItems = assets;
                foreach(var item in m_rootAssets)
                {
                    ExpandAll(item);
                }
            }
            else
            {
                Editor.IsBusy = false;

                m_parentDialog.IsOkInteractable = false;

                NoItemsToImport = true;
            }
        }
        #endregion
    }
}
