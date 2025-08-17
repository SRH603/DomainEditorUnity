// Assets/Scripts/Utilities/RTE/Windows/ChartInfoView.cs
using UnityEngine;
using Battlehub.RTEditor; // 仅为继承 RuntimeWindow
using System;
using System.Reflection;
using Battlehub.RTEditor.Views;

namespace ChartInfo.Views
{
    /// <summary>
    /// 只使用自绘 BPM 面板；在本窗口层级内移除 UnityWeld 绑定，避免 ViewBinding.Connect NRE。
    /// </summary>
    /// 
    [DefaultExecutionOrder(-10000)] // 提前于同物体上的其它组件执行，先进行“去绑定”
    public class ChartInfoView : View
    {
        [Header("把自绘 BPM 面板渲染到此区域（RectTransform，锚定四角）")]
        [SerializeField] private RectTransform host;

        [Header("可选：直接指定 GameData；若为空可勾选下方自动获取")]
        [SerializeField] private GameData gameDataOverride;

        [Header("若未手动指定 GameData，则尝试 ChartManager.Instance.gameData")]
        [SerializeField] private bool autoGetFromChartManager = true;

        [Header("备用：若未绑定 Host，则使用该屏幕矩形绘制")]
        [SerializeField] private Rect fallbackScreenRect = new Rect(24, 24, 560, 520);

        private ChartInfoBpmListPanel m_panel;

        protected override void Awake()
        {
            base.Awake();

            // 1) 先“去绑定”：只在当前窗口 GameObject 子树中移除 UnityWeld / ViewBinding 组件
            StripBindingsInSubtree(this.gameObject);

            // 2) 确保自绘面板存在并配置
            m_panel = GetComponent<ChartInfoBpmListPanel>() ?? gameObject.AddComponent<ChartInfoBpmListPanel>();
            m_panel.host = host;
            m_panel.fallbackScreenRect = fallbackScreenRect;

            if (gameDataOverride != null)
            {
                m_panel.targetGameData = gameDataOverride;
                m_panel.autoGetFromChartManager = false;
            }
            else
            {
                m_panel.autoGetFromChartManager = autoGetFromChartManager;
            }

            Debug.Log("[ChartInfo] 自绘 BPM 面板就绪（已移除本窗口中的 UnityWeld 绑定组件）。");
        }

        /// <summary>运行时切换 GameData（例如新建/打开后调用）。</summary>
        public void InjectGameData(GameData so)
        {
            if (m_panel != null)
            {
                m_panel.targetGameData = so;
                m_panel.autoGetFromChartManager = false;
            }
        }

        /// <summary>
        /// 仅在当前窗口子树内移除可能导致 NRE 的绑定组件：
        /// - Battlehub.RTEditor.Binding.ViewBinding 及其子类
        /// - UnityWeld.Binding.BindingContext 及其子类
        /// - UnityWeld.Binding.AbstractMemberBinding 及其子类（包含所有具体绑定）
        /// </summary>
        private void StripBindingsInSubtree(GameObject root)
        {
            TryRemoveAll(root, "Battlehub.RTEditor.Binding.ViewBinding");
            TryRemoveAll(root, "UnityWeld.Binding.BindingContext");
            TryRemoveAll(root, "UnityWeld.Binding.AbstractMemberBinding");
            Debug.Log("[ChartInfo] 从窗口层级移除了可能触发 NRE 的 MVVM 绑定脚本。");
        }

        private static void TryRemoveAll(GameObject root, string typeFullName)
        {
            var t = FindType(typeFullName);
            if (t == null) return;

            var comps = root.GetComponentsInChildren<Component>(true);
            int removed = 0;
            foreach (var c in comps)
            {
                if (c == null) continue;
                var ct = c.GetType();
                if (t.IsAssignableFrom(ct))
                {
                    // 仅干预本窗口层级
                    UnityEngine.Object.Destroy(c);
                    removed++;
                }
            }
            if (removed > 0)
            {
                Debug.Log($"[ChartInfo] 移除 {typeFullName} 及其派生类 {removed} 个。");
            }
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, throwOnError: false);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }
    }
}
