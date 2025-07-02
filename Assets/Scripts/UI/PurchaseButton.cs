using UnityEngine;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public SongSelect manager;
    public GameObject successPanel;
    public GameObject failurePanel;
    public GameObject unlockPanel;

    private Button _button;

    private void Awake()
    {
        // 获取当前物体上的 Button 组件
        _button = GetComponent<Button>();

        // 订阅点击事件
        _button.onClick.AddListener(OnPurchaseClicked);

        // 初始时隐藏成功／失败面板
        if (successPanel != null) successPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
    }

    /// <summary>
    /// 按钮点击回调：执行购买并显示对应 UI
    /// </summary>
    private void OnPurchaseClicked()
    {
        bool isSuccess = manager.Purchase();
        if (isSuccess)
        {
            // 弹出成功界面
            successPanel?.SetActive(true);
            failurePanel?.SetActive(false);
            unlockPanel?.SetActive(false);
        }
        else
        {
            // 弹出失败界面
            successPanel?.SetActive(false);
            failurePanel?.SetActive(true);
        }
    }
}