using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;

namespace Battlehub.Storage.Editor
{
    public class UpdateSurrogatesWindow : SurrogatesWindow
    {
        [MenuItem("Tools/Runtime Asset Database/Update Surrogates ...")]
        public static void ShowWindow()
        {
            UpdateSurrogatesWindow wnd = GetWindow<UpdateSurrogatesWindow>();
            wnd.titleContent = new GUIContent("Update Surrogates");
        }

        protected override bool EnumeratorTypeFilter(Type type)
        {
            return type != null && type.GetCustomAttribute<ObjectEnumeratorAttribute>() != null && typeof(IObjectEnumerator).IsAssignableFrom(type);
        }

        protected override string UXMLFile => "Editor/Windows/UpdateSurrogatesWindow.uxml";

        protected override bool TypeFilter(Type type)
        {
            return base.TypeFilter(type) && (CanUpdateSurrogate(type) || CanUpdateEnumerator(type));

        }

        private bool CanUpdateSurrogate(Type type)
        {
            bool result = TypeToSurrogateType.TryGetValue(type, out var surrogateType) &&  SurrogatesGen.CanUpdateSurrogate(type, surrogateType);
            return result;
        }

        private bool CanUpdateEnumerator(Type type)
        {
            string dir = StoragePath.DataFolder;
            bool result = TypeToSurrogateType.TryGetValue(type, out var surrogateType) && 
                          SurrogatesGen.CanUpdateSurrogate(surrogateType) &&
                          SurrogatesGen.CanCreateEnumerator(type) &&
                          !File.Exists(GetEnumeratorPath(type, dir));
            return result;
        }

        protected override void OnDefaultAction()
        {
            string dir = StoragePath.DataFolder;
            var config = SettingsMenu.LoadConfig(); 
            try
            {
                var selectedTypes = SelectedTypes;
                var dependenciesHs = GetDependenciesOfSelectedTypes(recursive: false);

                foreach (Type type in selectedTypes)
                {
                    if (type == typeof(UnityEngine.Object))
                    {
                        continue;
                    }

                    if (CanUpdateSurrogate(type))
                    {
                        UpdateSurrogate(type);
                    }

                    UpdateEnumerator(dir, type);
                }

                foreach (Type type in dependenciesHs)
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

   
        private void UpdateEnumerator(string dir, Type type)
        {
            string enumeratorsPath = $"{dir}/Surrogates/Enumerators";
            Directory.CreateDirectory(Path.GetDirectoryName(enumeratorsPath));

            string enumeratorText = SurrogatesGen.GetUpdatedEnumeratorCode(type, TypeToSurrogateType[type]);
            if (!string.IsNullOrEmpty(enumeratorText))
            {
                string enumeratorPath = GetEnumeratorPath(type, dir);
                File.WriteAllText(enumeratorPath, enumeratorText);
            }
        }

        private string GetEnumeratorPath(Type type, string dir)
        {
            string enumeratorsPath = $"{dir}/Surrogates/Enumerators";
            string enumeratorPath;
            if (TypeToEnumeratorType.TryGetValue(type, out var enumeratorType))
            {
                enumeratorPath = enumeratorType.GetCustomAttribute<ObjectEnumeratorAttribute>().FilePath;
            }
            else
            {
                enumeratorPath = $"{enumeratorsPath}/{type.FullName}Enumerator.cs";
            }

            return enumeratorPath;
        }

        private void UpdateSurrogate(Type type)
        {
            string surrogatePath = TypeToSurrogateType[type].GetCustomAttribute<SurrogateAttribute>().FilePath;
            string surrogateText = File.ReadAllText(surrogatePath);

            surrogateText = SurrogatesGen.GetUpdatedSurrogateCode(type, TypeToSurrogateType[type], surrogateText);

            File.WriteAllText(surrogatePath, surrogateText);
        }
    }
}

