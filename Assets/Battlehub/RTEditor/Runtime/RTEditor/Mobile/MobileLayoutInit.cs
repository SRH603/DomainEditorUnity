using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Battlehub.RTEditor.Mobile.Models;

namespace Battlehub.RTEditor.Mobile
{
    public static class MobileWindowNames
    {
        public const string GameObject = "MobileGameObject";
        public const string Component = "MobileComponent";
        public const string AddComponent = "MobileAddComponent";
        public const string Creator = "MobileCreator";
        public const string ContextMenu = "MobileContextMenu";
        public const string MainMenu = "MobileMainMenu";
        public const string Importer = "MobileImporter";
    }

    public class MobileLayoutInit : LayoutExtension
    {
        [SerializeField]
        private Region m_regionPrefab = null;

        [SerializeField]
        private Tab m_tabPrefab = null;

        [SerializeField]
        private Sprite m_componentWindowIcon = null;

        [SerializeField]
        private Sprite m_contextMenuWindowIcon = null;

        [SerializeField]
        private Sprite m_creatorWindowIcon = null;

        [SerializeField]
        private Sprite m_importWindowIcon = null;

        [SerializeField]
        private GameObject m_gameObjectWindow = null;

        [SerializeField]
        private GameObject m_componentWindow = null;

        [SerializeField]
        private GameObject m_addComponentWindow = null;

        [SerializeField]
        private GameObject m_contextMenuWindow = null;

        [SerializeField]
        private GameObject m_mainMenuWindow = null;

        [SerializeField]
        private GameObject m_creatorWindow = null;

        [SerializeField]
        private GameObject m_sceneWindow = null;

        [SerializeField]
        private GameObject m_importerWindow = null;

        [SerializeField]
        private GameObject m_headerPanel = null;

        [SerializeField]
        private GameObject m_footerPanel = null;

        [SerializeField]
        private bool m_hideMainMenu = true;

        [SerializeField]
        private bool m_hideFooter = true;

        [SerializeField]
        private bool m_setUIScale = true;

        [SerializeField]
        private float m_uiScaleOverride = 0.0f;

        private ISettingsComponent m_settingsComponent;
   
        protected override void Awake()
        {
            if (PersistentLayout && string.IsNullOrEmpty(PersistentLayoutName))
            {
                PersistentLayoutName = nameof(MobileLayoutInit);
            }
            base.Awake();
        }

