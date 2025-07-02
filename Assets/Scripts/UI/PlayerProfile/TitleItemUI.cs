using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleItemUI : MonoBehaviour
{
    public TMP_Text titleText;
    public Image rarityBorder;
    public GameObject lockOverlay;
    public Button selectButton;

    private string id;

    public void Setup(TitleData data, bool isUnlocked, System.Action<string> onClick)
    {
        id = data.id;
        // 根据系统语言选择文本
        bool useZh = Application.systemLanguage == SystemLanguage.ChineseSimplified
                     || Application.systemLanguage == SystemLanguage.ChineseTraditional;
        titleText.text = useZh ? data.zhText : data.enText;

        // 边框颜色可根据稀有度定制
        rarityBorder.color = GetColorByRarity(data.rarity);

        lockOverlay.SetActive(!isUnlocked);
        selectButton.interactable = isUnlocked;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick(id));
    }

    private Color GetColorByRarity(Rarity r)
    {
        switch (r)
        {
            case Rarity.Common:    return Color.white;
            case Rarity.Rare:      return Color.cyan;
            case Rarity.Epic:      return Color.magenta;
            case Rarity.Legendary: return Color.yellow;
            default:               return Color.gray;
        }
    }
}