using Blackout.UI;
using UnityEditor;

namespace BlackoutEditor.UI
{
    [CustomEditor(typeof(AnimationCurveButton), true)]
    [CanEditMultipleObjects]
    public class AnimationCurveButtonEditor : Editor
    {
        private SerializedProperty _interactableProperty;
        private SerializedProperty _curveProperty;
        private SerializedProperty _curveRendererProperty;
        private SerializedProperty _updateModeProperty;
        private SerializedProperty _editorProperty;
        private SerializedProperty _onEditProperty;
        private SerializedProperty _onEndEditProperty;

        protected void OnEnable()
        {
            _interactableProperty   = serializedObject.FindProperty("m_Interactable");
            _curveProperty          = serializedObject.FindProperty("curve");
            _curveRendererProperty  = serializedObject.FindProperty("curveRenderer");
            _updateModeProperty     = serializedObject.FindProperty("updateMode");
            _editorProperty         = serializedObject.FindProperty("editor");
            _onEditProperty         = serializedObject.FindProperty("onCurveChange");
            _onEndEditProperty      = serializedObject.FindProperty("onFinishedEditing");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_interactableProperty);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_curveProperty);
            EditorGUILayout.PropertyField(_curveRendererProperty);
            EditorGUILayout.PropertyField(_updateModeProperty);
            EditorGUILayout.PropertyField(_editorProperty);
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onEditProperty);
            EditorGUILayout.PropertyField(_onEndEditProperty);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
