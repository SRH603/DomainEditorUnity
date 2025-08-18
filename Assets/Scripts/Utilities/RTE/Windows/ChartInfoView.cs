using UnityEngine;
using System;
using System.Collections.Generic;
using Battlehub.RTEditor.Views;

namespace ChartInfo.Views
{
    public class ChartInfoView : View
    {
        [Header("BPM List 面板容器（可空：自动创建并放在上方）")]
        [SerializeField] private RectTransform bpmHost;

        [Header("Info 面板容器（可空：自动创建并放在下方）")]
        [SerializeField] private RectTransform infoHost;

        [Header("在 Awake 时移除 MVVM 绑定脚本以避免 NRE")]
        [SerializeField] private bool stripMvvmBindings = true;

        [Header("标题（可选）")]
        [SerializeField] private string bpmTitle  = "BPM List";
        [SerializeField] private string infoTitle = "Info";

        [Header("Info 面板固定高度")]
        [SerializeField] private float infoFixedHeight = 160f;

        private ChartInfoBpmListPanel _bpmPanel;
        private ChartInfoInfoPanel    _infoPanel;

        protected override void Awake()
        {
            base.Awake();

            if (stripMvvmBindings) StripBindingsInSubtree(gameObject);
            EnsureRootRect();

            if (bpmHost == null) bpmHost = CreateTopHost("BpmHost");
            if (infoHost == null) infoHost = CreateBottomHost("InfoHost", infoFixedHeight);

            // 组装 BPM
            _bpmPanel = GetComponent<ChartInfoBpmListPanel>();
            if (_bpmPanel == null) _bpmPanel = gameObject.AddComponent<ChartInfoBpmListPanel>();
            _bpmPanel.host      = bpmHost;
            _bpmPanel.title     = bpmTitle;
            _bpmPanel.arrowOnly = true;
            _bpmPanel.rowHeight = 22f;
            _bpmPanel.rowPadV   = 2;

            // 组装 Info
            _infoPanel = GetComponent<ChartInfoInfoPanel>();
            if (_infoPanel == null) _infoPanel = gameObject.AddComponent<ChartInfoInfoPanel>();
            _infoPanel.host  = infoHost;
            _infoPanel.title = infoTitle;

            UpdateLayoutInstant();
            Debug.Log("[ChartInfo] 布局就绪：BPM 上方自适应（自身滚动），Info 固定底部（零缝隙）。");
        }

        private void Update()
        {
            UpdateLayoutInstant();
        }

        private void UpdateLayoutInstant()
        {
            if (bpmHost == null || infoHost == null) return;

            var rt = GetComponent<RectTransform>();
            float viewH = Mathf.Max(0f, rt.rect.height);

            // info 固定贴底
            var isz = infoHost.sizeDelta; isz.y = infoFixedHeight; infoHost.sizeDelta = isz;
            infoHost.anchorMin = new Vector2(0f, 0f);
            infoHost.anchorMax = new Vector2(1f, 0f);
            infoHost.pivot     = new Vector2(0.5f, 0f);
            infoHost.anchoredPosition = Vector2.zero;

            // bpm 占据顶部到 info 顶部之间的全部高度
            float bpmH = Mathf.Max(0f, viewH - infoFixedHeight);
            var bsz = bpmHost.sizeDelta; bsz.y = bpmH; bpmHost.sizeDelta = bsz;
            bpmHost.anchorMin = new Vector2(0f, 1f);
            bpmHost.anchorMax = new Vector2(1f, 1f);
            bpmHost.pivot     = new Vector2(0.5f, 1f);
            bpmHost.anchoredPosition = Vector2.zero;
        }

        private void EnsureRootRect()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private RectTransform CreateTopHost(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 100); // 初值，UpdateLayoutInstant 会覆盖
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        private RectTransform CreateBottomHost(string name, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        /* ========= 移除 MVVM 绑定：防 NRE ========= */
        private static void StripBindingsInSubtree(GameObject root)
        {
            TryRemoveAll(root, "Battlehub.RTEditor.Binding.ViewBinding");
            TryRemoveAll(root, "UnityWeld.Binding.AbstractMemberBinding");
        }
        private static void TryRemoveAll(GameObject root, string fullTypeName)
        {
            var toRemove = new List<Component>();
            CollectByFullName(root.transform, fullTypeName, toRemove);
            foreach (var c in toRemove) if (c != null) UnityEngine.Object.Destroy(c);
            if (toRemove.Count > 0)
                Debug.Log($"[ChartInfo] 移除 {fullTypeName} 及其派生类 {toRemove.Count} 个。");
        }
        private static void CollectByFullName(Transform t, string fullTypeName, List<Component> outList)
        {
            var comps = t.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var type = c.GetType();
                while (type != null)
                {
                    if (type.FullName == fullTypeName) { outList.Add(c); break; }
                    type = type.BaseType;
                }
            }
            for (int i = 0; i < t.childCount; i++)
                CollectByFullName(t.GetChild(i), fullTypeName, outList);
        }
    }
}