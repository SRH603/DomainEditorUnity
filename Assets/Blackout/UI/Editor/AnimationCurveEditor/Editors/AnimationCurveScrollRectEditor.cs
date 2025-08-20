using Blackout.UI;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEngine.UI;

namespace BlackoutEditor.UI
{
    [CustomEditor(typeof(CurveScrollRect), true)]
    [CanEditMultipleObjects]
    public class AnimationCurveScrollRectEditor : Editor
    {
        private SerializedProperty _content;
        private SerializedProperty _grid;
        private SerializedProperty _inertia;
        private SerializedProperty _decelerationRate;
        private SerializedProperty _zoomSensitivity;
        private SerializedProperty _minimumZoom;
        private SerializedProperty _maximumZoom;
        private SerializedProperty _viewport;
        private SerializedProperty _horizontalScrollbar;
        private SerializedProperty _verticalScrollbar;
        private SerializedProperty _horizontalScrollbarSpacing;
        private SerializedProperty _verticalScrollbarSpacing;
        private SerializedProperty _onPositionChanged;
        private SerializedProperty _onScaleChanged;
        
        private AnimBool _showDecelerationRate;
        private bool _viewportIsNotChild, _hScrollbarIsNotChild, _vScrollbarIsNotChild;

        private const string HorizontalError = "For this visibility mode, the Viewport property and the Horizontal Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";
        private const string VerticalError = "For this visibility mode, the Viewport property and the Vertical Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";

        protected virtual void OnEnable()
        {
            _content                        = serializedObject.FindProperty("content");
            _grid                           = serializedObject.FindProperty("grid");
            _inertia                        = serializedObject.FindProperty("inertia");
            _decelerationRate               = serializedObject.FindProperty("decelerationRate");
            _zoomSensitivity                = serializedObject.FindProperty("zoomSensitivity");
            _minimumZoom                    = serializedObject.FindProperty("minimumZoom");
            _maximumZoom                    = serializedObject.FindProperty("maximumZoom");
            _viewport                       = serializedObject.FindProperty("viewport");
            _horizontalScrollbar            = serializedObject.FindProperty("horizontalScrollbar");
            _verticalScrollbar              = serializedObject.FindProperty("verticalScrollbar");
            _horizontalScrollbarSpacing     = serializedObject.FindProperty("horizontalScrollbarSpacing");
            _verticalScrollbarSpacing       = serializedObject.FindProperty("verticalScrollbarSpacing");
            _onPositionChanged              = serializedObject.FindProperty("onPositionChanged");
            _onScaleChanged                 = serializedObject.FindProperty("onScaleChanged");

            _showDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable()
        {
            _showDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        private void SetAnimBools(bool instant) 
            => SetAnimBool(_showDecelerationRate, !_inertia.hasMultipleDifferentValues && _inertia.boolValue == true, instant);

        private void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else a.target = value;
        }

        private void CalculateCachedValues()
        {
            _viewportIsNotChild = false;
            _hScrollbarIsNotChild = false;
            _vScrollbarIsNotChild = false;
            if (targets.Length == 1)
            {
                Transform transform = ((CurveScrollRect)target).transform;
                
                if (_viewport.objectReferenceValue == null || ((RectTransform)_viewport.objectReferenceValue).transform.parent != transform)
                    _viewportIsNotChild = true;
                
                if (_horizontalScrollbar.objectReferenceValue == null || ((Scrollbar)_horizontalScrollbar.objectReferenceValue).transform.parent != transform)
                    _hScrollbarIsNotChild = true;
                
                if (_verticalScrollbar.objectReferenceValue == null || ((Scrollbar)_verticalScrollbar.objectReferenceValue).transform.parent != transform)
                    _vScrollbarIsNotChild = true;
            }
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);

            serializedObject.Update();
            // Once we have a reliable way to know if the object changed, only re-cache in that case.
            CalculateCachedValues();

            EditorGUILayout.PropertyField(_content);
            EditorGUILayout.PropertyField(_grid);

            EditorGUILayout.PropertyField(_inertia);
            if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_decelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_zoomSensitivity);
            EditorGUILayout.PropertyField(_minimumZoom);
            EditorGUILayout.PropertyField(_maximumZoom);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_viewport);

            EditorGUILayout.PropertyField(_horizontalScrollbar);
            if (_horizontalScrollbar.objectReferenceValue && !_horizontalScrollbar.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
               
                if (_viewportIsNotChild || _hScrollbarIsNotChild)
                    EditorGUILayout.HelpBox(HorizontalError, MessageType.Error);
                EditorGUILayout.PropertyField(_horizontalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_verticalScrollbar);
            if (_verticalScrollbar.objectReferenceValue && !_verticalScrollbar.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;

                if (_viewportIsNotChild || _vScrollbarIsNotChild)
                    EditorGUILayout.HelpBox(VerticalError, MessageType.Error);
                EditorGUILayout.PropertyField(_verticalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onPositionChanged);
            EditorGUILayout.PropertyField(_onScaleChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
