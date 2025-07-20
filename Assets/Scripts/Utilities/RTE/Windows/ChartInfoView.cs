using UnityEngine;
using Battlehub.RTCommon;       // IOC.Resolve<T>(), IRuntimeSelection
using Battlehub.RTEditor;       // RuntimeWindow, IWindowManager, BuiltInWindowNames

namespace ChartInfo.Views
{
    /// <summary>
    /// ChartInfo 窗口：打开时弹出 Runtime Editor 自带的 Inspector，
    /// 选中 GameData 并让它可编辑；当 bpmList 改动时再次选中以刷新 Inspector。
    /// </summary>
    public class ChartInfoView : RuntimeWindow
    {
        private IWindowManager    m_windowManager;
        private IRuntimeSelection m_selection;
        private GameData          m_gameData;
        private System.Action     m_onBpmListChanged;

        protected override void Awake()
        {
            base.Awake();

            // 1) 获取全局单例和 SO
            var cm = ChartManager.Instance;
            if (cm == null)
            {
                Debug.LogError("ChartManager.Instance 为空！");
                return;
            }

            m_gameData = cm.gameData;
            if (m_gameData == null)
            {
                Debug.LogError("ChartManager.Instance.gameData 为空！");
            }

            // 2) 解析窗口管理器和选择服务
            m_windowManager = IOC.Resolve<IWindowManager>();
            if (m_windowManager == null)
            {
                Debug.LogError("无法解析 IWindowManager，请检查 Battlehub.RTEditor.dll 是否正确导入。");
            }

            m_selection = IOC.Resolve<IRuntimeSelection>();
            if (m_selection == null)
            {
                Debug.LogError("无法解析 IRuntimeSelection，请检查 Battlehub.RTCommon.dll 是否正确导入。");
            }

            // 3) 弹出并激活 Inspector 窗口
            m_windowManager.CreateWindow(BuiltInWindowNames.Inspector);

            // 4) 选中 GameData，让 Inspector 展示并可编辑它（包括 bpmList 数组）
            m_selection.Select(m_gameData, new Object[] { m_gameData });

            // 5) 监听 bpmList 改动，变动时再次 Select 以刷新 Inspector
            m_onBpmListChanged = () =>
            {
                m_selection.Select(m_gameData, new Object[] { m_gameData });
            };
            cm.OnBpmListChanged += m_onBpmListChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            var cm = ChartManager.Instance;
            if (cm != null)
            {
                cm.OnBpmListChanged -= m_onBpmListChanged;
            }
        }
    }
}