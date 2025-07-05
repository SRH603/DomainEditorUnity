using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public interface IComponentFactoryModel
    {
        void RegisterComponentBuilder(ScriptInfo componentInfo, Func<GameObject[], ScriptInfo, Task> builder);

        void UnregisterComponentBuilder(ScriptInfo componentInfo);

        IReadOnlyList<ScriptInfo> GetSupportedComponents();

        Task BuildComponentAsync(GameObject[] gameObjects, ScriptInfo componentInfo);
    }

    public class ScriptInfo
    {
        public object Key
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Type ComponentType
        {
            get; 
            set;
        }
    }

    public class ComponentFactoryModel : IComponentFactoryModel
    {
        private readonly Dictionary<object, ScriptInfo> m_keyToComponentInfo = new Dictionary<object, ScriptInfo>();
        private readonly Dictionary<object, Func<GameObject[], ScriptInfo, Task>> m_keyToComponentBuilder = new Dictionary<object, Func<GameObject[], ScriptInfo, Task>>();

        private bool m_useEditorsMap = true;
        public bool UseEditorsMap
        {
            get { return m_useEditorsMap; }
            set { m_useEditorsMap = value; }
        }

        public void RegisterComponentBuilder(ScriptInfo componentInfo, Func<GameObject[], ScriptInfo, Task> builder)
        {
            m_keyToComponentInfo[componentInfo.Key] = componentInfo;
            m_keyToComponentBuilder[componentInfo.Key] = builder;
        }

        public void UnregisterComponentBuilder(ScriptInfo componentInfo)
        {
            m_keyToComponentInfo.Remove(componentInfo.Key);
            m_keyToComponentBuilder.Remove(componentInfo.Key);
        }

        public IReadOnlyList<ScriptInfo> GetSupportedComponents()
        {
            var lc = IOC.Resolve<ILocalization>();
            var componentsList = m_keyToComponentInfo.Values.ToList();
           
            if (m_useEditorsMap)
            {
                var editorsMap = IOC.Resolve<IEditorsMap>();
                if (editorsMap != null)
                {
                    var editableTypes = editorsMap.GetEditableTypes();
                    foreach (var type in editableTypes)
                    {
                        if (!type.IsSubclassOf(typeof(Component)))
                        {
                            continue;
                        }

                        if (m_keyToComponentInfo.ContainsKey(type.FullName))
                        {
                            continue;
                        }

                        componentsList.Add(new ScriptInfo
                        {
                            ComponentType = type,
                            Name = lc.GetString(string.Format("ID_RTEditor_CD_{0}", type.Name), type.Name),
                            Key = type.FullName
                        });
                    }
                }
            }

            return componentsList;            
        }

        public async Task BuildComponentAsync(GameObject[] gameObjects, ScriptInfo componentInfo)
        {
            if (m_keyToComponentBuilder.TryGetValue(componentInfo.Key, out var builder))
            {
                await builder(gameObjects, componentInfo);
            }
            else
            {
                AddComponent(gameObjects, componentInfo.ComponentType);
            }
        }

        private void AddComponent(GameObject gameObject, Type type)
        {
            var exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null)
            {
                var editor = IOC.Resolve<IRTE>();
                editor.Undo.AddComponentWithRequirements(exposeToEditor, type);
            }
            else
            {
                gameObject.AddComponent(type);
            }
        }

        private void AddComponent(GameObject[] gameObjects, Type type)
        {
            var editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                AddComponent(go, type);
            }

            editor.Undo.EndRecord();
        }
    }

}
