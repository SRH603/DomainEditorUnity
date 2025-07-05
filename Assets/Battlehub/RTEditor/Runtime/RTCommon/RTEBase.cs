using Battlehub.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    [Serializable]
    public struct CameraLayerSettings
    {
        public readonly static CameraLayerSettings Default = new CameraLayerSettings(20, 21, 4, 17, 18, 19, 16);

        public int ResourcePreviewLayer;
        public int RuntimeGraphicsLayer;
        public int MaxGraphicsLayers;
        public int AllScenesLayer;
        public int ExtraLayer2;
        public int ExtraLayer;
        public int UIBackgroundLayer;

        public int RaycastMask
        {
            get
            {
                return ~((((1 << MaxGraphicsLayers) - 1) << RuntimeGraphicsLayer) | (1 << AllScenesLayer) | (1 << ExtraLayer) | (1 << ExtraLayer2) | (1 << ResourcePreviewLayer));
            }
        }

        public CameraLayerSettings(int resourcePreviewLayer, int runtimeGraphicsLayer, int maxLayers, int allSceneLayer, int extraLayer, int hiddenLayer, int uiBackgroundLayer)
        {
            ResourcePreviewLayer = resourcePreviewLayer;
            RuntimeGraphicsLayer = runtimeGraphicsLayer;
            MaxGraphicsLayers = maxLayers;
            AllScenesLayer = allSceneLayer;
            ExtraLayer = extraLayer;
            ExtraLayer2 = hiddenLayer;
            UIBackgroundLayer = uiBackgroundLayer;
        }
    }

    public class BusyContext : IDisposable
    {
        private IRTE m_editor;
        public BusyContext(IRTE editor)
        {
            m_editor = editor;
            m_editor.IsBusy = true;
        }

        public void Dispose()
        {
            if (m_editor != null)
            {
                m_editor.IsBusy = false;
            }
        }
    }


    public interface IRTE
    {
        event RTEEvent BeforePlaymodeStateChange;
        event RTEEvent PlaymodeStateChanging;
        event RTEEvent PlaymodeStateChanged;
        event RTEEvent<RuntimeWindow> ActiveWindowChanging;
        event RTEEvent<RuntimeWindow> ActiveWindowChanged;
        event RTEEvent<RuntimeWindow> WindowRegistered;
        event RTEEvent<RuntimeWindow> WindowUnregistered;
        event RTEEvent IsBusyChanged;
        event RTEEvent IsOpenedChanged;
        event RTEEvent IsDirtyChanged;
        event RTEEvent<GameObject[]> ObjectsRegistered;
        event RTEEvent<GameObject[]> ObjectsDuplicated;
        event RTEEvent<GameObject[]> ObjectsDeleted;

        CameraLayerSettings CameraLayerSettings
        {
            get;
        }

        IUIRaycaster Raycaster
        {
            get;
        }

        EventSystem EventSystem
        {
            get;
        }

        //[Obsolete]
        bool IsVR
        {
            get;
        }

        IInput Input
        {
            get;
        }

        ITouchInput TouchInput
        {
            get;
        }

        IRuntimeSelection Selection
        {
            get;
        }

        IRuntimeUndo Undo
        {
            get;
        }

        RuntimeTools Tools
        {
            get;
        }

        CursorHelper CursorHelper
        {
            get;
        }

        IRuntimeObjects Object
        {
            get;
        }

        IDragDrop DragDrop
        {
            get;
        }

        bool IsDirty
        {
            get;
            set;
        }

        bool IsOpened
        {
            get;
            set;
        }

        bool IsBusy
        {
            get;
            set;
        }

        BusyContext SetBusy();

        bool IsPlaymodeStateChanging
        {
            get;
        }

        bool IsPlaying
        {
            get;
            set;
        }

        bool IsApplicationPaused
        {
            get;
        }

        GameObject SceneRoot
        {
            get;
        }

        GameObject InstanceRoot
        {
            get;
        }

        GameObject HierarchyRoot
        {
            get;
        }

        Transform Root
        {
            get;
        }

        bool IsInputFieldActive
        {
            get;
        }

        bool IsInputFieldFocused
        {
            get;
        }

        void UpdateCurrentInputField();

        RuntimeWindow ActiveWindow
        {
            get;
        }

        RuntimeWindow PointerOverWindow
        {
            get;
        }

        RuntimeWindow[] Windows
        {
            get;
        }

        bool Contains(RuntimeWindow window);
        int GetIndex(RuntimeWindowType windowType);
        RuntimeWindow GetWindow(RuntimeWindowType windowType);
        void ActivateWindow(RuntimeWindowType window);
        void ActivateWindow(RuntimeWindow window);
        void SetPointerOverWindow(RuntimeWindow window);
        void RegisterWindow(RuntimeWindow window);
        void UnregisterWindow(RuntimeWindow window);
        void Close();
        Coroutine StartCoroutine(IEnumerator method);
        void StopCoroutine(IEnumerator method);

        void RegisterCreatedObjects(GameObject[] gameObjects, bool select = true);
        void AddGameObjectToHierarchy(GameObject gameObject, bool scaleStays = true);
        void Duplicate(GameObject[] gameObjects);
        Task DuplicateAsync(GameObject[] gameObjects);
        void Delete(GameObject[] gameObjects);
        Task DeleteAsync(GameObject[] gameObjects);
    }

    public delegate void RTEEvent();
    public delegate void RTEEvent<T>(T arg);

    [DefaultExecutionOrder(-90)]
    public class RTEBase : MonoBehaviour, IRTE
    {
        [SerializeField]
        private CameraLayerSettings m_cameraLayerSettings = CameraLayerSettings.Default;

        [SerializeField, HideInInspector]
        private bool m_createHierarchyRoot = false;

        [SerializeField]
        private bool m_useBuiltinUndo = true;

#pragma warning disable CS0414
        [SerializeField, HideInInspector, Obsolete]
        private bool m_enableVRIfAvailable = true;
#pragma warning restore CS0414

        [SerializeField]
        private bool m_isOpened = true;
        [SerializeField]
        private UnityEvent IsOpenedEvent = null;
        [SerializeField]
        private UnityEvent IsClosedEvent = null;

        public event RTEEvent BeforePlaymodeStateChange;
        public event RTEEvent PlaymodeStateChanging;
        public event RTEEvent PlaymodeStateChanged;
        public event RTEEvent<RuntimeWindow> ActiveWindowChanging;
        public event RTEEvent<RuntimeWindow> ActiveWindowChanged;
        public event RTEEvent<RuntimeWindow> WindowRegistered;
        public event RTEEvent<RuntimeWindow> WindowUnregistered;
        public event RTEEvent IsOpenedChanged;
        public event RTEEvent IsDirtyChanged;
        public event RTEEvent IsBusyChanged;
        public event RTEEvent<GameObject[]> ObjectsRegistered;
        public event RTEEvent<GameObject[]> ObjectsDuplicated;
        public event RTEEvent<GameObject[]> ObjectsDeleted;

        private DisabledInput m_disabledInput;
        private IInput m_input;
        private IInput m_activeInput;
        private ITouchInput m_touchInput;
        private ITouchInput m_activeTouchInput;

        private RuntimeSelection m_selection;
        private RuntimeTools m_tools = new RuntimeTools();
        private CursorHelper m_cursorHelper = new CursorHelper();
        private IRuntimeUndo m_undo;
        private DragDrop m_dragDrop;
        private IRuntimeObjects m_object;

        protected GameObject m_currentSelectedGameObject;
        protected TMP_InputField m_currentInputFieldTMP;
        protected InputField m_currentInputFieldUI;
        protected float m_zAxis;

        private IUIRaycaster m_uiRaycaster;
        public IUIRaycaster Raycaster
        {
            get { return m_uiRaycaster; }
        }

        [SerializeField]
        protected EventSystem m_eventSystem;
        public EventSystem EventSystem
        {
            get { return m_eventSystem; }
        }

        protected readonly HashSet<GameObject> m_windows = new HashSet<GameObject>();
        protected RuntimeWindow[] m_windowsArray;
        public bool IsInputFieldActive
        {
            get { return m_currentInputFieldTMP != null || m_currentInputFieldUI != null; }
        }

        public bool IsInputFieldFocused
        {
            get
            {
                if (m_currentInputFieldTMP != null)
                {
                    return m_currentInputFieldTMP.isFocused;
                }
                if (m_currentInputFieldUI != null)
                {
                    return m_currentInputFieldUI.isFocused;
                }
                return false;
            }

        }

        private RuntimeWindow m_activeWindow;
        public virtual RuntimeWindow ActiveWindow
        {
            get { return m_activeWindow; }
        }

        private RuntimeWindow m_pointerOverWindow;
        public virtual RuntimeWindow PointerOverWindow
        {
            get { return m_pointerOverWindow; }
        }

        public virtual RuntimeWindow[] Windows
        {
            get { return m_windowsArray; }
        }

        public bool Contains(RuntimeWindow window)
        {
            return m_windows.Contains(window.gameObject);
        }

        public virtual CameraLayerSettings CameraLayerSettings
        {
            get { return m_cameraLayerSettings; }
        }

        //[Obsolete]
        public virtual bool IsVR
        {
            get;
            private set;
        }

        public virtual IInput Input
        {
            get { return m_activeInput; }
        }

        public virtual ITouchInput TouchInput
        {
            get { return m_activeTouchInput; }
        }

        public virtual IRuntimeSelection Selection
        {
            get { return m_selection; }
        }

        public virtual IRuntimeUndo Undo
        {
            get { return m_undo; }
        }

        public virtual RuntimeTools Tools
        {
            get { return m_tools; }
        }

        public virtual CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
        }

        public virtual IRuntimeObjects Object
        {
            get { return m_object; }
        }

        public virtual IDragDrop DragDrop
        {
            get { return m_dragDrop; }
        }

        private bool m_isDirty;
        public virtual bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    if (IsDirtyChanged != null)
                    {
                        IsDirtyChanged();
                    }
                }
            }
        }

        public virtual bool IsOpened
        {
            get { return m_isOpened; }
            set
            {
                if (m_isOpened != value)
                {
                    if (IsBusy)
                    {
                        return;
                    }

                    m_isOpened = value;
                    SetInput();
                    if (!m_isOpened)
                    {
                        IsPlaying = false;
                    }

                    if (!m_isOpened)
                    {
                        ActivateWindow(GetWindow(RuntimeWindowType.Game));
                    }

                    if (Root != null)
                    {
                        Root.gameObject.SetActive(m_isOpened);
                    }

                    if (IsOpenedChanged != null)
                    {
                        IsOpenedChanged();
                    }
                    if (m_isOpened)
                    {
                        if (IsOpenedEvent != null)
                        {
                            IsOpenedEvent.Invoke();
                        }
                    }
                    else
                    {
                        if (IsClosedEvent != null)
                        {
                            IsClosedEvent.Invoke();
                        }
                    }
                }
            }
        }

        private int m_counter;
        public virtual bool IsBusy
        {
            get { return m_counter > 0; }
            set
            {
                int newValue = value ? m_counter + 1 : m_counter - 1;
                newValue = Math.Max(0, newValue);

                if (m_counter != newValue)
                {
                    m_counter = newValue;
                    if (m_counter == 1)
                    {
                        Application.logMessageReceived += OnApplicationLogMessageReceived;
                    }
                    else if (m_counter == 0)
                    {
                        Application.logMessageReceived -= OnApplicationLogMessageReceived;
                    }

                    SetInput();
                    if (IsBusyChanged != null)
                    {
                        IsBusyChanged();
                    }
                }
            }
        }

        private void OnApplicationLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                m_counter = 1;
                IsBusy = false;
            }
        }

        public BusyContext SetBusy()
        {
            return new BusyContext(this);
        }

        private bool m_isPlayModeStateChanging;
        public virtual bool IsPlaymodeStateChanging
        {
            get { return m_isPlayModeStateChanging; }
        }

        private bool m_isPlaying;
        public virtual bool IsPlaying
        {
            get
            {
                return m_isPlaying;
            }
            set
            {
                if (IsBusy)
                {
                    return;
                }

                if (!m_isOpened && value)
                {
                    return;
                }

                if (m_isPlaying != value)
                {
                    if (BeforePlaymodeStateChange != null)
                    {
                        BeforePlaymodeStateChange();
                    }

                    m_isPlayModeStateChanging = true;
                    m_isPlaying = value;

                    //Wait for possible cleanup performed in BeforePlaymodeStateChange handler
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(CoIsPlayingChanged());
                    }
                    else
                    {
                        RaisePlaymodeStateChangeEvents();
                    }

                }
            }
        }

        private IEnumerator CoIsPlayingChanged()
        {
            yield return new WaitForEndOfFrame();

            RaisePlaymodeStateChangeEvents();
        }

        private void RaisePlaymodeStateChangeEvents()
        {
            if (PlaymodeStateChanging != null)
            {
                PlaymodeStateChanging();
            }

            if (PlaymodeStateChanged != null)
            {
                PlaymodeStateChanged();
            }
            m_isPlayModeStateChanging = false;
        }

        protected bool CreateHierarchyRoot
        {
            get { return m_createHierarchyRoot; }
            set { m_createHierarchyRoot = value; }
        }

        public virtual GameObject SceneRoot
        {
            get { return HierarchyRoot; }
        }

        public virtual GameObject InstanceRoot
        {
            get { return HierarchyRoot; }
        }

        public virtual GameObject HierarchyRoot
        {
            get;
            protected set;
        }

        public virtual Transform Root
        {
            get { return transform; }
        }

        private static IRTE Instance
        {
            get { return IOC.Resolve<IRTE>("Instance"); }
            set
            {
                if (value != null)
                {
                    IOC.Register("Instance", value);
                }
                else
                {
                    IOC.Unregister("Instance", Instance);
                }
            }
        }

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            Debug.Log($"RTE {RTEVersion.Version} initialized. Persistent Data Path: {Application.persistentDataPath}");
            IOC.RegisterFallback(RegisterRTE);
        }


        private static IRTE RegisterRTE()
        {
            if (Instance == null)
            {
                GameObject editor = new GameObject("RTE");
                RTEBase instance = editor.AddComponent<RTEBase>();
                instance.BuildUp(editor);
            }
            return Instance;
        }

        protected virtual void BuildUp(GameObject editor)
        {
            editor.AddComponent<RTEGraphics>();

            var scene = new GameObject("Scene");
            scene.transform.SetParent(editor.transform, false);
            scene.AddComponent<RectTransform>();
            scene.SetActive(false);

            var canvas = scene.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var baseSceneWindow = scene.AddComponent<RTESceneWindow>();
            if (Camera.main == null)
            {
                GameObject camera = new GameObject();
                camera.name = "RTE SceneView Camera";
                baseSceneWindow.Camera = camera.AddComponent<Camera>();
            }
            else
            {
                baseSceneWindow.Camera = Camera.main;
            }

            scene.SetActive(true);
        }

        private bool m_isPaused;
        public bool IsApplicationPaused
        {
            get { return m_isPaused; }
        }

