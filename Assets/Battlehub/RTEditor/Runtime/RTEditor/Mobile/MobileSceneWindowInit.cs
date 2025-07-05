using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Controls;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileSceneWindowInit : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_mobileLayerPrefab = null;

        [SerializeField]
        private bool m_hideSceneSettings = true;

        public override string WindowTypeName
        {
            get { return BuiltInWindowNames.Scene; }
        }

        protected override void Extend(RuntimeWindow window)
        {
            RectTransform mobileLayer = Instantiate(m_mobileLayerPrefab, window.ViewRoot, false);
            mobileLayer.name = "Mobile Layer";

            if (m_hideSceneSettings)
            {
                ISceneSettingsComponent sceneSettingsComponent = window.IOCContainer.Resolve<ISceneSettingsComponent>();
                if (sceneSettingsComponent != null)
                {
                    sceneSettingsComponent.IsUserDefined = false;
                }
            }

            IRTE editor = IOC.Resolve<IRTE>();
            IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();
            if (sceneComponent != null)
            {
                if (editor.TouchInput.IsTouchSupported)
                {
                    MobileContextPanelPositionUpdater positionUpdater = mobileLayer.GetComponentInChildren<MobileContextPanelPositionUpdater>(true);
                    positionUpdater.MarginBottom = 100;

                    if (sceneComponent.GameObject.GetComponent<RuntimeSelectionInputBase>() == null)
                    {
                        sceneComponent.GameObject.AddComponent<MobileSceneInput>();
                    }

                    sceneComponent.IsBoxSelectionEnabled = false;
                }

                editor.Tools.LockAxes = new LockObject { RotationFree = true };
            }


            PositionHandleModel positionHandleModel = sceneComponent.PositionHandle.Model as PositionHandleModel;
            if (positionHandleModel != null)
            {
                positionHandleModel.QuadLength = 0.33f;
            }

            IRuntimeHandlesComponent runtimeHandlesComponent = IOC.Resolve<IRuntimeHandlesComponent>();
            if (runtimeHandlesComponent != null)
            {
                runtimeHandlesComponent.SelectionMargin = 2;
            }
        }
    }
}