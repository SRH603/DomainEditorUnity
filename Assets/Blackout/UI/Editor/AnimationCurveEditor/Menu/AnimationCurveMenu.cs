using Blackout.UI;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;

namespace BlackoutEditor.UI
{
    /// <summary>
    /// This script adds the UI menu options to the Unity Editor.
    /// </summary>
    public static class AnimationCurveMenu
    {
        [MenuItem("GameObject/UI/Blackout/Animation Curve Button (Legacy)", false, 2038)]
        public static void AddCurveButton(MenuCommand menuCommand)
        {
            AnimationCurveEditor editor = ComponentUtility.FindSceneComponentOfType<AnimationCurveEditor>();
            if (!editor)
            {

                if (EditorUtility.DisplayDialog(
                        "No Animation Curve Editor found",
                        "No Animation Curve Editor found in scene. Would you like to generate one now?\nIf not you will need to manually assign the editors reference to the Animation Curve Button", "Yes", "No"))
                {
                    GameObject editorGo = AnimationCurveBuilder.CreateCurveEditor(BlackoutMenu.GetStandardResources());
                    BlackoutMenu.PlaceUIElementRoot(editorGo, menuCommand);
            
                    editor = editorGo.GetComponent<AnimationCurveEditor>();
                    editor.CurveRenderer.MarkDirty();
                }
            }
            
            GameObject go = AnimationCurveBuilder.CreateCurveButton(editor, BlackoutMenu.GetStandardResources());
            BlackoutMenu.PlaceUIElementRoot(go, menuCommand);
        }
        
        [MenuItem("GameObject/UI/Blackout/Animation Curve Editor (Legacy)", false, 2039)]
        public static void AddCurveEditor(MenuCommand menuCommand)
        {
            GameObject go = AnimationCurveBuilder.CreateCurveEditor(BlackoutMenu.GetStandardResources());
            BlackoutMenu.PlaceUIElementRoot(go, menuCommand);
            
            go.GetComponent<AnimationCurveEditor>().CurveRenderer.MarkDirty();
        }

        
    }
}
