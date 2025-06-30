using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // 确保导入TextMeshPro命名空间
using UnityEngine.UI;

public class GameLogger : MonoBehaviour
{
    public TextMeshProUGUI logText; // 将在Inspector中设置此变量

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }


    private void HandleLog(string message, string stackTrace, LogType type)
    {
        if (logText == null)
        {
            Debug.LogError("logText is not set in the inspector");
            return;
        }

        // 日志逻辑
        logText.text += $"{type}: {message}\n{stackTrace}\n";
    }

}
