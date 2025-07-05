using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.Utils;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AnimationViewModel : ViewModel
    {
        private byte[][] m_state;

        private GameObject m_targetGameObject;

        [Binding]
        public GameObject TargetGameObject
        {
            get { return m_targetGameObject; }
            set
            {
                if(m_targetGameObject != value)
                {
                    m_targetGameObject = value;
                    RaisePropertyChanged(nameof(TargetGameObject));
                }
            }
        }

        private RuntimeAnimation m_target;

        [Binding]
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set 
            {
                if(m_target != value)
                {
                    m_target = value;
                    RaisePropertyChanged(nameof(Target));
                }
            }
            
        }

        private RuntimeAnimationClip m_currentClip;

        [Binding]
        public RuntimeAnimationClip CurrentClip
        {
            get { return m_currentClip; }
            set
            {
                if(m_currentClip != value)
                {
                    m_currentClip = value;
                    RaisePropertyChanged(nameof(CurrentClip));
                }
            }
        }

        private bool m_isEditing;

        [Binding]
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if(m_isEditing != value)
                {
                    m_isEditing = value;
                    RaisePropertyChanged(nameof(IsEditing));
                }
            }
        }

        private IInspectorModel m_inspector;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            m_inspector = IOC.Resolve<IInspectorModel>();
            if (m_inspector != null)
            {
                m_inspector.BeginEdit += OnInspectorBeginEdit;
                m_inspector.EndEdit += OnInspectorEndEdit;
            }

            Editor.BeforeCreateAsset += OnBeforeCreateAsset;
            Editor.Selection.SelectionChanged += OnSelectionChanged;
            OnSelectionChanged(null);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if(m_inspector != null)
            {
                m_inspector.BeginEdit -= OnInspectorBeginEdit;
                m_inspector.EndEdit -= OnInspectorEndEdit;
                m_inspector = null;
            }

            Editor.BeforeCreateAsset -= OnBeforeCreateAsset;
            Editor.Selection.SelectionChanged -= OnSelectionChanged;
        }

        protected virtual void Update()
        {
            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (activeTool is BaseHandle)
            {
                IsEditing = true;
            }
            else
            {
                if (!m_inspector.IsEditing)
                {
                    IsEditing = false;
                }
            }
        }
  
        #region Bound Unity EventHandlers

        [Binding]
        public virtual async void OnSaveCurrentClip()
        {
            if (CurrentClip != null)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if (editor != null && !editor.IsPlaying)
                {
                    var id = editor.GetAssetID(CurrentClip);
                    if (id != ID.Empty)
                    {
                        using var b = SetBusy();
                        await editor.SaveAssetAsync(id);
                    }
                }
            }
        }

        [Binding]
        public async void OnClipBeginModify()
        {
            if (Target != null)
            {
                using var b = SetBusy();
                m_state = await SaveStateAsync();
            }
        }

        [Binding]
        public async void OnClipModified()
        {
            if (Target != null)
            {
                Target.Refresh();

                using var b = SetBusy();
                byte[][] newState = await SaveStateAsync();
                byte[][] oldState = m_state;

                m_state = null;

                Editor.Undo.CreateRecord(redoRecord =>
                {
                    OnUndoRedo(newState);
                    return true;
                },
                undoRecord =>
                {
                    OnUndoRedo(oldState);
                    return true;
                });
            }
        }

        protected async void OnUndoRedo(byte[][] state)
        {
            using var b = SetBusy();
            await LoadStateAsync(state);
            UpdateTargetAnimation();
            RaisePropertyChanged(nameof(CurrentClip));
        }

        #endregion

        #region Methods

        private void OnBeforeCreateAsset(object sender, BeforeCreateAssetEventArgs e)
        {
            if (ReferenceEquals(e.Object, Editor.CurrentScene))
            {
                OnSaveCurrentClip();
            }
        }

        private void OnInspectorBeginEdit(object sender, InspectorModelEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }

            if (CurrentClip == null || CurrentClip.Properties == null || CurrentClip.Properties.Count == 0)
            {
                return;
            }

            Component targetComponent = e.TargetComponents.FirstOrDefault();
            if (targetComponent == null)
            {
                return;
            }

            bool canBeginEdit = false;
            foreach (RuntimeAnimationProperty property in CurrentClip.Properties)
            {
                if (property.ComponentType == targetComponent.GetType())
                {
                    canBeginEdit = true;
                }
            }

            if (!canBeginEdit)
            {
                return;
            }

            IsEditing = true;
        }
        private void OnInspectorEndEdit(object sender, InspectorModelEventArgs e)
        {
            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (IsEditing && activeTool == null)
            {
                IsEditing = false;
            }
        }

        protected virtual void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            GameObject target = Editor.Selection.activeGameObject;
            if (target != null && target.IsPrefab())
            {
                TargetGameObject = null;
            }
            else
            {
                TargetGameObject = Editor.Selection.activeGameObject;
            }
        }

        protected void UpdateTargetAnimation()
        {
            if (TargetGameObject != null)
            {
                RuntimeAnimation animation = TargetGameObject.GetComponent<RuntimeAnimation>();
                Target = animation;
            }
            else
            {
                Target = null;
            }
        }

        protected async Task<byte[][]> SaveStateAsync()
        {
            var clips = Target.Clips;
            byte[][] state = new byte[clips.Count][];
            for (int i = 0; i < state.Length; ++i)
            {
                state[i] = await Editor.SerializeAsync(clips[i]);
            }
            return state;
        }

        protected async Task LoadStateAsync(byte[][] state)
        {
            var editor = Editor;

            var clips = Target.Clips;
            var stateLength = state.Length;
            while (stateLength < clips.Count)
            {
                clips.RemoveAt(clips.Count - 1);
            }

            for (int i = 0; i < clips.Count; ++i)
            {
                var clip = clips[i];
                await editor.DeserializeAsync(state[i], clip);
            }

            if (Target.ClipIndex >= clips.Count)
            {
                Target.ClipIndex = clips.Count - 1;
            }
        }

        #endregion
    }
}
