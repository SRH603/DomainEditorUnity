#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "DECH/打包工具 (Pack DECH)", fileName = "DechPackUtility")]
public class DechPackUtility : ScriptableObject
{
    [Header("输入")]
    public GameData gameData;         // 要打包的 ScriptableObject
    public AudioClip audioClip;       // 要打包的音频（建议来自项目内文件）

    [Header("输出")]
    [Tooltip("输出文件名（可空，默认使用 GameData 名称或 audio 名称）")]
    public string outputName;

    const string kOutDirRel = "Assets/dech";  // 相对路径
    static string OutDirAbs => Path.Combine(Application.dataPath, "dech"); // 绝对路径

    /// <summary>执行打包：生成 .dech 到 Assets/dech</summary>
    public void PackToDech()
    {
        if (gameData == null) { EditorUtility.DisplayDialog("DECH 打包", "请先指定 GameData。", "OK"); return; }
        if (audioClip == null) { EditorUtility.DisplayDialog("DECH 打包", "请先指定音频 AudioClip。", "OK"); return; }

        // 1) 生成 DTO
        var dto = GameDataMapper.ToDTO(gameData);

        // 2) 准备音频字节 & 扩展名
        byte[] audioBytes = null;
        string audioExt   = null;

        try
        {
            var audPathRel = AssetDatabase.GetAssetPath(audioClip);
            if (!string.IsNullOrEmpty(audPathRel) && File.Exists(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, audPathRel)))
            {
                // 从项目源文件直接读取字节（首选，保留原格式）
                var absPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, audPathRel));
                audioBytes = File.ReadAllBytes(absPath);
                audioExt   = Path.GetExtension(absPath)?.TrimStart('.').ToLowerInvariant();
                if (string.IsNullOrEmpty(audioExt)) audioExt = "wav";
            }
            else
            {
                // 兜底：将 AudioClip 转 WAV
                audioBytes = WavWriter.FromAudioClip(audioClip);
                audioExt   = "wav";
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("DECH 打包", "读取音频失败：\n" + ex.Message, "OK");
            return;
        }

        // 3) 输出目录 & 文件名
        try { Directory.CreateDirectory(OutDirAbs); } catch (Exception ex) { Debug.LogException(ex); }
        string baseName = !string.IsNullOrWhiteSpace(outputName)
            ? outputName.Trim()
            : (gameData != null ? gameData.name : (audioClip != null ? audioClip.name : "chart"));

        // 统一清理非法文件名字符
        foreach (var c in Path.GetInvalidFileNameChars()) baseName = baseName.Replace(c, '_');

        string outRel = AssetDatabase.GenerateUniqueAssetPath($"{kOutDirRel}/{baseName}.dech");
        string outAbs = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, outRel));

        // 4) 打包写入
        try
        {
            DechContainer.PackAndWrite(outAbs, dto, audioExt, audioBytes);
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(outRel);
            EditorUtility.DisplayDialog("DECH 打包", $"生成成功：\n{outRel}", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("DECH 打包", "生成失败：\n" + ex.Message, "OK");
        }
    }
}

// ============= 自定义 Inspector =============
[CustomEditor(typeof(DechPackUtility))]
public class DechPackUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameData"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("audioClip"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outputName"));

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(
            serializedObject.FindProperty("gameData").objectReferenceValue == null ||
            serializedObject.FindProperty("audioClip").objectReferenceValue == null))
        {
            if (GUILayout.Button("▶ 生成 .dech 到 Assets/dech"))
            {
                (target as DechPackUtility)?.PackToDech();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
