using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(GameObject), typeof(Sprite), typeof(GameObjectToHierarchyIconAdapterOptions))]
    public class GameObjectToHierarchyIconAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var go = (GameObject)valueIn;
            var adapterOptions = (GameObjectToHierarchyIconAdapterOptions)options;
            return ToSprite(go, adapterOptions);
        }

        private Sprite ToSprite(GameObject go, GameObjectToHierarchyIconAdapterOptions options)
        {
            if (go == null)
            {
                return null;
            }

            var runtimeEditor = IOC.Resolve<IRuntimeEditor>();
            var settings = IOC.Resolve<ISettingsComponent>();
            var theme = settings.SelectedTheme;
            if (runtimeEditor.CompatibilityMode == CompatibilityMode.LegacyRTSL)
            {
                return options.MainIcon ?
                    theme.GetIcon("GameObject Icon") :
                    theme.GetIcon("None");
            }

            if (options.MainIcon)
            {
                bool isInstanceOfAssetVariant = runtimeEditor.IsInstanceOfAssetVariant(go) || runtimeEditor.IsInstanceOfAssetVariantRef(go);
                if (isInstanceOfAssetVariant)
                {
                    return theme.GetIcon("PrefabVariant Icon");
                }

                bool isInstanceRoot = runtimeEditor.IsInstanceRoot(go) || runtimeEditor.IsInstanceRootRef(go);
                if (isInstanceRoot)
                {
                    return theme.GetIcon("Prefab Icon");
                }


                return theme.GetIcon("GameObject Icon");
            }

            if (runtimeEditor.IsAddedObject(go))
            {
                return theme.GetIcon("PrefabOverlayAdded Icon");
            }

            if (runtimeEditor.HasChanges(go, runtimeEditor.CurrentPrefab))
            {
                return theme.GetIcon("PrefabOverlayModified Icon");
            }

            return theme.GetIcon("None");
        }
    }
}
