using Battlehub.RTCommon;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(GameObject), typeof(Color))]
    public class GameObjectToHierarchyTextColorAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var go = (GameObject)valueIn;
            return ToColor(go);
        }

        private Color ToColor(GameObject go)
        {
            var editor = IOC.Resolve<IRuntimeEditor>();
            var settings = IOC.Resolve<ISettingsComponent>();
            var theme = settings.SelectedTheme;
            if (editor.CompatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                return go.activeInHierarchy ?
                    theme.Colors.Hierarchy.NormalItem :
                    theme.Colors.Hierarchy.DisabledItem;
            }

            bool isInstance = editor.IsInstance(go);
            if (isInstance)
            {
                return go.activeInHierarchy ?
                    theme.Colors.Hierarchy.InstanceItem :
                    theme.Colors.Hierarchy.DisabledInstanceItem;
            }

            return go.activeInHierarchy ?
                 theme.Colors.Hierarchy.NormalItem :
                 theme.Colors.Hierarchy.DisabledItem;
        }
    }
}
