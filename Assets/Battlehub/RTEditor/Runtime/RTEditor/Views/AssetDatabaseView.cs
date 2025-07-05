using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Views
{
    public class AssetDatabaseView : HierarchicalDataView
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            DockPanel dockPanelsRoot = GetComponentInParent<DockPanel>();
            if (dockPanelsRoot != null)
            {
                dockPanelsRoot.CursorHelper = Editor.CursorHelper;
            }
        }

        protected virtual void Update()
        {
            ViewInput.HandleInput();
        }
    }
}
