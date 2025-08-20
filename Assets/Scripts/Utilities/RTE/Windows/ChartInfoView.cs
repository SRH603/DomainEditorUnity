// Assets/Scripts/Utilities/RTE/Windows/ChartInfoView.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using Battlehub.RTEditor.Views;

namespace ChartInfo.Views
{
    public class ChartInfoView : View
    {
        [Header("BPM List 面板容器")]
        [SerializeField] private RectTransform bpmHost;

        [Header("Info 面板容器")]
        [SerializeField] private RectTransform infoHost;

        [Header("是否在 Awake 时移除 MVVM 绑定脚本以避免 NRE")]
        [SerializeField] private bool stripMvvmBindings = true;

        [Header("可选：由本脚本接管上下布局")]
        [SerializeField] private bool controlLayout = false;

        [Header("标题（可选）")]
        [SerializeField] private string bpmTitle  = "BPM List";
        [SerializeField] private string infoTitle = "Info";

        // Info 面板用于测量高度
        [SerializeField] private float minInfoHeight = 100f;

        private ChartInfoBpmListPanel _bpmPanel;
        private ChartInfoInfoPanel    _infoPanel;

        protected override void Awake()
        {
            base.Awake();

            if (stripMvvmBindings) StripBindingsInSubtree(gameObject);

            if (bpmHost == null || infoHost == null)
            {
                Debug.LogError("[ChartInfo] 请在 Inspector 上指定 bpmHost / infoHost。");
                enabled = false;
                return;
            }

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
        }

        private void Update()
        {
            // 只有当你“明确选择让本脚本接管上下布局”时才会去改 RectTransform
            if (controlLayout)
            {
                UpdateTopBottomLayout();
            }
            // 否则什么都不做——完全尊重你在外部设置的左右/网格等布局。
        }

        /// <summary>
        /// 可选：脚本接管“上=BPM，下=Info”的布局（只有 controlLayout==true 时才会生效）
        /// </summary>
        private void UpdateTopBottomLayout()
        {
            if (bpmHost == null || infoHost == null || _infoPanel == null) return;

            var viewRT = (RectTransform)transform;
            float viewH = Mathf.Max(0f, viewRT.rect.height);

            // Info 高度 = 测量值
            float infoH = Mathf.Clamp(minInfoHeight, minInfoHeight, viewH);

            // Info 贴底
            infoHost.anchorMin = new Vector2(0f, 0f);
            infoHost.anchorMax = new Vector2(1f, 0f);
            infoHost.pivot     = new Vector2(0.5f, 0f);
            infoHost.sizeDelta = new Vector2(0f, infoH);
            infoHost.anchoredPosition = Vector2.zero;

            // BPM 贴顶，占剩余高度
            float bpmH = Mathf.Max(0f, viewH - infoH);
            bpmHost.anchorMin = new Vector2(0f, 1f);
            bpmHost.anchorMax = new Vector2(1f, 1f);
            bpmHost.pivot     = new Vector2(0.5f, 1f);
            bpmHost.sizeDelta = new Vector2(0f, bpmH);
            bpmHost.anchoredPosition = Vector2.zero;
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