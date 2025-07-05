using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class ManageProjectsViewModel : HierarchicalDataViewModel<ProjectListEntry>
    {
        #region ProjectInfoViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ProjectInfo without modifying the ProjectInfo itself.
        /// </summary>
        [Binding]
        internal class ProjectInfoViewModel
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            [Binding]
            public string DisplayName
            {
                get;
                set;
            }

            private ProjectInfoViewModel() { Debug.Assert(false); }
        }
        #endregion

        [SerializeField]
        private InputViewModel m_inputDialog = null;

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

        private IProjectListModel m_projectListModel;

        private ProjectListEntry[] m_listEntries;

        protected override async void Start()
        {
            base.Start();

            m_projectListModel = IOC.Resolve<IProjectListModel>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Open", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            await LoadProjectsAsync();
        }

        protected override void OnDestroy()
        {
            m_projectListModel = null;

            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private async void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (!HasSelectedItems)
            {
                args.Cancel = true;
            }
            else
            {
                var editor = IOC.Resolve<IRuntimeEditor>();
                if (!editor.IsProjectSupported(SelectedItem.Version))
                {
                    args.Cancel = true;
                    var windowManager = WindowManager;
                    windowManager.MessageBox(
                        Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToOpenProject", "Unable to open project"),
                        Localization.GetString("ID_RTEditor_ProjectsDialog_ProjectVersionNotSupported", "Project version not supported"));
                    
                }
                else
                {
                    await OpenProjectAsync();
                }
            }
        }
        #endregion

        #region Bound UnityEvent Handlers

        public override void OnItemDoubleClick()
        {
            ParentDialog.Close(true);
        }

        [Binding]
        public virtual void OnCreateProject()
        {
            InputViewModel input = Instantiate(m_inputDialog);
            input.gameObject.SetActive(true);

            WindowManager.Dialog(Localization.GetString("ID_RTEditor_ProjectsDialog_CreateProject", "Create Project"), input.transform,
                async (sender, args) =>
                {
                    string projectName = input.Text;
                    if (string.IsNullOrEmpty(projectName))
                    {
                        args.Cancel = true;
                        return;
                    }

                    if (projectName.Contains(".") || Path.GetInvalidFileNameChars().Any(projectName.Contains))
                    {
                        WindowManager.MessageBox(
                           Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToCreateProject", "Unable to create project"),
                           Localization.GetString("ID_RTEditor_ProjectsDialog_ProjectNameIsInvalid", "Project name is invalid"));
                        args.Cancel = true;
                        return;
                    }

                    if (m_listEntries != null && m_listEntries.Any(p => p.Name.ToLower() == projectName.ToLower()))
                    {
                        WindowManager.MessageBox(
                            Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToCreateProject", "Unable to create project"),
                            Localization.GetString("ID_RTEditor_ProjectsDialog_ProjectWithSameNameExists", "Project with the same name already exists"));
                        args.Cancel = true;
                        return;
                    }

                    try
                    {
                        Editor.IsBusy = true;
                        await CreateProjectAsync(projectName);
                    }
                    catch (Exception e)
                    {
                        WindowManager.MessageBox(Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToCreateProject", "Unable to create project"), e.Message);
                        args.Cancel = true;
                        Debug.LogException(e);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                },
                Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Create", "Create"),
                (sender, args) => { },
                Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"));
        }

        private async Task CreateProjectAsync(string projectName)
        {
            var listEntry = await m_projectListModel.CreateProjectAsync(projectName);
            var listEntries = m_listEntries.Union(new[] { listEntry }).OrderBy(p => p.Name).ToArray();

            SelectedItem = null;

            RaiseItemAdded(null, listEntry);
            int index = Array.IndexOf(listEntries, listEntry);
            if (index == 0)
            {
                var sibling = m_listEntries.FirstOrDefault();
                if (sibling != null)
                {
                    RaisePrevSiblingChanged(listEntry, sibling);
                }
            }
            else
            {
                var sibling = m_listEntries.ElementAt(index - 1);
                RaiseNextSiblingChanged(listEntry, sibling);
            }

            m_listEntries = listEntries;

            ScrollIntoView = true;
            SelectedItem = listEntry;
            ScrollIntoView = false;
        }

        [Binding]
        public virtual void OnDestroyProject()
        {
            var listEntry = SelectedItem;
            if (listEntry == null)
            {
                return;
            }

            if (m_listEntries.Length == 1)
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_ProjectsDialog_DeleteProject", "Delete Project"),
                    Localization.GetString("ID_RTEditor_ProjectsDialog_CantDeleteProject", "Can't remove the last project from the list"));
                return;
            }

            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_ProjectsDialog_DeleteProject", "Delete Project"),
                string.Format(Localization.GetString("ID_RTEditor_ProjectsDialog_AreYouSureDeleteProject", "Delete {0} project?"), listEntry.Name),
                async (sender, args) =>
                {
                    try
                    {
                        Editor.IsBusy = true;

                        await m_projectListModel.DeleteProjectAsync(listEntry.Name);
                        RaiseItemRemoved(null, listEntry);

                        var listEntires = m_listEntries.ToArray();
                        int index = Array.IndexOf(listEntires, listEntry);
                        listEntires = m_listEntries.Where(p => p != listEntry).ToArray();

                        if (index == listEntires.Length)
                        {
                            if (index > 0)
                            {
                                SelectedItem = listEntires[index - 1];
                            }
                            else
                            {
                                SelectedItem = null;
                            }
                        }
                        else
                        {
                            SelectedItem = listEntires[index];
                        }

                        if (!Editor.IsProjectSupported(listEntry.Name))
                        {
                            SelectedItem = listEntires.Where(l => Editor.IsProjectSupported(l.Version)).FirstOrDefault();
                        }

                        m_listEntries = listEntires.ToArray();

                        if (Editor != null && !string.IsNullOrEmpty(Editor.ProjectID) && Editor.ProjectID.ToLower() == listEntry.ProjectPath.ToLower())
                        {
                            await OpenProjectAsync();   
                        }
                    }
                    catch (Exception e)
                    {
                        WindowManager.MessageBox("Unable to delete project", e.Message);
                        args.Cancel = true;
                        Debug.LogException(e);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                },
            (sender, args) => { },
            Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Delete", "Delete"),
            Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"));
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
            flags &= ~HierarchicalDataFlags.CanMultiSelect;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectListEntry item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<ProjectListEntry> GetChildren(ProjectListEntry parent)
        {
            return m_listEntries;
        }

        #endregion

        #region Methods
        protected virtual async Task LoadProjectsAsync()
        {
            try
            {
                ParentDialog.IsInteractable = false;
                Editor.IsBusy = true;

                var listEntries = await m_projectListModel.GetProjectsAsync();
                m_listEntries = listEntries.OrderBy(p => p.Name).ToArray();

                BindData();

                if (Editor.ProjectID != null)
                {
                    SelectedItem = m_listEntries.Where(p => p.ProjectPath == Editor.ProjectID).FirstOrDefault();
                }

                if(SelectedItem == null)
                {
                    SelectedItem = m_listEntries.FirstOrDefault();
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                ParentDialog.IsInteractable = true;
                Editor.IsBusy = false;
            }
        }

        protected virtual async Task OpenProjectAsync()
        {
            var editor = Editor;
            try
            {
                editor.IsPlaying = false;
                editor.IsBusy = true;

                if (editor.ProjectID != null && editor.ProjectID.ToLower() == SelectedItem.ProjectPath.ToLower())
                {
                    return;
                }

                if (editor.IsProjectLoaded)
                {
                    await editor.UnloadProjectAsync();
                }

                await editor.LoadProjectAsync(SelectedItem?.ProjectPath, SelectedItem?.Version);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                var windowManager = WindowManager;
                await Task.Yield();

                windowManager.MessageBox(Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToOpenProject", "Unable to open project"), e.Message);
            }
            finally
            {
                editor.IsBusy = false;
            }
        }
        #endregion
    }
}
