﻿using UnityEngine;
using UnityEditor;

using ShapeProperties = ThisOtherThing.UI.GeoUtils.ShapeProperties;

namespace ThisOtherThing.UI.Shapes
{
	[CustomPropertyDrawer(typeof(ShapeProperties))]
	public class ShapePropertiesDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

			if (!property.isExpanded)
				return;

			EditorGUI.BeginProperty(position, label, property);

			//		ShapeProperties shapeProperties = 
			//			(ShapeProperties)fieldInfo.GetValue(property.serializedObject.targetObject);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 1;

			Rect propertyPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

			EditorGUI.PropertyField(propertyPosition, property.FindPropertyRelative("FillColor"), new GUIContent("Color"));

			propertyPosition.y += EditorGUIUtility.singleLineHeight * 1.25f;

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded)
			{
				return EditorGUIUtility.singleLineHeight;
			}

			float height = EditorGUIUtility.singleLineHeight * 2.0f;

			return height;
		}
	}
}
