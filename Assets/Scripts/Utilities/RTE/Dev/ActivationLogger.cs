using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

//using System.Diagnostics;

public class ActivationLogger : MonoBehaviour
{
    void OnDisable()
    {
        // 第一次禁用时打印警告和调用堆栈
        Debug.LogWarning($"{name} 在第 {Time.frameCount} 帧 被禁用了！", this);
        Debug.Log(new StackTrace(1, true).ToString(), this);
    }

    void OnEnable()
    {
        // 可选：打印重新启用
        Debug.Log($"{name} 被启用", this);
    }
}