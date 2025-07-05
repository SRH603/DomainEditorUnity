using UnityEngine;
using Battlehub.Utils;

namespace Battlehub.RTGizmos
{
    public class PointLightGizmo : SphereGizmo
    {
        [SerializeField]
        private Light m_light;
        public override object TargetComponent => m_light;

        protected override Vector3 Center
        {
            get { return Vector3.zero; }
            set {                      }
        }

        protected override float Radius
        {
            get
            {
                if (m_light == null)
                {
                    return 0;
                }

                return m_light.range;
            }
            set
            {
                if (m_light != null)
                {
                    m_light.range = value;
                }
            }
        }


        protected override void Awake()
        {
            if (m_light == null)
            {
                m_light = GetComponent<Light>();
            }

            if (m_light == null)
            {
                Debug.LogError("Set Light");
            }

            if(m_light.type != LightType.Point)
            {
                Debug.LogWarning("m_light.Type != LightType.Point");
            }

            base.Awake();
        }


        protected override void BeginRecord()
        {
            base.BeginRecord();
            Window.Editor.Undo.BeginRecordValue(m_light, Strong.PropertyInfo((Light x) => x.range, "range"));
        }

        protected override void EndRecord()
        {
            base.EndRecord();
            Window.Editor.Undo.EndRecordValue(m_light, Strong.PropertyInfo((Light x) => x.range, "range"));
        }

        public override void Reset()
        {
            base.Reset();
            LineColor = new Color(1, 1, 0.5f, 0.5f);
            HandlesColor = new Color(1, 1, 0.35f, 0.95f);
            SelectionColor = new Color(1.0f, 1.0f, 0, 1.0f);
        }
    }
}

