using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace ChartInfo
{
    [MenuDefinition]
    public class RegisterChartInfoWindow : EditorExtension
    {
        [SerializeField]
        private GameObject m_prefab = null;

        [SerializeField]
        private Sprite m_icon = null;
		
        [SerializeField]
        private string m_header = "ChartInfo";

        [SerializeField]
        private bool m_isDialog = false;
		
        [SerializeField]
        private int m_maxWindows = -1;

        protected override void OnInit()
        {
            base.OnInit();

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.RegisterWindow("ChartInfo", m_header, m_icon, m_prefab, m_isDialog, m_maxWindows);
        }

        [MenuCommand("MenuWindow/ChartInfo", "")]
        public void Open()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("ChartInfo");
        }
    }
}



