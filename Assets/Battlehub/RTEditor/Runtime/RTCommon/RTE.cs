using UnityEngine;

namespace Battlehub.RTCommon
{
    public class RTE : RTEBase
    {
        [SerializeField]
        private Camera m_camera;

        protected override void Awake()
        {
            base.Awake();

            if (m_camera == null)
            {
                m_camera = Camera.main;
            }

            if (m_camera == null)
            {
                GameObject camera = new GameObject();
                camera.transform.SetParent(transform);
                camera.name = "RTECamera";
                m_camera = camera.AddComponent<Camera>();
            }

            WindowRegistered += OnWindowRegistered;
        }

        private void OnWindowRegistered(RuntimeWindow window)
        {
            if (window.WindowType == RuntimeWindowType.Scene)
            {
                if (window != null && m_camera != null)
                {
                    window.Camera = m_camera;
                }
            }
        }
    }
}

