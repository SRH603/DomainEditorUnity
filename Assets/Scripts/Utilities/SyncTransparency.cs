using UnityEngine;
using UnityEngine.UI;

public class SyncTransparency : MonoBehaviour
{
    private Button button;
    private ColorBlock originalColors;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            originalColors = button.colors;
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        Color newColor = originalColors.pressedColor; // 或者你可以指定一个自定义颜色
        SetChildrenColor(transform, newColor);
    }

    void SetChildrenColor(Transform parent, Color color)
    {
        foreach (Transform child in parent)
        {
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = color;
            }

            // 递归调用，确保所有子物体都被处理
            SetChildrenColor(child, color);
        }
    }
}
