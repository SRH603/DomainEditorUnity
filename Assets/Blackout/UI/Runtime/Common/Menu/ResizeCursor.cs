using System.Collections.Generic;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// Handles changing the cursor image when inside a resize zone
    /// </summary>
    public class ResizeCursor : MonoBehaviour
    {
        [SerializeField]
        private Texture2D resizeNS;
        
        [SerializeField]
        private Texture2D resizeEW;
        
        [SerializeField]
        private Texture2D resizeNESW;
        
        [SerializeField]
        private Texture2D resizeNWSE;
        
        private Dictionary<ResizeCursorType, Texture2D> _cursors = new Dictionary<ResizeCursorType, Texture2D>();
        
        private Component _locker;
        private Texture2D _texture;

        private void Awake()
        {
            _cursors[ResizeCursorType.ResizeNS] = resizeNS;
            _cursors[ResizeCursorType.ResizeEW] = resizeEW;
            _cursors[ResizeCursorType.ResizeNESW] = resizeNESW;
            _cursors[ResizeCursorType.ResizeNWSE] = resizeNWSE;
        }
        
        public void SetCursor(Component component, ResizeCursorType value)
        {
            if (_locker)
                return;

            _locker = component;
            _texture = _cursors[value];
            
            Cursor.SetCursor(_texture, new Vector2(_texture.width * 0.5f, _texture.height * 0.5f), CursorMode.Auto);
        }
        
        public void ResetCursor(Component component)
        {
            if (_locker != component)
                return;

            _locker = null;
            _texture = null;
            
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!resizeNS)
                resizeNS = Resources.Load<Texture2D>("Blackout/Common/resize_cursor_ns");
            
            if (!resizeEW)
                resizeEW = Resources.Load<Texture2D>("Blackout/Common/resize_cursor_we");
            
            if (!resizeNESW)
                resizeNESW = Resources.Load<Texture2D>("Blackout/Common/resize_cursor_ne_sw");
            
            if (!resizeNWSE)
                resizeNWSE = Resources.Load<Texture2D>("Blackout/Common/resize_cursor_nw_se");
        }
        #endif
    }
}