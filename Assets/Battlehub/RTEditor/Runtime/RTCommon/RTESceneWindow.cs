using UnityEngine;

namespace Battlehub.RTCommon
{
    public class RTESceneWindow : RuntimeCameraWindow
    {
        public override Camera Camera
        {
            get { return m_camera; }
            set
            {
                if (m_camera == value)
                {
                    return;
                }

                if (m_camera != null)
                {
                    UnregisterGraphicsCamera();
                }

                m_camera = value;

                if (m_camera != null)
                {
                    RegisterGraphicsCamera();
                }
            }
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Scene;
            IsPointerOver = true;

            if (m_camera != null)
            {
                RegisterGraphicsCamera();
            }

            Editor.RegisterWindow(this);
            if (m_pointer == null)
            {
                m_pointer = gameObject.AddComponent<Pointer>();
            }

            if (RenderPipelineInfo.Type == RPType.Standard)
            {
                if (gameObject.GetComponent<RTEGraphicsLayer>() == null)
                {
                    gameObject.AddComponent<RTEGraphicsLayer>();
                }
            }
        }

        public override bool IsPointerOver
        {
            get { return true; }
            set { }
        }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnDestroyOverride()
        {
            UnregisterGraphicsCamera();
            Editor.UnregisterWindow(this);
        }

        protected override void UpdateOverride()
        {

        }
    }
}

