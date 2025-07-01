using System.Collections;
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
            //StartCoroutine(LoadSongSelectAsync());
            // 切换场景
            SceneLoader.Load(sceneName);

            //SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty or invalid!");
        }
    }
    IEnumerator LoadSongSelectAsync()
    {
        // 1. 开始加载，但先别切
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        // 2. 在这里可以显示一个 loading UI，更新进度条
        while (op.progress < 0.9f)
        {
            //LoadingUI.Instance.SetProgress(op.progress);  // 0~0.9
            yield return null;                            // 让一帧
        }

        // 3. 可做自定义预处理（如加载用户数据、配置文件等）
        yield return new WaitForSeconds(0.1f);            // 小缓冲，可选

        // 4. 切场景
        op.allowSceneActivation = true;

        // 5. 等真正切完（isDone == true）
        while (!op.isDone) yield return null;
    }
}
