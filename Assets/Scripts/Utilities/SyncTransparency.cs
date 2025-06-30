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
        Color newColor = originalColors.pressedColor; // ���������ָ��һ���Զ�����ɫ
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

            // �ݹ���ã�ȷ�����������嶼������
            SetChildrenColor(child, color);
        }
    }
}
