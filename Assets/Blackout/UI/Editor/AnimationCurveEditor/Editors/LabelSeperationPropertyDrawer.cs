using Blackout.UI;
using UnityEngine;
using UnityEditor;

namespace BlackoutEditor.UI
{
    [CustomPropertyDrawer(typeof(LegacyCurveGridMarkers.LabelSeparation))]
    public class LegacyLabelSeparationPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty scale = property.FindPropertyRelative("scale");
            SerializedProperty cells = property.FindPropertyRelative("cells");

            float halfWidth = (position.width - 5f) * 0.5f;

            EditorGUIUtility.labelWidth = 60f;
            
            Rect rect = new Rect(position.x, position.y, halfWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, scale);
            
            rect = new Rect(position.x + halfWidth + 5f, position.y, halfWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, cells);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight * 1.1f;
    }
}