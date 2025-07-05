using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public interface IGameObjectEditor
    {
        public GameObject[] SelectedGameObjects
        {
            get;
            set;
        }
    }

    public class GameObjectEditor : MonoBehaviour, IGameObjectEditor
    {
        [SerializeField]
        private BoolEditor IsActiveEditor = null;
        [SerializeField]
        private TMP_InputField InputName = null;
        [SerializeField]
        private GameObject LayersEditorRoot = null;
        [SerializeField]
        private OptionsEditor LayerEditor = null;
        [SerializeField]
        private GameObject PrefabEditorRoot = null;
        [SerializeField]
        private Button EditLayersButton = null;
        [SerializeField]
        private Transform ComponentsPanel = null;

        [SerializeField]
        private GameObject m_addComponentRoot = null;
        [SerializeField]
        private AddComponentControl m_addComponentControl = null;

        private GameObjectEditorUtils.GameObjectWrapper[] m_selectedGameObjects;

        private IRuntimeEditor m_editor;
        private ISettingsComponent m_settingsComponent;
        private IRuntimeSelection m_selectionOverride;

        private GameObject SelectedGameObject
        {
            get { return SelectedObject as GameObject; }
        }

        public GameObject[] SelectedGameObjects
        {
            get
            {
                if (SelectedObjects == null)
                {
                    return null;
                }
                return SelectedObjects.OfType<GameObject>().ToArray();
            }
            set
            {
                if (m_selectionOverride != null)
                {
                    m_selectionOverride.objects = value;
                }
                else
                {
                    m_editor.Selection.objects = value;
                }
            }
        }

        private UnityObject SelectedObject
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return m_editor.Selection.activeObject;
                }

                return m_selectionOverride.activeObject;
            }
        }

        private UnityObject[] SelectedObjects
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return m_editor.Selection.objects;
                }

                return m_selectionOverride.objects;
            }
        }

        private bool IsSelected(UnityObject obj)
        {
            if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
            {
                return m_editor.Selection.IsSelected(obj);
            }

            return m_selectionOverride.IsSelected(obj);
        }

        private bool m_initOnStart = false;
        private void Awake()
        {
            m_settingsComponent = IOC.Resolve<ISettingsComponent>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Object.ComponentAdded += OnComponentAdded;

            m_initOnStart = SelectedGameObjects == null;
            if (!m_initOnStart)
            {
                Init();
            }
        }

        private void Start()
        {
            RuntimeWindow window = GetComponentInParent<RuntimeWindow>();
            if (window != null)
            {
                m_selectionOverride = window.IOCContainer.Resolve<IRuntimeSelection>();
            }

            if (m_initOnStart)
            {
                Init();
            }
        }

        private bool CanRename(GameObject go)
        {
            var exposeToEditor = go.GetComponent<ExposeToEditor>();
            return exposeToEditor == null || exposeToEditor.CanRename;
        }

        private void Init()
        {
            GameObject[] selectedGameObjects = SelectedGameObjects;
            InputName.text = GameObjectEditorUtils.GetObjectName(selectedGameObjects);
            InputName.interactable = selectedGameObjects.All(go => CanRename(go));
            InputName.onEndEdit.AddListener(OnEndEditName);
            InputName.gameObject.SetActive(m_settingsComponent.BuiltInWindowsSettings.Inspector.GameObjectEditor.ShowName);

            m_selectedGameObjects = selectedGameObjects.Select(go => new GameObjectEditorUtils.GameObjectWrapper(go)).ToArray();
            IsActiveEditor.Init(m_selectedGameObjects, Strong.PropertyInfo((GameObjectEditorUtils.GameObjectWrapper x) => x.IsActive), string.Empty, true, null, null, null, null, null, () =>
            {
                var inspectorModel = IOC.Resolve<IInspectorModel>();
                inspectorModel?.SetDirty(selectedGameObjects);
            });

            IsActiveEditor.gameObject.SetActive(m_settingsComponent.BuiltInWindowsSettings.Inspector.GameObjectEditor.ShowIsActiveToggle);
            IRuntimeEditor editor = m_editor;
            editor.IsBusy = true;
            LayersEditor.LoadLayers(layersInfo =>
            {
                if (editor == null)
                {
                    return;
                }
                editor.IsBusy = false;
                if (SelectedGameObject == null)
                {
                    return;
                }

                InitLayersEditor(layersInfo);
                CreateComponentEditors(selectedGameObjects);
                InitAddComponentControl();
            });

            if (PrefabEditorRoot != null)
            {
                PrefabEditorRoot.SetActive(
                    editor.IsProjectLoaded &&
                    selectedGameObjects.Length == 1 &&
                    editor.CanSelectPrefab(selectedGameObjects[0]));
            }
        }

        private void OnDestroy()
        {
            if (InputName != null)
            {
                InputName.onEndEdit.RemoveListener(OnEndEditName);
            }

            if (m_editor != null)
            {
                if (m_editor.Object != null)
                {
                    m_editor.Object.ComponentAdded -= OnComponentAdded;
                }
            }

            UnityEventHelper.RemoveListener(EditLayersButton, btn => btn.onClick, OnEditLayersClick);

            if (m_addComponentControl != null)
            {
                m_addComponentControl.SelectComponent -= OnSelectComponent;
            }

            m_editor = null;
            m_settingsComponent = null;
            m_selectionOverride = null;
        }

        private void Update()
        {
            GameObject go = SelectedGameObject;
            if (go == null)
            {
                return;
            }

            UnityObject[] objects = SelectedObjects;
            if (objects[0] == null)
            {
                return;
            }

            if (InputName != null && !InputName.isFocused)
            {
                string objectName = GameObjectEditorUtils.GetObjectName(objects);
                if (InputName.text != objectName)
                {
                    InputName.text = objectName;
                }
            }
        }

        private void InitLayersEditor(LayersInfo layersInfo)
        {
            List<RangeOptions.Option> layers = new List<RangeOptions.Option>();

            foreach (LayersInfo.Layer layer in layersInfo.Layers)
            {
                if (!string.IsNullOrEmpty(layer.Name))
                {
                    layers.Add(new RangeOptions.Option(string.Format("{0}: {1}", layer.Index, layer.Name), layer.Index));
                }
            }

            LayerEditor.Options = layers.ToArray();

            var selectedGameObjects = SelectedGameObjects;
            LayerEditor.Init(selectedGameObjects, Strong.PropertyInfo((GameObject x) => x.layer), string.Empty, true, null, null, null, null, null, () =>
            {
                var inspectorModel = IOC.Resolve<IInspectorModel>();
                inspectorModel?.SetDirty(selectedGameObjects);
            });
            UnityEventHelper.AddListener(EditLayersButton, btn => btn.onClick, OnEditLayersClick);

            bool showLayers = m_settingsComponent.BuiltInWindowsSettings.Inspector.GameObjectEditor.ShowLayers;
            if (LayersEditorRoot != null)
            {
                LayersEditorRoot.gameObject.SetActive(showLayers);
            }
        }

        private void CreateComponentEditors(GameObject[] selectedObjects)
        {
            List<List<Component>> groups = GameObjectEditorUtils.GetComponentGroups(selectedObjects);
            for (int i = 0; i < groups.Count; ++i)
            {
                List<Component> group = groups[i];
                GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, group);
            }
        }

        private void InitAddComponentControl()
        {
            ExposeToEditor exposeToEditor = SelectedGameObject.GetComponent<ExposeToEditor>();
            if (exposeToEditor && (m_settingsComponent == null || m_settingsComponent.BuiltInWindowsSettings.Inspector.ShowAddComponentButton) && !m_editor.IsAssetRoot(exposeToEditor.gameObject))
            {
                if (m_addComponentControl != null)
                {
                    m_addComponentControl.SelectComponent += OnSelectComponent;
                }
            }
            else
            {
                if (m_addComponentRoot != null)
                {
                    m_addComponentRoot.SetActive(false);
                }
            }
        }

        [Obsolete] //13.04.2021
        public static List<List<Component>> GetComponentGroups(GameObject[] gameObjects)
        {
            return GameObjectEditorUtils.GetComponentGroups(gameObjects);
        }

        private void OnEndEditName(string name)
        {
            GameObjectEditorUtils.EndEditName(name, SelectedGameObjects);
        }

        private async void OnSelectComponent(object sender, SelectComponentEventArgs args)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            IComponentFactoryModel factory = IOC.Resolve<IComponentFactoryModel>();

            using var b = editor.SetBusy();
            await factory.BuildComponentAsync(SelectedGameObjects, args.ComponentInfo);
        }

        private void OnComponentAdded(ExposeToEditor obj, Component component)
        {
            if (component == null)
            {
                IWindowManager wnd = IOC.Resolve<IWindowManager>();
                wnd.MessageBox("Unable to add component", "Component was not added");
            }
            else
            {
                if (!IsSelected(component.gameObject))
                {
                    return;
                }

                if (SelectedGameObject == null)
                {
                    return;
                }

                HashSet<Component> ignoreComponents = GameObjectEditorUtils.IgnoreComponents(obj.gameObject);
                if (!GameObjectEditorUtils.IsComponentValid(component, ignoreComponents))
                {
                    return;
                }

                GameObject[] gameObjects = SelectedGameObjects;
                if (gameObjects.Length == 1)
                {
                    GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, new List<Component> { component });
                }
                else
                {
                    if (gameObjects[gameObjects.Length - 1] != component.gameObject)
                    {
                        return;
                    }

                    List<List<Component>> groups = GameObjectEditorUtils.GetComponentGroups(gameObjects);
                    for (int i = 0; i < groups.Count; ++i)
                    {
                        List<Component> group = groups[i];

                        //This is to handle case when AddComponent called on multiple objects. 
                        //See InspectorView.cs void OnAddComponent(Type type) method for details.
                        if (group[group.Count - 1] == component)
                        {
                            GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, group);
                            break;
                        }
                    }
                }
            }
        }

        private void OnEditLayersClick()
        {
            LayersEditor.BeginEdit();
        }
    }
}