        protected override void OnInit()
        {
            base.OnInit();

            RenderPipelineInfo.UseForegroundLayerForUI = false;

            IRTE editor = IOC.Resolve<IRTE>();

            if (m_regionPrefab != null)
            {
                DockPanel dockPanel = editor.Root.GetComponentInChildren<DockPanel>();
                if (dockPanel != null)
                {
                    dockPanel.RegionPrefab = m_regionPrefab;
                }
            }

            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.IsMainMenuActive = !m_hideMainMenu;
            appearance.IsFooterActive = !m_hideFooter;
            appearance.IsUIBackgroundActive = appearance.IsMainMenuActive || appearance.IsFooterActive;

            if (m_setUIScale)
            {
                m_settingsComponent = IOC.Resolve<ISettingsComponent>();
                m_settingsComponent.SettingsKeyPrefix = "Battlehub.RTEditor.Mobile.";
                if (m_uiScaleOverride > 0)
                {
                    m_settingsComponent.UIScale = m_uiScaleOverride;
                }
                else
                {
                    if (!PlayerPrefs.HasKey("Buttlehub.RTEditor.Mobile.IsIntialized"))
                    {
                        m_settingsComponent.UIScale = 3;
                        PlayerPrefs.SetInt("Buttlehub.RTEditor.Mobile.IsIntialized", 1);
                    }
                }

                m_settingsComponent.RotationSensitivity = 0.5f;
            }


            BusyIndicator busyIndicator = editor.Root.GetComponentInChildren<BusyIndicator>(true);
            if (busyIndicator != null)
            {
                RectTransform busyIndictorRT = busyIndicator.transform as RectTransform;
                if (busyIndictorRT != null)
                {
                    busyIndictorRT.anchorMin = new Vector2(0, 1);
                    busyIndictorRT.anchorMax = new Vector2(0, 1);
                    busyIndictorRT.pivot = new Vector2(0.5f, 0.5f);
                    busyIndictorRT.anchoredPosition = new Vector2(25, -25);
                }
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (m_coOrientationChangeHandler != null)
            {
                StopCoroutine(m_coOrientationChangeHandler);
                m_coOrientationChangeHandler = null;
            }
        }

        protected override void OnRegisterWindows(IWindowManager wm)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            GameObject tabPrefab = m_tabPrefab != null ? m_tabPrefab.gameObject : null;

            wm.OverrideWindow(BuiltInWindowNames.Scene, new WindowDescriptor { ContentPrefab = m_sceneWindow });

            RegisterWindow(wm, MobileWindowNames.GameObject, string.Empty, null, tabPrefab, m_gameObjectWindow);
            RegisterWindow(wm, MobileWindowNames.Component, string.Empty, m_componentWindowIcon, tabPrefab, m_componentWindow);
            RegisterWindow(wm, MobileWindowNames.AddComponent, string.Empty, null, tabPrefab, m_addComponentWindow);
            RegisterWindow(wm, MobileWindowNames.Creator, string.Empty, m_creatorWindowIcon, tabPrefab, m_creatorWindow);
            RegisterWindow(wm, MobileWindowNames.ContextMenu, string.Empty, m_contextMenuWindowIcon, tabPrefab, m_contextMenuWindow);
            RegisterWindow(wm, MobileWindowNames.MainMenu, string.Empty, null, tabPrefab, m_mainMenuWindow);

            RegisterWindow(wm, MobileWindowNames.Importer,
                lc.GetString("ID_RTEditor_MobileImporterView", "Import"),
                m_importWindowIcon,
                m_importerWindow,
                true);

            EnableStyling(tabPrefab);
            EnableStyling(m_gameObjectWindow);
            EnableStyling(m_addComponentWindow);
            EnableStyling(m_creatorWindow);
            EnableStyling(m_contextMenuWindow);
            EnableStyling(m_mainMenuWindow);
            EnableStyling(m_importerWindow);
        }

        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            if (m_coOrientationChangeHandler != null)
            {
                StopCoroutine(m_coOrientationChangeHandler);
                m_coOrientationChangeHandler = null;
            }

            if (m_hideFooter && m_footerPanel != null)
            {
                GameObject footerPanel = m_footerPanel;
                if (footerPanel.IsPrefab())
                {
                    footerPanel = Instantiate(m_footerPanel);
                    footerPanel.name = m_footerPanel.name;
                }

                EnableStyling(footerPanel);
                wm.SetBottomBar(footerPanel.transform);
            }

            if (m_hideMainMenu && m_headerPanel != null)
            {
                GameObject headerPanel = m_headerPanel;
                if (headerPanel.IsPrefab())
                {
                    headerPanel = Instantiate(m_headerPanel);
                    headerPanel.name = m_headerPanel.name;
                }

                EnableStyling(headerPanel);
                wm.SetTopBar(headerPanel.transform);
            }
        }

        protected override void OnAfterBuildLayout(IWindowManager wm)
        {
            base.OnAfterBuildLayout(wm);

            m_isPortrait = IsPortrait();
            m_coOrientationChangeHandler = CoOrientationChangeHandler();
            StartCoroutine(m_coOrientationChangeHandler);
        }

