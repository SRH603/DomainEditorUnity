using UnityEditor;
using UnityEngine;
using UnityWeld.Binding.Internal;

namespace Battlehub.RTEditor.Binding
{
    [CustomEditor(typeof(OptionsEditorBinding))]
    class OptionsEditorBindingEditor : PropertyEditorBindingEditor
    {
        private OptionsEditorBinding targetScript;
        private bool viewModelOptionsPropertyPrefabModified;

        protected override void OnEnable()
        {
            base.OnEnable();
            targetScript = (OptionsEditorBinding)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var defaultLabelStyle = EditorStyles.label.fontStyle;
      
            EditorStyles.label.fontStyle = viewModelOptionsPropertyPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "View-model options property",
                    "Options property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelOptionsPropertyName = updatedValue,
                targetScript.ViewModelOptionsPropertyName,
                property => true
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, Next(false)
            // will iterate through the properties.
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "viewModelOptionsPropertyName":
                        viewModelOptionsPropertyPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
