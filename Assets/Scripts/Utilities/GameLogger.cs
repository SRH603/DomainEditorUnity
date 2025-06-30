using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // ȷ������TextMeshPro�����ռ�
using UnityEngine.UI;

public class GameLogger : MonoBehaviour
{
    public TextMeshProUGUI logText; // ����Inspector�����ô˱���

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

        // ��־�߼�
        logText.text += $"{type}: {message}\n{stackTrace}\n";
    }

}
