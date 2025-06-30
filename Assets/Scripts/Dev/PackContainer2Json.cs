using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class PackContainer2Json : MonoBehaviour
{
    public GameObject Main;
    public TextAsset JsonFile;
    [FormerlySerializedAs("packsContainer")] public ArchivePacksContainer archivePacksContainer;

    private void Start()
    {
        // ��ѡ��������������ʼ��һЩ����
    }

    // ���� JSON �ļ�
    public void ExportToJson(string directoryPath, string fileName)
    {
        // ȷ��Ŀ¼����
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string combinedPath = Path.Combine(directoryPath, fileName + ".json");
        archivePacksContainer = Main.GetComponent<SongSelect>().archivePackContainer;
        string json = JsonUtility.ToJson(archivePacksContainer, true); // ʹ��������ʽ��
        File.WriteAllText(combinedPath, json);
        Debug.Log("Exported to JSON: " + combinedPath);
    }

    // �� JSON �ļ�����
    public void ImportFromJson()
    {
        if (JsonFile != null)
        {
            string json = JsonFile.text;
            Main.GetComponent<SongSelect>().archivePackContainer = JsonUtility.FromJson<ArchivePacksContainer>(json);
            Debug.Log("Imported from JSON: ");
        }
        else
        {
            Debug.LogError("No JSON file assigned.");
        }
    }
}
