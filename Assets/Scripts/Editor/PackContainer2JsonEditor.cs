using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(PackContainer2Json))]
public class PackContainer2JsonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PackContainer2Json myScript = (PackContainer2Json)target;

        if (GUILayout.Button("Export to JSON"))
        {
            string filePath = EditorUtility.SaveFilePanel("Export to JSON", Application.dataPath, "packsContainer", "json");
            if (!string.IsNullOrEmpty(filePath))
            {
                myScript.ExportToJson(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
            }
        }

        if (GUILayout.Button("Import from JSON"))
        {
            string filePath = EditorUtility.OpenFilePanel("Import from JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(filePath))
            {
                myScript.JsonFile = new TextAsset(File.ReadAllText(filePath));
                myScript.ImportFromJson();
            }
        }
    }
}
