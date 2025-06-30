using UnityEditor;
using UnityEngine;
using System.IO;

public class JsonManagerEditor : Editor
{
    [MenuItem("CONTEXT/Songselect/Export to JSON")]
    private static void ExportToJson(MenuCommand menuCommand)
    {
        SongSelect songSelect = (SongSelect)menuCommand.context;
        if (songSelect == null)
        {
            Debug.LogError("Songselect component not found.");
            return;
        }

        string outputDirectory = "output/json";
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string filePath = Path.Combine(outputDirectory, "packsContainer.json");
        string json = JsonUtility.ToJson(songSelect.archivePackContainer, true); // ʹ��������ʽ��
        File.WriteAllText(filePath, json);

        Debug.Log("Exported to JSON: " + filePath);
    }
}
