using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Handles color tint transition when hovering over
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class TangentHoverColorTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        protected AnimationCurveEditor editor;
        
        [SerializeField]
        private Graphic graphic;
        
        #endregion
        
        #region Private Fields
        
        protected RectTransform rectTransform;
        protected bool pointerInside;
        
        private Color _fadeColor;
        private float _fadeDuration;
        
        #endregion
        
        #region Properties
        
        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        public Graphic Graphic
        {
            get => graphic;
            set => graphic = value;
        }
        
        #endregion

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;

            if (!editor)
                editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
            
            if (!graphic)
                graphic = GetComponent<Graphic>();
            
            graphic.CrossFadeColor(Color.clear, 0f, true, true);
        }

        protected virtual void OnEnable()
        {
            if (!editor)
                editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
            
            _fadeColor = editor.Settings.keyframeEditorButtonColor;
            _fadeDuration = editor.Settings.keyframeEditorButtonFadeDuration;
        }

        protected virtual void OnDisable()
        {
            pointerInside = false;
            graphic.CrossFadeColor(Color.clear, 0f, true, true);
        }
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            EvaluateAndTransitionToSelectionState();
        }
        
        private void EvaluateAndTransitionToSelectionState()
        {
            if (!gameObject.activeSelf || !enabled)
                return;

            if (!graphic)
                return;

            graphic.CrossFadeColor(pointerInside ? _fadeColor : Color.clear, _fadeDuration, true, true);
        }
        
        #if UNITY_EDITOR
        private void OnValidate() => Awake();
        #endif
    }
}