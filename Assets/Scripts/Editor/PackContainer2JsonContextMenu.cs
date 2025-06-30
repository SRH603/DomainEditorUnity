using UnityEngine;
using UnityEditor;
using System.IO;

public class PackContainer2JsonContextMenu
{
    [MenuItem("GameObject/PackContainer2Json/Export to JSON", false, 10)]
    private static void ExportToJsonMenuItem(MenuCommand menuCommand)
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            PackContainer2Json myScript = selectedObject.GetComponent<PackContainer2Json>();
            if (myScript != null)
            {
                string filePath = EditorUtility.SaveFilePanel("Export to JSON", Application.dataPath, "packsContainer", "json");
                if (!string.IsNullOrEmpty(filePath))
                {
                    myScript.ExportToJson(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
                }
            }
            else
            {
                Debug.LogWarning("Selected GameObject does not have PackContainer2Json component.");
            }
        }
    }

    [MenuItem("GameObject/PackContainer2Json/Import from JSON", false, 10)]
    private static void ImportFromJsonMenuItem(MenuCommand menuCommand)
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            PackContainer2Json myScript = selectedObject.GetComponent<PackContainer2Json>();
            if (myScript != null)
            {
                string filePath = EditorUtility.OpenFilePanel("Import from JSON", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(filePath))
                {
                    myScript.JsonFile = new TextAsset(File.ReadAllText(filePath));
                    myScript.ImportFromJson();
                }
            }
            else
            {
                Debug.LogWarning("Selected GameObject does not have PackContainer2Json component.");
            }
        }
    }
}
