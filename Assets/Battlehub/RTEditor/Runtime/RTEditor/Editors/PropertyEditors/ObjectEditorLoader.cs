using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTSL.Interface;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class ObjectEditorLoader : MonoBehaviour, IObjectEditorLoader
    {
        private void Awake()
        {
            IOC.RegisterFallback<IObjectEditorLoader>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IObjectEditorLoader>(this);
        }

        private Type ToType(GameObject go, Type memberInfoType)
        {
            Type type;
            if (typeof(Component).IsAssignableFrom(memberInfoType) && go.GetComponent(memberInfoType) != null)
            {
                type = memberInfoType;
            }
            else
            {
                type = typeof(GameObject);
            }

            return type;
        }

        public Type GetObjectType(object dragObject, Type memberInfoType)
        {
            Type type = null;
            if (dragObject is ExposeToEditor)
            {
                ExposeToEditor exposeToEditor = (ExposeToEditor)dragObject;
                GameObject go = exposeToEditor.gameObject;
                type = ToType(go, memberInfoType);
            }
            else if (dragObject is GameObject)
            {
                type = ToType((GameObject)dragObject, memberInfoType);
            }
            else if (dragObject is IAsset)
            {
                var asset = (IAsset)dragObject; 
                var editor = IOC.Resolve<IRuntimeEditor>();
                return editor.GetType(asset.ID);
            }
            else if (dragObject is ProjectItem)
            {
                ProjectItem projectItem = (ProjectItem)dragObject;
                if(!projectItem.IsFolder)
                {
                    IProjectAsync project = IOC.Resolve<IProjectAsync>();
                    type = project.Utils.ToType(projectItem);
                }
            }
            return type;
        }

        public async void Load(object dragObject, Type memberInfoType, Action<UnityObject> callback)
        {
            ProjectItem projectItem = dragObject as ProjectItem;
            if (projectItem != null && !projectItem.IsFolder)
            {
                IProjectAsync project = IOC.Resolve<IProjectAsync>();
                try
                {
                    UnityObject[] loadedObjects = await project.Safe.LoadAsync(new[] { projectItem });
                    callback(loadedObjects[0]);
                }
                catch(Exception e)
                {
                    IWindowManager wnd = IOC.Resolve<IWindowManager>();
                    wnd.MessageBox("Unable to load object", e.Message);
                    Debug.LogException(e);
                }
            }
            else if (dragObject is IAsset)
            {
                var asset = (IAsset)dragObject;
                try
                {
                    var editor = IOC.Resolve<IRuntimeEditor>();
                    using var b = editor.SetBusy();
             
                    await editor.LoadAssetAsync(asset.ID);
                    var obj = editor.GetAsset(asset.ID);

                    if (obj is UnityObject)
                    {
                        callback?.Invoke((UnityObject)obj);
                    }
                }
                catch (Exception e)
                {
                    IWindowManager wnd = IOC.Resolve<IWindowManager>();
                    wnd.MessageBox("Unable to load object", e.Message);
                    Debug.LogException(e);
                }
            }
            else if (dragObject is GameObject)
            {
                UnityObject value = GetGameObjectOrComponent((GameObject)dragObject, memberInfoType);
                callback(value);
            }
            else if (dragObject is ExposeToEditor)
            {
                UnityObject value = GetGameObjectOrComponent(((ExposeToEditor)dragObject).gameObject, memberInfoType);
                callback(value);
            }
        }

        private UnityObject GetGameObjectOrComponent(GameObject go, Type memberInfoType)
        {
            Type goType = typeof(GameObject);
            if(goType == memberInfoType || goType.IsSubclassOf(memberInfoType))
            {
                return go;
            }

            Component component = go.GetComponent(memberInfoType);
            if (component != null)
            {
                return component;
            }
            return go;
        }
    }
}