        protected override LayoutInfo GetSavedLayoutInfo(IWindowManager wm)
        {
            return GetLayoutInfo(wm);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            var scene = wm.CreateWindow(BuiltInWindowNames.Scene, out WindowDescriptor sceneDesc);
            var inspector = wm.CreateWindow(BuiltInWindowNames.Inspector, out WindowDescriptor inspectorDesc);
            var hierarchy = wm.CreateWindow(BuiltInWindowNames.Hierarchy, out WindowDescriptor hierarchyDesc);

            WindowDescriptor projectDesc;
            var project = wm.CreateWindow(BuiltInWindowNames.Project, out projectDesc);
            var animation = wm.CreateWindow(BuiltInWindowNames.Animation, out WindowDescriptor animationDesc);
            var console = wm.CreateWindow(BuiltInWindowNames.Console, out WindowDescriptor consoleDesc);
            var game = wm.CreateWindow(BuiltInWindowNames.Game, out WindowDescriptor gameDesc);

            LayoutInfo sceneInfo = wm.CreateLayoutInfo(scene, sceneDesc);
            sceneInfo.IsHeaderVisible = false;

            LayoutInfo inspectorInfo = CreateLayoutInfo(inspector, inspectorDesc);
            inspectorInfo.IsOn = true;

            LayoutInfo hierarchyInfo = CreateLayoutInfo(hierarchy, hierarchyDesc);
            LayoutInfo projectInfo = CreateLayoutInfo(project, projectDesc);
            LayoutInfo animationInfo = CreateLayoutInfo(animation, animationDesc);
            LayoutInfo consoleInfo = CreateLayoutInfo(console, consoleDesc);

            var tabGroup = new List<LayoutInfo> { inspectorInfo, hierarchyInfo, projectInfo, animationInfo, consoleInfo };
            if (wm.IsWindowRegistered("ProBuilder"))
            {
                var builder = wm.CreateWindow("ProBuilder", out WindowDescriptor builderDesc);
                tabGroup.Add(CreateLayoutInfo(builder, builderDesc));
            }

            LayoutInfo gameInfo = CreateLayoutInfo(game, gameDesc);
            tabGroup.Add(gameInfo);

            m_isPortrait = IsPortrait();

            LayoutInfo layoutInfo = new LayoutInfo(m_isPortrait,
                    sceneInfo,
                    new LayoutInfo(tabGroup.ToArray()),
                    0.6f);

            return layoutInfo;
        }

        private LayoutInfo CreateLayoutInfo(Transform content, WindowDescriptor desc)
        {
            Tab tab = Instantiate(m_tabPrefab);
            tab.name = $"Tab {desc.Header}";
            tab.Text = desc.Header;
            tab.Icon = desc.Icon;

            return new LayoutInfo(content, tab, false, false);
        }

        private IEnumerator m_coOrientationChangeHandler;
        private bool m_isPortrait;
        private bool IsPortrait()
        {
            return Screen.width < Screen.height;
        }

        private bool m_hasFocus = true;
        private void OnApplicationFocus(bool hasFocus)
        {
            m_hasFocus = hasFocus;
        }

        private IEnumerator CoOrientationChangeHandler()
        {
            while (true)
            {
                if (!m_hasFocus)
                {
                    yield return null;
                }

                bool wasPortrait = m_isPortrait;
                if (IsPortrait())
                {
                    m_isPortrait = true;
                }
                else
                {
                    m_isPortrait = false;
                }

                if (m_isPortrait != wasPortrait)
                {
                    IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                    if (editor != null)
                    {
                        var appModel = IOC.Resolve<IMobileEditorModel>();
                        if (appModel.IsMainMenuOpened)
                        {
                            appModel.IsMainMenuOpened = false;
                            yield return null;
                            yield return null;
                        }


                        IWindowManager wm = IOC.Resolve<IWindowManager>();
                        while (wm.IsDialogOpened)
                        {
                            wm.DestroyDialogWindow();
                        }

                        editor.ResetToDefaultLayout();
                        Canvas.ForceUpdateCanvases();
                    }
                }

                yield return null;
            }
        }
    }
}