using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Battlehub.Storage.Editor
{
    public class CreateSurrogatesWindow : SurrogatesWindow
    {
        [MenuItem("Tools/Runtime Asset Database/Create Surrogates ...")]
        public static void ShowWindow()
        {
            CreateSurrogatesWindow wnd = GetWindow<CreateSurrogatesWindow>();
            wnd.titleContent = new GUIContent("Create Surrogates");
        }

        protected override string UXMLFile => "Editor/Windows/CreateSurrogatesWindow.uxml";

        protected override bool TypeFilter(Type type)
        {
            return base.TypeFilter(type) && !TypeToSurrogateType.TryGetValue(type, out _) && !EnumeratorTypes.Contains(type);
        }

        public override void CreateGUI()
        {
            base.CreateGUI();
        }

        protected override void OnDefaultAction()
        {
            string dir = StoragePath.DataFolder;
            var config = SettingsMenu.LoadConfig();
            try
            {
                var selectTypesAndDependencies = GetSelectedTypesAndDependencies(recursive: true);
                foreach (Type type in selectTypesAndDependencies)
                {
                    if (type == typeof(UnityEngine.Object))
                    {
                        continue;
                    }

                    if (!TypeToSurrogateType.ContainsKey(type))
                    {
                        CreateSurrogate(dir, config, type);
                        CreateEnumerator(dir, type);
                    }
                }
            }
            finally
            {
                SettingsMenu.SaveConfig(config);
                base.OnDefaultAction();
            }
        }
    }
}

