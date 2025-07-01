using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaMaskOverlay : MonoBehaviour
{
    /* ───── 保留原有公有字段 ───── */
    public Image  maskImage;            // 遮罩图：沿用旧名字

    /* ───── 新增公有字段 ───── */
    [Header("可选：Inspector 指定专用 Shader（优先级最高）")]
    public Shader maskShaderOverride;   // ← 只有你手动赋值时才会使用

    /* ───── 私有成员 ───── */
    private const string SHADER_PATH = "Shaders/UI_ImageWithAlphaMask";
    private const string SHADER_NAME = "UI/ImageWithAlphaMask";

    private Shader   _maskShader;       // 真正被实例化的 Shader
    private Material _mat;
    private Image    _img;
    private Sprite   _lastSprite;       // 缓存上一次 sprite

    /* ─────────── 生命周期 ─────────── */

    private void Awake()
    {
        _img = GetComponent<Image>();

        /* ① 先看 Inspector 是否手动拖了 Shader */
        if (maskShaderOverride != null)
        {
            _maskShader = maskShaderOverride;
        }
        else
        {
            /* ② 再尝试 Resources.Load（打包不会被裁剪） */
            _maskShader = Resources.Load<Shader>(SHADER_PATH);

            /* ③ 最后兜底 Shader.Find（编辑器 / DevBuild 仍可用） */
            if (_maskShader == null)
                _maskShader = Shader.Find(SHADER_NAME);
        }

        if (_maskShader == null)
        {
            Debug.LogError(
                $"[AlphaMaskOverlay] 无法找到 Shader '{SHADER_NAME}'，" +
                $"请检查 Inspector、Resources 路径或 Graphics Settings。");
            enabled = false;
            return;
        }

        CreateMaterial();
    }

    private void OnDestroy()
    {
        if (_mat != null) Destroy(_mat);
    }

    /* ─────────── 更新逻辑 ─────────── */

    private void LateUpdate()
    {
        if (_img.sprite != _lastSprite)
            UpdateMaterialTextures();
    }

    /* ─────────── 保留的公有 API ─────────── */

    public void Apply()
    {
        if (!enabled) return;
        UpdateMaterialTextures();
    }

    /* ─────────── 内部工具函数 ─────────── */

    void CreateMaterial()
    {
        _mat = new Material(_maskShader) { hideFlags = HideFlags.DontSave };
        UpdateMaterialTextures();
        _img.material = _mat;
    }

    void UpdateMaterialTextures()
    {
        if (_img.sprite == null) return;

        // 主纹理
        _mat.SetTexture("_MainTex", _img.sprite.texture);

        // 遮罩纹理：优先使用 maskImage 指定的 sprite
        var maskSprite = (maskImage != null && maskImage.sprite != null)
                       ? maskImage.sprite
                       : _img.sprite;

        _mat.SetTexture("_MaskTex", maskSprite.texture);

        _lastSprite = _img.sprite;
    }
}
