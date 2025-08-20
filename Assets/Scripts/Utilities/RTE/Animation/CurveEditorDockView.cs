using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using RuntimeCurveEditor; // RCE 的命名空间

namespace CurveEditorIntegration
{
    public class CurveEditorDockView : MonoBehaviour
    {
        [Header("RCE 入口 (RTAnimationCurve)")]
        public RTAnimationCurve rtAnimationCurve;

        [Header("承载区域（RTE窗口里的 Content）")]
        public RectTransform host;

        Component _editorRoot; // RCE 编辑器的根组件
        Canvas _spawnedCanvas; // 若自带 Canvas，则临时关掉

        void OnEnable()
        {
            StartCoroutine(Embed());
        }

        IEnumerator Embed()
        {
            if (rtAnimationCurve == null)
            {
#if UNITY_2022_1_OR_NEWER
                rtAnimationCurve = FindFirstObjectByType<RTAnimationCurve>(FindObjectsInactive.Include);
#else
                rtAnimationCurve = FindObjectOfType<RTAnimationCurve>();
#endif
            }

            // 尝试：有些版本可能有 ShowCurveEditor(RectTransform parent) 这样的重载
            var t = typeof(RTAnimationCurve);
            var mi = t.GetMethod("ShowCurveEditor", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(RectTransform) }, null);
            if (mi != null)
            {
                mi.Invoke(rtAnimationCurve, new object[] { host });
                yield break;
            }

            // 否则：先让它生成默认窗口，再把根 UI 拆壳挪到 host
            rtAnimationCurve.ShowCurveEditor();
            yield return null; // 等一帧，等待实例化

            _editorRoot = FindRceRoot();
            if (_editorRoot != null)
            {
                var rootRT = (_editorRoot as Component).GetComponent<RectTransform>();

                // 关掉它自带的 Canvas 壳，避免双 Canvas / Sorting / 输入冲突
                _spawnedCanvas = rootRT.GetComponentInParent<Canvas>();
                if (_spawnedCanvas != null) _spawnedCanvas.enabled = false;

                // 挂到 RTE 窗口的 Content 下并拉伸
                rootRT.SetParent(host, false);
                rootRT.anchorMin = Vector2.zero;
                rootRT.anchorMax = Vector2.one;
                rootRT.offsetMin = Vector2.zero;
                rootRT.offsetMax = Vector2.zero;
            }
        }

        Component FindRceRoot()
        {
            // 1) 常见类型名（不同版本/包名不尽相同，这里多尝试几个）
            var t1 = Type.GetType("RuntimeCurveEditor.CurveEditorCanvas, RuntimeCurveEditor");
            if (t1 != null)
            {
#if UNITY_2022_1_OR_NEWER
                var c = FindFirstObjectByType(t1, FindObjectsInactive.Include) as Component;
#else
                var c = FindObjectOfType(t1) as Component;
#endif
                if (c != null) return c;
            }

            var t2 = Type.GetType("RuntimeCurveEditor.CurveEditor, RuntimeCurveEditor");
            if (t2 != null)
            {
#if UNITY_2022_1_OR_NEWER
                var c = FindFirstObjectByType(t2, FindObjectsInactive.Include) as Component;
#else
                var c = FindObjectOfType(t2) as Component;
#endif
                if (c != null) return c;
            }

            // 2) 兜底：通过 RTAnimationCurve 的私有字段反射（字段名可能是 curveEditor）
            var fld = typeof(RTAnimationCurve).GetField("curveEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fld != null)
            {
                var v = fld.GetValue(rtAnimationCurve) as Component;
                if (v != null) return v;
            }

            return null;
        }

        void OnDisable()
        {
            // 关闭 RTE 窗口时做清理（销毁内嵌的 RCE UI）
            if (_editorRoot != null)
            {
                Destroy((_editorRoot as Component).gameObject);
                _editorRoot = null;
            }
        }
    }
}