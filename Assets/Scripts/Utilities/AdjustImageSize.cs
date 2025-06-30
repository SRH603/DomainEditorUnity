using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AdjustImageSize : MonoBehaviour
{
    void Start()
    {
        AdjustSize();
    }

    void AdjustSize()
    {
        Image image = GetComponent<Image>();
        Sprite sprite = image.sprite;

        if (sprite == null)
        {
            Debug.LogError("Sprite is null");
            return;
        }

        Texture2D texture = sprite.texture;
        if (texture == null)
        {
            Debug.LogError("Texture is null");
            return;
        }

        if (!texture.isReadable)
        {
            Debug.LogError("Texture is not readable");
            return;
        }

        Rect spriteRect = sprite.textureRect;

        // Ensuring spriteRect dimensions are within bounds of the texture
        if (spriteRect.x < 0 || spriteRect.y < 0 || spriteRect.x + spriteRect.width > texture.width || spriteRect.y + spriteRect.height > texture.height)
        {
            Debug.LogError("Sprite Rect is out of bounds of the texture");
            return;
        }

        Color[] pixels = texture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);

        int minX = (int)spriteRect.width;
        int maxX = 0;
        int minY = (int)spriteRect.height;
        int maxY = 0;

        for (int y = 0; y < spriteRect.height; y++)
        {
            for (int x = 0; x < spriteRect.width; x++)
            {
                int index = y * (int)spriteRect.width + x;

                // Check index bounds
                if (index < 0 || index >= pixels.Length)
                {
                    Debug.LogError("Index out of bounds: " + index);
                    continue;
                }

                Color pixel = pixels[index];
                if (pixel.a > 0) // ·ÇÍ¸Ã÷ÏñËØ
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        float width = maxX - minX + 1;
        float height = maxY - minY + 1;

        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.anchoredPosition = new Vector2(minX - (spriteRect.width / 2) + (width / 2), minY - (spriteRect.height / 2) + (height / 2));
    }
}
