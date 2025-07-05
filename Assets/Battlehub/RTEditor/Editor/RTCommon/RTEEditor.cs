#if UNITY_EDITOR
using Battlehub.RTCommon;
using UnityEditor;

namespace Battlehub.RTHandles
{
    [CustomEditor(typeof(RTE))]
    public class RTEEditor : Editor
    {
        private readonly string[] m_legacyProperties =
        {
            "m_isOpened",
            "m_useBuiltinUndo",
            "IsOpenedEvent",
            "IsClosedEvent",
            "m_cameraLayerSettings",
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, m_legacyProperties);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

