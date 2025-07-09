using Battlehub.RTEditor;
using UnityEngine;

public class ToolsPanelOverride : LayoutExtension
{
    [SerializeField]
    private Transform m_toolsPrefab = null;

    protected override void OnBeforeBuildLayout(IWindowManager wm)
    {
        wm.OverrideTools(m_toolsPrefab);
    }
}