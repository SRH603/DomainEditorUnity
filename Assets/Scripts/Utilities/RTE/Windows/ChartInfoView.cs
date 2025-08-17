// Assets/Scripts/Utilities/RTE/Windows/ChartInfoView.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using Battlehub.RTEditor.Views;

namespace ChartInfo.Views
{
    public class ChartInfoView : View
    {
        [Header("BPM List 面板容器（可空，不填会自动创建在上方、占剩余空间）")]
        [SerializeField] private RectTransform bpmHost;

        [Header("Info 面板容器（可空，不填会自动创建在下方、固定高度）")]
        [SerializeField] private RectTransform infoHost;

        [Header("在 Awake 时移除 MVVM 绑定脚本以避免 NRE")]
        [SerializeField] private bool stripMvvmBindings = true;

        [Header("标题（可选）")]
        [SerializeField] private string bpmTitle  = "BPM List";
        [SerializeField] private string infoTitle = "Info";

        // 底部 Info 固定高度（可按需改）
        private const float kInfoHeight = 160f;

        private ChartInfoBpmListPanel _bpmPanel;
        private ChartInfoInfoPanel    _infoPanel;

        protected override void Awake()
        {
            base.Awake();

            if (stripMvvmBindings)
            {
                StripBindingsInSubtree(this.gameObject);
            }

            EnsureRootRect();

            // 先创建底部 Info（固定高度）
            if (infoHost == null)
                infoHost = CreateBottomHost("InfoHost", kInfoHeight);

            // 再创建上方 BPM（占剩余空间，并为底部腾出 kInfoHeight）
            if (bpmHost == null)
                bpmHost  = CreateFillHostWithBottomPadding("BpmHost", kInfoHeight);

            // 组装 BPM 面板（放上面）
            _bpmPanel = GetComponent<ChartInfoBpmListPanel>();
            if (_bpmPanel == null) _bpmPanel = gameObject.AddComponent<ChartInfoBpmListPanel>();
            _bpmPanel.host       = bpmHost;
            _bpmPanel.title      = bpmTitle;        // 固定显示 "BPM List"
            _bpmPanel.arrowOnly  = true;            // 只保留上下箭头图标
            _bpmPanel.rowHeight  = 22f;             // 紧凑行高
            _bpmPanel.rowPadV    = 2;               // 上下留白更小

            // 组装 Info 面板（放下面）
            _infoPanel = GetComponent<ChartInfoInfoPanel>();
            if (_infoPanel == null) _infoPanel = gameObject.AddComponent<ChartInfoInfoPanel>();
            _infoPanel.host  = infoHost;
            _infoPanel.title = infoTitle;

            Debug.Log("[ChartInfo] 布局：上=BPM List（紧凑），下=Info（固定 160），两者零间隙。");
        }

        private void EnsureRootRect()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private RectTransform CreateBottomHost(string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(this.transform, false);

            // 底部固定高度（零间隙）
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;

            return rt;
        }

        private RectTransform CreateFillHostWithBottomPadding(string name, float bottomPadding)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(this.transform, false);

            // 占满剩余空间，并为底部 Info 腾出 bottomPadding；中间不引入额外缝隙
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(0, bottomPadding); // 仅底部内边距
            rt.offsetMax = new Vector2(0, 0);             // 顶部紧贴

            return rt;
        }

        // ========= 工具：移除绑定脚本，避免 RTE 的 MVVM 对接报错 =========
        private static void StripBindingsInSubtree(GameObject root)
        {
            TryRemoveAll(root, "Battlehub.RTEditor.Binding.ViewBinding");
            TryRemoveAll(root, "UnityWeld.Binding.AbstractMemberBinding");
        }

        private static void TryRemoveAll(GameObject root, string fullTypeName)
        {
            var toRemove = new List<Component>();
            CollectComponentsByFullName(root.transform, fullTypeName, toRemove);

            foreach (var c in toRemove)
                if (c != null) GameObject.Destroy(c);

            if (toRemove.Count > 0)
                Debug.Log($"[ChartInfo] 移除 {fullTypeName} 及其派生类 {toRemove.Count} 个。");
        }

        private static void CollectComponentsByFullName(Transform t, string fullTypeName, List<Component> outList)
        {
            var comps = t.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var type = c.GetType();
                if (TypeOrBaseMatches(type, fullTypeName))
                    outList.Add(c);
            }
            for (int i = 0; i < t.childCount; i++)
                CollectComponentsByFullName(t.GetChild(i), fullTypeName, outList);
        }

        private static bool TypeOrBaseMatches(Type type, string fullName)
        {
            while (type != null)
            {
                if (type.FullName == fullName) return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
