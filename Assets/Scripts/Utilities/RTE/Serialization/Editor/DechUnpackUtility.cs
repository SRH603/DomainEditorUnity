#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "DECH/分解工具 (Unpack DECH)", fileName = "DechUnpackUtility")]
public class DechUnpackUtility : ScriptableObject
{
    [Header("输入")]
    [Tooltip("拖一个 .dech 资产（Project 视图里的 .dech 文件）")]
    public UnityEngine.Object dechAsset;

    const string kOutDirRel = "Assets/dech";  // 输出目录（相对）
    static string OutDirAbs => Path.Combine(Application.dataPath, "dech");

    /// <summary>执行分解：从 .dech 解析出 GameData 资产 + 音频文件 到 Assets/dech</summary>
    public void UnpackDech()
    {
        if (dechAsset == null) { EditorUtility.DisplayDialog("DECH 分解", "请先指定 .dech 资产。", "OK"); return; }

        var dechRel = AssetDatabase.GetAssetPath(dechAsset);
        if (string.IsNullOrEmpty(dechRel) || !dechRel.EndsWith(".dech", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("DECH 分解", "指定对象不是 .dech 文件。", "OK"); 
            return;
        }

        string dechAbs = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, dechRel));

        // 1) 读取容器
        GameDataDTO dto;
        string audioExt;
        byte[] audioBytes;
        try
        {
            (dto, audioExt, audioBytes) = DechContainer.ReadAndUnpack(dechAbs);
            if (string.IsNullOrEmpty(audioExt)) audioExt = "wav";
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("DECH 分解", "读取 .dech 失败：\n" + ex.Message, "OK");
            return;
        }

        // 2) 准备输出路径
        try { Directory.CreateDirectory(OutDirAbs); } catch (Exception ex) { Debug.LogException(ex); }
        string baseName = Path.GetFileNameWithoutExtension(dechRel);

        string soRel = AssetDatabase.GenerateUniqueAssetPath($"{kOutDirRel}/{baseName}_GameData.asset");
        string audRel = AssetDatabase.GenerateUniqueAssetPath($"{kOutDirRel}/{baseName}_audio.{audioExt}");

        string soAbs  = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, soRel));
        string audAbs = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, audRel));

        // 3) 生成 GameData 资产
        try
        {
            var so = ScriptableObject.CreateInstance<GameData>();
            GameDataMapper.FromDTO(dto, so);
            AssetDatabase.CreateAsset(so, soRel);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("DECH 分解", "创建 GameData 资产失败：\n" + ex.Message, "OK");
            return;
        }

        // 4) 写出音频文件
        try
        {
            File.WriteAllBytes(audAbs, audioBytes);
            AssetDatabase.ImportAsset(audRel, ImportAssetOptions.ForceUpdate);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("DECH 分解", "写入音频文件失败：\n" + ex.Message, "OK");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(soRel);

        EditorUtility.DisplayDialog("DECH 分解", $"成功输出：\n- {soRel}\n- {audRel}", "OK");
    }
}

// ============= 自定义 Inspector =============
[CustomEditor(typeof(DechUnpackUtility))]
public class DechUnpackUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dechAsset"));

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(
            serializedObject.FindProperty("dechAsset").objectReferenceValue == null))
        {
            if (GUILayout.Button("⬇ 分解 .dech 到 Assets/dech"))
            {
                (target as DechUnpackUtility)?.UnpackDech();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
