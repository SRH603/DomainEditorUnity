using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.UIControls.Common
{
    public class HideOnRegionResizing : MonoBehaviour
    {
        private DockPanel m_dockPanel;
        private Region m_region;

        private void Start()
        {
            m_dockPanel = GetComponentInParent<DockPanel>();
            m_region = GetComponentInParent<Region>();
            if (m_dockPanel != null)
            {
                m_dockPanel.RegionBeginResize += OnBeginResize;
                m_dockPanel.RegionEndResize += OnEndResize;
            }
        }

        private void OnDestroy()
        {
            if (m_dockPanel != null)
            {
                m_dockPanel.RegionBeginResize -= OnBeginResize;
                m_dockPanel.RegionEndResize -= OnEndResize;
            } 
        }

        private void OnBeginResize(Resizer resizer, Region region)
        {
            if (region == m_region)
            {
                gameObject.SetActive(false);
            }
            
        }

        private void OnEndResize(Resizer resizer, Region region)
        {
            if (region == m_region)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
