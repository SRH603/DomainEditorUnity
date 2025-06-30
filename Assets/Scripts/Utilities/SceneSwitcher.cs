using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 引入场景管理库

public class SceneSwitcher : MonoBehaviour
{
    // 公开的场景名称，可以在 Unity 编辑器中设置
    public string sceneName;

    private void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }

    // 触发切换场景的方法
    public void SwitchScene()
    {
        // 检查场景是否有效，避免发生错误
        if (!string.IsNullOrEmpty(sceneName))
        {
            // 切换场景
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty or invalid!");
        }
    }
}
