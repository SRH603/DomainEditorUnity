using UnityEngine;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// Creates a draggable area to move a RectTransform around the screen
    /// </summary>
    public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [SerializeField, Tooltip("The RectTransform of the object to be moved")]
        private RectTransform windowRectTransform;
        
        private RectTransform _canvasRectTransform;
        private Canvas _canvas;
        private Vector2 _pointerOffset;

        private void Awake()
        {
            if (!windowRectTransform)
                windowRectTransform = transform.parent != null? transform.parent.GetComponent<RectTransform>() : transform.GetComponent<RectTransform>();

            if (windowRectTransform)
            {
                windowRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                windowRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                windowRectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            
            _canvas = CanvasUtility.GetRootCanvas(gameObject);
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform, eventData.position, eventData.pressEventCamera, out _pointerOffset);
            _pointerOffset -= windowRectTransform.anchoredPosition;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (windowRectTransform == null)
                return;

            Vector2 pointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRectTransform, eventData.position, eventData.pressEventCamera, out pointerPosition))
            {
                Vector2 newPosition = pointerPosition - _pointerOffset;
                Vector2 clampedPosition = ClampToWindow(newPosition);

                windowRectTransform.anchoredPosition = clampedPosition;
            }
        }

        private Vector2 ClampToWindow(Vector2 position)
        {
            Rect canvasRect = _canvasRectTransform.rect;
            Rect windowRect = windowRectTransform.rect;
            
            float minX = canvasRect.xMin - windowRect.xMin;
            float maxX = canvasRect.xMax - windowRect.xMax;

            float minY = canvasRect.yMin - windowRect.yMin;
            float maxY = canvasRect.yMax - windowRect.yMax;

            return new Vector2(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY)
            );
        }
    }
}