using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTGizmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class BeforeSaveSceneEventArgs
    {
        public bool Cancel
        {
            get;
            private set;
        }

        public GameObject Scene
        {
            get;
            private set;
        }

        public BeforeSaveSceneEventArgs(GameObject scene)
        {
            Scene = scene;
        }
    }

    public class SaveSceneEventArgs
    {
        public GameObject Scene
        {
            get;
            private set;
        }

        public string ScenePath
        {
            get;
            private set;
        }

        public ID SceneID
        {
            get;
            private set;
        }

        public SaveSceneEventArgs(GameObject scene, string scenePath, ID sceneID)
        {
            Scene = scene;
            ScenePath = scenePath;
            SceneID = sceneID;
        }
    }

    public enum CompatibilityMode
    {
        /// <summary>
        /// Incompatible with RTSL projects
        /// </summary>
        None,

        /// <summary>
        /// Use this option to be able to open new and old projects.
        /// </summary>
        AssetDatabaseOverRTSL,

        /// <summary>
        /// Use legacy api and legacy windows (project, save scene, save asset, select object and import asset)
        /// Some of the new events or methods will not work or throw exceptions.
        /// </summary>
        LegacyRTSL
    }

    public interface IRuntimeEditor : IRTE, IAssetDatabaseModel
    {
        event EventHandler<BeforeSaveSceneEventArgs> BeforeSaveCurrentScene;

        event EventHandler<SaveSceneEventArgs> SaveCurrentScene;

        CompatibilityMode CompatibilityMode
        {
            get;
        }

        IAssetThumbnailUtil ThumbnailUtil
        {
            get;
        }

        
        void CreateWindow(string window);
        void CreateOrActivateWindow(string window);
        void ResetToDefaultLayout();

        string ProjectsRootFolderPath
        {
            get;
            set;
        }

        bool IsProjectSupported(string projectType);
        Task<ProjectListEntry[]> GetProjectsAsync();
        Task<ProjectListEntry> CreateProjectAsync(string projectPath);
        Task<ProjectListEntry> DeleteProjectAsync(string projectPath);
        Task ImportProjectAsync(Stream istream, string projectPath, string password = null);
        Task ExportProjectAsync(Stream ostream, string projectPath, string password = null);

        void NewScene(bool confirm = true);
        void SaveScene();
        void SaveSceneAs();
        Task SaveCurrentSceneAsync(ID folderID, string name);
        Task SaveCurrentSceneAsync(string path = null);

        void AddGameObjectToScene(GameObject go, bool select = true);
        void AddGameObjectToScene(GameObject go, Vector3 position, bool select = true);

        Task CreateAssetAsync(byte[] binaryData, string path, bool forceOverwrite = false, bool select = true);
        Task CreateAssetAsync(string text, string path, bool forceOverwrite = false, bool select = true);
        Task CreateAssetAsync(UnityObject obj, string path = null, bool forceOverwrite = false, bool? extractSubAssets = null, bool? variant = null, bool select = true);
        void SelectAsset(ID assetID);

        #region Legacy

        //[Obsolete("Use BeforeSaveCurrentScene")]
        event RTEEvent<UIControls.CancelArgs> BeforeSceneSave;

        //[Obsolete]
        event RTEEvent SceneSaving;

        // [Obsolete("Use SaveCurrentScene")]
        event RTEEvent SceneSaved;

        //[Obsolete("User BeforeOpenScene")]
        event RTEEvent SceneLoading;

        //[Obsolete("Use OpenScene")]
        event RTEEvent SceneLoaded;

        [Obsolete]
        void OverwriteScene(RTSL.Interface.AssetItem scene, Action<RTSL.Interface.Error> callback = null);

        [Obsolete]
        Task OverwriteSceneAsync(RTSL.Interface.ProjectItem scene);

        [Obsolete]
        void SaveSceneToFolder(RTSL.Interface.ProjectItem folder, string name, Action<RTSL.Interface.Error> callback = null);

        [Obsolete]
        Task<RTSL.Interface.ProjectItem[]> CreatePrefabAsync(RTSL.Interface.ProjectItem folder, ExposeToEditor obj, bool? includeDependencies);

        [Obsolete]
        Task<RTSL.Interface.ProjectItem[]> SaveAssetsAsync(UnityObject[] assets);

        [Obsolete]
        Task<RTSL.Interface.ProjectItem[]> DeleteAssetsAsync(RTSL.Interface.ProjectItem[] projectItems);

        [Obsolete]
        Task<RTSL.Interface.ProjectItem> UpdatePreviewAsync(UnityObject obj);

        [Obsolete]
        RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.ProjectItem[]> DeleteAssetsLegacy(RTSL.Interface.ProjectItem[] projectItems, Action<RTSL.Interface.ProjectItem[]> done);

        [Obsolete]
        RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> CreatePrefab(RTSL.Interface.ProjectItem folder, ExposeToEditor obj, bool? includeDependencies = null, Action<RTSL.Interface.AssetItem[]> done = null);

        [Obsolete]
        RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> SaveAssets(UnityObject[] assets, Action<RTSL.Interface.AssetItem[]> done = null);

        #endregion
    }

    [DefaultExecutionOrder(-92)]
    [RequireComponent(typeof(RuntimeObjects))]
    public class RuntimeEditor : RTEBase, IRuntimeEditor
    {
        public event EventHandler BeforeLoadProject;

        public event EventHandler LoadProject;

        public event EventHandler BeforeUnloadProject;

        public event EventHandler UnloadProject;

        public event EventHandler BeforeReloadProject;

        public event EventHandler ReloadProject;

        public event EventHandler ChangeCurrentFolder;

        public event EventHandler<AssetSelectionEventArgs> ChangeAssetSelection;

        public event EventHandler<CreateFolderEventArgs> CreateFolder;

        public event EventHandler<BeforeCreateAssetEventArgs> BeforeCreateAsset;

        public event EventHandler<CreateAssetEventArgs> CreateAsset;

        public event EventHandler<SaveAssetEventArgs> SaveAsset;

        public event EventHandler<BeforeSaveSceneEventArgs> BeforeSaveCurrentScene;

        public event EventHandler<SaveSceneEventArgs> SaveCurrentScene;

        public event EventHandler InitializeNewScene;

        public event EventHandler<SaveAssetEventArgs> UpdateAssetThumbnail;

        public event EventHandler<MoveAssetsEventArgs> BeforeMoveAssets;

        public event EventHandler<MoveAssetsEventArgs> MoveAssets;

        public event EventHandler<DuplicateAssetsEventArgs> DuplicateAssets;

        public event EventHandler<DeleteAssetsEventArgs> BeforeDeleteAssets;

        public event EventHandler<DeleteAssetsEventArgs> DeleteAssets;

        public event EventHandler<AssetEventArgs> BeforeOpenAsset;

        public event EventHandler<AssetEventArgs> OpenAsset;

        public event EventHandler<AssetEventArgs> BeforeOpenPrefab;

        public event EventHandler<AssetEventArgs> OpenPrefab;

        public event EventHandler<AssetEventArgs> BeforeClosePrefab;

        public event EventHandler<AssetEventArgs> ClosePrefab;

        public event EventHandler<AssetEventArgs> BeforeOpenScene;

        public event EventHandler<AssetEventArgs> OpenScene;

        public event EventHandler<InstancesEventArgs> InstantiateAssets;

        public event EventHandler<InstancesEventArgs> Detach;

        public event EventHandler<InstanceEventArgs> SetDirty;

        public new event EventHandler<InstancesEventArgs> Duplicate;

        public event EventHandler<InstancesEventArgs> Release;

        public event EventHandler<InstanceEventArgs> BeforeApplyChanges;

        public event EventHandler<InstanceEventArgs> ApplyChanges;

        public event EventHandler<InstanceEventArgs> BeforeApplyChangesToBase;

        public event EventHandler<InstanceEventArgs> ApplyChangesToBase;

        public event EventHandler<InstanceEventArgs> BeforeRevertChangesToBase;

        public event EventHandler<InstanceEventArgs> RevertChangesToBase;

        private const string RTSLProject = "RTSL";
        private const string AssetDatabaseProject = "AssetDatabase";
        private string m_currentProjectType;
        private string CurrentProjectType
        {
            get { return m_currentProjectType; }
            set { m_currentProjectType = value; }
        }

        [Tooltip("If true, Runtime Editor sets Time.timeScale to 0 in edit mode and Time.timeScale to 'm_playModeTimeScale' in play mode. Useful for editing Rigidbodies or any objects with scripts that update themselves using Time.deltaTime.")]
        [SerializeField]
        private bool m_useZeroTimeScaleInEditMode = false;

        [Tooltip("Time.timeScale value used when m_useZeroTimeScaleInEditMode is true.")]
        [SerializeField]
        private float m_playModeTimeScale = 1.0f;

        [SerializeField]
        private CompatibilityMode m_compatibilityMode = CompatibilityMode.None;

        [SerializeField]
        private GameObject m_assetDatabasePrefab = null;

        [SerializeField]
        private GameObject m_assetDatabaseOverRTSLPrefab = null;

        public CompatibilityMode CompatibilityMode
        {
            get { return m_compatibilityMode; }
        }

        private IAssetThumbnailUtil m_thumbnailUtil;

        public IAssetThumbnailUtil ThumbnailUtil
        {
            get { return m_thumbnailUtil; }
        }

        private const string k_DefaultProjectKey = "RuntimeEditor.DefaultProjectKey";
        private const string k_DefaultProjectTypeKey = "RuntimeEditor.DefaultProjectKey.ProjectType";

        [Serializable]
        public class Settings
        {
            [Tooltip("Set the folder path where RuntimeEditor stores projects. By default, Application.persistentDataPath is used")]
            public string ProjectsRootFolderPath = null;

            [Tooltip("This specifies whether the default project will automatically open after launching the runtime editor. By default, it is set to true. If set to false, you must manually call IRuntimeEditor.LoadProjectAsync")]
            public bool OpenDefaultProject = true;

            [Tooltip("Specifies whether the opened project will be closed and unloaded automatically after closing the runtime editor. By default, this is set to false. If set to true, the project will be unloaded and all assets will be destroyed.")]
            public bool CloseProject = false;

            [Tooltip("Name of the project opened by default.")]
            public string DefaultProjectName = null;

            [Tooltip("Create a camera when creating a new empty scene. If set to false, a camera will not be created.")]
            public bool CreateCamera = true;

            [Tooltip("Create a directional light when creating a new empty scene. If set to false, no directional light will be created.")]
            public bool CreateLight = true;

            [Tooltip("Load layers after opening the runtime editor. If set to false, layers will be lazily loaded after opening the inspector.")]
            public bool LoadLayers = true;

            [Tooltip("If true, scene assets will be selected after they are created or saved.")]
            public bool SelectSceneAfterSaving = true;
        }

        [SerializeField]
        private Settings m_extraSettings;


        [SerializeField]
        private GameObject m_progressIndicator = null;

        public override bool IsBusy
        {
            get { return base.IsBusy; }
            set
            {
                base.IsBusy = value;

                if (m_progressIndicator != null)
                {
                    m_progressIndicator.gameObject.SetActive(base.IsBusy);
                }
            }
        }

        public override bool IsPlaying
        {
            get
            {
                return base.IsPlaying;
            }
            set
            {
                if (value != base.IsPlaying)
                {
                    if (CurrentPrefab != null)
                    {
                        m_wm.MessageBox(
                            m_localization.GetString("ID_RTEditor_Information", "Information"),
                            m_localization.GetString("ID_RTEditor_ClosePrefabBeforeEnteringPlayMode", "Close the prefab before entering play mode"));
                        return;
                    }

                    if (!IsPlaying)
                    {
                        RuntimeWindow gameView = GetWindow(RuntimeWindowType.Game);
                        if (gameView != null)
                        {
                            ActivateWindow(gameView);
                        }
                    }

                    if (m_useZeroTimeScaleInEditMode)
                    {
                        RefreshTimeScale(value);
                    }

                    base.IsPlaying = value;

                    if (!IsPlaying)
                    {
                        if (ActiveWindow == null || ActiveWindow.WindowType != RuntimeWindowType.Scene)
                        {
                            RuntimeWindow sceneView = GetWindow(RuntimeWindowType.Scene);
                            if (sceneView != null)
                            {
                                ActivateWindow(sceneView);
                            }
                        }
                    }
                }
            }
        }

        private void RefreshTimeScale(bool isInPlayMode)
        {
            Time.timeScale = isInPlayMode ? m_playModeTimeScale : 0.0f;
        }

        public override GameObject SceneRoot
        {
            get { return m_activeAssetDatabase.CurrentScene; }
        }

        public override GameObject InstanceRoot
        {
            get
            {
                var root = m_activeAssetDatabase.CurrentPrefab != null ?
                    m_activeAssetDatabase.CurrentPrefab :
                    m_activeAssetDatabase.CurrentScene;
                if (root != null)
                {
                    return root;
                }

                return base.SceneRoot;
            }
        }

        public override GameObject HierarchyRoot
        {
            get
            {
                var root = m_activeAssetDatabase.CurrentPrefab != null ?
                    m_activeAssetDatabase.CurrentPrefab.transform.parent.gameObject :
                    m_activeAssetDatabase.CurrentScene;
                if (root != null)
                {
                    return root;
                }

                return base.HierarchyRoot;
            }

            protected set
            {
                base.HierarchyRoot = value;
            }
        }

        private string m_activeAssetDatabaseHostName;
        private string ActiveAssetDatabaseHostName
        {
            set
            {
                if (m_activeAssetDatabase != null)
                {
                    UnsubscribeAssetDatabaseEvents();
                }

                m_activeAssetDatabaseHostName = value;
                if (m_activeAssetDatabaseHostName != null)
                {
                    m_activeAssetDatabase = IOC.Resolve<IAssetDatabaseModel>(m_activeAssetDatabaseHostName);
                    m_assetObjects = IOC.Resolve<IAssetObjectModel>(m_activeAssetDatabaseHostName);
                    m_thumbnailUtil = IOC.Resolve<IAssetThumbnailUtil>(m_activeAssetDatabaseHostName);
                    
                    SubscribeAssetDatabaseEvents();
                }
                else
                {
                    m_activeAssetDatabase = null;
                    m_assetObjects = null;
                    m_thumbnailUtil = null;
                }
            }
        }


        private IAssetDatabaseModel m_assetDatabase;
        private IAssetDatabaseModel m_assetDatabaseOverRTSL;
        private IAssetDatabaseModel m_activeAssetDatabase;
        private IAssetObjectModel m_assetObjects;
        private IWindowManager m_wm;
        private ILocalization m_localization;
        private IPlacementModel m_placement;
        private IGroupingModel m_grouping;
        private IInspectorModel m_inspector;
        private ILayoutStorageModel m_layoutStorage;
        private IContextMenuModel m_contextMenu;
        private IComponentFactoryModel m_componentFactory;
        private IProjectListModel m_projectList;

        private RuntimeSceneManagerInternal m_sceneManager;
        private RTSL.Interface.IProjectAsync m_project;

        private IAssetDatabaseModel CreateAssetDatabase(GameObject modelsRoot, GameObject assetDatabasePrefab)
        {
            assetDatabasePrefab.SetActive(false);
            var assetDatabaseHost = Instantiate(assetDatabasePrefab, modelsRoot.transform);
            assetDatabaseHost.name = assetDatabasePrefab.name;
            assetDatabaseHost.SetActive(true);
            assetDatabasePrefab.SetActive(true);

            return IOC.Resolve<IAssetDatabaseModel>(assetDatabasePrefab.name);
        }

        protected override void Awake()
        {
            IOC.RegisterFallback<IAssetDatabaseModel>(this);

            GizmoUtility.Initialize();

            m_sceneManager = new RuntimeSceneManagerInternal();
            m_sceneManager.NewSceneCreating += OnNewSceneCreating;
            m_sceneManager.NewSceneCreated += OnNewSceneCreated;
            IOC.Register<RTSL.Interface.IRuntimeSceneManager>(m_sceneManager);

            var modelsRoot = GetModelsRoot();
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (m_compatibilityMode == CompatibilityMode.AssetDatabaseOverRTSL)
                {
                    m_assetDatabaseOverRTSL = IOC.Resolve<IAssetDatabaseModel>(m_assetDatabaseOverRTSLPrefab.name);
                    if (m_assetDatabaseOverRTSL == null)
                    {
                        m_assetDatabaseOverRTSL = CreateAssetDatabase(modelsRoot, m_assetDatabaseOverRTSLPrefab);
                    }
                }

                m_assetDatabase = IOC.Resolve<IAssetDatabaseModel>(m_assetDatabasePrefab.name);
                if (m_assetDatabase == null)
                {
                    m_assetDatabase = CreateAssetDatabase(modelsRoot, m_assetDatabasePrefab);
                }

                if (m_assetDatabase != null && m_assetDatabase.IsProjectLoaded)
                {
                    ActiveAssetDatabaseHostName = m_assetDatabasePrefab.name;
                }
                else if (m_assetDatabaseOverRTSL != null && m_assetDatabaseOverRTSL.IsProjectLoaded)
                {
                    ActiveAssetDatabaseHostName = m_assetDatabaseOverRTSLPrefab.name;
                }
                else
                {
                    ActiveAssetDatabaseHostName = m_assetDatabase != null ?
                        m_assetDatabasePrefab.name :
                        m_assetDatabaseOverRTSLPrefab.name;
                }
                    
                CreateHierarchyRoot = true;
            }
            else
            {
                m_assetDatabaseOverRTSL = IOC.Resolve<IAssetDatabaseModel>(m_assetDatabaseOverRTSLPrefab.name);
                if (m_assetDatabaseOverRTSL == null)
                {
                    m_assetDatabaseOverRTSL = CreateAssetDatabase(modelsRoot, m_assetDatabaseOverRTSLPrefab);
                }

                ActiveAssetDatabaseHostName = m_assetDatabaseOverRTSLPrefab.name;
            }

            if (!RenderPipelineInfo.UseRenderTextures)
            {
                CameraLayerSettings layerSettings = CameraLayerSettings;
                Transform uiBgCameraTransform = transform.Find("UIBackgroundCamera");
                Transform uiCameraTransform = transform.Find("UICamera");
                Transform uiBgTransform = transform.Find("UIBackground");
                if (uiBgCameraTransform != null && uiCameraTransform != null && uiBgTransform != null)
                {
                    Camera uiBgCamera = uiBgCameraTransform.GetComponent<Camera>();
                    Camera uiCamera = uiCameraTransform.GetComponent<Camera>();
                    Canvas uiBg = uiBgTransform.GetComponent<Canvas>();
                    if (uiBgCamera != null && uiCamera != null && uiBg != null)
                    {
                        uiBgCamera.enabled = true;
                        uiBg.worldCamera = uiBgCamera;
                        uiBgCamera.gameObject.SetActive(true);

                        uiCamera.clearFlags = CameraClearFlags.Depth;
                        uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
                    }
                }
            }
            else
            {
                Transform uiBgCameraTransform = transform.Find("UIBackgroundCamera");
                if (uiBgCameraTransform != null)
                {
                    Destroy(uiBgCameraTransform.gameObject);
                }
            }


            bool canCreateLightAndCamera = CreateHierarchyRoot && FindHierarchyRoot() == null;
            
            base.Awake();

            if (canCreateLightAndCamera)
            {
                TryCreateLightAndCamera();
            }

            IOC.Resolve<IRTEAppearance>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_localization = IOC.Resolve<ILocalization>();
            if (m_useZeroTimeScaleInEditMode)
            {
                RefreshTimeScale(IsPlaying);
            }

            RegisterModels();
            SubscribeRTSLEvents();

            Selection.SelectionChanged += OnEditorSelectionChanged;
            Object.TransformChanged += OnObjectTransformChanged;
            Object.NameChanged += OnObjectName;
            Object.Destroyed += OnObjectDestroyed;
            Tools.ActiveToolChanged += OnActiveToolChanged;

            if (m_extraSettings == null)
            {
                m_extraSettings = new Settings();
            }

            ApplyExtraSettingsAsync();
        }

        protected override void Start()
        {
            base.Start();
            if (m_eventSystem == null)
            {
                m_eventSystem = gameObject.AddComponent<EventSystem>();
            }
            RTSLIgnoreEventSystem();
        }

        protected override void Update()
        {
            //Don't remove (disables default RTEBase.Update implementation)    ;
        }

        protected override async void OnDestroy()
        {
            Selection.SelectionChanged -= OnEditorSelectionChanged;
            Object.TransformChanged -= OnObjectTransformChanged;
            Object.NameChanged -= OnObjectName;
            Object.Destroyed -= OnObjectDestroyed;
            Tools.ActiveToolChanged -= OnActiveToolChanged;
            m_activeTool = null;

            base.OnDestroy();
            StopAllCoroutines();

            UnsubscribeRTSLEvents();
            UnsubscribeAssetDatabaseEvents();

            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating -= OnNewSceneCreating;
                m_sceneManager.NewSceneCreated -= OnNewSceneCreated;
                IOC.Unregister<RTSL.Interface.IRuntimeSceneManager>(m_sceneManager);
                m_sceneManager = null;
            }

            UnregisterModels();

            if (m_extraSettings.CloseProject)
            {
                if (m_activeAssetDatabase != null)
                {
                    await m_activeAssetDatabase.UnloadProjectAsync();
                }
            }

            m_wm = null;
            m_localization = null;
            ActiveAssetDatabaseHostName = null;

            GizmoUtility.Cleanup();

            IOC.UnregisterFallback<IAssetDatabaseModel>(this);
        }

        private void SubscribeAssetDatabaseEvents()
        {
            if (m_activeAssetDatabase != null)
            {
                m_activeAssetDatabase.BeforeLoadProject += OnBeforeLoadProject;
                m_activeAssetDatabase.LoadProject += OnLoadProject;
                m_activeAssetDatabase.BeforeUnloadProject += OnBeforeUnloadProject;
                m_activeAssetDatabase.UnloadProject += OnUnloadProject;
                m_activeAssetDatabase.BeforeReloadProject += OnBeforeReloadProject;
                m_activeAssetDatabase.ReloadProject += OnReloadProject;
                m_activeAssetDatabase.ChangeCurrentFolder += OnChangeCurrentFolder;
                m_activeAssetDatabase.ChangeAssetSelection += OnChangeAssetSelection;
                m_activeAssetDatabase.CreateFolder += OnCreateFolder;
                m_activeAssetDatabase.BeforeCreateAsset += OnBeforeCreateAsset;
                m_activeAssetDatabase.CreateAsset += OnCreateAsset;
                m_activeAssetDatabase.InitializeNewScene += OnInitializeNewScene;
                m_activeAssetDatabase.SaveAsset += OnSaveAsset;
                m_activeAssetDatabase.UpdateAssetThumbnail += OnUpdateAssetThumbnail;
                m_activeAssetDatabase.BeforeMoveAssets += OnBeforeMoveAssets;
                m_activeAssetDatabase.MoveAssets += OnMoveAssets;
                m_activeAssetDatabase.DuplicateAssets += OnDuplicateAssets;
                m_activeAssetDatabase.BeforeDeleteAssets += OnBeforeDeleteAssets;
                m_activeAssetDatabase.DeleteAssets += OnDeleteAssets;
                m_activeAssetDatabase.BeforeOpenAsset += OnBeforeOpenAsset;
                m_activeAssetDatabase.OpenAsset += OnOpenAsset;
                m_activeAssetDatabase.BeforeOpenPrefab += OnBeforeOpenPrefab;
                m_activeAssetDatabase.OpenPrefab += OnOpenPrefab;
                m_activeAssetDatabase.BeforeClosePrefab += OnBeforeClosePrefab;
                m_activeAssetDatabase.ClosePrefab += OnClosePrefab;
                m_activeAssetDatabase.BeforeOpenScene += OnBeforeOpenScene;
                m_activeAssetDatabase.OpenScene += OnOpenScene;
                m_activeAssetDatabase.InstantiateAssets += OnInstantiateAssets;
                m_activeAssetDatabase.Detach += OnDetach;
                m_activeAssetDatabase.SetDirty += OnSetDirty;
                m_activeAssetDatabase.Duplicate += OnDuplicate;
                m_activeAssetDatabase.Release += OnRelease;
                m_activeAssetDatabase.BeforeApplyChanges += OnBeforeApplyChanges;
                m_activeAssetDatabase.ApplyChanges += OnApplyChanges;
                m_activeAssetDatabase.BeforeApplyChangesToBase += OnBeforeApplyChangesToBase;
                m_activeAssetDatabase.ApplyChangesToBase += OnApplyChangesToBase;
                m_activeAssetDatabase.BeforeRevertChangesToBase += OnBeforeRevertChangesToBase;
                m_activeAssetDatabase.RevertChangesToBase += OnRevertChangesToBase;
            }
        }

        private void UnsubscribeAssetDatabaseEvents()
        {
            if (m_activeAssetDatabase != null)
            {
                m_activeAssetDatabase.BeforeLoadProject -= OnBeforeLoadProject;
                m_activeAssetDatabase.LoadProject -= OnLoadProject;
                m_activeAssetDatabase.BeforeUnloadProject -= OnBeforeUnloadProject;
                m_activeAssetDatabase.UnloadProject -= OnUnloadProject;
                m_activeAssetDatabase.BeforeReloadProject -= OnBeforeReloadProject;
                m_activeAssetDatabase.ReloadProject -= OnReloadProject;
                m_activeAssetDatabase.ChangeCurrentFolder -= OnChangeCurrentFolder;
                m_activeAssetDatabase.ChangeAssetSelection -= OnChangeAssetSelection;
                m_activeAssetDatabase.CreateFolder -= OnCreateFolder;
                m_activeAssetDatabase.BeforeCreateAsset -= OnBeforeCreateAsset;
                m_activeAssetDatabase.CreateAsset -= OnCreateAsset;
                m_activeAssetDatabase.InitializeNewScene -= OnInitializeNewScene;
                m_activeAssetDatabase.SaveAsset -= OnSaveAsset;
                m_activeAssetDatabase.UpdateAssetThumbnail -= OnUpdateAssetThumbnail;
                m_activeAssetDatabase.BeforeMoveAssets -= OnBeforeMoveAssets;
                m_activeAssetDatabase.MoveAssets -= OnMoveAssets;
                m_activeAssetDatabase.DuplicateAssets -= OnDuplicateAssets;
                m_activeAssetDatabase.BeforeDeleteAssets -= OnBeforeDeleteAssets;
                m_activeAssetDatabase.DeleteAssets -= OnDeleteAssets;
                m_activeAssetDatabase.BeforeOpenAsset -= OnBeforeOpenAsset;
                m_activeAssetDatabase.OpenAsset -= OnOpenAsset;
                m_activeAssetDatabase.BeforeOpenPrefab -= OnBeforeOpenPrefab;
                m_activeAssetDatabase.OpenPrefab -= OnOpenPrefab;
                m_activeAssetDatabase.BeforeClosePrefab -= OnBeforeClosePrefab;
                m_activeAssetDatabase.ClosePrefab -= OnClosePrefab;
                m_activeAssetDatabase.BeforeOpenScene -= OnBeforeOpenScene;
                m_activeAssetDatabase.OpenScene -= OnOpenScene;
                m_activeAssetDatabase.InstantiateAssets -= OnInstantiateAssets;
                m_activeAssetDatabase.Detach -= OnDetach;
                m_activeAssetDatabase.SetDirty -= OnSetDirty;
                m_activeAssetDatabase.Duplicate -= OnDuplicate;
                m_activeAssetDatabase.Release -= OnRelease;
                m_activeAssetDatabase.BeforeApplyChanges -= OnBeforeApplyChanges;
                m_activeAssetDatabase.ApplyChanges -= OnApplyChanges;
                m_activeAssetDatabase.BeforeApplyChangesToBase -= OnBeforeApplyChangesToBase;
                m_activeAssetDatabase.ApplyChangesToBase -= OnApplyChangesToBase;
                m_activeAssetDatabase.BeforeRevertChangesToBase -= OnBeforeRevertChangesToBase;
                m_activeAssetDatabase.RevertChangesToBase -= OnRevertChangesToBase;
            }
        }

        protected virtual void RegisterModels()
        {
            if (!IOC.IsFallbackRegistered<IPlacementModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_placement == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_placement = modelsRoot.AddComponent<PlacementModel>();
                    }
                    return m_placement;
                });
            }

            if (!IOC.IsFallbackRegistered<IGroupingModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_grouping == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_grouping = modelsRoot.AddComponent<GroupingModel>();
                    }

                    return m_grouping;
                });
            }

            if (!IOC.IsFallbackRegistered<IInspectorModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_inspector == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_inspector = modelsRoot.AddComponent<InspectorModel>();
                    }

                    return m_inspector;
                });
            }

            if (!IOC.IsFallbackRegistered<ILayoutStorageModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_layoutStorage == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_layoutStorage = modelsRoot.AddComponent<PlayerPrefsLayoutStorageModel>();
                    }

                    return m_layoutStorage;
                });
            }

            if (!IOC.IsFallbackRegistered<IContextMenuModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_contextMenu == null)
                    {
                        m_contextMenu = new ContextMenuModel();
                    }
                    return m_contextMenu;
                });
            }

            if (!IOC.IsFallbackRegistered<IProjectListModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_projectList == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_projectList = CompatibilityMode == CompatibilityMode.LegacyRTSL ?
                            new ProjectListLegacyModel() :
                            new ProjectListModel();
                    }
                    return m_projectList;
                });
            }

            if (!IOC.IsFallbackRegistered<IComponentFactoryModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_componentFactory == null)
                    {
                        m_componentFactory = new ComponentFactoryModel();
                    }
                    return m_componentFactory;
                });
            }
        }
        protected virtual void UnregisterModels()
        {
            IOC.UnregisterFallback<IPlacementModel>();
            m_placement = null;

            IOC.UnregisterFallback<IGroupingModel>();
            m_grouping = null;

            IOC.UnregisterFallback<IInspectorModel>();
            m_inspector = null;

            IOC.UnregisterFallback<ILayoutStorageModel>();
            m_layoutStorage = null;

            IOC.UnregisterFallback<IContextMenuModel>();
            var disposable = m_contextMenu as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            m_contextMenu = null;

            IOC.UnregisterFallback<IComponentFactoryModel>();
            disposable = m_componentFactory as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            m_componentFactory = null;

            IOC.UnregisterFallback<IProjectListModel>();
            m_projectList = null;
        }

        protected GameObject GetModelsRoot()
        {
            Transform models = transform.Find("Models");
            if (models == null)
            {
                models = transform;
            }
            return models.gameObject;
        }

        private async void ApplyExtraSettingsAsync()
        {
            if (!string.IsNullOrEmpty(m_extraSettings.ProjectsRootFolderPath))
            {
                if (m_project != null)
                {
                    await m_project.SetRootPathAsync(m_extraSettings.ProjectsRootFolderPath);
                }
            }

            if (m_extraSettings.OpenDefaultProject)
            {
                try
                {
                    IsBusy = true;
                    await Task.Yield();
                    await LoadProjectAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public void ResetToDefaultLayout()
        {
            ILayoutStorageModel layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            bool layoutExist = layoutStorage.LayoutExists(layoutStorage.DefaultLayoutName);
            if (layoutExist)
            {
                layoutStorage.DeleteLayout(layoutStorage.DefaultLayoutName);
            }

            m_wm.SetDefaultLayout();
        }

        #region IRTEditor methods
        public virtual void CreateWindow(string windowTypeName)
        {
            m_wm.CreateWindow(windowTypeName);
        }

        public virtual void CreateOrActivateWindow(string windowTypeName)
        {
            if (!m_wm.CreateWindow(windowTypeName))
            {
                if (m_wm.Exists(windowTypeName))
                {
                    if (!m_wm.IsActive(windowTypeName))
                    {
                        m_wm.ActivateWindow(windowTypeName);

                        Transform windowTransform = m_wm.GetWindow(windowTypeName);

                        RuntimeWindow window = windowTransform.GetComponentInChildren<RuntimeWindow>();
                        if (window != null)
                        {
                            base.ActivateWindow(window);
                        }
                    }
                }
            }
        }

        public override void ActivateWindow(RuntimeWindow windowToActivate)
        {
            base.ActivateWindow(windowToActivate);
            if (windowToActivate != null)
            {
                m_wm.ActivateWindow(windowToActivate.transform);
                windowToActivate.EnableRaycasts();
            }
            else
            {
                if (Windows != null)
                {
                    foreach (RuntimeWindow window in Windows)
                    {
                        window.EnableRaycasts();
                    }
                }
            }
        }

        public virtual async void NewScene(bool confirm)
        {
            await NewSceneAsync(confirm);
        }

        public async Task NewSceneAsync(bool confirm)
        {
            if (confirm)
            {
                var completionSource = new TaskCompletionSource<bool>();
                m_wm.Confirmation(m_localization.GetString("ID_RTEditor_CreateNewScene", "Create New Scene"),
                    m_localization.GetString("ID_RTEditor_DoYouWantCreateNewScene", "Do you want to create new scene?") + Environment.NewLine +
                    m_localization.GetString("ID_RTEditor_UnsavedChangesWillBeLost", "All unsaved changes will be lost"), async (dialog, args) =>
                    {
                        try
                        {
                            using var b = SetBusy();
                            await InitializeNewSceneAsync();
                            completionSource.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }
                    },
                    (dialog, args) => { },
                    m_localization.GetString("ID_RTEditor_Create", "Create"),
                    m_localization.GetString("ID_RTEditor_Cancel", "Cancel"));

                await completionSource.Task;
            }
            else
            {
                using var b = SetBusy();
                await InitializeNewSceneAsync();
            }
        }

        public virtual async void SaveScene()
        {
            await SaveSceneAsync();
        }

        private bool RaiseBeginSaveScene()
        {
            if (BeforeSceneSave != null)
            {
                var args = new UIControls.CancelArgs();
                BeforeSceneSave(args);
                if (args.Cancel)
                {
                    return false;
                }
            }

            if (BeforeSaveCurrentScene != null)
            {
                var args = new BeforeSaveSceneEventArgs(CurrentScene);
                BeforeSaveCurrentScene(this, args);
                if (args.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task SaveSceneAsync()
        {
            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_Information", "Unable to save scene"),
                    m_localization.GetString("ID_RTEditor_UnableToSaveSceneInPlayMode", "Unable to save scene in play mode"));
                return;
            }

            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (m_activeAssetDatabase.CurrentSceneID == ID.Empty)
                {
                    if (!RaiseBeginSaveScene())
                    {
                        return;
                    }

                    m_wm.CreateWindow(BuiltInWindowNames.SaveScene);
                    SetSaveAssetDialogSettings();
                }
                else
                {
                    string path = m_activeAssetDatabase.GetPath(m_activeAssetDatabase.CurrentSceneID);
                    using var b = SetBusy();
                    await SaveCurrentSceneAsync(path);
                }
            }
            else
            {
                if (m_project == null)
                {
                    throw new InvalidOperationException("Project is not initialized");
                }

                if (m_project.State.LoadedScene == null)
                {
                    m_wm.CreateWindow(BuiltInWindowNames.SaveScene);
                    SetSaveAssetDialogSettings();
                }
                else
                {
                    var scene = m_project.State.LoadedScene;
                    try
                    {
#pragma warning disable CS0612 // Type or member is obsolete
                        await OverwriteSceneAsync(scene);
#pragma warning restore CS0612 // Type or member is obsolete

                    }
                    catch (Exception e)
                    {
                        m_wm.MessageBox(m_localization.GetString("ID_RTEditor_UnableToSaveScene", "Unable to save scene"), e.ToString());
                    }
                }
            }
        }

        private void SetSaveAssetDialogSettings()
        {
            var saveAssetDialog = IOC.Resolve<ISaveAssetDialog>();
            if (saveAssetDialog != null)
            {
                saveAssetDialog.SelectSavedAssets = m_extraSettings.SelectSceneAfterSaving;
            }
        }

        public Task SaveCurrentSceneAsync(ID folderID, string name)
        {
            return SaveCurrentSceneAsync($"{GetPath(folderID)}/{name}");
        }

        public async Task SaveCurrentSceneAsync(string path)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = m_activeAssetDatabase.GetPath(m_activeAssetDatabase.CurrentSceneID);
                    if (string.IsNullOrEmpty(path))
                    {
                        path = m_activeAssetDatabase.GetUniquePath(m_activeAssetDatabase.CurrentFolderID, CurrentScene, "Scene");
                    }
                }

                string ext = m_activeAssetDatabase.GetExt(CurrentScene);
                if (!path.EndsWith(ext))
                {
                    path = $"{path}{ext}";
                }

                if (!RaiseBeginSaveScene())
                {
                    return;
                }

                await CreateAssetAsync(m_activeAssetDatabase.CurrentScene, path,
                    forceOverwrite: true,
                    extractSubAssets:false,
                    variant: false,
                    select: m_extraSettings.SelectSceneAfterSaving);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual void SaveSceneAs()
        {
            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_UnableToSaveScene", "Unable to save scene"),
                    m_localization.GetString("ID_RTEditor_UnableToSaveSceneInPlayMode", "Unable to save scene in play mode"));
                return;
            }

            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (!RaiseBeginSaveScene())
                {
                    return;
                }
            }
            else
            {
                if (m_project == null)
                {
                    throw new InvalidOperationException("Project is not initialized");
                }
            }

            CreateOrActivateWindow(BuiltInWindowNames.SaveScene);
            SetSaveAssetDialogSettings();
        }

        public void AddGameObjectToScene(GameObject go, bool select = true)
        {
            var placement = IOC.Resolve<IPlacementModel>();
            placement.AddGameObjectToScene(go, select);
        }

        public void AddGameObjectToScene(GameObject go, Vector3 position, bool select = true)
        {
            var placement = IOC.Resolve<IPlacementModel>();
            placement.AddGameObjectToScene(go, position, select);
        }

        public virtual Task CreateAssetAsync(byte[] binaryData, string path, bool forceOverwrite = false, bool select = true)
        {
            return CreateAssetAsyncInternal(binaryData, path, forceOverwrite, false, false, select);
        }

        public virtual Task CreateAssetAsync(string text, string path, bool forceOverwrite = false, bool select = true)
        {
            return CreateAssetAsyncInternal(text, path, forceOverwrite, false, false, select);
        }

        public virtual Task CreateAssetAsync(UnityObject obj, string path, bool forceOverwrite = false, bool? extractSubAssets = null, bool? variant = null, bool select = true)
        {
            return CreateAssetAsyncInternal(obj, path, forceOverwrite, extractSubAssets, variant, select);
        }

        protected virtual async Task CreateAssetAsyncInternal(object obj, string path, bool forceOverwrite = false, bool? extractSubAssets = null, bool? variant = null, bool select = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = m_activeAssetDatabase.GetPath(m_activeAssetDatabase.CurrentFolderID);
            }

            if (m_compatibilityMode == CompatibilityMode.LegacyRTSL || m_activeAssetDatabase.IsFolder(path) || !m_activeAssetDatabase.Exists(path))
            {
                await CreateAssetAsyncInternal(obj, path, extractSubAssets, variant);
            }
            else
            {
                if (forceOverwrite)
                {
                    //await m_activeAssetDatabase.DeleteAssetsAsync(new[] { path });
                    await CreateAssetAsyncInternal(obj, path, extractSubAssets, variant);
                }
                else
                {
                    throw new InvalidOperationException($"Asset {path} already exists");
                }
            }

            if (select && m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                ID id;
                if (obj is GameObject)
                {
                    id = m_activeAssetDatabase.GetAssetIDByInstance(obj);
                }
                else
                {
                    id = m_activeAssetDatabase.GetAssetID(obj);
                }

                if (id == ID.Empty && !m_activeAssetDatabase.IsFolder(path))
                {
                    id = m_activeAssetDatabase.GetAssetID(path);
                }

                if (id != ID.Empty)
                {
                    m_activeAssetDatabase.SelectedAssets = new[] { id };
                }
            }
        }

        protected virtual async Task CreateAssetAsyncInternal(object obj, string path, bool? extractSubAssets, bool? variant)
        {
            if (obj is GameObject go)
            {
                if (m_compatibilityMode == CompatibilityMode.LegacyRTSL || !ReferenceEquals(m_activeAssetDatabase.CurrentScene, obj))
                {
                    await CreatePrefabAsync(go, path, extractSubAssets, variant);
                }
                else
                {
                    await m_activeAssetDatabase.CreateAssetAsync(obj, path);
                }
            }
            else
            {
                await m_activeAssetDatabase.CreateAssetAsync(obj, path);
            }
        }

        protected virtual async Task CreatePrefabAsync(GameObject obj, string path, bool? extractSubAssets, bool? variant)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                await CreateOriginalPrefabOrPrefabVariant(obj, path, extractSubAssets, variant);

                foreach (var exposeToEditor in obj.GetComponentsInChildren<ExposeToEditor>(true))
                {
                    exposeToEditor.RaisePropertyChanged(nameof(gameObject));
                }
            }
            else
            {
#pragma warning disable CS0612 // Type or member is obsolete
                RTSL.Interface.ProjectItem folderProjectItem = null;
                if (path == null)
                {
                    IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
                    if (projectTree != null)
                    {
                        var projectItem = projectTree.SelectedItem;
                        if (projectItem == null || !projectItem.IsFolder)
                        {
                            projectItem = m_project.State.RootFolder;
                        }
                        folderProjectItem = projectItem;
                    }
                }
                else
                {
                    folderProjectItem = RTSL.Interface.IProjectAsyncExtensions.GetFolder(m_project.Utils, path);
                }

                await CreatePrefabAsync(folderProjectItem, obj.GetComponent<ExposeToEditor>(), extractSubAssets);
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        protected virtual async Task CreateOriginalPrefabOrPrefabVariant(GameObject obj, string path, bool? extractSubAssets, bool? variant)
        {
            bool canCreatePrefab = m_activeAssetDatabase.CanCreatePrefab(obj);
            bool canCreateVariant = m_activeAssetDatabase.CanCreatePrefabVariant(obj);

            if (canCreateVariant && canCreatePrefab)
            {
                if (!variant.HasValue)
                {
                    var completionSource = new TaskCompletionSource<bool>();
                    var lc = IOC.Resolve<ILocalization>();
                    var wm = IOC.Resolve<IWindowManager>();
                    wm.Confirmation(
                        lc.GetString("ID_RTEditor_CreatePrefab", "Create Prefab"),
                        lc.GetString("ID_RTEditor_PrefabOrPrefabVariant", "Would you like to create a new original Prefab or a variant of this Prefab?"),
                        async (sender, _) =>
                        {
                            await Task.Yield();
                            await CreateOriginalPrefab(obj, path, extractSubAssets);
                            completionSource.SetResult(true);
                        },
                        (sender, _) =>
                        {
                            completionSource.SetResult(true);
                        },
                        async (sender, _) =>
                        {
                            using var b = SetBusy();

                            await m_activeAssetDatabase.CreateAssetAsync(obj, path, true);
                            completionSource.SetResult(true);
                        },
                        lc.GetString("ID_RTEditor_OriginalPrefab", "Original Prefab"),
                        lc.GetString("ID_RTEditor_Cancel", "Cancel"),
                        lc.GetString("ID_RTEditor_PrefabVariant", "Prefab Variant"));

                    await completionSource.Task;
                }
                else if (variant == true)
                {
                    using var b = SetBusy();

                    await m_activeAssetDatabase.CreateAssetAsync(obj, path, true);
                }
                else
                {
                    await CreateOriginalPrefab(obj, path, extractSubAssets);
                }
            }
            else if (canCreateVariant)
            {
                using var b = SetBusy();

                await m_activeAssetDatabase.CreateAssetAsync(obj, path, true);
            }
            else if (canCreatePrefab)
            {
                await CreateOriginalPrefab(obj, path, extractSubAssets);
            }
        }

        protected virtual async Task CreateOriginalPrefab(GameObject obj, string path, bool? extractSubAssets = null)
        {
            if (!extractSubAssets.HasValue)
            {
                using (var b = SetBusy())
                {
                    var subAssets = await m_activeAssetDatabase.ExtractSubAssetsAsync(obj);
                    if (!subAssets.Any())
                    {
                        extractSubAssets = false;
                    }
                }
            }

            if (!extractSubAssets.HasValue)
            {
                var lc = IOC.Resolve<ILocalization>();
                var wm = IOC.Resolve<IWindowManager>();
                var completionSource = new TaskCompletionSource<bool>();

                wm.Confirmation(
                    lc.GetString("ID_RTEditor_CreatePrefab", "Create Prefab"),
                    lc.GetString("ID_RTEditor_extractSubAssets", "Extract sub assets?"),
                    async (sender, args) =>
                    {
                        try
                        {
                            using var b = SetBusy();

                            await m_activeAssetDatabase.CreateAssetAsync(obj, path, variant: false, extractSubassets: true);

                            completionSource.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }
                    },
                    async (sender, args) =>
                    {
                        try
                        {
                            using var b = SetBusy();

                            await m_activeAssetDatabase.CreateAssetAsync(obj, path, variant: false);
                            completionSource.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }

                    },
                    lc.GetString("ID_RTEditor_Yes", "Yes"),
                    lc.GetString("ID_RTEditor_No", "No"));

                await completionSource.Task;
            }
            else
            {
                using var b = SetBusy();

                await m_activeAssetDatabase.CreateAssetAsync(obj, path, variant: false, extractSubassets: extractSubAssets.Value);
            }
        }

        public void SelectAsset(ID assetID)
        {
            Asset asset = m_assetObjects.GetAsset(assetID);
            if (asset != null)
            {
                Selection.activeObject = asset;
            }
        }

        #endregion

        #region IRTE Overrides

        private bool CanDuplicate(GameObject go)
        {
            var exposeToEditor = go.GetComponent<ExposeToEditor>();
            return exposeToEditor == null || exposeToEditor.CanDuplicate;
        }

        public override Task DuplicateAsync(GameObject[] gameObjects)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (gameObjects.Any(go => InstanceRoot))
                {
                    gameObjects = gameObjects.Where(go => go != m_activeAssetDatabase.CurrentPrefab && CanDuplicate(go)).ToArray();
                }

                return m_activeAssetDatabase.DuplicateAsync(gameObjects);
            }

            return base.DuplicateAsync(gameObjects);
        }

        public override Task DeleteAsync(GameObject[] gameObjects)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (gameObjects != null && gameObjects.Any(go => InstanceRoot))
                {
                    gameObjects = gameObjects.Where(go => go != m_activeAssetDatabase.CurrentPrefab).ToArray();
                }
            }

            return base.DeleteAsync(gameObjects);
        }

        #endregion

        #region AssetDatabase EventHandlers

        private void OnBeforeLoadProject(object sender, EventArgs e)
        {
            Selection.objects = null;
            Undo.Purge();

            BeforeLoadProject?.Invoke(this, e);
        }

        private async void OnLoadProject(object sender, EventArgs e)
        {
            m_activeAssetDatabase.CurrentFolderID = m_activeAssetDatabase.RootFolderID;

            if (CompatibilityMode == CompatibilityMode.None)
            {
                if (m_extraSettings != null && m_extraSettings.LoadLayers)
                {
                    IRTE editor = IOC.Resolve<IRTE>();
                    editor.IsBusy = true;
                    await LayersEditor.LoadLayersAsync(layers =>
                    {
                        editor.IsBusy = false;
                    });
                }
            }

            LoadProject?.Invoke(this, e);
        }

        private void OnBeforeUnloadProject(object sender, EventArgs e)
        {
            BeforeUnloadProject?.Invoke(this, e);
        }

        private void OnUnloadProject(object sender, EventArgs e)
        {
            Selection.objects = null;
            Undo.Purge();

            TryCreateHierarchyRoot();
            TryCreateLightAndCamera();

            UnloadProject?.Invoke(this, e);
        }

        private void OnBeforeReloadProject(object sender, EventArgs e)
        {
            BeforeReloadProject?.Invoke(this, e);
        }

        private void OnReloadProject(object sender, EventArgs e)
        {
            ReloadProject?.Invoke(this, e);
        }

        private void OnChangeCurrentFolder(object sender, EventArgs e)
        {
            ChangeCurrentFolder?.Invoke(this, e);
        }

        private bool m_handleSelectionChange = false;
        private void HandleSelectionChange<T>(T args, Action<T> action)
        {
            if (m_handleSelectionChange)
            {
                return;
            }
            m_handleSelectionChange = true;
            try
            {
                action(args);
            }
            finally
            {
                m_handleSelectionChange = false;
            }
        }
        private void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            HandleSelectionChange(unselectedObjects, _ =>
            {
                if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
                {
                    return;
                }

                var selectedAssets = new List<ID>();
                if (!Selection.IsNullOrEmpty())
                {
                    foreach (var obj in Selection.objects)
                    {
                        Asset asset = obj as Asset;
                        if (asset != null)
                        {
                            selectedAssets.Add(asset.ID);
                        }
                        else
                        {
                            ID assetID = m_activeAssetDatabase.GetAssetID(obj);
                            if (assetID != ID.Empty)
                            {
                                selectedAssets.Add(assetID);
                            }
                        }
                    }
                }

                m_activeAssetDatabase.SelectedAssets = selectedAssets.ToArray();
            });
        }
        private void HandleAssetSelectionChange(ID[] assetIDs)
        {
            HandleSelectionChange(assetIDs, selectedAssets =>
            {
                static bool SupportsDirectEditing(IAssetDatabaseModel adb, ID id)
                {
                    return adb.IsLoaded(id) && !adb.CanInstantiateAsset(id);
                }

                var assets = selectedAssets.All(id => SupportsDirectEditing(m_activeAssetDatabase, id)) ?
                    selectedAssets.Select(id => m_activeAssetDatabase.GetAsset(id)) :
                    selectedAssets.Select(id => m_assetObjects.GetAsset(id));

                Selection.objects = assets.OfType<UnityObject>().ToArray();
            });
        }

        private void OnChangeAssetSelection(object sender, AssetSelectionEventArgs e)
        {
            HandleAssetSelectionChange(e.SelectedAssets);
            ChangeAssetSelection?.Invoke(this, e);
        }

        private void OnCreateFolder(object sender, CreateFolderEventArgs e)
        {
            CreateFolder?.Invoke(this, e);
        }

        private void OnBeforeCreateAsset(object sender, BeforeCreateAssetEventArgs e)
        {
            if (ReferenceEquals(e.Object, m_activeAssetDatabase.CurrentScene))
            {
                SceneSaving?.Invoke();
            }

            BeforeCreateAsset?.Invoke(this, e);
        }

        private void OnCreateAsset(object sender, CreateAssetEventArgs e)
        {
            var instances = m_activeAssetDatabase.GetInstancesByAssetID(e.AssetID).ToArray();
            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = instances[i];
                Undo.Erase(instance);

                var exposeToEditor = instance.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null)
                {
                    string name = instance.name;
                    instance.name = string.Empty;
                    exposeToEditor.SetName(name);
                }
            }

            if (m_activeAssetDatabase.IsScene(e.AssetID))
            {
                SceneSaved?.Invoke();

                if (CurrentSceneID == e.AssetID)
                {
                    SaveCurrentScene?.Invoke(this, new SaveSceneEventArgs(CurrentScene, GetPath(e.AssetID), e.AssetID));
                }
            }

            CreateAsset?.Invoke(this, e);
        }

        private void OnInitializeNewScene(object sender, EventArgs e)
        {
            InitializeNewScene?.Invoke(this, e);

            if (m_sceneManager != null)
            {
                m_sceneManager.RaiseNewSceneCreated();
            }
        }

        private void OnSaveAsset(object sender, SaveAssetEventArgs e)
        {
            SaveAsset?.Invoke(this, e);
        }


        private object[] m_notifyPreviewChangedArgs = new object[1];
        private void OnUpdateAssetThumbnail(object sender, SaveAssetEventArgs e)
        {
            if (m_inspector != null)
            {
                var asset = m_activeAssetDatabase.GetAsset(e.AssetID);
                m_notifyPreviewChangedArgs[0] = asset;
                m_inspector.NotifyPreviewsChanged(m_notifyPreviewChangedArgs);
            }

            UpdateAssetThumbnail?.Invoke(this, e);
        }

        private void OnBeforeMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            BeforeMoveAssets?.Invoke(this, e);
        }

        private void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            if (m_activeAssetDatabase.CurrentPrefab != null)
            {
                var currentPrefabID = m_activeAssetDatabase.GetAssetIDByInstance(m_activeAssetDatabase.CurrentPrefab);
                for (int i = 0; i < e.AssetID.Count; ++i)
                {
                    var id = e.AssetID[i];
                    if (id == currentPrefabID)
                    {
                        var exposeToEditor = m_activeAssetDatabase.CurrentPrefab.GetComponent<ExposeToEditor>();
                        if (exposeToEditor != null)
                        {
                            string name = m_activeAssetDatabase.GetDisplayName(id);
                            exposeToEditor.SetName(name);
                        }
                        break;
                    }
                }
            }

            HandleAssetSelectionChange(m_activeAssetDatabase.SelectedAssets);

            MoveAssets?.Invoke(this, e);
        }

        private void OnDuplicateAssets(object sender, DuplicateAssetsEventArgs e)
        {
            DuplicateAssets?.Invoke(this, e);
        }

        private void OnBeforeDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
            var selectedObjects = Selection.activeObject != null ?
                Selection.objects.ToList() :
                null;

            var assetObjects = new object[e.AssetID.Count];
            var assets = new Asset[e.AssetID.Count];
            var instances = new List<GameObject>();

            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                var id = e.AssetID[i];

                var assetObj = m_activeAssetDatabase.GetAsset(id);
                assetObjects[i] = assetObj;

                var asset = m_assetObjects.GetAsset(id);
                assets[i] = asset;

                instances.AddRange(m_activeAssetDatabase.GetInstancesByAssetID(id));

                if (selectedObjects != null)
                {
                    if (assetObj is UnityObject)
                    {
                        selectedObjects.Remove((UnityObject)assetObj);
                    }
                    selectedObjects.Remove(asset);
                    for (int j = 0; j < instances.Count; ++j)
                    {
                        selectedObjects.Remove(instances[j]);
                    }
                }
            }

            if (selectedObjects != null)
            {
                Selection.objects =
                    selectedObjects.Count > 0 ?
                    selectedObjects.ToArray() :
                    null;

            }

            if (assetObjects.OfType<UnityObject>().Any())
            {
                Undo.EraseFromSelection(assetObjects.OfType<UnityObject>().ToArray());
                for (int i = 0; i < assetObjects.Length; ++i)
                {
                    Undo.Erase(assetObjects[i]);
                }
            }

            Undo.EraseFromSelection(assets);
            for (int i = 0; i < assets.Length; ++i)
            {
                Undo.Erase(assets[i]);
            }

            Undo.EraseFromSelection(instances.ToArray());
            for (int i = 0; i < instances.Count; ++i)
            {
                Undo.Erase(instances[i]);
            }

            BeforeDeleteAssets?.Invoke(this, e);
        }

        private void OnDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
            DeleteAssets?.Invoke(this, e);
        }

        private void OnBeforeOpenAsset(object sender, AssetEventArgs e)
        {
            BeforeOpenAsset?.Invoke(this, e);
        }

        private void OnOpenAsset(object sender, AssetEventArgs e)
        {
            OpenAsset?.Invoke(this, e);
        }

        private void OnBeforeOpenPrefab(object sender, AssetEventArgs e)
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Can't open prefab in play mode");
            }

            BeforeOpenPrefab?.Invoke(this, e);
        }

        private void OnOpenPrefab(object sender, AssetEventArgs e)
        {
            Undo.Store();

            Selection.Select(m_activeAssetDatabase.CurrentPrefab, new[] { m_activeAssetDatabase.CurrentPrefab });

            OpenPrefab?.Invoke(this, e);
        }

        private void OnBeforeClosePrefab(object sender, AssetEventArgs e)
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Can't open close prefab in play mode");
            }

            BeforeClosePrefab?.Invoke(this, e);
        }

        private void OnClosePrefab(object sender, AssetEventArgs e)
        {
            IsPlaying = false;

            Undo.Restore();

            if (m_activeAssetDatabase.CurrentPrefab != null)
            {
                Selection.Select(m_activeAssetDatabase.CurrentPrefab, new[] { m_activeAssetDatabase.CurrentPrefab });
            }
            else
            {
                Asset asset = m_assetObjects.GetAsset(e.AssetID);
                if (asset != null)
                {
                    Selection.Select(asset, new[] { asset });
                }
                else
                {
                    Selection.Select(null, null);
                }
            }

            ClosePrefab?.Invoke(this, e);
        }

        private void OnInstantiateAssets(object sender, InstancesEventArgs e)
        {
            if (e.Instances.Length > 0 && e.Instances[0] != m_activeAssetDatabase.CurrentScene)
            {
                RegisterCreatedObjects(e.Instances, select: false);
            }

            InstantiateAssets?.Invoke(this, e);
        }

        private void OnBeforeOpenScene(object sender, AssetEventArgs e)
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Can't open scene in play mode");
            }

            Selection.objects = null;
            Undo.Purge();

            SceneLoading?.Invoke();
            BeforeOpenScene?.Invoke(this, e);
        }

        private void OnOpenScene(object sender, AssetEventArgs e)
        {
            SceneLoaded?.Invoke();
            OpenScene?.Invoke(this, e);
        }

        private void OnDetach(object sender, InstancesEventArgs e)
        {
            Detach?.Invoke(this, e);
        }

        private void OnSetDirty(object sender, InstanceEventArgs e)
        {
            SetDirty?.Invoke(this, e);
        }

        private void OnDuplicate(object sender, InstancesEventArgs e)
        {
            if (e.Instances.Length > 0)
            {
                ExposeToEditor[] exposeToEditor = e.Instances.Select(o => o.GetComponent<ExposeToEditor>()).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
                Undo.BeginRecord();
                Undo.RegisterCreatedObjects(exposeToEditor);
                Selection.objects = e.Instances.ToArray();
                Undo.EndRecord();
            }

            RaiseObjectsDuplicated(e.Instances);
            Duplicate?.Invoke(this, e);
        }

        private void OnRelease(object sender, InstancesEventArgs e)
        {
            Release?.Invoke(this, e);
        }

        private void OnBeforeApplyChanges(object sender, InstanceEventArgs e)
        {
            Undo.Purge();
            BeforeApplyChanges?.Invoke(this, e);
        }

        private void OnApplyChanges(object sender, InstanceEventArgs e)
        {
            ApplyChanges?.Invoke(this, e);
        }

        private void OnBeforeApplyChangesToBase(object sender, InstanceEventArgs e)
        {
            Undo.Purge();
            BeforeApplyChangesToBase?.Invoke(this, e);
        }

        private void OnApplyChangesToBase(object sender, InstanceEventArgs e)
        {
            ApplyChangesToBase?.Invoke(this, e);
        }

        private void OnBeforeRevertChangesToBase(object sender, InstanceEventArgs e)
        {
            Undo.Purge();
            BeforeRevertChangesToBase?.Invoke(this, e);
        }

        private void OnRevertChangesToBase(object sender, InstanceEventArgs e)
        {
            RevertChangesToBase?.Invoke(this, e);
        }

        private void OnObjectTransformChanged(ExposeToEditor obj)
        {
            if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                return;
            }

            if (m_inspector != null && m_inspector.IsEditing)
            {
                return;
            }

            if (Tools.ActiveTool == null)
            {
                return;
            }

            if (!Selection.IsSelected(obj.gameObject))
            {
                return;
            }

            var transform = obj.transform;
            if (m_activeAssetDatabase.IsInstance(transform) && !m_activeAssetDatabase.IsDirtyObject(transform))
            {
                m_activeAssetDatabase.SetDirtyAsync(transform);
            }
        }

        private async void OnObjectName(ExposeToEditor obj)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                if (m_activeAssetDatabase.CurrentPrefab == obj.gameObject)
                {
                    using var b = SetBusy();
                    var id = m_activeAssetDatabase.GetAssetIDByInstance(obj.gameObject);
                    var name = m_activeAssetDatabase.GetDisplayName(id);
                    if (name != obj.Name)
                    {
                        var parentItemID = m_activeAssetDatabase.GetParent(id);
                        string ext = m_activeAssetDatabase.GetExt(obj.gameObject);
                        string targetPath = m_activeAssetDatabase.GetUniquePath(parentItemID, $"{obj.Name}{ext}");
                        await m_activeAssetDatabase.MoveAssetsAsync(new[] { id }, new[] { targetPath });
                    }
                }
            }
        }

        private void OnObjectDestroyed(ExposeToEditor obj)
        {
            if (m_compatibilityMode != CompatibilityMode.LegacyRTSL)
            {
                m_activeAssetDatabase.ReleaseAsync(new[] { obj.gameObject });
            }
        }

        private object m_activeTool;
        private void OnActiveToolChanged()
        {
            if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                return;
            }

            if (Tools.ActiveTool == null)
            {
                BaseGizmo gizmo = m_activeTool as BaseGizmo;
                Component targetComponent = gizmo?.TargetComponent as Component;
                if (targetComponent != null)
                {
                    if (m_activeAssetDatabase.IsInstance(targetComponent) && !m_activeAssetDatabase.IsDirtyObject(targetComponent))
                    {
                        m_activeAssetDatabase.SetDirtyAsync(targetComponent);
                    }
                }
            }

            m_activeTool = Tools.ActiveTool;
        }

        #endregion

        #region IAssetDatabaseModel

        public bool CanSaveScene
        {
            get { return m_activeAssetDatabase.CanSaveScene; }
        }

        public bool CanInitializeNewScene
        {
            get { return m_activeAssetDatabase.CanInitializeNewScene; }
        }

        public GameObject CurrentScene
        {
            get { return m_activeAssetDatabase.CurrentScene; }
        }

        public ID CurrentSceneID
        {
            get { return m_activeAssetDatabase.CurrentSceneID; }
        }

        public bool CanCreatePrefab(object obj)
        {
            return m_activeAssetDatabase.CanCreatePrefab(obj);
        }

        public bool CanCreatePrefabVariant(object obj)
        {
            return m_activeAssetDatabase.CanCreatePrefabVariant(obj);
        }

        public bool CanCreatePrefabVariant(ID id)
        {
            return m_activeAssetDatabase.CanCreatePrefabVariant(id);
        }

        public bool CanSelectPrefab(object obj)
        {
            return m_activeAssetDatabase.CanSelectPrefab(obj);
        }

        public bool CanOpenPrefab(object obj)
        {
            return m_activeAssetDatabase.CanOpenPrefab(obj) && obj is GameObject go && go.GetComponent<ExposeToEditor>();
        }

        public bool CanClosePrefab
        {
            get { return m_activeAssetDatabase.CanClosePrefab; }
        }

        public void AddOpenableAssetExt(string ext)
        {
            m_assetDatabase?.AddOpenableAssetExt(ext);
            m_assetDatabaseOverRTSL?.AddOpenableAssetExt(ext);
        }

        public void RemoveOpenableAssetExt(string ext)
        {
            m_assetDatabase?.RemoveOpenableAssetExt(ext);
            m_assetDatabaseOverRTSL?.RemoveOpenableAssetExt(ext);
        }

        public bool CanOpenAsset(ID assetID)
        {
            return m_activeAssetDatabase.CanOpenAsset(assetID);
        }

        public bool CanEditAsset(ID assertID)
        {
            return m_activeAssetDatabase.CanEditAsset(assertID);
        }

        public bool CanInstantiateAsset(ID assetID)
        {
            return m_activeAssetDatabase.CanInstantiateAsset(assetID);
        }

        public bool CanDuplicateAsset(ID assetID)
        {
            return m_activeAssetDatabase.CanDuplicateAsset(assetID);
        }

        public bool CanDetach(object[] instances)
        {
            return m_activeAssetDatabase.CanDetach(instances);
        }

        public bool CanDuplicate(object[] instances)
        {
            return m_activeAssetDatabase.CanDuplicate(instances);
        }

        public bool CanRelease(object[] instances)
        {
            return m_activeAssetDatabase.CanRelease(instances);
        }

        public bool CanApplyChanges(object instance)
        {
            return m_activeAssetDatabase.CanApplyChanges(instance);
        }

        public bool CanApplyToBase(object instance)
        {
            return m_activeAssetDatabase.CanApplyToBase(instance);
        }

        public bool CanRevertToBase(object instance)
        {
            return m_activeAssetDatabase.CanRevertToBase(instance);
        }

        public GameObject CurrentHierarchyParent
        {
            get { return m_activeAssetDatabase?.CurrentHierarchyParent; }
            set { m_activeAssetDatabase.CurrentHierarchyParent = value; }
        }

        public GameObject CurrentPrefab
        {
            get { return m_activeAssetDatabase?.CurrentPrefab; }
        }

        public string ProjectID
        {
            get 
            {
                if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
                {
                    return m_project?.State.ProjectInfo?.Name;
                }

                return m_activeAssetDatabase?.ProjectID; 
            }
        }

        public bool IsProjectLoaded
        {
            get 
            {
                if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
                {
                    if (m_project == null)
                    {
                        return false;
                    }

                    return m_project.State.IsOpened;
                }

                return m_activeAssetDatabase != null && m_activeAssetDatabase.IsProjectLoaded; 
            }
        }

        public ID RootFolderID
        {
            get { return m_activeAssetDatabase != null ? m_activeAssetDatabase.RootFolderID : ID.Empty; }
        }

        public string LibraryRootFolder
        {
            get { return m_activeAssetDatabase?.LibraryRootFolder; }
        }

        public string GetFolderInLibrary(ID assetID)
        {
            return m_activeAssetDatabase.GetFolderInLibrary(assetID);
        }

        public ID CurrentFolderID
        {
            get { return m_activeAssetDatabase.CurrentFolderID; }
            set { m_activeAssetDatabase.CurrentFolderID = value; }
        }

        public ID[] SelectedAssets
        {
            get { return m_activeAssetDatabase.SelectedAssets; }
            set { m_activeAssetDatabase.SelectedAssets = value; }
        }

        public void AddRuntimeSerializableTypes(params Type[] type)
        {
            m_assetDatabase?.AddRuntimeSerializableTypes(type);
            //m_assetDatabaseOverRTSL?.AddRuntimeSerializableTypes(type);
        }

        public void AddRuntimeSerializableTypes(Type[] type, Guid[] typeID)
        {
            m_assetDatabase?.AddRuntimeSerializableTypes(type, typeID);
            m_assetDatabaseOverRTSL?.AddRuntimeSerializableTypes(type, typeID);
        }

        public void RemoveRuntimeSerializableTypes(params Type[] types)
        {
            m_assetDatabase?.RemoveRuntimeSerializableTypes(types);
            m_assetDatabaseOverRTSL?.RemoveRuntimeSerializableTypes(types);
        }

        [Obsolete("Use AddRuntimeSerializableTypes")]
        public void AddRuntimeSerializableType(Type type, Guid typeID)
        {
            m_assetDatabase?.AddRuntimeSerializableType(type, typeID);
            m_assetDatabaseOverRTSL?.AddRuntimeSerializableType(type, typeID);
        }

        [Obsolete("Use AddRuntimeSerializableTypes")]
        public void RemoveRuntimeSerializableType(Type type)
        {
            m_assetDatabase?.RemoveRuntimeSerializableType(type);
            m_assetDatabaseOverRTSL?.RemoveRuntimeSerializableType(type);
        }

        public void SetRuntimeTypeResolver(Func<string, Type> resolveType)
        {
            m_assetDatabase?.SetRuntimeTypeResolver(resolveType);
            m_assetDatabaseOverRTSL?.SetRuntimeTypeResolver(resolveType);
        }

        public void AddExtension(IAssetDatabaseProjectExtension extension)
        {
            m_assetDatabase?.AddExtension(extension);
            m_assetDatabaseOverRTSL?.AddExtension(extension);
        }

        public void RemoveExtension(IAssetDatabaseProjectExtension extension)
        {
            m_assetDatabase?.RemoveExtension(extension);
            m_assetDatabaseOverRTSL?.RemoveExtension(extension);
        }

        public void AddExternalAssetLoader(string loaderID, IExternalAssetLoaderModel loader)
        {
            m_assetDatabase?.AddExternalAssetLoader(loaderID, loader);
            m_assetDatabaseOverRTSL?.AddExternalAssetLoader(loaderID, loader);
        }

        public void RemoveExternalAssetLoader(string loaderID)
        {
            m_assetDatabase?.RemoveExternalAssetLoader(loaderID);
            m_assetDatabaseOverRTSL?.RemoveExternalAssetLoader(loaderID);
        }

        public IReadOnlyList<IImportSourceModel> ImportSources
        {
            get { return m_activeAssetDatabase.ImportSources; }
        }

        public void AddImportSource(IImportSourceModel importSource)
        {
            m_assetDatabase?.AddImportSource(importSource);
            m_assetDatabaseOverRTSL?.AddImportSource(importSource);
        }

        public void RemoveImportSource(IImportSourceModel importSource)
        {
            m_assetDatabase?.RemoveImportSource(importSource);
            m_assetDatabaseOverRTSL?.RemoveImportSource(importSource);
        }

        private string NormalizeProjectType(string projectType)
        {
            if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                if (projectType == null)
                {
                    projectType = RTSLProject;
                }

                if (projectType != RTSLProject)
                {
                    return null;
                }
            }
            else if (m_compatibilityMode == CompatibilityMode.None)
            {
                if (projectType == null)
                {
                    projectType = AssetDatabaseProject;
                }

                if (projectType != AssetDatabaseProject)
                {
                    return null;
                }
            }
            else
            {
                if (projectType == null)
                {
                    projectType = AssetDatabaseProject;
                }

                if (projectType != AssetDatabaseProject && projectType != RTSLProject)
                {
                    return null;
                }
            }
            return projectType;
        }

        public string ProjectsRootFolderPath
        {
            get 
            {
                if (string.IsNullOrEmpty(m_extraSettings.ProjectsRootFolderPath))
                {
                    return Application.persistentDataPath;
                }

                if (!Path.IsPathRooted(m_extraSettings.ProjectsRootFolderPath))
                {
                    Debug.LogError($"{m_extraSettings.ProjectsRootFolderPath}  path is not rooted");
                    return Application.persistentDataPath;
                }

                if (m_extraSettings.ProjectsRootFolderPath == "/" || m_extraSettings.ProjectsRootFolderPath == "\\")
                {
                    Debug.LogError($"Invalid ProjectsRootFolderPath");
                    return Application.persistentDataPath;
                }

                return m_extraSettings.ProjectsRootFolderPath;
            }
            set 
            { 
                if (!Path.IsPathRooted(value))
                {
                    throw new ArgumentException($"{value} path is not rooted", "value");
                }

                if (value == "/" || value == "\\")
                {
                    throw new ArgumentException($"{value} path is invalid", "value");
                }

                m_extraSettings.ProjectsRootFolderPath = value; 
            }
        }

        public bool IsProjectSupported(string projectType)
        {
            return NormalizeProjectType(projectType) != null;
        }

        public Task<ProjectListEntry[]> GetProjectsAsync()
        {
            var projectListModel = IOC.Resolve<IProjectListModel>();
            return projectListModel.GetProjectsAsync();
        }

        public Task<ProjectListEntry> CreateProjectAsync(string projectPath)
        {
            var projectListModel = IOC.Resolve<IProjectListModel>();
            return projectListModel.CreateProjectAsync(projectPath);
        }

        public async Task<ProjectListEntry> DeleteProjectAsync(string projectPath)
        {
            var projectListModel = IOC.Resolve<IProjectListModel>();
            var entry = await projectListModel.DeleteProjectAsync(projectPath);

            if (IsProjectLoaded && ProjectID == entry?.ProjectPath)
            {
                await m_activeAssetDatabase.UnloadProjectAsync();
            }

            return entry;
        }

        public async Task ImportProjectAsync(Stream istream, string projectPath, string password)
        {
            await DeleteProjectAsync(projectPath);

            var projectListModel = IOC.Resolve<IProjectListModel>();
            await projectListModel.ImportProjectAsync(istream, projectPath, password);
        }

        public Task ExportProjectAsync(Stream ostream, string projectPath, string password)
        {
            var projectListModel = IOC.Resolve<IProjectListModel>();
            return projectListModel.ExportProjectAsync(ostream, projectPath, password);
        }

        public async Task LoadProjectAsync(string projectID = null, string projectType = null)
        {
            string rootPath = ProjectsRootFolderPath;
            if (string.IsNullOrEmpty(projectID))
            {
                projectID = string.IsNullOrEmpty(m_extraSettings.DefaultProjectName) ?
                    PlayerPrefs.GetString(k_DefaultProjectKey, $"{rootPath}/Project") :
                    m_extraSettings.DefaultProjectName;

                if (!projectID.StartsWith(rootPath))
                {
                    projectID = null;
                }

                projectType = PlayerPrefs.GetString(k_DefaultProjectTypeKey, null);
                projectType = NormalizeProjectType(projectType);
                if (string.IsNullOrEmpty(projectID) || projectType == null)
                {
                    if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
                    {
                        projectID = $"RTSLProject";
                    }
                    else
                    {
                        projectID = "Project";
                    }
                }
            }

            if (!Path.IsPathRooted(projectID))
            {
                projectID = $"{rootPath}/{projectID}";
            }

            projectType = NormalizeProjectType(projectType);
            if (projectType == null)
            {
                throw new NotSupportedException("Project is not supported");
            }

            switch (projectType)
            {
                case AssetDatabaseProject:
                    ActiveAssetDatabaseHostName = m_assetDatabasePrefab.name;
                    break;
                case RTSLProject:
                    ActiveAssetDatabaseHostName = m_assetDatabaseOverRTSLPrefab.name;
                    break;
            }

            CurrentProjectType = projectType;

            if (!m_activeAssetDatabase.IsProjectLoaded)
            {
                await m_activeAssetDatabase.LoadProjectAsync(projectID);
                PlayerPrefs.SetString(k_DefaultProjectKey, projectID);
                PlayerPrefs.SetString(k_DefaultProjectTypeKey, projectType);
            }
        }

        public Task UnloadProjectAsync()
        {
            return m_activeAssetDatabase.UnloadProjectAsync();
        }

        public Task ReloadProjectAsync()
        {
            return m_activeAssetDatabase.ReloadProjectAsync();
        }

        public bool IsPrefabOperationAllowed(object instance)
        {
            return m_activeAssetDatabase.IsPrefabOperationAllowed(instance);
        }

        public bool IsAssetRoot(object obj)
        {
            return m_activeAssetDatabase.IsAssetRoot(obj);
        }

        public bool IsAsset(object obj)
        {
            return m_activeAssetDatabase.IsAsset(obj);
        }

        public bool IsInstanceOfAssetVariant(object obj)
        {
            return m_activeAssetDatabase.IsInstanceOfAssetVariant(obj);
        }

        public bool IsInstanceOfAssetVariantRef(object obj)
        {
            return m_activeAssetDatabase.IsInstanceOfAssetVariantRef(obj);
        }

        public bool IsInstanceRoot(object obj)
        {
            return m_activeAssetDatabase.IsInstanceRoot(obj);
        }

        public bool IsInstanceRootRef(object obj)
        {
            return m_activeAssetDatabase.IsInstanceRootRef(obj);
        }

        public bool IsInstance(object obj)
        {
            return m_activeAssetDatabase.IsInstance(obj);
        }

        public bool IsDirtyObject(object obj)
        {
            return m_activeAssetDatabase.IsDirtyObject(obj);
        }

        public bool IsAddedObject(object obj)
        {
            return m_activeAssetDatabase.IsAddedObject(obj);
        }

        public bool HasChanges(object instance, object instanceRootOpenedForEditing)
        {
            return m_activeAssetDatabase.HasChanges(instance, instanceRootOpenedForEditing);
        }

        public bool IsScene(ID id)
        {
            return m_activeAssetDatabase.IsScene(id);
        }

        public bool IsPrefab(ID id)
        {
            return m_activeAssetDatabase.IsPrefab(id);
        }

        public bool IsPrefabVariant(ID id)
        {
            return m_activeAssetDatabase.IsPrefabVariant(id);
        }

        public bool IsExternalAsset(ID id)
        {
            return m_activeAssetDatabase.IsExternalAsset(id);
        }

        public bool IsExternalAsset(object obj)
        {
            return m_activeAssetDatabase.IsExternalAsset(obj);
        }

        public bool IsFolder(ID id)
        {
            return m_activeAssetDatabase.IsFolder(id);
        }

        public bool Exists(ID id)
        {
            return m_activeAssetDatabase.Exists(id);
        }

        public Type GetType(ID id)
        {
            return m_activeAssetDatabase.GetType(id);
        }

        public ID GetTypeID(Type type)
        {
            return m_activeAssetDatabase.GetTypeID(type);
        }

        public object CreateObjectOfType(Type type)
        {
            return m_activeAssetDatabase.CreateObjectOfType(type);
        }

        public string GetName(ID id)
        {
            return m_activeAssetDatabase.GetName(id);
        }

        public string GetDisplayName(ID id)
        {
            return m_activeAssetDatabase.GetDisplayName(id);
        }

        public string GetName(string path)
        {
            return m_activeAssetDatabase.GetName(path);
        }

        public string GetDisplayName(string path)
        {
            return m_activeAssetDatabase.GetDisplayName(path);
        }

        public bool IsValidName(string name)
        {
            return m_activeAssetDatabase.IsValidName(name);
        }

        public ID GetAssetID(string path)
        {
            return m_activeAssetDatabase.GetAssetID(path);
        }

        public ID GetAssetID(object asset)
        {
            return m_activeAssetDatabase.GetAssetID(asset);
        }

        public ID GetSubAssetID(object subAsset)
        {
            return m_activeAssetDatabase.GetSubAssetID(subAsset);
        }

        public ID GetAssetIDByInstance(object instance)
        {
            return m_activeAssetDatabase.GetAssetIDByInstance(instance);
        }

        public IEnumerable<GameObject> GetInstancesByAssetID(ID assetID)
        {
            return m_activeAssetDatabase.GetInstancesByAssetID(assetID);
        }

        public bool IsLoaded(ID id)
        {
            return m_activeAssetDatabase.IsLoaded(id);
        }

        public object GetAsset(ID id)
        {
            return m_activeAssetDatabase.GetAsset(id);
        }

        public object GetAssetByInstance(object instance)
        {
            return m_activeAssetDatabase.GetAssetByInstance(instance);
        }

        public bool IsRawData(ID id)
        {
            return m_activeAssetDatabase.IsRawData(id);
        }

        public T GetRawData<T>(ID id)
        {
            return m_activeAssetDatabase.GetRawData<T>(id);
        }

        public void SetRawData<T>(ID id, T data)
        {
            m_activeAssetDatabase.SetRawData(id, data);
        }

        public Task<object> LoadAssetAsync(ID id)
        {
            return m_activeAssetDatabase.LoadAssetAsync(id);
        }

        public string GetPath(ID id)
        {
            return m_activeAssetDatabase.GetPath(id);
        }

        public string GetExt(object obj)
        {
            return m_activeAssetDatabase.GetExt(obj);
        }

        public string GetUniquePath(string path)
        {
            return m_activeAssetDatabase.GetUniquePath(path);
        }

        public string GetUniquePath(ID folderId, string desiredName)
        {
            return m_activeAssetDatabase.GetUniquePath(folderId, desiredName);
        }

        public byte[] GetThumbnailData(ID id)
        {
            return m_activeAssetDatabase.GetThumbnailData(id);
        }

        public Task<byte[]> LoadThumbnailDataAsync(ID id)
        {
            return m_activeAssetDatabase.LoadThumbnailDataAsync(id);
        }

        public ID GetParent(ID id)
        {
            return m_activeAssetDatabase.GetParent(id);
        }

        public bool HasChildren(ID id)
        {
            return m_activeAssetDatabase.HasChildren(id);
        }

        public IEnumerable<ID> GetChildren(ID id, bool sortByName = true, bool recursive = false, string searchPattern = null)
        {
            return m_activeAssetDatabase.GetChildren(id, sortByName, recursive, searchPattern);
        }

        public Task<IEnumerable<object>> ExtractSubAssetsAsync(object asset, ExtractSubAssetOptions options)
        {
            return m_activeAssetDatabase.ExtractSubAssetsAsync(asset, options);
        }

        public Task DontDestroySubAssetsAsync(object obj)
        {
            return m_activeAssetDatabase.DontDestroySubAssetsAsync(obj);
        }

        public Task<ID> CreateFolderAsync(string path)
        {
            return m_activeAssetDatabase.CreateFolderAsync(path);
        }

        public Task<ID> CreateAssetAsync(object obj, string path, bool variant, bool extractSubassets)
        {
            return m_activeAssetDatabase.CreateAssetAsync(obj, path, variant, extractSubassets);
        }

        public Task<ID> ImportExternalAssetAsync(ID folderID, object key, string loaderID, string desiredName)
        {
            return m_activeAssetDatabase.ImportExternalAssetAsync(folderID, key, loaderID, desiredName);
        }

        public Task<ID> ImportExternalAssetAsync(ID folderID, ID assetID, object key, string loaderID, string desiredName)
        {
            return m_activeAssetDatabase.ImportExternalAssetAsync(folderID, assetID, key, loaderID, desiredName);
        }

        public Task ExportAssetsAsync(ID[] assets, Stream ostream, bool includeDependencies)
        {
            return m_activeAssetDatabase.ExportAssetsAsync(assets, ostream, includeDependencies);
        }

        public Task ImportAssetsAsync(Stream istream)
        {
            return m_activeAssetDatabase.ImportAssetsAsync(istream);
        }

        public async Task InitializeNewSceneAsync()
        {
            if (m_compatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                m_sceneManager.RaiseNewSceneCreating();

                Task task = m_activeAssetDatabase.UnloadAllAndClearSceneAsync();

                while (!task.IsCompleted && !task.IsFaulted) { }
                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                InitializeNewScene?.Invoke(this, EventArgs.Empty);

                m_sceneManager.RaiseNewSceneCreated();
            }
            else
            {
                m_sceneManager.RaiseNewSceneCreating();

                await m_activeAssetDatabase.InitializeNewSceneAsync();
            }
        }

        public Task UnloadAllAndClearSceneAsync()
        {
            return m_activeAssetDatabase.UnloadAllAndClearSceneAsync();
        }

        public Task SaveAssetAsync(ID assetID)
        {
            return m_activeAssetDatabase.SaveAssetAsync(assetID);
        }

        public Task UpdateThumbnailAsync(ID assetID)
        {
            return m_activeAssetDatabase.UpdateThumbnailAsync(assetID);
        }

        public Task MoveAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            return m_activeAssetDatabase.MoveAssetsAsync(assetIDs, toPaths);
        }

        public Task DuplicateAssetsAsync(IReadOnlyList<ID> ids, IReadOnlyList<string> toPaths)
        {
            return m_activeAssetDatabase.DuplicateAssetsAsync(ids, toPaths);
        }

        public Task DeleteAssetsAsync(IReadOnlyList<ID> ids)
        {
            if (ids.Contains(RootFolderID))
            {
                throw new InvalidOperationException($"Can't delete root folder {RootFolderID}");
            }

            return m_activeAssetDatabase.DeleteAssetsAsync(ids);
        }

        public Task SelectPrefabAsync(GameObject instance)
        {
            return m_activeAssetDatabase.SelectPrefabAsync(instance);
        }

        public Task OpenPrefabAsync(GameObject instance)
        {
            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_Information", "Information"),
                    m_localization.GetString("ID_RTEditor_UnableToOpenPrefabInPlayMode", "Unable to open prefab in play mode"));
                return Task.CompletedTask;
            }

            return m_activeAssetDatabase.OpenPrefabAsync(instance);
        }

        public Task ClosePrefabAsync()
        {
            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_Information", "Information"),
                    m_localization.GetString("ID_RTEditor_UnableToClosePrefabInPlayMode", "Unable to close prefab in play mode"));
                return Task.CompletedTask;
            }

            return m_activeAssetDatabase.ClosePrefabAsync();
        }

        public Task OpenAssetAsync(ID assetID)
        {
            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_Information", "Information"),
                    m_localization.GetString("ID_RTEditor_UnableToOpenAssetInPlayMode", "Unable to open asset in playmode"));
                return Task.CompletedTask;
            }

            return m_activeAssetDatabase.OpenAssetAsync(assetID);
        }

        public bool IsCyclicNesting(GameObject instance, Transform parent)
        {
            return m_activeAssetDatabase.IsCyclicNesting(instance, parent);
        }

        public Task<InstantiateAssetsResult> InstantiateAssetsAsync(ID[] assetIDs, Transform parent)
        {
            return m_activeAssetDatabase.InstantiateAssetsAsync(assetIDs, parent);
        }

        public Task DetachAsync(GameObject[] instances, bool completely, bool cloneSubAssets)
        {
            return m_activeAssetDatabase.DetachAsync(instances, completely, cloneSubAssets);
        }

        public Task SetDirtyAsync(object instance)
        {
            return m_activeAssetDatabase.SetDirtyAsync(instance);
        }

        public Task ReleaseAsync(GameObject[] instances)
        {
            return m_activeAssetDatabase.ReleaseAsync(instances);
        }

        public bool IsCyclicNestingAfterApplyingChanges(GameObject instance, bool toBase)
        {
            return m_activeAssetDatabase.IsCyclicNestingAfterApplyingChanges(instance, toBase);
        }

        public Task ApplyChangesAsync(GameObject instance)
        {
            return m_activeAssetDatabase.ApplyChangesAsync(instance);
        }

        public Task ApplyToBaseAsync(GameObject instance)
        {
            return m_activeAssetDatabase.ApplyToBaseAsync(instance);
        }

        public Task RevertToBaseAsync(GameObject instance)
        {
            return m_activeAssetDatabase.RevertToBaseAsync(instance);
        }

        // Serializer

        public Task<byte[]> SerializeAsync(object asset)
        {
            return m_activeAssetDatabase.SerializeAsync(asset);
        }

        public Task<object> DeserializeAsync(byte[] data, object target)
        {
            return m_activeAssetDatabase.DeserializeAsync(data, target);
        }

        // KeyValue Storage

        public Task<T> GetValueAsync<T>(string key)
        {
            return m_activeAssetDatabase.GetValueAsync<T>(key);
        }

        public Task SetValueAsync<T>(string key, T obj)
        {
            return m_activeAssetDatabase.SetValueAsync(key, obj);
        }

        public Task DeleteValueAsync<T>(string key)
        {
            return m_activeAssetDatabase.DeleteValueAsync<T>(key);
        }

        #endregion

        #region Legacy     

        [Obsolete("Use BeforeSaveCurrentScene")]
        public event RTEEvent<UIControls.CancelArgs> BeforeSceneSave;

        [Obsolete]
        public event RTEEvent SceneSaving;

        [Obsolete("Use SaveCurrentScene")]
        public event RTEEvent SceneSaved;

        [Obsolete("Use BeforeOpenScene")]
        public event RTEEvent SceneLoading;

        [Obsolete("Use OpenScene")]
        public event RTEEvent SceneLoaded;

        private void SubscribeRTSLEvents()
        {
            if (CompatibilityMode == CompatibilityMode.None)
            {
                return;
            }

            m_project = IOC.Resolve<RTSL.Interface.IProjectAsync>();
            if (m_project != null)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                m_project.Events.BeginSave += OnBeginSave;
                m_project.Events.BeginLoad += OnBeginLoad;
                m_project.Events.SaveCompleted += OnSaveCompleted;
                m_project.Events.LoadCompleted += OnLoadCompleted;
                m_project.Events.OpenProjectCompleted += OnOpenProjectCompleted;
                m_project.Events.DeleteProjectCompleted += OnDeleteProjectCompleted;
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        private void UnsubscribeRTSLEvents()
        {
            if (m_project != null)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                m_project.Events.BeginSave -= OnBeginSave;
                m_project.Events.BeginLoad -= OnBeginLoad;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.LoadCompleted -= OnLoadCompleted;
                m_project.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
                m_project.Events.DeleteProjectCompleted -= OnDeleteProjectCompleted;
#pragma warning restore CS0612 // Type or member is obsolete
                m_project = null;
            }
        }

        private void RTSLIgnoreEventSystem()
        {
            if (EventSystem != null)
            {
                if (!EventSystem.GetComponent<RTSL.RTSLIgnore>() && EventSystem.transform.parent == null)
                {
                    EventSystem.gameObject.AddComponent<RTSL.RTSLIgnore>();
                }
            }
        }

        [Obsolete("Use OverwriteSceneAsync instead")] //12.11.2020
        public async void OverwriteScene(RTSL.Interface.AssetItem scene, Action<RTSL.Interface.Error> callback)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            try
            {
                await OverwriteSceneAsync(scene);
                callback?.Invoke(RTSL.Interface.Error.NoError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                callback?.Invoke(new RTSL.Interface.Error(RTSL.Interface.Error.E_Failed));
            }
        }

        [Obsolete]
        public async Task OverwriteSceneAsync(RTSL.Interface.ProjectItem scene)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (BeforeSceneSave != null)
            {
                var args = new UIControls.CancelArgs();
                BeforeSceneSave(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            Undo.Purge();

            IsBusy = true;

            await m_project.SaveAsync(new[] { scene }, new[] { (object)SceneManager.GetActiveScene() });

            IsBusy = false;
        }

        [Obsolete]
        public async void SaveSceneToFolder(RTSL.Interface.ProjectItem folder, string name, Action<RTSL.Interface.Error> callback)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            try
            {
                await SaveSceneToFolderAsync(folder, name);
                callback?.Invoke(RTSL.Interface.Error.NoError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                callback?.Invoke(new RTSL.Interface.Error(RTSL.Interface.Error.E_Failed));
            }
        }

        [Obsolete]
        public async Task SaveSceneToFolderAsync(RTSL.Interface.ProjectItem folder, string name)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (BeforeSceneSave != null)
            {
                var args = new UIControls.CancelArgs();
                BeforeSceneSave(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            Undo.Purge();

            IsBusy = true;

            await m_project.SaveAsync(new[] { folder }, new[] { new byte[0] }, new[] { (object)SceneManager.GetActiveScene() }, new[] { name });

            IsBusy = false;
        }

        [Obsolete]
        public Task<RTSL.Interface.ProjectItem[]> CreatePrefabAsync(RTSL.Interface.ProjectItem folder, ExposeToEditor obj, bool? extractSubAssets)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (folder == null)
            {
                folder = m_project.State.RootFolder;
            }

            var completionSource = new TaskCompletionSource<RTSL.Interface.ProjectItem[]>();
            if (!extractSubAssets.HasValue)
            {
                m_wm.Confirmation(
                    m_localization.GetString("ID_RTEditor_CreatePrefab", "Create Prefab"),
                    m_localization.GetString("ID_RTEditor_IncludeDependencies", "Include dependencies?"),
                    async (sender, args) =>
                    {
                        try
                        {
                            var result = await CreatePrefabWithDependenciesAsync(folder, obj);
                            completionSource.SetResult(result);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }
                    },
                    async (sender, args) =>
                    {
                        try
                        {
                            var result = await CreatePrefabWithoutDependenciesAsync(folder, obj);
                            completionSource.SetResult(result);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }

                    },
                    m_localization.GetString("ID_RTEditor_Yes", "Yes"),
                    m_localization.GetString("ID_RTEditor_No", "No"));
            }
            else
            {
                if (extractSubAssets.Value)
                {
                    return CreatePrefabWithDependenciesAsync(folder, obj);
                }
                else
                {
                    return CreatePrefabWithoutDependenciesAsync(folder, obj);
                }
            }

            return completionSource.Task;
        }

        [Obsolete]
        private async Task<RTSL.Interface.ProjectItem[]> CreatePrefabWithoutDependenciesAsync(RTSL.Interface.ProjectItem folder, ExposeToEditor obj)
        {
            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
            byte[] previewData = previewUtility.CreatePreviewData(obj.gameObject);
            return await CreatePrefabAsync(folder, new[] { previewData }, new[] { obj.gameObject });
        }

        [Obsolete]
        private async Task<RTSL.Interface.ProjectItem[]> CreatePrefabWithDependenciesAsync(RTSL.Interface.ProjectItem folder, ExposeToEditor obj)
        {
            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();

            object[] deps = await m_project.GetDependenciesAsync(obj.gameObject, true);
            object[] objects;
            if (!deps.Contains(obj.gameObject))
            {
                objects = new object[deps.Length + 1];
                objects[deps.Length] = obj.gameObject;
                for (int i = 0; i < deps.Length; ++i)
                {
                    objects[i] = deps[i];
                }
            }
            else
            {
                objects = deps;
            }

            var uoFactory = IOC.Resolve<RTSL.Interface.IUnityObjectFactory>();
            objects = objects.Where(o => uoFactory.CanCreateInstance(o.GetType())).ToArray();

            byte[][] previewData = new byte[objects.Length][];
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] is UnityObject)
                {
                    previewData[i] = previewUtility.CreatePreviewData((UnityObject)objects[i]);
                }
            }

            var result = await CreatePrefabAsync(folder, previewData, objects);
            return result;
        }

        [Obsolete]
        private async Task<RTSL.Interface.ProjectItem[]> CreatePrefabAsync(RTSL.Interface.ProjectItem folder, byte[][] previewData, object[] objects)
        {
            if (objects.Any(o => !(o is GameObject)))
            {
                if (folder.Children == null || folder.Get("Data") == null)
                {
                    await m_project.CreateFoldersAsync(new[] { folder }, new[] { "Data" });
                }

#pragma warning disable CS0612
                IProjectTree projectTree = IOC.Resolve<IProjectTree>();
#pragma warning restore CS0612
                if (projectTree != null)
                {
                    projectTree.SelectedItem = folder;
                }
                else
                {
                    IProjectTreeModel projectTreeViewModel = IOC.Resolve<IProjectTreeModel>();
                    if (projectTreeViewModel != null)
                    {
                        projectTreeViewModel.SelectedItem = folder;
                    }
                }
            }

            var dataFolder = folder.Get("Data");
            var parents = new List<RTSL.Interface.ProjectItem>();
            for (int i = 0; i < objects.Length; ++i)
            {
                object obj = objects[i];
                if (obj is GameObject)
                {
                    parents.Add(folder);
                }
                else
                {
                    parents.Add(dataFolder);
                }
            }

            var result = await m_project.SaveAsync(parents.ToArray(), previewData, objects, null);
            return result;
        }

        [Obsolete]
        public async Task<RTSL.Interface.ProjectItem[]> SaveAssetsAsync(UnityObject[] assets)
        {
            var assetsToSave = new List<UnityObject>();
            var projectItems = new List<RTSL.Interface.ProjectItem>();

            for (int i = 0; i < assets.Length; ++i)
            {
                UnityObject asset = assets[i];
                var projectItem = m_project.Utils.ToProjectItem(asset);
                if (projectItem == null)
                {
                    continue;
                }

                assetsToSave.Add(asset);
                projectItems.Add(projectItem);
            }

            if (assetsToSave.Count == 0)
            {
                return new RTSL.Interface.ProjectItem[0];
            }

            var items = projectItems.ToArray();
            await m_project.SaveAsync(items, assets.ToArray(), false);

            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            for (int i = 0; i < items.Length; ++i)
            {
                byte[] preview = previewUtil.CreatePreviewData(assets[i]);
                items[i].SetPreview(preview);
            }

            await m_project.SavePreviewsAsync(items);
            await UpdatePreviewsDependentOn(items);
            return items;
        }

        [Obsolete]
        public async Task<RTSL.Interface.ProjectItem[]> DeleteAssetsAsync(RTSL.Interface.ProjectItem[] projectItems)
        {
            var assetItems = projectItems.Where(pi => !pi.IsFolder).ToArray();
            for (int i = 0; i < assetItems.Length; ++i)
            {
                var assetItem = assetItems[i];
                UnityObject obj = m_project.Utils.FromProjectItem<UnityObject>(assetItem);

                if (obj != null)
                {
                    if (obj is GameObject)
                    {
                        GameObject go = (GameObject)obj;
                        Component[] components = go.GetComponentsInChildren<Component>(true);
                        for (int j = 0; j < components.Length; ++j)
                        {
                            Component component = components[j];
                            Undo.Erase(component, null);
                            if (component is Transform)
                            {
                                Undo.Erase(component.gameObject, null);
                            }
                        }
                    }
                    else
                    {
                        Undo.Erase(obj, null);
                    }
                }
            }

            var folders = projectItems.Where(pi => pi.IsFolder).ToArray();
            var result = assetItems.Union(folders).ToArray();
            await m_project.DeleteAsync(result);
            await Task.Yield();
            await UpdatePreviewsDependentOn(assetItems);
            return projectItems;
        }

        [Obsolete]
        private async Task UpdatePreviewsDependentOn(RTSL.Interface.ProjectItem[] assetItems)
        {
            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            var dependentItems = m_project.Utils.GetProjectItemsDependentOn(assetItems).Where(item => !m_project.Utils.IsScene(item)).ToArray();
            if (dependentItems.Length == 0)
            {
                return;
            }

            UnityObject[] loadedObjects = await m_project.LoadAsync(dependentItems);

            for (int i = 0; i < loadedObjects.Length; ++i)
            {
                UnityObject loadedObject = loadedObjects[i];
                var dependentItem = dependentItems[i];
                if (loadedObject != null)
                {
                    byte[] previewData = previewUtil.CreatePreviewData(loadedObject);
                    dependentItem.SetPreview(previewData);
                }
                else
                {
                    dependentItem.SetPreview(null);
                }
            }

            await m_project.SavePreviewsAsync(dependentItems);
        }

        [Obsolete]
        public async Task<RTSL.Interface.ProjectItem> UpdatePreviewAsync(UnityObject obj)
        {
            using (await m_project.LockAsync())
            {
                var projectItem = m_project.Utils.ToProjectItem(obj);
                if (projectItem != null)
                {
                    IResourcePreviewUtility resourcePreviewUtility = IOC.Resolve<IResourcePreviewUtility>();
                    byte[] preview = resourcePreviewUtility.CreatePreviewData(obj);
                    projectItem.SetPreview(preview);
                }
                return projectItem;
            }
        }

        private void OnNewSceneCreating(object sender, EventArgs e)
        {
            IsPlaying = false;

            SceneLoading?.Invoke();
        }

        private async void OnNewSceneCreated(object sender, EventArgs e)
        {
            TryCreateLightAndCamera();

            Selection.objects = null;
            Undo.Purge();

            await Task.Yield();
            await Task.Yield();

            SceneLoaded?.Invoke();
        }

        private void TryCreateLightAndCamera()
        {
            if (m_extraSettings.CreateLight)
            {
                if (m_activeAssetDatabase.GetTypeID(typeof(Light)) != ID.Empty)
                {
                    GameObject lightGO = new GameObject(m_localization.GetString("ID_RTEditor_DirectionalLight", "Directional Light"));
                    lightGO.transform.SetParent(HierarchyRoot?.transform);
                    lightGO.transform.position = Vector3.up * 3;
                    lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

                    Light light = lightGO.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.shadows = LightShadows.Soft;
                    lightGO.AddComponent<ExposeToEditor>();

                    if (RTSL.RTSLSettings.SaveIncludedObjectsOnly)
                    {
                        lightGO.AddComponent<RTSL.RTSLInclude>();
                    }

                    if (RenderPipelineInfo.Type == RPType.HDRP)
                    {
                        light.intensity = 10000;
                    }
                }
            }

            if (m_extraSettings.CreateCamera)
            {
                if (m_activeAssetDatabase.GetTypeID(typeof(Light)) != ID.Empty)
                {
                    GameObject cameraGO = new GameObject(m_localization.GetString("ID_RTEditor_Camera", "Camera"));
                    cameraGO.transform.SetParent(HierarchyRoot?.transform);
                    cameraGO.transform.position = new Vector3(0, 1, -10);
                    cameraGO.gameObject.SetActive(false);
                    try
                    {
                        cameraGO.tag = "MainCamera";
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("Can't set MainCamera tag");
                    }
                    cameraGO.AddComponent<Camera>();
                    cameraGO.AddComponent<ExposeToEditor>();
                    cameraGO.gameObject.SetActive(true);

                    if (RTSL.RTSLSettings.SaveIncludedObjectsOnly)
                    {
                        cameraGO.AddComponent<RTSL.RTSLInclude>();
                    }

                    cameraGO.AddComponent<GameViewCamera>();
                }
            }
        }

        [Obsolete]
        private async void OnOpenProjectCompleted(object sender, RTSL.Interface.ProjectEventArgs<RTSL.Interface.ProjectInfo> e)
        {
            PlayerPrefs.SetString(k_DefaultProjectKey, e.Payload.Name);

            if (m_extraSettings != null && m_extraSettings.LoadLayers)
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                await LayersEditor.LoadLayersAsync(layers =>
                {
                    editor.IsBusy = false;
                });
            }
        }

        [Obsolete]
        private void OnDeleteProjectCompleted(object sender, RTSL.Interface.ProjectEventArgs<string> e)
        {
            if (e.Payload == PlayerPrefs.GetString(k_DefaultProjectKey))
            {
                PlayerPrefs.DeleteKey(k_DefaultProjectKey);
                PlayerPrefs.DeleteKey(k_DefaultProjectTypeKey);
            }
        }

        [Obsolete]
        private void OnBeginLoad(object sender, RTSL.Interface.ProjectEventArgs<RTSL.Interface.ProjectItem[]> e)
        {
            RaiseIfIsScene(e.Payload, () =>
            {
                IsPlaying = false;

                Selection.objects = null;
                Undo.Purge();

                SceneLoading?.Invoke();
            });
        }

        [Obsolete]
        private void OnLoadCompleted(object sender, RTSL.Interface.ProjectEventArgs<(RTSL.Interface.ProjectItem[] LoadedItems, UnityObject[] LoadedObjects)> e)
        {
            RaiseIfIsScene(e.Payload.LoadedItems, () =>
            {
                SceneLoaded?.Invoke();
            });
        }

        [Obsolete]
        private void OnBeginSave(object sender, RTSL.Interface.ProjectEventArgs<object[]> e)
        {
            object[] result = e.Payload;
            if (result != null && result.Length > 0)
            {
                IsPlaying = false;

                object obj = result[0];
                if (obj != null && obj is Scene)
                {
                    SceneSaving?.Invoke();
                }
            }
        }

        [Obsolete]
        private void OnSaveCompleted(object sender, RTSL.Interface.ProjectEventArgs<(RTSL.Interface.ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            RaiseIfIsScene(e.Payload.SavedItems, () =>
            {
                m_project.State.LoadedScene = e.Payload.SavedItems[0];
                SceneSaved?.Invoke();
            });
        }

        [Obsolete]

        private void RaiseIfIsScene(RTSL.Interface.ProjectItem[] projectItems, Action callback)
        {
            if (projectItems != null && projectItems.Length > 0)
            {
                var projectItem = projectItems[0];
                if (projectItem != null && m_project.Utils.IsScene(projectItem))
                {
                    callback();
                }
            }
        }

        [Obsolete]
        public RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.ProjectItem[]> DeleteAssetsLegacy(RTSL.Interface.ProjectItem[] projectItems, Action<RTSL.Interface.ProjectItem[]> done)
        {
            var ao = new RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.ProjectItem[]>();
            DeleteAssetsAsync(projectItems, ao, done);
            return ao;
        }

        [Obsolete]
        private async void DeleteAssetsAsync(RTSL.Interface.ProjectItem[] projectItems, RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.ProjectItem[]> ao, Action<RTSL.Interface.ProjectItem[]> callback)
        {
            var result = await DeleteAssetsAsync(projectItems);
            callback?.Invoke(result);
            ao.Result = result;
            ao.IsCompleted = true;
        }

        [Obsolete]
        public RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> CreatePrefab(RTSL.Interface.ProjectItem folder, ExposeToEditor obj, bool? includeDependencies, Action<RTSL.Interface.AssetItem[]> done)
        {
            var ao = new RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]>();
            CreatePrefabAsync(folder, obj, includeDependencies, ao, done);
            return ao;
        }


        [Obsolete]
        private async void CreatePrefabAsync(RTSL.Interface.ProjectItem folder, ExposeToEditor obj, bool? includeDependencies, RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> ao, Action<RTSL.Interface.AssetItem[]> callback)
        {
            var result = await CreatePrefabAsync(folder, obj, includeDependencies);
            callback?.Invoke(result.OfType<RTSL.Interface.AssetItem>().ToArray());
            ao.Result = result.OfType<RTSL.Interface.AssetItem>().ToArray();
            ao.IsCompleted = true;
        }


        [Obsolete] //12.11.2020
        public RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> SaveAssets(UnityObject[] assets, Action<RTSL.Interface.AssetItem[]> done)
        {
            RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> ao = new RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]>();

            SaveAssetsAsync(assets, ao, done);

            return ao;
        }

        [Obsolete] //12.11.2020
        private async void SaveAssetsAsync(UnityObject[] assets, RTSL.Interface.ProjectAsyncOperation<RTSL.Interface.AssetItem[]> ao, Action<RTSL.Interface.AssetItem[]> done)
        {
            var result = await SaveAssetsAsync(assets);
            done?.Invoke(result.OfType<RTSL.Interface.AssetItem>().ToArray());
            ao.Result = result.OfType<RTSL.Interface.AssetItem>().ToArray();
            ao.IsCompleted = true;
        }

        /// <summary>
        /// API Compatibility
        /// </summary>
        private class RuntimeSceneManagerInternal : RTSL.Interface.IRuntimeSceneManager
        {
            public event EventHandler NewSceneCreating;
            public event EventHandler NewSceneCreated;

            public void RaiseNewSceneCreating()
            {
                NewSceneCreating?.Invoke(this, EventArgs.Empty);
            }

            public void RaiseNewSceneCreated()
            {
                NewSceneCreated?.Invoke(this, EventArgs.Empty);
            }

            public async void CreateNewScene()
            {
                var editor = IOC.Resolve<IRuntimeEditor>();
                await editor?.InitializeNewSceneAsync();
            }

            public async void ClearScene()
            {
                var editor = IOC.Resolve<IRuntimeEditor>();
                await editor?.UnloadAllAndClearSceneAsync();
            }
        }

        #endregion
    }
}
