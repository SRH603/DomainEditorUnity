using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Monitors mouse clicks around the curve to add keyframes and open popup menus
    /// </summary>
    [RequireComponent(typeof(MaskableGraphic))]
    public class CurveClickDetector : MonoBehaviour, IPointerClickHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        private GameObject quickActionMenu;
        
        #endregion
        
        #region Private Fields
        
        private RectTransform _rectTransform;
        private Transform _content;
        
        private AnimationCurveEditor _editor;
        private float _lastClickTime;

        #endregion
        
        #region Properties
        
        public GameObject Menu
        {
            get => quickActionMenu;
            set => quickActionMenu = value;
        }
        
        #endregion
        
        private void Start()
        {
            _rectTransform = (RectTransform)transform;
            
            _content = _rectTransform.parent;
            
            _editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            _editor.SelectKeyframe(null);
            
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
                {
                    quickActionMenu.transform.localPosition = localPoint;
                    quickActionMenu.SetActive(true);
                }

                return;
            }
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Time.time - _lastClickTime < _editor.Settings.curveDoubleClickTime)
            {
                Rect rect = _rectTransform.rect;

                // Local position of mouse
                Vector2 keyframeValues = (Vector2)_rectTransform.InverseTransformPoint(eventData.position);

                // Offset by pivot
                keyframeValues += rect.size * _rectTransform.pivot;

                // To normalized values
                keyframeValues /= rect.size;

                // Convert time/value to a range on the curve. This value currently is 0-1 on both axes
                keyframeValues = _editor.ConvertNormalizedToCurveRange(keyframeValues);

                if (_editor.CanInsertAtTime(keyframeValues.x))
                {
                    // Calculate a usable distance to detect a valid click
                    float thickness = ((_editor.Settings.curveClickThickness * (1f / _content.localScale.y)) / rect.size.y);

                    // Evaluate the curve to find the value at the time determined by the mouse position
                    float value = _editor.Curve.Evaluate(keyframeValues.x);

                    // Check if the click position is withing the tolerance of the curve
                    if (Mathf.Abs(value - keyframeValues.y) <= thickness)
                    {
                        // Insert it in to the curve and update everything
                        _editor.InsertKeyframe(keyframeValues.x, value);
                        _lastClickTime = 0;
                        return;
                    }
                }
            }

            _lastClickTime = Time.time;
        }
    }
}