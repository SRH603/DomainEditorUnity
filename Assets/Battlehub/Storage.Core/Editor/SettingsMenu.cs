using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.Storage.Editor
{
    public class SettingsMenu : MonoBehaviour
    {
        private static bool IsSettingEnabled;
        private const string IncludePropertiesSettingPath = "Tools/Runtime Asset Database/Surrogate Settings/Include Properties";

        private static string GetConfigPath(string dir)
        {
            string devConfigPath = $"Assets/Battlehub.Extensions/Storage.DevUtils/Editor/SurrogatesGenConfig.json";
            string surrogatesGenConfigPath = File.Exists(devConfigPath) ?
                devConfigPath :
                $"{dir}/Surrogates/Editor/SurrogatesGenConfig.json";
            Directory.CreateDirectory(Path.GetDirectoryName(surrogatesGenConfigPath));
            return surrogatesGenConfigPath;
        }

        public static SurrogatesGenConfig LoadConfig()
        {
            string dir = StoragePath.DataFolder;
            string surrogatesGenConfigPath = GetConfigPath(dir);
            SurrogatesGenConfig config = SurrogatesGenConfig.Instance;
            if (File.Exists(surrogatesGenConfigPath))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(surrogatesGenConfigPath), config);
            }
            return config;
        }

        public static void SaveConfig(SurrogatesGenConfig config)
        {
            string dir = StoragePath.DataFolder;
            string surrogatesGenConfigPath = GetConfigPath(dir);
            File.WriteAllText(surrogatesGenConfigPath, JsonUtility.ToJson(config));
        }

        [MenuItem(IncludePropertiesSettingPath)]
        private static void ToggleIncludePropertiesSetting()
        {
            var config = LoadConfig();
            config.IncludeProperties = !config.IncludeProperties;
            SaveConfig(config);
        }

        [MenuItem(IncludePropertiesSettingPath, validate = true)]
        private static bool ValidateIncludePropertiesSetting()
        {
            var config = LoadConfig();
            Menu.SetChecked(IncludePropertiesSettingPath, config.IncludeProperties);
            return true;
        }
    }
}

