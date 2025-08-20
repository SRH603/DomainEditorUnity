using UnityEngine;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// Opens the tangent side menu when the mouse hovers over or is clicked
    /// </summary>
    public class TangentFoldoutButton : TangentMenuButton
    {
        #region Serialized Fields
        
        [SerializeField]
        private KeyframeEditorPopup editorPopup;
        
        [SerializeField]
        private CurveTangent.Side side;
        
        #endregion
        
        #region Private Fields
        
        private float _timeInside;
        private float _timeOutside;

        #endregion
        
        #region Properties
        
        public KeyframeEditorPopup EditorPopup
        {
            get => editorPopup;
            set => editorPopup = value;
        }
        
        public CurveTangent.Side Side
        {
            get => side;
            set => side = value;
        }
        
        #endregion
        
        private bool _isOpen = false;

        protected override void Awake()
        {
            base.Awake();
            
            if (!editorPopup)
                editorPopup = ComponentUtility.GetComponentInParent<KeyframeEditorPopup>(gameObject);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _timeInside = 0f;
            _timeOutside = 0f;
            _isOpen = false;
        }

        private void Update()
        {
            if (pointerInside)
            {
                if (_isOpen)
                    return;
                
                _timeInside += Time.deltaTime;

                if (_timeInside > 0.25f)
                {
                    editorPopup.OpenTangentMenu(rectTransform, side);
                    _isOpen = true;
                    _timeInside = 0f;
                }
            }
            else
            {
                if (!_isOpen)
                    return;
                
                _timeOutside += Time.deltaTime;
                if (_timeOutside > 0.2f)
                {
                    editorPopup.CloseTangentMenu(side);
                    _isOpen = false;
                    _timeOutside = 0f;
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            _timeInside = 0f;
            _timeOutside = 0f;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            _timeOutside = 0f;
            _timeInside = 0f;
        }
    }
}
