﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    [DefaultExecutionOrder(-60)]
    public class RuntimeWindow : DragDropTarget
    {
        public event EventHandler Activated;
        public event EventHandler Deactivated;

        private bool m_isActivated;

        [SerializeField]
        private bool m_canActivate = true;
        public bool CanActivate
        {
            get { return m_canActivate; }
            set { m_canActivate = value; }
        }

        [SerializeField]
        private bool m_activateOnAnyKey = false;
        public bool ActivateOnAnyKey
        {
            get { return m_activateOnAnyKey; }
            set { m_activateOnAnyKey = true; }
        }

        public virtual Camera Camera
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        [SerializeField]
        internal Pointer m_pointer;
        public virtual Pointer Pointer
        {
            get { return m_pointer; }
        }

        private IOCContainer m_container = new IOCContainer();
        public IOCContainer IOCContainer
        {
            get { return m_container; }
        }

        [SerializeField]
        private RuntimeWindowType m_windowType = RuntimeWindowType.Scene;

        public virtual RuntimeWindowType WindowType
        {
            get { return m_windowType; }
            set
            {
                if (m_windowType != value)
                {
                    m_index = Editor.GetIndex(value);
                    m_windowType = value;
                }
            }
        }

        private int m_index;
        public virtual int Index
        {
            get { return m_index; }
        }

        private int m_depth;
        public virtual int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        private CanvasGroup m_canvasGroup;
        protected CanvasGroup CanvasGroup
        {
            get { return m_canvasGroup; }
        }

        private Canvas m_canvas;
        protected Canvas Canvas
        {
            get { return m_canvas; }
        }

        [SerializeField]
        private Image m_background;
        public Image Background
        {
            get { return m_background; }
        }

        public override bool IsPointerOver
        {
            get { return base.IsPointerOver; }
            set
            {
                if (base.IsPointerOver != value)
                {
                    if (value)
                    {
                        Editor.SetPointerOverWindow(this);
                    }
                    else
                    {
                        Editor.SetPointerOverWindow(null);
                    }
                    base.IsPointerOver = value;
                }
            }
        }

        [SerializeField]
        private RectTransform m_viewRoot;
        public RectTransform ViewRoot
        {
            get { return m_viewRoot != null ? m_viewRoot : (RectTransform)transform; }
        }

        public string Args
        {
            get;
            set;
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if (m_viewRoot == null)
            {
                m_viewRoot = GetComponent<RectTransform>();
            }
            if (m_background == null)
            {
                if (!Editor.IsVR)
                {
                    m_background = GetComponent<Image>();
                    if (m_background == null)
                    {
                        m_background = gameObject.AddComponent<Image>();
                        m_background.color = new Color(0, 0, 0, 0);
                        m_background.raycastTarget = true;
                    }
                    else
                    {
                        m_background.raycastTarget = true;
                    }
                }
            }

            m_canvas = GetComponentInParent<Canvas>();
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (m_canvasGroup != null)
            {
                m_canvasGroup.blocksRaycasts = true;
                m_canvasGroup.ignoreParentGroups = true;
            }

            Editor.ActiveWindowChanged += OnActiveWindowChanged;
            if (WindowType != RuntimeWindowType.Custom)
            {
                m_index = Editor.GetIndex(WindowType);
            }
            else
            {
                m_index = 0;
            }

            if (m_pointer == null)
            {
                m_pointer = gameObject.AddComponent<Pointer>();
            }

            Editor.RegisterWindow(this);
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void UpdateOverride()
        {

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (Editor != null)
            {
                Editor.ActiveWindowChanged -= OnActiveWindowChanged;
                Editor.UnregisterWindow(this);
            }
        }

        protected virtual void OnTransformParentChanged()
        {
            EnableRaycasts();
        }

        public void EnableRaycasts()
        {
            if (m_canvasGroup != null)
            {
                m_canvasGroup.blocksRaycasts = true;
            }
        }

        public void DisableRaycasts()
        {
            if (!m_isActivated)
            {
                if (m_canvasGroup != null)
                {
                    m_canvasGroup.blocksRaycasts = false;
                }
            }
        }

        public virtual void HandleResize()
        {
        }

        protected virtual void OnActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (Editor.ActiveWindow == this)
            {
                if (!m_isActivated)
                {
                    m_isActivated = true;
                    if (WindowType == RuntimeWindowType.Game)
                    {
                        if (m_background != null)
                        {
                            m_background.raycastTarget = false;  // allow to interact with world space ui
                        }
                    }
                    OnActivated();
                    Activated?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                if (m_isActivated)
                {
                    m_isActivated = false;
                    if (m_background != null)
                    {
                        m_background.raycastTarget = true;
                    }
                    OnDeactivated();
                    Deactivated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected virtual void OnRectTransformDimensionsChange()
        {
        }

        protected virtual void OnActivated()
        {
        }

        protected virtual void OnDeactivated()
        {
        }
    }

    //[Obsolete] 
    public enum RuntimeWindowType
    {
        None = 0,
        Game = 1,
        Scene = 2,
        Hierarchy = 3,
        Project = 4,
        ProjectTree = 5,
        ProjectFolder = 6,
        Inspector = 7,
        Console = 8,
        Animation = 9,

        ToolsPanel = 21,

        ImportFile = 50,
        OpenProject = 51,
        SelectAssetLibrary = 52,
        ImportAssets = 53,
        SaveScene = 54,
        About = 55,
        SaveAsset = 56,
        SaveFile = 70,
        OpenFile = 72,

        SelectObject = 101,
        SelectColor = 102,
        SelectAnimationProperties = 109,

        Settings = 120,

        Empty = 256,
        EmptyDialog = 257,
        
        NoteEditor = 301,
        JudgmentLineEditor = 302,
        LevelEditor = 303,
        NoteDetailEditor = 304,

        Custom = 1 << 16,
    }
}
