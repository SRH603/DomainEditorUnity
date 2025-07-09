using Battlehub.RTCommon;
using System;
using UnityEngine;

namespace Battlehub.RTEditor
{
   
    [DefaultExecutionOrder(-100)]
    public class BuiltInWindows : MonoBehaviour
    {
        [SerializeField]
        private CustomWindowDescriptor[] m_windows = null;
        private void Awake()
        {
            IRTEAppearance appearance = GetComponent<IRTEAppearance>();
            WindowManager wm = GetComponent<WindowManager>();
            if(wm.UseLegacyBuiltInWindows)
            {
                return;
            }

            for(int i = 0; i < m_windows.Length; ++i)
            {
                CustomWindowDescriptor desc = m_windows[i];
                if(desc == null)
                {
                    Debug.LogWarning($"CustomWindowDescriptor is null. Index: {i}");
                    continue;
                }

                if(!wm.RegisterWindow(desc))
                {
                    Debug.LogWarning($"Window of type {desc.TypeName} already registered");
                }

                if (desc.Descriptor.ContentPrefab != null)
                {
                    appearance.RegisterPrefab(desc.Descriptor.ContentPrefab);
                }
            }
        }
    }

    public static class BuiltInWindowNames
    {
        public readonly static string Game = RuntimeWindowType.Game.ToString().ToLower();
        public readonly static string Scene = RuntimeWindowType.Scene.ToString().ToLower();
        public readonly static string Hierarchy = RuntimeWindowType.Hierarchy.ToString().ToLower();
        public readonly static string ProjectTree = RuntimeWindowType.ProjectTree.ToString().ToLower();
        public readonly static string ProjectFolder = RuntimeWindowType.ProjectFolder.ToString().ToLower();
        public readonly static string Inspector = RuntimeWindowType.Inspector.ToString().ToLower();
        public readonly static string Console = RuntimeWindowType.Console.ToString().ToLower();
        public readonly static string Animation = RuntimeWindowType.Animation.ToString().ToLower();

        public readonly static string ToolsPanel = RuntimeWindowType.ToolsPanel.ToString().ToLower();

        public readonly static string ImportFile = RuntimeWindowType.ImportFile.ToString().ToLower();
        public readonly static string OpenProject = RuntimeWindowType.OpenProject.ToString().ToLower();
        
        public readonly static string About = RuntimeWindowType.About.ToString().ToLower();
        public readonly static string SaveFile = RuntimeWindowType.SaveFile.ToString().ToLower();
        public readonly static string OpenFile = RuntimeWindowType.OpenFile.ToString().ToLower();

        public readonly static string SelectColor = RuntimeWindowType.SelectColor.ToString().ToLower();
        public readonly static string SelectAnimationProperties = RuntimeWindowType.SelectAnimationProperties.ToString().ToLower();

        public readonly static string Settings = RuntimeWindowType.Settings.ToString().ToLower();
        
        public readonly static string NoteEditor = RuntimeWindowType.NoteEditor.ToString().ToLower();
        public readonly static string JudgmentLineEditor = RuntimeWindowType.JudgmentLineEditor.ToString().ToLower();
        public readonly static string LevelEditor = RuntimeWindowType.LevelEditor.ToString().ToLower();
        public readonly static string NoteDetailEditor = RuntimeWindowType.NoteDetailEditor.ToString().ToLower();
        
        public readonly static string Empty = RuntimeWindowType.Empty.ToString().ToLower();
        public readonly static string EmptyDialog = RuntimeWindowType.EmptyDialog.ToString().ToLower();

        private static bool LegacyRTSLMode
        {
            get { return IOC.Resolve<IRuntimeEditor>().CompatibilityMode == CompatibilityMode.LegacyRTSL; }
        }

        public readonly static string AssetDatabase = "assetdatabase";
        public readonly static string AssetDatabaseSaveScene = "assetdatabasesavescene";
        public readonly static string AssetDatabaseSave = "assetdatabasesave";
        public readonly static string AssetDatabaseSelect = "assetdatabaseselect";
        public readonly static string AssetDatabaseImportSource = "assetdatabaseimportsource";
        public readonly static string AssetDatabaseImport = "assetdatabaseimport";

        public static string Project => LegacyRTSLMode ? RuntimeWindowType.Project.ToString().ToLower() : AssetDatabase;
        public static string SaveScene => LegacyRTSLMode ? RuntimeWindowType.SaveScene.ToString().ToLower() : AssetDatabaseSaveScene;
        public static string SaveAsset => LegacyRTSLMode ? RuntimeWindowType.SaveAsset.ToString().ToLower() : AssetDatabaseSave;
        public static string SelectObject => LegacyRTSLMode ? RuntimeWindowType.SelectObject.ToString().ToLower() : AssetDatabaseSelect;
        public static string SelectAssetLibrary => LegacyRTSLMode ? RuntimeWindowType.SelectAssetLibrary.ToString().ToLower() : AssetDatabaseImportSource;
        public static string ImportAssets => LegacyRTSLMode ? RuntimeWindowType.ImportAssets.ToString().ToLower() : AssetDatabaseImport;


    }
}

