using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaMaskOverlay : MonoBehaviour
{
    public Image maskImage; // 图像 A：遮罩图
    private Material materialInstance;

    void Start()
    {
        var image = GetComponent<Image>();

        // 创建材质实例，防止影响其他使用该 Shader 的 Image
        materialInstance = new Material(Shader.Find("UI/ImageWithAlphaMask"));

        materialInstance.SetTexture("_MainTex", image.sprite.texture);
        if (maskImage != null && maskImage.sprite != null)
        {
            materialInstance.SetTexture("_MaskTex", maskImage.sprite.texture);
        }

        image.material = materialInstance;
    }
    
    public void Apply()
    {
        var image = GetComponent<Image>();
        if (image.sprite == null) return;

        if (materialInstance != null)
            DestroyImmediate(materialInstance);

        materialInstance = new Material(Shader.Find("UI/ImageWithAlphaMask"));

        materialInstance.SetTexture("_MainTex", image.sprite.texture);

        if (maskImage != null && maskImage.sprite != null)
            materialInstance.SetTexture("_MaskTex", maskImage.sprite.texture);

        image.material = materialInstance;
    }

    private void Update()
    {
        Apply();
    }

    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}