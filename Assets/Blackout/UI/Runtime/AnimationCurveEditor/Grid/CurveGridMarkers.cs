using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// Creates and positions text objects in the correct place to line up with the grid
    /// </summary>
    public abstract class CurveGridMarkers<T> : MonoBehaviour where T : Component
    {
        #region Serialized Fields
        
        [SerializeField]
        private CurveScrollRect scrollRect;
        
        [SerializeField]
        private AnimationCurveEditor editor;
        
        [SerializeField, Tooltip("The direction to draw the value markers in.")]
        private MovementDirection direction = MovementDirection.Horizontal;
        
        [SerializeField, Tooltip("The separation between value markers at different scales.\nThe scale is the scale of the content transform.\nThe cells is the number of grid cells between each value marker.")]
        private LabelSeparation[] labelSeparation = new LabelSeparation[]
        {
            new LabelSeparation { scale = 0.5f, cells = 10f },
            new LabelSeparation { scale = 1f, cells = 5f },
            new LabelSeparation { scale = 2f, cells = 2f },
            new LabelSeparation { scale = 4f, cells = 1f },
            new LabelSeparation { scale = 5f, cells = 0.5f },
        };
        
        #endregion
       
        #region Private Fields
        
        private RectTransform _rectTransform;
        
        private readonly List<T> _textElements = new List<T>();
        private readonly Stack<T> _textPool = new Stack<T>();
        
        #endregion

        #region Properties
        
        public CurveScrollRect ScrollRect
        {
            get => scrollRect;
            set => scrollRect = value;
        }
        
        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        public MovementDirection Direction
        {
            get => direction;
            set => direction = value;
        }
        
        #endregion
        
        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            
            if (!editor)
            {
                editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
                
                if (!editor)
                {
                    enabled = false;
                    Debug.LogError("AnimationCurveGridShader requires an AnimationCurveEditor parent to function.");
                    return;
                }
            }
            
            if (!scrollRect)
            {
                scrollRect = ComponentUtility.GetComponentInParent<CurveScrollRect>(gameObject);
                if (!scrollRect)
                {
                    Debug.LogError("AnimationCurveValueMarkers must be a child of AnimationCurveScrollRect.");
                    return;
                }
            }
            
            scrollRect.OnPositionChanged.AddListener(OnScrollRectValueChanged);
            scrollRect.OnScaleChanged.AddListener(OnScrollRectValueChanged);
            
            OnScrollRectValueChanged(scrollRect.NormalizedPosition);
        }

        private void OnDisable()
        {
            if (scrollRect)
            {
                scrollRect.OnPositionChanged.RemoveListener(OnScrollRectValueChanged);
                scrollRect.OnScaleChanged.AddListener(OnScrollRectValueChanged);
            }
            
            for (int i = _textElements.Count - 1; i >= 0; i--)
            {
                T text = _textElements[i];
                text.gameObject.SetActive(false);
                _textPool.Push(text);
                _textElements.RemoveAt(i);
            }
        }

        private void OnScrollRectValueChanged(Vector2 v)
        {
            // The scale of the grid
            float scale = scrollRect.Grid.localScale[(int)direction];
            
            // The size of a single grid cell, scaled by the amount of zoom on the grid
            float cellSize = editor.Settings.gridPixelsPerCell * scale;

            // The anchored position of the grid parent
            float anchoredPosition = scrollRect.Content.anchoredPosition[(int)direction];
            
            // Calculate the local position of the grid parent and offset it by 5 cells so its at cell index 0
            float localPosition = anchoredPosition - scrollRect.Content.pivot[(int)direction] - (cellSize * 5f);
            
            // The size of this container
            float size = direction == MovementDirection.Horizontal ? _rectTransform.rect.width : _rectTransform.rect.height;
            
            // Find the closest separation value for the current scale
            int closestScaleIndex = 0;
            float minDifference = Mathf.Abs(scale - labelSeparation[0].scale);
            for (int i = 1; i < labelSeparation.Length; i++)
            {
                float s = labelSeparation[i].scale;
                
                float difference = Mathf.Abs(scale - s);

                if (difference < minDifference || (Mathf.Approximately(difference, minDifference) && s > labelSeparation[closestScaleIndex].scale))
                {
                    minDifference = difference;
                    closestScaleIndex = i;
                }
            }
            
            // The number of cells between text elements
            float cellSeparation = labelSeparation[closestScaleIndex].cells;
            
            // Retrieve the separation value based on the closest scale index
            float positionSeparation = cellSeparation * cellSize;
            
            // The amount of space to the left of the grid center
            float spaceLeft = localPosition + size * 0.5f;
            
            // The number of text cells to draw to the left
            int cellsLeft = (int)Mathf.Floor(spaceLeft / positionSeparation);
            
            // The number of text cells to draw in total
            int cellsVisible = Mathf.CeilToInt(size / positionSeparation);
           
            // Layout the text elements
            int idx = 0;
            for (int i = -cellsLeft; i < cellsVisible - cellsLeft; i++)
            {
                T t;
                
                // Try get an existing text element
                if (_textElements.Count > idx)
                    t = _textElements[idx];
                else
                {
                    // We don't currently have enough elements
                    if (_textPool.Count > 0)
                    {
                        // Fetch one from the pool
                        t = _textPool.Pop();
                        t.gameObject.SetActive(true);
                    }
                    else
                    {
                        // Otherwise instantiate a new one
                        t = Instantiate();
                        t.gameObject.SetActive(true);
                    }
                    
                    _textElements.Add(t);
                }
                
                float pos = (i * positionSeparation) + localPosition;

                ((RectTransform)t.transform).anchoredPosition = new Vector2(
                    direction == MovementDirection.Horizontal ? pos : 0,
                    direction == MovementDirection.Vertical ? pos : 0);
                
                SetText(t, ((i * cellSeparation) * 0.1f).ToString("0.0#", CultureInfo.CurrentCulture));
                
                idx++;
            }

            // Pool any unused text elements
            for (int i = _textElements.Count - 1; i >= cellsVisible; i--)
            {
                T t = _textElements[i];
                t.gameObject.SetActive(false);
                _textPool.Push(t);
                _textElements.RemoveAt(i);
            }
        }

        protected abstract T Instantiate();

        protected abstract void SetText(T t, string s);
        
        [Serializable]
        public class LabelSeparation
        {
            public float scale;
            public float cells;
        }
        
        public enum MovementDirection { Horizontal, Vertical }
    }
}