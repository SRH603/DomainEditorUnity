using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.ViewModels;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor
{
    [Binding]
    public class PrefabCommandsEditor : ViewModelBase
    {
        private IRuntimeEditor m_editor;
        private IRuntimeObjects m_object;
        private IGameObjectEditor m_gameObjectEditor;

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

        private bool m_canSelect;

        [Binding]
        public bool CanSelect
        {
            get { return m_canSelect; }
            set
            {
                if (m_canSelect != value)
                {
                    m_canSelect = value;
                    RaisePropertyChanged(nameof(CanSelect));
                }
            }
        }

        private bool m_canApplyChanges;
        [Binding]
        public bool CanApplyChanges
        {
            get { return m_canApplyChanges; }
            set
            {
                if (m_canApplyChanges != value)
                {
                    m_canApplyChanges = value;
                    RaisePropertyChanged(nameof(CanApplyChanges));
                }
            }
        }

        private bool m_canApplyChangesVisible;
        [Binding]
        public bool CanApplyChangesVisible
        {
            get { return m_canApplyChangesVisible; }
            set
            {
                if (m_canApplyChangesVisible != value)
                {
                    m_canApplyChangesVisible = value;
                    RaisePropertyChanged(nameof(CanApplyChangesVisible));
                }
            }
        }

        private bool m_canApplyRevertToBase;
        [Binding]
        public bool CanApplyRevertToBase
        {
            get { return m_canApplyRevertToBase; }
            set
            {
                if (m_canApplyRevertToBase != value)
                {
                    m_canApplyRevertToBase = value;
                    RaisePropertyChanged(nameof(CanApplyRevertToBase));
                }
            }
        }

        private bool m_canApplyToBase;
        [Binding]
        public bool CanApplyToBase
        {
            get { return m_canApplyToBase; }
            set
            {
                if (m_canApplyToBase != value)
                {
                    m_canApplyToBase = value;
                    RaisePropertyChanged(nameof(CanApplyToBase));
                }
            }
        }

        private bool m_canRevertToBase;
        [Binding]
        public bool CanRevertToBase
        {
            get { return m_canRevertToBase; }
            set
            {
                if (m_canRevertToBase != value)
                {
                    m_canRevertToBase = value;
                    RaisePropertyChanged(nameof(CanRevertToBase));
                }
            }
        }

        private GameObject[] SelectedGameObjects
        {
            get {
                return
                    m_gameObjectEditor != null ?
                    m_gameObjectEditor.SelectedGameObjects :
                    m_editor.Selection.gameObjects;
            }
        }

        private GameObject SelectedGameObject
        {
            get { return SelectedGameObjects != null ? SelectedGameObjects[0] : null; }
        }

        private void Start()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_object = m_editor.Object;
            m_object.ComponentAdded += OnAddComponent;
            m_object.ComponentDestroyed += OnDestroyComponent;
            m_editor.SetDirty += OnSetDirty;
            
            m_gameObjectEditor = GetComponentInParent<IGameObjectEditor>();
            RefreshState();
        }

 
        private void OnDestroy()
        {
            m_gameObjectEditor = null;
            
            if (m_editor != null)
            {
                m_editor.SetDirty -= OnSetDirty;
                m_editor = null;
            }       
            
            if (m_object != null)
            {
                m_object.ComponentAdded -= OnAddComponent;
                m_object.ComponentDestroyed -= OnDestroyComponent;
                m_object = null;
            }
        }

        private void OnAddComponent(ExposeToEditor obj, Component component)
        {
            if (component == null || m_editor.GetTypeID(component.GetType()) == ID.Empty)
            {
                return;
            }

            RefreshState();
        }

        private void OnSetDirty(object sender, InstanceEventArgs e)
        {
            RefreshState();
        }

        private async void OnDestroyComponent(ExposeToEditor obj, Component component)
        {
            if (m_editor.GetTypeID(component.GetType()) == ID.Empty)
            {
                return;
            }

            using var b = m_editor.SetBusy();
            await Task.Yield();
            await Task.Yield();
            RefreshState();
        }


        [Binding]
        public async void OnOpen()
        {
            using var b = m_editor.SetBusy();
            await m_editor.OpenPrefabAsync(SelectedGameObject);
            RefreshState();
        }

        [Binding]
        public void OnSelect()
        {
            using var b = m_editor.SetBusy();
            m_editor.SelectPrefabAsync(SelectedGameObject);
        }

        [Binding]
        public async void OnApplyChanges()
        {
            using var b = m_editor.SetBusy();
            await m_editor.ApplyChangesAsync(SelectedGameObject);
            RefreshState();
        }

        [Binding]
        public async void OnApplyChangesToBase()
        {
            using var b = m_editor.SetBusy();
            await m_editor.ApplyToBaseAsync(SelectedGameObject);
            RefreshState();
        }

        [Binding]
        public async void OnRevertChangesToBase()
        {
            using var b = m_editor.SetBusy();
            await m_editor.RevertToBaseAsync(SelectedGameObject);
            RefreshState();
        }

        private void RefreshState()
        {
            var go = SelectedGameObject;
            
            CanOpen = m_editor.CanOpenPrefab(go);
            CanSelect = m_editor.CanSelectPrefab(go);
            CanApplyChanges = m_editor.CanApplyChanges(go);

            //CanApplyChangesVisible = m_editor.IsInstanceRoot(go);
            CanApplyChangesVisible = true;

            bool canApplyToBase = m_editor.CanApplyToBase(go) && m_editor.CurrentPrefab != null;
            bool canRevertToBase = m_editor.CanRevertToBase(go) && m_editor.CurrentPrefab != null;
            CanApplyRevertToBase = canApplyToBase || canRevertToBase;
            CanApplyToBase = canApplyToBase;
            CanRevertToBase = canRevertToBase;
        }
    }
}

