using Battlehub.RTCommon;
using Battlehub.UIControls;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileProjectWindowInit : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_treeViewItemPrefab = null;

        [SerializeField]
        private RectTransform m_listBoxItemPrefab = null;

        [SerializeField]
        private RectTransform m_treeViewItemPrefab_Legacy = null;

        [SerializeField]
        private RectTransform m_listBoxItemPrefab_Legacy = null;

        public override string WindowTypeName
        {
            get { return BuiltInWindowNames.Project; }
        }

        protected override void Extend(RuntimeWindow window)
        {
            RuntimeWindow[] windows = window.GetComponentsInChildren<RuntimeWindow>();

            var editor = IOC.Resolve<IRuntimeEditor>();

            var treeViewItemPrefab = editor.CompatibilityMode == CompatibilityMode.LegacyRTSL ?
                m_treeViewItemPrefab_Legacy :
                m_treeViewItemPrefab;

            var listBoxItemPrefab = editor.CompatibilityMode == CompatibilityMode.LegacyRTSL ?
                m_listBoxItemPrefab_Legacy :
                m_listBoxItemPrefab;

            EnableStyling(treeViewItemPrefab.gameObject);
            EnableStyling(listBoxItemPrefab.gameObject);

            VirtualizingScrollRect scrollRect = windows
                .Where(w => w.WindowType == RuntimeWindowType.ProjectTree)
                .First()
                .GetComponentInChildren<VirtualizingScrollRect>(true);

            scrollRect.ContainerPrefab = treeViewItemPrefab;

            scrollRect = windows
                .Where(w => w.WindowType == RuntimeWindowType.ProjectFolder)
                .First()
                .GetComponentInChildren<VirtualizingScrollRect>(true);

            scrollRect.ContainerPrefab = listBoxItemPrefab;
        }
    }
}
