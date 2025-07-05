#if UNITY_EDITOR
using Battlehub.RTCommon;
using Battlehub.Utils;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public static class RTHandlesMenu
    {
        public static GameObject InstantiatePrefab(string name)
        {
            string path = BHRoot.PackageRuntimeContentPath + "/RTHandles/Prefabs/" + name;
            Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            if (prefab == null)
            {
                throw new FileNotFoundException($"{path} does not exist.");
            }
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.SetActive(true);
            return go;
        }

        [MenuItem("Tools/Runtime Handles/Create/Position Handle", priority = 1)]
        public static void CreatePositionHandle()
        {
            GameObject go = InstantiatePrefab("PositionHandle.prefab");
            Undo.RegisterCreatedObjectUndo(go, "Position Handle");
        }

        [MenuItem("Tools/Runtime Handles/Create/Position Handle (Light)", priority = 2)]
        public static void CreatePositionHandleLight()
        {
            if (Selection.activeGameObject != null)
            {
                Undo.RegisterCreatedObjectUndo(Selection.activeGameObject.AddComponent<PositionHandle>(), "Positon Handle");
            }
            else
            {
                GameObject go = new GameObject("PositionHandle");
                go.AddComponent<PositionHandle>();
                Undo.RegisterCreatedObjectUndo(go, "Position Handle");
            }
        }


        [MenuItem("Tools/Runtime Handles/Create/Rotation Handle", priority = 3)]
        public static void CreateRotationHandle()
        {
            GameObject go = InstantiatePrefab("RotationHandle.prefab");
            Undo.RegisterCreatedObjectUndo(go, "Rotation Handle");
        }

        [MenuItem("Tools/Runtime Handles/Create/Rotation Handle (Light)", priority = 4)]
        public static void CreateRotationHandleLight()
        {
            if (Selection.activeGameObject != null)
            {
                Undo.RegisterCreatedObjectUndo(Selection.activeGameObject.AddComponent<RotationHandle>(), "Rotation Handle");
            }
            else
            {
                GameObject go = new GameObject("RotationHandle");
                go.AddComponent<RotationHandle>();
                Undo.RegisterCreatedObjectUndo(go, "Rotation Handle");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Scale Handle", priority = 5)]
        public static void ScaleHandle()
        {
            GameObject go = InstantiatePrefab("ScaleHandle.prefab");
            Undo.RegisterCreatedObjectUndo(go, "Scale Handle");
        }

        [MenuItem("Tools/Runtime Handles/Create/Scale Handle (Light)", priority = 6)]
        public static void ScaleHandleLight()
        {
            if (Selection.activeGameObject != null)
            {
                Undo.RegisterCreatedObjectUndo(Selection.activeGameObject.AddComponent<ScaleHandle>(), "Scale Handle");
            }
            else
            {
                GameObject go = new GameObject("ScaleHandle");
                go.AddComponent<RotationHandle>();
                Undo.RegisterCreatedObjectUndo(go, "Scale Handle");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Rect Tool", priority = 7)]
        public static void RectTool()
        {
            GameObject go = InstantiatePrefab("RectTool.prefab");
            Undo.RegisterCreatedObjectUndo(go, "Rect Tool");
        }

        [MenuItem("Tools/Runtime Handles/Create/Scene Grid", priority = 8)]
        public static void SceneGrid()
        {
            var sceneGrid = Object.FindAnyObjectByType<SceneGrid>(FindObjectsInactive.Include);
            if (sceneGrid != null)
            {
                Selection.activeGameObject = sceneGrid.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("SceneGrid.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Scene Grid");
            }

        }

        [MenuItem("Tools/Runtime Handles/Create/Scene Gizmo", priority = 9)]
        public static void SceneGizmo()
        {
            var sceneGizmo = Object.FindAnyObjectByType<SceneGizmo>(FindObjectsInactive.Include);
            if (sceneGizmo != null)
            {
                Selection.activeGameObject = sceneGizmo.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("SceneGizmo.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Scene Gizmo");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Scene Component", priority = 10)]
        public static void SceneComponent()
        {
            var sceneComponent = Object.FindAnyObjectByType<RuntimeSceneComponent>(FindObjectsInactive.Include);
            if (sceneComponent != null)
            {
                Selection.activeGameObject = sceneComponent.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Components/SceneComponent.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Scene Component");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Scene Component (No Camera Controls)", priority = 10)]
        public static void SceneComponentNoCameraControls()
        {
            var sceneComponent = Object.FindAnyObjectByType<RuntimeSelectionComponent>(FindObjectsInactive.Include);
            if (sceneComponent != null)
            {
                Selection.activeGameObject = sceneComponent.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Components/SceneComponent (NoCameraControls).prefab");
                Undo.RegisterCreatedObjectUndo(go, "Scene Component");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Settings Component", priority = 11)]
        public static void HandlesComponent()
        {
            var handlesComponent = Object.FindAnyObjectByType<RuntimeHandlesComponent>(FindObjectsInactive.Include);
            if (handlesComponent != null)
            {
                Selection.activeGameObject = handlesComponent.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Components/HandlesComponent.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Handles Component");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/Tools Component", priority = 12)]
        public static void ToolsComponent()
        {
            var toolsComponent = Object.FindAnyObjectByType<RuntimeToolsInput>(FindObjectsInactive.Include);
            if (toolsComponent != null)
            {
                Selection.activeGameObject = toolsComponent.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Components/ToolsComponent.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Tools Component");
            }
        }


        [MenuItem("Tools/Runtime Handles/Create/Undo Component", priority = 13)]
        public static void UndoComponent()
        {
            var undoComponent = Object.FindAnyObjectByType<RuntimeUndoInput>(FindObjectsInactive.Include);
            if (undoComponent != null)
            {
                Selection.activeGameObject = undoComponent.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Components/UndoComponent.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Undo Component");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create/RTE Component", priority = 14)]
        public static void CreateRTE()
        {
            var rte = Object.FindAnyObjectByType<RTE>(FindObjectsInactive.Include);
            if (rte != null)
            {
                Selection.activeGameObject = rte.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("RTE.prefab");
                Undo.RegisterCreatedObjectUndo(go, "RTE Component");
            }
        }


        [MenuItem("Tools/Runtime Handles/Create Editor", priority = 10)]
        public static void CreateEditor()
        {
            var editor = Object.FindAnyObjectByType<RTE>(FindObjectsInactive.Include);
            if (editor != null)
            {
                EditorUtility.DisplayDialog("Can't create editor", "Another editor already exists in the scene", "OK");
                Selection.activeGameObject = editor.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("Editor.prefab");
                Undo.RegisterCreatedObjectUndo(go, "RTE");
            }
        }

        [MenuItem("Tools/Runtime Handles/Create Transform Handles", priority = 11)]
        public static void CreateTransformHandles()
        {
            var transformHandles = Object.FindAnyObjectByType<RuntimeSceneComponent>(FindObjectsInactive.Include);
            if (transformHandles != null)
            {
                Selection.activeGameObject = transformHandles.gameObject;
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {
                GameObject go = InstantiatePrefab("TransformHandles.prefab");
                Undo.RegisterCreatedObjectUndo(go, "Create Transform Handles");
            }
        }


        [MenuItem("Tools/Runtime Handles/Enable Editing", validate = true)]
        private static bool CanEnableEditing()
        {
            return Selection.gameObjects != null
                && Selection.gameObjects.Length > 0
                && Selection.gameObjects.Any(g => !g.GetComponent<ExposeToEditor>() && !g.IsPrefab())
                && UnityObjectExt.FindAnyObjectByType<RuntimeSelectionComponent>();
        }

        [MenuItem("Tools/Runtime Handles/Enable Editing", priority = 20)]
        private static void EnableEditing()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                if (!exposeToEditor && !go.IsPrefab())
                {
                    Undo.RegisterCreatedObjectUndo(go.AddComponent<ExposeToEditor>(), "Enable Object Editing");
                }
            }
        }

        [MenuItem("Tools/Runtime Handles/Disable Editing", validate = true)]
        private static bool CanDisableEditing()
        {
            return Selection.gameObjects != null
                && Selection.gameObjects.Length > 0
                && Selection.gameObjects.Any(g => g.GetComponent<ExposeToEditor>() && !g.IsPrefab());
        }

        [MenuItem("Tools/Runtime Handles/Disable Editing", priority = 21)]
        private static void DisableEditing()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                if (exposeToEditor && !go.IsPrefab())
                {
                    Undo.DestroyObjectImmediate(exposeToEditor);
                }
            }
        }
    }
}
#endif
