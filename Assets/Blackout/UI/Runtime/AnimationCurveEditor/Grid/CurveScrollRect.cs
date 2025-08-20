using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// An infinitely scrolling ScrollRect component that resizes itself to the position of the content.
    /// Contains logic for zooming and panning
    /// </summary>
    [AddComponentMenu("UI/Animation Curve Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class CurveScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        #region Serialized Fields
        [SerializeField]
        private RectTransform content;

        [SerializeField]
        private RectTransform grid;
        
        [SerializeField]
        private bool inertia = true;

        [SerializeField]
        private float decelerationRate = 0.135f;

        [SerializeField]
        private float zoomSensitivity = 0.1f;
        
        [SerializeField]
        private float minimumZoom = 0.2f;
        
        [SerializeField]
        private float maximumZoom = 5.0f;

        [SerializeField]
        private RectTransform viewport;

        [SerializeField]
        private Scrollbar horizontalScrollbar;

        [SerializeField]
        private Scrollbar verticalScrollbar;

        [SerializeField]
        private float horizontalScrollbarSpacing;

        [SerializeField]
        private float horizontalScrollbarOffset;
        
        [SerializeField]
        private float verticalScrollbarSpacing;

        
        [SerializeField]
        private AnimationCurveScrollRectEvent onPositionChanged = new AnimationCurveScrollRectEvent();
        
        [SerializeField]
        private AnimationCurveScrollRectEvent onScaleChanged = new AnimationCurveScrollRectEvent();
        #endregion

        #region Private Fields

        private Vector2 _pointerStartLocalCursor = Vector2.zero;
        private Vector2 _contentStartPosition = Vector2.zero;
        private Vector2 _prevPosition = Vector2.zero;
        private Vector2 _velocity;

        private RectTransform _viewRectTransform;

        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;
        private Bounds _contentBounds;
        private Bounds _viewBounds;

        private bool _dragging;

        private float _hSliderHeight;
        private float _vSliderWidth;

        [NonSerialized]
        private bool _hasRebuiltLayout = false;

        [NonSerialized]
        private RectTransform _rectTransform;

        private RectTransform _horizontalScrollbarRect;
        private RectTransform _verticalScrollbarRect;
        
        private readonly Vector3[] _contentCorners = new Vector3[4];
        private readonly Vector3[] _viewportCorners = new Vector3[4];
        #endregion

        #region Properties
        public RectTransform Content
        {
            get => content;
            set => content = value;
        }
        
        public RectTransform Grid
        {
            get => grid;
            set => grid = value;
        }

        public bool Inertia
        {
            get => inertia;
            set => inertia = value;
        }

        public float DecelerationRate
        {
            get => decelerationRate;
            set => decelerationRate = value;
        }

        public float ZoomSensitivity
        {
            get => zoomSensitivity;
            set => zoomSensitivity = value;
        }
        
        public float MinimumZoom
        {
            get => minimumZoom;
            set => minimumZoom = value;
        }
        
        public float MaximumZoom
        {
            get => maximumZoom;
            set => maximumZoom = value;
        }

        public RectTransform Viewport
        {
            get => viewport;
            set
            {
                viewport = value;
                SetDirtyCaching();
            }
        }

        public Scrollbar HorizontalScrollbar
        {
            get => horizontalScrollbar;
            set
            {
                if (horizontalScrollbar)
                    horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);

                horizontalScrollbar = value;

                if (horizontalScrollbar)
                    horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);

                SetDirtyCaching();
            }
        }


        public Scrollbar VerticalScrollbar
        {
            get => verticalScrollbar;
            set
            {
                if (verticalScrollbar)
                    verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

                verticalScrollbar = value;

                if (verticalScrollbar)
                    verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

                SetDirtyCaching();
            }
        }

        public float HorizontalScrollbarSpacing
        {
            get => horizontalScrollbarSpacing;
            set
            {
                horizontalScrollbarSpacing = value;
                SetDirty();
            }
        }

        public float VerticalScrollbarSpacing
        {
            get => verticalScrollbarSpacing;
            set
            {
                verticalScrollbarSpacing = value;
                SetDirty();
            }
        }

        public AnimationCurveScrollRectEvent OnPositionChanged
        {
            get => onPositionChanged;
            set => onPositionChanged = value;
        } 
        
        public AnimationCurveScrollRectEvent OnScaleChanged
        {
            get => onScaleChanged;
            set => onScaleChanged = value;
        }

        protected RectTransform ViewRect
        {
            get
            {
                if (!_viewRectTransform)
                    _viewRectTransform = viewport;

                if (!_viewRectTransform)
                    _viewRectTransform = (RectTransform)transform;

                return _viewRectTransform;
            }
        }

        public Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }
        
        private RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        #endregion

        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker _tracker;
