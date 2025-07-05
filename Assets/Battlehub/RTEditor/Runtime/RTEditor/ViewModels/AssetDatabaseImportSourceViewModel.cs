using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AssetDatabaseImportSourceViewModel : HierarchicalDataViewModel<IImportGroup>
    {
        [Binding]
        internal class ImportGroupViewModel
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            public ImportGroupViewModel(string name)
            {
                Name = name;
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

        private string[] m_importSources = new string[0];

        [Binding]
        public IList<string> ImportSources
        {
            get { return m_importSources; }
        }

        private int m_selectedImportSourceIndex;

        [Binding]
        public int SelectedImportSourceIndex
        {
            get { return m_selectedImportSourceIndex; }
            set 
            {
                if (m_selectedImportSourceIndex != value)
                {
                    m_selectedImportSourceIndex = value;
                    RaisePropertyChanged(nameof(SelectedImportSourceIndex));
                    LoadAndBindData();
                }
            }
        }

        public override IEnumerable<IImportGroup> SelectedItems 
        {
            get { return base.SelectedItems; }
            set
            {
                base.SelectedItems = value;
                ParentDialog.IsOkInteractable = SelectedItem != null;
            }
        }

        private IImportGroup[] m_importGroups;

        protected override void Start()
        {
            base.Start();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_AssetDatabaseImportSource_Select", "Select"),
                CancelText = Localization.GetString("ID_RTEditor_AssetDatabaseImportSource_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            m_importSources = Editor.ImportSources.Where(src => src.IsEnabled).OrderBy(src => src.SortIndex).Select(src => src.DisplayName).ToArray();
            RaisePropertyChanged(nameof(ImportSources));

            LoadAndBindData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }
        }

        #region Dialog Event Handlers
        protected virtual void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            args.Cancel = true;
            Import(SelectedItem);
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(IImportGroup item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<IImportGroup> GetChildren(IImportGroup parent)
        {
            return m_importGroups;
        }

        #endregion

        #region Bound Unity EventHandlers

        public override void OnItemDoubleClick()
        {
            base.OnItemDoubleClick();
            ParentDialog.Close(true);
        }

        #endregion

        #region Methods

        protected virtual async void LoadAndBindData()
        {
            using var b = Editor.SetBusy();

            SelectedItems = new List<ImportGroup>();    

            var importSource = Editor.ImportSources.Where(src => src.IsEnabled).OrderBy(src => src.SortIndex).ElementAt(SelectedImportSourceIndex);
            m_importGroups = await importSource.GetGroupsAsync();

            BindData();
        }

        protected virtual void Import(IImportGroup importGroup)
        {
            WindowManager.CreateWindow(BuiltInWindowNames.ImportAssets);

            var assetLibraryImporter = IOC.Resolve<IAssetDatabaseImportDialog>();
            if (assetLibraryImporter != null)
            {
                assetLibraryImporter.ImportSourceIndex = SelectedImportSourceIndex;
                assetLibraryImporter.GroupKey = importGroup.Key;
                assetLibraryImporter.Closed += OnAssetLibraryImporterClosed;
            }
         }

        private void OnAssetLibraryImporterClosed(object sender, IAssetDatabaseImportDialog.CloseEventArgs args)
        {
            if (args.Result != null)
            {
                var dialog = (IAssetDatabaseImportDialog)sender;
                dialog.Closed -= OnAssetLibraryImporterClosed;

                if (args.Result == true)
                {
                    if (m_parentDialog != null)
                    {
                        m_parentDialog.Close();
                    }
                }
            }
        }

        #endregion
    }
}

