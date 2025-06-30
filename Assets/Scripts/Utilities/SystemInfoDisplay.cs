using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SystemInfoDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private GUIStyle guiStyle = new GUIStyle();
    public Toggle toggle;
    public bool Frameratelimit;
    public int Frameratelimits;

    void Start()
    {
        guiStyle.fontSize = 40; // ���Ը�����Ҫ�������ֵ
        guiStyle.normal.textColor = Color.white; // ����������ɫΪ��ɫ
        if(Frameratelimit)
        Application.targetFrameRate = Frameratelimits;
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (!toggle.isOn) return;

        float fps = 1.0f / deltaTime;
        string fpsText = string.Format("{0:0.} fps", fps);

        string info = "Operating System: " + SystemInfo.operatingSystem +
                      "\nDevice Model: " + SystemInfo.deviceModel +
                      "\nDevice Type: " + SystemInfo.deviceType +
                      "\nProcessor Type: " + SystemInfo.processorType +
                      "\nProcessor Count: " + SystemInfo.processorCount +
                      "\nGraphics Device Name: " + SystemInfo.graphicsDeviceName +
                      "\nGraphics Device Type: " + SystemInfo.graphicsDeviceType +
                      "\nGraphics Device Vendor: " + SystemInfo.graphicsDeviceVendor +
                      "\nGraphics Memory Size: " + SystemInfo.graphicsMemorySize + " MB" +
                      "\nMemory Size: " + SystemInfo.systemMemorySize + " MB" +
                      "\nScreen Resolution: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
                      "\nScreen DPI: " + Screen.dpi +
                      "\nInput Mode: " + (Input.touchSupported ? "Touch" : "Mouse") +
                      "\nFrame Rate: " + fpsText;

        // ����Ϸ��ͼ��ʹ���Զ�����ʽ��ʾ��Ϣ
        GUI.Label(new Rect(10, 10, 800, 600), info, guiStyle);
    }
}
