using UnityEngine;

namespace Blackout.UI
{
    public static class CurveSettingsUtility
    {
        #if UNITY_EDITOR
        public static CurveEditorSettings GetOrCreateDefaultSettings()
        {
            CurveEditorSettings settings = Resources.Load<CurveEditorSettings>("Blackout/AnimationCurve/CurveEditorSettings");
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<CurveEditorSettings>();

                string folderPath = "Assets/Resources/Blackout/AnimationCurve";

                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    UnityEditor.AssetDatabase.Refresh();
                }
                
                UnityEditor.AssetDatabase.CreateAsset(settings, System.IO.Path.Combine(folderPath, "CurveEditorSettings.asset"));
                    
                Debug.LogWarning("No CurveEditorSettings was assigned, nor was there one in the project. A new one was created and assigned.");
            }
            else Debug.LogWarning("No CurveEditorSettings was assigned, the default settings asset was loaded from the resources folder.");

            return settings;
        }
        #endif
    }
}