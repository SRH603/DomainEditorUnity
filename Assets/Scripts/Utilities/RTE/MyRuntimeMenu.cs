using UnityEngine;
using UnityEngine.SceneManagement;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;

[MenuDefinition]
public static class MyRuntimeMenu
{
    // ======== 配置：返回 Hub 场景的方式 ========
    // 若为 true 则按 BuildIndex 加载；否则按场景名加载
    private const bool HUB_LOAD_BY_INDEX = false;

    // 用场景名返回（当 HUB_LOAD_BY_INDEX == false 时生效）
    private const string HUB_SCENE_NAME = "HubScene";
    
    // 用 BuildIndex 返回（当 HUB_LOAD_BY_INDEX == true 时生效）
    private const int HUB_SCENE_BUILD_INDEX = 0;

    // ============================
    // File：Back / Save / Save as
    // ============================

    [MenuCommand("MenuFile/Back")]
    public static void File_Back()
    {
        // 如需离开前自动保存，可在此调用 DechHub.Instance.Save();
        if (HUB_LOAD_BY_INDEX)
            SceneManager.LoadScene(HUB_SCENE_BUILD_INDEX, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(HUB_SCENE_NAME, LoadSceneMode.Single);
    }

    [MenuCommand("MenuFile/Save")]
    public static void File_Save()
    {
        // 与 Hub 行为一致：内部会在未打开会话时给出错误提示（你在 Hub 里已实现）
        DechHub.Instance.Save();
    }

    [MenuCommand("MenuFile/Save as")]
    public static void File_SaveAs()
    {
        // 与 Hub 行为一致：弹系统原生另存为对话框并写入
        DechHub.Instance.SaveAs();
    }

    // —— 彻底移除 File 下不需要的项（包含多种命名/省略号变体，以兜底清理）——

    // 1) New / New … / New Scene …
    [MenuCommand("MenuFile/New", hide: true)] public static void _hideFileNew() { }
    [MenuCommand("MenuFile/New...", hide: true)] public static void _hideFileNewDots() { }
    [MenuCommand("MenuFile/New Scene", hide: true)] public static void _hideFileNewScene() { }
    [MenuCommand("MenuFile/New scene", hide: true)] public static void _hideFileNewscene() { }
    [MenuCommand("MenuFile/New Scene...", hide: true)] public static void _hideFileNewSceneDots() { }
    [MenuCommand("MenuFile/New scene...", hide: true)] public static void _hideFileNewsceneDots() { }

    // 2) Save Scene / Save Scene As
    [MenuCommand("MenuFile/Save Scene", hide: true)] public static void _hideSaveScene() { }
    [MenuCommand("MenuFile/Save Scene As", hide: true)] public static void _hideSaveSceneAs() { }
    [MenuCommand("MenuFile/Save Scene as", hide: true)] public static void _hideSaveSceneas() { }
    [MenuCommand("MenuFile/Save Scene As...", hide: true)] public static void _hideSaveSceneAsDots() { }
    [MenuCommand("MenuFile/Save Scene as...", hide: true)] public static void _hideSaveSceneasDots() { }

    // 3) Manage Projects
    [MenuCommand("MenuFile/Manage Projects", hide: true)] public static void _hideManageProjects() { }
    [MenuCommand("MenuFile/Manage Project", hide: true)] public static void _hideManageProject() { }
    [MenuCommand("MenuFile/Projects", hide: true)] public static void _hideProjects() { }
    [MenuCommand("MenuFile/Project Manager", hide: true)] public static void _hideProjectManager() { }

    // 4) 其它默认项（兜底）
    [MenuCommand("MenuFile/Open", hide: true)] public static void _hideFileOpen() { }
    [MenuCommand("MenuFile/Import From File", hide: true)] public static void _hideImportFromFile() { }
    [MenuCommand("MenuFile/Import Assets", hide: true)] public static void _hideImportAssets() { }
    [MenuCommand("MenuFile/Close", hide: true)] public static void _hideFileClose() { }
    [MenuCommand("MenuFile/Exit", hide: true)] public static void _hideFileExit() { }

    // ============================
    // Edit：删除 Play / Stop（其余不变）
    // ============================
    [MenuCommand("MenuEdit/Play", hide: true)] public static void _hideEditPlay() { }
    [MenuCommand("MenuEdit/Stop", hide: true)] public static void _hideEditStop() { }

    // ============================
    // GameObject：整类菜单移除（根 + 常见子项兜底）
    // ============================
    [MenuCommand("MenuGameObject", hide: true)] public static void _hideGO_Root() { }
    [MenuCommand("MenuGameObject/Create Empty", hide: true)] public static void _hideGO_CreateEmpty() { }
    [MenuCommand("MenuGameObject/3D Object", hide: true)] public static void _hideGO_3D() { }
    [MenuCommand("MenuGameObject/UI", hide: true)] public static void _hideGO_UI() { }
    [MenuCommand("MenuGameObject/Light", hide: true)] public static void _hideGO_Light() { }
    [MenuCommand("MenuGameObject/Audio", hide: true)] public static void _hideGO_Audio() { }
    [MenuCommand("MenuGameObject/Effects", hide: true)] public static void _hideGO_Effects() { }
    [MenuCommand("MenuGameObject/Camera", hide: true)] public static void _hideGO_Camera() { }
    [MenuCommand("MenuGameObject/Create Prefab", hide: true)] public static void _hideGO_CreatePrefab() { }

    // ============================
    // Help：移除 About RTE，保留你的外链
    // ============================
    [MenuCommand("MenuHelp/About RTE", hide: true)]
    public static void _hideAboutRTE() { }

    [MenuCommand("MenuHelp/Project Site")]
    public static void Help_OpenExternal()
    {
        Application.OpenURL("https://your.link.here/"); // TODO: 替换为你的外链
    }
}
