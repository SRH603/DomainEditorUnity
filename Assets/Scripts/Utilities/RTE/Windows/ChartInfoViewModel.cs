using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;

namespace ChartInfo.ViewModels
{
    /// <summary>
    /// ViewModel for ChartInfoView 窗口。
    /// 下属两个内嵌类，分别对应 InspectorView 的两个实例：
    /// - InfoEditor  ：序列化 gameData.info 的所有字段（除 condition）。
    /// - ContentEditor：序列化 gameData.content，包括 bpmList 数组。
    /// </summary>
    [Binding]
    public class ChartInfoViewModel : ViewModel
    {
        protected override void Start()
        {
            base.Start();
        }
        private GameData _gameData => ChartManager.Instance.gameData;

        // 供 InspectorView 绑定 Info
        public Info Info => _gameData.info;

        // 供 InspectorView 绑定 Content（包含 bpmList）
        public Content Content => _gameData.content;

        // 嵌套类型，用于在 Prefab InspectorView 上设置 ViewModel Type Name
        public class InfoEditor { }
        public class ContentEditor { }

        // 在 bpmList 改动时触发
        public void OnBpmListChanged()
        {
            // 通知 Grid 大切换刷新
            ChartManager.Instance.NotifyBpmListChanged();
        }
    }
}


