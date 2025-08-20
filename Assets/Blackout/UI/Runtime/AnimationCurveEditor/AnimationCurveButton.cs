using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// A custom button that draws a animation curve, and opens the editor when clicked.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class AnimationCurveButton : Selectable, IPointerClickHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [SerializeField]
        private ButtonCurveRenderer curveRenderer;

        [SerializeField, Tooltip("When to update this components curve renderer with changes from the editor.")]
        private CurveUpdateMode updateMode = CurveUpdateMode.None;
        
        [SerializeField]
        private AnimationCurveEditor editor;

        [SerializeField, Tooltip("Event that is called every frame the curve is being edited.")]
        private AnimationCurveEvent onCurveChange = new AnimationCurveEvent();
        
        [SerializeField, Tooltip("Event that is called when the curve has been edited and the editor has been closed.")]
        private AnimationCurveEvent onFinishedEditing = new AnimationCurveEvent();
        
        private AnimationCurveEvent _onClick = new AnimationCurveEvent();
        private bool _overrideOnClick;
        
        #endregion
        
        #region Properties
        
        public AnimationCurve Curve
        {
            get => curve;
            set
            {
                curve = value;
                curveRenderer.SetCurve(curve);
            }
        }
        
        public CurveRenderer CurveRenderer
        {
            get => curveRenderer;
            set => curveRenderer = value as ButtonCurveRenderer;
        }
        
        public CurveUpdateMode UpdateMode
        {
            get => updateMode;
            set => updateMode = value;
        }
        
        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        /// <summary>
        /// Called when the curve button is clicked.
        /// </summary>
        public AnimationCurveEvent OnClick
        {
            get => _onClick;
            set => _onClick = value;
        }
        
        /// <summary>
        /// Called every frame the curve is being edited and a change has been made.
        /// </summary>
        public AnimationCurveEvent OnCurveChange
        {
            get => onCurveChange;
            set => onCurveChange = value;
        }
        
        /// <summary>
        /// Called when the curve has been edited and the editor has been closed.
        /// </summary>
        public AnimationCurveEvent OnFinishedEditing
        {
            get => onFinishedEditing;
            set => onFinishedEditing = value;
        }
        
        /// <summary>
        /// If true, the OnClick event will be invoked instead of the button press defaulting to
        /// opening the curve editor and editing the curve
        /// </summary>
        public bool OverrideOnClick
        {
            get => _overrideOnClick;
            set => _overrideOnClick = value;
        }
        
        #endregion

        protected override void OnEnable()
        {
            if (curveRenderer)
                curveRenderer.SetCurve(curve);
            base.OnEnable();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }
        
        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("AnimationCurveButton.onClick", this);
            
            if (_overrideOnClick)
                _onClick.Invoke(curve);
            else
            {
                if (!editor)
                {
                    Debug.LogError("AnimationCurveButton requires an AnimationCurveEditor assigned to edit the curve");
                    return;
                }
                
                editor.EditCurve(curve);
                
                editor.OnValueChanged += OnCurveUpdate;
                editor.OnEndEdit += OnEndEdit;
            }
        }

        private void OnCurveUpdate()
        {
            if (updateMode == CurveUpdateMode.OnUpdate)
                curveRenderer.MarkDirty();
            
            onCurveChange.Invoke(curve);
        }
        
        private void OnEndEdit()
        {
            if (updateMode == CurveUpdateMode.OnEndEdit)
                curveRenderer.MarkDirty();
            
            onFinishedEditing.Invoke(curve);
        }
        
        [Serializable]
        public class AnimationCurveEvent : UnityEvent<AnimationCurve>
        {
        }
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (curve == null)
                curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            if (!curveRenderer)
                curveRenderer = GetComponentInChildren<ButtonCurveRenderer>();
                
            if (!curveRenderer.HasCurve)
                curveRenderer.SetCurve(curve);
            else curveRenderer.MarkDirty();
        }
        #endif

        /// <summary>
        /// Determines at what point the button renderer should update when using the editor.
        /// </summary>
        public enum CurveUpdateMode
        {
            /// <summary>
            /// Never updates (useful for curve presets as they never change)
            /// </summary>
            None, 
            /// <summary>
            /// Updates the button renderer every frame when the curve in the editor has changed
            /// </summary>
            OnUpdate, 
            /// <summary>
            /// Updates the button renderer when the editor has been closed
            /// </summary>
            OnEndEdit
        }
    }
}