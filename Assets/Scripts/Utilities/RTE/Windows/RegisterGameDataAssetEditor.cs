// Assets/Scripts/Utilities/RTE/Windows/RegisterGameDataAssetEditor.cs
using UnityEngine;
using Battlehub.RTCommon;
using Battlehub.RTEditor;

public class RegisterGameDataAssetEditor : EditorExtension
{
    [Header("把 RTE 自带的 AssetEditor 预制体拖进来")]
    [SerializeField] private GameObject assetEditorPrefab;

    protected override void OnInit()
    {
        base.OnInit();

        if (assetEditorPrefab == null)
        {
            Debug.LogWarning("[RTE] 未指定 AssetEditor 预制体，若 Inspector 已能显示 ScriptableObject 可忽略此项。");
            return;
        }

        var editorsMap = IOC.Resolve<IEditorsMap>();
        if (editorsMap == null)
        {
            Debug.LogError("[RTE] 无法解析 IEditorsMap。");
            return;
        }

        // 若已经有映射就不重复添加；否则为 GameData 注册 AssetEditor
        if (!editorsMap.HasMapping(typeof(GameData)))
        {
            editorsMap.AddMapping(typeof(GameData), assetEditorPrefab, enabled: true, isPropertyEditor: false);
            Debug.Log("[RTE] 已为 GameData 注册 AssetEditor 映射。");
        }
    }
}