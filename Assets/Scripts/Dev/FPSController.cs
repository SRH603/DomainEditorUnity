using UnityEngine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    public Text fpsText;  // 用来显示FPS的Text UI元素
    private float deltaTime = 0.0f;

    void Start()
    {
        Application.targetFrameRate = 240;
    }

    void Update()
    {
        // 使用一个平滑过渡来计算帧时间
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // 计算 FPS
        if (fpsText != null && deltaTime > 0.0f)
        {
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        }
    }
}
