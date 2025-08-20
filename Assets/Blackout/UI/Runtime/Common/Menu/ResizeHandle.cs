using UnityEngine;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// Handles resize manipulation of a RectTransform
    /// </summary>
    public class ResizeHandle : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Serialized Fields

        [SerializeField]
        private ResizeCursor resizeCursor;
        
        [SerializeField]
        private ResizeCursorType cursor;

        [SerializeField]
        private RectTransform windowRectTransform;

        [SerializeField, Tooltip("The direction of movement for this handle")]
        private Vector2Int direction = new Vector2Int(0, 0);

        [SerializeField, Tooltip("The minimum width of the window")]
        private float minWidth = 250;

        [SerializeField, Tooltip("The minimum height of the window")]
        private float minHeight = 250;
        #endregion
        
        #region Private Fields
        
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;

        private bool _isDragging;
        private Vector2 _pointerOffset;
        
        #endregion
        
        #region Properties
        
        public RectTransform WindowRectTransform
        {
            get => windowRectTransform;
            set => windowRectTransform = value;
        }

        public Vector2Int Direction
        {
            get => direction;
            set => direction = value;
        }
        
        public ResizeCursorType CursorType
        {
            get => cursor;
            set => cursor = value;
        }

        public ResizeCursor ResizeCursor
        {
            get => resizeCursor;
            set => resizeCursor = value;
        }
        
        #endregion

        #region Unity
        private void Awake()
        {
            _canvas = CanvasUtility.GetRootCanvas(gameObject);
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        }
        #endregion

        #region Interfaces

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;

            resizeCursor.SetCursor(this, cursor);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out _pointerOffset);
            
            _isDragging = true;
        }
        
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localMousePosition))
                return;
            
            Vector2 canvasSize = _canvasRectTransform.rect.size;
            Vector2 canvasPivot = _canvasRectTransform.pivot;
            
            localMousePosition.x = Mathf.Clamp(localMousePosition.x, -(canvasSize.x * canvasPivot.x), canvasSize.x * (1f - canvasPivot.x));
            localMousePosition.y = Mathf.Clamp(localMousePosition.y, -(canvasSize.y * canvasPivot.y), canvasSize.y * (1f - canvasPivot.y));
            
            Vector2 offsetMin = windowRectTransform.offsetMin;
            Vector2 offsetMax = windowRectTransform.offsetMax;

            if (direction.x < 0)  // Left side
            {
                offsetMin.x = localMousePosition.x + _pointerOffset.x;
                
                if (offsetMax.x - offsetMin.x < minWidth)
                    offsetMin.x = offsetMax.x - minWidth;
            }
            else if (direction.x > 0)  // Right side
            {
                offsetMax.x = localMousePosition.x - _pointerOffset.x;
                
                if (offsetMax.x - offsetMin.x < minWidth)
                    offsetMax.x = offsetMin.x + minWidth;
            }

            if (direction.y < 0)  // Bottom side
            {
                offsetMin.y = localMousePosition.y - _pointerOffset.y;
                
                if (offsetMax.y - offsetMin.y < minHeight)
                    offsetMin.y = offsetMax.y - minHeight;
            }
            else if (direction.y > 0)  // Top side
            {
                offsetMax.y = localMousePosition.y + _pointerOffset.y;

                if (offsetMax.y - offsetMin.y < minHeight)
                    offsetMax.y = offsetMin.y + minHeight;
            }

            windowRectTransform.offsetMin = offsetMin;
            windowRectTransform.offsetMax = offsetMax;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            
            resizeCursor.ResetCursor(this);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_isDragging)
                resizeCursor.SetCursor(this, cursor);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_isDragging)
                resizeCursor.ResetCursor(this);
        }
        #endregion
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!resizeCursor)
                resizeCursor = ComponentUtility.GetComponentInParent<ResizeCursor>(gameObject);
        }
        #endif
    }
}