#pragma warning restore 649

        /// <summary>
        /// Rebuilds the scroll rect data after initialization.
        /// </summary>
        /// <param name="executing">The current step in the rendering CanvasUpdate cycle.</param>
        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars();
                UpdatePrevData();

                _hasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        void UpdateCachedData()
        {
            _horizontalScrollbarRect = horizontalScrollbar == null ? null : horizontalScrollbar.transform as RectTransform;
            _verticalScrollbarRect = verticalScrollbar == null ? null : verticalScrollbar.transform as RectTransform;

            _hSliderHeight = (_horizontalScrollbarRect == null ? 0 : _horizontalScrollbarRect.rect.height);
            _vSliderWidth = (_verticalScrollbarRect == null ? 0 : _verticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (horizontalScrollbar)
                horizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);

            if (verticalScrollbar)
                verticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (horizontalScrollbar)
                horizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);

            if (verticalScrollbar)
                verticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            _dragging = false;
            _hasRebuiltLayout = false;
            _tracker.Clear();
            _velocity = Vector2.zero;
            
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
            => base.IsActive() && content;

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Sets the velocity to zero on both axes so the content stops moving.
        /// </summary>
        public virtual void StopMovement()
            => _velocity = Vector2.zero;

        #region Zoom
        void IScrollHandler.OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;
            
            PerformZoom(data.scrollDelta.y, data.position, InputUtility.Ctrl(), InputUtility.Shift());
        }

        private void PerformZoom(float delta, Vector3 mousePosition, bool ctrl, bool shift)
        {
            Vector2 zoomAround = grid.InverseTransformPoint(mousePosition);
            if (!grid.rect.Contains(zoomAround))
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector3 scale = grid.localScale;
            
            scale.x = shift ? scale.x : Mathf.Clamp(scale.x + (delta * zoomSensitivity), minimumZoom, maximumZoom);
            scale.y = ctrl ? scale.y : Mathf.Clamp(scale.y + (delta * zoomSensitivity), minimumZoom, maximumZoom);
            scale.z = 1f;
            
            grid.localScale = scale;

            SetGridSize();
            
            OnScaleChanged.Invoke(scale);
        }

        private void SetGridSize()
        {
            Vector3 localScale = grid.localScale;
            
            grid.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, content.rect.width * (1f / localScale.x));
            grid.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.rect.height * (1f / localScale.y));
        }

        public void ZoomToFit(float width, float height)
        {
            Vector2 visible = ViewRect.rect.size;
            float min = Mathf.Min(visible.x / width, visible.y / height);
            
            Vector3 zoom = new Vector3(min, min, 1f);

            grid.localScale = zoom;
            
            SetContentAnchoredPosition(Vector2.zero);
            
            OnScaleChanged.Invoke(zoom);
        }
        #endregion

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle)
                return;

            _velocity = Vector2.zero;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            _pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
            _contentStartPosition = content.anchoredPosition;
            _dragging = true;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle)
                return;

            _dragging = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            if (eventData.button != PointerEventData.InputButton.Middle)
                return;

            if (!IsActive())
                return;
            
            bool ctrl = InputUtility.Ctrl();
            bool shift = InputUtility.Shift();

            if (ctrl || shift)
            {
                PerformZoom(eventData.delta.normalized.x, eventData.position, ctrl, shift);
                return;
            }
            
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(ViewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            Vector2 pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition + pointerDelta;

            SetContentAnchoredPosition(position);
        }

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (position != content.anchoredPosition)
            {
                content.anchoredPosition = position;
                UpdateBounds();
                
                SetGridSize();
            }
        }

        protected virtual void LateUpdate()
        {
            if (!content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;

            if (deltaTime > 0.0f)
            {
                if (!_dragging && _velocity != Vector2.zero)
                {
                    Vector2 position = content.anchoredPosition;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        if (inertia)
                        {
                            _velocity[axis] *= Mathf.Pow(decelerationRate, deltaTime);
                            
                            if (Mathf.Abs(_velocity[axis]) < 1)
                                _velocity[axis] = 0;
                            
                            position[axis] += _velocity[axis] * deltaTime;
                        }
                        else _velocity[axis] = 0;
                    }

                    SetContentAnchoredPosition(position);
                }

                if (_dragging && inertia)
                {
                    Vector3 newVelocity = (content.anchoredPosition - _prevPosition) / deltaTime;
                    _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
                }
            }

            if (_viewBounds != _prevViewBounds || _contentBounds != _prevContentBounds || content.anchoredPosition != _prevPosition)
            {
                UpdateScrollbars();
                UISystemProfilerApi.AddMarker("AnimationCurveScrollRect.value", this);

                ResizeContentForPositionOffset();

                onPositionChanged.Invoke(NormalizedPosition);
                UpdatePrevData();
            }

            UpdateScrollbarVisibility();
        }

        /// <summary>
        /// Resize the content so the sizes are at least as big as the viewport
        /// </summary>
        private void ResizeContentForPositionOffset()
        {
            Rect viewportRect = viewport.rect;
            Vector2 contentPivot = content.pivot;

            // Calculate the contents offset to the center of the viewport
            Vector2 centerOffset = new Vector2(-(viewportRect.width * contentPivot.x), viewportRect.height * contentPivot.y);

            // Get the local position of the content and apply the offset so the position is
            // calculated at the center of the original size of the content rect
            Vector2 position = (Vector2)content.localPosition + centerOffset;

            // Get the world corners for both the viewport and the content
            viewport.GetWorldCorners(_viewportCorners);
            content.GetWorldCorners(_contentCorners);

            // But we only need the bottom left and top right corners
            Vector3 viewportBL = _viewportCorners[0];
            Vector3 viewportTR = _viewportCorners[2];

            Vector3 contentBL = _contentCorners[0];
            Vector3 contentTR = _contentCorners[2];

            // Get the current offsets for the content container
            Vector2 offsetMin = content.offsetMin;
            Vector2 offsetMax = content.offsetMax;

            // Using the calculated position of where the center of the content in relation
            // to the view port we can now calculate the offset adjustments we need
            // A positive position value means we need to adjust the offsetMin, a negative
            // value means we need to adjust the offsetMax
            if (position.x < 0)
                offsetMax.x += (viewportTR.x - contentTR.x);
            else offsetMin.x += (viewportBL.x - contentBL.x);
            
            if (position.y < 0)
                offsetMax.y += (viewportTR.y - contentTR.y);
            else offsetMin.y += (viewportBL.y - contentBL.y);
            
            // Apply the new offsets
            content.offsetMin = offsetMin;
            content.offsetMax = offsetMax;
            
            SetGridSize();
        }
        
        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        private void UpdatePrevData()
        {
            if (!content)
                _prevPosition = Vector2.zero;
            else _prevPosition = content.anchoredPosition;

            _prevViewBounds = _viewBounds;
            _prevContentBounds = _contentBounds;
        }

        private void UpdateScrollbars()
        {
            if (horizontalScrollbar)
            {
                if (_contentBounds.size.x > 0)
                    horizontalScrollbar.size = Mathf.Clamp01((_viewBounds.size.x) / _contentBounds.size.x);
                else horizontalScrollbar.size = 1;

                horizontalScrollbar.value = HorizontalNormalizedPosition;
            }

            if (verticalScrollbar)
            {
                if (_contentBounds.size.y > 0)
                    verticalScrollbar.size = Mathf.Clamp01((_viewBounds.size.y) / _contentBounds.size.y);
                else verticalScrollbar.size = 1;

                verticalScrollbar.value = VerticalNormalizedPosition;
            }
        }

        public Vector2 NormalizedPosition
        {
            get => new Vector2(HorizontalNormalizedPosition, VerticalNormalizedPosition);
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float HorizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();

                if ((_contentBounds.size.x <= _viewBounds.size.x) || Mathf.Approximately(_contentBounds.size.x, _viewBounds.size.x))
                    return (_viewBounds.min.x > _contentBounds.min.x) ? 1 : 0;
                return (_viewBounds.min.x - _contentBounds.min.x) / (_contentBounds.size.x - _viewBounds.size.x);
            }
            set => SetNormalizedPosition(value, 0);
        }

        public float VerticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((_contentBounds.size.y <= _viewBounds.size.y) || Mathf.Approximately(_contentBounds.size.y, _viewBounds.size.y))
                    return (_viewBounds.min.y > _contentBounds.min.y) ? 1 : 0;

                return (_viewBounds.min.y - _contentBounds.min.y) / (_contentBounds.size.y - _viewBounds.size.y);
            }
            set => SetNormalizedPosition(value, 1);
        }

        private void SetHorizontalNormalizedPosition(float value)
            => SetNormalizedPosition(value, 0);

        private void SetVerticalNormalizedPosition(float value)
            => SetNormalizedPosition(value, 1);

        /// <summary>
        /// >Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();

            float hiddenLength = _contentBounds.size[axis] - _viewBounds.size[axis];

            float contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;

            float newAnchoredPosition = content.anchoredPosition[axis] + contentBoundsMinPosition - _contentBounds.min[axis];

            Vector3 anchoredPosition = content.anchoredPosition;
            if (Mathf.Abs(anchoredPosition[axis] - newAnchoredPosition) > 0.01f)
            {
                anchoredPosition[axis] = newAnchoredPosition;
                content.anchoredPosition = anchoredPosition;
                _velocity[axis] = 0;
                UpdateBounds();
            }
        }

        protected override void OnRectTransformDimensionsChange()
            => SetDirty();

        private bool HScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.x > _viewBounds.size.x + 0.01f;
                return true;
            }
        }

        private bool VScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.y > _viewBounds.size.y + 0.01f;
                return true;
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical()
        {
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleWidth => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleHeight => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual int layoutPriority => -1;

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();
            UpdateCachedData();
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();

            Rect rect = ViewRect.rect;
            _viewBounds = new Bounds(rect.center, rect.size);
            _contentBounds = GetBounds();
        }

        private void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(verticalScrollbar);
            UpdateOneScrollbarVisibility(horizontalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (!scrollbar.gameObject.activeSelf)
                    scrollbar.gameObject.SetActive(true);
            }
        }

        private void UpdateScrollbarLayout()
        {
            if (horizontalScrollbar)
            {
                _tracker.Add(this, _horizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);

                _horizontalScrollbarRect.anchorMin = new Vector2(0, _horizontalScrollbarRect.anchorMin.y);
                _horizontalScrollbarRect.anchorMax = new Vector2(1, _horizontalScrollbarRect.anchorMax.y);
                _horizontalScrollbarRect.anchoredPosition = new Vector2(0, _horizontalScrollbarRect.anchoredPosition.y);

                if (VScrollingNeeded)
                    _horizontalScrollbarRect.sizeDelta = new Vector2(-(_vSliderWidth + verticalScrollbarSpacing), _horizontalScrollbarRect.sizeDelta.y);
                else _horizontalScrollbarRect.sizeDelta = new Vector2(0, _horizontalScrollbarRect.sizeDelta.y);
            }

            if (verticalScrollbar)
            {
                _tracker.Add(this, _verticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);

                _verticalScrollbarRect.anchorMin = new Vector2(_verticalScrollbarRect.anchorMin.x, 0);
                _verticalScrollbarRect.anchorMax = new Vector2(_verticalScrollbarRect.anchorMax.x, 1);
                _verticalScrollbarRect.anchoredPosition = new Vector2(_verticalScrollbarRect.anchoredPosition.x, 0);

                if (HScrollingNeeded)
                    _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, -(_hSliderHeight + horizontalScrollbarSpacing));
                else _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        private void UpdateBounds()
        {
            _viewBounds = new Bounds(ViewRect.rect.center, ViewRect.rect.size);
            _contentBounds = GetBounds();
        }

        private Bounds GetBounds()
        {
            if (!content)
                return new Bounds();

            content.GetWorldCorners(_contentCorners);

            Matrix4x4 viewWorldToLocalMatrix = ViewRect.worldToLocalMatrix;
            return InternalGetBounds(_contentCorners, ref viewWorldToLocalMatrix);
        }

        private static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        /// <summary>
        /// Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.
        /// </summary>
        private void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        /// <summary>
        /// Override to alter or add to the code that caches data to avoid repeated heavy operations.
        /// </summary>
        private void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);

            _viewRectTransform = null;
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
            => SetDirtyCaching();
        #endif

        [Serializable]
        public class AnimationCurveScrollRectEvent : UnityEvent<Vector2>
        {
        }
    }
}
