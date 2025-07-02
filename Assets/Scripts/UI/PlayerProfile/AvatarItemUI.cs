using UnityEngine;
using UnityEngine.UI;

public class AvatarItemUI : MonoBehaviour
{
    public Image avatarImage;
    public Image rarityBorder;
    public GameObject lockOverlay;
    public Button selectButton;

    private string id;

    public void Setup(AvatarData data, bool isUnlocked, System.Action<string> onClick)
    {
        id = data.id;
        avatarImage.sprite = data.sprite;
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