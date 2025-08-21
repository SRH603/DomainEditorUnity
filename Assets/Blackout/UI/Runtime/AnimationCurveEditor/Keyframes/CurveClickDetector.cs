using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Monitors mouse clicks around the curve to add keyframes and open popup menus
    /// </summary>
    [RequireComponent(typeof(MaskableGraphic))]
    public class CurveClickDetector : MonoBehaviour, IPointerClickHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        private GameObject quickActionMenu;
        
        #endregion
        
        #region Private Fields
        
        private RectTransform _rectTransform;
        private Transform _content;
        
        private AnimationCurveEditor _editor;
        private float _lastClickTime;

        #endregion
        
        #region Properties
        
        public GameObject Menu
        {
            get => quickActionMenu;
            set => quickActionMenu = value;
        }
        
        #endregion
        
        private void Start()
        {
            _rectTransform = (RectTransform)transform;
            
            _content = _rectTransform.parent;
            
            _editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
{
    _editor.SelectKeyframe(null);

    // 统一解析事件相机（Overlay 下可为 null；Camera/World 下必须有）
    var cam = eventData.pressEventCamera 
              ?? eventData.enterEventCamera 
              ?? (GetComponentInParent<Canvas>()?.worldCamera);

    // 右键：弹出菜单（这段你原来就写对了，只是补了 cam 兜底）
    if (eventData.button == PointerEventData.InputButton.Right)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, cam, out var localMenu))
        {
            quickActionMenu.transform.localPosition = localMenu;
            quickActionMenu.SetActive(true);
        }
        return;
    }

    if (eventData.button != PointerEventData.InputButton.Left)
        return;

    // 推荐用系统计数来判双击，更稳
    bool isDoubleClick = eventData.clickCount >= 2;
    if (!isDoubleClick)
    {
        _lastClickTime = Time.time; // 为了兼容你原逻辑，保留这行也无妨
        return;
    }

    // 1) 屏幕点 -> 当前RectTransform的本地坐标（相对 pivot 的像素）
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, cam, out var local))
        return;

    // 2) 转成 [0,1]^2 的归一化坐标（以 rect + pivot 计算）
    Rect rect = _rectTransform.rect;
    Vector2 norm = (local + rect.size * _rectTransform.pivot) / rect.size;

    // 3) 归一化 -> 实际曲线可视范围（你已有工具函数）
    Vector2 keyframeValues = _editor.ConvertNormalizedToCurveRange(norm);

    // 4) 时间位置是否合法
    if (!_editor.CanInsertAtTime(keyframeValues.x))
        return;

    // 5) 计算“点到曲线”的允许误差厚度（随缩放自适应）
    //    注意：真正缩放的是 ScrollRect.Grid（非 _rectTransform.parent）
    float gridScaleY = _editor.ScrollRect.Grid.localScale.y;
    float thickness = (_editor.Settings.curveClickThickness / gridScaleY) / rect.size.y;

    // 6) 用曲线在该时间的值，判断是否“点在曲线上”
    float curveValue = _editor.Curve.Evaluate(keyframeValues.x);
    if (Mathf.Abs(curveValue - keyframeValues.y) <= thickness)
    {
        _editor.InsertKeyframe(keyframeValues.x, curveValue);
        _lastClickTime = 0f;
        return;
    }
}

    }
}