#if UNITY_EDITOR
        private bool m_appQuit;
#endif
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            m_appQuit = true;
#endif
            m_isPaused = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (m_dragDrop != null)
            {
                m_dragDrop.Reset();
            }

            if (Application.isEditor)
            {
                return;
            }
            m_isPaused = !hasFocus;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (m_dragDrop != null)
            {
                m_dragDrop.Reset();
            }

            m_isPaused = pauseStatus;
        }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Another instance of RTE exists");
                return;
            }
            if (m_useBuiltinUndo)
            {
                m_undo = new RuntimeUndo(this);
            }
            else
            {
                m_undo = new DisabledUndo();
            }

            m_uiRaycaster = IOC.Resolve<IUIRaycaster>();
            if (m_uiRaycaster == null)
            {
                m_uiRaycaster = GetComponentInChildren<IUIRaycaster>();
            }

            IsVR = /*UnityEngine.XR.XRDevice.isPresent*/ false /* && m_enableVRIfAvailable*/;
            m_selection = new RuntimeSelection(this);
            m_dragDrop = new DragDrop(this);
            m_object = gameObject.GetComponent<RuntimeObjects>();
            m_disabledInput = new DisabledInput();
            m_activeInput = m_disabledInput;
            m_activeTouchInput = m_disabledInput;

            Instance = this;

            bool isOpened = m_isOpened;
            m_isOpened = !isOpened;
            IsOpened = isOpened;

            TryCreateHierarchyRoot();
            
            if (m_eventSystem == null)
            {
                m_eventSystem = UnityObjectExt.FindAnyObjectByType<EventSystem>();
            }
        }

        protected virtual void Start()
        {
            InputLow input = new InputLow();
            m_input = IOC.Resolve<IInput>();
            if (m_input == null)
            {
                m_input = input;
            }

            m_touchInput = IOC.Resolve<ITouchInput>();
            if (m_touchInput == null)
            {
                m_touchInput = input;
            }

            SetInput();

            if (m_eventSystem == null)
            {
                m_eventSystem = UnityObjectExt.FindAnyObjectByType<EventSystem>();
            }

            if (m_object == null)
            {
                m_object = gameObject.AddComponent<RuntimeObjects>();
            }
        }

        protected virtual void OnDestroy()
        {
            IsOpened = false;

            if (m_object != null)
            {
                m_object = null;
            }

            if (m_dragDrop != null)
            {
                m_dragDrop.Reset();
            }
            if (((object)Instance) == this)
            {
                Instance = null;
            }

            #if UNITY_EDITOR
            if (m_appQuit)
            {
                // required when running the Unity editor in "Don't reload domain and scene" mode
                IOC.ClearAll(); 
            }
            #endif
        }

        private void SetInput()
        {
            if (!IsOpened || IsBusy || m_input == null)
            {
                m_activeInput = m_disabledInput;
            }
            else
            {
                m_activeInput = m_input;
            }

            if (!IsOpened || IsBusy || m_touchInput == null)
            {
                m_activeTouchInput = m_disabledInput;
            }
            else
            {
                m_activeTouchInput = m_touchInput;
            }
        }

        public void RegisterWindow(RuntimeWindow window)
        {
            if (!m_windows.Contains(window.gameObject))
            {
                m_windows.Add(window.gameObject);
            }

            WindowRegistered?.Invoke(window);
            m_windowsArray = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).ToArray();
        }

        public void UnregisterWindow(RuntimeWindow window)
        {
            m_windows.Remove(window.gameObject);
            WindowUnregistered?.Invoke(window);
            m_windowsArray = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).ToArray();
        }

        protected virtual void Update()
        {
            UpdateCurrentInputField();

            bool mwheel = false;
            if (m_zAxis != Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z))))
            {
                mwheel = m_zAxis == 0;
                m_zAxis = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z)));
            }

            bool pointerDownOrUp = Input.GetPointerDown(0) ||
                Input.GetPointerDown(1) ||
                Input.GetPointerDown(2) ||
                Input.GetPointerUp(0);

            if (pointerDownOrUp || mwheel || Input.IsAnyKeyDown() && !IsInputFieldFocused)
            {
                if (m_uiRaycaster == null)
                {
                    if (IsPointerOverGameObject())
                    {
                        ActivateWindow(null);
                    }
                    else
                    {
                        RuntimeWindow window = GetWindow(RuntimeWindowType.Scene);
                        if (window != null)
                        {
                            if (window.Camera == null || !window.Camera.isActiveAndEnabled)
                            {
                                window = null;
                            }
                        }

                        ActivateWindow(window);
                    }
                }
                else
                {
                    List<RaycastResult> results = new List<RaycastResult>();
                    m_uiRaycaster.Raycast(results);

                    IEnumerable<Selectable> selectables = results.Select(r => r.gameObject.GetComponent<Selectable>()).Where(s => s != null);
                    if (selectables.Count() == 1)
                    {
                        Selectable selectable = selectables.First() as Selectable;
                        if (selectable != null)
                        {
                            selectable.Select();
                        }
                    }

                    foreach (RaycastResult result in results)
                    {
                        if (m_windows.Contains(result.gameObject))
                        {
                            RuntimeWindow editorWindow = result.gameObject.GetComponent<RuntimeWindow>();
                            if (pointerDownOrUp || editorWindow.ActivateOnAnyKey)
                            {
                                ActivateWindow(editorWindow);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool IsPointerOverGameObject()
        {
            if (m_eventSystem == null)
            {
                return false;
            }

            if (m_eventSystem.IsPointerOverGameObject())
            {
                return true;
            }

            if (TouchInput != null)
            {
                for (int i = 0; i < TouchInput.TouchCount; i++)
                {
                    var touch = TouchInput.GetTouch(i);

                    if (m_eventSystem.IsPointerOverGameObject(touch.fingerId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateCurrentInputField()
        {
            if (m_eventSystem != null && m_eventSystem.currentSelectedGameObject != null && m_eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                if (m_eventSystem.currentSelectedGameObject != m_currentSelectedGameObject)
                {
                    m_currentSelectedGameObject = m_eventSystem.currentSelectedGameObject;
                    if (m_currentSelectedGameObject != null)
                    {
                        m_currentInputFieldTMP = m_currentSelectedGameObject.GetComponent<TMP_InputField>();
                        if (m_currentInputFieldTMP == null)
                        {
                            m_currentInputFieldUI = m_currentSelectedGameObject.GetComponent<InputField>();
                        }
                    }
                    else
                    {
                        if (m_currentInputFieldTMP != null)
                        {
                            m_currentInputFieldTMP.DeactivateInputField();
                        }
                        m_currentInputFieldTMP = null;

                        if (m_currentInputFieldUI != null)
                        {
                            m_currentInputFieldUI.DeactivateInputField();
                        }
                        m_currentInputFieldUI = null;
                    }
                }
            }
            else
            {
                m_currentSelectedGameObject = null;
                if (m_currentInputFieldTMP != null)
                {
                    m_currentInputFieldTMP.DeactivateInputField();
                }
                m_currentInputFieldTMP = null;

                if (m_currentInputFieldUI != null)
                {
                    m_currentInputFieldUI.DeactivateInputField();
                }
                m_currentInputFieldUI = null;
            }
        }

        public int GetIndex(RuntimeWindowType windowType)
        {
            IEnumerable<RuntimeWindow> windows = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).Where(w => w.WindowType == windowType).OrderBy(w => w.Index);
            int freeIndex = 0;
            foreach (RuntimeWindow window in windows)
            {
                if (window.Index != freeIndex)
                {
                    return freeIndex;
                }
                freeIndex++;
            }
            return freeIndex;
        }

        public RuntimeWindow GetWindow(RuntimeWindowType window)
        {
            return m_windows.Select(w => w.GetComponent<RuntimeWindow>()).FirstOrDefault(w => w.WindowType == window);
        }

        public virtual void ActivateWindow(RuntimeWindowType windowType)
        {
            RuntimeWindow window = GetWindow(windowType);
            if (window != null)
            {
                ActivateWindow(window);
            }
        }

        public virtual void ActivateWindow(RuntimeWindow window)
        {
            if (m_activeWindow != window && (window == null || window.CanActivate))
            {
                RuntimeWindow deactivatedWindow = m_activeWindow;

                ActiveWindowChanging?.Invoke(window);
                m_activeWindow = window;
                ActiveWindowChanged?.Invoke(deactivatedWindow);
            }
        }

        public virtual void SetPointerOverWindow(RuntimeWindow window)
        {
            m_pointerOverWindow = window;
        }

        public void Close()
        {
            IsOpened = false;
            Destroy(gameObject);
        }

        public virtual void RegisterCreatedObjects(GameObject[] gameObjects, bool select = true)
        {
            ExposeToEditor[] exposeToEditor = gameObjects.Select(o => o.GetComponent<ExposeToEditor>()).Where(o => o != null).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
            if (exposeToEditor.Length == 0)
            {
                if (select)
                {
                    Selection.objects = gameObjects;
                }
                return;
            }

            bool isRecording = Undo.IsRecording;
            if (!isRecording)
            {
                Undo.BeginRecord();
            }

            if (exposeToEditor.Length == 0)
            {
                Debug.LogWarning("To register created object GameObject add ExposeToEditor script to it");
            }
            else
            {
                Undo.RegisterCreatedObjects(exposeToEditor);
            }

            if (select)
            {
                Selection.objects = gameObjects;
            }

            if (!isRecording)
            {
                Undo.EndRecord();
            }

            RaiseObjectsRegistered(gameObjects);
        }

        protected void RaiseObjectsRegistered(GameObject[] gameObjects)
        {
            if (ObjectsRegistered != null)
            {
                ObjectsRegistered(gameObjects);
            }
        }

        public virtual void AddGameObjectToHierarchy(GameObject go, bool scaleStays = true)
        {
            if (m_createHierarchyRoot)
            {
                GameObject instanceRoot = InstanceRoot;
                if (instanceRoot != null)
                {
                    var parent = go.transform.parent;
                    while (parent != null)
                    {
                        parent = parent.parent;
                        if (parent == instanceRoot.transform)
                        {
                            return;
                        }
                    }

                    Vector3 localScale = go.transform.localScale;

                    go.transform.SetParent(instanceRoot.transform, true);

                    if (scaleStays)
                    {
                        go.transform.localScale = localScale;
                    }
                }
            }
        }
        public virtual void Duplicate(GameObject[] gameObjects)
        {
            DuplicateAsync(gameObjects);
        }

        public virtual Task DuplicateAsync(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return Task.CompletedTask;
            }

            List<GameObject> duplicates = new List<GameObject>();
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go == null)
                {
                    continue;
                }

                ExposeToEditor exposed = go.GetComponent<ExposeToEditor>();
                if (exposed != null && !exposed.CanDuplicate)
                {
                    continue;
                }

                GameObject duplicate = Instantiate(go, go.transform.position, go.transform.rotation, go.transform.parent);
                duplicate.SetActive(true);
                duplicate.SetActive(go.activeSelf);
                duplicates.Add(duplicate);
            }

            GameObject[] duplicatesArray = duplicates.ToArray();
            if (duplicatesArray.Length > 0)
            {
                ExposeToEditor[] exposeToEditor = duplicates.Select(o => o.GetComponent<ExposeToEditor>()).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
                Undo.BeginRecord();
                Undo.RegisterCreatedObjects(exposeToEditor);
                Selection.objects = duplicatesArray;
                Undo.EndRecord();
            }

            RaiseObjectsDuplicated(duplicatesArray);

            return Task.CompletedTask;
        }

        protected void RaiseObjectsDuplicated(GameObject[] duplicatesArray)
        {
            if (ObjectsDuplicated != null)
            {
                // behavior changed (duplicates are returned rather than the original assets)
                ObjectsDuplicated(duplicatesArray);
            }
        }

        public virtual void Delete(GameObject[] gameObjects)
        {
            DeleteAsync(gameObjects);
        }

        public virtual Task DeleteAsync(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return Task.CompletedTask;
            }

            ExposeToEditor[] exposeToEditor = gameObjects.Select(o => o.GetComponent<ExposeToEditor>()).Where(exposed => exposed != null && exposed.CanDelete).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
            if (exposeToEditor.Length == 0)
            {
                return Task.CompletedTask;
            }

            HashSet<GameObject> removeObjectsHs = new HashSet<GameObject>(exposeToEditor.Select(exposed => exposed.gameObject));
            bool isRecording = Undo.IsRecording;
            if (!isRecording)
            {
                Undo.BeginRecord();
            }

            if (Selection.objects != null)
            {
                List<UnityEngine.Object> selection = Selection.objects.ToList();
                for (int i = selection.Count - 1; i >= 0; --i)
                {
                    if (removeObjectsHs.Contains(selection[i]))
                    {
                        selection.RemoveAt(i);
                    }
                }

                Selection.objects = selection.ToArray();
            }

            Undo.DestroyObjects(exposeToEditor);

            if (!isRecording)
            {
                Undo.EndRecord();
            }

            RaiseObjectsDeleted(gameObjects);

            return Task.CompletedTask;
        }

        protected void RaiseObjectsDeleted(GameObject[] gameObjects)
        {
            if (ObjectsDeleted != null)
            {
                ObjectsDeleted(gameObjects);
            }
        }

        protected void TryCreateHierarchyRoot()
        {
            if (m_createHierarchyRoot)
            {
                GameObject hierarchyRoot = FindHierarchyRoot();
                if (hierarchyRoot == null)
                {
                    hierarchyRoot = new GameObject("Scene");
                }

                hierarchyRoot.transform.position = Vector3.zero;
                hierarchyRoot.tag = ExposeToEditor.HierarchyRootTag;
                HierarchyRoot = hierarchyRoot;
            }
        }

        internal static GameObject FindHierarchyRoot()
        {
            GameObject hierarchyRoot = null;
            try
            {
                hierarchyRoot = GameObject.FindGameObjectWithTag(ExposeToEditor.HierarchyRootTag);
            }
            catch (Exception)
            {
                Debug.LogWarning($"Add '{ExposeToEditor.HierarchyRootTag}' tag in Tags & Layers Window.");
                Debug.LogWarning($"Using 'Respawn' tag instead of '{ExposeToEditor.HierarchyRootTag}'");
                ExposeToEditor.HierarchyRootTag = "Respawn";
            }

            return hierarchyRoot;
        }
    }
}
